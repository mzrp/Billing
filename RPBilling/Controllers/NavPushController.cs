using RackPeople.BillingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RackPeople.BillingAPI.NAVSalesInvoiceService;
using System.Net.Mail;

namespace RackPeople.BillingAPI.Controllers
{
    public class NavPushController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        protected List<Sales_Invoice_Line> GetInvoiceLines(Subscription s, DateTime billingPeriode) {
            var lines = new List<Sales_Invoice_Line>();

            // Add subscription period as first line
            var starts = billingPeriode.AddDays(0);
            DateTime ends = billingPeriode.AddDays(0);

            switch (s.BillingCycle) {
                case "Monthly":
                    ends = starts.AddMonths(1);
                    break;
                case "Quaterly":
                    ends = starts.AddMonths(3);
                    break;
                case "Biannually":
                    ends = starts.AddMonths(6);
                    break;
                case "Annually":
                    ends = starts.AddMonths(12);
                    break;
            }

            var period = new Sales_Invoice_Line();
            period.Type = NAVSalesInvoiceService.Type._blank_;

            // Subtract one day before writing
            ends = ends.AddDays(-1);
            period.Description = String.Format("Periode {0} - {1}", starts.ToShortDateString(), ends.ToShortDateString());

            lines.Add(period);

            foreach (var p in s.Products) {
                string description = p.Description;

                bool isUnitPerMonths = (!String.IsNullOrEmpty(p.UnitType) && p.UnitType.ToLower().Contains("/md"));

                // If the unit type is /md, we add the amount of items to the description
                if (isUnitPerMonths) {
                    description = String.Format("{0} ({1} {2})", description, (int)p.UnitAmount, p.UnitType.ToLower());
                }

                var numberOfLines = Math.Ceiling(description.Length / 50.0);
                var chars = description.ToCharArray();
                for (int i = 0; i < numberOfLines; i++) {
                    var range = chars.Skip(i * 50).Take(50);

                    var line = new Sales_Invoice_Line();
                    
                    if (i == 0) {
                        // If the product contains /md we need to multiple the quantity
                        decimal amount = p.UnitAmount;
                        if (isUnitPerMonths) {
                            amount = p.UnitAmount * s.MonthsInBillingCycle;
                        }

                        line.Type = NAVSalesInvoiceService.Type.Item;
                        line.No = p.NavProductNumber;
                        line.Quantity = amount;
                        line.Unit_Price = p.UnitPrice;
                        line.Unit_of_Measure = p.UnitType;
                        line.Description = String.Join("", range);

                        if (p.NavPrice > p.UnitPrice) {
                            line.Unit_Price = p.NavPrice;
                            line.Line_Discount_Amount = (p.NavPrice - p.UnitPrice) * amount;
                        }
                        else {
                            line.Line_Discount_Amount = 0;
                        }

                        // If the NavPrice is 0, we need to assign
                    }
                    else {
                        line.Type = NAVSalesInvoiceService.Type._blank_;
                        line.Description = String.Join("", range);
                    }

                    lines.Add(line);
                }
            }

            // Add the additonal text if available
            if (!String.IsNullOrEmpty(s.AdditionalText)) {
                var textLines = this.SplitIntoLines(s.AdditionalText, 50);
                foreach(var line in textLines) {
                    var sil = new Sales_Invoice_Line();
                    sil.Type = NAVSalesInvoiceService.Type._blank_;
                    sil.Description = line;
                    lines.Add(sil);
                }
            }

            return lines;
        }

        protected string[] SplitIntoLines(string value, int maxChars) {
            var result = new List<string>();

            var numberOfLines = Math.Ceiling(value.Length / (float)maxChars);
            var chars = value.ToCharArray();
            for (int i = 0; i < numberOfLines; i++) {
                var range = chars.Skip(i * 50).Take(50);
                result.Add(String.Join("", range));
            }

            return result.ToArray();
        } 

        protected void SendResultEmail(List<string> result, string recipients) {
            // If there aren't any lines in the result array, we assume 
            // nothing has been submitted.
            if (result.Count == 0) {
                return;
            }

            // Compose the result message
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("billing@rackpeople.dk", "RackPeople NAV Hub");
            foreach(var adr in recipients.Split(';')) {
                msg.To.Add(adr);
            }

            // additional recepients
            msg.To.Add("sa@rackpeople.dk");
            msg.To.Add("aop@rackpeople.dk");

            msg.Subject = "New invoices are pending in Navision";
            msg.IsBodyHtml = true;
            msg.Body = String.Join("<br />", result);

            // Send the message through RP relay
            SmtpClient client = new SmtpClient("relay.rackpeople.com", 25);
            client.UseDefaultCredentials = true;
            client.Send(msg);
        }

