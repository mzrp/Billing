using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace RPNAVConnect
{

    using RPNAVConnect.TimelogReport;

    /// <summary>
    /// Summary description for RPNAVConnectWS
    /// </summary>
    [WebService(Namespace = "https://nav.gowingu.net:8091/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class RPNAVConnectWS : System.Web.Services.WebService
    {

        public string SiteCode = "da10d62d40024fbfb960d378d";
        public string ApiUser = "rpconsult";
        public string ApiPw = "1234";

        public int iCustomersCount = 0;
        public int iInvoicesCount = 0;

        public struct strRPNAVConnectWS
        {
            public string sResultDesc;
            public string sResultCustomers;
            public string sResultInvoices;
            public string sResultInvoiceLines;
        }

        private string[] GetAllLines(string sInputLine)
        {
            string sResult = "";

            // sisngle line
            if (sInputLine.Length < 85)
            {
                string[] sResultArr = new string[1];
                sResult = sInputLine;
                sResultArr[0] = sResult;
                return sResultArr;
            }

            try
            {
                if (sInputLine.Length > 85)
                {
                    bool bEnd = false;
                    while (bEnd == false)
                    {
                        string sTmp = sInputLine.Substring(0, 85);
                        char[] chArr = new char[1];
                        chArr[0] = ' ';
                        int iLastSpace = sTmp.LastIndexOfAny(chArr);
                        if (iLastSpace != -1)
                        {
                            sResult += sTmp.Substring(0, iLastSpace) + "$";
                            try
                            {
                                sInputLine = sInputLine.Substring(iLastSpace).TrimStart(' ');
                                if (sInputLine.Length <= 85)
                                {
                                    sResult += sInputLine;
                                    bEnd = true;
                                }
                            }
                            catch
                            {
                                bEnd = true;
                            }
                        }
                        else
                        {
                            sResult += sTmp + "$";
                            try
                            {
                                sInputLine = sInputLine.Substring(85).TrimStart(' ');
                                if (sInputLine.Length <= 85)
                                {
                                    sResult += sInputLine;
                                    bEnd = true;
                                }
                            }
                            catch
                            {
                                bEnd = true;
                            }
                        }
                    }
                }

                if (sResult[sResult.Length - 1] == '$') sResult = sResult.Substring(0, sResult.Length - 1);
                string[] sResultArr = sResult.Split('$');
                return sResultArr;
            }
            catch
            {
                string[] sResultArr = new string[1];
                sResult = sInputLine;
                sResultArr[0] = sResult;
                return sResultArr;
            }
        }

        [WebMethod]
        public strRPNAVConnectWS GetTimeLogData(string sVATNos, string sInvoiceStatus, string sStartMonth, string sStartYear, string sEndMonth, string sEndYear)
        {
            var client = new ServiceSoapClient();

            string sResultCustomers = "";
            string sResultInvoices = "";
            string sResultInvoiceLines = "";
            string sResultDesc = "Ok";

            // get status
            int iInvoiceStatus = 1;
            try
            {
                iInvoiceStatus = Convert.ToInt32(sInvoiceStatus);
            }
            catch (Exception ex)
            {
                ex.ToString();
                iInvoiceStatus = 1;
            }
            // get StartMonth
            int iStartMonth = -1;
            try
            {
                iStartMonth = Convert.ToInt32(sStartMonth);
            }
            catch (Exception ex)
            {
                ex.ToString();
                iStartMonth = -1;
            }
            // get StartYear
            int iStartYear = -1;
            try
            {
                iStartYear = Convert.ToInt32(sStartYear);
            }
            catch (Exception ex)
            {
                ex.ToString();
                iStartYear = -1;
            }
            // get EndMonth
            int iEndMonth = -1;
            try
            {
                iEndMonth = Convert.ToInt32(sEndMonth);
            }
            catch (Exception ex)
            {
                ex.ToString();
                iEndMonth = -1;
            }
            // get EndYear
            int iEndYear = -1;
            try
            {
                iEndYear = Convert.ToInt32(sEndYear);
            }
            catch (Exception ex)
            {
                ex.ToString();
                iEndYear = -1;
            }

            // start date
            bool bStartDate = false;
            DateTime dtStartDate = DateTime.Now;
            if ((iStartMonth != -1) && (iStartYear != -1))
            {
                try
                {
                    dtStartDate = new DateTime(iStartYear, iStartMonth, 1, 0, 0, 0);
                    bStartDate = true;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            // end date
            int iEndDay = 28;
            if ((iEndMonth == 1) || (iEndMonth == 3) || (iEndMonth == 5) || (iEndMonth == 7) || (iEndMonth == 8) || (iEndMonth == 1) || (iEndMonth == 10) || (iEndMonth == 12))
            {
                iEndDay = 31;
            }
            if ((iEndMonth == 4) || (iEndMonth == 6) || (iEndMonth == 9) || (iEndMonth == 11))
            {
                iEndDay = 30;
            }
            if (iEndMonth == 2)
            {
                iEndDay = 28;
                if (DateTime.IsLeapYear(iEndYear) == true)
                {
                    iEndDay = 29;
                }
            }
            if ((iEndYear == 2016) && (iEndDay == 28)) iEndDay = 29;
            if ((iEndYear == 2020) && (iEndDay == 28)) iEndDay = 29;
            if ((iEndYear == 2024) && (iEndDay == 28)) iEndDay = 29;
            if ((iEndYear == 2028) && (iEndDay == 28)) iEndDay = 29;

            bool bEndDate = false;
            DateTime dtEndDate = DateTime.Now;
            if ((iEndMonth != -1) && (iEndYear != -1))
            {
                try
                {
                    dtEndDate = new DateTime(iEndYear, iEndMonth, iEndDay, 23, 59, 59);
                    bEndDate = true;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            // get all customers
            System.Xml.XmlNode xmlNode = client.GetCustomersRaw(SiteCode, ApiUser, ApiPw, -1, -1, 0);
            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.LoadXml(xmlNode.OuterXml);
            try
            {
                System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("tlp", "http://www.timelog.com/XML/Schema/tlp/v4_4");
                System.Xml.XmlNodeList xnList = xml.SelectNodes("//tlp:Customer", nsmgr);

                iCustomersCount = 0;
                iInvoicesCount = 0;

                foreach (System.Xml.XmlNode xn in xnList)
                {
                    System.Xml.XmlAttribute attCol = xn.Attributes["ID"];

                    string sID = "n/a";
                    try { sID = attCol.Value; }
                    catch { }
                    string sName = "n/a";
                    try { sName = xn["tlp:Name"].InnerText; }
                    catch { }
                    string sNo = "n/a";
                    try { sNo = xn["tlp:No"].InnerText; }
                    catch { }
                    string sVATNo = "n/a";
                    try { sVATNo = xn["tlp:VATNo"].InnerText; }
                    catch { }
                    string sAddress1 = "n/a";
                    try { sAddress1 = xn["tlp:Address1"].InnerText; }
                    catch { }
                    string sZipCode = "n/a";
                    try { sZipCode = xn["tlp:ZipCode"].InnerText; }
                    catch { }
                    string sCity = "n/a";
                    try { sCity = xn["tlp:City"].InnerText; }
                    catch { }
                    string sCountry = "n/a";
                    try { sCountry = xn["tlp:Country"].InnerText; }
                    catch { }
                    string sPhone = "n/a";
                    try { sPhone = xn["tlp:Phone"].InnerText; }
                    catch { }
                    string sEmail = "n/a";
                    try { sEmail = xn["tlp:Email"].InnerText; }
                    catch { }
                    string sCustomerStatus = "n/a";
                    try { sCustomerStatus = xn["tlp:CustomerStatus"].InnerText; }
                    catch { }

                    string sAccountManagerID = "n/a";
                    try { sAccountManagerID = xn["tlp:AccountManagerID"].InnerText; }
                    catch { }
                    string sAccountManagerFullName = "n/a";
                    try { sAccountManagerFullName = xn["tlp:AccountManagerFullName"].InnerText; }
                    catch { }

                    iCustomersCount++;

                    int iCustomerID = -1;
                    try
                    {
                        iCustomerID = Convert.ToInt32(sID);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        iCustomerID = -1;
                    }

                    bool bProcessCustomer = false;
                    if (sVATNos == "n/a")
                    {
                        bProcessCustomer = true;
                    }
                    else
                    {
                        string[] sVATNosArray = sVATNos.Split(',');
                        foreach (string sVATNoSingle in sVATNosArray)
                        {
                            if (sVATNoSingle == sVATNo)
                            {
                                bProcessCustomer = true;
                            }
                        }
                    }

                    if (sCustomerStatus == "04. Kunde")
                    {
                        if (iCustomerID != -1)
                        {
                            if (bProcessCustomer == true)
                            {

                                sResultCustomers += "\"" + sID.Replace(",", "╬") + "\",\"" + sName.Replace(",", "╬") + "\",\"" + sNo.Replace(",", "╬") + "\",\"" + sVATNo.Replace(",", "╬") + "\",\"" + sAddress1.Replace(",", "╬") + "\",\"" + sZipCode.Replace(",", "╬") + "\",\"" + sCity.Replace(",", "╬") + "\",\"" + sCountry.Replace(",", "╬") + "\",\"" + sPhone.Replace(",", "╬") + "\",\"" + sEmail.Replace(",", "╬") + "\",\"" + sAccountManagerID.Replace(",", "╬") + "\",\"" + sAccountManagerFullName.Replace(",", "╬") + "\"<br />";

                                ///*

                                // get all invoices for the customer
                                System.Xml.XmlNode xmlNodeInvDetails = null;

                                try
                                {
                                    // get invoices
                                    if ((bStartDate == true) && (bEndDate == true))
                                    {
                                        xmlNodeInvDetails = client.GetInvoicesRaw(SiteCode, ApiUser, ApiPw, 0, iCustomerID, iInvoiceStatus, (DateTime?)dtStartDate, (DateTime?)dtEndDate);
                                    }
                                    else
                                    {
                                        xmlNodeInvDetails = client.GetInvoicesRaw(SiteCode, ApiUser, ApiPw, 0, iCustomerID, iInvoiceStatus, null, null);
                                    }

                                    // process invoice line details
                                    System.Xml.XmlDocument xmlInvDetails = new System.Xml.XmlDocument();
                                    xmlInvDetails.LoadXml(xmlNodeInvDetails.OuterXml);
                                    try
                                    {
                                        System.Xml.XmlNamespaceManager nsmgrInvDetails = new System.Xml.XmlNamespaceManager(xmlInvDetails.NameTable);
                                        nsmgrInvDetails.AddNamespace("tlp", "http://www.timelog.com/XML/Schema/tlp/v5_0");
                                        System.Xml.XmlNodeList xnListInvDetails = xmlInvDetails.SelectNodes("//tlp:Invoice", nsmgrInvDetails);

                                        if (xnListInvDetails.Count > 0)
                                        {

                                            foreach (System.Xml.XmlNode xnInvDetails in xnListInvDetails)
                                            {

                                                System.Xml.XmlAttribute attColInvDetails = xnInvDetails.Attributes["ID"];
                                                string sInvoiceIDInv = attColInvDetails.Value;

                                                string sInvoiceNo = "n/a";
                                                try
                                                {
                                                    sInvoiceNo = xnInvDetails["tlp:InvoiceNo"].InnerText;
                                                }
                                                catch (Exception invex)
                                                {
                                                    invex.ToString();
                                                    sInvoiceNo = "n/a";
                                                }
                                                if (sInvoiceNo.Trim() == "") sInvoiceNo = "n/a";

                                                string sHeader = "n/a";
                                                try { sHeader = xnInvDetails["tlp:Header"].InnerText.Replace("\"", "'"); }
                                                catch { }
                                                string sTextInv = "n/a";
                                                try { sTextInv = xnInvDetails["tlp:Text"].InnerText.Replace("\"", "'"); }
                                                catch { }
                                                string sInvoiceDate = "n/a";
                                                try { sInvoiceDate = xnInvDetails["tlp:InvoiceDate"].InnerText; }
                                                catch { }
                                                string sDueDate = "n/a";
                                                try { sDueDate = xnInvDetails["tlp:DueDate"].InnerText; }
                                                catch { }
                                                string sAmountInv = "n/a";
                                                try { sAmountInv = xnInvDetails["tlp:Amount"].InnerText; }
                                                catch { }
                                                string sStatus = "n/a";
                                                try { sStatus = xnInvDetails["tlp:Status"].InnerText; }
                                                catch { }
                                                string sType = "n/a";
                                                try { sType = xnInvDetails["tlp:Type"].InnerText; }
                                                catch { }
                                                string sCustomerID = "n/a";
                                                try { sCustomerID = xnInvDetails["tlp:CustomerID"].InnerText; }
                                                catch { }
                                                string sCustomerName = "n/a";
                                                try { sCustomerName = xnInvDetails["tlp:CustomerName"].InnerText.Replace("\"", "'"); }
                                                catch { }
                                                string sCustomerNo = "n/a";
                                                try { sCustomerNo = xnInvDetails["tlp:CustomerNo"].InnerText; }
                                                catch { }
                                                string sCustomerAddress1 = "n/a";
                                                try { sCustomerAddress1 = xnInvDetails["tlp:CustomerAddress1"].InnerText.Replace("\"", "'"); }
                                                catch { }
                                                string sCustomerZipCode = "n/a";
                                                try { sCustomerZipCode = xnInvDetails["tlp:CustomerZipCode"].InnerText; }
                                                catch { }
                                                string sCustomerCity = "n/a";
                                                try { sCustomerCity = xnInvDetails["tlp:CustomerCity"].InnerText; }
                                                catch { }
                                                string sCustomerState = "n/a";
                                                try { sCustomerState = xnInvDetails["tlp:CustomerState"].InnerText; }
                                                catch { }
                                                string sCustomerCountry = "n/a";
                                                try { sCustomerCountry = xnInvDetails["tlp:CustomerCountry"].InnerText; }
                                                catch { }
                                                string sPaymentTermID = "n/a";
                                                try { sPaymentTermID = xnInvDetails["tlp:PaymentTermID"].InnerText; }
                                                catch { }
                                                string sPaymentTermText = "n/a";
                                                try { sPaymentTermText = xnInvDetails["tlp:PaymentTermText"].InnerText.Replace("\"", "'"); }
                                                catch { }
                                                string sCurrencyAbb = "n/a";
                                                try { sCurrencyAbb = xnInvDetails["tlp:CurrencyAbb"].InnerText; }
                                                catch { }
                                                string sCurrencyRate = "n/a";
                                                try { sCurrencyRate = xnInvDetails["tlp:CurrencyRate"].InnerText; }
                                                catch { }
                                                string sVATInv = "n/a";
                                                try { sVATInv = xnInvDetails["tlp:DefaultVAT"].InnerText; }
                                                catch { }
                                                string sAddVAT = "n/a";
                                                try { sAddVAT = xnInvDetails["tlp:AddVAT"].InnerText; }
                                                catch { }
                                                string sNetAmount = "n/a";
                                                try { sNetAmount = xnInvDetails["tlp:NetAmount"].InnerText; }
                                                catch { }
                                                string sNetAmountSystemCurrency = "n/a";
                                                try { sNetAmountSystemCurrency = xnInvDetails["tlp:NetAmountSystemCurrency"].InnerText; }
                                                catch { }
                                                string sVATIncluded = "n/a";
                                                try { sVATIncluded = xnInvDetails["tlp:VATIncluded"].InnerText; }
                                                catch { }
                                                string sVATIncludedSystemCurrency = "n/a";
                                                try { sVATIncludedSystemCurrency = xnInvDetails["tlp:VATIncludedSystemCurrency"].InnerText; }
                                                catch { }
                                                string sProjectNo = "n/a";
                                                try { sProjectNo = xnInvDetails["tlp:ProjectNo"].InnerText; }
                                                catch { }
                                                string sPurchaseNo = "n/a";
                                                try { sPurchaseNo = xnInvDetails["tlp:PurchaseNo"].InnerText; }
                                                catch { }
                                                string sContactFullName = "n/a";
                                                try { sContactFullName = xnInvDetails["tlp:ContactFullName"].InnerText; }
                                                catch { }

                                                if (((sType == "1") || (sType == "2")) && (sInvoiceNo != "n/a"))
                                                {
                                                    sResultInvoices += "\"" + sInvoiceIDInv.Replace(",", "╬") + "\",\"" + sInvoiceNo.Replace(",", "╬") + "\",\"" + sHeader.Replace(",", "╬") + "\",\"" +
                                                        sTextInv.Replace(",", "╬") + "\",\"" + sInvoiceDate.Replace(",", "╬") + "\",\"" + sDueDate.Replace(",", "╬") + "\",\"" +
                                                        sAmountInv.Replace(",", "╬") + "\",\"" + sStatus.Replace(",", "╬") + "\",\"" + sType.Replace(",", "╬") + "\",\"" +
                                                        sCustomerID.Replace(",", "╬") + "\",\"" + sCustomerName.Replace(",", "╬") + "\",\"" + sCustomerNo.Replace(",", "╬") + "\",\"" +
                                                        sCustomerAddress1.Replace(",", "╬") + "\",\"" + sCustomerZipCode.Replace(",", "╬") + "\",\"" + sCustomerCity.Replace(",", "╬") + "\",\"" +
                                                        sCustomerState.Replace(",", "╬") + "\",\"" + sCustomerCountry.Replace(",", "╬") + "\",\"" + sPaymentTermID.Replace(",", "╬") + "\",\"" +
                                                        sPaymentTermText.Replace(",", "╬") + "\",\"" + sCurrencyAbb.Replace(",", "╬") + "\",\"" + sCurrencyRate.Replace(",", "╬") + "\",\"" +
                                                        sVATInv.Replace(",", "╬") + "\",\"" + sAddVAT.Replace(",", "╬") + "\",\"" + sNetAmount.Replace(",", "╬") + "\",\"" +
                                                        sNetAmountSystemCurrency.Replace(",", "╬") + "\",\"" + sVATIncluded.Replace(",", "╬") + "\",\"" + sVATIncludedSystemCurrency.Replace(",", "╬") + "\",\"" +
                                                        sProjectNo.Replace(",", "╬") + "\",\"" + sPurchaseNo.Replace(",", "╬") + "\",\"" + sContactFullName.Replace(",", "╬") + "\"" + "<br /><br />";

                                                    iInvoicesCount++;

                                                    int iInvoiceIDInv = -1;
                                                    try
                                                    {
                                                        iInvoiceIDInv = Convert.ToInt32(sInvoiceIDInv);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                        iInvoiceIDInv = -1;
                                                    }

                                                    if (iInvoiceIDInv != -1)
                                                    {
                                                        // get all invoice lines
                                                        System.Xml.XmlNode xmlNodeInv = null;

                                                        try
                                                        {
                                                            // get invoice lines
                                                            if ((bStartDate == true) && (bEndDate == true))
                                                            {
                                                                xmlNodeInv = client.GetInvoiceLinesRaw(SiteCode, ApiUser, ApiPw, 0, iCustomerID, 0, iInvoiceIDInv, (DateTime?)dtStartDate, (DateTime?)dtEndDate);
                                                            }
                                                            else
                                                            {
                                                                xmlNodeInv = client.GetInvoiceLinesRaw(SiteCode, ApiUser, ApiPw, 0, iCustomerID, 0, iInvoiceIDInv, null, null);
                                                            }

                                                            // process invoice lines
                                                            System.Xml.XmlDocument xmlInv = new System.Xml.XmlDocument();
                                                            xmlInv.LoadXml(xmlNodeInv.OuterXml);
                                                            try
                                                            {
                                                                System.Xml.XmlNamespaceManager nsmgrInv = new System.Xml.XmlNamespaceManager(xmlInv.NameTable);
                                                                nsmgrInv.AddNamespace("tlp", "http://www.timelog.com/XML/Schema/tlp/v5_0");
                                                                System.Xml.XmlNodeList xnListInv = xmlInv.SelectNodes("//tlp:InvoiceLine", nsmgrInv);
                                                                foreach (System.Xml.XmlNode xnInv in xnListInv)
                                                                {
                                                                    System.Xml.XmlAttribute attColInv = xnInv.Attributes["ID"];
                                                                    string sInvLineID = "n/a";
                                                                    try { sInvLineID = attColInv.Value; }
                                                                    catch { }
                                                                    string sInvoiceID = "n/a";
                                                                    try { sInvoiceID = xnInv["tlp:InvoiceID"].InnerText; }
                                                                    catch { }
                                                                    string sInvoiceLineNo = "n/a";
                                                                    try { sInvoiceLineNo = xnInv["tlp:InvoiceNo"].InnerText; }
                                                                    catch { }
                                                                    string sDate = "n/a";
                                                                    try { sDate = xnInv["tlp:Date"].InnerText; }
                                                                    catch { }
                                                                    string sText = "n/a";
                                                                    try { sText = xnInv["tlp:Text"].InnerText.Replace("\"", "'"); }
                                                                    catch { }
                                                                    string sQuantity = "n/a";
                                                                    try { sQuantity = xnInv["tlp:Quantity"].InnerText; }
                                                                    catch { }
                                                                    string sRate = "n/a";
                                                                    try { sRate = xnInv["tlp:Rate"].InnerText; }
                                                                    catch { }
                                                                    string sRateSystemCurrency = "n/a";
                                                                    try { sRateSystemCurrency = xnInv["tlp:RateSystemCurrency"].InnerText; }
                                                                    catch { }
                                                                    string sAmount = "n/a";
                                                                    try { sAmount = xnInv["tlp:Amount"].InnerText; }
                                                                    catch { }
                                                                    string sAmountSystemCurrency = "n/a";
                                                                    try { sAmountSystemCurrency = xnInv["tlp:AmountSystemCurrency"].InnerText; }
                                                                    catch { }

                                                                    string sProjectID = "n/a";
                                                                    string sProjectName = "n/a";
                                                                    string sProjectTypeID = "n/a";
                                                                    string sProjectTypeName = "n/a";
                                                                    try
                                                                    {
                                                                        sProjectID = xnInv["tlp:ProjectID"].InnerText;
                                                                        int iProjectId = -1;

                                                                        try
                                                                        {
                                                                            iProjectId = Convert.ToInt32(sProjectID);
                                                                        }
                                                                        catch (Exception exP)
                                                                        {
                                                                            exP.ToString();
                                                                            iProjectId = -1;
                                                                        }

                                                                        if (iProjectId != -1)
                                                                        {
                                                                            // get project name
                                                                            System.Xml.XmlNode xmlNodeProject = null;
                                                                            xmlNodeProject = client.GetProjectsRaw(SiteCode, ApiUser, ApiPw, iProjectId, -2, iCustomerID, 0);

                                                                            System.Xml.XmlDocument xmlInvP = new System.Xml.XmlDocument();
                                                                            xmlInvP.LoadXml(xmlNodeProject.OuterXml);
                                                                            try
                                                                            {
                                                                                System.Xml.XmlNamespaceManager nsmgrInvP = new System.Xml.XmlNamespaceManager(xmlInvP.NameTable);
                                                                                nsmgrInvP.AddNamespace("tlp", "http://www.timelog.com/XML/Schema/tlp/v4_4");
                                                                                System.Xml.XmlNodeList xnListInvP = xmlInvP.SelectNodes("//tlp:Project", nsmgrInvP);
                                                                                foreach (System.Xml.XmlNode xnInvP in xnListInvP)
                                                                                {
                                                                                    System.Xml.XmlAttribute attColInvP = xnInvP.Attributes["ID"];
                                                                                    string sInvProjectID = "n/a";
                                                                                    try { sInvProjectID = attColInvP.Value; }
                                                                                    catch { }
                                                                                    try { sProjectName = xnInvP["tlp:Name"].InnerText; }
                                                                                    catch { }
                                                                                    sProjectTypeID = "n/a";
                                                                                    try { sProjectTypeID = xnInvP["tlp:ProjectTypeID"].InnerText; }
                                                                                    catch { }
                                                                                    sProjectTypeName = "n/a";
                                                                                    try { sProjectTypeName = xnInvP["tlp:ProjectTypeName"].InnerText; }
                                                                                    catch { }
                                                                                    break;
                                                                                }
                                                                            }
                                                                            catch (Exception exP2)
                                                                            {
                                                                                exP2.ToString();
                                                                                sProjectName = sProjectName + exP2.ToString();
                                                                            }

                                                                        }
                                                                    }
                                                                    catch (Exception pidex)
                                                                    {
                                                                        pidex.ToString();
                                                                        sProjectID = "n/a";
                                                                        sProjectName = "n/a6";
                                                                    }

                                                                    string sVAT = "n/a";
                                                                    try { sVAT = xnInv["tlp:VAT"].InnerText; }
                                                                    catch { }
                                                                    string sDiscount = "n/a";
                                                                    try { sDiscount = xnInv["tlp:Discount"].InnerText; }
                                                                    catch { }
                                                                    string sUnitType = "n/a";
                                                                    try { sUnitType = xnInv["tlp:UnitType"].InnerText; }
                                                                    catch { }

                                                                    // invoice lines
                                                                    sResultInvoiceLines += "\"" + sInvLineID.Replace(",", "╬") + "\",\"" + sInvoiceID.Replace(",", "╬") + "\",\"" + sInvoiceNo.Replace(",", "╬") + "\",\"" + sDate.Replace(",", "╬") + "\",\"" + sText.Replace(",", "╬") + "\",\"" + sQuantity.Replace(",", "╬") + "\",\"" + sRate.Replace(",", "╬") + "\",\"" + sRateSystemCurrency.Replace(",", "╬") + "\",\"" + "Amount: " + sAmount.Replace(",", "╬") + "\",\"" + sAmountSystemCurrency.Replace(",", "╬") + "\",\"" + sProjectID.Replace(",", "╬") + "\",\"" + sVAT.Replace(",", "╬") + "\",\"" + sDiscount.Replace(",", "╬") + "\",\"" + sUnitType.Replace(",", "╬") + "\",\"" + sProjectName.Replace(",", "╬") + "\",\"" + sInvoiceLineNo.Replace(",", "╬") + "\",\"" + sProjectTypeID.Replace(",", "╬") + "\",\""+ sProjectTypeName.Replace(",", "╬") + "\"" + " <br /><br />";
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                string sErr = ex.ToString();
                                                            }

                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            ex.ToString();
                                                        }
                                                    }

                                                }

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        sResultDesc = ex.ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    sResultDesc = ex.ToString();
                                }

                                //*/
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sResultDesc = ex.ToString();
            }

            if (sResultInvoices == "") sResultInvoices = "Not found.";
            if (sResultInvoiceLines == "") sResultInvoiceLines = "Not found.";

            strRPNAVConnectWS stRPNAVConnectWS = new strRPNAVConnectWS();
            stRPNAVConnectWS.sResultDesc = sResultDesc;
            stRPNAVConnectWS.sResultCustomers = sResultCustomers;
            stRPNAVConnectWS.sResultInvoices = sResultInvoices;
            stRPNAVConnectWS.sResultInvoiceLines = sResultInvoiceLines;

            return stRPNAVConnectWS;
        }
    }
}
