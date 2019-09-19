using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RPNAVConnect.NAVCustomersWS;
using RPNAVConnect.NAVOrdersWS;

using System.Net;
using System.Xml;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data.OleDb;
using System.Globalization;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Web.Script.Serialization;

namespace RPNAVConnect
{
    public partial class WHMCSImportDF : System.Web.UI.Page
    {
        public static string sNAVLogin = "rpnavapi";
        public static string sNAVPassword = "Telefon1";
        public static string sNAVDomain = "";

        public string sToolboxUrl = "https://toolbox.rackpeople.com/mumle2nav/var/files/result.json";

        protected void Page_Load(object sender, EventArgs e)
        {
            // nothing to do here
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            //Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'", cert.Subject, error.ToString());
            return false;
        }

        protected void WHMCSDataB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sResultJsonFilePath = Request.PhysicalApplicationPath + "whmcs\\" + "result.json";

            // get result.json
            bool bJsonFound = false;

            // trying to get new file
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                using (WebClient webClient = new WebClient())
                {
                    webClient.UseDefaultCredentials = true;
                    webClient.Credentials = new NetworkCredential("mz", "Telefon1");
                    var stream = webClient.OpenRead(sToolboxUrl);
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        var page = sr.ReadToEnd();

                        // save html
                        try
                        {
                            System.IO.File.WriteAllText(sResultJsonFilePath, page);
                            bJsonFound = true;
                        }
                        catch (Exception ex)
                        {
                            WHMCSDataL.Text = ex.ToString();
                            bJsonFound = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WHMCSDataL.Text = ex.ToString();
                bJsonFound = false;
            }

            // load result.json
            if (bJsonFound == true)
            {
                using (StreamReader r = new StreamReader(sResultJsonFilePath))
                {
                    string json = r.ReadToEnd();
                    var jss = new JavaScriptSerializer();
                    RootObject sLoginData = jss.Deserialize<RootObject>(json);

                    int iInvoicesAllCount = 0;

                    int iCustomersCount = 0;
                    foreach (Client clientsingle in sLoginData.clients)
                    {
                        int iInvoicesCount = 0;
                        foreach (Invoice invoicesingle in clientsingle.invoices)
                        {
                            if (iInvoicesCount == 0)
                            {
                                if (iCustomersCount == 0)
                                {
                                    WHMCSDataL.Text += "<table cellpadding='3' cellspacing='3' border='0' width='100%'>";
                                    iCustomersCount++;
                                }

                                // customer - show only if invoices exists
                                WHMCSDataL.Text += "<tr>";
                                WHMCSDataL.Text += "<td><a href='javascript:toogleInvoices(\"" + clientsingle.client.userid + "\");'><b>" + clientsingle.client.companyname + "</b></a><br/>";
                                WHMCSDataL.Text += "<div style='display: none;' id='cgs_" + clientsingle.client.userid + "' name='cgs_" + clientsingle.client.userid + "'>";
                                WHMCSDataL.Text += clientsingle.client.address1 + "<br />";
                                WHMCSDataL.Text += clientsingle.client.postcode + " " + clientsingle.client.city + "<br />";
                                WHMCSDataL.Text += clientsingle.client.country + "<br />";
                                WHMCSDataL.Text += "Tel. " + clientsingle.client.phonenumber + "<br /><br />";
                                WHMCSDataL.Text += "UserID: " + clientsingle.client.userid + "<br />";
                                WHMCSDataL.Text += "WHMCS No: " + clientsingle.client.customfields1 + "<br /><br />";
                                WHMCSDataL.Text += "</div>";
                                WHMCSDataL.Text += "</td>";
                                WHMCSDataL.Text += "</tr>";

                                WHMCSDataL.Text += "<tr><td>";
                                WHMCSDataL.Text += "<div style='display: none;' id='cg_" + clientsingle.client.userid + "' name='cg_" + clientsingle.client.userid + "'>";
                            }

                            iInvoicesCount++;

                            int iInvoiceLinesCount = 0;
                            WHMCSDataL.Text += "<table border='0' width='100%'>";
                            WHMCSDataL.Text += "<tr><td colspan='4' style='border-bottom:1pt solid black;'>&nbsp;Invoice No: " + invoicesingle.invoicenum + "&nbsp;";
                            WHMCSDataL.Text += "&nbsp;Invoice Date: " + invoicesingle.date + "&nbsp;</td>";
                            WHMCSDataL.Text += "</td></tr>";

                            foreach (Item invoicelinesingle in invoicesingle.items.item)
                            {
                                // remove multiple spaces & odd empty chars
                                RegexOptions options = RegexOptions.None;
                                Regex regex = new Regex(@"[ ]{2,}", options);
                                string sLineDescription = invoicelinesingle.description.Replace("\\", "");
                                sLineDescription = regex.Replace(sLineDescription, @" ");
                                sLineDescription = Regex.Replace(sLineDescription, @"\p{Z}", " ");

                                string sProjectName = invoicelinesingle.description.Replace("\\", "");
                                bool bAllInvoiceLinesPrinted = false;

                                int iInvLinesCount = 0;
                                while (bAllInvoiceLinesPrinted == false)
                                {
                                    string sInvoiceLineDescToPrint = "";
                                    if (sLineDescription.Length <= 50)
                                    {
                                        if (iInvLinesCount == 0)
                                        {
                                            // extra line
                                            sInvoiceLineDescToPrint = sProjectName;
                                            if (sInvoiceLineDescToPrint.Length > 50)
                                            {
                                                sInvoiceLineDescToPrint = sInvoiceLineDescToPrint.Substring(0, 50);
                                            }
                                            WHMCSDataL.Text += "<tr>";
                                            WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;" + invoicesingle.date + "&nbsp;</td>";
                                            WHMCSDataL.Text += "<td style='width:55%;'>&nbsp;<b>" + invoicelinesingle.type + "</b>&nbsp;</td>";

                                            // quantity and price
                                            decimal dQuantity = 1;
                                            decimal dUnitPrice = 0;
                                            try
                                            {
                                                dUnitPrice = Convert.ToDecimal(invoicelinesingle.amount);
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.ToString();
                                                dUnitPrice = 0;
                                            }

                                            WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;" + dQuantity.ToString("N") + "&nbsp;</td>";
                                            WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;" + dUnitPrice.ToString("N") + "&nbsp;</td>";
                                            WHMCSDataL.Text += "</tr>";
                                            iInvLinesCount++;
                                        }

                                        // description
                                        sInvoiceLineDescToPrint = sLineDescription;
                                        WHMCSDataL.Text += "<tr>";
                                        WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                        WHMCSDataL.Text += "<td style='width:55%;'>&nbsp;" + sInvoiceLineDescToPrint + "&nbsp;</td>";
                                        WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                        WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                        WHMCSDataL.Text += "</tr>";
                                        iInvLinesCount++;

                                        bAllInvoiceLinesPrinted = true;
                                    }
                                    else
                                    {
                                        // create as many new lines as needed to fit comment length
                                        int partLength = 50;

                                        string sLineDescriptionFriendlyChars = sLineDescription.Replace(" ", "≡");
                                        string[] sLineDescriptionWords = sLineDescriptionFriendlyChars.Split('≡');
                                        var parts = new Dictionary<int, string>();
                                        string part = string.Empty;
                                        int partCounter = 0;
                                        foreach (var sLineDescriptionWord in sLineDescriptionWords)
                                        {
                                            if (part.Length + sLineDescriptionWord.Length < partLength)
                                            {
                                                part += string.IsNullOrEmpty(part) ? sLineDescriptionWord : " " + sLineDescriptionWord;
                                            }
                                            else
                                            {
                                                parts.Add(partCounter, part);
                                                part = sLineDescriptionWord;
                                                partCounter++;
                                            }
                                        }
                                        parts.Add(partCounter, part);

                                        int iPartsCount = 0;
                                        foreach (var item in parts)
                                        {
                                            if (iPartsCount == 0)
                                            {
                                                if (iInvLinesCount == 0)
                                                {
                                                    // extra line
                                                    sInvoiceLineDescToPrint = sProjectName;
                                                    if (sInvoiceLineDescToPrint.Length > 50)
                                                    {
                                                        sInvoiceLineDescToPrint = sInvoiceLineDescToPrint.Substring(0, 50);
                                                    }
                                                    WHMCSDataL.Text += "<tr>";
                                                    WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;" + invoicesingle.duedate + "&nbsp;</td>";
                                                    WHMCSDataL.Text += "<td style='width:55%;'>&nbsp;<b>" + invoicelinesingle.type + "</b>&nbsp;</td>";
                                                    // quantity and price
                                                    decimal dQuantity = 1;
                                                    decimal dUnitPrice = 0;
                                                    try
                                                    {
                                                        dUnitPrice = Convert.ToDecimal(invoicelinesingle.amount);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                        dUnitPrice = 0;
                                                    }

                                                    WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;" + dQuantity.ToString("N") + "&nbsp;</td>";
                                                    WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;" + dUnitPrice.ToString("N") + "&nbsp;</td>";
                                                    WHMCSDataL.Text += "</tr>";
                                                    iInvLinesCount++;
                                                }

                                                // description
                                                sInvoiceLineDescToPrint = item.Value;
                                                WHMCSDataL.Text += "<tr>";
                                                WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                WHMCSDataL.Text += "<td style='width:55%;'>&nbsp;" + sInvoiceLineDescToPrint + "&nbsp;</td>";
                                                WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                WHMCSDataL.Text += "</tr>";
                                                iInvLinesCount++;
                                            }
                                            else
                                            {
                                                // description
                                                sInvoiceLineDescToPrint = item.Value;
                                                WHMCSDataL.Text += "<tr>";
                                                WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                WHMCSDataL.Text += "<td style='width:55%;'>&nbsp;" + sInvoiceLineDescToPrint + "&nbsp;</td>";
                                                WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                WHMCSDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                WHMCSDataL.Text += "</tr>";
                                                iInvLinesCount++;
                                            }
                                            iPartsCount++;
                                        }
                                        bAllInvoiceLinesPrinted = true;
                                    }
                                }
                                iInvoiceLinesCount = iInvoiceLinesCount + iInvLinesCount;
                            }

                            if (iInvoiceLinesCount == 0)
                            {
                                WHMCSDataL.Text += "<tr><td colspan='5'>&nbsp;<i>No invoice lines found</i>&nbsp;</td></tr>";
                            }
                            WHMCSDataL.Text += "<tr height='15px'><td colspan='5' height='15px'>&nbsp;</td></tr>";
                            WHMCSDataL.Text += "</table>";

                        }

                        // toggle div 
                        if (iInvoicesCount > 0)
                        {
                            WHMCSDataL.Text += "</div>";
                            WHMCSDataL.Text += "</td></tr>";
                        }

                        // all invoices count
                        iInvoicesAllCount = iInvoicesAllCount + iInvoicesCount;
                    }

                    if (iCustomersCount > 0)
                    {
                        WHMCSDataL.Text += "</table>";
                    }

                    if (iInvoicesAllCount > 0)
                    {
                        PPSep1.Visible = true;
                        PushDataToNavB.Visible = true;
                        PushingDataL.Text = "<p style='height:15px;'>&nbsp;</p>";
                    }
                    else
                    {
                        WHMCSDataL.Text += "<i>Der blev ikke fundet bilag til fakturering i WHMCS</i><br />";
                    }

                    // scroll down
                    ClientScript.RegisterStartupScript(GetType(), "ScrollScript", "window.onload = function() {document.getElementById('lastscriptdiv').scrollIntoView(true);}", true);
                }
            }
        }