        protected Dictionary<string, object> BillSubscription(Subscription s, DateTime period, SalesInvoice_Service_Service service, bool onPickedDate, bool dryRun) {
            try {
                // Create a new sales invoice
                // Based on Milans code, it seems the invoice needs to be created, before we can start adding info
                var invoice = new SalesInvoice_Service();
                if (!dryRun) {
                    service.Create(ref invoice);
                }

                // Update the fields
                invoice.Sell_to_Customer_No = s.NavCustomerId;

                if (onPickedDate) {
                    //invoice.Posting_Date = period;
                    invoice.Posting_Date = DateTime.Now;
                } else {
                    invoice.Posting_Date = s.NextInvoice;
                }

                invoice.Your_Reference = String.Format("RPB #{0}", s.Id);
                if (!dryRun) {
                    service.Update(ref invoice);
                }

                // Create each sales line
                var lines = this.GetInvoiceLines(s, period);

                // Create the empty line container
                invoice.SalesLines = new Sales_Invoice_Line[lines.Count];
                for (var i = 0; i < lines.Count; i++) {
                    invoice.SalesLines[i] = new Sales_Invoice_Line();
                }
                if (!dryRun) {
                    service.Update(ref invoice);
                }

                // Update the lines
                for (var i = 0; i < lines.Count; i++) {
                    var line = lines[i];
                    invoice.SalesLines[i].Type = line.Type;
                    invoice.SalesLines[i].Total_Amount_Incl_VATSpecified = false;
                    invoice.SalesLines[i].Total_Amount_Excl_VATSpecified = false;
                    invoice.SalesLines[i].Total_VAT_AmountSpecified = false;

                    invoice.SalesLines[i].No = line.No;
                    invoice.SalesLines[i].Quantity = line.Quantity;
                    invoice.SalesLines[i].Unit_Price = line.Unit_Price;
                    invoice.SalesLines[i].Unit_of_Measure = line.Unit_of_Measure;
                    invoice.SalesLines[i].Description = line.Description;
                    invoice.SalesLines[i].Line_Discount_Amount = line.Line_Discount_Amount;
                }
                if (!dryRun) {
                    service.Update(ref invoice);
                }

                // Update the billing cycle
                //s.UpdateBillingCycle(DateTime.Now);
                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                var dict = new Dictionary<string, object>();
                dict.Add("success", true);
                dict.Add("message", String.Format("New invoice to {0} for subscription {1} is ready", s.NavCustomerName, invoice.Your_Reference));
                return dict;
            }       
            catch (Exception e) {
                var dict = new Dictionary<string, object>();
                dict.Add("success", false);
                dict.Add("exception", e);
                //dict.Add("message", String.Format("Failed to create invoice to {0} for agreement #{1}", s.NavCustomerName, s.Id));
                dict.Add("message", e.ToString());
                return dict;
            }
        }

        // POST: api/Subscription/5/bill
        [Route("api/subscriptions/{id}/bill/{date}")]
        public IHttpActionResult BillSubscription(int id, string date) {
            Subscription subscription = db.Subscriptions.Where(x => x.Id == id).FirstOrDefault();
            if (subscription == null) {
                return NotFound();
            }

            // Connect to the server
            var service = new NAVSalesInvoiceService.SalesInvoice_Service_Service();
            service.Credentials = this.GetNetworkCredentials();

            // Create the invoice
            var period = DateTime.Parse(date);
            var result = BillSubscription(subscription, period, service, true, false);

            this.Audit(subscription, "manually sent invoice for {0}/{1}", period.Day, period.Month);
            return Ok(result);
        }

        private int MonthsInBillingCycleDetail(string sBillingCycle)
        {
            switch (sBillingCycle)
            {
                case "Quaterly":
                    return 3;
                case "Biannually":
                    return 6;
                case "Annually":
                    return 12;
            }

            return 1;
        }


