﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using TeleBilling_v02_.Models;
using TeleBilling_v02_.Models.Navision;
using TeleBilling_v02_.NAVSalesInvoice;
using System.Globalization;

namespace TeleBilling_v02_.Repository.Navision
{
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
        public double inventory { get; set; }
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

    public class BCCustomers
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        public List<BCCustomer> value { get; set; }
    }

    public class BCCustomer
    {
        public string id { get; set; }
        public string number { get; set; }
        public string displayName { get; set; }
        public string type { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string postalCode { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public bool taxLiable { get; set; }
        public string taxAreaId { get; set; }
        public string taxAreaDisplayName { get; set; }
        public string taxRegistrationNumber { get; set; }
        public string currencyId { get; set; }
        public string currencyCode { get; set; }
        public string paymentTermsId { get; set; }
        public string shipmentMethodId { get; set; }
        public string paymentMethodId { get; set; }
        public string blocked { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
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

    public class InvoiceGenerator
    {
        private static string DoesCustomerExists(string filter, string sBCToken)
        {
            string sResult = "n/a";

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

                    webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

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
            }

            return sResult;
        }

        private static string GetBCToken()
        {
            string sResult = "n/a";

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                string sDate = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                sDate += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                sDate += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                sDate += DateTime.Now.AddMinutes(10).Hour.ToString().PadLeft(2, '0') + ":";
                sDate += DateTime.Now.AddMinutes(10).Minute.ToString().PadLeft(2, '0') + ":";
                sDate += DateTime.Now.AddMinutes(10).Second.ToString().PadLeft(2, '0') + ".000";

                string strSqlQuery = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[BCLoginLog] WHERE [TokenExpiresAt] > '" + sDate + "' ORDER BY Id DESC";
                System.Data.OleDb.OleDbDataReader oleReader;
                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSqlQuery, dbConn);
                oleReader = cmd.ExecuteReader();
                if (oleReader.Read())
                {
                    if (!oleReader.IsDBNull(1))
                    {
                        string sAuthToken = oleReader.GetString(1);
                        string sTokenType = oleReader.GetString(2);
                        int lExpiresIn = oleReader.GetInt32(3);
                        DateTime dExpiresAt = oleReader.GetDateTime(4);

                        if (DateTime.Now.AddMinutes(15) < dExpiresAt)
                        {
                            sResult = sAuthToken;
                        }
                    }
                }
                oleReader.Close();

                dbConn.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = "n/a";
            }

            return sResult;
        }

        private static string GetItemId(string sItemName, string sBCToken)
        {
            string sResult = "n/a";

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

                var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/items?$filter=number eq '" + sItemName + "'") as HttpWebRequest;
                if (webRequestAUTH != null)
                {
                    webRequestAUTH.Method = "GET";
                    webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                    webRequestAUTH.ContentType = "application/json";
                    webRequestAUTH.MediaType = "application/json";
                    webRequestAUTH.Accept = "application/json";

                    webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

                    using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                    {
                        using (var srW = new StreamReader(rW))
                        {
                            var sExportAsJson = srW.ReadToEnd();
                            var sExport = JsonConvert.DeserializeObject<GetItems>(sExportAsJson);
                            foreach (var it in sExport.value)
                            {
                                sResult = it.id;
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
                sResult = "n/a";
            }

            return sResult;
        }

        public static List<string> BillDidww(IEnumerable<InvoiceModel> billableList)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            List<string> errorMsg = new List<string>();
            string msg = string.Empty;

            string sAuthToken = GetBCToken();

            if (sAuthToken != "n/a")
            {
                string sItem10036 = "3010.015"; // 10036 
                string sItem10036Id = GetItemId(sItem10036, sAuthToken);

                string sItem10037 = "3010.020"; // 10037
                string sItem10037Id = GetItemId(sItem10037, sAuthToken);

                foreach (InvoiceModel invoice in billableList)
                {
                    // create order first and create empty order lines
                    PostSalesInvoice order = new PostSalesInvoice();

                    string sBCCuromerData = DoesCustomerExists(invoice.CVR, sAuthToken);
                    if (sBCCuromerData == "n/a")
                    {
                        sBCCuromerData = "n/aђn/a";
                    }
                    string sCustomerVATNo = sBCCuromerData.Split('ђ')[0];
                    string sCustomerVATId = sBCCuromerData.Split('ђ')[1];

                    order.customerNumber = invoice.CVR;
                    order.billToCustomerNumber = invoice.CVR;
                    order.customerId = sCustomerVATId;
                    order.billToCustomerId = sCustomerVATId;

                    try
                    {
                        DateTime dtOrderDate = DateTime.Now;
                        order.invoiceDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                        order.postingDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }

                    // create invoice now
                    string sNewInvoiceId = "n/a";
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
                    }

                    // order lines
                    List<PostSalesInvoiceLine> orderlines = new List<PostSalesInvoiceLine>();
                    bool bPeriodAdded = false;

                    foreach (InvoiceLineCollectionModel line in invoice.LineCollections)
                    {
                        // Time period line
                        if (bPeriodAdded == false)
                        {
                            PostSalesInvoiceLine orderlinePeriod = new PostSalesInvoiceLine();
                            orderlinePeriod.itemId = "";
                            orderlinePeriod.lineType = "";
                            orderlinePeriod.lineObjectNumber = "";

                            DateTime dtMonthStart = new DateTime(line.StartDate.Year, line.StartDate.Month, 1);
                            DateTime dtMonthEnd = dtMonthStart.AddMonths(1).AddDays(-1);

                            orderlinePeriod.description = "Periode " + dtMonthStart.ToString("dd/MM/yyyy") + " til " + dtMonthEnd.ToString("dd/MM/yyyy");
                            orderlinePeriod.unitPrice = 0;
                            orderlinePeriod.quantity = 0;

                            orderlines.Add(orderlinePeriod);

                            // Filler line
                            PostSalesInvoiceLine fillerLinePerdiod = new PostSalesInvoiceLine();
                            fillerLinePerdiod.itemId = "";
                            fillerLinePerdiod.lineType = "";
                            fillerLinePerdiod.lineObjectNumber = "";
                            fillerLinePerdiod.description = "   ";
                            fillerLinePerdiod.unitPrice = 0;
                            fillerLinePerdiod.quantity = 0;
                            orderlines.Add(fillerLinePerdiod);

                            bPeriodAdded = true;
                        }

                        // Subscriber range line
                        PostSalesInvoiceLine orderlineNummerSerie = new PostSalesInvoiceLine();
                        orderlineNummerSerie.itemId = "";
                        orderlineNummerSerie.lineType = "";
                        orderlineNummerSerie.lineObjectNumber = "";
                        orderlineNummerSerie.description = "Nummerserie " + line.Subscriber_Range_Start + " - " + line.Subscriber_Range_End;
                        orderlineNummerSerie.unitPrice = 0;
                        orderlineNummerSerie.quantity = 0;

                        orderlines.Add(orderlineNummerSerie);

                        foreach (AccumulatedModel zoneLines in line.Accumulated)
                        {
                            if (zoneLines.Seconds > 0)
                            {
                                PostSalesInvoiceLine zoneCallsLine = new PostSalesInvoiceLine();

                                zoneCallsLine.lineType = "Item";
                                zoneCallsLine.lineObjectNumber = sItem10037; // 10037
                                zoneCallsLine.itemId = sItem10037Id;

                                zoneCallsLine.description = zoneLines.ZoneName; //Zone navn + opkald

                                double dQuant = (double)zoneLines.Seconds / 60.0;

                                //Beregn pris fra zonelines
                                zoneCallsLine.quantity = Convert.ToDecimal(dQuant.ToString());
                                //zoneCallsLine.Line_Amount = zoneLines.Call_price;// Math.Round((decimal)zoneLines.styk * zoneLines.Call_price, 4, MidpointRounding.AwayFromZero);
                                //zoneCallsLine.Total_Amount_Excl_VAT = zoneLines.Call_price;// Math.Round((decimal)zoneLines.styk * zoneLines.Call_price, 4, MidpointRounding.AwayFromZero);
                                zoneCallsLine.unitPrice = zoneLines.Minute_price;//Math.Round((zoneCallsLine.Total_Amount_Excl_VAT / zoneCallsLine.Quantity), 4, MidpointRounding.AwayFromZero);

                                orderlines.Add(zoneCallsLine);
                            }
                        }

                        // Filler line
                        PostSalesInvoiceLine fillerLine = new PostSalesInvoiceLine();
                        fillerLine.itemId = "";
                        fillerLine.lineType = "";
                        fillerLine.lineObjectNumber = "";
                        fillerLine.description = "******";
                        fillerLine.unitPrice = 0;
                        fillerLine.quantity = 0;
                        orderlines.Add(fillerLine);
                    }

                    // push item lines
                    if (sNewInvoiceId != "n/a")
                    { 
                        foreach (PostSalesInvoiceLine ord in orderlines)
                        {
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

                                    string sParams = "{\"itemId\": \"" + ord.itemId + "\", \"lineType\": \"" + ord.lineType + "\", \"lineObjectNumber\": \"" + ord.lineObjectNumber + "\", \"description\": \"" + ord.description + "\", \"unitPrice\": " + ord.unitPrice + ", \"quantity\": " + ord.quantity + "}";
                                    if (ord.itemId == "")
                                    {
                                        sParams = "{\"description\": \"" + ord.description + "\"}";
                                    }

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
                            }
                        }                        
                    }
                }
            }
            else
            {
                msg = "Can not connect to the BC";
                errorMsg.Add(msg);
            }

            return errorMsg;
        }

        public static List<string> Bill(IEnumerable<InvoiceModel> billableList)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            List<string> errorMsg = new List<string>();
            string msg = string.Empty;

            string sAuthToken = GetBCToken();

            if (sAuthToken != "n/a")
            {
                string sItem10036 = "3010.000"; // 10036 
                string sItem10036Id = GetItemId(sItem10036, sAuthToken);

                string sItem10037 = "3010.005"; // 10037
                string sItem10037Id = GetItemId(sItem10037, sAuthToken);

                foreach (InvoiceModel invoice in billableList)
                {
                    // create order first and create empty order lines
                    PostSalesInvoice order = new PostSalesInvoice();

                    string sBCCuromerData = DoesCustomerExists(invoice.CVR, sAuthToken);
                    if (sBCCuromerData == "n/a")
                    {
                        sBCCuromerData = "n/aђn/a";
                    }
                    string sCustomerVATNo = sBCCuromerData.Split('ђ')[0];
                    string sCustomerVATId = sBCCuromerData.Split('ђ')[1];

                    order.customerNumber = invoice.CVR;
                    order.billToCustomerNumber = invoice.CVR;
                    order.customerId = sCustomerVATId;
                    order.billToCustomerId = sCustomerVATId;

                    try
                    {
                        DateTime dtOrderDate = DateTime.Now;
                        order.invoiceDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                        order.postingDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }

                    // create invoice now
                    string sNewInvoiceId = "n/a";
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
                    }

                    // order lines
                    List<PostSalesInvoiceLine> orderlines = new List<PostSalesInvoiceLine>();

                    bool bPeriodAdded = false;

                    foreach (InvoiceLineCollectionModel line in invoice.LineCollections)
                    {
                        // Time period line
                        if (bPeriodAdded == false)
                        {
                            PostSalesInvoiceLine orderlinePeriod = new PostSalesInvoiceLine();
                            orderlinePeriod.itemId = "";
                            orderlinePeriod.lineType = "";
                            orderlinePeriod.lineObjectNumber = "";

                            DateTime dtMonthStart = new DateTime(line.StartDate.Year, line.StartDate.Month, 1);
                            DateTime dtMonthEnd = dtMonthStart.AddMonths(1).AddDays(-1);

                            orderlinePeriod.description = "Periode " + dtMonthStart.ToString("dd/MM/yyyy") + " til " + dtMonthEnd.ToString("dd/MM/yyyy");
                            orderlinePeriod.unitPrice = 0;
                            orderlinePeriod.quantity = 0;

                            orderlines.Add(orderlinePeriod);

                            // Filler line
                            PostSalesInvoiceLine fillerLinePerdiod = new PostSalesInvoiceLine();
                            fillerLinePerdiod.itemId = "";
                            fillerLinePerdiod.lineType = "";
                            fillerLinePerdiod.lineObjectNumber = "";
                            fillerLinePerdiod.description = "   ";
                            fillerLinePerdiod.unitPrice = 0;
                            fillerLinePerdiod.quantity = 0;
                            orderlines.Add(fillerLinePerdiod);

                            bPeriodAdded = true;
                        }

                        // Subscriber range line
                        PostSalesInvoiceLine orderlineNummerSerie = new PostSalesInvoiceLine();
                        orderlineNummerSerie.itemId = "";
                        orderlineNummerSerie.lineType = "";
                        orderlineNummerSerie.lineObjectNumber = "";
                        orderlineNummerSerie.description = "Nummerserie " + line.Subscriber_Range_Start + " - " + line.Subscriber_Range_End;
                        orderlineNummerSerie.unitPrice = 0;
                        orderlineNummerSerie.quantity = 0;

                        orderlines.Add(orderlineNummerSerie);

                        // Agreement description line(s)
                        if (line.Agreement_Description.Length > 0)
                        {
                            if (line.Agreement_Description.Length < 51)
                            {
                                PostSalesInvoiceLine orderlineNummerAgr = new PostSalesInvoiceLine();

                                orderlineNummerAgr.itemId = "";
                                orderlineNummerAgr.lineType = "";
                                orderlineNummerAgr.lineObjectNumber = "";
                                orderlineNummerAgr.description = line.Agreement_Description;
                                orderlineNummerAgr.unitPrice = 0;
                                orderlineNummerAgr.quantity = 0;

                                orderlines.Add(orderlineNummerAgr);
                            }
                            else
                            {
                                int intRunTimesAgreementDescription = 0;
                                while ((double)intRunTimesAgreementDescription < (((double)line.Agreement_Description.Length) / 50)) //Dodgy
                                {
                                    int intRemainingChars = line.Agreement_Description.Length - intRunTimesAgreementDescription * 50;
                                    if (intRemainingChars == 0)
                                    {
                                        break;
                                    }

                                    PostSalesInvoiceLine orderlineNummerAgrLD = new PostSalesInvoiceLine();
                                    orderlineNummerAgrLD.itemId = "";
                                    orderlineNummerAgrLD.lineType = "";
                                    orderlineNummerAgrLD.lineObjectNumber = "";
                                    orderlineNummerAgrLD.description = line.Agreement_Description.Substring((intRunTimesAgreementDescription * 50), (((double)intRemainingChars / 50) >= 1 ? 50 : intRemainingChars % 50));
                                    orderlineNummerAgrLD.unitPrice = 0;
                                    orderlineNummerAgrLD.quantity = 0;

                                    orderlines.Add(orderlineNummerAgrLD);
                                    intRunTimesAgreementDescription++;
                                }
                            }
                        }

                        foreach (AccumulatedModel zoneLines in line.Accumulated)
                        {
                            if (zoneLines.Minute_price == 0)
                            {
                                string strNewDescription = "Ingen minuttakst for " + zoneLines.ZoneName;
                                if (strNewDescription.Length < 51)
                                {
                                    PostSalesInvoiceLine orderline1 = new PostSalesInvoiceLine();

                                    orderline1.itemId = "";
                                    orderline1.lineType = "";
                                    orderline1.lineObjectNumber = "";
                                    orderline1.description = "Ingen minuttakst for " + zoneLines.ZoneName;
                                    orderline1.unitPrice = 0;
                                    orderline1.quantity = 0;

                                    orderlines.Add(orderline1);
                                }
                                else
                                {
                                    PostSalesInvoiceLine orderline2 = new PostSalesInvoiceLine();

                                    orderline2.itemId = "";
                                    orderline2.lineType = "";
                                    orderline2.lineObjectNumber = "";
                                    orderline2.description = "Ingen minuttakst for ";
                                    orderline2.unitPrice = 0;
                                    orderline2.quantity = 0;

                                    orderlines.Add(orderline2);

                                    PostSalesInvoiceLine orderline3 = new PostSalesInvoiceLine();

                                    orderline3.itemId = "";
                                    orderline3.lineType = "";
                                    orderline3.lineObjectNumber = "";
                                    orderline3.description = zoneLines.ZoneName;
                                    orderline3.unitPrice = 0;
                                    orderline3.quantity = 0;

                                    orderlines.Add(orderline3);
                                }
                            }
                            else if (zoneLines.Seconds > 0)
                            {
                                PostSalesInvoiceLine zoneTimeLine = new PostSalesInvoiceLine();

                                zoneTimeLine.lineType = "Item";
                                zoneTimeLine.lineObjectNumber = sItem10037; // 10037
                                zoneTimeLine.itemId = sItem10037Id;
                                zoneTimeLine.description = zoneLines.ZoneName + " minutter"; //Zone navn + tid

                                // Beregn pris fra zonelines
                                zoneTimeLine.quantity = Math.Round(Convert.ToDecimal(zoneLines.Seconds) / 60, 2, MidpointRounding.AwayFromZero);
                                //zoneTimeLine.Line_Amount = Math.Round((Convert.ToDecimal(zoneLines.Seconds) / 60) * zoneLines.Minute_price, 4, MidpointRounding.AwayFromZero);
                                //zoneTimeLine.Total_Amount_Excl_VAT = Math.Round((Convert.ToDecimal(zoneLines.Seconds) / 60) * zoneLines.Minute_price, 4, MidpointRounding.AwayFromZero);
                                decimal TAEVAT = Math.Round((Convert.ToDecimal(zoneLines.Seconds) / 60) * zoneLines.Minute_price, 4, MidpointRounding.AwayFromZero);
                                zoneTimeLine.unitPrice = Math.Round((TAEVAT / zoneTimeLine.quantity), 4, MidpointRounding.AwayFromZero);

                                orderlines.Add(zoneTimeLine);
                            }
                            else
                            {
                                string strNewDescription = "0 minutter for opkald til " + zoneLines.ZoneName;
                                if (strNewDescription.Length < 51)
                                {
                                    PostSalesInvoiceLine orderline4 = new PostSalesInvoiceLine();

                                    orderline4.itemId = "";
                                    orderline4.lineType = "";
                                    orderline4.lineObjectNumber = "";
                                    orderline4.description = "0 minutter for opkald til " + zoneLines.ZoneName;
                                    orderline4.unitPrice = 0;
                                    orderline4.quantity = 0;

                                    orderlines.Add(orderline4);
                                }
                                else
                                {
                                    PostSalesInvoiceLine orderline5 = new PostSalesInvoiceLine();

                                    orderline5.itemId = "";
                                    orderline5.lineType = "";
                                    orderline5.lineObjectNumber = "";
                                    orderline5.description = "0 minutter for opkald til ";
                                    orderline5.unitPrice = 0;
                                    orderline5.quantity = 0;

                                    orderlines.Add(orderline5);

                                    PostSalesInvoiceLine orderline6 = new PostSalesInvoiceLine();

                                    orderline6.itemId = "";
                                    orderline6.lineType = "";
                                    orderline6.lineObjectNumber = "";
                                    orderline6.description = zoneLines.ZoneName;
                                    orderline6.unitPrice = 0;
                                    orderline6.quantity = 0;

                                    orderlines.Add(orderline6);
                                }
                            }

                            // Do zone calls line
                            if (zoneLines.Call_price == 0)
                            {
                                string strNewDescription = "Ingen opkaldsafgift for " + zoneLines.ZoneName;
                                if (strNewDescription.Length < 51)
                                {
                                    PostSalesInvoiceLine orderline7 = new PostSalesInvoiceLine();

                                    orderline7.itemId = "";
                                    orderline7.lineType = "";
                                    orderline7.lineObjectNumber = "";
                                    orderline7.description = "Ingen opkaldsafgift for " + zoneLines.ZoneName;
                                    orderline7.unitPrice = 0;
                                    orderline7.quantity = 0;

                                    orderlines.Add(orderline7);
                                }
                                else
                                {
                                    PostSalesInvoiceLine orderline8 = new PostSalesInvoiceLine();

                                    orderline8.itemId = "";
                                    orderline8.lineType = "";
                                    orderline8.lineObjectNumber = "";
                                    orderline8.description = "Ingen opkaldsafgift for ";
                                    orderline8.unitPrice = 0;
                                    orderline8.quantity = 0;

                                    orderlines.Add(orderline8);

                                    PostSalesInvoiceLine orderline9 = new PostSalesInvoiceLine();

                                    orderline9.itemId = "";
                                    orderline9.lineType = "";
                                    orderline9.lineObjectNumber = "";
                                    orderline9.description = zoneLines.ZoneName;
                                    orderline9.unitPrice = 0;
                                    orderline9.quantity = 0;

                                    orderlines.Add(orderline9);
                                }
                            }
                            else
                            {
                                PostSalesInvoiceLine zoneCallsLine = new PostSalesInvoiceLine();

                                zoneCallsLine.lineType = "Item";
                                zoneCallsLine.lineObjectNumber = sItem10036; // 10036
                                zoneCallsLine.itemId = sItem10036Id;
                                zoneCallsLine.description = zoneLines.ZoneName + " opkald"; //Zone navn + tid

                                //Beregn pris fra zonelines
                                zoneCallsLine.quantity = (decimal)zoneLines.styk;
                                //zoneCallsLine.Line_Amount = Math.Round((decimal)zoneLines.styk * zoneLines.Call_price, 4, MidpointRounding.AwayFromZero);
                                //zoneCallsLine.Total_Amount_Excl_VAT = Math.Round((decimal)zoneLines.styk * zoneLines.Call_price, 4, MidpointRounding.AwayFromZero);
                                decimal TAEVAT = Math.Round((decimal)zoneLines.styk * zoneLines.Call_price, 4, MidpointRounding.AwayFromZero);
                                zoneCallsLine.unitPrice = Math.Round((TAEVAT / zoneCallsLine.quantity), 4, MidpointRounding.AwayFromZero);

                                orderlines.Add(zoneCallsLine);
                            }

                        }

                        // Filler line
                        PostSalesInvoiceLine orderlineFL = new PostSalesInvoiceLine();

                        orderlineFL.itemId = "";
                        orderlineFL.lineType = "";
                        orderlineFL.lineObjectNumber = "";
                        orderlineFL.description = "******";
                        orderlineFL.unitPrice = 0;
                        orderlineFL.quantity = 0;

                        orderlines.Add(orderlineFL);
                    }

                    // push item lines
                    if (sNewInvoiceId != "n/a")
                    {
                        foreach (PostSalesInvoiceLine ord in orderlines)
                        {
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

                                    string sParams = "{\"itemId\": \"" + ord.itemId + "\", \"lineType\": \"" + ord.lineType + "\", \"lineObjectNumber\": \"" + ord.lineObjectNumber + "\", \"description\": \"" + ord.description + "\", \"unitPrice\": " + ord.unitPrice + ", \"quantity\": " + ord.quantity + "}";
                                    if (ord.itemId == "")
                                    {
                                        sParams = "{\"description\": \"" + ord.description + "\"}";
                                    }

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
                            }
                        }
                    }


                }
            }
            else
            {
                msg = "Can not connect to the BC";
                errorMsg.Add(msg);
            }

            return errorMsg;
        }
    }
}