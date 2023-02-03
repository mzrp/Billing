using RackPeople.BillingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RackPeople.BillingAPI.NAVSalesInvoiceService;
using System.Net.Mail;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using RackPeople.BillingAPI.Services;
using System.Web.Configuration;
using System.Security.Cryptography;
using System.Globalization;
using System.Collections;

namespace RackPeople.BillingAPI.Controllers
{
    public class PostSalesInvoiceLineResponse
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }

        [JsonProperty("@odata.etag")]
        public string odataetag { get; set; }
        public string id { get; set; }
        public string documentId { get; set; }
        public int sequence { get; set; }
        public string itemId { get; set; }
        public string accountId { get; set; }
        public string lineType { get; set; }
        public string lineObjectNumber { get; set; }
        public string description { get; set; }
        public string unitOfMeasureId { get; set; }
        public string unitOfMeasureCode { get; set; }
        public double unitPrice { get; set; }
        public int quantity { get; set; }
        public int discountAmount { get; set; }
        public int discountPercent { get; set; }
        public bool discountAppliedBeforeTax { get; set; }
        public double amountExcludingTax { get; set; }
        public string taxCode { get; set; }
        public int taxPercent { get; set; }
        public double totalTaxAmount { get; set; }
        public double amountIncludingTax { get; set; }
        public int invoiceDiscountAllocation { get; set; }
        public double netAmount { get; set; }
        public double netTaxAmount { get; set; }
        public double netAmountIncludingTax { get; set; }
        public string shipmentDate { get; set; }
        public string itemVariantId { get; set; }
        public string locationId { get; set; }
    }
    
    public class BillingPostalAddress
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string countryLetterCode { get; set; }
        public string postalCode { get; set; }
    }

    public class PostSalesInvoiceResponse
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }

        [JsonProperty("@odata.etag")]
        public string odataetag { get; set; }
        public string id { get; set; }
        public string number { get; set; }
        public string externalDocumentNumber { get; set; }
        public string invoiceDate { get; set; }
        public string postingDate { get; set; }
        public string dueDate { get; set; }
        public string customerPurchaseOrderReference { get; set; }
        public string customerId { get; set; }
        public string contactId { get; set; }
        public string customerNumber { get; set; }
        public string customerName { get; set; }
        public string billToName { get; set; }
        public string billToCustomerId { get; set; }
        public string billToCustomerNumber { get; set; }
        public string shipToName { get; set; }
        public string shipToContact { get; set; }
        public string currencyId { get; set; }
        public string currencyCode { get; set; }
        public string orderId { get; set; }
        public string orderNumber { get; set; }
        public string paymentTermsId { get; set; }
        public string shipmentMethodId { get; set; }
        public string salesperson { get; set; }
        public bool pricesIncludeTax { get; set; }
        public int remainingAmount { get; set; }
        public int discountAmount { get; set; }
        public bool discountAppliedBeforeTax { get; set; }
        public int totalAmountExcludingTax { get; set; }
        public int totalTaxAmount { get; set; }
        public int totalAmountIncludingTax { get; set; }
        public string status { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public SellingPostalAddress sellingPostalAddress { get; set; }
        public BillingPostalAddress billingPostalAddress { get; set; }
        public ShippingPostalAddress shippingPostalAddress { get; set; }
    }

    public class SellingPostalAddress
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string countryLetterCode { get; set; }
        public string postalCode { get; set; }
    }

    public class ShippingPostalAddress
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string countryLetterCode { get; set; }
        public string postalCode { get; set; }
    }


    public class PostSalesInvoice
    {
        public string externalDocumentNumber { get; set; }
        public string invoiceDate { get; set; }
        public string postingDate { get; set; }
        public string customerId { get; set; }
        public string customerNumber { get; set; }
        public string billToCustomerId { get; set; }
        public string billToCustomerNumber { get; set; }
        public PostSalesInvoiceLine[] SalesLines { get; set; }
    }

    public class PostSalesInvoiceLine
    {
        public string itemId { get; set; }
        public string lineType { get; set; }
        public string lineObjectNumber { get; set; }
        public string description { get; set; }
        public decimal unitPrice { get; set; }
        public decimal quantity { get; set; }
        public string Document_No { get; set; }
    }

    public class PostSalesInvoiceLineDB
    {
        public string no { get; set; }
        public string itemId { get; set; }
        public string lineType { get; set; }
        public string lineObjectNumber { get; set; }
        public string description { get; set; }
        public decimal unitPrice { get; set; }
        public string unitofmeasure { get; set; }
        public decimal quantity { get; set; }
        public string Document_No { get; set; }
        public decimal Line_Discount_Amount { get; set; }
        public string Type { get; set; }
        
    }

    public class GetItems
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        public List<GetItem> value { get; set; }
    }

    public class GetItem
    {
        [JsonProperty("@odata.etag")]
        public string odataetag { get; set; }
        public string id { get; set; }
        public string number { get; set; }
        public string displayName { get; set; }
        public string type { get; set; }
        public string itemCategoryId { get; set; }
        public string itemCategoryCode { get; set; }
        public bool blocked { get; set; }
        public string gtin { get; set; }
        public int inventory { get; set; }
        public int unitPrice { get; set; }
        public bool priceIncludesTax { get; set; }
        public int unitCost { get; set; }
        public string taxGroupId { get; set; }
        public string taxGroupCode { get; set; }
        public string baseUnitOfMeasureId { get; set; }
        public string baseUnitOfMeasureCode { get; set; }
        public string generalProductPostingGroupId { get; set; }
        public string generalProductPostingGroupCode { get; set; }
        public string inventoryPostingGroupId { get; set; }
        public string inventoryPostingGroupCode { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
    }


    public class NavPushController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        protected List<PostSalesInvoiceLineDB> GetInvoiceLines(Subscription s, DateTime billingPeriode, DateTime billingPeriodeInvDate) {

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var lines = new List<PostSalesInvoiceLineDB>();

            // Add subscription period as first line
            //var starts = billingPeriode.AddDays(0);
            //DateTime ends = billingPeriode.AddDays(0);

            DateTime starts = new DateTime(billingPeriodeInvDate.Year, billingPeriodeInvDate.Month, 1);
            starts = starts.AddMonths(1);

            DateTime ends = starts;

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

            var period = new PostSalesInvoiceLineDB();
            //period.Type = NAVSalesInvoiceService.Type._blank_;

            // Subtract one day before writing
            ends = ends.AddDays(-1);
            period.description = String.Format("Periode {0} - {1}", starts.ToShortDateString(), ends.ToShortDateString());

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

                    var line = new PostSalesInvoiceLineDB();
                    
                    if (i == 0) {
                        // If the product contains /md we need to multiple the quantity
                        decimal amount = p.UnitAmount;
                        if (isUnitPerMonths) {
                            amount = p.UnitAmount * s.MonthsInBillingCycle;
                        }

                        line.Type = "Item";
                        line.no = p.NavProductNumber;
                        line.quantity = amount;
                        line.unitPrice = p.UnitPrice;
                        line.unitofmeasure = p.UnitType;
                        line.description = String.Join("", range);
                        line.Line_Discount_Amount = 0;

                        /*
                        if (p.NavPrice > p.UnitPrice) {
                            line.Unit_Price = p.NavPrice;
                            line.Line_Discount_Amount = (p.NavPrice - p.UnitPrice) * amount;
                        }
                        else {
                            line.Line_Discount_Amount = 0;
                        }
                        */

                        // If the NavPrice is 0, we need to assign
                    }
                    else {
                        line.Type = "_blank_";
                        line.description = String.Join("", range);
                    }

                    lines.Add(line);
                }
            }

            // Add the additonal text if available
            if (!String.IsNullOrEmpty(s.AdditionalText)) {
                var textLines = this.SplitIntoLines(s.AdditionalText, 50);
                foreach(var line in textLines) {
                    var sil = new PostSalesInvoiceLineDB();
                    sil.Type = "_blank_";
                    sil.description = line;
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
            //msg.To.Add("sa@rackpeople.dk");
            //msg.To.Add("aop@rackpeople.dk");

            msg.Subject = "New invoices are pending in RPBilling";
            msg.IsBodyHtml = true;
            msg.Body = String.Join("<br />", result);

            // Send the message through RP relay
            SmtpClient client = new SmtpClient("relay.rackpeople.com", 25);
            client.UseDefaultCredentials = true;
            client.Send(msg);
        }

        private string GetCustomer(string filter, string BCToken)
        {
            string sResult = "n/aђn/a";

            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/customers?$filter=number eq '" + filter + "'") as HttpWebRequest;
                if (webRequestAUTH != null)
                {
                    webRequestAUTH.Method = "GET";
                    webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                    webRequestAUTH.ContentType = "application/json";
                    webRequestAUTH.MediaType = "application/json";
                    webRequestAUTH.Accept = "application/json";

                    webRequestAUTH.Headers["Authorization"] = "Bearer " + BCToken;

                    using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                    {
                        using (var srW = new StreamReader(rW))
                        {
                            var sExportAsJson = srW.ReadToEnd();
                            var sExport = JsonConvert.DeserializeObject<BCCustomers>(sExportAsJson);

                            int iCount = 1;
                            foreach (var cust in sExport.value)
                            {
                                sResult = cust.number + "ђ" + cust.id;
                                break;

                            }
                        }
                    }

                    webRequestAUTH = null;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = "n/aђn/a";
            }

            return sResult;
        }


        private string BCCreateInvoice(string sCustomerId, DateTime dtBCPostInvoice, List<PostSalesInvoiceLineDB> lines)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sLog = "";
            string sResult = "OK";

            // create invoice now
            string sNewInvoiceId = "n/a";

            BCService bcs = new BCService();

            string sAuthToken = bcs.GetBCToken();
            if (sAuthToken != "n/a")
            {
                // get customer first
                string sCustomerData = GetCustomer(sCustomerId, sAuthToken);
                sLog += "CD:" + sCustomerData + ";";

                if (sCustomerData != "n/aђn/a")
                {
                    string sCustomerVATNo = sCustomerData.Split('ђ')[0];
                    string sCustomerVATId = sCustomerData.Split('ђ')[1];

                    // create order first and create empty order lines
                    PostSalesInvoice order = new PostSalesInvoice();

                    order.customerNumber = sCustomerVATNo.PadLeft(8, '0');
                    order.billToCustomerNumber = sCustomerVATNo.PadLeft(8, '0');
                    order.customerId = sCustomerVATId;
                    order.billToCustomerId = sCustomerVATId;

                    order.invoiceDate = dtBCPostInvoice.Year.ToString().PadLeft(4, '0') + "-" + dtBCPostInvoice.Month.ToString().PadLeft(2, '0') + "-" + dtBCPostInvoice.Day.ToString().PadLeft(2, '0');
                    order.postingDate = dtBCPostInvoice.Year.ToString().PadLeft(4, '0') + "-" + dtBCPostInvoice.Month.ToString().PadLeft(2, '0') + "-" + dtBCPostInvoice.Day.ToString().PadLeft(2, '0');

                    try
                    {
                        //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;

                        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                        var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v1.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/salesInvoices") as HttpWebRequest;
                        if (webRequestAUTH != null)
                        {
                            webRequestAUTH.Method = "POST";
                            webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                            webRequestAUTH.ContentType = "application/json";
                            webRequestAUTH.MediaType = "application/json";
                            webRequestAUTH.Accept = "application/json";

                            webRequestAUTH.Headers["Authorization"] = "Bearer " + sAuthToken;

                            string sParams = "{\"externalDocumentNumber\": \"\", \"invoiceDate\": \"" + order.invoiceDate + "\", \"postingDate\": \"" + order.postingDate + "\", \"customerId\": \"" + order.customerId + "\", \"customerNumber\": \"" + order.customerNumber + "\", \"billToCustomerId\": \"" + order.billToCustomerId + "\", \"billToCustomerNumber\": \"" + order.billToCustomerNumber + "\"}";
                            var data = Encoding.ASCII.GetBytes(sParams);
                            webRequestAUTH.ContentLength = data.Length;

                            using (var sW = webRequestAUTH.GetRequestStream())
                            {
                                sW.Write(data, 0, data.Length);
                            }

                            using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                            {
                                using (var srW = new StreamReader(rW))
                                {
                                    var sExportAsJson = srW.ReadToEnd();
                                    var sExport = JsonConvert.DeserializeObject<PostSalesInvoiceResponse>(sExportAsJson);
                                    if (sExport.id != null)
                                    {
                                        if (sExport.id != "")
                                        {
                                            sNewInvoiceId = sExport.id;                                            
                                        }
                                    }
                                }
                            }

                            webRequestAUTH = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        sNewInvoiceId = "n/a";
                        sLog += "NIErr:" + ex.ToString() + ";";
                        sResult = "NOTOK";
                    }
                }
                sLog += "NI:" + sNewInvoiceId + ";";

                if (sNewInvoiceId != "n/a")
                {
                    for (var i = 0; i < lines.Count; i++)
                    {
                        var line = lines[i];

                        /*
                        invoice.SalesLines[i].Type = line.Type;
                        invoice.SalesLines[i].No = line.No;
                        invoice.SalesLines[i].Quantity = line.Quantity;
                        invoice.SalesLines[i].Unit_Price = line.Unit_Price;
                        invoice.SalesLines[i].Unit_of_Measure = line.Unit_of_Measure;
                        invoice.SalesLines[i].Description = line.Description;
                        invoice.SalesLines[i].Line_Discount_Amount = line.Line_Discount_Amount;
                        */

                        PostSalesInvoiceLine ord = new PostSalesInvoiceLine();
                        ord.quantity = line.quantity;
                        ord.description = line.description;
                        if (ord.description.Length > 50)
                        {
                            ord.description = ord.description.Substring(0, 50);
                        }
                        ord.unitPrice = line.unitPrice;
                        ord.lineType = "Item";
                        ord.itemId = "";
                        ord.lineObjectNumber = "";

                        sLog += "LINo:" + line.no + ";";

                        if (line.no != "")
                        {
                            // get itemid and lineobjectnumber
                            try
                            {
                                //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                       | SecurityProtocolType.Tls11
                                       | SecurityProtocolType.Tls12
                                       | SecurityProtocolType.Ssl3;

                                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                                var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/items?$filter=number eq '" + line.no + "'") as HttpWebRequest;
                                if (webRequestAUTH != null)
                                {
                                    webRequestAUTH.Method = "GET";
                                    webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                                    webRequestAUTH.ContentType = "application/json";
                                    webRequestAUTH.MediaType = "application/json";
                                    webRequestAUTH.Accept = "application/json";

                                    webRequestAUTH.Headers["Authorization"] = "Bearer " + sAuthToken;

                                    using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                                    {
                                        using (var srW = new StreamReader(rW))
                                        {
                                            var sExportAsJson = srW.ReadToEnd();
                                            var sExport = JsonConvert.DeserializeObject<GetItems>(sExportAsJson);
                                            foreach (var it in sExport.value)
                                            {
                                                ord.itemId = it.id;
                                                ord.lineObjectNumber = it.number;
                                                sLog += "IT:" + ord.itemId + "," + ord.lineObjectNumber + ";";
                                                break;
                                            }
                                        }
                                    }

                                    webRequestAUTH = null;
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                                ord.itemId = "";
                                ord.lineObjectNumber = "";
                                sLog += "ITErr:" + ex.ToString() + ";";
                                sResult = "NOTOK";
                            }
                        }

                        string sNewInvoiceLineId = "n/a";
                        try
                        {
                            //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                   | SecurityProtocolType.Tls11
                                   | SecurityProtocolType.Tls12
                                   | SecurityProtocolType.Ssl3;

                            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                            var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/salesInvoices(" + sNewInvoiceId + ")/salesInvoiceLines") as HttpWebRequest;
                            if (webRequestAUTH != null)
                            {
                                webRequestAUTH.Method = "POST";
                                webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                                webRequestAUTH.ContentType = "application/json";
                                webRequestAUTH.MediaType = "application/json";
                                webRequestAUTH.Accept = "application/json";

                                webRequestAUTH.Headers["Authorization"] = "Bearer " + sAuthToken;

                                string sParams = "{\"description\": \"" + ord.description + "\"}";
                                if ((ord.itemId != "") && (ord.lineObjectNumber != ""))
                                {
                                    sParams = "{\"itemId\": \"" + ord.itemId + "\", \"lineType\": \"" + ord.lineType + "\", \"lineObjectNumber\": \"" + ord.lineObjectNumber + "\", \"description\": \"" + ord.description + "\", \"unitPrice\": " + ord.unitPrice + ", \"quantity\": " + ord.quantity + "}";
                                }

                                sLog += "ITPar:" + sParams + ";";

                                var data = Encoding.UTF8.GetBytes(sParams);
                                webRequestAUTH.ContentLength = data.Length;
                                using (var sW = webRequestAUTH.GetRequestStream())
                                {
                                    sW.Write(data, 0, data.Length);
                                }
                                using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                                {
                                    using (var srW = new StreamReader(rW))
                                    {
                                        var sExportAsJson = srW.ReadToEnd();
                                        var sExport = JsonConvert.DeserializeObject<PostSalesInvoiceLineResponse>(sExportAsJson);
                                        if (sExport.id != null)
                                        {
                                            if (sExport.id != "")
                                            {
                                                sNewInvoiceLineId = sExport.id;
                                                sLog += "IL:" + sNewInvoiceLineId + ";";
                                            }
                                        }
                                    }
                                }

                                webRequestAUTH = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            sNewInvoiceLineId = "n/a";
                            sLog += "ILErr:" + ex.ToString() + ";";
                            sResult = "NOTOK";
                        }

                    }
                }
            }

            return sLog + "ш" + sResult;
        }

        protected Dictionary<string, object> BillSubscription(Subscription s, DateTime period, DateTime periodinvdate, bool onPickedDate, bool dryRun) {
            try {
                // Create a new sales invoice
                // Based on Milans code, it seems the invoice needs to be created, before we can start adding info

                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                string sYourRef = String.Format("RPB #{0}", s.Id);
                string sResultLog = sYourRef;
                string sLog = sYourRef;

                if (!dryRun) 
                {
                    
                    DateTime dtBCPostInvoice = DateTime.Now;                    
                    if (onPickedDate)
                    {
                        dtBCPostInvoice = DateTime.Now;
                    }
                    else
                    {
                        dtBCPostInvoice = s.NextInvoice;
                    }

                    // Create each sales line now
                    var lines = this.GetInvoiceLines(s, period, periodinvdate);

                    // cretae invoice now
                    sLog = BCCreateInvoice(s.NavCustomerId, dtBCPostInvoice, lines);
                    string[] sLogArray = sLog.Split('ш');
                    if (sLogArray[1] == "OK")
                    {
                        sResultLog = sYourRef;
                    }
                    else
                    {
                        sResultLog = sYourRef + sLogArray[0];
                    }
                }

                // Update the billing cycle
                //s.UpdateBillingCycle(DateTime.Now);

                // TESTINV
                //db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                var dict = new Dictionary<string, object>();
                dict.Add("success", true);
                dict.Add("message", String.Format("New invoice to {0} for subscription: {1}", s.NavCustomerName, sResultLog));
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

            // Create the invoice
            var period = DateTime.Parse(date);
            var result = BillSubscription(subscription, period, period, true, false);

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

            // Build up a result list
            var result = new List<String>();

            foreach (var s in subscriptions)
            {
                if (s.BillingCycle == billcycle)
                {
                    // Create the invoice
                    var period = DateTime.Parse(date);
                    var entry = BillSubscription(s, period, period, true, false);
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
        public IHttpActionResult Push(bool sendemail = false, bool dryRun = false) {
            // Get a copy of all active subscription
            var subscriptions = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);
            bool bNewInvoicesExists = false;

            // Build up a result list
            var result = new List<String>();
            var resultnew = new List<String>();

            if (dryRun) {
                result.Add("               This is just a dry run. Nothing will change.");
            }

            result.Add("               BCName,BCNo,Id,Description,FirstInvoice,BillingPeriod,InvoiceDate,NextInvoice,BillingCycle");

            foreach (var s in subscriptions) {

                DateTime billingstarts = new DateTime(s.InvoiceDate.Year, s.InvoiceDate.Month, 1);
                billingstarts = billingstarts.AddMonths(1);
                DateTime billingends = billingstarts;

                switch (s.BillingCycle)
                {
                    case "Monthly":
                        billingends = billingstarts.AddMonths(1);
                        break;
                    case "Quaterly":
                        billingends = billingstarts.AddMonths(3);
                        break;
                    case "Biannually":
                        billingends = billingstarts.AddMonths(6);
                        break;
                    case "Annually":
                        billingends = billingstarts.AddMonths(12);
                        break;
                }
                billingends = billingends.AddDays(-1);

                if (!s.IsDue()) {
                    if (dryRun) {
                        result.Add(String.Format(
                            "               {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                            s.NavCustomerName.Replace(",", ";"),
                            s.NavCustomerId.Replace(",", ";"),
                            s.Id,
                            s.Description.Replace(",", ";"),
                            s.FirstInvoice.ToString("dd/MM/yyyy"),
                            billingstarts.ToString("dd/MM/yyyy") + "--" + billingends.ToString("dd/MM/yyyy"),
                            s.InvoiceDate.ToString("dd/MM/yyyy"),
                            s.NextInvoice.ToString("dd/MM/yyyy"),
                            s.BillingCycle
                        ));

                        /*
                        s.NextInvoice = s.InvoiceDate;
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        */
                    }
                    continue;
                }

                var entry = BillSubscription(s, s.BillingPeriod, s.InvoiceDate, false, dryRun);

                if (entry["message"].ToString().IndexOf("New invoice to") != -1)
                {
                    result.Add(String.Format(
                        "NEW_INVOICE    {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        s.NavCustomerName.Replace(",", ";"),
                        s.NavCustomerId.Replace(",", ";"),
                        s.Id,
                        s.Description.Replace(",", ";"),
                        s.FirstInvoice.ToString("dd/MM/yyyy"),
                        billingstarts.ToString("dd/MM/yyyy") + "--" + billingends.ToString("dd/MM/yyyy"),
                        s.InvoiceDate.ToString("dd/MM/yyyy"),
                        s.NextInvoice.ToString("dd/MM/yyyy"),
                        s.BillingCycle
                    ));

                    resultnew.Add(String.Format(
                        "NEW_INVOICE    {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        s.NavCustomerName.Replace(",", ";"),
                        s.NavCustomerId.Replace(",", ";"),
                        s.Id,
                        s.Description.Replace(",", ";"),
                        s.FirstInvoice.ToString("dd/MM/yyyy"),
                        billingstarts.ToString("dd/MM/yyyy") + "--" + billingends.ToString("dd/MM/yyyy"),
                        s.InvoiceDate.ToString("dd/MM/yyyy"),
                        s.NextInvoice.ToString("dd/MM/yyyy"),
                        s.BillingCycle
                    ));

                    bNewInvoicesExists = true;
                }

                // save next invoice date
                if (!dryRun)
                {
                    // update billing cycle now
                    s.NextInvoice = s.InvoiceDate;
                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                    /*
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
                    */
                }
                
            }

            // Save changes made to the database
            try {
                if (!dryRun)
                {
                    db.SaveChanges();
                }
            }
            catch (Exception) {
                result.Add("De overstående aftaler kunne ikke gemmes i den lokale database, og deres 'First Invoice' skal opdateres manuelt.");
                if (dryRun) { 
                    throw;
                }
            }

            // Send the result email
            if (dryRun)
            {
                string recipients = "finance@rackpeople.com;bogholderi@rackpeople.dk;sa@rackpeople.dk;aop@rackpeople.dk;ltp@rackpeople.dk;mz@rackpeople.dk";
                //string recipients = "mz@rackpeople.dk;zivic.milan@gmail.com";
                try
                {
                    if (sendemail == true)
                    {
                        if (bNewInvoicesExists == true)
                        {
                            this.SendResultEmail(resultnew, recipients);
                        }
                    }
                }
                catch (Exception)
                {
                    result.Add(String.Format("Failed to send an email to '{0}'", recipients));
                }
            }

            // Render

            return Ok(result);
        }
    }
}