        // GET: api/subinfosums/
        [HttpGet]
        [Route("api/subinfosums")]
        public IHttpActionResult Subinfosums()
        {
            var result = new List<String>();
            //result.Add("Subscirptions Info Sums");

            try
            {
                var allSubs = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);
                db.Configuration.ProxyCreationEnabled = false;

                decimal dAnnually = 0;
                decimal dBiannually = 0;
                decimal dQuaterly = 0;
                decimal dMonthly = 0;
                decimal dAll = 0;

                foreach (var singleSub in allSubs)
                {
                    decimal dProdValue = 0;
                    foreach (var singleProd in singleSub.Products)
                    {
                        dProdValue += singleProd.UnitAmount * singleProd.UnitPrice;
                    }

                    if (singleSub.BillingCycle == "Annually") dAnnually += dProdValue;
                    if (singleSub.BillingCycle == "Biannually") dBiannually += dProdValue;
                    if (singleSub.BillingCycle == "Quaterly") dQuaterly += dProdValue;
                    if (singleSub.BillingCycle == "Monthly") dMonthly += dProdValue;
                    dAll += dProdValue;
                }

                result.Add("All: " + dAll.ToString("N") + " Annually: " + dAnnually.ToString("N") + " Biannually: " + dBiannually.ToString("N") + " Quaterly: " + dQuaterly.ToString("N") + " Monthly: " + dMonthly.ToString("N"));
            }
            catch (Exception ex)
            {
                ex.ToString();
                result.Add("error");
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("api/nav/pushnow")]
        public IHttpActionResult PushNow(string date, string billcycle)
        {
            // Get a copy of all active subscription
            var subscriptions = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);

            // Connect to the server
            var service = new SalesInvoice_Service_Service();
            service.Credentials = this.GetNetworkCredentials();

            // Build up a result list
            var result = new List<String>();

            foreach (var s in subscriptions)
            {
                if (s.BillingCycle == billcycle)
                {
                    // Create the invoice
                    var period = DateTime.Parse(date);
                    var entry = BillSubscription(s, period, service, true, false);
                    result.Add(entry["message"].ToString());

                    // test for just one subscription
                    //break;
                }
            }

            // Render
            return Ok(result);
        }

        [HttpGet]
        [Route("api/nav/push")]
        public IHttpActionResult Push(string recipients = "", bool dryRun = false) {
            // Get a copy of all active subscription
            var subscriptions = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);

            // Connect to the server
            var service = new SalesInvoice_Service_Service();
            service.Credentials = this.GetNetworkCredentials();

            // Build up a result list
            var result = new List<String>();
            if (dryRun) {
                result.Add("This is just a dry run. Nothing gets changed.");
            }

            foreach (var s in subscriptions) {
                if (!s.IsDue()) {
                    if (dryRun) {
                        result.Add(String.Format(
                            "description: {0}, first period: {1}, billing period: {2}, billing date: {3}",
                            s.Description,
                            s.FirstInvoice.ToString("dd/MM"),
                            s.BillingPeriod.ToString("dd/MM"),
                            s.InvoiceDate.ToString("dd/MM")
                        ));

                        // update billing cycle now
                        s.NextInvoice = s.InvoiceDate;
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                    }
                    continue;
                }

                var entry = BillSubscription(s, s.BillingPeriod, service, false, dryRun);

                // save next invoice date
                int offset = 30;
                if (s.PaymentTerms != null)
                {
                    offset = s.PaymentTerms.Value;
                }
                DateTime NID = s.InvoiceDate.AddDays(Math.Abs(offset));
                int iBC = MonthsInBillingCycleDetail(s.BillingCycle);
                s.NextInvoice = NID.AddMonths(iBC);
                s.NextInvoice = s.NextInvoice.AddDays(-Math.Abs(offset));
                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                result.Add(entry["message"].ToString());
            }

            // Save changes made to the database
            try {
                //if (!dryRun) { 
                    db.SaveChanges();
                //}
            }
            catch (Exception) {
                result.Add("De overstående aftaler kunne ikke gemmes i den lokale database, og deres 'First Invoice' skal opdateres manuelt.");
                if (dryRun) { 
                    throw;
                }
            }

            // Send the result email
            try {
                if (recipients == "") {
                    recipients = "bogholderi@rackpeople.dk";
                }

                if (!dryRun) { 
                    this.SendResultEmail(result, recipients);
                }
            }
            catch (Exception) {
                result.Add(String.Format("Failed to send an email to '{0}'", recipients));
            }
            
            // Render
            return Ok(result);
        }
    }
}
