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
        public double Balance_LCY { get; set; }
        public int BalanceAsVendor { get; set; }
        public double Balance_Due_LCY { get; set; }
        public int Credit_Limit_LCY { get; set; }
        public string Blocked { get; set; }
        public bool Privacy_Blocked { get; set; }
        public string Salesperson_Code { get; set; }
        public string Responsibility_Center { get; set; }
        public string Service_Zone_Code { get; set; }
        public string Document_Sending_Profile { get; set; }
        public double TotalSales2 { get; set; }
        public int CustSalesLCY_CustProfit_AdjmtCostLCY { get; set; }
        public double AdjCustProfit { get; set; }
        public double AdjProfitPct { get; set; }
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

    public class GetItems
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        public List<GetItem> value { get; set; }
    }

    public class GetItem
    {
        [JsonProperty("@odata.etag")]
        public string odataetag { get; set; }
        public string id { get; set; }
        public string number { get; set; }
        public string displayName { get; set; }
        public string type { get; set; }
        public string itemCategoryId { get; set; }
        public string itemCategoryCode { get; set; }
        public bool blocked { get; set; }
        public string gtin { get; set; }
        public int inventory { get; set; }
        public int unitPrice { get; set; }
        public bool priceIncludesTax { get; set; }
        public int unitCost { get; set; }
        public string taxGroupId { get; set; }
        public string taxGroupCode { get; set; }
        public string baseUnitOfMeasureId { get; set; }
        public string baseUnitOfMeasureCode { get; set; }
        public string generalProductPostingGroupId { get; set; }
        public string generalProductPostingGroupCode { get; set; }
        public string inventoryPostingGroupId { get; set; }
        public string inventoryPostingGroupCode { get; set; }
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

            InfoDataL.Text = "NAV->BC Migration<br /><br />";

            // open db connection
            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            try
            {
                using (var reader = new StreamReader(@"C:\Users\adm_mz\Desktop\BCNewItemsNumbers.csv"))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (line != "")
                        {

                            var values = line.Split(',');

                            string sBCNo = values[0];
                            string sNAVNo = values[1];

                            string sResultMessage = "";

                            if (sNAVNo != "")
                            {
                                string sUpdateField = "UPDATE [RPNAVConnect].[dbo].[BillingProducts_09012023] SET [NavProductNumber] = '" + sBCNo + "' WHERE [NavProductNumber] = '" + sNAVNo + "'";
                                sResultMessage = InsertUpdateDatabase(sUpdateField, dbConn);
                                if (sResultMessage != "DBOK")
                                {
                                    sResultMessage += "  ::  " + sUpdateField + "<br />";
                                }
                            }

                            InfoDataL.Text += sBCNo + "," + sNAVNo + ", " + sResultMessage + "<br />";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InfoDataL.Text += ex.ToString();
            }

            dbConn.Close();
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
    }
}