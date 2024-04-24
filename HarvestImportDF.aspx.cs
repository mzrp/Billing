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
using System.Security.Cryptography;
using Microsoft.Office.Interop.Excel;
using Microsoft.Store.PartnerCenter.Models.Orders;
using System.Xml.Linq;
using Microsoft.Store.PartnerCenter.Models;

namespace RPNAVConnect
{
    public class HarvestClientPost
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool is_active { get; set; }
        public object address { get; set; }
        public string statement_key { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string currency { get; set; }
    }

    public class HarvestClient
    {
        public int id { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
    }

    public class HarvestProjectPost
    {
        public int id { get; set; }
        public string name { get; set; }
        public object code { get; set; }
        public bool is_active { get; set; }
        public string bill_by { get; set; }
        public double? budget { get; set; }
        public string budget_by { get; set; }
        public bool budget_is_monthly { get; set; }
        public bool notify_when_over_budget { get; set; }
        public double? over_budget_notification_percentage { get; set; }
        public object over_budget_notification_date { get; set; }
        public bool show_budget_to_all { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object starts_on { get; set; }
        public object ends_on { get; set; }
        public bool is_billable { get; set; }
        public bool is_fixed_fee { get; set; }
        public string notes { get; set; }
        public HarvestClient client { get; set; }
        public object cost_budget { get; set; }
        public bool cost_budget_include_expenses { get; set; }
        public double? hourly_rate { get; set; }
        public object fee { get; set; }
    }

    public class Creator
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
    }

    public class LineItem
    {
        public int id { get; set; }
        public string kind { get; set; }
        public string description { get; set; }
        public double quantity { get; set; }
        public double unit_price { get; set; }
        public double amount { get; set; }
        public bool taxed { get; set; }
        public bool taxed2 { get; set; }
        public Project project { get; set; }
    }

    public class Invoice
    {
        public int id { get; set; }
        public string client_key { get; set; }
        public string number { get; set; }
        public string purchase_order { get; set; }
        public double amount { get; set; }
        public double? due_amount { get; set; }
        public object tax { get; set; }
        public double? tax_amount { get; set; }
        public object tax2 { get; set; }
        public double? tax2_amount { get; set; }
        public object discount { get; set; }
        public double? discount_amount { get; set; }
        public string subject { get; set; }
        public string notes { get; set; }
        public string state { get; set; }
        public string period_start { get; set; }
        public string period_end { get; set; }
        public string issue_date { get; set; }
        public string due_date { get; set; }
        public string payment_term { get; set; }
        public object sent_at { get; set; }
        public object paid_at { get; set; }
        public object closed_at { get; set; }
        public object recurring_invoice_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object paid_date { get; set; }
        public string currency { get; set; }
        public Client client { get; set; }
        public object estimate { get; set; }
        public object retainer { get; set; }
        public Creator creator { get; set; }
        public List<LineItem> line_items { get; set; }
    }

    public class Links
    {
        public string first { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public string last { get; set; }
    }

