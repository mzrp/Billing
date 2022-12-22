using RackPeople.BillingAPI.Models;
using RackPeople.BillingAPI.NAVSalesInvoiceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace RackPeople.BillingAPI.Services
{
    public class NAVService
    {
        private SalesInvoice_Service_Service service = new SalesInvoice_Service_Service();

        /// <summary>
        /// Updates the SentToNAV column for each of the sales lines
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="db"></param>
        public void MarkAsSentToNAV(NAVInvoice invoice, BillingEntities db) {
            foreach(var line in invoice.SalesLines) {
                var import = db.TeleBillingImport.Find(line.ImportId);
                import.SentToNAV = DateTime.Now;
            }

            db.SaveChanges();
        }

        public void CreateInvoiceDraft(NAVInvoice invoice) {
            // Create the new sales invoice draft
            var draft = new SalesInvoice_Service();
            service.Create(ref draft);

            // Update the draft, with the new fields
            draft.Sell_to_Customer_No = invoice.CustomerNumber;
            draft.Posting_Date = invoice.PostingDate;
            draft.Your_Reference = "TELE BILLING TEST"; //invoice.Reference;
            service.Update(ref draft);

            // Create a reference for each of the lines
            draft.SalesLines = new Sales_Invoice_Line[invoice.SalesLines.Length];
            for (var i = 0; i < invoice.SalesLines.Length; i++) {
                draft.SalesLines[i] = new Sales_Invoice_Line();
            }
            service.Update(ref draft);

            // Update each for the lines with some content
            for (var i = 0; i < invoice.SalesLines.Length; i++) {
                var line = invoice.SalesLines[i];
                draft.SalesLines[i].Type = line.Type;
                draft.SalesLines[i].Total_Amount_Incl_VATSpecified = false;
                draft.SalesLines[i].Total_Amount_Excl_VATSpecified = false;
                draft.SalesLines[i].Total_VAT_AmountSpecified = false;

                draft.SalesLines[i].No = line.Number;
                draft.SalesLines[i].Quantity = line.Quantity;
                draft.SalesLines[i].Unit_Price = line.UnitPrice;
                draft.SalesLines[i].Unit_of_Measure = line.UnitType;
                draft.SalesLines[i].Description = line.Description;
                draft.SalesLines[i].Line_Discount_Amount = line.UnitDiscount;
            }
            service.Update(ref draft);
        }

        public void SendEmailToBookkeeping(string recipients, string subject, string[] lines) {
            // Compose the result message
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("billing@rackpeople.dk", "RackPeople Billing");
            foreach (var adr in recipients.Split(';')) {
                msg.To.Add(adr);
            }

            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = String.Join("<br />", lines);

            // Send the message through RP relay
            SmtpClient client = new SmtpClient("relay.rackpeople.com", 25);
            client.UseDefaultCredentials = true;
            client.Send(msg);
        }

        public NAVService() {
            this.service = new SalesInvoice_Service_Service();
            this.service.Credentials = new NetworkCredential("rpnavapi", "Telefon1");
        }
    }
}