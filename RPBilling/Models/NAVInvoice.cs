using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using RackPeople.BillingAPI.NAVSalesInvoiceService;

namespace RackPeople.BillingAPI.Models
{
    public class NAVInvoice
    {
        public string Reference { get; set; }

        public string CustomerNumber { get; set; }

        public string CustomerName { get; set; }

        public DateTime PostingDate { get; set; }

        [JsonIgnore]
        [NonSerialized]
        public List<NAVInvoiceLine> Lines = new List<NAVInvoiceLine>();

        /// <summary>
        /// Returns the sales lines, but splits lines when the description exceeds 50 characters
        /// </summary>
        public NAVInvoiceLine[] SalesLines
        {
            get
            {
                var lines = new List<NAVInvoiceLine>();
                foreach(var line in this.Lines) {
                    // Check how many lines the description takes up
                    var numberOfLines = Math.Ceiling(line.Description.Length / 50.0);
                    var chars = line.Description.ToCharArray();

                    // Go through each description lines
                    for (int i = 0; i < numberOfLines; i++) {
                        var range = chars.Skip(i * 50).Take(50);

                        if (i == 0) {
                            var entry = line.Copy();
                            entry.Description = String.Join("", range);
                            lines.Add(entry);
                        }
                        else {
                            var entry = new NAVInvoiceLine();
                            entry.Description = String.Join("", range);
                            entry.Type = NAVSalesInvoiceService.Type._blank_;
                            lines.Add(entry);
                        }
                    }
                }

                return lines.ToArray();
            }
        }
    }

    public class NAVInvoiceLine
    {
        public long ImportId { get; set; }

        public NAVSalesInvoiceService.Type Type = NAVSalesInvoiceService.Type.Item;

        public string Number { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal UnitDiscount = 0;

        public string UnitType { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public string Log { get; set; }

        [JsonIgnore]
        public Int64[] ImportIds { get; set; }

        public NAVInvoiceLine Copy() {
            return new NAVInvoiceLine() {
                ImportId = this.ImportId,
                Number = this.Number,
                Quantity = this.Quantity,
                UnitPrice = this.UnitPrice,
                UnitDiscount = this.UnitDiscount,
                UnitType = this.UnitType,
                Description = this.Description,
                Log = this.Log
            };
        }

        public static implicit operator NAVInvoiceLine(Sales_Invoice_Line v) {
            throw new NotImplementedException();
        }
    }
}