    public class HarvestInvocies
    {
        public List<Invoice> invoices { get; set; }
        public int per_page { get; set; }
        public int total_pages { get; set; }
        public int total_entries { get; set; }
        public object next_page { get; set; }
        public object previous_page { get; set; }
        public int page { get; set; }
        public Links links { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Client
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool is_active { get; set; }
        public string address { get; set; }
        public string statement_key { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string currency { get; set; }
    }

    public class HarvestClients
    {
        public List<Client> clients { get; set; }
        public int per_page { get; set; }
        public int total_pages { get; set; }
        public int total_entries { get; set; }
        public object next_page { get; set; }
        public object previous_page { get; set; }
        public int page { get; set; }
        public Links links { get; set; }
    }



    public partial class HarvestImportDF : System.Web.UI.Page
    {
        public string sBCToken = "n/a";

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            HarvestDataL.Text = "";

            // get bc token
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

            //sUserId = "f43f4edb-7436-4561-89a0-d08c543767c0";

            if (sUserId != "n/a")
            {
                DatabaseService db = new DatabaseService();
                sBCToken = db.GetBCToken(sUserId);
                if (sBCToken == "n/a")
                {
                    // go to the dashboard
                    string sLoginUrl = "https://billing.gowingu.net/RPBilling/dashboard";
                    lastscriptdiv.InnerHtml = "<script>window.location='" + sLoginUrl + "';</script>";
                }
            }
            else
            {
                // go to the dashboard
                string sLoginUrl = "https://billing.gowingu.net/RPBilling/dashboard";
                lastscriptdiv.InnerHtml = "<script>window.location='" + sLoginUrl + "';</script>";
            }

            // init page
            if (Page.IsPostBack == false)
            {
                StartMonthTB.Text = DateTime.Now.AddMonths(-1).Month.ToString();
                StartYearTB.Text = DateTime.Now.AddMonths(-1).Year.ToString();
                EndMonthTB.Text = DateTime.Now.AddMonths(-1).Month.ToString();
                EndYearTB.Text = DateTime.Now.AddMonths(-1).Year.ToString();
            }
        }

        protected void GetBCCustomersB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            GetBCCustomersL.Text = "";

            List<Client> sAllClients = new List<Client>();

            // get Harvest customers
            try
            {
                var url = "https://api.harvestapp.com/v2/clients";

                bool bHasMore = true;
                while (bHasMore == true)
                {
                    //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;

                    var httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                    httpRequest.Method = "GET";
                    httpRequest.Host = "api.harvestapp.com";
                    httpRequest.Headers["Harvest-Account-ID"] = "1475424";
                    httpRequest.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                    httpRequest.UserAgent = "Harvest API Example";

                    var httpResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                    {
                        var sResultJson = streamReader.ReadToEnd();

                        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
                        HarvestClients allHarvestClients = Newtonsoft.Json.JsonConvert.DeserializeObject<HarvestClients>(sResultJson);

                        foreach (var cli in allHarvestClients.clients)
                        {
                            sAllClients.Add(cli);
                        }

                        if (allHarvestClients.next_page != null)
                        {
                            url = allHarvestClients.links.next.ToString();
                            bHasMore = true;
                        }
                        else
                        {
                            bHasMore = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            // get BC customers
            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/ODataV4/Company('RackPeople ApS')/CustomerDetails") as HttpWebRequest;
                if (webRequestAUTH != null)
                {
                    webRequestAUTH.Method = "GET";
                    webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                    webRequestAUTH.ContentType = "application/json";
                    webRequestAUTH.MediaType = "application/json";
                    webRequestAUTH.Accept = "application/json";

                    webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

                    using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                    {
                        using (var srW = new StreamReader(rW))
                        {
                            var sExportAsJson = srW.ReadToEnd();
                            var sExport = JsonConvert.DeserializeObject<ODataV4Customers>(sExportAsJson);

                            int iCount = 1;
                            GetBCCustomersL.Text = "<br />";

                            foreach (var cust in sExport.value)
                            {
                                if (cust.Name != null)
                                {
                                    if (cust.Name != "")
                                    {
                                        if (cust.No != null)
                                        {
                                            if (cust.No.Length == 8)
                                            {

                                                bool bHarvestClientExists = false;
                                                foreach (Client sResultCustomer in sAllClients)
                                                {
                                                    if (sResultCustomer != null)
                                                    {
                                                        if (sResultCustomer.address == cust.No)
                                                        {
                                                            bHarvestClientExists = true;
                                                            break;
                                                        }
                                                    }
                                                }

                                                if (bHarvestClientExists == false)
                                                {
                                                    string sNewCustId = "n/a";

                                                    // do only several - developer test
                                                    if (iCount < 50000)
                                                    {
                                                        // Create Harvest client
                                                        try
                                                        {
                                                            //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                                                            ServicePointManager.Expect100Continue = true;
                                                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                                   | SecurityProtocolType.Tls11
                                                                   | SecurityProtocolType.Tls12
                                                                   | SecurityProtocolType.Ssl3;

                                                            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                                                            var webRequestCUST = WebRequest.Create("https://api.harvestapp.com/v2/clients") as HttpWebRequest;
                                                            if (webRequestCUST != null)
                                                            {
                                                                webRequestCUST.Method = "POST";
                                                                webRequestCUST.ContentType = "application/json";

                                                                webRequestCUST.Host = "api.harvestapp.com";
                                                                webRequestCUST.Headers["Harvest-Account-ID"] = "1475424";
                                                                webRequestCUST.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                webRequestCUST.UserAgent = "Harvest API Example";

                                                                string sParams = "{\"name\":\"" + cust.Name + "\",\"address\":\"" + cust.No + "\"}";
                                                                
                                                                byte[] bytes = Encoding.UTF8.GetBytes(sParams);
                                                                webRequestCUST.ContentLength = bytes.Length;

                                                                Stream requestStream = webRequestCUST.GetRequestStream();
                                                                requestStream.Write(bytes, 0, bytes.Length);
                                                                requestStream.Close();

                                                                using (var rWCUST = webRequestCUST.GetResponse().GetResponseStream())
                                                                {
                                                                    using (var srWCUST = new StreamReader(rWCUST))
                                                                    {
                                                                        var sExportAsJsonCUST = srWCUST.ReadToEnd();
                                                                        var sExportCUST = JsonConvert.DeserializeObject<HarvestClientPost>(sExportAsJsonCUST);
                                                                        if (sExportCUST.address != null)
                                                                        {
                                                                            if (sExportCUST.address.ToString() == cust.No)
                                                                            {
                                                                                sNewCustId = sExportCUST.id.ToString();

                                                                                // create project for this new client
                                                                                var webRequestPROJECT = WebRequest.Create("https://api.harvestapp.com/v2/projects") as HttpWebRequest;
                                                                                if (webRequestPROJECT != null)
                                                                                {
                                                                                    webRequestPROJECT.Method = "POST";
                                                                                    webRequestPROJECT.ContentType = "application/json";

                                                                                    webRequestPROJECT.Host = "api.harvestapp.com";
                                                                                    webRequestPROJECT.Headers["Harvest-Account-ID"] = "1475424";
                                                                                    webRequestPROJECT.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                    webRequestPROJECT.UserAgent = "Harvest API Example";

                                                                                    string sParamsPROJECT = "{\"client_id\":" + sNewCustId + ",\"name\":\"" + cust.Name + " - Løbende timer\",\"is_billable\":true,\"bill_by\":\"Project\",\"hourly_rate\":100.0,\"budget_by\":\"project\"}";
                                                                                    
                                                                                    byte[] bytesPROJECT = Encoding.UTF8.GetBytes(sParamsPROJECT);
                                                                                    webRequestPROJECT.ContentLength = bytesPROJECT.Length;

                                                                                    Stream requestStreamPROJECT = webRequestPROJECT.GetRequestStream();
                                                                                    requestStreamPROJECT.Write(bytesPROJECT, 0, bytesPROJECT.Length);
                                                                                    requestStreamPROJECT.Close();

                                                                                    using (var rWPROJECT = webRequestPROJECT.GetResponse().GetResponseStream())
                                                                                    {
                                                                                        using (var srWPROJECT = new StreamReader(rWPROJECT))
                                                                                        {
                                                                                            var sExportAsJsonPROJECT = srWPROJECT.ReadToEnd();

                                                                                            try
                                                                                            {
                                                                                                var sExportPROJECT = JsonConvert.DeserializeObject<HarvestProjectPost>(sExportAsJsonPROJECT);

                                                                                                if (sExportPROJECT.id != null)
                                                                                                {
                                                                                                    string sNewProjectId = sExportPROJECT.id.ToString();

                                                                                                    // create Kørsel
                                                                                                    var webRequestTASK1 = WebRequest.Create("https://api.harvestapp.com/v2/projects/" + sNewProjectId + "/task_assignments") as HttpWebRequest;
                                                                                                    if (webRequestTASK1 != null)
                                                                                                    {
                                                                                                        webRequestTASK1.Method = "POST";
                                                                                                        webRequestTASK1.ContentType = "application/json";

                                                                                                        webRequestTASK1.Host = "api.harvestapp.com";
                                                                                                        webRequestTASK1.Headers["Harvest-Account-ID"] = "1475424";
                                                                                                        webRequestTASK1.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                                        webRequestTASK1.UserAgent = "Harvest API Example";

                                                                                                        // Kørsel
                                                                                                        string sParamsTASK1 = "{\"task_id\":18363546,\"is_active\":true,\"billable\":true}";

                                                                                                        byte[] bytesTASK1 = Encoding.UTF8.GetBytes(sParamsTASK1);
                                                                                                        webRequestTASK1.ContentLength = bytesTASK1.Length;

                                                                                                        Stream requestStreamTASK1 = webRequestTASK1.GetRequestStream();
                                                                                                        requestStreamTASK1.Write(bytesTASK1, 0, bytesTASK1.Length);
                                                                                                        requestStreamTASK1.Close();

                                                                                                        using (var rWTASK1 = webRequestTASK1.GetResponse().GetResponseStream())
                                                                                                        {
                                                                                                            using (var srTASK1 = new StreamReader(rWTASK1))
                                                                                                            {
                                                                                                                var sExportAsJsonTASK1 = srTASK1.ReadToEnd();
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    // create Løbende Timer - N1
                                                                                                    var webRequestTASK2 = WebRequest.Create("https://api.harvestapp.com/v2/projects/" + sNewProjectId + "/task_assignments") as HttpWebRequest;
                                                                                                    if (webRequestTASK2 != null)
                                                                                                    {
                                                                                                        webRequestTASK2.Method = "POST";
                                                                                                        webRequestTASK2.ContentType = "application/json";

                                                                                                        webRequestTASK2.Host = "api.harvestapp.com";
                                                                                                        webRequestTASK2.Headers["Harvest-Account-ID"] = "1475424";
                                                                                                        webRequestTASK2.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                                        webRequestTASK2.UserAgent = "Harvest API Example";

                                                                                                        // Løbende Timer - N1
                                                                                                        string sParamsTASK2 = "{\"task_id\":18326258,\"is_active\":true,\"billable\":true}";

                                                                                                        byte[] bytesTASK2 = Encoding.UTF8.GetBytes(sParamsTASK2);
                                                                                                        webRequestTASK2.ContentLength = bytesTASK2.Length;

                                                                                                        Stream requestStreamTASK2 = webRequestTASK2.GetRequestStream();
                                                                                                        requestStreamTASK2.Write(bytesTASK2, 0, bytesTASK2.Length);
                                                                                                        requestStreamTASK2.Close();

                                                                                                        using (var rWTASK2 = webRequestTASK2.GetResponse().GetResponseStream())
                                                                                                        {
                                                                                                            using (var srTASK2 = new StreamReader(rWTASK2))
                                                                                                            {
                                                                                                                var sExportAsJsonTASK2 = srTASK2.ReadToEnd();
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    // create Løbende Timer - N2
                                                                                                    var webRequestTASK3 = WebRequest.Create("https://api.harvestapp.com/v2/projects/" + sNewProjectId + "/task_assignments") as HttpWebRequest;
                                                                                                    if (webRequestTASK3 != null)
                                                                                                    {
                                                                                                        webRequestTASK3.Method = "POST";
                                                                                                        webRequestTASK3.ContentType = "application/json";

                                                                                                        webRequestTASK3.Host = "api.harvestapp.com";
                                                                                                        webRequestTASK3.Headers["Harvest-Account-ID"] = "1475424";
                                                                                                        webRequestTASK3.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                                        webRequestTASK3.UserAgent = "Harvest API Example";

                                                                                                        // Løbende Timer - N2
                                                                                                        string sParamsTASK3 = "{\"task_id\":18363537,\"is_active\":true,\"billable\":true}";

                                                                                                        byte[] bytesTASK3 = Encoding.UTF8.GetBytes(sParamsTASK3);
                                                                                                        webRequestTASK3.ContentLength = bytesTASK3.Length;

                                                                                                        Stream requestStreamTASK3 = webRequestTASK3.GetRequestStream();
                                                                                                        requestStreamTASK3.Write(bytesTASK3, 0, bytesTASK3.Length);
                                                                                                        requestStreamTASK3.Close();

                                                                                                        using (var rWTASK3 = webRequestTASK3.GetResponse().GetResponseStream())
                                                                                                        {
                                                                                                            using (var srTASK3 = new StreamReader(rWTASK3))
                                                                                                            {
                                                                                                                var sExportAsJsonTASK3 = srTASK3.ReadToEnd();
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    // create Løbende Timer - N3
                                                                                                    var webRequestTASK4 = WebRequest.Create("https://api.harvestapp.com/v2/projects/" + sNewProjectId + "/task_assignments") as HttpWebRequest;
                                                                                                    if (webRequestTASK4 != null)
                                                                                                    {
                                                                                                        webRequestTASK4.Method = "POST";
                                                                                                        webRequestTASK4.ContentType = "application/json";

                                                                                                        webRequestTASK4.Host = "api.harvestapp.com";
                                                                                                        webRequestTASK4.Headers["Harvest-Account-ID"] = "1475424";
                                                                                                        webRequestTASK4.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                                        webRequestTASK4.UserAgent = "Harvest API Example";

                                                                                                        // Løbende Timer - N3
                                                                                                        string sParamsTASK4 = "{\"task_id\":18363538,\"is_active\":true,\"billable\":true}";

                                                                                                        byte[] bytesTASK4 = Encoding.UTF8.GetBytes(sParamsTASK4);
                                                                                                        webRequestTASK4.ContentLength = bytesTASK4.Length;

                                                                                                        Stream requestStreamTASK4 = webRequestTASK4.GetRequestStream();
                                                                                                        requestStreamTASK4.Write(bytesTASK4, 0, bytesTASK4.Length);
                                                                                                        requestStreamTASK4.Close();

                                                                                                        using (var rWTASK4 = webRequestTASK4.GetResponse().GetResponseStream())
                                                                                                        {
                                                                                                            using (var srTASK4 = new StreamReader(rWTASK4))
                                                                                                            {
                                                                                                                var sExportAsJsonTASK4 = srTASK4.ReadToEnd();
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    // create Projektledelse
                                                                                                    var webRequestTASK5 = WebRequest.Create("https://api.harvestapp.com/v2/projects/" + sNewProjectId + "/task_assignments") as HttpWebRequest;
                                                                                                    if (webRequestTASK5 != null)
                                                                                                    {
                                                                                                        webRequestTASK5.Method = "POST";
                                                                                                        webRequestTASK5.ContentType = "application/json";

                                                                                                        webRequestTASK5.Host = "api.harvestapp.com";
                                                                                                        webRequestTASK5.Headers["Harvest-Account-ID"] = "1475424";
                                                                                                        webRequestTASK5.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                                        webRequestTASK5.UserAgent = "Harvest API Example";

                                                                                                        // Projektledelse
                                                                                                        string sParamsTASK5 = "{\"task_id\":17019086,\"is_active\":true,\"billable\":true}";

                                                                                                        byte[] bytesTASK5 = Encoding.UTF8.GetBytes(sParamsTASK5);
                                                                                                        webRequestTASK5.ContentLength = bytesTASK5.Length;

                                                                                                        Stream requestStreamTASK5 = webRequestTASK5.GetRequestStream();
                                                                                                        requestStreamTASK5.Write(bytesTASK5, 0, bytesTASK5.Length);
                                                                                                        requestStreamTASK5.Close();

                                                                                                        using (var rWTASK5 = webRequestTASK5.GetResponse().GetResponseStream())
                                                                                                        {
                                                                                                            using (var srTASK5 = new StreamReader(rWTASK5))
                                                                                                            {
                                                                                                                var sExportAsJsonTASK5 = srTASK5.ReadToEnd();
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    // create Support
                                                                                                    var webRequestTASK6 = WebRequest.Create("https://api.harvestapp.com/v2/projects/" + sNewProjectId + "/task_assignments") as HttpWebRequest;
                                                                                                    if (webRequestTASK6 != null)
                                                                                                    {
                                                                                                        webRequestTASK6.Method = "POST";
                                                                                                        webRequestTASK6.ContentType = "application/json";

                                                                                                        webRequestTASK6.Host = "api.harvestapp.com";
                                                                                                        webRequestTASK6.Headers["Harvest-Account-ID"] = "1475424";
                                                                                                        webRequestTASK6.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                                        webRequestTASK6.UserAgent = "Harvest API Example";

                                                                                                        // Support
                                                                                                        string sParamsTASK6 = "{\"task_id\":18497644,\"is_active\":true,\"billable\":true}";

                                                                                                        byte[] bytesTASK6 = Encoding.UTF8.GetBytes(sParamsTASK6);
                                                                                                        webRequestTASK6.ContentLength = bytesTASK6.Length;

                                                                                                        Stream requestStreamTASK6 = webRequestTASK6.GetRequestStream();
                                                                                                        requestStreamTASK6.Write(bytesTASK6, 0, bytesTASK6.Length);
                                                                                                        requestStreamTASK6.Close();

                                                                                                        using (var rWTASK6 = webRequestTASK6.GetResponse().GetResponseStream())
                                                                                                        {
                                                                                                            using (var srTASK6 = new StreamReader(rWTASK6))
                                                                                                            {
                                                                                                                var sExportAsJsonTASK6 = srTASK6.ReadToEnd();
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    // create Timer uden beregning
                                                                                                    var webRequestTASK7 = WebRequest.Create("https://api.harvestapp.com/v2/projects/" + sNewProjectId + "/task_assignments") as HttpWebRequest;
                                                                                                    if (webRequestTASK7 != null)
                                                                                                    {
                                                                                                        webRequestTASK7.Method = "POST";
                                                                                                        webRequestTASK7.ContentType = "application/json";

                                                                                                        webRequestTASK7.Host = "api.harvestapp.com";
                                                                                                        webRequestTASK7.Headers["Harvest-Account-ID"] = "1475424";
                                                                                                        webRequestTASK7.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                                                                                                        webRequestTASK7.UserAgent = "Harvest API Example";

                                                                                                        // Timer uden beregning
                                                                                                        string sParamsTASK7 = "{\"task_id\":18498616,\"is_active\":true,\"billable\":true}";

                                                                                                        byte[] bytesTASK7 = Encoding.UTF8.GetBytes(sParamsTASK7);
                                                                                                        webRequestTASK7.ContentLength = bytesTASK7.Length;

                                                                                                        Stream requestStreamTASK7 = webRequestTASK7.GetRequestStream();
                                                                                                        requestStreamTASK7.Write(bytesTASK7, 0, bytesTASK7.Length);
                                                                                                        requestStreamTASK7.Close();

                                                                                                        using (var rWTASK7 = webRequestTASK7.GetResponse().GetResponseStream())
                                                                                                        {
                                                                                                            using (var srTASK7 = new StreamReader(rWTASK7))
                                                                                                            {
                                                                                                                var sExportAsJsonTASK7 = srTASK7.ReadToEnd();
                                                                                                            }
                                                                                                        }
                                                                                                    }

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
                                                                    }
                                                                }
                                                                webRequestCUST = null;
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            if (ex.ToString().IndexOf("The remote server returned an error: (422) unknown.") != -1)
                                                            {
                                                                sNewCustId = "422";
                                                            }
                                                            //GetBCCustomersL.Text += "<br /><br />" + ex.ToString() + "<br /><br />";
                                                        }
                                                    }

                                                    if (sNewCustId != "422")
                                                    {
                                                        GetBCCustomersL.Text += iCount.ToString().PadLeft(3, '0') + ". " + cust.Name + " (" + cust.No + ") Harvest client/project id: " + sNewCustId + " <br />";
                                                        iCount++;
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }

                            if (GetBCCustomersL.Text == "<br />")
                            {
                                GetBCCustomersL.Text = "<br />No new customers exist in BC.";
                            }
                        }
                    }

                    webRequestAUTH = null;
                }
            }
            catch (Exception ex)
            {
                GetBCCustomersL.Text += ex.ToString();
            }

        }

        protected void HarvestDataB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            PPSep1.Visible = false;
            PPSep2.Visible = false;

            PushDataToNavB.Visible = false;
            AllowInvoicesWithoutLinesCB.Visible = false;
            PushingDataL.Text = "<p style='height:15px;'>&nbsp;</p>";

            string sVATNos = VATNoTB.Text;
            if (sVATNos == "") sVATNos = "n/a";

            // dates "2017-03-01"
            string sStartMonth = StartMonthTB.Text;
            string sStartYear = StartYearTB.Text;
            string sEndMonth = EndMonthTB.Text;
            string sEndYear = EndYearTB.Text;

            DateTime dtStart = DateTime.Now;
            bool bDatesOk = true;
            try
            {
                dtStart = new DateTime(Convert.ToInt32(sStartYear), Convert.ToInt32(sStartMonth), 1);
            }
            catch (Exception ex)
            {
                ex.ToString();
                bDatesOk = false;
            }
            DateTime dtEnd = DateTime.Now;
            try
            {
                dtEnd = new DateTime(Convert.ToInt32(sEndYear), Convert.ToInt32(sEndMonth), 1);
            }
            catch (Exception ex)
            {
                ex.ToString();
                bDatesOk = false;
            }

            if (bDatesOk == true)
            {
                dtEnd = dtEnd.AddMonths(1);
                dtEnd = dtEnd.AddDays(-1);

                string sStartDate = dtStart.Year.ToString().PadLeft(4, '0') + "-" + dtStart.Month.ToString().PadLeft(2, '0') + "-" + dtStart.Day.ToString().PadLeft(2, '0');
                string sEndDate = dtEnd.Year.ToString().PadLeft(4, '0') + "-" + dtEnd.Month.ToString().PadLeft(2, '0') + "-" + dtEnd.Day.ToString().PadLeft(2, '0');

                List<Client> sAllClients = new List<Client>();
                List<Invoice> sAllInvoices = new List<Invoice>();

                // get customers
                try
                {
                    var url = "https://api.harvestapp.com/v2/clients";

                    bool bHasMore = true;
                    while (bHasMore == true)
                    {
                        //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;

                        var httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                        httpRequest.Method = "GET";
                        httpRequest.Host = "api.harvestapp.com";
                        httpRequest.Headers["Harvest-Account-ID"] = "1475424";
                        httpRequest.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                        httpRequest.UserAgent = "Harvest API Example";

                        var httpResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                        {
                            var sResultJson = streamReader.ReadToEnd();

                            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
                            HarvestClients allHarvestClients = Newtonsoft.Json.JsonConvert.DeserializeObject<HarvestClients>(sResultJson);

                            foreach (var cli in allHarvestClients.clients)
                            {
                                sAllClients.Add(cli);
                            }

                            if (allHarvestClients.next_page != null)
                            {
                                url = allHarvestClients.links.next.ToString();
                                bHasMore = true;
                            }
                            else
                            {
                                bHasMore = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                // get invoices
                try
                {
                    var url = "https://api.harvestapp.com/v2/invoices?from=" + sStartDate + "&to=" + sEndDate;

                    bool bHasMore = true;
                    while (bHasMore == true)
                    {
                        //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;

                        var httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                        httpRequest.Method = "GET";
                        httpRequest.Host = "api.harvestapp.com";
                        httpRequest.Headers["Harvest-Account-ID"] = "1475424";
                        httpRequest.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                        httpRequest.UserAgent = "Harvest API Example";

                        var httpResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                        {
                            var sResultJson = streamReader.ReadToEnd();

                            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
                            HarvestInvocies allHarvestInvocies = Newtonsoft.Json.JsonConvert.DeserializeObject<HarvestInvocies>(sResultJson);

                            foreach (var inv in allHarvestInvocies.invoices)
                            {
                                if (inv.state != "draft")
                                {
                                    sAllInvoices.Add(inv);
                                }
                            }

                            if (allHarvestInvocies.next_page != null)
                            {
                                url = allHarvestInvocies.links.next.ToString();
                                bHasMore = true;
                            }
                            else
                            {
                                bHasMore = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                int iInvoicesAllCount = 0;

                // open db connection
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                // show invoices & lines
                int iCustomersCount = 0;
                foreach (Client sResultCustomer in sAllClients)
                {
                    if (sResultCustomer != null)
                    {
                        int iInvoicesCount = 0;
                        foreach (Invoice sResultInvoice in sAllInvoices)
                        {
                            if (sResultInvoice != null)
                            {
                                if (sResultCustomer.id == sResultInvoice.client.id)
                                {
                                    if (IsOrderAlreadyProcessed(sResultInvoice.id.ToString(), dbConn) == false)
                                    {
                                        if (iInvoicesCount == 0)
                                        {
                                            if (iCustomersCount == 0)
                                            {
                                                HarvestDataL.Text += "<table cellpadding='3' cellspacing='3' border='0' width='100%'>";
                                                iCustomersCount++;
                                            }

                                            // customer - show only if invoices exists
                                            HarvestDataL.Text += "<tr>";
                                            HarvestDataL.Text += "<td><a href='javascript:toogleInvoices(\"" + sResultCustomer.id.ToString() + "\");'><b>" + sResultCustomer.name + "</b></a><br/>";
                                            HarvestDataL.Text += "<div style='display: none;' id='cgs_" + sResultCustomer.id.ToString() + "' name='cgs_" + sResultCustomer.id.ToString() + "'>";
                                            HarvestDataL.Text += "Id: " + sResultCustomer.id.ToString() + "<br />";
                                            HarvestDataL.Text += "Invoices#: ###123###<br />";
                                            HarvestDataL.Text += "Address (NAV#): " + sResultCustomer.address.PadLeft(8, '0') + "<br /><br />";
                                            HarvestDataL.Text += "</div>";
                                            HarvestDataL.Text += "</td>";
                                            HarvestDataL.Text += "</tr>";

                                            HarvestDataL.Text += "<tr><td>";
                                            HarvestDataL.Text += "<div style='display: none;' id='cg_" + sResultCustomer.id.ToString() + "' name='cg_" + sResultCustomer.id.ToString() + "'>";
                                        }

                                        iInvoicesCount++;

                                        int iInvoiceLinesCount = 0;
                                        HarvestDataL.Text += "<table border='0' width='100%'>";

                                        HarvestDataL.Text += "<tr style='border-top:1pt solid silver; border-bottom:1pt solid silver;'><td colspan='8'><input type='checkbox' name='inv_" + sResultCustomer.id.ToString() + "_" + sResultInvoice.id.ToString() + "' id='inv_" + sResultCustomer.id.ToString() + "_" + sResultInvoice.id.ToString() + "' value='TL_SELECTED_INVOICE' autocomplete='off' checked />";
                                        HarvestDataL.Text += "&nbsp;Invoice No: " + sResultInvoice.id.ToString() + "&nbsp;";
                                        HarvestDataL.Text += "&nbsp;Issue Date: " + sResultInvoice.issue_date.ToString() + "&nbsp;";
                                        HarvestDataL.Text += "&nbsp;Creator: " + sResultInvoice.creator.name + "&nbsp;";
                                        HarvestDataL.Text += "</td></tr>";

                                        HarvestDataL.Text += "<tr style='height:5px'><td style='height:5px' colspan='8'>&nbsp;</td></tr>";

                                        HarvestDataL.Text += "<tr style='border-bottom:1pt solid silver;'>";
                                        HarvestDataL.Text += "<td><b>#</b></td>";
                                        HarvestDataL.Text += "<td><b>Product #</b></td>";
                                        HarvestDataL.Text += "<td><b>Project Name</b></td>";
                                        HarvestDataL.Text += "<td><b>Project Description D</b></td>";
                                        HarvestDataL.Text += "<td><b>Description</b></td>";
                                        HarvestDataL.Text += "<td><b>Quantity</b></td>";
                                        HarvestDataL.Text += "<td><b>Unit Price</b></td>";
                                        HarvestDataL.Text += "<td><b>Amount</b></td>";
                                        HarvestDataL.Text += "</tr>";

                                        foreach (LineItem sResultInvoiceLine in sResultInvoice.line_items)
                                        {
                                            if (sResultInvoiceLine != null)
                                            {
                                                string sKind1 = "";
                                                string sKind2 = sResultInvoiceLine.kind;
                                                if (sResultInvoiceLine.kind != "")
                                                {
                                                    try
                                                    {
                                                        sKind1 = sResultInvoiceLine.kind.Substring(0, sResultInvoiceLine.kind.IndexOf('-')).Trim();
                                                        sKind2 = sResultInvoiceLine.kind.Substring(sResultInvoiceLine.kind.IndexOf('-') + 1).Trim();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                    }
                                                }

                                                HarvestDataL.Text += "<tr>";
                                                HarvestDataL.Text += "<td>" + sResultInvoiceLine.id.ToString() + "&nbsp;</td>";
                                                HarvestDataL.Text += "<td>" + sKind1 + "&nbsp;</td>";
                                                HarvestDataL.Text += "<td>" + sKind2 + "&nbsp;</td>";
                                                string sProjectName = "n/a";
                                                if (sResultInvoiceLine.project != null)
                                                {
                                                    if (sResultInvoiceLine.project.name != null)
                                                    {
                                                        sProjectName = sResultInvoiceLine.project.name;
                                                    }
                                                }
                                                HarvestDataL.Text += "<td>" + sProjectName + "&nbsp;</td>";
                                                HarvestDataL.Text += "<td>" + sResultInvoiceLine.description + "&nbsp;</td>";
                                                HarvestDataL.Text += "<td>" + sResultInvoiceLine.quantity.ToString("N") + "&nbsp;</td>";
                                                HarvestDataL.Text += "<td>" + sResultInvoiceLine.unit_price.ToString("N") + "&nbsp;</td>";
                                                HarvestDataL.Text += "<td>" + sResultInvoiceLine.amount.ToString("N") + "&nbsp;</td>";
                                                HarvestDataL.Text += "</tr>";

                                                iInvoiceLinesCount++;
                                            }
                                        }

                                        if (iInvoiceLinesCount == 0)
                                        {
                                            HarvestDataL.Text += "<tr><td colspan='5'>&nbsp;<i>No invoice lines found</i>&nbsp;</td></tr>";
                                        }
                                        HarvestDataL.Text += "<tr height='15px'><td colspan='5' height='15px'>&nbsp;</td></tr>";
                                        HarvestDataL.Text += "</table>";
                                    }
                                }
                            }
                        }

                        HarvestDataL.Text = HarvestDataL.Text.Replace("###123###", iInvoicesCount.ToString());
                        
                        // all invoices count
                        iInvoicesAllCount = iInvoicesAllCount + iInvoicesCount;

                        if (iInvoicesCount > 0)
                        {
                            PPSep1.Visible = true;
                            PPSep2.Visible = true;
                            AllowInvoicesWithoutLinesCB.Visible = true;
                            PushDataToNavB.Visible = true;
                            HarvestDataL.Text += "</div>";
                            HarvestDataL.Text += "</td></tr>";
                        }
                    }
                }

                dbConn.Close();

                if (iInvoicesAllCount == 0)
                {
                    HarvestDataL.Text += "<tr><td colspan='5'>&nbsp;<i>No invoices found</i>&nbsp;</td></tr>";
                }
                else
                {
                    HarvestDataL.Text = "<tr><td colspan='5'><i>Period: " + sStartDate + " - " + sEndDate + "<br />" + iInvoicesAllCount.ToString() + " invoice(s) found</i>&nbsp;</td></tr>" + HarvestDataL.Text + "<br /><br />";
                }

                if (iCustomersCount > 0)
                {
                    HarvestDataL.Text += "</table>";
                }

            }
            else
            {
                HarvestDataL.Text = "Please add dates in correct format.";
            }
        }

        private string GetItemId(string sItemName)
        {
            string sResult = "n/a";

            // get itemid and lineobjectnumber
            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/items?$filter=number eq '" + sItemName + "'") as HttpWebRequest;
                if (webRequestAUTH != null)
                {
                    webRequestAUTH.Method = "GET";
                    webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                    webRequestAUTH.ContentType = "application/json";
                    webRequestAUTH.MediaType = "application/json";
                    webRequestAUTH.Accept = "application/json";

                    webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

                    using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                    {
                        using (var srW = new StreamReader(rW))
                        {
                            var sExportAsJson = srW.ReadToEnd();
                            var sExport = JsonConvert.DeserializeObject<GetItems>(sExportAsJson);
                            foreach (var it in sExport.value)
                            {
                                sResult = it.id;
                                break;
                            }
                        }
                    }

                    webRequestAUTH = null;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = "n/a";
            }

            return sResult;
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

            // open db connection
            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            // get customers & invoices

            string sVATNos = VATNoTB.Text;
            if (sVATNos == "") sVATNos = "n/a";

            // dates "2017-03-01"
            string sStartMonth = StartMonthTB.Text;
            string sStartYear = StartYearTB.Text;
            string sEndMonth = EndMonthTB.Text;
            string sEndYear = EndYearTB.Text;

            DateTime dtStart = DateTime.Now;
            bool bDatesOk = true;
            try
            {
                dtStart = new DateTime(Convert.ToInt32(sStartYear), Convert.ToInt32(sStartMonth), 1);
            }
            catch (Exception ex)
            {
                ex.ToString();
                bDatesOk = false;
            }
            DateTime dtEnd = DateTime.Now;
            try
            {
                dtEnd = new DateTime(Convert.ToInt32(sEndYear), Convert.ToInt32(sEndMonth), 1);
            }
            catch (Exception ex)
            {
                ex.ToString();
                bDatesOk = false;
            }

            if (bDatesOk == true)
            {
                dtEnd = dtEnd.AddMonths(1);
                dtEnd = dtEnd.AddDays(-1);

                string sStartDate = dtStart.Year.ToString().PadLeft(4, '0') + "-" + dtStart.Month.ToString().PadLeft(2, '0') + "-" + dtStart.Day.ToString().PadLeft(2, '0');
                string sEndDate = dtEnd.Year.ToString().PadLeft(4, '0') + "-" + dtEnd.Month.ToString().PadLeft(2, '0') + "-" + dtEnd.Day.ToString().PadLeft(2, '0');

                List<Client> sAllClients = new List<Client>();
                List<Invoice> sAllInvoices = new List<Invoice>();

                // get customers
                try
                {
                    var url = "https://api.harvestapp.com/v2/clients";

                    bool bHasMore = true;
                    while (bHasMore == true)
                    {
                        //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;

                        var httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                        httpRequest.Method = "GET";
                        httpRequest.Host = "api.harvestapp.com";
                        httpRequest.Headers["Harvest-Account-ID"] = "1475424";
                        httpRequest.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                        httpRequest.UserAgent = "Harvest API Example";

                        var httpResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                        {
                            var sResultJson = streamReader.ReadToEnd();

                            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
                            HarvestClients allHarvestClients = Newtonsoft.Json.JsonConvert.DeserializeObject<HarvestClients>(sResultJson);

                            foreach (var cli in allHarvestClients.clients)
                            {
                                sAllClients.Add(cli);
                            }

                            if (allHarvestClients.next_page != null)
                            {
                                url = allHarvestClients.links.next.ToString();
                                bHasMore = true;
                            }
                            else
                            {
                                bHasMore = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                // get invoices
                try
                {
                    var url = "https://api.harvestapp.com/v2/invoices?from=" + sStartDate + "&to=" + sEndDate;

                    bool bHasMore = true;
                    while (bHasMore == true)
                    {
                        //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;

                        var httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                        httpRequest.Method = "GET";
                        httpRequest.Host = "api.harvestapp.com";
                        httpRequest.Headers["Harvest-Account-ID"] = "1475424";
                        httpRequest.Headers["Authorization"] = "Bearer 2986822.pt.yW1hq4HFMNZa1WSgSr-PHVe5lhROrpNLVhhZbI6k_iVqRc2jJSMes_-Kw_8cH5jjQLCqamoWFCqxOxt-0q-iaw";
                        httpRequest.UserAgent = "Harvest API Example";

                        var httpResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                        {
                            var sResultJson = streamReader.ReadToEnd();

                            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
                            HarvestInvocies allHarvestInvocies = Newtonsoft.Json.JsonConvert.DeserializeObject<HarvestInvocies>(sResultJson);

                            foreach (var inv in allHarvestInvocies.invoices)
                            {
                                if (inv.state != "draft")
                                {
                                    sAllInvoices.Add(inv);
                                }
                            }

                            if (allHarvestInvocies.next_page != null)
                            {
                                url = allHarvestInvocies.links.next.ToString();
                                bHasMore = true;
                            }
                            else
                            {
                                bHasMore = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                // show invoices & lines
                foreach (Client sResultCustomer in sAllClients)
                {
                    if (sResultCustomer != null)
                    {
                        bool bProblematicCustomer = false;
                        if (sResultCustomer.address == null)
                        {
                            if (sResultCustomer.address == "")
                            {
                                bProblematicCustomer = true;
                            }
                        }

                        // searh if this customer already exists
                        bool bCustomerCreatedInNav = false;
                        if (bProblematicCustomer == false)
                        {
                            if (DoesCustomerExists(sResultCustomer.address.PadLeft(8, '0')) == "n/a")
                            {
                                if (sResultCustomer.address != "")
                                {
                                    if (sResultCustomer.address.Length > 5)
                                    {
                                        try
                                        {
                                            string sNewGuid = Guid.NewGuid().ToString();
                                            var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/customers") as HttpWebRequest;
                                            if (webRequestAUTH != null)
                                            {
                                                webRequestAUTH.Method = "POST";
                                                webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                                                webRequestAUTH.ContentType = "application/json";
                                                webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;
                                                webRequestAUTH.Headers["If-Match"] = "*";

                                                // NAV restriction for 50 chars max
                                                string sCustName = sResultCustomer.name;
                                                if (sResultCustomer.name.Length > 50)
                                                {
                                                    sCustName = sResultCustomer.name.Substring(0, 50);
                                                }

                                                string jsonToSend = "{";
                                                jsonToSend += "\"displayName\": \"" + sCustName + "\",";
                                                jsonToSend += "\"number\": \"" + sResultCustomer.address.PadLeft(8, '0') + "\",";
                                                jsonToSend += "\"type\": \"Company\",";
                                                jsonToSend += "\"addressLine1\": \"" + sResultCustomer.address.PadLeft(8, '0') + "\",";
                                                jsonToSend += "\"addressLine2\": \"\",";
                                                jsonToSend += "\"city\": \"\",";
                                                jsonToSend += "\"state\": \"\",";
                                                jsonToSend += "\"country\": \"\",";
                                                jsonToSend += "\"postalCode\": \"\",";
                                                jsonToSend += "\"phoneNumber\": \"\",";
                                                jsonToSend += "\"email\": \"\",";
                                                jsonToSend += "\"website\": \"\",";
                                                jsonToSend += "\"taxLiable\": true,";
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
                                                    }
                                                }

                                                webRequestAUTH = null;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.ToString();
                                            bCustomerCreatedInNav = false;
                                        }
                                    }
                                }
                            }
                        }

                        string sBCCuromerData = DoesCustomerExists(sResultCustomer.address.PadLeft(8, '0'));
                        if (sBCCuromerData == "n/a")
                        {
                            sBCCuromerData = "n/aђn/a";
                        }
                        string sCustomerVATNo = sBCCuromerData.Split('ђ')[0];
                        string sCustomerVATId = sBCCuromerData.Split('ђ')[1];

                        if ((sCustomerVATNo != "n/a") || (bCustomerCreatedInNav == true) || (bProblematicCustomer == true))
                        {
                            foreach (Invoice sResultInvoice in sAllInvoices)
                            {
                                if (sResultInvoice != null)
                                {
                                    if (sResultCustomer.id == sResultInvoice.client.id)
                                    {
                                        if (bProblematicCustomer == true)
                                        {
                                            sProblematicCustomers += sResultCustomer + "<br />";
                                            break;
                                        }

                                        // search if invoice already processed
                                        if (IsOrderAlreadyProcessed(sResultInvoice.id.ToString(), dbConn) == false)
                                        {
                                            // get checkbox state
                                            string sInvoiceChosen = "n/a";
                                            try
                                            {
                                                sInvoiceChosen = FormPageVars["inv_" + sResultCustomer.id.ToString() + "_" + sResultInvoice.id.ToString()];
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
                                                int iInvoiceLinesNumber = sResultInvoice.line_items.Count;

                                                // create new invoice
                                                if (iInvoiceLinesNumber == 0)
                                                {
                                                    if (AllowInvoicesWithoutLinesCB.Checked == true)
                                                    {
                                                        // create order first and create empty order lines
                                                        PostSalesInvoice order = new PostSalesInvoice();

                                                        order.customerNumber = sResultCustomer.address.PadLeft(8, '0');
                                                        order.billToCustomerNumber = sResultCustomer.address.PadLeft(8, '0');
                                                        order.customerId = sCustomerVATId;
                                                        order.billToCustomerId = sCustomerVATId;

                                                        // date YYYY-MM-DD (2022-03-03)
                                                        string sTLInvoiceDate = sResultInvoice.issue_date;
                                                        if (sTLInvoiceDate.Length >= 10)
                                                        {
                                                            string sYYYY = sTLInvoiceDate.Substring(0, 4);
                                                            string sMM = sTLInvoiceDate.Substring(5, 2);
                                                            string sDD = sTLInvoiceDate.Substring(8, 2);
                                                            try
                                                            {
                                                                DateTime dtOrderDate = new DateTime(Convert.ToInt32(sYYYY), Convert.ToInt32(sMM), Convert.ToInt32(sDD));
                                                                order.invoiceDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                                                                order.postingDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ex.ToString();
                                                            }
                                                        }

                                                        // create invoice now
                                                        string sNewInvoiceId = "n/a";
                                                        try
                                                        {
                                                            //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                                                            ServicePointManager.Expect100Continue = true;
                                                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                                   | SecurityProtocolType.Tls11
                                                                   | SecurityProtocolType.Tls12
                                                                   | SecurityProtocolType.Ssl3;

                                                            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                                                            var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v1.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/salesInvoices") as HttpWebRequest;
                                                            if (webRequestAUTH != null)
                                                            {
                                                                webRequestAUTH.Method = "POST";
                                                                webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                                                                webRequestAUTH.ContentType = "application/json";
                                                                webRequestAUTH.MediaType = "application/json";
                                                                webRequestAUTH.Accept = "application/json";

                                                                webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

                                                                string sParams = "{\"externalDocumentNumber\": \"\", \"invoiceDate\": \"" + order.invoiceDate + "\", \"postingDate\": \"" + order.postingDate + "\", \"customerId\": \"" + order.customerId + "\", \"customerNumber\": \"" + order.customerNumber + "\", \"billToCustomerId\": \"" + order.billToCustomerId + "\", \"billToCustomerNumber\": \"" + order.billToCustomerNumber + "\"}";
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
                                                                        var sExport = JsonConvert.DeserializeObject<PostSalesInvoiceResponse>(sExportAsJson);
                                                                        if (sExport.id != null)
                                                                        {
                                                                            if (sExport.id != "")
                                                                            {
                                                                                sNewInvoiceId = sExport.id;
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
                                                            sNewInvoiceId = "n/a";
                                                        }

                                                        // processed invoices count
                                                        iInvoiceNumber++;

                                                        string sCurrentDateTime = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                                                        sCurrentDateTime += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                                                        sCurrentDateTime += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                                                        sCurrentDateTime += DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":";
                                                        sCurrentDateTime += DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":";
                                                        sCurrentDateTime += DateTime.Now.Second.ToString().PadLeft(2, '0');

                                                        string sSql = "INSERT INTO [RPNAVConnect].[dbo].[Log] ([refid] ,[result] ,[source] ,[datestamp] ,[description]) ";
                                                        sSql += "VALUES ('" + sResultInvoice.id.ToString() + "', 'Pushed', 'TimeLog', '" + sCurrentDateTime + "', 'No Invoice Lines')";
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
                                                    PostSalesInvoice order = new PostSalesInvoice();

                                                    order.customerNumber = sResultCustomer.address.PadLeft(8, '0');
                                                    order.billToCustomerNumber = sResultCustomer.address.PadLeft(8, '0');
                                                    order.customerId = sCustomerVATId;
                                                    order.billToCustomerId = sCustomerVATId;
                                                    order.invoiceDate = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0');
                                                    order.postingDate = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0');

                                                    // date YYYY-MM-DD
                                                    string sTLInvoiceDate = sResultInvoice.issue_date;
                                                    if (sTLInvoiceDate.Length >= 10)
                                                    {
                                                        string sYYYY = sTLInvoiceDate.Substring(0, 4);
                                                        string sMM = sTLInvoiceDate.Substring(5, 2);
                                                        string sDD = sTLInvoiceDate.Substring(8, 2);
                                                        try
                                                        {
                                                            DateTime dtOrderDate = new DateTime(Convert.ToInt32(sYYYY), Convert.ToInt32(sMM), Convert.ToInt32(sDD));
                                                            order.invoiceDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                                                            order.postingDate = dtOrderDate.Year.ToString().PadLeft(4, '0') + "-" + dtOrderDate.Month.ToString().PadLeft(2, '0') + "-" + dtOrderDate.Day.ToString().PadLeft(2, '0');
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            ex.ToString();
                                                        }
                                                    }

                                                    // create invoice now
                                                    string sNewInvoiceId = "n/a";
                                                    try
                                                    {
                                                        //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                                                        ServicePointManager.Expect100Continue = true;
                                                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                               | SecurityProtocolType.Tls11
                                                               | SecurityProtocolType.Tls12
                                                               | SecurityProtocolType.Ssl3;

                                                        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                                                        var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v1.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/salesInvoices") as HttpWebRequest;
                                                        if (webRequestAUTH != null)
                                                        {
                                                            webRequestAUTH.Method = "POST";
                                                            webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                                                            webRequestAUTH.ContentType = "application/json";
                                                            webRequestAUTH.MediaType = "application/json";
                                                            webRequestAUTH.Accept = "application/json";

                                                            webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

                                                            string sParams = "{\"externalDocumentNumber\": \"\", \"invoiceDate\": \"" + order.invoiceDate + "\", \"postingDate\": \"" + order.postingDate + "\", \"customerId\": \"" + order.customerId + "\", \"customerNumber\": \"" + order.customerNumber + "\", \"billToCustomerId\": \"" + order.billToCustomerId + "\", \"billToCustomerNumber\": \"" + order.billToCustomerNumber + "\"}";
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
                                                                    var sExport = JsonConvert.DeserializeObject<PostSalesInvoiceResponse>(sExportAsJson);
                                                                    if (sExport.id != null)
                                                                    {
                                                                        if (sExport.id != "")
                                                                        {
                                                                            sNewInvoiceId = sExport.id;
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
                                                        sNewInvoiceId = "n/a";
                                                    }

                                                    // prepare space for invocie lines
                                                    List<PostSalesInvoiceLine> InvoiceLinesList = new List<PostSalesInvoiceLine>();
                                                    int iInvoiceLinesCount = 0;

                                                    // processed invoices count
                                                    iInvoiceNumber++;

                                                    foreach (LineItem sResultInvoiceLine in sResultInvoice.line_items)
                                                    {
                                                        if (sResultInvoiceLine != null)
                                                        {
                                                            PostSalesInvoiceLine invoiceLine = new PostSalesInvoiceLine();

                                                            invoiceLine.lineObjectNumber = "1010.000"; // 100
                                                            invoiceLine.itemId = GetItemId("1010.000");
                                                            invoiceLine.lineType = "Item";
                                                            invoiceLine.Document_No = "";

                                                            // type
                                                            if (sResultInvoiceLine.description.ToLower().IndexOf("kørsel") != -1)
                                                            {
                                                                invoiceLine.lineObjectNumber = "1010.025"; // 500
                                                                invoiceLine.itemId = GetItemId("1010.025");
                                                            }

                                                            string sKind1 = "";
                                                            string sKind2 = sResultInvoiceLine.kind;
                                                            if (sResultInvoiceLine.kind != "")
                                                            {
                                                                sKind1 = sResultInvoiceLine.kind.Substring(0, sResultInvoiceLine.kind.IndexOf('-')).Trim();
                                                                sKind2 = sResultInvoiceLine.kind.Substring(sResultInvoiceLine.kind.IndexOf('-') + 1).Trim();
                                                            }

                                                            string sProductNo = "";
                                                            try
                                                            {
                                                                sProductNo = sKind1.ToLower();

                                                                if (sProductNo == "1010.000")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1010.000"; // 150
                                                                    invoiceLine.itemId = GetItemId("1010.000");
                                                                }

                                                                if (sProductNo == "1010.005")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1010.005"; // 150
                                                                    invoiceLine.itemId = GetItemId("1010.005");
                                                                }

                                                                if (sProductNo == "1010.010")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1010.010"; // 150
                                                                    invoiceLine.itemId = GetItemId("1010.010");
                                                                }

                                                                if (sProductNo == "1010.015")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1010.015"; // 150
                                                                    invoiceLine.itemId = GetItemId("1010.015");
                                                                }

                                                                if (sProductNo == "1010.020")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1010.020"; // 150
                                                                    invoiceLine.itemId = GetItemId("1010.020");
                                                                }

                                                                if (sProductNo == "1010.025")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1010.025"; // 150
                                                                    invoiceLine.itemId = GetItemId("1010.025");
                                                                }

                                                                if (sProductNo == "1030.000")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.000"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.000");
                                                                }

                                                                if (sProductNo == "1030.005")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.005"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.005");
                                                                }

                                                                if (sProductNo == "1030.020")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.020"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.020");
                                                                }

                                                                if (sProductNo == "1030.025")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.025"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.025");
                                                                }

                                                                if (sProductNo == "1030.030")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.030"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.030");
                                                                }

                                                                if (sProductNo == "1030.035")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.035"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.035");
                                                                }

                                                                if (sProductNo == "1030.040")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.040"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.040");
                                                                }

                                                                if (sProductNo == "1030.045")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1030.045"; // 150
                                                                    invoiceLine.itemId = GetItemId("1030.045");
                                                                }

                                                                if (sProductNo == "1040.000")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1040.000"; // 150
                                                                    invoiceLine.itemId = GetItemId("1040.000");
                                                                }

                                                                if (sProductNo == "1050.000")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1050.000"; // 150
                                                                    invoiceLine.itemId = GetItemId("1050.000");
                                                                }

                                                                if (sProductNo == "1050.005")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1050.005"; // 150
                                                                    invoiceLine.itemId = GetItemId("1050.005");
                                                                }

                                                                if (sProductNo == "1050.030")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1050.030"; // 600
                                                                    invoiceLine.itemId = GetItemId("1050.030");
                                                                }

                                                                if (sProductNo == "1050.035")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1050.035"; // 600
                                                                    invoiceLine.itemId = GetItemId("1050.035");
                                                                }

                                                                if (sProductNo == "1050.040")
                                                                {
                                                                    invoiceLine.lineObjectNumber = "1050.040"; // 605
                                                                    invoiceLine.itemId = GetItemId("1050.040");
                                                                }

                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ex.ToString();
                                                            }

                                                            string sProjectName = sKind2;

                                                            // quantity and price
                                                            try
                                                            {
                                                                invoiceLine.quantity = Convert.ToDecimal(sResultInvoiceLine.quantity);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ex.ToString();
                                                            }
                                                            try
                                                            {
                                                                invoiceLine.unitPrice = Convert.ToDecimal(sResultInvoiceLine.unit_price);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ex.ToString();
                                                            }

                                                            // date & description
                                                            // date YYYY-MM-DD (2022-03-03)
                                                            string sOrderLineDate = sResultInvoice.issue_date.Substring(8, 2) + "-";
                                                            sOrderLineDate += sResultInvoice.issue_date.Substring(5, 2) + "-";
                                                            sOrderLineDate += sResultInvoice.issue_date.Substring(0, 4);
                                                            string sLineDescription = "(" + sOrderLineDate + ") " + sResultInvoiceLine.description;

                                                            if (sLineDescription.Length <= 50)
                                                            {
                                                                PostSalesInvoiceLine extraLine = new PostSalesInvoiceLine();

                                                                extraLine.itemId = "";
                                                                extraLine.lineType = "Item";
                                                                extraLine.Document_No = "";

                                                                extraLine.lineObjectNumber = "";

                                                                // quantity and price
                                                                extraLine.quantity = 0;
                                                                extraLine.unitPrice = 0;

                                                                // extra line
                                                                extraLine.description = sProjectName;
                                                                if (extraLine.description.Length > 50)
                                                                {
                                                                    extraLine.description = extraLine.description.Substring(0, 50);
                                                                }

                                                                // add extra line
                                                                InvoiceLinesList.Add(extraLine);

                                                                // count added lines
                                                                iInvoiceLinesCount++;

                                                                // description
                                                                invoiceLine.description = sLineDescription;

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
                                                                        PostSalesInvoiceLine extraLine = new PostSalesInvoiceLine();

                                                                        extraLine.itemId = "";
                                                                        extraLine.lineType = "Item";
                                                                        extraLine.Document_No = "";

                                                                        extraLine.lineObjectNumber = "";

                                                                        // quantity and price
                                                                        extraLine.quantity = 0;
                                                                        extraLine.unitPrice = 0;

                                                                        // extra line
                                                                        extraLine.description = sProjectName;
                                                                        if (extraLine.description.Length > 50)
                                                                        {
                                                                            extraLine.description = extraLine.description.Substring(0, 50);
                                                                        }

                                                                        // add extra line
                                                                        InvoiceLinesList.Add(extraLine);

                                                                        // count added lines
                                                                        iInvoiceLinesCount++;

                                                                        // include first 50 chars in the current line
                                                                        invoiceLine.description = item.Value;

                                                                        // add invoice line
                                                                        InvoiceLinesList.Add(invoiceLine);

                                                                        // count added lines
                                                                        iInvoiceLinesCount++;
                                                                    }
                                                                    else
                                                                    {
                                                                        PostSalesInvoiceLine extraLine = new PostSalesInvoiceLine();

                                                                        extraLine.itemId = "";
                                                                        extraLine.lineType = "Item";
                                                                        extraLine.Document_No = "";

                                                                        extraLine.lineObjectNumber = "";

                                                                        // quantity and price
                                                                        extraLine.quantity = 0;
                                                                        extraLine.unitPrice = 0;

                                                                        // extra line
                                                                        extraLine.description = item.Value;

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

                                                    //if there was lines update them into order
                                                    if (iInvoiceLinesCount > 0)
                                                    {
                                                        order.SalesLines = new PostSalesInvoiceLine[iInvoiceLinesCount];
                                                        for (int i = 0; i < iInvoiceLinesCount; i++)
                                                        {
                                                            order.SalesLines[i] = new PostSalesInvoiceLine();
                                                        }

                                                        int iCount = 0;
                                                        foreach (PostSalesInvoiceLine sil in InvoiceLinesList)
                                                        {
                                                            order.SalesLines[iCount].itemId = sil.itemId;
                                                            order.SalesLines[iCount].lineType = sil.lineType;
                                                            order.SalesLines[iCount].lineObjectNumber = sil.lineObjectNumber;
                                                            order.SalesLines[iCount].description = sil.description;
                                                            order.SalesLines[iCount].unitPrice = sil.unitPrice;
                                                            order.SalesLines[iCount].quantity = sil.quantity;
                                                            iCount++;
                                                        }

                                                        // post invoice lines now
                                                        if (sNewInvoiceId != "n/a")
                                                        {
                                                            foreach (var ord in order.SalesLines)
                                                            {
                                                                string sNewInvoiceLineId = "n/a";
                                                                try
                                                                {
                                                                    //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                                                                    ServicePointManager.Expect100Continue = true;
                                                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                                           | SecurityProtocolType.Tls11
                                                                           | SecurityProtocolType.Tls12
                                                                           | SecurityProtocolType.Ssl3;

                                                                    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                                                                    var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/salesInvoices(" + sNewInvoiceId + ")/salesInvoiceLines") as HttpWebRequest;
                                                                    if (webRequestAUTH != null)
                                                                    {
                                                                        webRequestAUTH.Method = "POST";
                                                                        webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                                                                        webRequestAUTH.ContentType = "application/json";
                                                                        webRequestAUTH.MediaType = "application/json";
                                                                        webRequestAUTH.Accept = "application/json";

                                                                        webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

                                                                        string sParams = "{\"itemId\": \"" + ord.itemId + "\", \"lineType\": \"" + ord.lineType + "\", \"lineObjectNumber\": \"" + ord.lineObjectNumber + "\", \"description\": \"" + ord.description + "\", \"unitPrice\": " + ord.unitPrice + ", \"quantity\": " + ord.quantity + "}";
                                                                        if (ord.itemId == "")
                                                                        {
                                                                            sParams = "{\"description\": \"" + ord.description + "\"}";
                                                                        }
                                                                        
                                                                        var data = Encoding.UTF8.GetBytes(sParams);
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
                                                                                var sExport = JsonConvert.DeserializeObject<PostSalesInvoiceLineResponse>(sExportAsJson);
                                                                                if (sExport.id != null)
                                                                                {
                                                                                    if (sExport.id != "")
                                                                                    {
                                                                                        sNewInvoiceLineId = sExport.id;
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
                                                                    sNewInvoiceLineId = "n/a";
                                                                }
                                                            }
                                                        }

                                                        iInvoiceAllLinesCount += iInvoiceLinesCount;

                                                        string sCurrentDateTime = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                                                        sCurrentDateTime += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                                                        sCurrentDateTime += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                                                        sCurrentDateTime += DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":";
                                                        sCurrentDateTime += DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":";
                                                        sCurrentDateTime += DateTime.Now.Second.ToString().PadLeft(2, '0');

                                                        string sSql = "INSERT INTO [RPNAVConnect].[dbo].[Log] ([refid] ,[result] ,[source] ,[datestamp] ,[description]) ";
                                                        sSql += "VALUES ('" + sResultInvoice.id.ToString() + "', 'Pushed', 'TimeLog', '" + sCurrentDateTime + "', '" + iInvoiceLinesCount.ToString() + " invoice lines')";
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
                                                        sSql += "VALUES ('" + sResultInvoice.id.ToString() + "', 'Pushed', 'TimeLog', '" + sCurrentDateTime + "', 'No invoice lines')";
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

            dbConn.Close();

            // show message
            PushingDataL.Text = "Total " + iInvoiceNumber.ToString() + " fakturarer herunder " + iInvoiceAllLinesCount.ToString() + " linjer er skubbet til Dynamics 365 Business Central.</br ><br />";
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

        private string DoesCustomerExists(string filter)
        {
            string sResult = "n/a";

            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/customers?$filter=number eq '" + filter + "'") as HttpWebRequest;
                if (webRequestAUTH != null)
                {
                    webRequestAUTH.Method = "GET";
                    webRequestAUTH.Host = "api.businesscentral.dynamics.com";
                    webRequestAUTH.ContentType = "application/json";
                    webRequestAUTH.MediaType = "application/json";
                    webRequestAUTH.Accept = "application/json";

                    webRequestAUTH.Headers["Authorization"] = "Bearer " + sBCToken;

                    using (var rW = webRequestAUTH.GetResponse().GetResponseStream())
                    {
                        using (var srW = new StreamReader(rW))
                        {
                            var sExportAsJson = srW.ReadToEnd();
                            var sExport = JsonConvert.DeserializeObject<BCCustomers>(sExportAsJson);

                            int iCount = 1;
                            foreach (var cust in sExport.value)
                            {
                                sResult = cust.number + "ђ" + cust.id;
                                break;

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

            return sResult;
        }

        protected void DeleteMarkedInvoicesB_Click(object sender, EventArgs e)
        {

        }

    }
}