        public class Customfield
        {
            public string id { get; set; }
            public string value { get; set; }
        }

        public class Client2
        {
            public string result { get; set; }
            public int userid { get; set; }
            public int id { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string fullname { get; set; }
            public string companyname { get; set; }
            public string email { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public string fullstate { get; set; }
            public string state { get; set; }
            public string postcode { get; set; }
            public string countrycode { get; set; }
            public string country { get; set; }
            public string phonenumber { get; set; }
            public string password { get; set; }
            public string statecode { get; set; }
            public string countryname { get; set; }
            public int phonecc { get; set; }
            public string phonenumberformatted { get; set; }
            public int billingcid { get; set; }
            public string notes { get; set; }
            public bool twofaenabled { get; set; }
            public int currency { get; set; }
            public string defaultgateway { get; set; }
            public string cctype { get; set; }
            public string cclastfour { get; set; }
            public int securityqid { get; set; }
            public string securityqans { get; set; }
            public int groupid { get; set; }
            public string status { get; set; }
            public string credit { get; set; }
            public bool taxexempt { get; set; }
            public bool latefeeoveride { get; set; }
            public bool overideduenotices { get; set; }
            public bool separateinvoices { get; set; }
            public bool disableautocc { get; set; }
            public bool emailoptout { get; set; }
            public bool overrideautoclose { get; set; }
            public string language { get; set; }
            public string lastlogin { get; set; }
            public string customfields1 { get; set; }
            public List<Customfield> customfields { get; set; }
            public string customfields2 { get; set; }
            public string customfields3 { get; set; }
            public string customfields4 { get; set; }
            public string customfields5 { get; set; }
            public string customfields6 { get; set; }
            public string customfields7 { get; set; }
            public string currency_code { get; set; }
        }

        public class Item
        {
            public string id { get; set; }
            public string type { get; set; }
            public string relid { get; set; }
            public string description { get; set; }
            public string amount { get; set; }
            public string taxed { get; set; }
        }

        public class Items
        {
            public List<Item> item { get; set; }
        }

        public class Invoice
        {
            public string result { get; set; }
            public string invoiceid { get; set; }
            public string invoicenum { get; set; }
            public string userid { get; set; }
            public string date { get; set; }
            public string duedate { get; set; }
            public string datepaid { get; set; }
            public string subtotal { get; set; }
            public string credit { get; set; }
            public string tax { get; set; }
            public string tax2 { get; set; }
            public string total { get; set; }
            public string balance { get; set; }
            public string taxrate { get; set; }
            public string taxrate2 { get; set; }
            public string status { get; set; }
            public string paymentmethod { get; set; }
            public string notes { get; set; }
            public bool ccgateway { get; set; }
            public Items items { get; set; }
            public string transactions { get; set; }
        }

        public class Client
        {
            public Client2 client { get; set; }
            public List<Invoice> invoices { get; set; }
        }

        public class RootObject
        {
            public List<Client> clients { get; set; }
            public string duration { get; set; }
        }

        protected void PushDataToNavB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            int iInvoiceNumber = 0;
            int iInvoiceAllLinesCount = 0;
            string sResultMessage = "";
            string sMissedCustomers = "";
            string sProblematicCustomers = "";

            string sResultJsonFilePath = Request.PhysicalApplicationPath + "whmcs\\" + "result.json";

            // get result.json
            bool bJsonFound = false;

            // trying to get new file
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                using (WebClient webClient = new WebClient())
                {
                    webClient.UseDefaultCredentials = true;
                    webClient.Credentials = new NetworkCredential("mz", "Telefon1");
                    var stream = webClient.OpenRead(sToolboxUrl);
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        var page = sr.ReadToEnd();

                        // save html
                        try
                        {
                            System.IO.File.WriteAllText(sResultJsonFilePath, page);
                            bJsonFound = true;
                        }
                        catch (Exception ex)
                        {
                            WHMCSDataL.Text = ex.ToString();
                            bJsonFound = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WHMCSDataL.Text = ex.ToString();
                bJsonFound = false;
            }

            // load result.json
            if (bJsonFound == true)
            {
                using (StreamReader r = new StreamReader(sResultJsonFilePath))
                {
                    string json = r.ReadToEnd();
                    var jss = new JavaScriptSerializer();
                    RootObject sLoginData = jss.Deserialize<RootObject>(json);

                    // open db connection
                    string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                    System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                    dbConn.Open();

                    try
                    {
                        // get access to NAVDebtor
                        CustomerInfo2_Service nav = new CustomerInfo2_Service();
                        nav.UseDefaultCredentials = true;
                        //nav.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword, sNAVDomain);
                        nav.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword);

                        // get access to NAVSalgsordre
                        //SalesOrder_Service_Service sal = new SalesOrder_Service_Service();
                        SalesInvoice_Service_Service sal = new SalesInvoice_Service_Service();
                        sal.UseDefaultCredentials = true;
                        //sal.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword, sNAVDomain);
                        sal.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword);

                        foreach (Client clientsingle in sLoginData.clients)
                        {
                            if (clientsingle.client.customfields1 != "")
                            {
                                // searh filter for the customer
                                List<CustomerInfo2_Filter> filterArray = new List<CustomerInfo2_Filter>();
                                CustomerInfo2_Filter nameFilter = new CustomerInfo2_Filter();
                                nameFilter.Field = CustomerInfo2_Fields.No;
                                nameFilter.Criteria = clientsingle.client.customfields1;
                                filterArray.Add(nameFilter);

                                // searh if this customer already exists
                                bool bCustomerCreatedInNav = false;
                                if (DoesCustomerExists(nav, filterArray) == false)
                                {
                                    try
                                    {
                                        // customer doesn't exists - create customer first
                                        CustomerInfo2 cust = new CustomerInfo2();
                                        cust.No = clientsingle.client.customfields1;

                                        // NAV restriction for 50 chars max
                                        if (clientsingle.client.companyname.Length > 50)
                                        {
                                            cust.Name = clientsingle.client.companyname.Substring(0, 50);
                                        }
                                        else
                                        {
                                            cust.Name = clientsingle.client.companyname;
                                        }
                                        cust.Address = clientsingle.client.address1;
                                        cust.Post_Code = clientsingle.client.postcode;
                                        cust.City = clientsingle.client.city;
                                        cust.Phone_No = clientsingle.client.phonenumber;
                                        //cust.E_Mail = clientsingle.client.email;
                                        cust.Search_Name = clientsingle.client.userid.ToString();

                                        string strFullName = clientsingle.client.fullname;
                                        string strInitial = "";
                                        strFullName.Split(' ').ToList().ForEach(i => strInitial += i[0]);
                                        cust.Salesperson_Code = strInitial;

                                        // workaround for fields that should be retrieved by SP
                                        cust.Payment_Terms_Code = "NET8";
                                        cust.Gen_Bus_Posting_Group = "INDLAND";
                                        cust.Customer_Posting_Group = "DANMARK";
                                        cust.VAT_Bus_Posting_Group = "INDLAND";

                                        // first try to create customer
                                        string sAddingError = "";
                                        try
                                        {
                                            nav.Create(ref cust);
                                            bCustomerCreatedInNav = true;
                                            sMissedCustomers += cust.Name.Replace("<br />", "") + " " + cust.No.Replace("<br />", "") + "<br />";
                                        }
                                        catch (Exception exIn)
                                        {
                                            sAddingError = exIn.ToString();
                                        }

                                        // second try
                                        if (sAddingError.ToLower().IndexOf("that cannot be found in the related table (salesperson/purchaser)") != -1)
                                        {
                                            cust.Salesperson_Code = "DOE";
                                            nav.Create(ref cust);
                                            bCustomerCreatedInNav = true;
                                            sMissedCustomers += cust.Name.Replace("<br />", "") + " " + cust.No.Replace("<br />", "") + "<br />";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                        bCustomerCreatedInNav = false;
                                    }
                                }


                                // searh if this customer already exists
                                if ((DoesCustomerExists(nav, filterArray) == true) || (bCustomerCreatedInNav == true))
                                {
                                    // iterate all invoices
                                    foreach (Invoice invoicesingle in clientsingle.invoices)
                                    {


                                        // search if invoice already processed
                                        if (IsOrderAlreadyProcessed(invoicesingle.invoiceid, dbConn) == false)
                                        {
                                            // get invoice lines count
                                            int iInvoiceLinesNumber = 0;
                                            foreach (Item invoicelinesingle in invoicesingle.items.item)
                                            {
                                                if (invoicelinesingle.id != "")
                                                {
                                                    iInvoiceLinesNumber++;
                                                }
                                            }

                                            // create new invoice
                                            if (iInvoiceLinesNumber == 0)
                                            {
                                                // create order first and create empty order lines
                                                SalesInvoice_Service order = new SalesInvoice_Service();
                                                sal.Create(ref order);

                                                // invoice data
                                                order.Sell_to_Customer_No = clientsingle.client.customfields1;

                                                // date YYYY-MM-DD
                                                if (invoicesingle.date.Length >= 10)
                                                {
                                                    string sYYYY = invoicesingle.date.Substring(0, 4);
                                                    string sMM = invoicesingle.date.Substring(5, 2);
                                                    string sDD = invoicesingle.date.Substring(8, 2);
                                                    try
                                                    {
                                                        DateTime dtOrderDate = new DateTime(Convert.ToInt32(sYYYY), Convert.ToInt32(sMM), Convert.ToInt32(sDD));
                                                        order.Posting_Date = dtOrderDate;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                    }
                                                }

                                                sal.Update(ref order);

                                                // processed invoices count
                                                iInvoiceNumber++;

                                                string sCurrentDateTime = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                                                sCurrentDateTime += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                                                sCurrentDateTime += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                                                sCurrentDateTime += DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":";
                                                sCurrentDateTime += DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":";
                                                sCurrentDateTime += DateTime.Now.Second.ToString().PadLeft(2, '0');

                                                string sSql = "INSERT INTO [RPNAVConnect].[dbo].[Log] ([refid] ,[result] ,[source] ,[datestamp] ,[description]) ";
                                                sSql += "VALUES ('" + invoicesingle.invoiceid.Replace("'", "\"") + "', 'Pushed', 'WHMCS', '" + sCurrentDateTime + "', 'No Invoice Lines')";
                                                string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                                                if (sDBResult != "DBOK")
                                                {
                                                    sResultMessage += sDBResult + "<br />";
                                                }

                                            }
                                            else
                                            {
                                                // create order first and create empty order lines
                                                SalesInvoice_Service order = new SalesInvoice_Service();
                                                sal.Create(ref order);

                                                // invoice data
                                                order.Sell_to_Customer_No = clientsingle.client.customfields1;

                                                // date YYYY-MM-DD
                                                if (invoicesingle.date.Length >= 10)
                                                {
                                                    string sYYYY = invoicesingle.date.Substring(0, 4);
                                                    string sMM = invoicesingle.date.Substring(5, 2);
                                                    string sDD = invoicesingle.date.Substring(8, 2);
                                                    try
                                                    {
                                                        DateTime dtOrderDate = new DateTime(Convert.ToInt32(sYYYY), Convert.ToInt32(sMM), Convert.ToInt32(sDD));
                                                        order.Posting_Date = dtOrderDate;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                    }
                                                }

                                                sal.Update(ref order);

                                                // prepare space for invocie lines
                                                List<Sales_Invoice_Line> InvoiceLinesList = new List<Sales_Invoice_Line>();
                                                int iInvoiceLinesCount = 0;

                                                // processed invoices count
                                                iInvoiceNumber++;

                                                foreach (Item invoicelinesingle in invoicesingle.items.item)
                                                {

                                                    Sales_Invoice_Line invoiceLine = new Sales_Invoice_Line();

                                                    // item
                                                    invoiceLine.Type = NAVOrdersWS.Type.Item;

                                                    // type
                                                    invoiceLine.No = "1300";

                                                    // quantity and price
                                                    try
                                                    {
                                                        invoiceLine.Quantity = 1;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                    }
                                                    try
                                                    {
                                                        invoiceLine.Unit_Price = Convert.ToDecimal(invoicelinesingle.amount);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                    }
                                                    // no vat values
                                                    invoiceLine.Total_Amount_Incl_VATSpecified = false;
                                                    invoiceLine.Total_Amount_Excl_VATSpecified = false;
                                                    invoiceLine.Total_VAT_AmountSpecified = false;

                                                    // date & description
                                                    string sOrderLineDate = invoicesingle.date;
                                                    string sLineDescription = "(" + sOrderLineDate + ") " + invoicelinesingle.description;

                                                    if (sLineDescription.Length <= 50)
                                                    {
                                                        Sales_Invoice_Line extraLine = new Sales_Invoice_Line();

                                                        extraLine.Type = NAVOrdersWS.Type.Item;
                                                        extraLine.No = "";

                                                        // quantity and price
                                                        extraLine.Quantity = 0;
                                                        extraLine.Unit_Price = 0;

                                                        extraLine.Total_Amount_Incl_VATSpecified = false;
                                                        extraLine.Total_Amount_Excl_VATSpecified = false;
                                                        extraLine.Total_VAT_AmountSpecified = false;

                                                        // extra line
                                                        extraLine.Description = invoicelinesingle.type;
                                                        if (extraLine.Description.Length > 50)
                                                        {
                                                            extraLine.Description = extraLine.Description.Substring(0, 50);
                                                        }

                                                        // add extra line
                                                        InvoiceLinesList.Add(extraLine);

                                                        // count added lines
                                                        iInvoiceLinesCount++;

                                                        // description
                                                        invoiceLine.Description = sLineDescription;

                                                        // add invoice line
                                                        InvoiceLinesList.Add(invoiceLine);

                                                        // count added lines
                                                        iInvoiceLinesCount++;
                                                    }
                                                    else
                                                    {
                                                        // remove multiple spaces & odd empty chars
                                                        RegexOptions options = RegexOptions.None;
                                                        Regex regex = new Regex(@"[ ]{2,}", options);
                                                        sLineDescription = regex.Replace(sLineDescription, @" ");
                                                        sLineDescription = Regex.Replace(sLineDescription, @"\p{Z}", " ");

                                                        // create as many new lines as needed to fit comment length
                                                        int partLength = 50;

                                                        string sLineDescriptionFriendlyChars = sLineDescription.Replace(" ", "≡");
                                                        string[] sLineDescriptionWords = sLineDescriptionFriendlyChars.Split('≡');
                                                        var parts = new Dictionary<int, string>();
                                                        string part = string.Empty;
                                                        int partCounter = 0;
                                                        foreach (var sLineDescriptionWord in sLineDescriptionWords)
                                                        {
                                                            if (part.Length + sLineDescriptionWord.Length < partLength)
                                                            {
                                                                part += string.IsNullOrEmpty(part) ? sLineDescriptionWord : " " + sLineDescriptionWord;
                                                            }
                                                            else
                                                            {
                                                                parts.Add(partCounter, part);
                                                                part = sLineDescriptionWord;
                                                                partCounter++;
                                                            }
                                                        }
                                                        parts.Add(partCounter, part);

                                                        int iPartsCount = 0;
                                                        foreach (var item in parts)
                                                        {
                                                            if (iPartsCount == 0)
                                                            {
                                                                Sales_Invoice_Line extraLine = new Sales_Invoice_Line();

                                                                extraLine.Type = NAVOrdersWS.Type.Item;
                                                                extraLine.No = "";

                                                                // quantity and price
                                                                extraLine.Quantity = 0;
                                                                extraLine.Unit_Price = 0;

                                                                extraLine.Total_Amount_Incl_VATSpecified = false;
                                                                extraLine.Total_Amount_Excl_VATSpecified = false;
                                                                extraLine.Total_VAT_AmountSpecified = false;

                                                                // extra line
                                                                extraLine.Description = invoicelinesingle.type;
                                                                if (extraLine.Description.Length > 50)
                                                                {
                                                                    extraLine.Description = extraLine.Description.Substring(0, 50);
                                                                }

                                                                // add extra line
                                                                InvoiceLinesList.Add(extraLine);

                                                                // count added lines
                                                                iInvoiceLinesCount++;

                                                                // include first 50 chars in the current line
                                                                invoiceLine.Description = item.Value;

                                                                // add invoice line
                                                                InvoiceLinesList.Add(invoiceLine);

                                                                // count added lines
                                                                iInvoiceLinesCount++;
                                                            }
                                                            else
                                                            {
                                                                Sales_Invoice_Line extraLine = new Sales_Invoice_Line();

                                                                extraLine.Type = NAVOrdersWS.Type.Item;
                                                                extraLine.No = "";

                                                                // quantity and price
                                                                extraLine.Quantity = 0;
                                                                extraLine.Unit_Price = 0;

                                                                extraLine.Total_Amount_Incl_VATSpecified = false;
                                                                extraLine.Total_Amount_Excl_VATSpecified = false;
                                                                extraLine.Total_VAT_AmountSpecified = false;

                                                                // extra line
                                                                extraLine.Description = item.Value;

                                                                // add extra line
                                                                InvoiceLinesList.Add(extraLine);

                                                                // count added lines
                                                                iInvoiceLinesCount++;
                                                            }
                                                            iPartsCount++;
                                                        }
                                                    }
                                                }

                                                //if there was lines update them into order
                                                if (iInvoiceLinesCount > 0)
                                                {
                                                    order.SalesLines = new Sales_Invoice_Line[iInvoiceLinesCount];
                                                    for (int i = 0; i < iInvoiceLinesCount; i++)
                                                    {
                                                        order.SalesLines[i] = new Sales_Invoice_Line();
                                                    }
                                                    sal.Update(ref order);

                                                    int iCount = 0;
                                                    foreach (Sales_Invoice_Line sil in InvoiceLinesList)
                                                    {
                                                        order.SalesLines[iCount].Type = sil.Type;
                                                        order.SalesLines[iCount].No = sil.No;
                                                        order.SalesLines[iCount].Quantity = sil.Quantity;
                                                        order.SalesLines[iCount].Unit_Price = sil.Unit_Price;
                                                        order.SalesLines[iCount].Total_Amount_Incl_VATSpecified = sil.Total_Amount_Incl_VATSpecified;
                                                        order.SalesLines[iCount].Total_Amount_Excl_VATSpecified = sil.Total_Amount_Excl_VATSpecified;
                                                        order.SalesLines[iCount].Total_VAT_AmountSpecified = sil.Total_VAT_AmountSpecified;
                                                        order.SalesLines[iCount].Description = sil.Description;
                                                        iCount++;
                                                    }
                                                    sal.Update(ref order);

                                                    iInvoiceAllLinesCount += iInvoiceLinesCount;

                                                    string sCurrentDateTime = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                                                    sCurrentDateTime += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                                                    sCurrentDateTime += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                                                    sCurrentDateTime += DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":";
                                                    sCurrentDateTime += DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":";
                                                    sCurrentDateTime += DateTime.Now.Second.ToString().PadLeft(2, '0');

                                                    string sSql = "INSERT INTO [RPNAVConnect].[dbo].[Log] ([refid] ,[result] ,[source] ,[datestamp] ,[description]) ";
                                                    sSql += "VALUES ('" + invoicesingle.invoiceid.Replace("'", "\"") + "', 'Pushed', 'WHMCS', '" + sCurrentDateTime + "', '" + iInvoiceLinesCount.ToString() + " invoice lines')";
                                                    string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                                                    if (sDBResult != "DBOK")
                                                    {
                                                        sResultMessage += sDBResult + "<br />";
                                                    }
                                                }
                                                else
                                                {
                                                    string sCurrentDateTime = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                                                    sCurrentDateTime += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                                                    sCurrentDateTime += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                                                    sCurrentDateTime += DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":";
                                                    sCurrentDateTime += DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":";
                                                    sCurrentDateTime += DateTime.Now.Second.ToString().PadLeft(2, '0');

                                                    string sSql = "INSERT INTO [RPNAVConnect].[dbo].[Log] ([refid] ,[result] ,[source] ,[datestamp] ,[description]) ";
                                                    sSql += "VALUES ('" + invoicesingle.invoiceid.Replace("'", "\"") + "', 'Pushed', 'WHMCS', '" + sCurrentDateTime + "', 'No invoice lines')";
                                                    string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                                                    if (sDBResult != "DBOK")
                                                    {
                                                        sResultMessage += sDBResult + "<br />";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        sResultMessage = ex.ToString();
                    }

                    dbConn.Close();
                }
            }

            // show message
            PushingDataL.Text = "Total " + iInvoiceNumber.ToString() + " fakturarer herunder " + iInvoiceAllLinesCount.ToString() + " linjer er skubbet til Dynamics NAV.</br ><br />";
            if (sResultMessage != "")
            {
                PushingDataL.Text += "Additional message:<br />";
                PushingDataL.Text += sResultMessage + "<br />";
            }
            if (sProblematicCustomers != "")
            {
                PushingDataL.Text += "Kunder som ikkke har et korrekt CVR nummer registreret i WHMCS<br />";
                PushingDataL.Text += sProblematicCustomers + "<br />";
            }
            if (sMissedCustomers != "")
            {
                PushingDataL.Text += "Kunder som blev oprettet i Dynamics NAV:<br />";
                PushingDataL.Text += sMissedCustomers;
            }
            PushingDataL.Text += "<p style='height:15px;'>&nbsp;</p>";

            PPSep1.Visible = false;

            PushDataToNavB.Visible = false;

            // scroll down
            ClientScript.RegisterStartupScript(GetType(), "ScrollScript", "window.onload = function() {document.getElementById('lastscriptdiv').scrollIntoView(true);}", true);
        }

        private string InsertUpdateDatabase(string SQL, System.Data.OleDb.OleDbConnection dbConn)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Get Connection string
            string sResult = "DBOK";

            try
            {
                // Database Object instancing here
                OleDbCommand OleCommand;
                OleCommand = new OleDbCommand(SQL, dbConn);
                OleCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = ex.ToString();
                return sResult;
            }

            return sResult;
        }

        private bool IsOrderAlreadyProcessed(string sInvoiceTimeLogId, System.Data.OleDb.OleDbConnection dbConn)
        {
            bool bResult = false;

            string strSqlQuery = "SELECT l.[id] FROM [RPNAVConnect].[dbo].[Log]as l WHERE l.[refid] = " + sInvoiceTimeLogId;
            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSqlQuery, dbConn);
            oleReader = cmd.ExecuteReader();
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0)) bResult = true;
            }
            oleReader.Close();

            return bResult;
        }

        private bool DoesCustomerExists(CustomerInfo2_Service service, List<CustomerInfo2_Filter> filter)
        {
            bool bResult = false;

            try
            {
                // Run the actual search.
                CustomerInfo2[] customers = service.ReadMultiple(filter.ToArray(), null, 100);
                foreach (CustomerInfo2 customer in customers)
                {
                    bResult = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                bResult = false;
            }

            return bResult;
        }

    }
}