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

            header.UserName = "sales@rackpeople.dk";
            header.UserPassword = "Tu6Y8AH_";            

            header.AuthenticationToken = "";
            header.CompanyId = 87;

            MachPanelWebService.MachPanelService svc = new MachPanelWebService.MachPanelService();
            svc.Url = "https://controlpanel.gowingu.net/webservices/machpanelservice.asmx";
            svc.MPAuthenticationHeaderValue = header;

            string sResellerName = "RackPeople ApS";
            int iResellerId = 87;

            string sMsg = "Reseller:<b> " + sResellerName  + "</b>";
            MachPanelDataL.Text += sMsg;

            // MachPanel data
            MachPanelWebService.ResponseArguments ra = svc.Authenticate();
            MachPanelWebService.Customer[] allCustomers = svc.GetAllCustomers();
            MachPanelWebService.SubscriptionInfo[] allSubs = svc.GetAllSubscriptions();
            MachPanelWebService.PaymentGroup[] AllPg = svc.GetAllPaymentGroups();

            // list all usage report
            string sAllUsagePrintReportData = "";
            try
            {
                MachPanelWebService.ReportingCriteria RC = new MachPanelWebService.ReportingCriteria();
                MachPanelWebService.LyncUserUsageReport[] lyncuserUsageReportList = svc.GetLyncUserUsageReport(RC);
                if (lyncuserUsageReportList.Length > 0)
                {
                    int iCount = 0;
                    int iAllCount = 0;

                    foreach (MachPanelWebService.LyncUserUsageReport lur in lyncuserUsageReportList)
                    {
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

                        if (lur.ResellerId == iResellerId)
                        {
                            if (bCountItIn == true)
                            {
                                // ArchivingPolicy,ClientPolicy,ClientVersionPolicy,CompanyId,CompanyName,ConferencingPolicy,
                                // CustomerID,CustomerName,CustomerNumber,DateCreated,DialPlan,ExternalAccessPolicy,
                                // IsChatEnabled,LocationPolicy,LyncUser,MicrosoftSPLAType,MobilityPolicy,OrganizationName,
                                // Owner,PersistentChatPolicy,PhoneNumber,PinPolicy,ResellerId,SoldPackage,TelephonyOption,
                                // VoiceMailPolicy,VoicePolicy

                                sAllUsagePrintReportData += lur.CompanyName.Replace(",", ";") + "ђ" + lur.CustomerID.ToString() + "ђ" + lur.LyncUser.Replace(",", ";") + "ђ" + lur.PhoneNumber.Replace(",", ";") + "ђ" + lur.SoldPackage.Replace(",", ";") + "ђ" + lur.TelephonyOption.Replace(",", ";") + "ђ" + lur.VoicePolicy.Replace(",", ";") + "ш";
                                iCount++;
                            }
                        }

                        iAllCount++;
                    }

                    sMsg = iAllCount.ToString() + " user reports found, " + iCount.ToString() + " within date period for reseller " + sResellerName;
                    MachPanelDataL.Text += "<br />" + sMsg + "<br /><br />";
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            bool bUsageReportFound = false;
            try
            {
                string[] sAllUsagePrintReportDataArray = sAllUsagePrintReportData.Split('ш');
                if (sAllUsagePrintReportDataArray.Length > 1)
                {
                    int iCount = 1;
                    string sCompanyName = "n/a";
                    int iCustomerId = -1;
                    string sCompanyVAT = "n/a";
                    List<string> sCompanyPricingDataArray = new List<string>();
                    string sCompanyPricingData = "n/a";
                    sMsg = "";

                    foreach (string sAllUsagePrintReportLine in sAllUsagePrintReportDataArray)
                    {
                        if (sAllUsagePrintReportLine != "")
                        {
                            if (sCompanyName != sAllUsagePrintReportLine.Split('ђ')[0])
                            {
                                bool bFirstCustomer = false;
                                if (sCompanyName == "n/a")
                                {
                                    bFirstCustomer = true;                                    
                                }

                                sCompanyName = sAllUsagePrintReportLine.Split('ђ')[0];
                                iCustomerId = Convert.ToInt32(sAllUsagePrintReportLine.Split('ђ')[1]);

                                string[] sCustInvoicesArray = svc.GetInvoicesByCustomer(iCustomerId);

                                sMsg = sMsg.Replace("##UN##", (iCount - 1).ToString());

                                iCount = 1;                                

                                foreach (MachPanelWebService.Customer cust in allCustomers)
                                {
                                    if (cust.CompanyName == sCompanyName)
                                    {
                                        // VAT
                                        sCompanyVAT = cust.VATNumber;

                                        if (bFirstCustomer == false)
                                        {
                                            sMsg += "</table><br />";
                                        }

                                        sMsg += "<b>Customer: " + sCompanyName + ", VAT: " + sCompanyVAT + ", Users No: ##UN##</b><br /><br />";

                                        sMsg += "<b>Invoices:</b><br /><br />";
                                        bool bInvoicesExist = false;
                                        foreach (string sCustInvoice in sCustInvoicesArray)
                                        {
                                            if (sCustInvoice != "")
                                            {
                                                MachPanelWebService.Invoice invDetail = svc.GetInvoiceDetail(Convert.ToInt32(sCustInvoice));

                                                bInvoicesExist = true;

                                                sMsg += "Inv.no: " + sCustInvoice + ", Amount: " + invDetail.InvoicedAmount + " " + invDetail.Currency + " DueDate: " + invDetail.DueDate + " Status: " + invDetail.InvoiceStatus + "<br />";
                                                int iInvLineCount = 1;
                                                sMsg += "<br />";
                                                foreach (MachPanelWebService.InvoiceLineItem ili in invDetail.LineItems)
                                                {
                                                    sMsg += "&nbsp;&nbsp;&nbsp;&nbsp;" + iInvLineCount.ToString() + ".&nbsp;" + ili.SubscriptionName + ", Price: " + ili.Price + " Quantity: " + ili.Quantity + "<br />";
                                                    iInvLineCount++;
                                                }

                                                sMsg += "<br />";
                                            }
                                        }

                                        if (bInvoicesExist == false)
                                        {
                                            sMsg += "<i>No invoices found.</i><br />";
                                        }

                                        sMsg += "<b>Usage Report:</b><br /><br />";

                                        sMsg += "<table border='0' cellpadding='3' cellpacing='3'>";

                                        // Addons/Cycle/Price
                                        sCompanyPricingDataArray.Clear();
                                        foreach (MachPanelWebService.SubscriptionInfo sub in allSubs)
                                        {
                                            if (sub.CustomerID == iCustomerId)
                                            {
                                                int iPaymentGroupId = -1;
                                                foreach (MachPanelWebService.PaymentGroup pg in AllPg)
                                                {
                                                    iPaymentGroupId = pg.PaymentGroupId;
                                                    MachPanelWebService.AddOnInfo[] allAoi = svc.GetAddOnsBySubscription(sub.HostingID, iPaymentGroupId, MachPanelWebService.ServiceTypes.Lync_Hosting);
                                                    foreach (MachPanelWebService.AddOnInfo aoi in allAoi)
                                                    {
                                                        MachPanelWebService.BillingCycle[] allBc = aoi.AddOnBillingCycles;
                                                        foreach (MachPanelWebService.BillingCycle bc in allBc)
                                                        {
                                                            sCompanyPricingDataArray.Add(aoi.AddOnTitle + "#$#" + bc.CycleName + ", " + bc.Price.ToString("N") + " " + pg.CurrencySymbol);
                                                        }
                                                    }
                                                }

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }
                                
                            }

                            string[] sAllUsagePrintReportLineArray = sAllUsagePrintReportLine.Split('ђ');
                            int iFieldsCount = 0;
                            sMsg += "<tr valign='top' align='left'><td valign='top' align='left'>" + iCount.ToString() + ". </td><td valign='top' align='left'>";
                            foreach (string sAllUsagePrintReportSingleLine in sAllUsagePrintReportLineArray)
                            {
                                if (iFieldsCount > 1)
                                {
                                    sMsg += sAllUsagePrintReportSingleLine;
                                    if (iFieldsCount != sAllUsagePrintReportLineArray.Length - 1)
                                    {
                                        if (iFieldsCount == 3)
                                        {
                                            sMsg += "<br />";
                                        }
                                        else
                                        {
                                            if (sAllUsagePrintReportSingleLine != "")
                                            {
                                                sMsg += ", ";
                                            }
                                        }
                                    }
                                }
                                iFieldsCount++;
                            }

                            // pricing
                            string sSoldPackage = sAllUsagePrintReportLine.Split('ђ')[4];
                            sCompanyPricingData = "Cycle: ";

                            foreach (string sCompanyPricingDataLine in sCompanyPricingDataArray)
                            {
                                if (sCompanyPricingDataLine.IndexOf(sSoldPackage + "[") == 0)
                                {
                                    sCompanyPricingData += sCompanyPricingDataLine.Substring(sCompanyPricingDataLine.IndexOf("#$#") + 3) + "; ";
                                }
                            }

                            sMsg += "<br />" + sCompanyPricingData + "<br /><br />";
                            sMsg += "</td></tr>";

                            bUsageReportFound = true;
                            iCount++;
                        }
                    }

                    // finish last one
                    sMsg += "</table>";
                    MachPanelDataL.Text += sMsg + "<br />";

                    // handle last one
                    MachPanelDataL.Text = MachPanelDataL.Text.Replace("##UN##", (iCount - 1).ToString());
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
                sMsg = "No User Usage Report found for Company: " + sResellerName;
                MachPanelDataL.Text += sMsg + "<br />";
            }
        }

        protected void PushDataToNavB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }
    }
}