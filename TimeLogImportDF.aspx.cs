using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RPNAVConnect.NAVCustomersWS;
using RPNAVConnect.NAVOrdersWS;
using RPNAVConnect.NAVSalesCRMemoWS;

using System.Net;
using System.Xml;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data.OleDb;
using System.Globalization;

namespace RPNAVConnect
{
    public partial class TimeLogImportDF : System.Web.UI.Page
    {
        public TimeLogDataWS.strRPNAVConnectWS stRPNAVConnectWS = null;

        public static string sNAVLogin = "rpnavapi";
        public static string sNAVPassword = "Telefon1";
        public static string sNAVDomain = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // init page
            if (Page.IsPostBack == false)
            {
                StartMonthTB.Text = DateTime.Now.AddMonths(-1).Month.ToString();
                StartYearTB.Text = DateTime.Now.AddMonths(-1).Year.ToString();
                EndMonthTB.Text = DateTime.Now.AddMonths(-1).Month.ToString();
                EndYearTB.Text = DateTime.Now.AddMonths(-1).Year.ToString();
            }
        }

        protected void TimeLogDataB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            PPSep1.Visible = false;
            PPSep2.Visible = false;
            PushDataToNavB.Visible = false;
            AllowInvoicesWithoutLinesCB.Visible = false;
            PushingDataL.Text = "<p style='height:15px;'>&nbsp;</p>";

            TimeLogDataWS.RPNAVConnectWS wsRPNAVConnectWS = new TimeLogDataWS.RPNAVConnectWS();
            wsRPNAVConnectWS.Timeout = 5400000;
            wsRPNAVConnectWS.UseDefaultCredentials = true;

            TLInfoLabel.Text = "TimeLog Web Service URL: ";
            TLInfoLabel.Text += ConfigurationManager.AppSettings["TLWSURL"].ToString();
            TLInfoLabel.Text += "<br />";
            TLInfoLabel.Text += wsRPNAVConnectWS.GetCredentials();

            string sVATNos = VATNoTB.Text;
            if (sVATNos == "") sVATNos = "n/a";
            stRPNAVConnectWS = wsRPNAVConnectWS.GetTimeLogData(sVATNos, InvoiceStatusTB.Text, StartMonthTB.Text, StartYearTB.Text, EndMonthTB.Text, EndYearTB.Text);

            // open db connection
            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            int iInvoicesAllCount = 0;

