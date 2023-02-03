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
using System.IO;

using Newtonsoft.Json;

using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace RPNAVConnect
{
    public partial class RPBillingLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            DatabaseService db = new DatabaseService();

            string sRefreshToken = "n/a";
            string sAuthToken = "n/a";
            string sTokenType = "n/a";
            long lExpiresIn = -1;
            string sExpirationDateTime = "n/a";
            string sGraphAuthToken = "n/a";
            string sGraphTokenType = "n/a";
            long lGraphExpiresIn = -1;

            string sMSCode = "n/a";
            try
            {
                sMSCode = Request.QueryString["code"];
                if (sMSCode == null)
                {
                    sMSCode = "n/a";
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sMSCode = "n/a";
            }

            if (sMSCode == "n/a")
            {
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
                if (sUserId == null)
                {
                    sUserId = "n/a";
                }
                if (sUserId == "")
                {
                    sUserId = "n/a";
                }

                if (sUserId == "n/a")
                {
                    // login first
                    //string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&response_type=code&scope=Files.ReadWrite%20User.Read%20Financials.ReadWrite.All&response_mode=query&state=12345&redirect_uri=https://billing.gowingu.net/RPBillingLogin.aspx";
                    //string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&response_type=code&scope=Files.ReadWrite%20User.Read%20Financials.ReadWrite.All&response_mode=query&state=12345&redirect_uri=http://localhost:57069/RPBillingLogin.aspx";
                    string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&response_type=code&scope=https://api.businesscentral.dynamics.com/.default&response_mode=query&state=12345&redirect_uri=https://billing.gowingu.net/RPBillingLogin.aspx";
                    //string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&response_type=code&scope=https://api.businesscentral.dynamics.com/.default&response_mode=query&state=12345&redirect_uri=http://localhost:57069/RPBillingLogin.aspx";
                    lastscriptdiv.InnerHtml = "<script>window.location='" + sLoginUrl + "';</script>";
                }
                else
                {
                    // check token
                    bool bTokenValid = db.IsTokenValid(sUserId);
                    if (bTokenValid == true)
                    {
                        // go to the dashboard
                        string sLoginUrl = "https://billing.gowingu.net/RPBilling/dashboard";
                        lastscriptdiv.InnerHtml = "<script>window.location='" + sLoginUrl + "';</script>";
                    }
                    else
                    {
                        // login first
                        //string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&response_type=code&scope=Files.ReadWrite%20User.Read%20Financials.ReadWrite.All&response_mode=query&state=12345&redirect_uri=https://billing.gowingu.net/RPBillingLogin.aspx";
                        //string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&response_type=code&scope=Files.ReadWrite%20User.Read%20Financials.ReadWrite.All&response_mode=query&state=12345&redirect_uri=http://localhost:57069/RPBillingLogin.aspx";
                        string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&response_type=code&scope=https://api.businesscentral.dynamics.com/.default&response_mode=query&state=12345&redirect_uri=https://billing.gowingu.net/RPBillingLogin.aspx";
                        //string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&response_type=code&scope=https://api.businesscentral.dynamics.com/.default&response_mode=query&state=12345&redirect_uri=http://localhost:57069/RPBillingLogin.aspx";
                        lastscriptdiv.InnerHtml = "<script>window.location='" + sLoginUrl + "';</script>";
                    }
                }
            }
            else
            {
                try
                {
                    // BC token
                    var webRequestAUTH = WebRequest.Create("https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/token") as HttpWebRequest;
                    if (webRequestAUTH != null)
                    {
                        webRequestAUTH.Method = "POST";
                        webRequestAUTH.Host = "login.microsoftonline.com";
                        webRequestAUTH.ContentType = "application/x-www-form-urlencoded";

                        //string sParams = "code=" + sMSCode + "&client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&scope=https://api.businesscentral.dynamics.com/.default%20offline_access&client_secret=q2H8Q~3jNmeqtG7e7jYnz04ZG4KQ4Wh6WwJ~Ucou&grant_type=authorization_code&redirect_uri=https://billing.gowingu.net/RPBillingLogin.aspx";
                        //string sParams = "code=" + sMSCode + "&client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&scope=https://api.businesscentral.dynamics.com/.default%20offline_access&client_secret=q2H8Q~3jNmeqtG7e7jYnz04ZG4KQ4Wh6WwJ~Ucou&grant_type=authorization_code&redirect_uri=http://localhost:57069/RPBillingLogin.aspx";
                        string sParams = "code=" + sMSCode + "&client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&scope=https://api.businesscentral.dynamics.com/.default%20offline_access&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&grant_type=authorization_code&redirect_uri=https://billing.gowingu.net/RPBillingLogin.aspx";
                        //string sParams = "code=" + sMSCode + "&client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&scope=https://api.businesscentral.dynamics.com/.default%20offline_access&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&grant_type=authorization_code&redirect_uri=http://localhost:57069/RPBillingLogin.aspx";
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
                                var sExport = JsonConvert.DeserializeObject<MsAuthToken>(sExportAsJson);

                                sRefreshToken = sExport.RefreshToken;
                                sAuthToken = sExport.AccessToken;
                                sTokenType = sExport.TokenType;
                                lExpiresIn = sExport.ExpiresIn;
                                DateTime AuthTokenExpireAt = DateTime.Now.AddSeconds(lExpiresIn);

                                sExpirationDateTime = AuthTokenExpireAt.Year.ToString().PadLeft(4, '0') + "-";
                                sExpirationDateTime += AuthTokenExpireAt.Month.ToString().PadLeft(2, '0') + "-";
                                sExpirationDateTime += AuthTokenExpireAt.Day.ToString().PadLeft(2, '0') + " ";
                                sExpirationDateTime += AuthTokenExpireAt.Hour.ToString().PadLeft(2, '0') + ":";
                                sExpirationDateTime += AuthTokenExpireAt.Minute.ToString().PadLeft(2, '0') + ":";
                                sExpirationDateTime += AuthTokenExpireAt.Second.ToString().PadLeft(2, '0');
                            }
                        }

                        webRequestAUTH = null;
                    }

                    // get token for graph
                    if (sRefreshToken != "n/a")
                    {
                        var webRequestAUTHG = WebRequest.Create("https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/token") as HttpWebRequest;
                        if (webRequestAUTHG != null)
                        {
                            webRequestAUTHG.Method = "POST";
                            webRequestAUTHG.Host = "login.microsoftonline.com";
                            webRequestAUTHG.ContentType = "application/x-www-form-urlencoded";

                            string sParamsG = "refresh_token=" + sRefreshToken + "&client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&scope=https://graph.microsoft.com/.default&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&grant_type=refresh_token&redirect_uri=https://billing.gowingu.net/RPBillingLogin.aspx";
                            //string sParamsG = "refresh_token=" + sRefreshToken + "&client_id=9d7b8b42-6e6e-47b3-9965-95be89a6e987&scope=https://graph.microsoft.com/.default&client_secret=g4q8Q~PqkWMvrkWMVXUNm4R2NszPnjeP.sygidsE&grant_type=refresh_token&redirect_uri=http://localhost:57069/RPBillingLogin.aspx";
                            var dataG = Encoding.ASCII.GetBytes(sParamsG);
                            webRequestAUTHG.ContentLength = dataG.Length;

                            using (var sWG = webRequestAUTHG.GetRequestStream())
                            {
                                sWG.Write(dataG, 0, dataG.Length);
                            }

                            using (var rWG = webRequestAUTHG.GetResponse().GetResponseStream())
                            {
                                using (var srWG = new StreamReader(rWG))
                                {
                                    var sExportAsJson = srWG.ReadToEnd();
                                    var sExport = JsonConvert.DeserializeObject<MsAuthToken>(sExportAsJson);

                                    sGraphAuthToken = sExport.AccessToken;
                                    sGraphTokenType = sExport.TokenType;
                                    lGraphExpiresIn = sExport.ExpiresIn;

                                    InfoDataL.Text += "Graph Token:<br />" + sGraphAuthToken + "<br />";
                                    InfoDataL.Text += "Graph Token type:<br />" + sGraphTokenType + "<br />";
                                    InfoDataL.Text += "Graph Token expires in:<br />" + lGraphExpiresIn.ToString() + "<br /><br />";
                                }
                            }
                            webRequestAUTHG = null;
                        }
                    }

                    // get logged user data
                    if (sGraphAuthToken != "n/a")
                    {
                        var webRequestAUTHG2 = WebRequest.Create("https://graph.microsoft.com/v1.0/me") as HttpWebRequest;
                        if (webRequestAUTHG2 != null)
                        {
                            webRequestAUTHG2.Method = "GET";
                            webRequestAUTHG2.Host = "graph.microsoft.com";
                            webRequestAUTHG2.Headers.Add("Authorization", "Bearer " + sGraphAuthToken);

                            using (var rW = webRequestAUTHG2.GetResponse().GetResponseStream())
                            {
                                using (var srW = new StreamReader(rW))
                                {
                                    var sExportAsJson = srW.ReadToEnd();
                                    var sExport = JsonConvert.DeserializeObject<GraphUserMe>(sExportAsJson);

                                    string sMail = sExport.mail;
                                    string sDisplayName = sExport.displayName;
                                    string sId = sExport.id;

                                    string sSql = "INSERT INTO [RPNAVConnect].[dbo].[BCLoginLog] ([Token], [TokenType], [TokenExpiresIn], [TokenExpiresAt], [TokeRefresh], [UserName], [UserMail], [UserId]) ";
                                    sSql += "VALUES ('" + sAuthToken + "', '" + sTokenType + "', " + lExpiresIn.ToString() + ", '" + sExpirationDateTime + "', '" + sRefreshToken + "', '" + sDisplayName + "', '" + sMail + "', '" + sId + "')";
                                    string sDBResult = db.InsertUpdateDatabase(sSql);
                                    if (sDBResult != "DBOK")
                                    {
                                        sDBResult += sSql + " ::: " + sDBResult;
                                    }

                                    try
                                    {
                                        System.Web.HttpContext.Current.Session.Add("UserId", sId);
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                    }

                                    try
                                    {
                                        System.Web.HttpContext.Current.Session.Add("UserDisplayName", sDisplayName);
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                    }

                                    try
                                    {
                                        System.Web.HttpContext.Current.Session.Add("UserAuthToken", sAuthToken);
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                    }

                                    try
                                    {
                                        System.Web.HttpContext.Current.Session.Add("UserExpirationDateTime", sExpirationDateTime);
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                    }
                                }
                            }

                            webRequestAUTHG2 = null;
                        }
                    }

                    // go to the dashboard
                    string sLoginUrl = "https://billing.gowingu.net/RPBilling/dashboard";
                    lastscriptdiv.InnerHtml = "<script>window.location='" + sLoginUrl + "';</script>";
                }
                catch (Exception ex)
                {
                    ex.ToString();

                    // go to the dashboard
                    string sLoginUrl = "https://billing.gowingu.net/RPBilling/dashboard";
                    lastscriptdiv.InnerHtml = "<script>window.location='" + sLoginUrl + "';</script>";
                }
            }

        }
    }
}