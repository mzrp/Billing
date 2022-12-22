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
    public partial class TimeLogCustomersImportDF : System.Web.UI.Page
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
                TimeLogDataWS.RPNAVConnectWS wsRPNAVConnectWS = new TimeLogDataWS.RPNAVConnectWS();
                wsRPNAVConnectWS.Timeout = 5400000;
                wsRPNAVConnectWS.UseDefaultCredentials = true;

                TLInfoLabel.Text = "TimeLog Web Service URL: ";
                TLInfoLabel.Text += ConfigurationManager.AppSettings["TLWSURL"].ToString();
                TLInfoLabel.Text += "<br />";
                TLInfoLabel.Text += wsRPNAVConnectWS.GetCredentials();

                stRPNAVConnectWS = wsRPNAVConnectWS.GetTimeLogCustomersData();

                // open db connection
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

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
                                    if (iCustomersCount == 0)
                                    {
                                        TimeLogDataL.Text += "<table cellpadding='3' cellspacing='3' border='0' width='100%'>";
                                        iCustomersCount++;
                                    }

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
                                }
                            }
                        }

                        if (iCustomersCount > 0)
                        {
                            TimeLogDataL.Text += "</table>";
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