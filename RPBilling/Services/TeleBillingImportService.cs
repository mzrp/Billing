using Newtonsoft.Json;
using RackPeople.BillingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RackPeople.BillingAPI.Services
{
    public class ImportSummaryLine {

    }

    public class ImportSummary
    {
        public String NAVCustomerID;
        public String NAVCustomerName;
        public Dictionary<Int64, ImportSummaryLine> Lines = new Dictionary<long, ImportSummaryLine>();
    }

    public class TeleBillingImportService
    {
        private BillingEntities db;

        const int PRODUCT_CALL = 1;
        const int PRODUCT_USAGE = 2;

        /// <summary>
        /// Generate a flat map of all numbers from the agreements number series.
        /// This is used when matching against the import database
        /// </summary>
        /// <returns></returns>
        protected List<ImportNumberRange> GenerateNumberRanges() {
            var numbers = new List<ImportNumberRange>();
            var activeNumbers = db.TeleAgreements.Where(e => e.Deleted == null).SelectMany(e => e.Numbers).ToList();

            foreach (var series in activeNumbers) {
                var range = new ImportNumberRange() {
                    NumberSeriesId = series.Id,
                    AgreementId = series.TeleAgreementId
                };

                foreach (var num in series.Numbers.Split(',')) {
                    // If the number contains a -, we treat it as a range, 
                    // else we just parse it, and add it to the list.
                    if (num.Contains("-")) {
                        var start = long.Parse(num.Split('-')[0]);
                        var end = long.Parse(num.Split('-')[1]);
                        for (var i = start; i <= end; i++) {
                            range.Numbers.Add(i);
                        }
                    }
                    else {
                        range.Numbers.Add(long.Parse(num));
                    }
                }

                numbers.Add(range);
            }

            return numbers;
        }

        /// <summary>
        /// Generates an array of invoices that can be sent to NAV
        /// </summary>
        /// <returns></returns>
        protected NAVInvoice[] GenerateAgreementInvoices() {
            var invoices = new List<NAVInvoice>();

            // Get a list of numbers to match the import against
            var numbers = GenerateNumberRanges();

            // Get all current imports, since we don't need to continue if the list is empty
            var imports = db.TeleBillingImport.Where(e => e.SentToNAV == null).ToList();
            if (imports.Count == 0) {
                return invoices.ToArray();
            }

            // Get a list of all agreements, so we can match imported statements with each client, 
            // and we don't ending up creating a invoce per line.
            var agreements = new Dictionary<long, TeleAgreement>();
            foreach (var a in db.TeleAgreements.Include("Numbers").Include("Numbers.Products")) {
                if (a.Numbers.Count > 0) { 
                    agreements.Add(a.Id, a);
                }
            }

            // Run through each of the import numbers, and attempt to resolve them
            foreach (var import in imports) {
                Int64 phone;
                bool parsed = Int64.TryParse(import.Number, out phone);
                if (!parsed) {
                    continue;
                }

                // Check if the number exists
                var number = numbers.Where(e => e.Numbers.Contains(phone)).FirstOrDefault();
                if (number == null) {
                    continue;
                }

                // Resolve the info for the number
                var agreement = agreements[number.AgreementId];
                var numberSeries = agreement.Numbers.Where(e => e.Id == number.NumberSeriesId).First();

                // Get products that have the same destination type
                var numberWithSameDestinationType = numberSeries.Products.Where(e => e.DestinationType == import.DestinationType);

                // Try to see if there are any numbers that matches the prefix.
                // If there aren't any numbers with the same prefix, we look for the * wildcard
                var numbersWithSamePrefix = numberWithSameDestinationType.Where(e => import.Destination.StartsWith(e.Prefix));
                if (numbersWithSamePrefix.Count() == 0) {
                    numbersWithSamePrefix = numberWithSameDestinationType.Where(e => e.Prefix == "*");
                }

                // If there weren't any specified prefixes, and the number doesn't have a wildcard,
                // we need to just continue.
                if (numbersWithSamePrefix.Count() == 0) {
                    continue;
                }

                // Check if the invoice already exists
                var invoice = invoices.FirstOrDefault(i => i.CustomerNumber == agreement.NavCustomerId);
                if (invoice == null) {
                    invoice = new NAVInvoice() {
                        CustomerNumber = agreement.NavCustomerId,
                        CustomerName = agreement.NavCustomerName,
                        PostingDate = new DateTime(),
                        Reference = String.Format("RP {0}", agreement.Id)
                    };

                    invoices.Add(invoice);
                }

                // Check if we need to bill the calls
                if (import.NumberOfCalls > 0) {
                    var p = numbersWithSamePrefix.FirstOrDefault(e => e.ProductType == PRODUCT_CALL);
                    if (p != null) {
                        var line = new NAVInvoiceLine() {
                            ImportId = import.Id,
                            Number = p.NavProductNumber,
                            Description = p.Description,
                            Log = String.Format("{0} from {1} to {2}", "Calls", import.Number, import.Destination),
                            UnitPrice = p.UnitPrice,
                            UnitType = p.UnitType,
                            UnitDiscount = 0,
                            Quantity = import.NumberOfCalls
                        };

                        invoice.Lines.Add(line);
                    }
                }

                // Check if we need to bill the usage
                if (import.DurationOfCalls > 0) {
                    var p = numbersWithSamePrefix.FirstOrDefault(e => e.ProductType == PRODUCT_USAGE);
                    if (p != null) {
                        var line = new NAVInvoiceLine() {
                            ImportId = import.Id,
                            Number = p.NavProductNumber,
                            Description = p.Description,
                            Log = String.Format("{0} from {1} to {2}", "Usage", import.Number, import.Destination),
                            UnitPrice = p.UnitPrice,
                            UnitType = p.UnitType,
                            UnitDiscount = 0,
                            Quantity = import.DurationOfCalls
                        };

                        invoice.Lines.Add(line);
                    }
                }
            }

            // Return only the invoices
            return invoices.Where(i => i.Lines.Count > 0).ToArray();
        }

        protected Int64 ParsePhoneNumber(string number) {
            Int64 value;
            bool parsed = Int64.TryParse(number, out value);
            if (parsed) {
                return value;
            }

            return 0;
        }

        protected NAVInvoice[] SummarizeInvoices(NAVInvoice[] sources) {
            var invoices = new List<NAVInvoice>();

            foreach(var inv in sources) {
                var invoice = new NAVInvoice();
                invoice.Reference = inv.Reference;
                invoice.CustomerName = inv.CustomerName;
                invoice.CustomerNumber = inv.CustomerNumber;
                invoice.PostingDate = inv.PostingDate;

                // Group each line, based on their description
                // @todo This should really by product id instead
                var products = inv.SalesLines.Select(x => x.Description).Distinct();

                // Create the new summarized lines
                foreach(var d in products) {
                    var lines = inv.Lines.Where(e => e.Description == d);
                    var line = lines.First();
                    line.Quantity = lines.Sum(e => e.Quantity);
                    line.Log = String.Join(", ", lines.Select(e => e.Log));
                    line.ImportIds = lines.Select(e => e.ImportId).ToArray();
                    invoice.Lines.Add(line);
                }

                // Add the invoice
                invoices.Add(invoice);
            }

            return invoices.ToArray();
        }

        /// <summary>
        /// Execute the billing import service
        /// </summary>
        /// <returns></returns>
        public NAVInvoice[] Execute(bool dryRun = false) {
            // Generate all invoiceable agreements
            var invoices = SummarizeInvoices(GenerateAgreementInvoices());

            // Run though each, and send them to NAV
            if (dryRun != true) {
                var nav = new NAVService();
                foreach (var invoice in invoices) {
                    nav.CreateInvoiceDraft(invoice);
                    nav.MarkAsSentToNAV(invoice, db);
                }
            }

            // Return the agreements and their sales lines
            return invoices;
        }

        public TeleBillingImportService(BillingEntities db) {
            this.db = db;
        }
    }
}