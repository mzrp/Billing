using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RPNAVConnect
{
    public class SubsDues
    {
        public List<string> data { get; set; }
    }

    public partial class RPBillingLoginSuccess : System.Web.UI.Page
    {
        //Get value from cokkie    
        public string GetCookieValue(string _str)
        {
            if (Request.Cookies[_str] != null)
                return Request.Cookies[_str].Value;
            else
                return "n/a";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sAuthToken = "n/a";
            string sTokenType = "n/a";
            long lExpiresIn = -1;
            DateTime dExpiresAt = DateTime.MinValue;
            bool bTokenExpired = false;
            
            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            string sUserId = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserId"] != null)
                {
                    sUserId = System.Web.HttpContext.Current.Session["UserId"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserId = "n/a";
            }

            string strSqlQuery = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[BCLoginLog] WHERE [UserId] = '" + sUserId + "' ORDER BY Id DESC";
            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSqlQuery, dbConn);
            oleReader = cmd.ExecuteReader();
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(1))
                {
                    sAuthToken = oleReader.GetString(1);
                    sTokenType = oleReader.GetString(2);
                    lExpiresIn = oleReader.GetInt32(3);
                    dExpiresAt = oleReader.GetDateTime(4);

                    if (DateTime.Now > dExpiresAt)
                    {
                        bTokenExpired = true;
                    }
                }
            }
            oleReader.Close();

            dbConn.Close();

            if (sAuthToken != "n/a")
            {
                InfoDataL.Text = "Token:<br />" + sAuthToken + "<br />";
                InfoDataL.Text += "Token type:<br />" + sTokenType + "<br />";
                InfoDataL.Text += "Token expires in:<br />" + lExpiresIn.ToString() + "<br />";
                InfoDataL.Text += "Token expires at:<br />" + dExpiresAt.ToString() + "<br /><br />";
            }
            
            // get subscriptions dues
            SubscriptionsDueL.Text = "<b>SUBSCRIPTIONS DUE TODAY</b><br /><br />";
            SubscriptionsDueL.Text += "<table cellspacing='2' cellpadding='2' width='100%'>";
            SubscriptionsDueL.Text += "<tr>";
            SubscriptionsDueL.Text += "  <th>BCName</th><th>BCNo</th><th>Id</th><th>Description</th><th>FirstInvoice</th><th>BillingPeriod</th><th>InvoiceDate</th><th>NextInvoice</th><th>BillingCycle</th>";
            SubscriptionsDueL.Text += "</tr>";
            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var webRequestAUTH = WebRequest.Create("https://billing.gowingu.net/RPBilling/api/nav/push?dryRun=true") as HttpWebRequest;
                if (webRequestAUTH != null)
                {
                    webRequestAUTH.Method = "GET";

                    using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                    {
                        using (var srW = new StreamReader(rW))
                        {
                            var sExportAsJson = srW.ReadToEnd().Replace("[\r\n", "").Replace("\r\n]", "");

                            string[] duesubs = sExportAsJson.Split(new string[] { ",\r\n" }, StringSplitOptions.None);

                            bool bNewInvoicesExist = false;
                            foreach (string sData in duesubs)
                            {
                                if (sData.IndexOf("NEW_INVOICE    ") != -1)
                                {
                                    bNewInvoicesExist = true;
                                    break;
                                }
                            }

                            foreach (string sData in duesubs)
                            {
                                if (sData.IndexOf("NEW_INVOICE    ") != -1)
                                {
                                    SubscriptionsDueL.Text += "<tr>";

                                    string sDataSub = sData.Replace("NEW_INVOICE    ", "");
                                    // "01234567"
                                    if (sDataSub[0] == '\"')
                                    {
                                        sDataSub = sDataSub.Substring(1);
                                    }
                                    if (sDataSub[sDataSub.Length - 1] == '\"')
                                    {
                                        sDataSub = sDataSub.Substring(0, sDataSub.Length - 1);
                                    }

                                    string[] sDataArray = sDataSub.Split(',');
                                    foreach (string sDataCol in sDataArray)
                                    {
                                        SubscriptionsDueL.Text += "<td>" + sDataCol + "</td>";
                                    }
                                    SubscriptionsDueL.Text += "</tr>";
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
            }
            SubscriptionsDueL.Text += "</table>";

            // set parent vars
            string sUserIdCache = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserId"] != null)
                {
                    sUserIdCache = System.Web.HttpContext.Current.Session["UserId"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserIdCache = "n/a";
            }

            string sUserDisplayNameCache = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserDisplayName"] != null)
                {
                    sUserDisplayNameCache = System.Web.HttpContext.Current.Session["UserDisplayName"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserDisplayNameCache = "n/a";
            }

            string sUserExpirationDateTimeCache = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserExpirationDateTime"] != null)
                {
                    sUserExpirationDateTimeCache = System.Web.HttpContext.Current.Session["UserExpirationDateTime"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserExpirationDateTimeCache = "n/a";
            }

            string sUserAuthTokenCache = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserAuthToken"] != null)
                {
                    sUserAuthTokenCache = System.Web.HttpContext.Current.Session["UserAuthToken"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserAuthTokenCache = "n/a";
            }

            // set parent vars
            string sJavaScriptToRun = "";
            sJavaScriptToRun += "parent.document.getElementById('billingupdate').value = 'Friday, March 3, 2023 - 10:39 AM'; ";
            sJavaScriptToRun += "parent.document.getElementById('userid').value = '" + sUserIdCache + "'; ";
            sJavaScriptToRun += "parent.document.getElementById('username').value = '" + sUserDisplayNameCache + "'; ";
            sJavaScriptToRun += "parent.document.getElementById('usertoken').value = '" + sUserAuthTokenCache + "'; ";
            sJavaScriptToRun += "parent.document.getElementById('userdate').value = '" + sUserExpirationDateTimeCache + "'; ";

            sJavaScriptToRun += "document.getElementById('UpdateVersionDataL').innerHTML = parent.document.getElementById('billingupdate').value; ";
            sJavaScriptToRun += "document.getElementById('OwnerDataL').innerHTML = parent.document.getElementById('username').value; ";

            lastscriptdiv.InnerHtml = "<script>" + sJavaScriptToRun + "</script>";
        }

        protected void GetTokenBtn_Click(object sender, EventArgs e)
        {
            lastscriptdiv.InnerHtml = "<script>parent.location = 'https://billing.gowingu.net/RPBilling/';</script>";
        }
    }
}