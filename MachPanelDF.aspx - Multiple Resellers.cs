using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
    public partial class MachPanelDF : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        protected void MachPanelDataB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            MachPanelDataL.Text = "";

            // start date
            DateTime dtStartDate = DateTime.Now;
            bool bStartDate = true;
            try
            {
                // dd-mm-yyyy
                int iDay = Convert.ToInt32(StartDateTB.Text.Substring(0, 2));
                int iMonth = Convert.ToInt32(StartDateTB.Text.Substring(3, 2));
                int iYear = Convert.ToInt32(StartDateTB.Text.Substring(6, 4));
                dtStartDate = new DateTime(iYear, iMonth, iDay, 0, 0, 0);
                bStartDate = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                bStartDate = false;
            }

            // end date
            DateTime dtEndDate = DateTime.Now;
            bool bEndDate = true;
            try
            {
                // dd-mm-yyyy
                int iDay = Convert.ToInt32(EndDateTB.Text.Substring(0, 2));
                int iMonth = Convert.ToInt32(EndDateTB.Text.Substring(3, 2));
                int iYear = Convert.ToInt32(EndDateTB.Text.Substring(6, 4));
                dtEndDate = new DateTime(iYear, iMonth, iDay, 23, 59, 59);
                bEndDate = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                bEndDate = false;
            }

            MachPanelWebService.MPAuthenticationHeader header = new MachPanelWebService.MPAuthenticationHeader();

            //header.UserName = "provider@gowingu.net";
            //header.UserPassword = "h6ECyLzvzQ&3sm";
			
            //header.UserName = "mz@rackpeople.dk";
            //header.UserPassword = "Jy2DvRq#";			

            header.UserName = "sales@rackpeople.dk";
            header.UserPassword = "Tu6Y8AH_";            

            header.AuthenticationToken = "";
            header.CompanyId = 87;
            //header.EmployeeId = 0;

            MachPanelWebService.MachPanelService svc = new MachPanelWebService.MachPanelService();
            svc.Url = "https://controlpanel.gowingu.net/webservices/machpanelservice.asmx";
            svc.MPAuthenticationHeaderValue = header;

            string sPrintReport = "";

            string sMsg = "<b>Reseller:</b>";
            MachPanelDataL.Text += sMsg;
            sPrintReport += sMsg + "<br />";

            MachPanelDataL.Text += "<br />";
            MachPanelDataL.Text += "<br />";
            sPrintReport += "<br />";

            MachPanelWebService.ResponseArguments ra = svc.Authenticate();
            //MachPanelWebService.ResponseArguments ra2 = svc.AuthenticateCustomer("sales@rackpeople.dk", "Tu6Y8AH_", true);
            MachPanelWebService.Customer[] allcustomers = svc.GetAllCustomers();

            string sResellersData = "";
            MachPanelWebService.Customer[] allresellers = svc.GetAllResellers();
            foreach (MachPanelWebService.Customer cust in allresellers)
            {
                // Rackpeople as reseller (ID=87)
                if (cust.CustomerId == 87)
                {
                    sMsg = cust.CompanyName + " [VAT:" + cust.VATNumber + "] " + cust.FirstName + " " + cust.LastName + " [ID:" + cust.CustomerId + "]";
                    sResellersData += cust.CompanyName + "ђ" + cust.VATNumber + "ђ" + cust.CustomerId + "ш";
                    MachPanelDataL.Text += sMsg + "<br />";
                    sPrintReport += sMsg + "<br />";
                }
            }

            MachPanelDataL.Text += "<br />";
            sPrintReport += "<br />";

            sMsg = "List of all SKYPE, TEAMS and DNS subscriptions for reseller Rackpeople:";
            MachPanelDataL.Text += "<b>" + sMsg + "</b><br />";
            sPrintReport += sMsg + "<br />";

            MachPanelDataL.Text += "<br />";
            sPrintReport += "<br />";

            // list of all active subscriptions
            string sCompaniesList = "";
            MachPanelWebService.SubscriptionInfo[] subsList = svc.GetAllSubscriptions();
            foreach (MachPanelWebService.SubscriptionInfo si in subsList)
            {
                //if (si.Status == MachPanelWebService.ServiceStatus.Active)
                //{
                bool bSubscriptionGranted = false;
                if (si.PackageName.ToLower().IndexOf("skype") != -1) bSubscriptionGranted = true;
                if (si.PackageName.ToLower().IndexOf("dns") != -1) bSubscriptionGranted = true;
                if (si.PackageName.ToLower().IndexOf("teams") != -1) bSubscriptionGranted = true;

                if (bSubscriptionGranted == true)
                {
                    if (si.CustomerID == 87)
                    {
                        //sMsg = si.PackageName + "; Hosting Id: " + si.HostingID + " >>> Company: " + si.CompanyName + "; Customer: " + si.CustomerFirstName + " " + si.CustomerLastName + " [" + si.CustomerID + "]";
                        sMsg = si.PackageName + "; Hosting Id: " + si.HostingID + ", Status: " + si.Status.ToString();
                        MachPanelDataL.Text += sMsg + "<br />";
                        sPrintReport += sMsg + "<br />";

                        if (sCompaniesList.IndexOf(si.CompanyName + "ђ") == -1)
                        {
                            sCompaniesList += si.CompanyName + "ђ";
                        }
                    }
                }
                //}
            }

            // list all usage report
            bool bAllUsageReportFound = false;
            string sAllUsagePrintReport = "";
            string sAllUsagePrintReportDataHeader = "";
            string sAllUsagePrintReportData = "";
            try
            {
                MachPanelWebService.ReportingCriteria RC = new MachPanelWebService.ReportingCriteria();
                MachPanelWebService.LyncUserUsageReport[] lyncuserUsageReportList = svc.GetLyncUserUsageReport(RC);
                if (lyncuserUsageReportList.Length > 0)
                {
                    string sUsageReportHeader = "ArchivingPolicy,ClientPolicy,ClientVersionPolicy,CompanyId,CompanyName,ConferencingPolicy,CustomerID,CustomerName,CustomerNumber,DateCreated,DialPlan,ExternalAccessPolicy,IsChatEnabled,LocationPolicy,LyncUser,MicrosoftSPLAType,MobilityPolicy,OrganizationName,Owner,PersistentChatPolicy,PhoneNumber,PinPolicy,ResellerId,SoldPackage,TelephonyOption,VoiceMailPolicy,VoicePolicy";
                    sAllUsagePrintReportDataHeader = sUsageReportHeader;
                    sMsg = sUsageReportHeader;
                    //MachPanelDataL.Text += sMsg + "<br />";
                    //sAllUsagePrintReport += sMsg + "<br />";

                    //MachPanelDataL.Text += "<br />";
                    //sAllUsagePrintReport += "<br />";

                    int iCount = 0;
                    int iAllCount = 0;

                    foreach (MachPanelWebService.LyncUserUsageReport lur in lyncuserUsageReportList)
                    {
                        bool bCompanyIncluded = false;
                        string[] sCompaniesListArrayFilter = sCompaniesList.Split('ђ');                        
                        foreach (string sCompany in sCompaniesListArrayFilter)
                        {
                            if (sCompany != "")
                            {
                                string[] sResellersDataArray = sResellersData.Split('ш');
                                foreach (string sResellerData in sResellersDataArray)
                                {
                                    if (sResellerData != "")
                                    {
                                        if (sResellerData.Split('ђ')[0].ToLower() == sCompany.ToLower())
                                        {
                                            string sResellerId = lur.ResellerId.ToString();
                                            string sCompanyId = sResellerData.Split('ђ')[2];
                                            if (sResellerId == sCompanyId)
                                            {
                                                bCompanyIncluded = true;
                                            }

                                            break;
                                        }
                                    }
                                }

                                if (bCompanyIncluded == true)
                                {
                                    break;
                                }
                            }
                        }

                        // date range filter
                        bool bCountItIn = false;
                        
                        // no date range
                        if ((StartDateTB.Text == "") && (EndDateTB.Text == "")) bCountItIn = true;

                        if ((bStartDate == true) && (bEndDate == false))
                        {
                            if (lur.DateCreated >= dtStartDate) bCountItIn = true;
                        }

                        if ((bStartDate == false) && (bEndDate == true))
                        {
                            if (lur.DateCreated <= dtEndDate) bCountItIn = true;
                        }

                        if ((bStartDate == true) && (bEndDate == true))
                        {
                            if ((lur.DateCreated >= dtStartDate) && (lur.DateCreated <= dtEndDate)) bCountItIn = true;
                        }

                        if (bCompanyIncluded == true)
                        {
                            if (bCountItIn == true)
                            {
                                string sUsageReportData = lur.ArchivingPolicy.Replace(",", ";") + "," + lur.ClientPolicy.Replace(",", ";") + "," + lur.ClientVersionPolicy.Replace(",", ";") + "," + lur.CompanyId + "," + lur.CompanyName.Replace(",", ";") + "," + lur.ConferencingPolicy.Replace(",", ";") + "," + lur.CustomerID + "," + lur.CustomerName.Replace(",", ";") + "," + lur.CustomerNumber.Replace(",", ";") + "," + lur.DateCreated + "," + lur.DialPlan.Replace(",", ";") + "," + lur.ExternalAccessPolicy.Replace(",", ";") + "," + lur.IsChatEnabled + "," + lur.LocationPolicy.Replace(",", ";") + "," + lur.LyncUser.Replace(",", ";") + "," + lur.MicrosoftSPLAType.Replace(",", ";") + "," + lur.MobilityPolicy.Replace(",", ";") + "," + lur.OrganizationName.Replace(",", ";") + "," + lur.Owner.Replace(",", ";") + "," + lur.PersistentChatPolicy.Replace(",", ";") + "," + lur.PhoneNumber.Replace(",", ";") + "," + lur.PinPolicy.Replace(",", ";") + "," + lur.ResellerId + "," + lur.SoldPackage.Replace(",", ";") + "," + lur.TelephonyOption.Replace(",", ";") + "," + lur.VoiceMailPolicy.Replace(",", ";") + "," + lur.VoicePolicy.Replace(",", ";");
                                sAllUsagePrintReportData += sUsageReportData.Replace(",", "ђ") + "ш";
                                sMsg = (iCount + 1).ToString() + ". " + sUsageReportData;
                                //MachPanelDataL.Text += sMsg + "<br />";
                                sAllUsagePrintReport += sMsg + "<br />";
                                bAllUsageReportFound = true;
                                iCount++;
                            }
                        }

                        iAllCount++;
                    }

                    sMsg = iAllCount.ToString() + " user reports found, " + iCount.ToString() + " within date period for reseller " + sCompaniesList.Replace("ђ", ", ").Replace(",", " ");
                    MachPanelDataL.Text += "<br /><b>" + sMsg + "</b><br />";
                }
                else
                {
                    bAllUsageReportFound = false;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                bAllUsageReportFound = false;
            }

            if (bAllUsageReportFound == true)
            {
                // show uer usage reports
                //MachPanelDataL.Text += sAllUsagePrintReport;
            }

            string[] sCompaniesListArray = sCompaniesList.Split('ђ');
            foreach (string sCompany in sCompaniesListArray)
            {
                if (sCompany != "")
                {
                    //MachPanelDataL.Text += "<br />";
                    //sPrintReport += "<br />";

                    //sMsg = "User Usage Report for reseller " + sCompany;
                    //MachPanelDataL.Text += "<b>" + sMsg + "</b><br />";
                    //sPrintReport += sMsg + "<br />";

                    //MachPanelDataL.Text += "<br />";
                    //sPrintReport += "<br />";

                    bool bUsageReportFound = false;
                    try
                    {
                        string[] sAllUsagePrintReportDataArray = sAllUsagePrintReportData.Split('ш');
                        if (sAllUsagePrintReportDataArray.Length > 1)
                        {
                            string[] sResellersDataArray = sResellersData.Split('ш');
                            foreach (string sResellerData in sResellersDataArray)
                            {
                                if (sResellerData != "")
                                {
                                    if (sResellerData.Split('ђ')[0].ToLower() == sCompany.ToLower())
                                    {
                                        string sCompanyId = sResellerData.Split('ђ')[2];
                                        string sCompanyVat = sResellerData.Split('ђ')[1];

                                        int iCount = 1;
                                        string sCompanyName = "n/a";
                                        foreach (string sAllUsagePrintReportLine in sAllUsagePrintReportDataArray)
                                        {
                                            if (sAllUsagePrintReportLine != "")
                                            {
                                                string sResellerId = sAllUsagePrintReportLine.Split('ђ')[22];
                                                if (sResellerId == sCompanyId)
                                                {
                                                    if (sCompanyName != sAllUsagePrintReportLine.Split('ђ')[4])
                                                    {
                                                        sCompanyName = sAllUsagePrintReportLine.Split('ђ')[4];
                                                        iCount = 1;

                                                        MachPanelWebService.CustomerResponse[] crAll = svc.SearchEndCustomer(sCompanyName);
                                                        foreach (MachPanelWebService.CustomerResponse cr in crAll)
                                                        {
                                                            if (cr.CustomerId == Convert.ToInt32(sAllUsagePrintReportLine.Split('ђ')[6]))
                                                            {
                                                                // customer data here
                                                                MachPanelWebService.TaxationSettings ts = svc.GetTaxationSettings(cr.CustomerId);
                                                                string s = ts.TaxTitle;
                                                            }
                                                        }

                                                        sMsg = sCompanyName;
                                                        MachPanelDataL.Text += "<br /><b>" + sMsg + "</b><br /><br />";
                                                    }

                                                    string sUsageReportData = sAllUsagePrintReportLine.Replace("ђ", ",").Replace("ш", "");
                                                    sMsg = iCount.ToString() + ". " + sUsageReportData;
                                                    MachPanelDataL.Text += sMsg + "<br />";
                                                    sPrintReport += sMsg + "<br />";
                                                    bUsageReportFound = true;
                                                    iCount++;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            bUsageReportFound = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MachPanelDataL.Text += ex.ToString();
                        bUsageReportFound = false;
                    }

                    if (bUsageReportFound == false)
                    {
                        sMsg = "No User Usage Report found for Company: " + sCompany;
                        MachPanelDataL.Text += sMsg + "<br />";
                        sPrintReport += sMsg + "<br />";
                    }
                }
            }

            // show user usage reports
            //MachPanelDataL.Text += sPrintReport;
        }

        protected void PushDataToNavB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }
    }
}