            try
            {
                Session["stRPNAVConnectWS"] = stRPNAVConnectWS;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            if (stRPNAVConnectWS.sResultDesc != "Ok")
            {
                TimeLogDataL.Text = "Error:<br /><br />" + stRPNAVConnectWS.sResultDesc;
            }
            else
            {
                try
                {
                    // default
                    TimeLogDataL.Text = "";

                    string[] sResultCustomerArray = stRPNAVConnectWS.sResultCustomers.Replace("<br />", "≡").Split('≡');
                    int iCustomersCount = 0;
                    foreach (string sResultCustomer in sResultCustomerArray)
                    {
                        if ((sResultCustomer != "") && (sResultCustomer != "Not found."))
                        {
                            if (sResultCustomer.Split(',')[3].Replace("\"", "").Replace("╬", ",").Replace(" ", "") != "")
                            {
                                // invoices
                                string[] sResultInvoicesArray = stRPNAVConnectWS.sResultInvoices.Replace("<br /><br />", "≡").Split('≡');

                                int iInvoicesCount = 0;
                                foreach (string sResultInvoice in sResultInvoicesArray)
                                {
                                    if ((sResultInvoice != "") && (sResultInvoice != "Not found."))
                                    {
                                        // search if invoice already processed
                                        if (IsOrderAlreadyProcessed(sResultInvoice.Split(',')[0].Replace("╬", ",").Replace("\"", ""), dbConn) == false)
                                        {
                                            if (sResultInvoice.Split(',')[9] == sResultCustomer.Split(',')[0])
                                            {
                                                if (iInvoicesCount == 0)
                                                {
                                                    if (iCustomersCount == 0)
                                                    {
                                                        TimeLogDataL.Text += "<table cellpadding='3' cellspacing='3' border='0' width='100%'>";
                                                        iCustomersCount++;
                                                    }
                                                    // customer - show only if inoices exists
                                                    TimeLogDataL.Text += "<tr>";
                                                    TimeLogDataL.Text += "<td><a href='javascript:toogleInvoices(\"" + sResultCustomer.Split(',')[0].Replace("\"", "").ToString() + "\");'><b>" + sResultCustomer.Split(',')[1].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "</b></a><br/>";
                                                    TimeLogDataL.Text += "<div style='display: none;' id='cgs_" + sResultCustomer.Split(',')[0].Replace("\"", "").ToString() + "' name='cgs_" + sResultCustomer.Split(',')[0].Replace("\"", "").ToString() + "'>";
                                                    TimeLogDataL.Text += sResultCustomer.Split(',')[4].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br />";
                                                    TimeLogDataL.Text += sResultCustomer.Split(',')[5].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + " " + sResultCustomer.Split(',')[6].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br />";
                                                    TimeLogDataL.Text += sResultCustomer.Split(',')[7].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br />";
                                                    TimeLogDataL.Text += "Tel. " + sResultCustomer.Split(',')[8].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br /><br />";
                                                    TimeLogDataL.Text += "CVR: " + sResultCustomer.Split(',')[3].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br /><br />";
                                                    TimeLogDataL.Text += "Timelog No: " + sResultCustomer.Split(',')[2].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br />";
                                                    TimeLogDataL.Text += "KAM: " + sResultCustomer.Split(',')[11].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br />";
                                                    TimeLogDataL.Text += "Faktura: " + sResultCustomer.Split(',')[9].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "<br /><br />";
                                                    TimeLogDataL.Text += "</div>";
                                                    TimeLogDataL.Text += "</td>";
                                                    TimeLogDataL.Text += "</tr>";

                                                    TimeLogDataL.Text += "<tr><td>";
                                                    TimeLogDataL.Text += "<div style='display: none;' id='cg_" + sResultCustomer.Split(',')[0].Replace("\"", "").ToString() + "' name='cg_" + sResultCustomer.Split(',')[0].Replace("\"", "").ToString() + "'>";
                                                }

                                                iInvoicesCount++;

                                                // invoice lines
                                                string[] sResultInvoiceLinesArray = stRPNAVConnectWS.sResultInvoiceLines.Replace("<br /><br />", "≡").Split('≡');
                                                int iInvoiceLinesCount = 0;
                                                TimeLogDataL.Text += "<table border='0' width='100%'>";

                                                TimeLogDataL.Text += "<tr><td colspan='4' style='border-bottom:1pt solid black;'><input type='checkbox' name='inv_" + sResultCustomer.Split(',')[2].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "_" + sResultInvoice.Split(',')[0].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "' id='inv_" + sResultCustomer.Split(',')[2].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "_" + sResultInvoice.Split(',')[0].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "' value='TL_SELECTED_INVOICE' autocomplete='off' checked />";
                                                TimeLogDataL.Text += "&nbsp;Invoice No: " + sResultInvoice.Split(',')[0].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "&nbsp;";
                                                TimeLogDataL.Text += "&nbsp;Invoice Date: " + sResultInvoice.Split(',')[4].Replace("\"", "").Replace("╬", ",").Replace(";", " ").Substring(8, 2) + "-";
                                                TimeLogDataL.Text += sResultInvoice.Split(',')[4].Replace("\"", "").Replace("╬", ",").Replace(";", " ").Substring(5, 2) + "-";
                                                TimeLogDataL.Text += sResultInvoice.Split(',')[4].Replace("\"", "").Replace("╬", ",").Replace(";", " ").Substring(0, 4) + "&nbsp;</td>";
                                                TimeLogDataL.Text += "</td></tr>";

                                                foreach (string sResultInvoiceLine in sResultInvoiceLinesArray)
                                                {
                                                    if ((sResultInvoiceLine != "") && (sResultInvoiceLine != "Not found."))
                                                    {
                                                        if (sResultInvoice.Split(',')[0] == sResultInvoiceLine.Split(',')[1])
                                                        {
                                                            // remove multiple spaces & odd empty chars
                                                            RegexOptions options = RegexOptions.None;
                                                            Regex regex = new Regex(@"[ ]{2,}", options);
                                                            string sLineDescription = sResultInvoiceLine.Split(',')[4].Replace("\"", "").Replace("╬", ",");
                                                            sLineDescription = regex.Replace(sLineDescription, @" ");
                                                            sLineDescription = Regex.Replace(sLineDescription, @"\p{Z}", " ");

                                                            string sProjectName = sResultInvoiceLine.Split(',')[14].Replace("╬", ",").Replace("\"", "");
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
                                                                        TimeLogDataL.Text += "<tr>";
                                                                        TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;" + sResultInvoiceLine.Split(',')[3].Replace("\"", "").Replace("╬", ",").Substring(8, 2) + "-";
                                                                        TimeLogDataL.Text += sResultInvoiceLine.Split(',')[3].Replace("\"", "").Replace("╬", ",").Substring(5, 2) + "-";
                                                                        TimeLogDataL.Text += sResultInvoiceLine.Split(',')[3].Replace("\"", "").Replace("╬", ",").Substring(0, 4) + "&nbsp;</td>";
                                                                        TimeLogDataL.Text += "<td style='width:55%;'>&nbsp;<b>" + sInvoiceLineDescToPrint + "</b>&nbsp;</td>";

                                                                        // quantity and price
                                                                        decimal dQuantity = 0;
                                                                        try
                                                                        {
                                                                            dQuantity = Convert.ToDecimal(sResultInvoiceLine.Split(',')[5].Replace("╬", ",").Replace("\"", ""));
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            ex.ToString();
                                                                            dQuantity = 0;
                                                                        }
                                                                        decimal dUnitPrice = 0;
                                                                        try
                                                                        {
                                                                            dUnitPrice = Convert.ToDecimal(sResultInvoiceLine.Split(',')[6].Replace("╬", ",").Replace("\"", ""));
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            ex.ToString();
                                                                            dUnitPrice = 0;
                                                                        }

                                                                        TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;" + dQuantity.ToString("N") + "&nbsp;</td>";
                                                                        TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;" + dUnitPrice.ToString("N") + "&nbsp;</td>";
                                                                        TimeLogDataL.Text += "</tr>";
                                                                        iInvLinesCount++;
                                                                    }

                                                                    // description
                                                                    sInvoiceLineDescToPrint = sLineDescription;
                                                                    string sProductNo = "&nbsp;ProductNo: " + sResultInvoiceLine.Split(',')[16].Replace("\"", "").Replace("╬", ",");
                                                                    TimeLogDataL.Text += "<tr>";
                                                                    TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                    TimeLogDataL.Text += "<td style='width:55%;'>&nbsp;" + sInvoiceLineDescToPrint + "&nbsp;<br />" + sProductNo + " &nbsp;</td>";
                                                                    TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                    TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                    TimeLogDataL.Text += "</tr>";
                                                                    iInvLinesCount++;

                                                                    bAllInvoiceLinesPrinted = true;
                                                                }
                                                                else
                                                                {
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
                                                                            if (iInvLinesCount == 0)
                                                                            {
                                                                                // extra line
                                                                                sInvoiceLineDescToPrint = sProjectName;
                                                                                if (sInvoiceLineDescToPrint.Length > 50)
                                                                                {
                                                                                    sInvoiceLineDescToPrint = sInvoiceLineDescToPrint.Substring(0, 50);
                                                                                }
                                                                                TimeLogDataL.Text += "<tr>";
                                                                                TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;" + sResultInvoiceLine.Split(',')[3].Replace("\"", "").Replace("╬", ",").Substring(8, 2) + "-";
                                                                                TimeLogDataL.Text += sResultInvoiceLine.Split(',')[3].Replace("\"", "").Replace("╬", ",").Substring(5, 2) + "-";
                                                                                TimeLogDataL.Text += sResultInvoiceLine.Split(',')[3].Replace("\"", "").Replace("╬", ",").Substring(0, 4) + "&nbsp;</td>";
                                                                                TimeLogDataL.Text += "<td style='width:55%;'>&nbsp;<b>" + sInvoiceLineDescToPrint + "</b>&nbsp;</td>";
                                                                                // quantity and price
                                                                                decimal dQuantity = 0;
                                                                                try
                                                                                {
                                                                                    dQuantity = Convert.ToDecimal(sResultInvoiceLine.Split(',')[5].Replace("╬", ",").Replace("\"", ""));
                                                                                }
                                                                                catch (Exception ex)
                                                                                {
                                                                                    ex.ToString();
                                                                                    dQuantity = 0;
                                                                                }
                                                                                decimal dUnitPrice = 0;
                                                                                try
                                                                                {
                                                                                    dUnitPrice = Convert.ToDecimal(sResultInvoiceLine.Split(',')[6].Replace("╬", ",").Replace("\"", ""));
                                                                                }
                                                                                catch (Exception ex)
                                                                                {
                                                                                    ex.ToString();
                                                                                    dUnitPrice = 0;
                                                                                }

                                                                                TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;" + dQuantity.ToString("N") + "&nbsp;</td>";
                                                                                TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;" + dUnitPrice.ToString("N") + "&nbsp;</td>";
                                                                                TimeLogDataL.Text += "</tr>";
                                                                                iInvLinesCount++;
                                                                            }

                                                                            // description
                                                                            sInvoiceLineDescToPrint = item.Value;
                                                                            string sProductNo = "&nbsp;ProductNo: " + sResultInvoiceLine.Split(',')[16].Replace("\"", "").Replace("╬", ",");
                                                                            TimeLogDataL.Text += "<tr>";
                                                                            TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                            TimeLogDataL.Text += "<td style='width:55%;'>&nbsp;" + sInvoiceLineDescToPrint + "&nbsp;<br />" + sProductNo + "&nbsp;</td>";
                                                                            TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                            TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                            TimeLogDataL.Text += "</tr>";
                                                                            iInvLinesCount++;
                                                                        }
                                                                        else
                                                                        {
                                                                            // description
                                                                            sInvoiceLineDescToPrint = item.Value;
                                                                            string sProductNo = "&nbsp;ProductNo: " + sResultInvoiceLine.Split(',')[16].Replace("\"", "").Replace("╬", ",");
                                                                            TimeLogDataL.Text += "<tr>";
                                                                            TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                            TimeLogDataL.Text += "<td style='width:55%;'>&nbsp;" + sInvoiceLineDescToPrint + "&nbsp;<br />" + sProductNo + "&nbsp;</td>";
                                                                            TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                            TimeLogDataL.Text += "<td style='width:15%;'>&nbsp;</td>";
                                                                            TimeLogDataL.Text += "</tr>";
                                                                            iInvLinesCount++;
                                                                        }
                                                                        iPartsCount++;
                                                                    }
                                                                    bAllInvoiceLinesPrinted = true;
                                                                }
                                                            }
                                                            iInvoiceLinesCount = iInvoiceLinesCount + iInvLinesCount;
                                                        }
                                                    }
                                                }
                                                if (iInvoiceLinesCount == 0)
                                                {
                                                    TimeLogDataL.Text += "<tr><td colspan='5'>&nbsp;<i>No invoice lines found</i>&nbsp;</td></tr>";
                                                }
                                                TimeLogDataL.Text += "<tr height='15px'><td colspan='5' height='15px'>&nbsp;</td></tr>";
                                                TimeLogDataL.Text += "</table>";
                                            }
                                        }
                                    }
                                }

                                // toggle div 
                                if (iInvoicesCount > 0)
                                {
                                    TimeLogDataL.Text += "</div>";
                                    TimeLogDataL.Text += "</td></tr>";
                                }

                                // all invoices count
                                iInvoicesAllCount = iInvoicesAllCount + iInvoicesCount;
                            }
                        }
                    }

                    if (iCustomersCount > 0)
                    {
                        TimeLogDataL.Text += "</table>";
                    }

                    if (iInvoicesAllCount > 0)
                    {
                        PPSep1.Visible = true;
                        PPSep2.Visible = true;
                        PushDataToNavB.Visible = true;
                        //DeleteMarkedInvoicesB.Visible = true;
                        AllowInvoicesWithoutLinesCB.Visible = true;
                        PushingDataL.Text = "<p style='height:15px;'>&nbsp;</p>";
                    }
                    else
                    {
                        TimeLogDataL.Text += "<i>Der blev ikke fundet bilag til fakturering i TimeLog</i><br />";
                    }

                    // scroll down
                    ClientScript.RegisterStartupScript(GetType(), "ScrollScript", "window.onload = function() {document.getElementById('lastscriptdiv').scrollIntoView(true);}", true);

                }
                catch (Exception ex)
                {
                    ex.ToString();

                    PPSep1.Visible = false;
                    PPSep2.Visible = false;

                    PushDataToNavB.Visible = false;
                    AllowInvoicesWithoutLinesCB.Visible = false;

                    TimeLogDataL.Text = "<b>Problem acquiring TimeLog data:</b><br />";
                    TimeLogDataL.Text += ex.ToString();
                }
            }

            dbConn.Close();
        }

        protected void PushDataToNavB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            NameValueCollection FormPageVars;
            FormPageVars = Request.Form;

            int iInvoiceNumber = 0;
            int iInvoiceAllLinesCount = 0;
            string sResultMessage = "";
            string sMissedCustomers = "";
            string sProblematicCustomers = "";
            PushingDataLErrorData.Text = "";

            try
            {
                stRPNAVConnectWS = (TimeLogDataWS.strRPNAVConnectWS)Session["stRPNAVConnectWS"];
            }
            catch (Exception ex)
            {
                sResultMessage = ex.ToString();
                stRPNAVConnectWS = null;
            }

            // open db connection
            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            if (stRPNAVConnectWS != null)
            {
                try
                {
                    if (stRPNAVConnectWS.sResultDesc == "Ok")
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

                        // get customers and invoices
                        string[] sResultCustomerArray = stRPNAVConnectWS.sResultCustomers.Replace("<br />", "≡").Split('≡');
                        string[] sResultInvoicesArray = stRPNAVConnectWS.sResultInvoices.Replace("<br /><br />", "≡").Split('≡');

                        foreach (string sResultCustomer in sResultCustomerArray)
                        {
                            if (sResultCustomer != "")
                            {
                                bool bProblematicCustomer = false;
                                if (sResultCustomer.Split(',')[3].Replace("\"", "").Replace("╬", ",").Replace(" ", "") == "")
                                {
                                    bProblematicCustomer = true;
                                }

                                // searh filter for the customer
                                List<CustomerInfo2_Filter> filterArray = new List<CustomerInfo2_Filter>();
                                CustomerInfo2_Filter nameFilter = new CustomerInfo2_Filter();
                                nameFilter.Field = CustomerInfo2_Fields.No; 
                                nameFilter.Criteria = sResultCustomer.Split(',')[3].Replace("\"", "").Replace("╬", ",");
                                filterArray.Add(nameFilter);

                                // searh if this customer already exists
                                bool bCustomerCreatedInNav = false;
                                if (bProblematicCustomer == false)
                                {
                                    if (DoesCustomerExists(nav, filterArray) == false)
                                    {
                                        try
                                        {
                                            // customer doesn't exists - create customer first
                                            CustomerInfo2 cust = new CustomerInfo2();
                                            cust.No = sResultCustomer.Split(',')[3].Replace("╬", ",").Replace("\"", "");

                                            // NAV restriction for 50 chars max
                                            if (sResultCustomer.Split(',')[1].Replace("╬", ",").Replace("\"", "").Length > 50)
                                            {
                                                cust.Name = sResultCustomer.Split(',')[1].Replace("╬", ",").Replace("\"", "").Substring(0, 50);
                                            }
                                            else
                                            {
                                                cust.Name = sResultCustomer.Split(',')[1].Replace("╬", ",").Replace("\"", "");
                                            }
                                            cust.Address = sResultCustomer.Split(',')[4].Replace("╬", ",").Replace("\"", "");
                                            cust.Post_Code = sResultCustomer.Split(',')[5].Replace("╬", ",").Replace("\"", "");
                                            cust.City = sResultCustomer.Split(',')[6].Replace("╬", ",").Replace("\"", "");
                                            //cust.Country_Region_Code = sResultCustomer.Split(',')[7].Replace("╬", ",").Replace("\"", "");
                                            cust.Phone_No = sResultCustomer.Split(',')[8].Replace("╬", ",").Replace("\"", "");
                                            cust.E_Mail = sResultCustomer.Split(',')[9].Replace("╬", ",").Replace("\"", "");
                                            cust.Search_Name = sResultCustomer.Split(',')[2].Replace("╬", ",").Replace("\"", "");

                                            string strFullName = sResultCustomer.Split(',')[11].Replace("╬", ",").Replace("\"", "");
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
                                }

                                // searh if this customer already exists
                                if ((DoesCustomerExists(nav, filterArray) == true) || (bCustomerCreatedInNav == true) || (bProblematicCustomer == true))
                                {
                                    // iterate all invoices
                                    foreach (string sResultInvoices in sResultInvoicesArray)
                                    {
                                        if ((sResultInvoices != "") && (sResultInvoices != "Not found."))
                                        {
                                            if (sResultInvoices.Split(',')[9] == sResultCustomer.Split(',')[0])
                                            {
                                                if (bProblematicCustomer == true)
                                                {
                                                    sProblematicCustomers += sResultCustomer + "<br />";
                                                    break;
                                                }

                                                // search if invoice already processed
                                                if (IsOrderAlreadyProcessed(sResultInvoices.Split(',')[0].Replace("╬", ",").Replace("\"", ""), dbConn) == false)
                                                {
                                                    // get checkbox state
                                                    string sInvoiceChosen = "n/a";
                                                    try
                                                    {
                                                        sInvoiceChosen = FormPageVars["inv_" + sResultCustomer.Split(',')[2].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "_" + sResultInvoices.Split(',')[0].Replace("\"", "").Replace("╬", ",").Replace(";", " ")];
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                        sInvoiceChosen = "n/a";
                                                    }
                                                    if (sInvoiceChosen == null) sInvoiceChosen = "n/a";
                                                    if (sInvoiceChosen == "") sInvoiceChosen = "n/a";

                                                    // import invoice only if it is selected by checkbox
                                                    if (sInvoiceChosen == "TL_SELECTED_INVOICE")
                                                    {
                                                        // get invoice lines count
                                                        string[] sResultInvoiceLinesArray = stRPNAVConnectWS.sResultInvoiceLines.Replace("<br /><br />", "≡").Split('≡');
                                                        int iInvoiceLinesNumber = 0;
                                                        foreach (string sResultInvoiceLine in sResultInvoiceLinesArray)
                                                        {
                                                            if (sResultInvoiceLine != "")
                                                            {
                                                                if (sResultInvoiceLine.Split(',')[1] == sResultInvoices.Split(',')[0])
                                                                {
                                                                    iInvoiceLinesNumber++;
                                                                }
                                                            }
                                                        }

                                                        // create new invoice
                                                        if (iInvoiceLinesNumber == 0)
                                                        {
                                                            if (AllowInvoicesWithoutLinesCB.Checked == true)
                                                            {
                                                                // create order first and create empty order lines
                                                                SalesInvoice_Service order = new SalesInvoice_Service();
                                                                sal.Create(ref order);

                                                                // invoice data
                                                                order.Sell_to_Customer_No = sResultCustomer.Split(',')[3].Replace("╬", ",").Replace("\"", "");

                                                                // date YYYY-MM-DD
                                                                string sTLInvoiceDate = sResultInvoices.Split(',')[4].Replace("╬", ",").Replace("\"", "");
                                                                if (sTLInvoiceDate.Length >= 10)
                                                                {
                                                                    string sYYYY = sTLInvoiceDate.Substring(0, 4);
                                                                    string sMM = sTLInvoiceDate.Substring(5, 2);
                                                                    string sDD = sTLInvoiceDate.Substring(8, 2);
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
                                                                sSql += "VALUES ('" + sResultInvoices.Split(',')[0].Replace("╬", ",").Replace("\"", "").Replace("'", "\"") + "', 'Pushed', 'TimeLog', '" + sCurrentDateTime + "', 'No Invoice Lines')";
                                                                string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                                                                if (sDBResult != "DBOK")
                                                                {
                                                                    sResultMessage += sDBResult + "<br />";
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // create order first and create empty order lines
                                                            SalesInvoice_Service order = new SalesInvoice_Service();
                                                            sal.Create(ref order);

                                                            // invoice data
                                                            order.Sell_to_Customer_No = sResultCustomer.Split(',')[3].Replace("╬", ",").Replace("\"", "");

                                                            // date YYYY-MM-DD
                                                            string sTLInvoiceDate = sResultInvoices.Split(',')[4].Replace("╬", ",").Replace("\"", "");
                                                            if (sTLInvoiceDate.Length >= 10)
                                                            {
                                                                string sYYYY = sTLInvoiceDate.Substring(0, 4);
                                                                string sMM = sTLInvoiceDate.Substring(5, 2);
                                                                string sDD = sTLInvoiceDate.Substring(8, 2);
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

                                                            foreach (string sResultInvoiceLine in sResultInvoiceLinesArray)
                                                            {
                                                                if ((sResultInvoiceLine != "") && (sResultInvoiceLine != "Not found."))
                                                                {
                                                                    if (sResultInvoices.Split(',')[0] == sResultInvoiceLine.Split(',')[1])
                                                                    {
                                                                        Sales_Invoice_Line invoiceLine = new Sales_Invoice_Line();

                                                                        // item
                                                                        invoiceLine.Type = NAVOrdersWS.Type.Item;

                                                                        // type
                                                                        invoiceLine.No = "100";
                                                                        if (sResultInvoiceLine.Split(',')[4].Replace("╬", ",").Replace("\"", "").ToLower().IndexOf("kørsel") != -1)
                                                                        {
                                                                            invoiceLine.No = "500";
                                                                        }

                                                                        string sProductNo = "";
                                                                        try
                                                                        {
                                                                            sProductNo = sResultInvoiceLine.Split(',')[16].Replace("╬", ",").Replace("\"", "").ToLower();
                                                                            if (sProductNo == "600") invoiceLine.No = "600";
                                                                            if (sProductNo == "605") invoiceLine.No = "605";
                                                                            if (sProductNo == "610") invoiceLine.No = "610";
                                                                            if (sProductNo == "700") invoiceLine.No = "700";
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            ex.ToString();
                                                                        }

                                                                        /*
                                                                        try
                                                                        {
                                                                            NAVSalesCRMemoWS.SalesCRMemo_Service_Service ml = new SalesCRMemo_Service_Service();
                                                                            NAVSalesCRMemoWS.SalesCRMemo_Service mlcn = new SalesCRMemo_Service();
                                                                            //mlcn.sales.No = "";
                                                                            ml.Create(ref mlcn);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            ex.ToString();
                                                                        }
                                                                        */

                                                                        // special case
                                                                        string sProjectName = sResultInvoiceLine.Split(',')[14].Replace("╬", ",").Replace("\"", "");
                                                                        /*
                                                                        if (sProjectName.ToLower().IndexOf("#2016") != -1)
                                                                        {
                                                                            invoiceLine.No = "600";
                                                                        }
                                                                        if (sProjectName.ToLower().IndexOf("#2017") != -1)
                                                                        {
                                                                            invoiceLine.No = "600";
                                                                        }
                                                                        if (sProjectName.ToLower().IndexOf("serviceaftale") != -1)
                                                                        {
                                                                            invoiceLine.No = "600";
                                                                        }
                                                                        */

                                                                        // quantity and price
                                                                        try
                                                                        {
                                                                            invoiceLine.Quantity = Convert.ToDecimal(sResultInvoiceLine.Split(',')[5].Replace("╬", ",").Replace("\"", ""));
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            ex.ToString();
                                                                        }
                                                                        try
                                                                        {
                                                                            invoiceLine.Unit_Price = Convert.ToDecimal(sResultInvoiceLine.Split(',')[6].Replace("╬", ",").Replace("\"", ""));
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
                                                                        string sOrderLineDate = sResultInvoiceLine.Split(',')[3].Replace("╬", ",").Replace("\"", "").Substring(8, 2) + "-";
                                                                        sOrderLineDate += sResultInvoiceLine.Split(',')[3].Replace("╬", ",").Replace("\"", "").Substring(5, 2) + "-";
                                                                        sOrderLineDate += sResultInvoiceLine.Split(',')[3].Replace("╬", ",").Replace("\"", "").Substring(0, 4);
                                                                        string sLineDescription = "(" + sOrderLineDate + ") " + sResultInvoiceLine.Split(',')[4].Replace("╬", ",").Replace("\"", "");

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
                                                                            extraLine.Description = sProjectName;
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
                                                                                    extraLine.Description = sProjectName;
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
                                                                sSql += "VALUES ('" + sResultInvoices.Split(',')[0].Replace("╬", ",").Replace("\"", "").Replace("'", "\"") + "', 'Pushed', 'TimeLog', '" + sCurrentDateTime + "', '" + iInvoiceLinesCount.ToString() + " invoice lines')";
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
                                                                sSql += "VALUES ('" + sResultInvoices.Split(',')[0].Replace("╬", ",").Replace("\"", "").Replace("'", "\"") + "', 'Pushed', 'TimeLog', '" + sCurrentDateTime + "', 'No invoice lines')";
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

            // show message
            PushingDataL.Text = "Total " + iInvoiceNumber.ToString() + " fakturarer herunder " + iInvoiceAllLinesCount.ToString() + " linjer er skubbet til Dynamics NAV.</br ><br />";
            if (sResultMessage != "")
            {
                PushingDataL.Text += "Additional message:<br />";
                PushingDataL.Text += sResultMessage + "<br />";
            }
            if (sProblematicCustomers != "")
            {
                PushingDataL.Text += "Kunder som ikkke har et korrekt CVR nummer registreret i TimeLog<br />";
                PushingDataL.Text += sProblematicCustomers + "<br />";
            }
            if (sMissedCustomers != "")
            {
                PushingDataL.Text += "Kunder som blev oprettet i Dynamics NAV:<br />";
                PushingDataL.Text += sMissedCustomers;
            }
            PushingDataL.Text += "<p style='height:15px;'>&nbsp;</p>";

            PPSep1.Visible = false;
            PPSep2.Visible = false;

            AllowInvoicesWithoutLinesCB.Visible = false;
            PushDataToNavB.Visible = false;

            // clear session
            Session.Clear();

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

            string strSqlQuery = "SELECT l.[id] FROM [RPNAVConnect].[dbo].[Log] as l WHERE l.[refid] = " + sInvoiceTimeLogId;
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
                //PushingDataLErrorData.Text += filter[0].Criteria + " ::: " + ex.ToString();
            }

            return bResult;
        }

        protected void DeleteMarkedInvoicesB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            NameValueCollection FormPageVars;
            FormPageVars = Request.Form;

            string sResultMessage = "";

            try
            {
                stRPNAVConnectWS = (TimeLogDataWS.strRPNAVConnectWS)Session["stRPNAVConnectWS"];
            }
            catch (Exception ex)
            {
                sResultMessage = ex.ToString();
                stRPNAVConnectWS = null;
            }

            if (stRPNAVConnectWS != null)
            {
                try
                {
                    if (stRPNAVConnectWS.sResultDesc == "Ok")
                    {
                        // get customers and invoices
                        string[] sResultCustomerArray = stRPNAVConnectWS.sResultCustomers.Replace("<br />", "≡").Split('≡');
                        string[] sResultInvoicesArray = stRPNAVConnectWS.sResultInvoices.Replace("<br /><br />", "≡").Split('≡');

                        foreach (string sResultCustomer in sResultCustomerArray)
                        {
                            if (sResultCustomer != "")
                            {
                                // iterate all invoices
                                foreach (string sResultInvoices in sResultInvoicesArray)
                                {
                                    if ((sResultInvoices != "") && (sResultInvoices != "Not found."))
                                    {
                                        if (sResultInvoices.Split(',')[9] == sResultCustomer.Split(',')[0])
                                        {
                                            string sInvoiceChosen = "n/a";
                                            try
                                            {
                                                sInvoiceChosen = FormPageVars["inv_" + sResultCustomer.Split(',')[2].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "_" + sResultInvoices.Split(',')[0].Replace("\"", "").Replace("╬", ",").Replace(";", " ")];
                                            }
                                            catch (Exception ex)
                                            {
                                                sResultMessage += "<br />" + ex.ToString();
                                                sInvoiceChosen = "n/a";
                                            }
                                            if (sInvoiceChosen == null) sInvoiceChosen = "n/a";
                                            if (sInvoiceChosen == "") sInvoiceChosen = "n/a";

                                            if (sInvoiceChosen == "TL_SELECTED_INVOICE")
                                            {
                                                sResultMessage += "Invoice:" + sResultInvoices.Split(',')[0].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + " [Customer:" + sResultCustomer.Split(',')[2].Replace("\"", "").Replace("╬", ",").Replace(";", " ") + "]<br />";
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
                    PushingDataL.Text += "<br />" + sResultMessage;
                }

                PushingDataL.Text = "<b>Selected invoices for deletion:</b><br />" + sResultMessage;
            }

            // update checkboxes and scroll down
            ClientScript.RegisterStartupScript(GetType(), "ScrollScript", "window.onload = function() { document.getElementById('lastscriptdiv').scrollIntoView(true);}", true);
        }
    }
}