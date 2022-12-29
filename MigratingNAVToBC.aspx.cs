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
using System.IO;

using Newtonsoft.Json;

using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace RPNAVConnect
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ODataV4Customers
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        public List<ODataV4Customer> value { get; set; }
    }

    public class ODataV4Customer
    {
        [JsonProperty("@odata.etag")]
        public string odataetag { get; set; }
        public string No { get; set; }
        public string Name { get; set; }
        public string Name_2 { get; set; }
        public string Search_Name { get; set; }
        public string IC_Partner_Code { get; set; }
        public int Balance_LCY { get; set; }
        public int BalanceAsVendor { get; set; }
        public int Balance_Due_LCY { get; set; }
        public int Credit_Limit_LCY { get; set; }
        public string Blocked { get; set; }
        public bool Privacy_Blocked { get; set; }
        public string Salesperson_Code { get; set; }
        public string Responsibility_Center { get; set; }
        public string Service_Zone_Code { get; set; }
        public string Document_Sending_Profile { get; set; }
        public int TotalSales2 { get; set; }
        public int CustSalesLCY_CustProfit_AdjmtCostLCY { get; set; }
        public int AdjCustProfit { get; set; }
        public int AdjProfitPct { get; set; }
        public string Last_Date_Modified { get; set; }
        public bool Disable_Search_by_Name { get; set; }
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string Country_Region_Code { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Post_Code { get; set; }
        public string ShowMap { get; set; }
        public string Phone_No { get; set; }
        public string MobilePhoneNo { get; set; }
        public string E_Mail { get; set; }
        public string Fax_No { get; set; }
        public string Home_Page { get; set; }
        public string Language_Code { get; set; }
        public string Primary_Contact_No { get; set; }
        public string ContactName { get; set; }
        public string Microsoft_CSP_ID { get; set; }
        public string Microsoft_CSP_ID2 { get; set; }
        public string Bill_to_Customer_No { get; set; }
        public string VAT_Registration_No { get; set; }
        public string EORI_Number { get; set; }
        public string GLN { get; set; }
        public bool Use_GLN_in_Electronic_Document { get; set; }
        public string Copy_Sell_to_Addr_to_Qte_From { get; set; }
        public string OIOUBL_Account_Code { get; set; }
        public string OIOUBL_Profile_Code { get; set; }
        public bool OIOUBL_Profile_Code_Required { get; set; }
        public bool Tax_Liable { get; set; }
        public string Tax_Area_Code { get; set; }
        public string Gen_Bus_Posting_Group { get; set; }
        public string VAT_Bus_Posting_Group { get; set; }
        public string Customer_Posting_Group { get; set; }
        public string Currency_Code { get; set; }
        public string Price_Calculation_Method { get; set; }
        public string Customer_Price_Group { get; set; }
        public string Customer_Disc_Group { get; set; }
        public bool Allow_Line_Disc { get; set; }
        public string Invoice_Disc_Code { get; set; }
        public bool Prices_Including_VAT { get; set; }
        public int Prepayment_Percent { get; set; }
        public string Application_Method { get; set; }
        public string Partner_Type { get; set; }
        public string Intrastat_Partner_Type { get; set; }
        public string Payment_Terms_Code { get; set; }
        public string Payment_Method_Code { get; set; }
        public string Reminder_Terms_Code { get; set; }
        public string Fin_Charge_Terms_Code { get; set; }
        public string Cash_Flow_Payment_Terms_Code { get; set; }
        public bool Print_Statements { get; set; }
        public int Last_Statement_No { get; set; }
        public bool Block_Payment_Tolerance { get; set; }
        public string Preferred_Bank_Account_Code { get; set; }
        public string Ship_to_Code { get; set; }
        public string Location_Code { get; set; }
        public bool Combine_Shipments { get; set; }
        public string Reserve { get; set; }
        public string Shipping_Advice { get; set; }
        public string Shipment_Method_Code { get; set; }
        public string Shipping_Agent_Code { get; set; }
        public string Shipping_Agent_Service_Code { get; set; }
        public string Shipping_Time { get; set; }
        public string Base_Calendar_Code { get; set; }
        public string Customized_Calendar { get; set; }
        public string Default_Trans_Type { get; set; }
        public string Default_Trans_Type_Return { get; set; }
        public string Def_Transport_Method { get; set; }
        public int ExpectedCustMoneyOwed { get; set; }
        public int TotalMoneyOwed { get; set; }
        public int CalcCreditLimitLCYExpendedPct { get; set; }
        public int Balance_Due { get; set; }
        public int Payments_LCY { get; set; }
        public int CustomerMgt_AvgDaysToPay_No { get; set; }
        public int DaysPaidPastDueDate { get; set; }
        public int AmountOnPostedInvoices { get; set; }
        public int AmountOnCrMemo { get; set; }
        public int AmountOnOutstandingInvoices { get; set; }
        public int AmountOnOutstandingCrMemos { get; set; }
        public int Totals { get; set; }
        public int CustInvDiscAmountLCY { get; set; }
        public string Global_Dimension_1_Filter { get; set; }
        public string Global_Dimension_2_Filter { get; set; }
        public string Currency_Filter { get; set; }
        public string Date_Filter { get; set; }
    }


    public partial class MsAuthToken
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("ext_expires_in")]
        public long ExtExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class GraphUserMe
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        public List<object> businessPhones { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public object jobTitle { get; set; }
        public string mail { get; set; }
        public object mobilePhone { get; set; }
        public object officeLocation { get; set; }
        public object preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string id { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string countryLetterCode { get; set; }
        public string postalCode { get; set; }
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



    public partial class MigratingNAVToBC : System.Web.UI.Page
    {
        public static string sNAVLogin = "rpnavapi";
        public static string sNAVPassword = "Telefon1";
        public static string sNAVDomain = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sAuthToken = "n/a";
            string sTokenType = "n/a";
            long lExpiresIn = -1;

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

            try
            {
                sAuthToken = Request.QueryString["token"];
                if (sAuthToken == null)
                {
                    sAuthToken = "n/a";
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sAuthToken = "n/a";
            }

            if (sAuthToken == "n/a")
            {
                if (sMSCode == "n/a")
                {
                    // login first
                    string sLoginUrl = "https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/authorize?client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&response_type=code&scope=Files.ReadWrite%20User.Read%20Financials.ReadWrite.All&response_mode=query&state=12345&redirect_uri=http://localhost:57069/MigratingNAVToBC.aspx";
                    Response.Redirect(sLoginUrl);
                }
                else
                {
                    try
                    {
                        var webRequestAUTH = WebRequest.Create("https://login.microsoftonline.com/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/oauth2/v2.0/token") as HttpWebRequest;
                        if (webRequestAUTH != null)
                        {
                            webRequestAUTH.Method = "POST";
                            webRequestAUTH.Host = "login.microsoftonline.com";
                            webRequestAUTH.ContentType = "application/x-www-form-urlencoded";

                            string sParams = "code=" + sMSCode + "&client_id=9df51886-7601-4456-a35f-a80c0e16f4c0&scope=https://api.businesscentral.dynamics.com/.default&client_secret=q2H8Q~3jNmeqtG7e7jYnz04ZG4KQ4Wh6WwJ~Ucou&grant_type=authorization_code&redirect_uri=http://localhost:57069/MigratingNAVToBC.aspx";
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

                                    sAuthToken = sExport.AccessToken;
                                    sTokenType = sExport.TokenType;
                                    lExpiresIn = sExport.ExpiresIn;

                                    DateTime AuthTokenExpireIn = DateTime.Now.AddSeconds(lExpiresIn);

                                    InfoDataL.Text += "Token:<br />" + sAuthToken + "<br />";
                                    InfoDataL.Text += "Token type:<br />" + sTokenType + "<br />";
                                    InfoDataL.Text += "Token expires in:<br />" + lExpiresIn.ToString() + "<br /><br />";
                                    InfoDataL.Text += "Relaod page:<br /><a href='http://localhost:57069/MigratingNAVToBC.aspx?token=" + sAuthToken + "'>http://localhost:57069/MigratingNAVToBC.aspx?token=" + sAuthToken + "</a>";
                                }
                            }

                            webRequestAUTH = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
            }
            else
            {
                // Get NAV customers and import them into BC
                /*
                  
                InfoDataL.Text += "<br /><br />NAV Customers:<br /><br />";

                try
                {
                    // get access to NAVDebtor
                    CustomerInfo2_Service nav = new CustomerInfo2_Service();
                    nav.UseDefaultCredentials = true;
                    //nav.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword, sNAVDomain);
                    nav.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword);

                    // Run the actual search.
                    CustomerInfo2[] customers = nav.ReadMultiple(null, null, 10000);
                    int iCount = 1;
                    foreach (CustomerInfo2 customer in customers)
                    {
                        InfoDataL.Text += iCount.ToString() + ". " + customer.Name + ", " + customer.No + ", " + customer.Address + ", " + customer.Country_Region_Code + ", " + customer.City + ", " + customer.Post_Code + ", " + customer.Phone_No + ", " + customer.E_Mail + ", " + customer.Home_Page + ", " + customer.Contact + "<br />";
                        iCount++;

                        try
                        {
                            string sNewGuid = Guid.NewGuid().ToString();
                            var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/production/api/v2.0/companies(2af24b6d-a627-ed11-9db8-000d3a21e61f)/customers") as HttpWebRequest;
                            if (webRequestAUTH != null)
                            {
                                webRequestAUTH.Method = "POST";
                                webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                                webRequestAUTH.ContentType = "application/json";
                                webRequestAUTH.Headers["Authorization"] = "Bearer " + sAuthToken;
                                //webRequestAUTH.Headers["If-Match"] = "*";

                                string jsonToSend = "{";
                                jsonToSend += "\"displayName\": \"" + customer.Name + "\",";
                                jsonToSend += "\"number\": \"" + customer.No + "\",";
                                jsonToSend += "\"type\": \"Company\",";
                                jsonToSend += "\"addressLine1\": \"" + customer.Address + "\",";
                                jsonToSend += "\"addressLine2\": \"\",";
                                jsonToSend += "\"city\": \"" + customer.City + "\",";
                                jsonToSend += "\"state\": \"\",";
                                jsonToSend += "\"country\": \"" + customer.Country_Region_Code + "\",";
                                jsonToSend += "\"postalCode\": \"" + customer.Post_Code + "\",";
                                jsonToSend += "\"phoneNumber\": \"" + customer.Phone_No + "\",";
                                jsonToSend += "\"email\": \"" + customer.E_Mail + "\",";
                                jsonToSend += "\"website\": \"\",";
                                jsonToSend += "\"taxLiable\": true,";
                                
                                //jsonToSend += "\"taxAreaId\": \"00000000-0000-0000-0000-000000000000\",";
                                //jsonToSend += "\"taxRegistrationNumber\": \"\",";
                                //jsonToSend += "\"currencyId\": \"00000000-0000-0000-0000-000000000000\",";
                                //jsonToSend += "\"currencyCode\": \"" + customer.Currency_Code + "\",";
                                //jsonToSend += "\"paymentTermsId\": \"00000000-0000-0000-0000-000000000000\",";
                                //jsonToSend += "\"shipmentMethodId\": \"00000000-0000-0000-0000-000000000000\",";
                                //jsonToSend += "\"paymentMethodId\": \"00000000-0000-0000-0000-000000000000\",";
                                
                                jsonToSend += "\"blocked\": \" \"";
                                jsonToSend += "}";

                                byte[] bytes = Encoding.UTF8.GetBytes(jsonToSend);
                                webRequestAUTH.ContentLength = bytes.Length;

                                Stream requestStream = webRequestAUTH.GetRequestStream();
                                requestStream.Write(bytes, 0, bytes.Length);
                                requestStream.Close();

                                using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                                {
                                    using (var srW = new StreamReader(rW))
                                    {
                                        var sExportAsJson = srW.ReadToEnd();
                                        var sExport = JsonConvert.DeserializeObject<BCCustomer>(sExportAsJson);

                                        string sNewCusotmerId = sExport.id;
                                        string sNewCusotmerName = sExport.displayName;
                                        string sNewCusotmerNumber = sExport.number;

                                        InfoDataL.Text += "Customer:" + sNewCusotmerId + " (" + sNewCusotmerId + "), Number: " + sNewCusotmerNumber + "<br />";
                                    }
                                }

                                webRequestAUTH = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                        }

                        break;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                */

                // get BC customers for RP Test
                InfoDataL.Text += "<br /><br />BC Customers:<br /><br />";

                try
                {
                    //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;

                    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Test/api/v2.0/companies(2af24b6d-a627-ed11-9db8-000d3a21e61f)/customers") as HttpWebRequest;
                    if (webRequestAUTH != null)
                    {
                        webRequestAUTH.Method = "GET";
                        webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                        webRequestAUTH.ContentType = "application/json";
                        webRequestAUTH.MediaType = "application/json";
                        webRequestAUTH.Accept = "application/json";

                        webRequestAUTH.Headers["Authorization"] = "Bearer " + sAuthToken;

                        using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                        {
                            using (var srW = new StreamReader(rW))
                            {
                                var sExportAsJson = srW.ReadToEnd();
                                var sExport = JsonConvert.DeserializeObject<BCCustomers>(sExportAsJson);

                                int iCount = 1;
                                foreach (var cust in sExport.value)
                                {
                                    InfoDataL.Text += iCount.ToString() + ". " + cust.displayName + "<br />";
                                    iCount++;

                                    /*
                                    try
                                    {
                                        // delete customer
                                        var webRequestAUTH2 = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Test/api/v2.0/companies(2af24b6d-a627-ed11-9db8-000d3a21e61f)/customers(" + cust.id + ")") as HttpWebRequest;
                                        if (webRequestAUTH2 != null)
                                        {
                                            webRequestAUTH2.Method = "DELETE";
                                            webRequestAUTH2.Host = "api.businesscentral.dynamics.com";
                                            webRequestAUTH2.Headers["If-Match"] = "*";

                                            webRequestAUTH2.Headers["Authorization"] = "Bearer " + sAuthToken;

                                            using (var rW2 = webRequestAUTH2.GetResponse().GetResponseStream())
                                            {
                                                using (var srW2 = new StreamReader(rW2))
                                                {
                                                    var sExportAsJson2 = srW2.ReadToEnd();
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                    }
                                    */
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



            }
        }
    }
}