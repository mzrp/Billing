using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using System.Threading;
using System.Globalization;
using System.Reflection;

/*
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Billing;
using Microsoft.Azure.Management.Billing.Models;
*/

using Microsoft.Store.PartnerCenter;
using Microsoft.Store.PartnerCenter.Models;
using Microsoft.Store.PartnerCenter.Models.Query;
using Microsoft.Store.PartnerCenter.Models.Invoices;
using Microsoft.Store.PartnerCenter.Extensions;

using System.Xml;
using System.Text;

using RPNAVConnect.NAVCustomersWS;
using RPNAVConnect.NAVOrdersWS;
using System.Net;

using System.Text.RegularExpressions;

namespace RPNAVConnect
{
    public class CustMarkup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Markup { get; set; }
    }

    public partial class AzureBillingDF : System.Web.UI.Page
    {
        private void CreateXml(string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlTextWriter xtw;
            xtw = new XmlTextWriter(filepath, Encoding.UTF8);
            xtw.WriteStartDocument();
            xtw.WriteStartElement("CustomerMarkups");
            xtw.WriteEndElement();
            xtw.Close();
        }

        private void WriteXml(string sId, string sName, string sMarkup, string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xd = new XmlDocument();
            FileStream lfile = new FileStream(filepath, FileMode.Open);
            xd.Load(lfile);
            XmlElement cl = xd.CreateElement("Customer");
            cl.SetAttribute("Id", sId);
            XmlElement na = xd.CreateElement("Name");
            XmlText natext = xd.CreateTextNode(sName);
            XmlElement na2 = xd.CreateElement("Markup");
            XmlText natext2 = xd.CreateTextNode(sMarkup);
            na.AppendChild(natext);
            na2.AppendChild(natext2);
            cl.AppendChild(na);
            cl.AppendChild(na2);
            xd.DocumentElement.AppendChild(cl);
            lfile.Close();
            xd.Save(filepath);
        }

        private bool CheckXml(string sId, string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);
            bool bResult = false;
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                if ((cl.GetAttribute("Id")) == sId)
                {
                    bResult = true;
                    break;
                }
            }
            rfile.Close();
            return bResult;
        }

        private string ReadXml(string sId, string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);
            string sResult = "n/a";
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Name")[i];
                XmlElement mp2 = (XmlElement)xdoc.GetElementsByTagName("Markup")[i];
                if ((cl.GetAttribute("Id")) == sId)
                {
                    sResult = mp.InnerText + ";" + mp2.InnerText;
                    break;
                }
            }
            rfile.Close();
            return sResult;
        }

        private bool UpdateXml(string sId, string sValue, string sMarkupFile)
        {
            bool bResult = false;
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream up = new FileStream(filepath, FileMode.Open);
            xdoc.Load(up);
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cu = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Markup")[i];
                if (cu.GetAttribute("Id") == sId)
                {
                    cu.SetAttribute("Markup", sValue);
                    mp.InnerText = sValue;
                    bResult = true;
                    break;
                }
            }
            up.Close();
            xdoc.Save(filepath);
            return bResult;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (Page.IsPostBack == false)
            {
                HandleCustomersData();
            }

            if (Page.IsPostBack == true)
            {
                System.Collections.Specialized.NameValueCollection FormPageVars;
                FormPageVars = Request.Form;

                var eventTarget = Request.Form["__EVENTTARGET"].ToString();

                // Check if some button is pressed
                if (eventTarget != null)
                {
                    if (eventTarget != "")
                    {
                        // UpdateBtn Button is pressed
                        if (eventTarget.IndexOf("butPushCustomer_") == 0)
                        {
                            string sCustVatNo = eventTarget.Substring(eventTarget.IndexOf("butPushCustomer_") + 16);
                            PushSingleCustomer(sCustVatNo);
                        }
                    }
                }
            }
        }

        private IAggregatePartner appPartnerOperations = null;
        private Task progressBackgroundTask;
        private CancellationTokenSource progressCancellationTokenSource = new CancellationTokenSource();
        private readonly int invoicePageSize = 100;
        private readonly int customerPageSize = 100;

        public static void WriteColored(string message, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            Console.Write(message + (newLine ? "\n" : string.Empty));
            Console.ResetColor();
        }

        public void StartProgress(string message)
        {
            if (progressBackgroundTask == null || progressBackgroundTask.Status != TaskStatus.Running)
            {
                progressBackgroundTask = new Task(() =>
                {
                    int dotCounter = 0;

                    while (!progressCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        for (dotCounter = 0; dotCounter < 5; dotCounter++)
                        {
                            Thread.Sleep(200);

                            if (progressCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                return;
                            }
                        }
                    }
                });

                progressBackgroundTask.Start();
            }
        }

        public void StopProgress()
        {
            if (progressBackgroundTask != null && progressBackgroundTask.Status == TaskStatus.Running)
            {
                progressCancellationTokenSource.Cancel();
                progressBackgroundTask.Wait();
                progressBackgroundTask.Dispose();
                progressBackgroundTask = null;

                progressCancellationTokenSource.Dispose();
                progressCancellationTokenSource = new CancellationTokenSource();
            }
        }

        public IAggregatePartner AppPartnerOperations
        {
            get
            {
                if (appPartnerOperations == null)
                {
                    StartProgress("Authenticating application");

                    IPartnerCredentials appCredentials = PartnerCredentials.Instance.GenerateByApplicationCredentials(
                        "1f7fcf68-2b68-49d7-97cc-5b915d26fb33",
                        "j5mr]2GpPhJo8_fiFhEe7qbVvO-h[qDr",
                        "GoWingu.onmicrosoft.com",
                        "https://login.windows.net",
                        "https://graph.windows.net");

                    StopProgress();
                    appPartnerOperations = PartnerService.Instance.CreatePartnerOperations(appCredentials);
                }

                return appPartnerOperations;
            }
        }

        public async void PushSingleCustomer(string sCustomerVAT)
        {
            if (rbtnSeats.Checked == true)
            {
                await GetInvoiceData("Seats", "Navision", sCustomerVAT);
            }

            if (rtbnUsage.Checked == true)
            {
                await GetInvoiceData("Usage", "Navision", sCustomerVAT);
            }
        }
        
        public async void HandleCustomersData()
        {
            // add new customer to xml
            await GetCustomers();

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            List<CustMarkup> cls = new List<CustMarkup>();

            string sMarkupFile = "MARKUPSeats.xml";
            if (rbtnSeats.Checked == true)
            {
                sMarkupFile = "MARKUPSeats.xml";
                MarkupType.Text = "SEATS Type: MARKUP";
            }
            if (rtbnUsage.Checked == true)
            {
                MarkupType.Text = "USAGE Type: MARKUP";
                sMarkupFile = "MARKUPUsage.xml";
            }            

            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;

            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);
            
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Name")[i];
                XmlElement mp2 = (XmlElement)xdoc.GetElementsByTagName("Markup")[i];

                CustMarkup cm = new CustMarkup();
                cm.Id = cl.GetAttribute("Id");
                cm.Name = mp.InnerText;
                cm.Markup = mp2.InnerText;

                cls.Add(cm);
            }
            rfile.Close();

            CustomersMarkup.DataSource = cls;
            CustomersMarkup.DataBind();
        }

        public async Task GetCustomers()
        {
            var partnerOperations = AppPartnerOperations;

            StartProgress("Querying customers");

            var customersPage = (customerPageSize <= 0) ? partnerOperations.Customers.Get() : partnerOperations.Customers.Query(QueryFactory.Instance.BuildIndexedQuery(customerPageSize));
            StopProgress();

            string sMarkupFile = "MARKUPSeats.xml";
            if (rbtnSeats.Checked == true)
            {
                //ClientScript.RegisterStartupScript(GetType(), "thMRName", "window.onload = function() { document.getElementById('thMRName').innerHTML = 'Markup %'; }", true);
                sMarkupFile = "MARKUPSeats.xml";
                MarkupType.Text = "SEATS Type: MARKUP";
            }
            if (rtbnUsage.Checked == true)
            {
                //ClientScript.RegisterStartupScript(GetType(), "thMRName", "window.onload = function() { document.getElementById('thMRName').innerHTML = 'Markup %'; }", true); 
                MarkupType.Text = "USAGE Type: MARKUP";
                sMarkupFile = "MARKUPUsage.xml";
            }
            
            foreach (var customer in customersPage.Items)
            {
                string sCustomerId = customer.Id;
                string sCustomerName = customer.CompanyProfile.CompanyName;

                if ((sCustomerId != "") && (sCustomerName != ""))
                {
                    if (CheckXml(sCustomerId, sMarkupFile) == false)
                    {
                        WriteXml(sCustomerId, sCustomerName, "25.0", sMarkupFile);
                    }
                }
            }
        }

        /*
        protected void SaveCustomerMarkupsB_Click(object sender, EventArgs e)
        {
            if (Page.IsPostBack == true)
            {
                for (int i = 0; i < CustomersMarkup.Items.Count; i++)
                {
                    ListViewDataItem lvdi = CustomersMarkup.Items[i];

                    foreach (var lvdicontrol in lvdi.Controls)
                    {
                        if (lvdicontrol is System.Web.UI.WebControls.TextBox)
                        {
                            TextBox tb = (TextBox)lvdicontrol;
                            AttributeCollection ac = tb.Attributes;
                            string sCustId = ac["CustId"];
                            string sCustName = ac["CustName"];
                            string sCustMarkup = tb.Text;

                            CustMarkup cm = (CustMarkup)lvdi.DataItem;

                        }
                    }
                }
            }
        }
        */

        public async Task GetInvoiceData(string sRPBillingType, string sAction, string sCustomerVAT)
        {

            string sRPInvoiceType = "Recurring";
            if (sRPBillingType == "Seats") sRPInvoiceType = "Recurring";
            if (sRPBillingType == "Usage") sRPInvoiceType = "OneTime";

            var partnerOperations = AppPartnerOperations;

            StartProgress("Querying invoices");
            var invoicesPage = (invoicePageSize <= 0) ? partnerOperations.Invoices.Get() : partnerOperations.Invoices.Query(QueryFactory.Instance.BuildIndexedQuery(invoicePageSize));
            StopProgress();

            // license-based-pricelist
            /*
            string sFilePath = Server.MapPath("~/License-based pricing.csv");
            List<string> sLicenseBasedPricelist = new List<string>();
            // OfferName, OfferId, ListPrice, ERPPrice
            using (var reader = new StreamReader(sFilePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    sLicenseBasedPricelist.Add(values[3] + "," + values[4] + "," + values[9] + "," + values[10]);
                }
            }
            */

            /*
            // usage-based-pricelist
            StartProgress("Querying ...");
            var azureRateCard = partnerOperations.RateCards.Azure.Get("DKK", "DK");
            StopProgress();

            foreach (var offer in azureRateCard.Items)
            {
                if (offer.Product.Id == "195416c1-3447-423a-b37b-ee59a99a19c4")
                {
                    string sPrice = offer.Product.Name;
                }
            }
            */

            DateTime dtCurrent = DateTime.Now;

            DateTime dtStartDate = new DateTime(dtCurrent.Year, dtCurrent.Month, 1);
            DateTime dtEndDate = dtStartDate.AddMonths(1).AddDays(-1);

            int iYear = -1;
            int iMonth = -1;
            if (YearTB.Text != "")
            {
                try
                {
                    int iYearChk = Convert.ToInt32(YearTB.Text);
                    if (iYearChk > 2000)
                    {
                        if (MonthTB.Text != "")
                        {
                            int iMonthChk = Convert.ToInt32(MonthTB.Text);
                            if ((iMonthChk >= 1) && (iMonthChk <= 12))
                            {
                                iMonth = iMonthChk;
                                iYear = iYearChk;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    iYear = -1;
                    iMonth = -1;
                }
            }

            string sMonthName = "";
            if (iMonth == 1) sMonthName = "January";
            if (iMonth == 2) sMonthName = "February";
            if (iMonth == 3) sMonthName = "March";
            if (iMonth == 4) sMonthName = "April";
            if (iMonth == 5) sMonthName = "May";
            if (iMonth == 6) sMonthName = "June";
            if (iMonth == 7) sMonthName = "July";
            if (iMonth == 8) sMonthName = "August";
            if (iMonth == 9) sMonthName = "September";
            if (iMonth == 10) sMonthName = "October";
            if (iMonth == 11) sMonthName = "November";
            if (iMonth == 12) sMonthName = "December";

            if ((iMonth != -1) && (iYear != -1))
            {
                dtStartDate = new DateTime(iYear, iMonth, 1);
                dtEndDate = dtStartDate.AddMonths(1).AddDays(-1);
            }

            AzureBillingDataL.Text = "<br />";

            foreach (var invoice in invoicesPage.Items)
            {
                if (invoice.InvoiceType == sRPInvoiceType)
                {
                    if ((dtStartDate >= invoice.BillingPeriodStartDate) && (dtEndDate <= invoice.BillingPeriodEndDate))
                    {
                        decimal dTotalCharges = invoice.TotalCharges;
                        string sInvoiceId = invoice.Id;

                        AzureBillingDataL.Text += "<font size='3'>Invoice found for " + dtStartDate.Month.ToString().PadLeft(2, '0') + "/" + dtStartDate.Year.ToString().PadLeft(4, '0') + ": <b>" + sInvoiceId + "</b></font>";
                        AzureBillingDataL.Text += "<br />";
                        AzureBillingDataL.Text += "Total charges: <b>" + dTotalCharges.ToString("N") + "</b>";

                        // Retrieving invoice line items
                        if (invoice.InvoiceDetails != null)
                        {
                            int iInvoiceDetailCount = 0;
                            foreach (var invoiceDetail in invoice.InvoiceDetails)
                            {
                                if (invoiceDetail.InvoiceLineItemType == Microsoft.Store.PartnerCenter.Models.Invoices.InvoiceLineItemType.BillingLineItems)
                                {
                                    //BillingProvider bp = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.Office;
                                    bool bBP = false;
                                    if (sRPBillingType == "Seats")
                                    {
                                        BillingProvider bp1 = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.Office;
                                        BillingProvider bp2 = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.OneTime;
                                        BillingProvider bp3 = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.Azure;
                                        BillingProvider bp4 = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.Marketplace;
                                        BillingProvider bp5 = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.All;
                                        BillingProvider bp6 = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.None;
                                        bBP = (invoiceDetail.BillingProvider == bp1) || (invoiceDetail.BillingProvider == bp2) || (invoiceDetail.BillingProvider == bp3) || (invoiceDetail.BillingProvider == bp4) || (invoiceDetail.BillingProvider == bp5) || (invoiceDetail.BillingProvider == bp6);
                                    }
                                    if (sRPBillingType == "Usage")
                                    {
                                        BillingProvider bp = Microsoft.Store.PartnerCenter.Models.Invoices.BillingProvider.OneTime;
                                        bBP = invoiceDetail.BillingProvider == bp;
                                    }

                                    if (bBP)
                                    {
                                        var invoiceOperations = partnerOperations.Invoices.ById(sInvoiceId);
                                        var seekBasedResourceCollection = invoiceOperations.By(invoiceDetail.BillingProvider.ToString(), invoiceDetail.InvoiceLineItemType.ToString(), invoice.CurrencyCode, "current", null).Get();

                                        AzureBillingDataL.Text += "<hr />";

                                        if (seekBasedResourceCollection.Items.Count<InvoiceLineItem>() > 0)
                                        {
                                            iInvoiceDetailCount++;

                                            // action for navision
                                            string sNAVLogin = "rpnavapi";
                                            string sNAVPassword = "Telefon1";

                                            // get access to NAVDebtor
                                            CustomerInfo2_Service nav = new CustomerInfo2_Service();
                                            nav.UseDefaultCredentials = true;
                                            nav.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword);

                                            // get access to NAVSalgsordre
                                            SalesInvoice_Service_Service sal = new SalesInvoice_Service_Service();
                                            sal.UseDefaultCredentials = true;
                                            sal.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword);

                                            string sAllInvoiceCustomers = "";

                                            // get all customers first
                                            foreach (var ilItem in seekBasedResourceCollection.Items)
                                            {
                                                if ((ilItem is LicenseBasedLineItem) || (ilItem is OneTimeInvoiceLineItem) || (ilItem is UsageBasedLineItem))
                                                {
                                                    System.Type t = ilItem.GetType();
                                                    PropertyInfo[] properties = t.GetProperties();

                                                    string sCustomerName = "";
                                                    string sCustomerId = "";

                                                    foreach (PropertyInfo property in properties)
                                                    {
                                                        string sValue = "";
                                                        try
                                                        {
                                                            sValue = property.GetValue(ilItem, null).ToString();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            ex.ToString();
                                                            sValue = "";
                                                        }

                                                        if (property.Name == "CustomerName") sCustomerName = sValue;

                                                        if (ilItem is UsageBasedLineItem)
                                                        {
                                                            if (property.Name == "CustomerCompanyName") sCustomerName = sValue;
                                                        }

                                                        if (property.Name == "CustomerId") sCustomerId = sValue;
                                                    }

                                                    if ((sCustomerId != "") && (sCustomerName != "")) 
                                                    {
                                                        // searh filter for the customer
                                                        List<CustomerInfo2_Filter> filterArray = new List<CustomerInfo2_Filter>();
                                                        CustomerInfo2_Filter nameFilter = new CustomerInfo2_Filter();
                                                        nameFilter.Field = CustomerInfo2_Fields.Account_Code;

                                                        string sAccountCode = sCustomerId;
                                                        if (sCustomerId.Length > 30)
                                                        {
                                                            sAccountCode = sCustomerId.Substring(0, 30);
                                                        }

                                                        nameFilter.Criteria = sAccountCode;
                                                        filterArray.Add(nameFilter);

                                                        string sCustomerVATNo = "n/a";
                                                        if (sAllInvoiceCustomers.IndexOf(sCustomerName + "ђ") == -1)
                                                        {
                                                            sCustomerVATNo = DoesCustomerExists(nav, filterArray);
                                                            sAllInvoiceCustomers += sCustomerName + "ђ" + sCustomerVATNo + "ш";
                                                        }
                                                    }
                                                }
                                            }

                                            string[] sAllInvoiceCustomersArrayFirst = sAllInvoiceCustomers.Split('ш');
                                            string sAllInvoiceCustomersSorted = "";

                                            // empty VATs first
                                            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArrayFirst)
                                            {
                                                if (sInvoiceCustomer != "")
                                                {
                                                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                                                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[1];

                                                    if (sCustVatNo == "n/a") sAllInvoiceCustomersSorted += sCust + "ђ" + sCustVatNo + "ш";
                                                }
                                            }
                                            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArrayFirst)
                                            {
                                                if (sInvoiceCustomer != "")
                                                {
                                                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                                                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[1];

                                                    if (sCustVatNo != "n/a") sAllInvoiceCustomersSorted += sCust + "ђ" + sCustVatNo + "ш";
                                                }
                                            }

                                            if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                            {
                                                AzureBillingDataL.Text += "<font size='3' color='blue'><b>BillingProvider - OFFICE</b></font>";
                                            }
                                            if (invoiceDetail.BillingProvider == BillingProvider.Azure)
                                            {
                                                AzureBillingDataL.Text += "<font size='3' color='blue'><b>BillingProvider - AZURE</b></font>";
                                            }
                                            if (invoiceDetail.BillingProvider == BillingProvider.OneTime)
                                            {
                                                AzureBillingDataL.Text += "<font size='3' color='blue'><b>BillingProvider - AZURE OneTime</b></font>";
                                            }

                                            AzureBillingDataL.Text += "<br />";
                                            AzureBillingDataL.Text += "Invoice Line Items: <b>" + seekBasedResourceCollection.Items.Count<InvoiceLineItem>() + "</b>";
                                            AzureBillingDataL.Text += "<hr /><br />";

                                            string[] sAllInvoiceCustomersArray = sAllInvoiceCustomersSorted.Split('ш');

                                            AzureBillingDataL.Text += "<table class='table table-bordered table-striped' style='width: 900px; '>";
                                            AzureBillingDataL.Text += "<tr class='bg-danger text-white'>";
                                            AzureBillingDataL.Text += "<th><b>Customer</b></th>";
                                            AzureBillingDataL.Text += "<th><b>VAT</b></th>";
                                            if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                            {
                                                /*
                                                AzureBillingDataL.Text += "<th><b>Tot. MS list price</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Tot. ERP Price</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Rebate %</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Tot. Customer price</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Tot. Db (Mark Up)</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Markup %</b></th>";
                                                */

                                                AzureBillingDataL.Text += "<th><b>Tot. MS price</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Markup %</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Tot. Customer price</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Tot. Db (Mark Up)</b></th>";
                                            }
                                            else
                                            {
                                                AzureBillingDataL.Text += "<th><b>Tot. MS price</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Markup %</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Tot. Customer price</b></th>";
                                                AzureBillingDataL.Text += "<th><b>Tot. Db (Mark Up)</b></th>";
                                            }
                                            AzureBillingDataL.Text += "<th>&nbsp;</th>";
                                            AzureBillingDataL.Text += "</tr>";
                                            AzureBillingDataL.Text += "<tbody>";
                                            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArray)
                                            {
                                                if (sInvoiceCustomer != "")
                                                {
                                                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                                                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[1];

                                                    string sWarning1 = "";
                                                    string sWarning2 = "";
                                                    if (sCustVatNo == "n/a")
                                                    {
                                                        sWarning1 = "<font color='red'>";
                                                        sWarning2 = "</font>";
                                                    }

                                                    AzureBillingDataL.Text += "<tr>";
                                                    AzureBillingDataL.Text += "<td>" + sWarning1 + sCust + sWarning2 + "</td>";
                                                    AzureBillingDataL.Text += "<td>" + sWarning1 + sCustVatNo + sWarning2 + "</td>";

                                                    if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                                    {
                                                        /*
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TL" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TE" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#R" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TC" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TM" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#M" + sCust + "#" + sWarning2 + "</td>";
                                                        */

                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TM" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#M" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TC" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TD" + sCust + "#" + sWarning2 + "</td>";
                                                    }
                                                    else
                                                    {
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TM" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#M" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TC" + sCust + "#" + sWarning2 + "</td>";
                                                        AzureBillingDataL.Text += "<td>" + sWarning1 + "#TD" + sCust + "#" + sWarning2 + "</td>";
                                                    }
                                                    
                                                    if (sCustVatNo == "n/a")
                                                    {
                                                        AzureBillingDataL.Text += "<td></td>";
                                                    }
                                                    else
                                                    {
                                                        string sButtonId = sCustVatNo;
                                                        AzureBillingDataL.Text += "<td><input id=\"butPushCustomer_" + sCustVatNo + "\" type=\"button\" name=\"butPushCustomer_" + sCustVatNo + "\" value=\"Push to NAV\" onclick=\"__doPostBack('butPushCustomer_" + sCustVatNo + "','')\" /></td>";
                                                    }
                                                    AzureBillingDataL.Text += "</tr>";
                                                }
                                            }
                                            AzureBillingDataL.Text += "<tr class='bg-danger text-white'>";
                                            AzureBillingDataL.Text += "<td></td>";
                                            AzureBillingDataL.Text += "<td align='right'>Sum:</td>";
                                            if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                            {
                                                /*
                                                AzureBillingDataL.Text += "<td><b>#SUMTL#</b></td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMTE#</b></td>";
                                                AzureBillingDataL.Text += "<td></td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMTC#</b></td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMTM#</b></td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMM#</b></td>";
                                                */

                                                AzureBillingDataL.Text += "<td><b>#SUMTM#</b></td>";
                                                AzureBillingDataL.Text += "<td align='right'>Sum:</td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMTC#</b></td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMTD#</b></td>";
                                            }
                                            else
                                            {
                                                AzureBillingDataL.Text += "<td><b>#SUMTM#</b></td>";
                                                AzureBillingDataL.Text += "<td align='right'>Sum:</td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMTC#</b></td>";
                                                AzureBillingDataL.Text += "<td><b>#SUMTD#</b></td>";
                                            }
                                            AzureBillingDataL.Text += "</tbody>";
                                            AzureBillingDataL.Text += "</table>";
                                            AzureBillingDataL.Text += "<br />";

                                            AzureBillingDataL.Text += "<font size='3'><b><a href = 'javascript:toogleINVDETAILS(" + iInvoiceDetailCount.ToString() + ");' id = 'tmidlink'>Show Invoice Details</a></b></font><br /><br />";
                                            AzureBillingDataL.Text += "<div id='INVDETAILS_" + iInvoiceDetailCount.ToString() + "' style='display: none;'>";

                                            decimal dTCustMSListAmount = 0;
                                            decimal dTCustERPAmount = 0;
                                            decimal dTCustRPTotalAmount = 0;
                                            decimal dTCustRPTotalDBMArkupAmount = 0;
                                            decimal dTCustRPMarkup = 0;
                                            int iCustomerCount = 0;

                                            decimal dTCustMSUTotalAmount = 0;
                                            decimal dTCustRPUMarkup = 0;
                                            decimal dTCustRPUTotalAmount = 0;
                                            decimal dTCustRPUDiffAmount = 0;

                                            bool bFistCustomer = true;
                                            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArray)
                                            {
                                                if (sInvoiceCustomer != "")
                                                {
                                                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                                                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[1];

                                                    if ((sCustVatNo != "n/a") || (sAction == "Data"))
                                                    {
                                                        // create order first
                                                        SalesInvoice_Service order = new SalesInvoice_Service();

                                                        List<Sales_Invoice_Line> InvoiceLinesList = new List<Sales_Invoice_Line>();
                                                        int iInvoiceLinesCount = 0;

                                                        if ((sAction == "Navision") && ((sCustomerVAT == "ALL") || (sCustomerVAT == sCustVatNo)))
                                                        {
                                                            sal.Create(ref order);

                                                            order.Sell_to_Customer_No = sCustVatNo;
                                                            order.Posting_Date = DateTime.Now;
                                                            sal.Update(ref order);
                                                        }

                                                        int iCount = 1;

                                                        decimal dCustMSListAmount = 0;
                                                        decimal dCustERPAmount = 0;
                                                        decimal dCustRPRebate = 0;
                                                        decimal dCustRPTotalAmount = 0;
                                                        decimal dCustRPTotalDBMArkupAmount = 0;
                                                        decimal dCustRPMarkup = 0;

                                                        decimal dCustMSUTotalAmount = 0;
                                                        decimal dCustRPUMarkup = 0;
                                                        decimal dCustRPUTotalAmount = 0;
                                                        decimal dCustRPUDiffAmount = 0;

                                                        foreach (var ilItem in seekBasedResourceCollection.Items)
                                                        {
                                                            if ((ilItem is LicenseBasedLineItem) || (ilItem is OneTimeInvoiceLineItem) || (ilItem is UsageBasedLineItem))
                                                            {
                                                                System.Type t = ilItem.GetType();
                                                                PropertyInfo[] properties = t.GetProperties();

                                                                string sCustomerId = "n/a";
                                                                string sCustomerName = "n/a";
                                                                string sProductNo = "310"; // hardcoded
                                                                string sDescription = "";
                                                                string sQuantity = "n/a";
                                                                string sLineAmount = "n/a";
                                                                string sTotalAmount = "n/a";
                                                                string sUnitPrice = "n/a";
                                                                string sDollarPrice = "n/a";
                                                                string sOfferId = "n/a";
                                                                string sOfferName = "n/a";

                                                                string sLine2 = "";
                                                                string sTransactionDatePeriod = "";
                                                                string sChargeType = "";
                                                                foreach (PropertyInfo property in properties)
                                                                {
                                                                    string sValue = "";
                                                                    try
                                                                    {
                                                                        sValue = property.GetValue(ilItem, null).ToString();
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        ex.ToString();
                                                                        sValue = "";
                                                                    }
                                                                    sLine2 += sValue + ", ";

                                                                    if (property.Name == "CustomerId") sCustomerId = sValue;
                                                                    if (property.Name == "CustomerName") sCustomerName = sValue;
                                                                    if (property.Name == "DurableOfferId") sOfferId = sValue;
                                                                    if (property.Name == "OfferName") sOfferName = sValue;

                                                                    if (ilItem is UsageBasedLineItem)
                                                                    {
                                                                        if (property.Name == "CustomerCompanyName") sCustomerName = sValue; // seats/oldusage
                                                                    }

                                                                    if (property.Name == "ProductName")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - ";
                                                                        }
                                                                    }
                                                                    if (property.Name == "SkuName")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - ";
                                                                        }
                                                                    }

                                                                    if (property.Name == "SubscriptionName")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription = sValue + " - "; // seats
                                                                        }
                                                                    }

                                                                    if (property.Name == "BillingCycleType")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - "; // seats
                                                                        }
                                                                    }

                                                                    if (property.Name == "OfferName")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - "; // seats
                                                                        }
                                                                    }

                                                                    if (property.Name == "ChargeType")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sChargeType = sValue;
                                                                        }
                                                                    }

                                                                    if (property.Name == "ServiceName")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - ";
                                                                        }
                                                                    }

                                                                    if (property.Name == "ServiceType")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - ";
                                                                        }
                                                                    }

                                                                    if (property.Name == "ResourceName")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - ";
                                                                        }
                                                                    }

                                                                    if (property.Name == "Region")
                                                                    {
                                                                        if (sValue != "")
                                                                        {
                                                                            sDescription += sValue + " - ";
                                                                        }
                                                                    }

                                                                    if (property.Name == "ChargeStartDate")
                                                                    {
                                                                        string sDateValue = sValue;
                                                                        if (sValue.IndexOf(" ") != -1)
                                                                        {
                                                                            sDateValue = sValue.Substring(0, sValue.IndexOf(" "));
                                                                        }
                                                                        sTransactionDatePeriod = sDateValue;
                                                                    }

                                                                    if (property.Name == "ChargeEndDate")
                                                                    {
                                                                        string sDateValue = sValue;
                                                                        if (sValue.IndexOf(" ") != -1)
                                                                        {
                                                                            sDateValue = sValue.Substring(0, sValue.IndexOf(" "));
                                                                        }
                                                                        sTransactionDatePeriod += "-" + sDateValue + "ђ";
                                                                        sTransactionDatePeriod += " " + sMonthName;
                                                                    }

                                                                    if (property.Name == "Quantity") sQuantity = sValue;
                                                                    if (property.Name == "Subtotal") sLineAmount = sValue;
                                                                    if (property.Name == "TotalForCustomer") sTotalAmount = sValue;
                                                                    if (property.Name == "UnitPrice") sUnitPrice = sValue;
                                                                    if (property.Name == "PCToBCExchangeRate") sDollarPrice = sValue;

                                                                    if (ilItem is UsageBasedLineItem)
                                                                    {
                                                                        if (property.Name == "IncludedQuantity") sQuantity = sValue;
                                                                        if (property.Name == "ListPrice") sLineAmount = sValue;
                                                                        if (property.Name == "PostTaxTotal") sTotalAmount = sValue;
                                                                        if (property.Name == "ListPrice") sUnitPrice = sValue;
                                                                        if (property.Name == "PCToBCExchangeRate") sDollarPrice = sValue;
                                                                    }

                                                                }
                                                                sDescription += sChargeType + "ђ" + sTransactionDatePeriod;
                                                                if (sLine2 != "")
                                                                {
                                                                    sLine2 = sLine2.Substring(0, sLine2.Length - 2);
                                                                }

                                                                if (sCust == sCustomerName)
                                                                {
                                                                    if (iCount == 1)
                                                                    {
                                                                        string sLine1 = "";
                                                                        foreach (PropertyInfo property in properties)
                                                                        {
                                                                            sLine1 += property.Name + ", ";
                                                                        }
                                                                        if (sLine1 != "")
                                                                        {
                                                                            sLine1 = sLine1.Substring(0, sLine1.Length - 2);
                                                                        }

                                                                        if (bFistCustomer == false) AzureBillingDataL.Text += "<hr />";
                                                                        bFistCustomer = false;

                                                                        AzureBillingDataL.Text += "<font size='4'><b>" + sCustomerName + "</b></font><br />";

                                                                        if (ilItem is LicenseBasedLineItem)
                                                                        {
                                                                            AzureBillingDataL.Text += "Markup: <b>#CUSTMARKUP#%</b>; Number of lines: <b>#LINESNUM#</b>; Total Amount: <b>#TOTALMS#</b>; Reseller Total Amount: <b>#TOTALRP#</b>; Reseller Total Diff.: <b>#TOTALRPDIFF#</b>";
                                                                        }
                                                                        else
                                                                        {
                                                                            AzureBillingDataL.Text += "Markup: <b>#CUSTMARKUP#%</b>; Number of lines: <b>#LINESNUM#</b>; Total Amount: <b>#TOTALMS#</b>; Reseller Total Amount: <b>#TOTALRP#</b>; Reseller Total Diff.: <b>#TOTALRPDIFF#</b>";
                                                                        }
                                                                        AzureBillingDataL.Text += "<br /><br />";

                                                                        AzureBillingDataL.Text += "<b><i>";
                                                                        AzureBillingDataL.Text += sLine1;
                                                                        AzureBillingDataL.Text += "</i></b>";
                                                                        AzureBillingDataL.Text += "<br /><br />";
                                                                    }

                                                                    AzureBillingDataL.Text += iCount.ToString();
                                                                    AzureBillingDataL.Text += "<br />";

                                                                    AzureBillingDataL.Text += sLine2;
                                                                    AzureBillingDataL.Text += "<br /><br />";

                                                                    string sMarkupFile = "MARKUPSeats.xml";
                                                                    if (rbtnSeats.Checked == true)
                                                                    {
                                                                        if (ilItem is LicenseBasedLineItem)
                                                                        {
                                                                            sMarkupFile = "MARKUPSeats.xml";
                                                                            MarkupType.Text = "SEATS Type: MARKUP";
                                                                        }
                                                                        if (ilItem is UsageBasedLineItem)
                                                                        {
                                                                            MarkupType.Text = "SEATS Type: MARKUP";
                                                                            sMarkupFile = "MARKUPUsage.xml";
                                                                        }
                                                                    }
                                                                    if (rtbnUsage.Checked == true)
                                                                    {
                                                                        MarkupType.Text = "USAGE Type: MARKUP";
                                                                        sMarkupFile = "MARKUPUsage.xml";
                                                                    }

                                                                    // RP Billing
                                                                    string sMarkupData = ReadXml(sCustomerId, sMarkupFile);
                                                                    string sMarkup = "n/a";
                                                                    if (sMarkupData != "n/a")
                                                                    {
                                                                        sMarkup = sMarkupData.Split(';')[1];
                                                                    }
                                                                    AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#CUSTMARKUP#", sMarkup);

                                                                    string sOfferNameToDisplay = "n/a";
                                                                    string sOfferIdToDisplay = "n/a";
                                                                    string sListPrice = "n/a";
                                                                    string sERPPrice = "n/a";
                                                                    decimal dListPrice = 0;
                                                                    decimal dERPPrice = 0;
                                                                    /*
                                                                    if (rbtnSeats.Checked == true)
                                                                    {
                                                                        if (ilItem is LicenseBasedLineItem)
                                                                        {
                                                                            var match = sLicenseBasedPricelist.FirstOrDefault(stringToCheck => stringToCheck.Contains(sOfferId));
                                                                            if (match != null)
                                                                            {
                                                                                sOfferNameToDisplay = match.Split(',')[0];
                                                                                sOfferIdToDisplay = match.Split(',')[1];

                                                                                sListPrice = match.Split(',')[2];
                                                                                try
                                                                                {
                                                                                    dListPrice = Convert.ToDecimal(sListPrice);
                                                                                }
                                                                                catch (Exception ex)
                                                                                {
                                                                                    ex.ToString();
                                                                                    dListPrice = 0;
                                                                                }

                                                                                sERPPrice = match.Split(',')[3];
                                                                                try
                                                                                {
                                                                                    dERPPrice = Convert.ToDecimal(sERPPrice);
                                                                                }
                                                                                catch (Exception ex)
                                                                                {
                                                                                    ex.ToString();
                                                                                    dERPPrice = 0;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    */

                                                                    string sRPCP = "";
                                                                    decimal dRPCP = 0;
                                                                    decimal dRPCPDiff = 0;
                                                                    string sRPCPDiff = "";
                                                                    string sRPCPPerUP = "";
                                                                    decimal dRPCPPerUP = 0;
                                                                    decimal dDollar = 1;
                                                                    decimal dUnitPrice = 0;
                                                                    decimal dMarkup = 0;
                                                                    decimal dTotalAmount = 0;

                                                                    try
                                                                    {
                                                                        dUnitPrice = 1;
                                                                        if (sUnitPrice != "n/a") dUnitPrice = Convert.ToDecimal(sUnitPrice);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        dUnitPrice = 1;
                                                                        ex.ToString();
                                                                    }

                                                                    try
                                                                    {
                                                                        dDollar = 1;
                                                                        if (sDollarPrice != "n/a") dDollar = Convert.ToDecimal(sDollarPrice);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        dDollar = 1;
                                                                        ex.ToString();
                                                                    }

                                                                    try
                                                                    {
                                                                        dMarkup = 0;
                                                                        if (sMarkup != "n/a") dMarkup = Convert.ToDecimal(sMarkup);
                                                                        dCustRPUMarkup = dMarkup;
                                                                        dCustRPRebate = dMarkup;
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        dMarkup = 0;
                                                                        ex.ToString();
                                                                    }

                                                                    try
                                                                    {
                                                                        dTotalAmount = 0;
                                                                        if (sTotalAmount != "n/a") dTotalAmount = Convert.ToDecimal(sTotalAmount);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        dTotalAmount = 0;
                                                                        ex.ToString();
                                                                    }

                                                                    // calculations
                                                                    try
                                                                    {
                                                                        if (rbtnSeats.Checked == true)
                                                                        {
                                                                            if (ilItem is LicenseBasedLineItem) 
                                                                            {
                                                                                // i.e. 300 * ((100 - 5) / 100)
                                                                                /*
                                                                                dRPCPPerUP = dERPPrice * ((100 - dMarkup) / 100);
                                                                                sRPCPPerUP = dRPCPPerUP.ToString("N");
                                                                                */
                                                                                dRPCP = dTotalAmount * ((100 + dMarkup) / 100);
                                                                                sRPCP = dRPCP.ToString("N");
                                                                            }
                                                                            if (ilItem is UsageBasedLineItem)
                                                                            {
                                                                                // i.e. 250 * ((100 + 25) / 100)
                                                                                dRPCP = dTotalAmount * ((100 + dMarkup) / 100);
                                                                                sRPCP = dRPCP.ToString("N");
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            // i.e. 250 * ((100 + 25) / 100)
                                                                            //dRPCP = dTotalAmount * ((100 + dMarkup) / 100);
                                                                            //sRPCP = dRPCP.ToString("N");

                                                                            // first calculate unit price
                                                                            try
                                                                            {                                                                                
                                                                                dRPCPPerUP = (dUnitPrice * dDollar) * ((100 + dMarkup) / 100);
                                                                                sRPCPPerUP = dRPCPPerUP.ToString("N");
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                ex.ToString();
                                                                                dRPCPPerUP = 0;
                                                                                sRPCPPerUP = "";
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        ex.ToString();
                                                                        sRPCP = "";
                                                                        dRPCP = 0;
                                                                        sRPCPPerUP = "";
                                                                        dRPCPPerUP = 0;
                                                                    }
                                                                        
                                                                    string sQuantityToShow = "";
                                                                    decimal dQuantity = 0;
                                                                    if (rbtnSeats.Checked == true)
                                                                    {
                                                                        if (ilItem is LicenseBasedLineItem)
                                                                        {
                                                                            sQuantityToShow = sQuantity;
                                                                            try
                                                                            {
                                                                                dQuantity = Convert.ToDecimal(sQuantity);
                                                                                sQuantityToShow = dQuantity.ToString("N");
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                ex.ToString();
                                                                                sQuantityToShow = "0.00";
                                                                                dQuantity = 0;
                                                                            }
                                                                        }
                                                                        if (ilItem is UsageBasedLineItem)
                                                                        {
                                                                            try
                                                                            {
                                                                                dQuantity = dRPCP / (dUnitPrice * dDollar);
                                                                                sQuantityToShow = dQuantity.ToString("N");
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                ex.ToString();
                                                                                sQuantityToShow = "0.00";
                                                                                dQuantity = 0;
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        try
                                                                        {
                                                                            //dQuantity = dRPCP / (dUnitPrice * dDollar);
                                                                            //sQuantityToShow = dQuantity.ToString("N");

                                                                            dRPCP = dTotalAmount * ((100 + dMarkup) / 100);
                                                                            sRPCP = dRPCP.ToString("N");
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            ex.ToString();
                                                                            //sQuantityToShow = "0.00";
                                                                            //dQuantity = 0;

                                                                            dRPCP = 0;
                                                                            sRPCP = "0.00";
                                                                        }
                                                                    }
                                                                                                                                     
                                                                    if (rbtnSeats.Checked == true)
                                                                    {
                                                                        if (ilItem is LicenseBasedLineItem)
                                                                        {
                                                                            /*
                                                                            try
                                                                            {
                                                                                dRPCP = dRPCPPerUP * dQuantity;
                                                                                sRPCP = dRPCP.ToString("N");
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                ex.ToString();
                                                                                dRPCPPerUP = 0;
                                                                                sRPCPPerUP = "";
                                                                            }
                                                                            */
                                                                            try
                                                                            {
                                                                                //dRPCPPerUP = dUnitPrice * dDollar;                                                                                
                                                                                dRPCPPerUP = dRPCP / dQuantity;
                                                                                sRPCPPerUP = dRPCPPerUP.ToString("N");
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                ex.ToString();
                                                                                dRPCPPerUP = 0;
                                                                                sRPCPPerUP = "";
                                                                            }
                                                                        }
                                                                        if (ilItem is UsageBasedLineItem)
                                                                        {
                                                                            try
                                                                            {
                                                                                //dRPCPPerUP = dUnitPrice * dDollar;                                                                                
                                                                                dRPCPPerUP = dRPCP / dQuantity;
                                                                                sRPCPPerUP = dRPCPPerUP.ToString("N");
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                ex.ToString();
                                                                                dRPCPPerUP = 0;
                                                                                sRPCPPerUP = "";
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        try
                                                                        {
                                                                            //dRPCPPerUP = dUnitPrice * dDollar;                                                                                
                                                                            //dRPCPPerUP = dRPCP / dQuantity;
                                                                            //sRPCPPerUP = dRPCPPerUP.ToString("N");

                                                                            dQuantity = dRPCP / dRPCPPerUP;
                                                                            sQuantityToShow = dQuantity.ToString("N");
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            ex.ToString();
                                                                            dQuantity = 0;
                                                                            sQuantityToShow = "0.00";
                                                                        }
                                                                    }

                                                                    dRPCPDiff = dRPCP - dTotalAmount;
                                                                    sRPCPDiff = dRPCPDiff.ToString("N");
                                                                    dCustRPUDiffAmount += dRPCPDiff;

                                                                    dCustMSListAmount += (dListPrice * dQuantity); 
                                                                    dCustERPAmount += (dERPPrice * dQuantity);
                                                                    dCustRPTotalAmount += dRPCP;

                                                                    dCustMSUTotalAmount += dTotalAmount;
                                                                    dCustRPUTotalAmount += dRPCP;

                                                                    // description
                                                                    int iNavDescStart = -1;
                                                                    if (sCustVatNo != "n/a")
                                                                    {
                                                                        if (true) // if (dRPCP != 0) - restrict invoice line to be pushed to nav
                                                                        {
                                                                            if (true) // if (dQuantity != 0) - restrict invoice line to be pushed to nav
                                                                            {
                                                                                // create invoice line
                                                                                Sales_Invoice_Line invoiceLine = new Sales_Invoice_Line();

                                                                                // item
                                                                                invoiceLine.Type = NAVOrdersWS.Type.Item;

                                                                                // hardcoded
                                                                                invoiceLine.No = "310";

                                                                                // quantity
                                                                                invoiceLine.Quantity = dQuantity;

                                                                                // unit price
                                                                                invoiceLine.Unit_Price = dRPCPPerUP;

                                                                                // no vat values
                                                                                invoiceLine.Total_Amount_Incl_VATSpecified = false;
                                                                                invoiceLine.Total_Amount_Excl_VATSpecified = false;
                                                                                invoiceLine.Total_VAT_AmountSpecified = false;

                                                                                // description
                                                                                string[] sLineDescriptionArray = sDescription.Split('ђ');
                                                                                string sLineDescription = sLineDescriptionArray[0];
                                                                                iNavDescStart = iInvoiceLinesCount;
                                                                                if (sLineDescription.Length <= 50)
                                                                                {
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

                                                                                    string sLineDescriptionFriendlyChars2 = sLineDescription.Replace(" ", "≡");
                                                                                    string[] sLineDescriptionWords2 = sLineDescriptionFriendlyChars2.Split('≡');

                                                                                    // check if there are words nigger than 50 chars
                                                                                    string sLineDescriptionFriendlyChars = "";
                                                                                    foreach (var sLineDescriptionWord in sLineDescriptionWords2)
                                                                                    {
                                                                                        if (sLineDescriptionWord.Length < partLength)
                                                                                        {
                                                                                            sLineDescriptionFriendlyChars += sLineDescriptionWord + "≡";
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            sLineDescriptionFriendlyChars += sLineDescriptionWord.Substring(0, partLength) + "≡";
                                                                                            string sTmp = sLineDescriptionWord.Substring(partLength);
                                                                                            if (sTmp.Length < partLength)
                                                                                            {
                                                                                                sLineDescriptionFriendlyChars += sTmp + "≡";
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                sLineDescriptionFriendlyChars += sTmp.Substring(0, partLength) + "≡";
                                                                                                sTmp = sTmp.Substring(partLength);
                                                                                                sLineDescriptionFriendlyChars += sTmp.Substring(partLength) + "≡";
                                                                                            }
                                                                                        }
                                                                                    }
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

                                                                                // date + month
                                                                                for (int i = 1; i <= 2; i++)
                                                                                {
                                                                                    string sLineDescriptionDateMonth = sLineDescriptionArray[i];

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
                                                                                    extraLine.Description = sLineDescriptionDateMonth;

                                                                                    // add extra line
                                                                                    InvoiceLinesList.Add(extraLine);

                                                                                    // count added lines
                                                                                    iInvoiceLinesCount++;
                                                                                }

                                                                                // extra empty line
                                                                                Sales_Invoice_Line extraemptyLine = new Sales_Invoice_Line();

                                                                                extraemptyLine.Type = NAVOrdersWS.Type.Item;
                                                                                extraemptyLine.No = "";

                                                                                // quantity and price
                                                                                extraemptyLine.Quantity = 0;
                                                                                extraemptyLine.Unit_Price = 0;

                                                                                extraemptyLine.Total_Amount_Incl_VATSpecified = false;
                                                                                extraemptyLine.Total_Amount_Excl_VATSpecified = false;
                                                                                extraemptyLine.Total_VAT_AmountSpecified = false;

                                                                                // extra line
                                                                                extraemptyLine.Description = " ";

                                                                                // add extra line
                                                                                InvoiceLinesList.Add(extraemptyLine);

                                                                                // count added lines
                                                                                iInvoiceLinesCount++;
                                                                            }
                                                                        }
                                                                    }

                                                                    string sCustomerNavDetails = "<font color='red'>Customer doesn't exist in NAV!</font>";
                                                                    if (sCustVatNo != "n/a")
                                                                    {
                                                                        sCustomerNavDetails = "<font color='green'>No (VAT): " + sCustVatNo + "</font>";
                                                                    }

                                                                    string sDescriptionNavLines = "";
                                                                    if (iNavDescStart != -1)
                                                                    {
                                                                        for(int iD=iNavDescStart; iD < iInvoiceLinesCount; iD++)
                                                                        {
                                                                            sDescriptionNavLines += InvoiceLinesList[iD].Description + "<br />";
                                                                        }
                                                                    }

                                                                    AzureBillingDataL.Text += "<b>Navision mapping:</b><br /><br />";
                                                                    AzureBillingDataL.Text += "<b>Sell_to_Customer_No:</b> " + sCustomerId + " - " + sCustomerNavDetails + "<br />";
                                                                    AzureBillingDataL.Text += "<b>Customer_Name:</b> " + sCustomerName + "<br />";
                                                                    AzureBillingDataL.Text += "<b>Type:</b> ITEM<br />";
                                                                    AzureBillingDataL.Text += "<b>No:</b> " + sProductNo + "<br />";
                                                                    //AzureBillingDataL.Text += "<br /><b>Description:</b> " + sDescription.Replace("ђ", " ") + "<br />";
                                                                    AzureBillingDataL.Text += "<br /><b>Description:</b><br />";
                                                                    AzureBillingDataL.Text += sDescriptionNavLines;
                                                                    AzureBillingDataL.Text += "<b>Total_Amount_Excl_VAT:</b> " + sTotalAmount + "<br />";
                                                                    AzureBillingDataL.Text += "<b>Unit_Price:</b> " + sUnitPrice + "<br />";

                                                                    /*
                                                                    if (ilItem is LicenseBasedLineItem)
                                                                    {
                                                                        AzureBillingDataL.Text += "<b>Offer Name:</b> " + sOfferNameToDisplay + "<br />";
                                                                        AzureBillingDataL.Text += "<b>Offer Id:</b> " + sOfferIdToDisplay + "<br />";
                                                                        AzureBillingDataL.Text += "<b>List Price:</b> " + dListPrice.ToString("N") + "<br />";
                                                                        AzureBillingDataL.Text += "<b>ERP Price:</b> " + dERPPrice.ToString("N") + "<br />";
                                                                    }
                                                                    */

                                                                    if (sDollarPrice != "n/a")
                                                                    {
                                                                        AzureBillingDataL.Text += "<b>PCToBCExchangeRate:</b> " + sDollarPrice + "<br />";
                                                                    }
                                                                    AzureBillingDataL.Text += "<br />";

                                                                    if (ilItem is LicenseBasedLineItem)
                                                                    {
                                                                        /*
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Quantity:</b></font> " + sQuantityToShow + "<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Rebate:</b></font> " + sMarkup + "%<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Rebate Unit Price:</b></font> " + sRPCPPerUP + " DKK<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Rebate Total Price:</b></font> " + sRPCP + " DKK<br />";
                                                                        */

                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Quantity:</b></font> " + sQuantityToShow + "<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup:</b></font> " + sMarkup + "%<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Unit Price:</b></font> " + sRPCPPerUP + " DKK<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Total Price:</b></font> " + sRPCP + " DKK<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Diff:</b></font> " + sRPCPDiff + " DKK<br />";
                                                                    }
                                                                    else
                                                                    {
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Quantity:</b></font> " + sQuantityToShow + "<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup:</b></font> " + sMarkup + "%<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Unit Price:</b></font> " + sRPCPPerUP + " DKK<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Total Price:</b></font> " + sRPCP + " DKK<br />";
                                                                        AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Diff:</b></font> " + sRPCPDiff + " DKK<br />";
                                                                    }

                                                                    AzureBillingDataL.Text += "<br /><b>Total_Amount_Incl_VATSpecified:</b> FALSE<br />";
                                                                    AzureBillingDataL.Text += "<b>Total_Amount_Excl_VATSpecified:</b> FALSE<br />";
                                                                    AzureBillingDataL.Text += "<b>Total_VAT_AmountSpecified:</b> FALSE<br />";
                                                                    AzureBillingDataL.Text += "<b>Allow_Invoice_Disc:</b> TRUE<br />";
                                                                    AzureBillingDataL.Text += "<b>Allow_Item_Charge_Assignment:</b> TRUE<br />";

                                                                    AzureBillingDataL.Text += "<br />";
                                                                    iCount++;
                                                                }
                                                            }
                                                        }

                                                        if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                                        {
                                                            /*
                                                            dCustRPTotalDBMArkupAmount = dCustRPTotalAmount - dCustMSListAmount;

                                                            if (dCustMSListAmount != 0)
                                                            {
                                                                dCustRPMarkup = (dCustRPTotalDBMArkupAmount / dCustMSListAmount) * 100;
                                                            }
                                                            else
                                                            {
                                                                dCustRPMarkup = 0;
                                                            }
                                                            
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TL" + sCust + "#", dCustMSListAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TE" + sCust + "#", dCustERPAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#R" + sCust + "#", dCustRPRebate.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TC" + sCust + "#", dCustRPTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TM" + sCust + "#", dCustRPTotalDBMArkupAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#M" + sCust + "#", dCustRPMarkup.ToString("N"));
                                                            */

                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TM" + sCust + "#", dCustMSUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#M" + sCust + "#", dCustRPUMarkup.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TC" + sCust + "#", dCustRPUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TD" + sCust + "#", dCustRPUDiffAmount.ToString("N"));
                                                        }
                                                        else
                                                        {
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TM" + sCust + "#", dCustMSUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#M" + sCust + "#", dCustRPUMarkup.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TC" + sCust + "#", dCustRPUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TD" + sCust + "#", dCustRPUDiffAmount.ToString("N"));
                                                        }

                                                        // sums
                                                        if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                                        {
                                                            /*
                                                            dTCustMSListAmount += dCustMSListAmount;
                                                            dTCustERPAmount += dCustERPAmount;
                                                            dTCustRPTotalAmount += dCustRPTotalAmount;
                                                            dTCustRPTotalDBMArkupAmount += dCustRPTotalDBMArkupAmount;
                                                            dTCustRPMarkup += dCustRPMarkup;
                                                            iCustomerCount++;
                                                            */

                                                            dTCustMSUTotalAmount += dCustMSUTotalAmount;
                                                            dTCustRPUTotalAmount += dCustRPUTotalAmount;
                                                            dTCustRPUDiffAmount += dCustRPUDiffAmount;
                                                            dTCustRPUMarkup += dCustRPUMarkup;
                                                        }
                                                        else
                                                        {
                                                            dTCustMSUTotalAmount += dCustMSUTotalAmount;
                                                            dTCustRPUTotalAmount += dCustRPUTotalAmount;
                                                            dTCustRPUDiffAmount += dCustRPUDiffAmount;
                                                            dTCustRPUMarkup += dCustRPUMarkup;
                                                        }

                                                        AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#LINESNUM#", (iCount - 1).ToString());

                                                        if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                                        {
                                                            /*
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALMS#", dCustERPAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRP#", dCustRPTotalAmount.ToString("N"));
                                                            */

                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALMS#", dCustMSUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRP#", dCustRPUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRPDIFF#", dCustRPUDiffAmount.ToString("N"));
                                                        }
                                                        else
                                                        {
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALMS#", dCustMSUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRP#", dCustRPUTotalAmount.ToString("N"));
                                                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRPDIFF#", dCustRPUDiffAmount.ToString("N"));
                                                        }

                                                        if (iCount > 0)
                                                        {
                                                            if (sAction == "Data")
                                                            {
                                                                PushDataToNavB.Enabled = true;
                                                                PushDataToNavB.Visible = true;
                                                                PushingDataL.Text = "";
                                                            }

                                                            if ((sAction == "Navision") && ((sCustomerVAT == "ALL") || (sCustomerVAT == sCustVatNo)))
                                                            {
                                                                PushDataToNavB.Enabled = false;
                                                                PushDataToNavB.Visible = false;
                                                                PushingDataL.Text = "Data pushed to Navision.";

                                                                if (sCustomerVAT == sCustVatNo)
                                                                {
                                                                    PushingDataL.Text = "Customer " + sCust + " (" + sCustVatNo + ") pushed to Navision.";
                                                                }
                                                            }
                                                        }

                                                        if ((sAction == "Navision") && ((sCustomerVAT == "ALL") || (sCustomerVAT == sCustVatNo)))
                                                        {
                                                            if (iInvoiceLinesCount > 0)
                                                            {
                                                                order.SalesLines = new Sales_Invoice_Line[iInvoiceLinesCount];
                                                                for (int i = 0; i < iInvoiceLinesCount; i++)
                                                                {
                                                                    order.SalesLines[i] = new Sales_Invoice_Line();
                                                                }
                                                                sal.Update(ref order);

                                                                int iOrderLinesCount = 0;
                                                                foreach (Sales_Invoice_Line sil in InvoiceLinesList)
                                                                {
                                                                    order.SalesLines[iOrderLinesCount].Type = sil.Type;
                                                                    order.SalesLines[iOrderLinesCount].No = sil.No;
                                                                    order.SalesLines[iOrderLinesCount].Quantity = sil.Quantity;
                                                                    order.SalesLines[iOrderLinesCount].Unit_Price = sil.Unit_Price;
                                                                    order.SalesLines[iOrderLinesCount].Total_Amount_Incl_VATSpecified = sil.Total_Amount_Incl_VATSpecified;
                                                                    order.SalesLines[iOrderLinesCount].Total_Amount_Excl_VATSpecified = sil.Total_Amount_Excl_VATSpecified;
                                                                    order.SalesLines[iOrderLinesCount].Total_VAT_AmountSpecified = sil.Total_VAT_AmountSpecified;
                                                                    order.SalesLines[iOrderLinesCount].Description = sil.Description;
                                                                    iOrderLinesCount++;
                                                                }
                                                                sal.Update(ref order);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            AzureBillingDataL.Text += "</div>";

                                            if (invoiceDetail.BillingProvider == BillingProvider.Office)
                                            {
                                                /*
                                                if (iCustomerCount > 0)
                                                {
                                                    dTCustRPMarkup = dTCustRPMarkup / iCustomerCount;
                                                }
                                                else
                                                {
                                                    dTCustRPMarkup = 0;
                                                }
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTL#", dTCustMSListAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTE#", dTCustERPAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTC#", dTCustRPTotalAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTM#", dTCustRPTotalDBMArkupAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMM#", dTCustRPMarkup.ToString("N"));
                                                */

                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTM#", dTCustMSUTotalAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTC#", dTCustRPUTotalAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTD#", dTCustRPUDiffAmount.ToString("N"));
                                            }
                                            else
                                            {
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTM#", dTCustMSUTotalAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTC#", dTCustRPUTotalAmount.ToString("N"));
                                                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTD#", dTCustRPUDiffAmount.ToString("N"));
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // close markups
            ClientScript.RegisterStartupScript(GetType(), "HideCustomers", "window.onload = function() { toogleMarkup(); }", true);

        }

        private string DoesCustomerExists(CustomerInfo2_Service service, List<CustomerInfo2_Filter> filter)
        {
            string sResult = "n/a";

            try
            {
                // Run the actual search.
                CustomerInfo2[] customers = service.ReadMultiple(filter.ToArray(), null, 100);
                foreach (CustomerInfo2 customer in customers)
                {
                    sResult = customer.No;
                    break;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = "n/a";
            }

            return sResult;
        }

        public async void AzureBillingDataB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (rbtnSeats.Checked == true)
            {
                await GetInvoiceData("Seats", "Data", "ALL");
            }

            if (rtbnUsage.Checked == true)
            {
                await GetInvoiceData("Usage", "Data", "ALL");
            }
        }

        protected async void PushDataToNavB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (rbtnSeats.Checked == true)
            {
                await GetInvoiceData("Seats", "Navision", "ALL");
            }

            if (rtbnUsage.Checked == true)
            {
                await GetInvoiceData("Usage", "Navision", "ALL");
            } 

            // close markups
            ClientScript.RegisterStartupScript(GetType(), "HideCustomers", "window.onload = function() { toogleMarkup(); }", true);
        }

        protected void Unnamed_TextChanged(object sender, EventArgs e)
        {
            if (Page.IsPostBack == true)
            {
                TextBox tb = (TextBox)sender;
                AttributeCollection ac = tb.Attributes;
                string sCustId = ac["CustId"];
                string sCustName = ac["CustName"];
                string sCustMarkup = tb.Text;

                string sMarkupFile = "MARKUPSeats.xml";
                if (rbtnSeats.Checked == true)
                {
                    sMarkupFile = "MARKUPSeats.xml";
                    MarkupType.Text = "SEATS Type: MARKUP";
                }
                if (rtbnUsage.Checked == true)
                {
                    MarkupType.Text = "USAGE Type: MARKUP";
                    sMarkupFile = "MARKUPUsage.xml";
                }

                if (sCustId != "")
                {
                    UpdateXml(sCustId, sCustMarkup, sMarkupFile);
                }
            }
        }

        protected void rbtnSeats_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnSeats.Checked == true)
            {
                LBFileInfo.Visible = false;
            }
            else
            {
                LBFileInfo.Visible = false;
            }
            HandleCustomersData();
        }

        protected void rtbnUsage_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnSeats.Checked == true)
            {
                LBFileInfo.Visible = false;
            }
            else
            {
                LBFileInfo.Visible = false;
            }
            HandleCustomersData();
        }
    }
}