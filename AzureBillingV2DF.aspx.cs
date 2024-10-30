using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using System.Threading;
using System.Globalization;
using System.Reflection;

/*
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Billing;
using Microsoft.Azure.Management.Billing.Models;
*/

using Microsoft.Store.PartnerCenter;
using Microsoft.Store.PartnerCenter.Models;
using Microsoft.Store.PartnerCenter.Models.Query;
using Microsoft.Store.PartnerCenter.Models.Invoices;
using Microsoft.Store.PartnerCenter.Extensions;

using System.Xml;
using System.Text;

using RPNAVConnect.NAVCustomersWS;
using RPNAVConnect.NAVOrdersWS;
using System.Net;

using System.Text.RegularExpressions;

using System.Data.OleDb;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Excel;
using System.Text.Json.Serialization;
using Microsoft.Identity.Client;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using System.IO.Compression;
using System.Net.Http;
using RPNAVConnect;
using Microsoft.Graph;
using Microsoft.Store.PartnerCenter.Models.Entitlements;
using Microsoft.Store.PartnerCenter.Models.Products;
using System.Diagnostics.Metrics;
using System.Net.PeerToPeer;
using System.Security.Policy;

namespace RPNAVConnect
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class MPCData
    {
        public string PartnerId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerDomainName { get; set; }
        public string CustomerCountry { get; set; }
        public string InvoiceNumber { get; set; }
        public string MpnId { get; set; }
        public string Tier2MpnId { get; set; }
        public string OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string ProductId { get; set; }
        public string SkuId { get; set; }
        public string AvailabilityId { get; set; }
        public string SkuName { get; set; }
        public string ProductName { get; set; }
        public string ChargeType { get; set; }
        public double? UnitPrice { get; set; }
        public double? Quantity { get; set; }
        public double? Subtotal { get; set; }
        public double? TaxTotal { get; set; }
        public double? Total { get; set; }
        public string Currency { get; set; }
        public string PriceAdjustmentDescription { get; set; }
        public string PublisherName { get; set; }
        public string PublisherId { get; set; }
        public string SubscriptionDescription { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime? ChargeStartDate { get; set; }
        public DateTime? ChargeEndDate { get; set; }
        public string TermAndBillingCycle { get; set; }
        public double? EffectiveUnitPrice { get; set; }
        public string UnitType { get; set; }
        public string AlternateId { get; set; }
        public double? BillableQuantity { get; set; }
        public string BillingFrequency { get; set; }
        public string PricingCurrency { get; set; }
        public double? PCToBCExchangeRate { get; set; }
        public DateTime? PCToBCExchangeRateDate { get; set; }
        public string MeterDescription { get; set; }
        public string ReservationOrderId { get; set; }
        public string CreditReasonCode { get; set; }
        public string ReferenceId { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public List<object> ProductQualifiers { get; set; }
        public string PromotionId { get; set; }
        public string ProductCategory { get; set; }

        public string PartnerName { get; set; }
        public string UsageDate { get; set; }
        public string MeterType { get; set; }
        public string MeterCategory { get; set; }
        public string MeterId { get; set; }
        public string MeterSubCategory { get; set; }
        public string MeterName { get; set; }
        public string MeterRegion { get; set; }
        public string Unit { get; set; }
        public string ResourceLocation { get; set; }
        public string ConsumedService { get; set; }
        public string ResourceGroup { get; set; }
        public string ResourceURI { get; set; }
        public double? BillingPreTaxTotal { get; set; }
        public string BillingCurrency { get; set; }
        public double? PricingPreTaxTotal { get; set; }
        public string ServiceInfo1 { get; set; }
        public string ServiceInfo2 { get; set; }
        public string Tags { get; set; }
        public string AdditionalInfo { get; set; }
        public string EntitlementId { get; set; }
        public string EntitlementDescription { get; set; }
        public double? PartnerEarnedCreditPercentage { get; set; }
        public double? CreditPercentage { get; set; }
        public string CreditType { get; set; }
        public string BenefitId { get; set; }
        public string BenefitOrderId { get; set; }
        public string BenefitType { get; set; }
    }

    public class ExportSuccessOperation : Operation
    {
        public Manifest ResourceLocation { get; set; }
    }

    public class ExportFailedOperation : Operation
    {
        public PublicError Error { get; set; }
    }

    public class PublicError
    {
        public string message { get; set; }

        public string code { get; set; }
    }

    public class BillingBlob
    {
        public string Name { get; set; }

        public string PartitionValue { get; set; }

        public long SizeInBytes { get; set; }

        public long ItemCount { get; set; }
    }

    public enum DataPartitionType
    {
        /// <summary>
        /// Data is partitioned based on max number of rows/records.
        /// </summary>
        Default
    }

    public class Manifest
    {
        public string Id { get; set; }

        public string SchemaVersion { get; set; }

        public string DataFormat { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public string ETag { get; set; }

        public string PartnerTenantId { get; set; }

        public string RootDirectory { get; set; }

        public string SASToken { get; set; }

        public DataPartitionType PartitionType { get; set; }

        public int BlobCount { get; set; }

        public IReadOnlyList<BillingBlob> Blobs { get; set; }
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OperationStatus
    {
        /// <summary>
        /// Processing of operation has not yet started
        /// </summary>
        NotStarted,
        /// <summary>
        /// processing is running, data should be available soon.
        /// </summary>
        Running,
        /// <summary>
        /// Data is ready
        /// </summary>
        Succeeded,

        /// <summary>
        /// failed to generate data, use a new operation
        /// </summary>
        Failed
    }

    public class Operation
    {
        public OperationStatus Status { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime LastActionDateTime { get; set; }

        public TimeSpan? RetryAfter { get; set; }
    }

    public partial class AzureBillingV2DF : System.Web.UI.Page
    {
        private void CreateCustomerXml(string sCustomerFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sCustomerFile;
            XmlTextWriter xtw;
            xtw = new XmlTextWriter(filepath, Encoding.UTF8);
            xtw.WriteStartDocument();
            xtw.WriteStartElement("CustomerComments");
            xtw.WriteEndElement();
            xtw.Close();
        }

        private void WriteCustomerXml(string sId, string sName, string sMarkup, string sCustomerFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sCustomerFile;
            XmlDocument xd = new XmlDocument();
            FileStream lfile = new FileStream(filepath, FileMode.Open);
            xd.Load(lfile);
            XmlElement cl = xd.CreateElement("Customer");
            cl.SetAttribute("Id", sId);
            XmlElement na = xd.CreateElement("Name");
            XmlText natext = xd.CreateTextNode(sName);
            XmlElement na2 = xd.CreateElement("Comment");
            XmlText natext2 = xd.CreateTextNode(sMarkup);
            na.AppendChild(natext);
            na2.AppendChild(natext2);
            cl.AppendChild(na);
            cl.AppendChild(na2);
            xd.DocumentElement.AppendChild(cl);
            lfile.Close();
            xd.Save(filepath);
        }

        private bool CheckCustomerXml(string sId, string sCustomerFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sCustomerFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);
            bool bResult = false;
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                if ((cl.GetAttribute("Id")) == sId)
                {
                    bResult = true;
                    break;
                }
            }
            rfile.Close();
            return bResult;
        }

        private string ReadCustomerXml(string sId, string sCustomerFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sCustomerFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);
            string sResult = "n/a";
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Name")[i];
                XmlElement mp2 = (XmlElement)xdoc.GetElementsByTagName("Comment")[i];
                if ((cl.GetAttribute("Id")) == sId)
                {
                    sResult = mp.InnerText + ";" + mp2.InnerText;
                    break;
                }
            }
            rfile.Close();
            return sResult;
        }

        private bool UpdateCustomerXml(string sId, string sValue, string sCustomerFile)
        {
            bool bResult = false;
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sCustomerFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream up = new FileStream(filepath, FileMode.Open);
            xdoc.Load(up);
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cu = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Comment")[i];
                if (cu.GetAttribute("Id") == sId)
                {
                    cu.SetAttribute("Comment", sValue);
                    mp.InnerText = sValue;
                    bResult = true;
                    break;
                }
            }
            up.Close();
            xdoc.Save(filepath);
            return bResult;
        }

        private void CreateXml(string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlTextWriter xtw;
            xtw = new XmlTextWriter(filepath, Encoding.UTF8);
            xtw.WriteStartDocument();
            xtw.WriteStartElement("CustomerMarkups");
            xtw.WriteEndElement();
            xtw.Close();
        }

        private void WriteXml(string sId, string sName, string sMarkup, string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xd = new XmlDocument();
            FileStream lfile = new FileStream(filepath, FileMode.Open);
            xd.Load(lfile);
            XmlElement cl = xd.CreateElement("Customer");
            cl.SetAttribute("Id", sId);
            XmlElement na = xd.CreateElement("Name");
            XmlText natext = xd.CreateTextNode(sName);
            XmlElement na2 = xd.CreateElement("Markup");
            XmlText natext2 = xd.CreateTextNode(sMarkup);
            na.AppendChild(natext);
            na2.AppendChild(natext2);
            cl.AppendChild(na);
            cl.AppendChild(na2);
            xd.DocumentElement.AppendChild(cl);
            lfile.Close();
            xd.Save(filepath);
        }

        private bool CheckXml(string sId, string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);
            bool bResult = false;
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                if ((cl.GetAttribute("Id")) == sId)
                {
                    bResult = true;
                    break;
                }
            }
            rfile.Close();
            return bResult;
        }

        private string ReadXml(string sId, string sMarkupFile)
        {
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);
            string sResult = "n/a";
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Name")[i];
                XmlElement mp2 = (XmlElement)xdoc.GetElementsByTagName("Markup")[i];
                if ((cl.GetAttribute("Id")) == sId)
                {
                    sResult = mp.InnerText + ";" + mp2.InnerText;
                    break;
                }
            }
            rfile.Close();
            return sResult;
        }

        private bool UpdateXml(string sId, string sValue, string sMarkupFile)
        {
            bool bResult = false;
            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;
            XmlDocument xdoc = new XmlDocument();
            FileStream up = new FileStream(filepath, FileMode.Open);
            xdoc.Load(up);
            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cu = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Markup")[i];
                if (cu.GetAttribute("Id") == sId)
                {
                    cu.SetAttribute("Markup", sValue);
                    mp.InnerText = sValue;
                    bResult = true;
                    break;
                }
            }
            up.Close();
            xdoc.Save(filepath);
            return bResult;
        }

        public string sBCToken = "n/a";

        private const string ClientId = "1f7fcf68-2b68-49d7-97cc-5b915d26fb33";
        private const string ClientSecret = "j5mr]2GpPhJo8_fiFhEe7qbVvO-h[qDr";
        private const string AadTenantId = "6b1aee4c-953a-4fbe-b586-f59dd221da67";
        private const string Authority = "https://login.microsoftonline.com/{AadTenantId}/oauth2/v2.0/token";
        private string authToken = "";

        public static async Task<string> GetAccessToken(string aadTenantId)
        {
            Uri uri = new Uri(Authority.Replace("{AadTenantId}", aadTenantId));
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithClientSecret(ClientSecret)
                .WithAuthority(uri)
                .Build();
            string[] scopes = new string[] { @"https://graph.microsoft.com/.default" };
            string sResult = "n/a";
            try
            {
                AuthenticationResult AuthResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                sResult = AuthResult.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                sResult = ex.ErrorCode + " ::: " + ex.Message;
                sResult = "n/a";
            }
            return sResult;
        }

        private static async Task<List<MPCData>> DownloadBlob(string blobDirectory, string rootDirectorySAS, IReadOnlyList<BillingBlob> blobs)
        {
            AzureSasCredential credential = new AzureSasCredential(rootDirectorySAS);
            List<MPCData> AllMPCData = new List<MPCData>();

            foreach (var blob in blobs)
            {
                var blobPath = $"{blobDirectory}/{blob.Name}";
                BlobClient blobClient = new BlobClient(new Uri(blobPath), credential);

                // download to stream
                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                var streamContents = downloadResult.Content.ToStream();
                using (var ms = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(streamContents, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(ms);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms, Encoding.UTF8))
                    {
                        var serializer = new JsonSerializer();
                        using (var stringReader = new StringReader(sr.ReadToEnd()))
                        using (var jsonReader = new JsonTextReader(stringReader))
                        {
                            jsonReader.SupportMultipleContent = true;

                            while (jsonReader.Read())
                            {
                                var json = serializer.Deserialize<MPCData>(jsonReader);
                                AllMPCData.Add(json);
                            }
                        }
                    }
                }
            }

            return AllMPCData;
        }


        private async Task<List<MPCData>> GetMPCData(string sInvoiceId)
        {
            List<MPCData> sResult = new List<MPCData>();

            authToken = await GetAccessToken(AadTenantId);

            if (authToken == "n/a")
            {
                return sResult;
            }

            // long operation url
            string sNewLocation = "n/a";

            // get operatrion location
            string url = "https://graph.microsoft.com/v1.0/reports/partners/billing/reconciliation/billed/export";
            string sContent = "{  \"invoiceId\" : \"" + sInvoiceId + "\",  \"attributeSet\" : \"full\" }";
            var clientId = Guid.NewGuid().ToString();
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(sContent, Encoding.UTF8, "application/json"),
                Headers =
                        {
                            { "client-request-id", clientId },
                            { "Authorization", "Bearer " + authToken }
                        },
            };
            using (var response = await client.SendAsync(request, CancellationToken.None))
            {
                string responseString = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string message = string.Format("StatusCode = {0}, ErrorMessage = {1}, client-request-id={2}", response.StatusCode.ToString(), responseString, clientId);
                }
                else
                {
                    var location = response.Headers.GetValues("Location").FirstOrDefault();
                    sNewLocation = location.ToString();
                }
            }

            // get data
            Manifest sResourceLocation = null;
            if (sNewLocation != "n/a")
            {
                bool shouldRetry = false;
                var noOfRetries = 10;
                var retryCount = 1;

                do
                {
                    retryCount++;

                    var clientIdLO = Guid.NewGuid().ToString();
                    var requestLO = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(sNewLocation),
                        Headers =
                        {
                            { "client-request-id", clientIdLO },
                            { "Authorization", "Bearer " + authToken }
                        },
                    };

                    using (var clientLO = new HttpClient())
                    {
                        using (var responseLO = await clientLO.SendAsync(requestLO, CancellationToken.None))
                        {
                            string responseStringLO = responseLO.Content == null ? string.Empty : await responseLO.Content.ReadAsStringAsync();

                            if (!responseLO.IsSuccessStatusCode)
                            {
                                string message = string.Format("StatusCode = {0}, ErrorMessage = {1}, client-request-id={2}", responseLO.StatusCode.ToString(), responseStringLO, clientId);
                            }
                            else
                            {
                                var resLO = JsonConvert.DeserializeObject<Operation>(responseStringLO);
                                if (resLO.Status == OperationStatus.Succeeded)
                                {
                                    resLO = JsonConvert.DeserializeObject<ExportSuccessOperation>(responseStringLO);
                                    shouldRetry = false;
                                    sResourceLocation = ((ExportSuccessOperation)resLO).ResourceLocation;
                                }
                                else
                                {
                                    if (resLO.Status == OperationStatus.Failed)
                                    {
                                        resLO = JsonConvert.DeserializeObject<ExportFailedOperation>(responseStringLO);
                                        shouldRetry = true;
                                    }
                                    else
                                    {
                                        resLO.RetryAfter = responseLO.Headers?.RetryAfter?.Delta;
                                        await Task.Delay(resLO.RetryAfter.Value);
                                        shouldRetry = true;
                                    }
                                }
                            }
                        }
                    }
                }
                while (shouldRetry && retryCount <= noOfRetries); // 10 retries

                // process manifest now                
                if (sResourceLocation != null)
                {
                    var rootDirectorySAS = sResourceLocation.SASToken;
                    var rootDirectory = sResourceLocation.RootDirectory;
                    var blobs = sResourceLocation.Blobs;
                    sResult = await DownloadBlob(rootDirectory, rootDirectorySAS, blobs);
                }
                else
                {
                    sResult = null;
                }
            }

            return sResult;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            rbtnSeats.Visible = false;

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
            //sUserId = "7a6e0a8f-d6b4-428d-8f6d-9287fa64642a";

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

            if (Page.IsPostBack == false)
            {
                string sCommentFile = "MARKUPSeats.xml";
                if (rbtnSeats.Checked == true)
                {
                    sCommentFile = "MARKUPSeats.xml";
                }
                if (rtbnUsage.Checked == true)
                {
                    sCommentFile = "MARKUPUsage.xml";
                }

                HandleCustomersData();
                HandleCustomerCommentsData();
            }

            if (Page.IsPostBack == true)
            {
                System.Collections.Specialized.NameValueCollection FormPageVars;
                FormPageVars = Request.Form;

                var eventTarget = Request.Form["__EVENTTARGET"].ToString();

                // Check if some button is pressed
                if (eventTarget != null)
                {
                    if (eventTarget != "")
                    {
                        // UpdateBtn Button is pressed
                        if (eventTarget.IndexOf("butPushCustomer_") == 0)
                        {
                            string sCustVatId = eventTarget.Substring(eventTarget.IndexOf("butPushCustomer_") + 16);
                            PushSingleCustomer(sCustVatId);
                        }
                    }
                }
            }
        }

        private IAggregatePartner appPartnerOperations = null;
        private Task progressBackgroundTask;
        private CancellationTokenSource progressCancellationTokenSource = new CancellationTokenSource();
        private readonly int invoicePageSize = 200;
        private readonly int customerPageSize = 200;

        public static void WriteColored(string message, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            Console.Write(message + (newLine ? "\n" : string.Empty));
            Console.ResetColor();
        }

        public void StartProgress(string message)
        {
            if (progressBackgroundTask == null || progressBackgroundTask.Status != TaskStatus.Running)
            {
                progressBackgroundTask = new Task(() =>
                {
                    int dotCounter = 0;

                    while (!progressCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        for (dotCounter = 0; dotCounter < 5; dotCounter++)
                        {
                            Thread.Sleep(200);

                            if (progressCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                return;
                            }
                        }
                    }
                });

                progressBackgroundTask.Start();
            }
        }

        public void StopProgress()
        {
            if (progressBackgroundTask != null && progressBackgroundTask.Status == TaskStatus.Running)
            {
                progressCancellationTokenSource.Cancel();
                progressBackgroundTask.Wait();
                progressBackgroundTask.Dispose();
                progressBackgroundTask = null;

                progressCancellationTokenSource.Dispose();
                progressCancellationTokenSource = new CancellationTokenSource();
            }
        }

        public IAggregatePartner AppPartnerOperations
        {
            get
            {
                if (appPartnerOperations == null)
                {
                    StartProgress("Authenticating application");

                    IPartnerCredentials appCredentials = PartnerCredentials.Instance.GenerateByApplicationCredentials(
                        "1f7fcf68-2b68-49d7-97cc-5b915d26fb33",
                        "j5mr]2GpPhJo8_fiFhEe7qbVvO-h[qDr",
                        "GoWingu.onmicrosoft.com",
                        "https://login.windows.net",
                        "https://graph.windows.net");

                    StopProgress();
                    appPartnerOperations = PartnerService.Instance.CreatePartnerOperations(appCredentials);
                }

                return appPartnerOperations;
            }
        }

        public async void PushSingleCustomer(string sCustomerVATId)
        {
            if (rbtnSeats.Checked == true)
            {
                await GetInvoiceData("Seats", "BC", sCustomerVATId);
            }

            if (rtbnUsage.Checked == true)
            {
                await GetInvoiceData("Usage", "BC", sCustomerVATId);
            }
        }

        protected async void GetCustSubs_Click(object sender, EventArgs e)
        {
            // add new customer to xml 
            await GetCustomers();
            HandleCustomersData();
            HandleCustomerCommentsData();
        }

        public void HandleCustomerCommentsData()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");

            List<CustComment> cls = new List<CustComment>();

            string sCustomerCommentsFile = "CUSTOMERSComments.xml";

            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sCustomerCommentsFile;

            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);

            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Name")[i];
                XmlElement mp2 = (XmlElement)xdoc.GetElementsByTagName("Comment")[i];

                string sCustomerName = mp.InnerText;
                string sCustomerId = cl.GetAttribute("Id");

                CustComment cm = new CustComment();
                cm.Id = cl.GetAttribute("Id");
                cm.ProdId = "Customer";
                cm.Name = "<b>" + mp.InnerText + "</b>";
                cm.Comment = mp2.InnerText;
                cls.Add(cm);

            }
            rfile.Close();

            CustomerComments.DataSource = cls;
            CustomerComments.DataBind();
        }

        public async void HandleCustomersData()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            List<CustMarkup> cls = new List<CustMarkup>();

            string sMarkupFile = "MARKUPSeats.xml";
            string sMarkupType = "MARKUPSeats";
            if (rbtnSeats.Checked == true)
            {
                sMarkupFile = "MARKUPSeats.xml";
                sMarkupType = "MARKUPSeats";
                MarkupType.Text = "SEATS Type: MARKUP";
            }
            if (rtbnUsage.Checked == true)
            {
                MarkupType.Text = "USAGE Type: MARKUP";
                sMarkupFile = "MARKUPUsage.xml";
                sMarkupType = "MARKUPUsage";
            }

            string sComment = ReadXml("D87883D1-AECE-48DE-8109-394F3A7E3EC2", sMarkupFile);
            if (sComment != "n/a")
            {
                InvoiceCommentTB.Text = sComment.Split(';')[1];
            }

            var docPath = HttpContext.Current.Server.MapPath("~");
            var filepath = $@"{docPath}\" + sMarkupFile;

            XmlDocument xdoc = new XmlDocument();
            FileStream rfile = new FileStream(filepath, FileMode.Open);
            xdoc.Load(rfile);

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            XmlNodeList list = xdoc.GetElementsByTagName("Customer");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xdoc.GetElementsByTagName("Customer")[i];
                XmlElement mp = (XmlElement)xdoc.GetElementsByTagName("Name")[i];
                XmlElement mp2 = (XmlElement)xdoc.GetElementsByTagName("Markup")[i];

                string sCustomerName = mp.InnerText;
                string sCustomerId = cl.GetAttribute("Id");

                if (sCustomerId != "D87883D1-AECE-48DE-8109-394F3A7E3EC2")
                {
                    CustMarkup cm = new CustMarkup();
                    cm.Id = cl.GetAttribute("Id");
                    cm.ProdId = "Customer";
                    cm.Name = "<b>" + mp.InnerText + "</b>";
                    cm.Markup = mp2.InnerText;
                    cls.Add(cm);

                    // get all products for the company
                    string sSql = "SELECT [Id], [ProductId], [ProductName], [Markup], [CustomerId] FROM [RPNAVConnect].[dbo].[MPCMarkups] WHERE (([CustomerId] = '" + sCustomerId.ToLower() + "') OR ([CustomerId] = '" + sCustomerId.ToUpper() + "')) AND [MarkupType] = '" + sMarkupType + "'";
                    System.Data.OleDb.OleDbDataReader oleReader;
                    System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
                    oleReader = cmd.ExecuteReader();
                    while (oleReader.Read())
                    {
                        if ((!oleReader.IsDBNull(1)) && (!oleReader.IsDBNull(2)) && (!oleReader.IsDBNull(3)))
                        {
                            CustMarkup cmp = new CustMarkup();
                            cmp.Id = oleReader.GetString(4).ToUpper();
                            cmp.ProdId = oleReader.GetString(1);
                            cmp.Name = "&nbsp;&nbsp;&nbsp;<i>" + oleReader.GetString(2) + "<br />&nbsp;&nbsp;&nbsp;<font size='1'>" + oleReader.GetString(1) + "</font></i>";
                            cmp.Markup = oleReader.GetDecimal(3).ToString();
                            cls.Add(cmp);
                        }
                    }
                    oleReader.Close();
                }

            }
            rfile.Close();

            dbConn.Close();

            CustomersMarkup.DataSource = cls;
            CustomersMarkup.DataBind();
        }

        private bool CustomerProductExists(string sCustomerId, string sProductId, string sMarkupType, OleDbConnection dbConn)
        {
            bool bResult = false;

            string sSql = "SELECT [Id] FROM [RPNAVConnect].[dbo].[MPCMarkups] WHERE [MarkupType] = '" + sMarkupType + "' AND [CustomerId] = '" + sCustomerId + "' AND [ProductId] = '" + sProductId + "'";

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0))
                {
                    bResult = true;
                }
            }
            oleReader.Close();

            return bResult;
        }

        private string GetMarkupDataProductName(string sProductName, string sCustomerId, string sMarkupType)
        {
            string sResult = "n/a";

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            string sSql = "SELECT TOP 1 [Markup] FROM [RPNAVConnect].[dbo].[MPCMarkups] WHERE [MarkupType] = '" + sMarkupType + "' AND [CustomerId] = '" + sCustomerId + "' AND [ProductName] = '" + sProductName + "'";

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0))
                {
                    sResult = oleReader.GetDecimal(0).ToString();
                }
            }
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        private string GetMarkupData(string sOfferId, string sCustomerId, string sMarkupType)
        {
            string sResult = "n/a";

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            string sSql = "SELECT TOP 1 [Markup] FROM [RPNAVConnect].[dbo].[MPCMarkups] WHERE [MarkupType] = '" + sMarkupType + "' AND [CustomerId] = '" + sCustomerId + "' AND [ProductId] = '" + sOfferId + "'";

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0))
                {
                    sResult = oleReader.GetDecimal(0).ToString();
                }
            }
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        private string InsertUpdateDatabase(string SQL, OleDbConnection dbConn)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Get Connection string
            string sResult = "DBOK";

            try
            {
                // Database Object instancing here
                OleDbCommand OleCommand;
                OleCommand = new OleDbCommand(SQL, dbConn);
                OleCommand.CommandTimeout = 600;
                OleCommand.ExecuteNonQuery();
            }
            catch (Exception Ex)
            {
                Ex.ToString();
                sResult = "DBERROR: " + Ex.ToString();
                PushingDataL.Text += sResult + " <br />";
                return sResult;
            }

            return sResult;
        }

        public async Task GetCustomers()
        {
            var partnerOperations = AppPartnerOperations;

            StartProgress("Querying customers");
            var customersPage = (customerPageSize <= 0) ? partnerOperations.Customers.Get() : partnerOperations.Customers.Query(QueryFactory.Instance.BuildIndexedQuery(customerPageSize));
            StopProgress();

            string sCustomerCommentsFile = "CUSTOMERSComments.xml";

            string sMarkupFile = "MARKUPSeats.xml";
            string sMarkupType = "MARKUPSeats";
            if (rbtnSeats.Checked == true)
            {
                sMarkupType = "MARKUPSeats";
                sMarkupFile = "MARKUPSeats.xml";
                MarkupType.Text = "SEATS Type: MARKUP";
            }
            if (rtbnUsage.Checked == true)
            {
                sMarkupType = "MARKUPUsage";
                MarkupType.Text = "USAGE Type: MARKUP";
                sMarkupFile = "MARKUPUsage.xml";
            }

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            PushingDataL.Visible = true;
            PushingDataL.Enabled = true;
            PushingDataL.Text = "Start updating customers.";

            DateTime dtStart = DateTime.Now;

            foreach (var customer in customersPage.Items)
            {
                string sCustomerId = customer.Id;
                string sCustomerName = customer.CompanyProfile.CompanyName;

                PushingDataL.Text += "Checking customer: " + sCustomerName + " (" + sCustomerId + ")";

                if ((sCustomerId != "") && (sCustomerName != ""))
                {
                    // update customers if there's new
                    if (CheckXml(sCustomerId, sMarkupFile) == false)
                    {
                        WriteXml(sCustomerId, sCustomerName, "25.0", sMarkupFile);

                        PushingDataL.Text += "Added customer: " + sCustomerName + " (" + sCustomerId + ") (" + sMarkupFile + ")";
                    }

                    // update customers if there's new
                    if (CheckCustomerXml(sCustomerId, sCustomerCommentsFile) == false)
                    {
                        WriteCustomerXml(sCustomerId, sCustomerName, "", sCustomerCommentsFile);
                    }

                    // update products if there's new
                    try
                    {
                        StartProgress("Querying products");
                        var customerSubscriptions = partnerOperations.Customers.ById(sCustomerId).Subscriptions.Get();
                        StopProgress();

                        foreach (var customerSubscription in customerSubscriptions.Items)
                        {

                            PushingDataL.Text += "    Product: " + customerSubscription.OfferId + " (" + customerSubscription.OfferName + ") (" + customerSubscription.IsMicrosoftProduct.ToString() + ")";

                            if (customerSubscription.IsMicrosoftProduct == true)
                            {
                                string sProductId = customerSubscription.OfferId;
                                string sProductName = customerSubscription.OfferName;

                                // Seats
                                if (CustomerProductExists(sCustomerId, sProductId, "MARKUPSeats", dbConn) == false)
                                {
                                    string sSql = "INSERT INTO [dbo].[MPCMarkups] ([MarkupType], [CustomerId], [CustomerName], [ProductId], [ProductName], [Markup]) ";
                                    sSql += "VALUES ('MARKUPSeats', '" + sCustomerId.ToUpper() + "', '" + sCustomerName.Replace("'", "''") + "', '" + sProductId.ToUpper() + "', '" + sProductName.Replace("'", "''") + "', 25.0)";
                                    string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                                    if (sDBResult != "DBOK")
                                    {
                                        PushingDataL.Text += sDBResult + " <br />";
                                    }
                                    else
                                    {
                                        PushingDataL.Text += "    Added Product (SEATS): " + customerSubscription.OfferId + " (" + customerSubscription.OfferName + ") (" + customerSubscription.IsMicrosoftProduct.ToString() + ")";
                                    }
                                }

                                // Usage
                                if (CustomerProductExists(sCustomerId, sProductId, "MARKUPUsage", dbConn) == false)
                                {
                                    string sSql = "INSERT INTO [dbo].[MPCMarkups] ([MarkupType], [CustomerId], [CustomerName], [ProductId], [ProductName], [Markup]) ";
                                    sSql += "VALUES ('MARKUPUsage', '" + sCustomerId.ToUpper() + "', '" + sCustomerName.Replace("'", "''") + "', '" + sProductId.ToUpper() + "', '" + sProductName.Replace("'", "''") + "', 25.0)";
                                    string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                                    if (sDBResult != "DBOK")
                                    {
                                        PushingDataL.Text += sDBResult + " <br />";
                                    }
                                    else
                                    {
                                        PushingDataL.Text += "    Added Product (USAGE): " + customerSubscription.OfferId + " (" + customerSubscription.OfferName + ") (" + customerSubscription.IsMicrosoftProduct.ToString() + ")";
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        PushingDataL.Text += ex.ToString();
                    }
                }
            }

            dbConn.Close();

            GetCustSubsLabel.Text = "Customers & Subscriptions updated";
            AzureBillingDataB.Visible = true;

            DateTime dtEnd = DateTime.Now;
            TimeSpan ts = dtEnd.Subtract(dtStart);
            double tssec = ts.TotalSeconds;
            string sSec = tssec.ToString();
        }

        /*
        protected void SaveCustomerMarkupsB_Click(object sender, EventArgs e)
        {
            if (Page.IsPostBack == true)
            {
                for (int i = 0; i < CustomersMarkup.Items.Count; i++)
                {
                    ListViewDataItem lvdi = CustomersMarkup.Items[i];

                    foreach (var lvdicontrol in lvdi.Controls)
                    {
                        if (lvdicontrol is System.Web.UI.WebControls.TextBox)
                        {
                            TextBox tb = (TextBox)lvdicontrol;
                            AttributeCollection ac = tb.Attributes;
                            string sCustId = ac["CustId"];
                            string sCustName = ac["CustName"];
                            string sCustMarkup = tb.Text;

                            CustMarkup cm = (CustMarkup)lvdi.DataItem;

                        }
                    }
                }
            }
        }
        */

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


        public async Task GetInvoiceData(string sRPBillingType, string sAction, string sCustomerVATIdSingle)
        {

            string sRPInvoiceType = "reconciliation";
            if (sRPBillingType == "Seats") sRPInvoiceType = "usage";
            if (sRPBillingType == "Usage") sRPInvoiceType = "reconciliation";

            string sInvoiceId = InvoiceIdTB.Text;
            List<MPCData> mPCDatas = await GetMPCData(sInvoiceId);

            // Get distinct customers
            string sCustomerDistinctList = "";
            List<MPCData> sResultCustomers = new List<MPCData>();
            foreach (var mpcData in mPCDatas)
            {
                if (sCustomerDistinctList.IndexOf(mpcData.CustomerId) == -1)
                {
                    sCustomerDistinctList += mpcData.CustomerId + "#";
                    sResultCustomers.Add(mpcData);
                }
            }
            string[] mpcCustomers = sCustomerDistinctList.Split('#');

            string sItem310Id = "";
            string sItem315Id = "";

            AzureBillingDataL.Text = "<br />";

            lastscriptdiv.InnerHtml = "<script>";

            AzureBillingDataL.Text += "<font size='3'>MPV Data for <b>" + sInvoiceId + "</b></font>";
            AzureBillingDataL.Text += "<br />";

            string sAllInvoiceCustomers = "";

            // get all customers first
            foreach (var mpcCustomer in sResultCustomers)
            {
                string sCustomerName = mpcCustomer.CustomerName;
                string sCustomerId = mpcCustomer.CustomerId;

                if ((sCustomerId != "") && (sCustomerName != ""))
                {
                    string sCustomerVATNo = "n/a";
                    string sCustomerVATId = "n/a";
                    string sCustomerVATName = "n/a";
                    string sCustomerCSP2 = "n/a";
                    string sCustomerCSP3 = "n/a";

                    if (sAllInvoiceCustomers.IndexOf(sCustomerName + "ђ" + sCustomerId) == -1)
                    {
                        string sBCCuromerData = DoesCustomerExists(sCustomerId);
                        if (sBCCuromerData == "n/a")
                        {
                            sBCCuromerData = "n/aђn/aђn/aђn/aђn/a";
                        }
                        sCustomerVATNo = sBCCuromerData.Split('ђ')[0];
                        sCustomerVATId = sBCCuromerData.Split('ђ')[1];
                        sCustomerVATName = sBCCuromerData.Split('ђ')[2];
                        sCustomerCSP2 = sBCCuromerData.Split('ђ')[3];
                        sCustomerCSP3 = sBCCuromerData.Split('ђ')[4];
                        sAllInvoiceCustomers += sCustomerName + "ђ" + sCustomerId + "ђ" + sCustomerVATNo + "ђ" + sCustomerVATId + "ђ" + sCustomerVATName + "ђ" + sCustomerCSP2 + "ђ" + sCustomerCSP3 + "ш";
                    }
                }
            }

            string[] sAllInvoiceCustomersArrayFirst = sAllInvoiceCustomers.Split('ш');
            string sAllInvoiceCustomersSorted = "";

            // empty VATs first
            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArrayFirst)
            {
                if (sInvoiceCustomer != "")
                {
                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                    string sCustId = sInvoiceCustomer.Split('ђ')[1];
                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[2];
                    string sCustVatId = sInvoiceCustomer.Split('ђ')[3];
                    string sCustVatName = sInvoiceCustomer.Split('ђ')[4];
                    string sCustCSP2 = sInvoiceCustomer.Split('ђ')[5];
                    string sCustCSP3 = sInvoiceCustomer.Split('ђ')[6];

                    if (sCustVatNo == "n/a")
                    {
                        sAllInvoiceCustomersSorted += sCust + "ђ" + sCustId + "ђ" + sCustVatNo + "ђ" + sCustVatId + "ђ" + sCustVatName + "ђ" + sCustCSP2 + "ђ" + sCustCSP3 + "ш";
                    }

                }
            }

            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArrayFirst)
            {
                if (sInvoiceCustomer != "")
                {
                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                    string sCustId = sInvoiceCustomer.Split('ђ')[1];
                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[2];
                    string sCustVatId = sInvoiceCustomer.Split('ђ')[3];
                    string sCustVatName = sInvoiceCustomer.Split('ђ')[4];
                    string sCustCSP2 = sInvoiceCustomer.Split('ђ')[5];
                    string sCustCSP3 = sInvoiceCustomer.Split('ђ')[6];

                    if (sCustVatNo != "n/a")
                    {
                        sAllInvoiceCustomersSorted += sCust + "ђ" + sCustId + "ђ" + sCustVatNo + "ђ" + sCustVatId + "ђ" + sCustVatName + "ђ" + sCustCSP2 + "ђ" + sCustCSP3 + "ш";
                    }
                }
            }

            AzureBillingDataL.Text += "<font size='3' color='blue'><b>BillingProvider - " + sRPInvoiceType + "</b></font>";

            AzureBillingDataL.Text += "<br />";

            AzureBillingDataL.Text += "<hr /><br />";

            string[] sAllInvoiceCustomersArray = sAllInvoiceCustomersSorted.Split('ш');

            AzureBillingDataL.Text += "<table class='table table-bordered table-striped' style='width: 1250px; '>";
            AzureBillingDataL.Text += "<tr class='bg-danger text-white'>";
            AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Customer</b></th>";
            AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>CSP Id</b></th>";
            AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>VAT</b></th>";
            if (sRPInvoiceType == "usage")
            {
                AzureBillingDataL.Text += "<th style='vertical-align: middle;' style='vertical-align: middle;'><b>Tot. MS price</b></th>";
                AzureBillingDataL.Text += "<th><b>Tot. RP price exc. Tax</b></th>";
                AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Tot. Db (MarkUp)</b></th>";
                AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Comment</b></th>";
            }
            else
            {
                AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Tot. MS price</b></th>";
                AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Tot. MS price exc. Tax</b></th>";
                AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Tot. RP price exc. Tax</b></th>";
                AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Tot. Db (MarkUp)</b></th>";
                AzureBillingDataL.Text += "<th style='vertical-align: middle;'><b>Comment</b></th>";
            }
            AzureBillingDataL.Text += "<th>&nbsp;</th>";
            AzureBillingDataL.Text += "</tr>";
            AzureBillingDataL.Text += "<tbody>";

            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArray)
            {
                if (sInvoiceCustomer != "")
                {
                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                    string sCustId = sInvoiceCustomer.Split('ђ')[1];
                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[2];
                    string sCustVatId = sInvoiceCustomer.Split('ђ')[3];
                    string sCustVatName = sInvoiceCustomer.Split('ђ')[4];
                    string sCustCSP2 = sInvoiceCustomer.Split('ђ')[5];
                    string sCustCSP3 = sInvoiceCustomer.Split('ђ')[6];

                    string sWarning1 = "";
                    string sWarning2 = "";
                    if (sCustVatNo == "n/a")
                    {
                        sWarning1 = "<font color='red'>";
                        sWarning1 = "<font color='red'>";
                        sWarning2 = "</font>";
                    }

                    AzureBillingDataL.Text += "<tr>";

                    if ((sCustCSP2 == "yes") || (sCustCSP3 == "yes"))
                    {
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + sCustVatName + " (" + sCust + ")" + sWarning2 + "</td>";
                    }
                    else
                    {
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + sCustVatName + sWarning2 + "</td>";
                    }

                    AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + sCustId + sWarning2 + "</td>";
                    AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + sCustVatNo + sWarning2 + "</td>";

                    if (sRPInvoiceType == "usage")
                    {
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + "#TM" + sCust + "#" + sWarning2 + "</td>";
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + "#TC" + sCust + "#" + sWarning2 + "</td>";
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + "#TD" + sCust + "#" + sWarning2 + "</td>";
                    }
                    else
                    {
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + "#TM" + sCust + "#" + sWarning2 + "</td>";
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + "#TMET" + sCust + "#" + sWarning2 + "</td>";
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + "#TC" + sCust + "#" + sWarning2 + "</td>";
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'>" + sWarning1 + "#TD" + sCust + "#" + sWarning2 + "</td>";
                    }

                    if (sCustVatNo == "n/a")
                    {
                        AzureBillingDataL.Text += "<td></td>";
                        AzureBillingDataL.Text += "<td></td>";
                    }
                    else
                    {
                        string sButtonId = sCustVatNo;
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'><input id=\"txtCommentCustomer_" + sCustId + "\" type=\"text\" name=\"txtCommentCustomer_" + sCustId + "\" value=\"\" /></td>";
                        AzureBillingDataL.Text += "<td style='vertical-align: middle;'><input id=\"butPushCustomer_" + sCustId + "\" type=\"button\" name=\"butPushCustomer_" + sCustId + "\" value=\"Push to BC\" onclick=\"invokeLoader();__doPostBack('butPushCustomer_" + sCustId + "','')\" /></td>";
                    }
                    AzureBillingDataL.Text += "</tr>";
                }
            }
            AzureBillingDataL.Text += "<tr class='bg-danger text-white'>";
            AzureBillingDataL.Text += "<td></td><td></td>";
            AzureBillingDataL.Text += "<td style='vertical-align: middle;' align='right'>Sum:</td>";

            if (sRPInvoiceType == "usage")
            {
                AzureBillingDataL.Text += "<td style='vertical-align: middle;'><b>#SUMTM#</b></td>";
                AzureBillingDataL.Text += "<td style='vertical-align: middle;'><b>#SUMTC#</b></td>";
                AzureBillingDataL.Text += "<td style='vertical-align: middle;'><b>#SUMTD#</b></td>";
            }
            else
            {
                AzureBillingDataL.Text += "<td style='vertical-align: middle;'><b>#SUMTM#</b></td>";
                AzureBillingDataL.Text += "<td style='vertical-align: middle;'><b>#SUMTMET#</b></td>";
                AzureBillingDataL.Text += "<td style='vertical-align: middle;'><b>#SUMTC#</b></td>";
                AzureBillingDataL.Text += "<td style='vertical-align: middle;'><b>#SUMTD#</b></td>";
            }
            AzureBillingDataL.Text += "</tbody>";
            AzureBillingDataL.Text += "</table>";
            AzureBillingDataL.Text += "<br />";

            AzureBillingDataL.Text += "<font size='3'><b><a href = 'javascript:toogleINVDETAILS(1);' id = 'tmidlink'>Show Invoice Details</a></b></font><br /><br />";
            AzureBillingDataL.Text += "<div id='INVDETAILS_1' style='display: none;'>";

            decimal dTCustMSListAmount = 0;
            decimal dTCustERPAmount = 0;
            decimal dTCustRPTotalAmount = 0;
            decimal dTCustRPTotalDBMArkupAmount = 0;
            decimal dTCustRPMarkup = 0;
            int iCustomerCount = 0;

            decimal dTCustMSUTotalAmount = 0;
            decimal dTCustMSUTotalAmountExcTax = 0;
            decimal dTCustRPUMarkup = 0;
            decimal dTCustRPUTotalAmount = 0;
            decimal dTCustRPUDiffAmount = 0;

            // csv file all customers
            List<string> sCSVFile = new List<string>();
            string sCSVLine = "Customer,CustomerId,CustomerNo,Description,Total_Amount_Excl_VAT,Unit_Price,PCToBCExchangeRate,Quantity,Rackpeople Markup,Rackpeople Markup Unit Price,Rackpeople Markup Total Price,Rackpeople Markup Diff";
            sCSVFile.Add(sCSVLine);

            // csv file single customer
            List<string> sCustomerCSVUsageFile = new List<string>();
            List<string> sCustomerCSVSeatsFile = new List<string>();
            string sCustomerCSVLine = "";

            bool bFistCustomer = true;
            foreach (string sInvoiceCustomer in sAllInvoiceCustomersArray)
            {
                if (sInvoiceCustomer != "")
                {
                    string sCust = sInvoiceCustomer.Split('ђ')[0];
                    string sCustId = sInvoiceCustomer.Split('ђ')[1];
                    string sCustVatNo = sInvoiceCustomer.Split('ђ')[2];
                    string sCustVatId = sInvoiceCustomer.Split('ђ')[3];
                    string sCustVatName = sInvoiceCustomer.Split('ђ')[4];
                    string sCustCSP2 = sInvoiceCustomer.Split('ђ')[5];

                    if ((sCustVatNo != "n/a") || (sAction == "Data"))
                    {
                        // csv file all customers
                        sCustomerCSVUsageFile.Clear();
                        sCustomerCSVSeatsFile.Clear();
                        sCustomerCSVLine = "Customer,CustomerId,CustomerNo,Description,Quantity,Unit Price";
                        sCustomerCSVUsageFile.Add(sCustomerCSVLine);
                        sCustomerCSVSeatsFile.Add(sCustomerCSVLine);

                        string sCustomerCSVName = "";
                        string sCustomerCSVId = "";

                        // create order first
                        PostSalesInvoice order = new PostSalesInvoice();

                        List<PostSalesInvoiceLine> InvoiceLinesList = new List<PostSalesInvoiceLine>();
                        int iInvoiceLinesCount = 0;

                        if ((sAction == "BC") && ((sCustomerVATIdSingle == "ALL") || (sCustomerVATIdSingle == sCustId)))
                        {
                            order.customerNumber = sCustVatNo;
                            order.billToCustomerNumber = sCustVatNo;
                            order.customerId = sCustVatId;
                            order.billToCustomerId = sCustVatId;
                            order.invoiceDate = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0');
                            order.postingDate = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0');
                        }

                        // add month line
                        PostSalesInvoiceLine commentmonthLine = new PostSalesInvoiceLine();

                        commentmonthLine.lineType = "";
                        commentmonthLine.lineObjectNumber = "";
                        commentmonthLine.itemId = "";
                        commentmonthLine.Document_No = "";

                        // quantity and price
                        commentmonthLine.quantity = 0;
                        commentmonthLine.unitPrice = 0;

                        // extra line
                        commentmonthLine.description = sInvoiceId + MonthTB.Text + "/" + YearTB.Text; 

                        // add extra line
                        InvoiceLinesList.Add(commentmonthLine);

                        // count added lines
                        iInvoiceLinesCount++;

                        // invoice comment - all customers
                        string sCommentFile = "MARKUPUsage.xml";
                        if (rbtnSeats.Checked == true)
                        {
                            sCommentFile = "MARKUPSeats.xml";
                        }
                        if (rtbnUsage.Checked == true)
                        {
                            sCommentFile = "MARKUPUsage.xml";
                        }
                        string sMarkComment = ReadXml("D87883D1-AECE-48DE-8109-394F3A7E3EC2", sCommentFile);
                        if ((sMarkComment != "") && (sMarkComment != "n/a"))
                        {
                            PostSalesInvoiceLine commentLine = new PostSalesInvoiceLine();

                            commentLine.lineType = "";
                            commentLine.lineObjectNumber = "";
                            commentLine.itemId = "";
                            commentLine.Document_No = "";

                            // quantity and price
                            commentLine.quantity = 0;
                            commentLine.unitPrice = 0;

                            // extra line
                            commentLine.description = sMarkComment.Split(';')[1];

                            // add extra line
                            InvoiceLinesList.Add(commentLine);

                            // count added lines
                            iInvoiceLinesCount++;
                        }

                        int iCount = 1;

                        decimal dCustMSListAmount = 0;
                        decimal dCustERPAmount = 0;
                        decimal dCustRPRebate = 0;
                        decimal dCustRPTotalAmount = 0;
                        decimal dCustRPTotalDBMArkupAmount = 0;
                        decimal dCustRPMarkup = 0;

                        decimal dCustMSUTotalAmount = 0;
                        decimal dCustMSUTotalAmountExcTax = 0;
                        decimal dCustRPUMarkup = 0;
                        decimal dCustRPUTotalAmount = 0;
                        decimal dCustRPUDiffAmount = 0;
                        decimal dCustRPUDiffAmountSeats = 0;

                        // select list to customer only
                        List<MPCData> selectedList = mPCDatas.Where(lst => lst.CustomerId == sCustId).ToList();
                        foreach (var mpcItem in selectedList)
                        {
                            string sCustomerId = mpcItem.CustomerId;
                            string sCustomerName = mpcItem.CustomerName;
                            string sProductNo = "2050.015"; // hardcoded 310
                            string sDescription = mpcItem.ProductName + " - " + mpcItem.SkuName + " - " + mpcItem.SubscriptionDescription;
                            string sQuantity = mpcItem.Quantity.ToString();
                            string sBillableQuantity = mpcItem.BillableQuantity.ToString(); ;
                            string sSubTotal = mpcItem.Subtotal.ToString();
                            string sTaxTotal = mpcItem.TaxTotal.ToString();
                            string sUnitType = mpcItem.UnitType;
                            string sLineAmount = "n/a";
                            string sTotalAmount = mpcItem.Total.ToString();
                            
                            
                            string sUnitPrice = mpcItem.UnitPrice.ToString();

                            string sEffectiveUnitPrice = mpcItem.EffectiveUnitPrice.ToString();
                            
                            string sDollarPrice = mpcItem.PCToBCExchangeRate.ToString();
                            string sOfferId = "n/a";
                            string sOfferName = "n/a";

                            string sProductIdUsage = mpcItem.ProductId;
                            string sSkuIdUsage = mpcItem.SkuId;
                            string sAvailabilityIdUsage = mpcItem.AvailabilityId;

                            string sLine2 = "";

                            if (rbtnSeats.Checked == true)
                            {
                                try
                                {
                                    sLine2 += mpcItem.PartnerId.ToString() + ", ";
                                    sLine2 += mpcItem.PartnerName.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerId.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerName.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerDomainName.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerCountry.ToString() + ", ";
                                    sLine2 += mpcItem.MpnId.ToString() + ", ";
                                    sLine2 += mpcItem.InvoiceNumber.ToString() + ", ";
                                    sLine2 += mpcItem.ProductId.ToString() + ", ";
                                    sLine2 += mpcItem.SkuId.ToString() + ", ";
                                    sLine2 += mpcItem.AvailabilityId.ToString() + ", ";
                                    sLine2 += mpcItem.SkuName.ToString() + ", ";
                                    sLine2 += mpcItem.ProductName.ToString() + ", ";
                                    sLine2 += mpcItem.PublisherName.ToString() + ", ";
                                    sLine2 += mpcItem.PublisherId.ToString() + ", ";
                                    sLine2 += mpcItem.SubscriptionDescription.ToString() + ", ";
                                    sLine2 += mpcItem.SubscriptionId.ToString() + ", ";
                                    sLine2 += mpcItem.ChargeStartDate.ToString() + ", ";
                                    sLine2 += mpcItem.ChargeEndDate.ToString() + ", ";
                                    sLine2 += mpcItem.UsageDate.ToString() + ", ";
                                    sLine2 += mpcItem.MeterType.ToString() + ", ";
                                    sLine2 += mpcItem.MeterCategory.ToString() + ", ";
                                    sLine2 += mpcItem.MeterId.ToString() + ", ";
                                    sLine2 += mpcItem.MeterSubCategory.ToString() + ", ";
                                    sLine2 += mpcItem.MeterName.ToString() + ", ";
                                    sLine2 += mpcItem.MeterRegion.ToString() + ", ";
                                    sLine2 += mpcItem.Unit.ToString() + ", ";
                                    sLine2 += mpcItem.ResourceLocation.ToString() + ", ";
                                    sLine2 += mpcItem.ConsumedService.ToString() + ", ";
                                    sLine2 += mpcItem.ResourceGroup.ToString() + ", ";
                                    sLine2 += mpcItem.ResourceURI.ToString() + ", ";
                                    sLine2 += mpcItem.ChargeType.ToString() + ", ";
                                    sLine2 += mpcItem.UnitPrice.ToString() + ", ";
                                    sLine2 += mpcItem.Quantity.ToString() + ", ";
                                    sLine2 += mpcItem.UnitType.ToString() + ", ";
                                    sLine2 += mpcItem.BillingPreTaxTotal.ToString() + ", ";
                                    sLine2 += mpcItem.BillingCurrency.ToString() + ", ";
                                    sLine2 += mpcItem.PricingPreTaxTotal.ToString() + ", ";
                                    sLine2 += mpcItem.PricingCurrency.ToString() + ", ";
                                    sLine2 += mpcItem.ServiceInfo1.ToString() + ", ";
                                    sLine2 += mpcItem.ServiceInfo2.ToString() + ", ";
                                    sLine2 += mpcItem.Tags.ToString() + ", ";
                                    sLine2 += mpcItem.AdditionalInfo.ToString() + ", ";
                                    sLine2 += mpcItem.EffectiveUnitPrice.ToString() + ", ";
                                    sLine2 += mpcItem.PCToBCExchangeRate.ToString() + ", ";
                                    sLine2 += mpcItem.PCToBCExchangeRateDate.ToString() + ", ";
                                    sLine2 += mpcItem.EntitlementId.ToString() + ", ";
                                    sLine2 += mpcItem.EntitlementDescription.ToString() + ", ";
                                    sLine2 += mpcItem.PartnerEarnedCreditPercentage.ToString() + ", ";
                                    sLine2 += mpcItem.CreditPercentage.ToString() + ", ";
                                    sLine2 += mpcItem.CreditType.ToString() + ", ";
                                    sLine2 += mpcItem.BenefitOrderId.ToString() + ", ";
                                    sLine2 += mpcItem.BenefitId.ToString() + ", ";
                                    sLine2 += mpcItem.BenefitType.ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }

                            if (rtbnUsage.Checked == true)
                            {
                                try
                                {
                                    sLine2 += mpcItem.PartnerId.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerId.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerName.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerDomainName.ToString() + ", ";
                                    sLine2 += mpcItem.CustomerCountry.ToString() + ", ";
                                    sLine2 += mpcItem.InvoiceNumber.ToString() + ", ";
                                    sLine2 += mpcItem.MpnId.ToString() + ", ";
                                    sLine2 += mpcItem.OrderId.ToString() + ", ";
                                    sLine2 += mpcItem.OrderDate.ToString() + ", ";
                                    sLine2 += mpcItem.ProductId.ToString() + ", ";
                                    sLine2 += mpcItem.SkuId.ToString() + ", ";
                                    sLine2 += mpcItem.AvailabilityId.ToString() + ", ";
                                    sLine2 += mpcItem.SkuName.ToString() + ", ";
                                    sLine2 += mpcItem.ProductName.ToString() + ", ";
                                    sLine2 += mpcItem.ChargeType.ToString() + ", ";
                                    sLine2 += mpcItem.UnitPrice.ToString() + ", ";
                                    sLine2 += mpcItem.Quantity.ToString() + ", ";
                                    sLine2 += mpcItem.Subtotal.ToString() + ", ";
                                    sLine2 += mpcItem.TaxTotal.ToString() + ", ";
                                    sLine2 += mpcItem.Total.ToString() + ", ";
                                    sLine2 += mpcItem.Currency.ToString() + ", ";
                                    sLine2 += mpcItem.PriceAdjustmentDescription.ToString() + ", ";
                                    sLine2 += mpcItem.PublisherName.ToString() + ", ";
                                    sLine2 += mpcItem.PublisherId.ToString() + ", ";
                                    sLine2 += mpcItem.SubscriptionDescription.ToString() + ", ";
                                    sLine2 += mpcItem.SubscriptionId.ToString() + ", ";
                                    sLine2 += mpcItem.ChargeStartDate.ToString() + ", ";
                                    sLine2 += mpcItem.ChargeEndDate.ToString() + ", ";
                                    sLine2 += mpcItem.TermAndBillingCycle.ToString() + ", ";
                                    sLine2 += mpcItem.EffectiveUnitPrice.ToString() + ", ";
                                    sLine2 += mpcItem.UnitType.ToString() + ", ";
                                    sLine2 += mpcItem.AlternateId.ToString() + ", ";
                                    sLine2 += mpcItem.BillableQuantity.ToString() + ", ";
                                    sLine2 += mpcItem.BillingFrequency.ToString() + ", ";
                                    sLine2 += mpcItem.PricingCurrency.ToString() + ", ";
                                    sLine2 += mpcItem.PCToBCExchangeRate.ToString() + ", ";
                                    sLine2 += mpcItem.PCToBCExchangeRateDate.ToString() + ", ";
                                    sLine2 += mpcItem.MeterDescription.ToString() + ", ";
                                    sLine2 += mpcItem.ReservationOrderId.ToString() + ", ";
                                    sLine2 += mpcItem.CreditReasonCode.ToString() + ", ";
                                    sLine2 += mpcItem.SubscriptionStartDate.ToString() + ", ";
                                    sLine2 += mpcItem.SubscriptionEndDate.ToString() + ", ";
                                    sLine2 += mpcItem.ReferenceId.ToString() + ", ";
                                    sLine2 += mpcItem.ProductQualifiers.ToString() + ", ";
                                    sLine2 += mpcItem.PromotionId.ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }

                            string sTransactionDatePeriod = mpcItem.ChargeStartDate.ToString() + " - " + mpcItem.ChargeEndDate.ToString();
                            sTransactionDatePeriod += "ђ " + sInvoiceId;

                            string sChargeType = mpcItem.ChargeType;

                            sDescription += sChargeType + "ђ" + sTransactionDatePeriod;

                            if (iCount == 1)
                            {
                                string sLine1 = "";

                                if (rbtnSeats.Checked == true)
                                {
                                    sLine1 = "PartnerId, PartnerName, CustomerId, CustomerName, CustomerDomainName, CustomerCountry, MpnId, InvoiceNumber, ProductId, SkuId, AvailabilityId, SkuName, ProductName, PublisherName, PublisherId, SubscriptionDescription, SubscriptionId, ChargeStartDate, ChargeEndDate, UsageDate, MeterType, MeterCategory, MeterId, MeterSubCategory, MeterName, MeterRegion, Unit, ResourceLocation, ConsumedService, ResourceGroup, ResourceURI, ChargeType, UnitPrice, Quantity, UnitType, BillingPreTaxTotal, BillingCurrency, PricingPreTaxTotal, PricingCurrency, ServiceInfo1, ServiceInfo2, Tags, AdditionalInfo, EffectiveUnitPrice, PCToBCExchangeRate, PCToBCExchangeRateDate, EntitlementId, EntitlementDescription, PartnerEarnedCreditPercentage, CreditPercentage, CreditType, BenefitOrderId, BenefitId, BenefitType";
                                }

                                if (rtbnUsage.Checked == true)
                                {
                                    sLine1 = "PartnerId, CustomerId, CustomerName, CustomerDomainName, CustomerCountry, InvoiceNumber, MpnId, OrderId, OrderDate, ProductId, SkuId, AvailabilityId, SkuName, ProductName, ChargeType, UnitPrice, Quantity, Subtotal, TaxTotal, Total, Currency, PriceAdjustmentDescription, PublisherName, PublisherId, SubscriptionDescription, SubscriptionId, ChargeStartDate, ChargeEndDate, TermAndBillingCycle, EffectiveUnitPrice, UnitType, AlternateId, BillableQuantity, BillingFrequency, PricingCurrency, PCToBCExchangeRate, PCToBCExchangeRateDate, MeterDescription, ReservationOrderId, CreditReasonCode, SubscriptionStartDate, SubscriptionEndDate, ReferenceId, ProductQualifiers, PromotionId";
                                }

                                if (bFistCustomer == false) AzureBillingDataL.Text += "<hr />";
                                bFistCustomer = false;

                                AzureBillingDataL.Text += "<font size='4'><b>" + sCustomerName + "</b></font><br />";

                                AzureBillingDataL.Text += "Markup: <b>#CUSTMARKUP#%</b>; Number of lines: <b>#LINESNUM#</b>; Total Amount: <b>#TOTALMS#</b>; Reseller Total Amount: <b>#TOTALRP#</b>; Reseller Total Diff.: <b>#TOTALRPDIFF#</b>";
                                AzureBillingDataL.Text += "<br /><br />";

                                AzureBillingDataL.Text += "<b><i>";
                                AzureBillingDataL.Text += sLine1;
                                AzureBillingDataL.Text += "</i></b>";
                                AzureBillingDataL.Text += "<br /><br />";

                                // customer comment
                                string sCustomerPermCommentFile = "CUSTOMERSComments.xml";
                                string sCustomerPermComment = ReadCustomerXml(sCustomerId, sCustomerPermCommentFile);
                                if ((sCustomerPermComment != "") && (sCustomerPermComment != "n/a"))
                                {
                                    PostSalesInvoiceLine commentLine = new PostSalesInvoiceLine();

                                    commentLine.lineType = "";
                                    commentLine.lineObjectNumber = "";
                                    commentLine.itemId = "";
                                    commentLine.Document_No = "";

                                    // quantity and price
                                    commentLine.quantity = 0;
                                    commentLine.unitPrice = 0;

                                    // extra line
                                    commentLine.description = sCustomerPermComment.Split(';')[1];

                                    // add extra line
                                    InvoiceLinesList.Add(commentLine);

                                    // count added lines
                                    iInvoiceLinesCount++;
                                }

                                // customer comment - temporary - after data acquision
                                string sCustComment = "";
                                if (sCustVatNo != null)
                                {
                                    if (sCustVatNo != "")
                                    {
                                        try
                                        {
                                            sCustComment = Request.Form["txtCommentCustomer_" + sCustId].ToString();
                                            if (sCustComment != "")
                                            {
                                                lastscriptdiv.InnerHtml += "document.getElementById(\"txtCommentCustomer_" + sCustId + "\").value = \"" + sCustComment.Replace("\"", "'") + "\";";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.ToString();
                                            sCustComment = "";
                                        }
                                    }
                                }
                                if (sCustComment != "")
                                {
                                    PostSalesInvoiceLine commentLine = new PostSalesInvoiceLine();

                                    commentLine.lineType = "";
                                    commentLine.lineObjectNumber = "";
                                    commentLine.itemId = "";
                                    commentLine.Document_No = "";

                                    // quantity and price
                                    commentLine.quantity = 0;
                                    commentLine.unitPrice = 0;

                                    // extra line
                                    commentLine.description = sCustComment;

                                    // add extra line
                                    InvoiceLinesList.Add(commentLine);

                                    // count added lines
                                    iInvoiceLinesCount++;
                                }
                            }

                            AzureBillingDataL.Text += iCount.ToString();
                            AzureBillingDataL.Text += "<br />";

                            AzureBillingDataL.Text += sLine2;
                            AzureBillingDataL.Text += "<br /><br />";

                            MarkupType.Text = "USAGE Type: MARKUP";
                            string sMarkupFile = "MARKUPUsage.xml";
                            string sMarkupType = "MARKUPUsage";

                            /*
                            if (rbtnSeats.Checked == true)
                            {
                                sMarkupFile = "MARKUPSeats.xml";
                                MarkupType.Text = "SEATS Type: MARKUP";
                                sMarkupType = "MARKUPSeats";

                                //MarkupType.Text = "SEATS Type: MARKUP";
                                //sMarkupFile = "MARKUPUsage.xml";
                                //sMarkupType = "MARKUPUsage";

                            }
                            */

                            if (rtbnUsage.Checked == true)
                            {

                                MarkupType.Text = "USAGE Type: MARKUP";
                                sMarkupFile = "MARKUPUsage.xml";
                                sMarkupType = "MARKUPUsage";

                                /*
                                if (sUnitType != "")
                                {
                                    MarkupType.Text = "USAGE Type: MARKUP";
                                    sMarkupFile = "MARKUPUsage.xml";
                                    sMarkupType = "MARKUPUsage";
                                }
                                else
                                {
                                    sMarkupFile = "MARKUPSeats.xml";
                                    MarkupType.Text = "USAGE Type: MARKUP";
                                    sMarkupType = "MARKUPSeats";
                                }
                                */
                            }

                            // RP Billing
                            string sMarkupData = ReadXml(sCustomerId, sMarkupFile);

                            string sMarkup = "n/a";
                            if (sMarkupData != "n/a")
                            {
                                sMarkup = sMarkupData.Split(';')[1];
                            }

                            // check if subscription is in the DB
                            if (sOfferId != "n/a")
                            {
                                string sMarkupDataDB = GetMarkupData(sOfferId.ToUpper(), sCustomerId.ToUpper(), sMarkupType);
                                if (sMarkupDataDB != "n/a")
                                {
                                    sMarkup = sMarkupDataDB;
                                }
                            }
                            else
                            {
                                if (sUnitType != "")
                                {
                                    string sMarkupDataDB = GetMarkupDataProductName("Azure plan", sCustomerId.ToUpper(), sMarkupType);
                                    if (sMarkupDataDB != "n/a")
                                    {
                                        sMarkup = sMarkupDataDB;
                                    }
                                }
                                else
                                {
                                    sOfferId = sProductIdUsage + ":" + sSkuIdUsage + ":" + sAvailabilityIdUsage;
                                    if (sOfferId.IndexOf("n/a") == -1)
                                    {
                                        string sMarkupDataDB = GetMarkupData(sOfferId.ToUpper(), sCustomerId.ToUpper(), sMarkupType);
                                        if (sMarkupDataDB != "n/a")
                                        {
                                            sMarkup = sMarkupDataDB;
                                        }
                                    }
                                }
                            }

                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#CUSTMARKUP#", sMarkup);

                            string sOfferNameToDisplay = "n/a";
                            string sOfferIdToDisplay = "n/a";
                            string sListPrice = "n/a";
                            string sERPPrice = "n/a";
                            decimal dListPrice = 0;
                            decimal dERPPrice = 0;

                            string sRPCP = "";
                            decimal dRPCP = 0;
                            decimal dRPCPDiff = 0;
                            string sRPCPDiff = "";
                            decimal dRPCPDiffSeats = 0;
                            string sRPCPDiffSeats = "";
                            string sRPCPPerUP = "";
                            decimal dRPCPPerUP = 0;
                            decimal dDollar = 1;
                            decimal dUnitPrice = 0;
                            decimal dEffectiveUnitPrice = 0;
                            decimal dMarkup = 0;
                            decimal dTotalAmount = 0;

                            try
                            {
                                dUnitPrice = 1;

                                if (mpcItem.UnitPrice != null)
                                {
                                    dUnitPrice = (decimal)mpcItem.UnitPrice;
                                }
                                else
                                {
                                    if (sUnitPrice != "n/a")
                                    {
                                        dUnitPrice = Convert.ToDecimal(sUnitPrice);
                                    }
                                }
                                sUnitPrice = dUnitPrice.ToString("N");
                            }
                            catch (Exception ex)
                            {
                                dUnitPrice = 1;
                                ex.ToString();
                            }

                            try
                            {
                                dEffectiveUnitPrice = 1;

                                if (mpcItem.EffectiveUnitPrice != null)
                                {
                                    dEffectiveUnitPrice = (decimal)mpcItem.EffectiveUnitPrice;
                                }
                                else
                                {
                                    if (sEffectiveUnitPrice != "n/a")
                                    {
                                        dEffectiveUnitPrice = Convert.ToDecimal(sEffectiveUnitPrice);
                                    }
                                }
                                sEffectiveUnitPrice = dEffectiveUnitPrice.ToString("N");
                            }
                            catch (Exception ex)
                            {
                                dEffectiveUnitPrice = 1;
                                ex.ToString();
                            }

                            dUnitPrice = dEffectiveUnitPrice;

                            try
                            {
                                dDollar = 1;
                                if (sDollarPrice != "n/a") dDollar = Convert.ToDecimal(sDollarPrice);
                            }
                            catch (Exception ex)
                            {
                                dDollar = 1;
                                ex.ToString();
                            }

                            try
                            {
                                dMarkup = 0;
                                if (sMarkup != "n/a") dMarkup = Convert.ToDecimal(sMarkup);
                                dCustRPUMarkup = dMarkup;
                                dCustRPRebate = dMarkup;
                            }
                            catch (Exception ex)
                            {
                                dMarkup = 0;
                                ex.ToString();
                            }

                            try
                            {
                                dTotalAmount = 0;
                                if (sTotalAmount != "n/a") dTotalAmount = Convert.ToDecimal(sTotalAmount);
                            }
                            catch (Exception ex)
                            {
                                dTotalAmount = 0;
                                ex.ToString();
                            }

                            // calculations
                            try
                            {
                                if (rbtnSeats.Checked == true)
                                {
                                    // i.e. 250 * ((100 + 25) / 100)
                                    dRPCP = dTotalAmount * ((100 + dMarkup) / 100);
                                    sRPCP = dRPCP.ToString("N");
                                }
                                else
                                {
                                    // i.e. 250 * ((100 + 25) / 100)
                                    //dRPCP = dTotalAmount * ((100 + dMarkup) / 100);
                                    //sRPCP = dRPCP.ToString("N");

                                    // first calculate unit price
                                    try
                                    {
                                        dRPCPPerUP = (dUnitPrice * dDollar) * ((100 + dMarkup) / 100);
                                        sRPCPPerUP = dRPCPPerUP.ToString("N");
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                        dRPCPPerUP = 0;
                                        sRPCPPerUP = "";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                                sRPCP = "";
                                dRPCP = 0;
                                sRPCPPerUP = "";
                                dRPCPPerUP = 0;
                            }

                            string sQuantityToShow = "";
                            string sBillableQuantityToShow = "";
                            decimal dQuantity = 0;
                            decimal dSubTotal = 0;
                            decimal dBillableQuantity = 0;
                            if (rbtnSeats.Checked == true)
                            {

                                /*
                                    sQuantityToShow = sQuantity;
                                    try
                                    {
                                        dQuantity = Convert.ToDecimal(sQuantity);
                                        sQuantityToShow = dQuantity.ToString("N");
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.ToString();
                                        sQuantityToShow = "0.00";
                                        dQuantity = 0;
                                    }
                                */

                                try
                                {
                                    dQuantity = dRPCP / (dUnitPrice * dDollar);
                                    sQuantityToShow = dQuantity.ToString("N");
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    sQuantityToShow = "0.00";
                                    dQuantity = 0;
                                }

                                try
                                {
                                    //dRPCPPerUP = dUnitPrice * dDollar;                                                                                
                                    dRPCPPerUP = dRPCP / dQuantity;
                                    sRPCPPerUP = dRPCPPerUP.ToString("N");
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    dRPCPPerUP = 0;
                                    sRPCPPerUP = "";
                                }
                            }
                            else
                            {
                                try
                                {
                                    dSubTotal = 0;
                                    if (sSubTotal != "")
                                    {
                                        dSubTotal = Convert.ToDecimal(sSubTotal);
                                    }
                                    sSubTotal = dSubTotal.ToString("N");

                                    dBillableQuantity = 0; // was dBillableQuantity = 1; ???
                                    if (sBillableQuantity != "")
                                    {
                                        try
                                        {
                                            dBillableQuantity = Convert.ToDecimal(sBillableQuantity);
                                        }
                                        catch (Exception ex)
                                        {
                                            dBillableQuantity = 0;
                                        }
                                    }
                                    sBillableQuantityToShow = dQuantity.ToString("N");

                                    dQuantity = 0;
                                    if (sQuantity != "")
                                    {
                                        dQuantity = Convert.ToDecimal(sQuantity);
                                        
                                        if (sUnitType != "")
                                        {
                                            if (dBillableQuantity > 0)
                                            {
                                                dQuantity = dBillableQuantity;
                                                sQuantityToShow = dQuantity.ToString("N");
                                            }
                                            else
                                            {
                                                sQuantityToShow = dQuantity.ToString("N");
                                            }
                                        }
                                        else
                                        {
                                            sQuantityToShow = dQuantity.ToString("N");
                                        }                                        
                                    }

                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    dQuantity = 0;
                                    sQuantityToShow = "0.00";
                                }

                                try
                                {
                                    //dQuantity = dRPCP / (dUnitPrice * dDollar);
                                    //sQuantityToShow = dQuantity.ToString("N");

                                    // backwards calc
                                    //dRPCP = dTotalAmount * ((100 + dMarkup) / 100);
                                    //sRPCP = dRPCP.ToString("N");

                                    dRPCP = dRPCPPerUP * dQuantity;
                                    sRPCP = dRPCP.ToString("N");

                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    //sQuantityToShow = "0.00";
                                    //dQuantity = 0;

                                    dRPCP = 0;
                                    sRPCP = "0.00";
                                }
                            }

                            dRPCPDiff = dRPCP - dSubTotal;
                            sRPCPDiff = dRPCPDiff.ToString("N");
                            dCustRPUDiffAmount += dRPCPDiff;

                            dRPCPDiffSeats = dRPCP - dTotalAmount;
                            sRPCPDiffSeats = dRPCPDiffSeats.ToString("N");
                            dCustRPUDiffAmountSeats += dRPCPDiffSeats;

                            dCustMSListAmount += (dListPrice * dQuantity);
                            dCustERPAmount += (dERPPrice * dQuantity);
                            dCustRPTotalAmount += dRPCP;

                            dCustMSUTotalAmount += dTotalAmount;
                            dCustMSUTotalAmountExcTax += dSubTotal;
                            dCustRPUTotalAmount += dRPCP;

                            // description
                            int iNavDescStart = -1;
                            if (sCustVatNo != "n/a")
                            {
                                if (true) // if (dRPCP != 0) - restrict invoice line to be pushed to nav
                                {
                                    if (true) // if (dQuantity != 0) - restrict invoice line to be pushed to nav
                                    {
                                        // create invoice line
                                        PostSalesInvoiceLine invoiceLine = new PostSalesInvoiceLine();

                                        invoiceLine.lineType = "Item";
                                        invoiceLine.lineObjectNumber = "2050.015"; // 310
                                        invoiceLine.itemId = sItem310Id;
                                        if (sItem310Id == "")
                                        {
                                            sItem310Id = GetItemId("2050.015");
                                            invoiceLine.itemId = sItem310Id;
                                        }

                                        // quantity and price
                                        invoiceLine.quantity = dQuantity;
                                        invoiceLine.unitPrice = dRPCPPerUP;

                                        // unit type
                                        invoiceLine.Document_No = sUnitType;

                                        // description
                                        string[] sLineDescriptionArray = sDescription.Split('ђ');
                                        string sLineDescription = sLineDescriptionArray[0];
                                        iNavDescStart = iInvoiceLinesCount;
                                        if (sLineDescription.Length <= 50)
                                        {
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
                                                    // include first 50 chars in the current line
                                                    invoiceLine.description = item.Value;

                                                    // add invoice line
                                                    InvoiceLinesList.Add(invoiceLine);

                                                    // count added lines
                                                    iInvoiceLinesCount++;
                                                }
                                                else
                                                {
                                                    if (rtbnUsage.Checked == true)
                                                    {
                                                        if (sUnitType == "")
                                                        {
                                                            PostSalesInvoiceLine extraLine = new PostSalesInvoiceLine();

                                                            extraLine.lineType = "";
                                                            extraLine.lineObjectNumber = "";
                                                            extraLine.itemId = "";

                                                            // quantity and price
                                                            extraLine.quantity = 0;
                                                            extraLine.unitPrice = 0;

                                                            extraLine.Document_No = "";

                                                            // extra line
                                                            extraLine.description = item.Value;

                                                            // add extra line
                                                            InvoiceLinesList.Add(extraLine);

                                                            // count added lines
                                                            iInvoiceLinesCount++;
                                                        }
                                                    }
                                                }
                                                iPartsCount++;
                                            }
                                        }

                                        // date + month
                                        if (rtbnUsage.Checked == true)
                                        {
                                            if (sUnitType == "")
                                            {
                                                for (int i = 1; i <= 2; i++)
                                                {
                                                    string sLineDescriptionDateMonth = sLineDescriptionArray[i];

                                                    PostSalesInvoiceLine extraLine = new PostSalesInvoiceLine();

                                                    extraLine.lineType = "";
                                                    extraLine.lineObjectNumber = "";
                                                    extraLine.itemId = "";

                                                    // quantity and price
                                                    extraLine.quantity = 0;
                                                    extraLine.unitPrice = 0;

                                                    extraLine.Document_No = "";

                                                    // extra line
                                                    extraLine.description = sLineDescriptionDateMonth;

                                                    // add extra line
                                                    InvoiceLinesList.Add(extraLine);

                                                    // count added lines
                                                    iInvoiceLinesCount++;
                                                }
                                            }
                                        }

                                        // extra empty line - exclude azure consumptions
                                        if (rtbnUsage.Checked == true)
                                        {
                                            if (sUnitType == "")
                                            {
                                                PostSalesInvoiceLine extraemptyLine = new PostSalesInvoiceLine();

                                                extraemptyLine.lineType = "";
                                                extraemptyLine.lineObjectNumber = "";
                                                extraemptyLine.itemId = "";
                                                extraemptyLine.Document_No = "";

                                                // quantity and price
                                                extraemptyLine.quantity = 0;
                                                extraemptyLine.unitPrice = 0;

                                                // extra line
                                                extraemptyLine.description = " ";

                                                // add extra line
                                                InvoiceLinesList.Add(extraemptyLine);

                                                // count added lines
                                                iInvoiceLinesCount++;
                                            }
                                        }
                                    }
                                }
                            }

                            string sCustomerNavDetails = "<font color='red'>Customer doesn't exist in NAV!</font>";
                            if (sCustVatNo != "n/a")
                            {
                                sCustomerNavDetails = "<font color='green'>No (VAT): " + sCustVatNo + "</font>";
                            }

                            sCSVLine = "";
                            sCSVLine += sCustomerName.Replace(",", ";") + ",";
                            sCSVLine += sCustId.Replace(",", ";") + ",";
                            sCSVLine += sCustVatNo.Replace(",", ";") + ",";

                            sCustomerCSVLine = "";
                            sCustomerCSVLine += sCustomerName.Replace(",", ";") + ",";
                            sCustomerCSVLine += sCustId.Replace(",", ";") + ",";
                            sCustomerCSVLine += sCustVatNo.Replace(",", ";") + ",";

                            sCustomerCSVName = sCustomerName;
                            sCustomerCSVId = sCustomerId;

                            string sDescriptionNavLines = "";
                            if (iNavDescStart != -1)
                            {
                                for (int iD = iNavDescStart; iD < iInvoiceLinesCount; iD++)
                                {
                                    sDescriptionNavLines += InvoiceLinesList[iD].description + "<br />";
                                    sCSVLine += InvoiceLinesList[iD].description.Replace(",", ";") + " ";
                                    sCustomerCSVLine += InvoiceLinesList[iD].description.Replace(",", ";") + " ";
                                }
                            }
                            sCSVLine += ",";
                            sCustomerCSVLine += ",";

                            AzureBillingDataL.Text += "<b>BC mapping:</b><br /><br />";
                            AzureBillingDataL.Text += "<b>Sell_to_Customer_No:</b> " + sCustomerId + " - " + sCustomerNavDetails + "<br />";
                            AzureBillingDataL.Text += "<b>Customer_Name:</b> " + sCustomerName + "<br />";
                            AzureBillingDataL.Text += "<b>Type:</b> ITEM<br />";
                            AzureBillingDataL.Text += "<b>No:</b> " + sProductNo + "<br />";
                            //AzureBillingDataL.Text += "<br /><b>Description:</b> " + sDescription.Replace("ђ", " ") + "<br />";
                            AzureBillingDataL.Text += "<br /><b>Description:</b><br />";
                            AzureBillingDataL.Text += sDescriptionNavLines;
                            AzureBillingDataL.Text += "<b>Unit Type:</b> " + sUnitType + "<br />";
                            AzureBillingDataL.Text += "<b>Quantity:</b> " + sQuantity + "<br />";
                            if (sUnitType != "")
                            {
                                AzureBillingDataL.Text += "<b>Billable Quantity:</b> " + sBillableQuantity + "<br />";
                            }
                            AzureBillingDataL.Text += "<b>Unit_Price:</b> " + sUnitPrice + "<br />";
                            AzureBillingDataL.Text += "<b>Effective_Unit_Price:</b> " + sEffectiveUnitPrice + "<br />";
                            AzureBillingDataL.Text += "<b>SubTotal:</b> " + sSubTotal + "<br />";
                            AzureBillingDataL.Text += "<b>Tax_Total:</b> " + sTaxTotal + "<br />";
                            AzureBillingDataL.Text += "<b>Total_Amount_VAT:</b> " + sTotalAmount + "<br />";

                            sCSVLine += sTotalAmount.Replace(",", "") + ",";
                            sCSVLine += sUnitPrice.Replace(",", "") + ",";

                            /*
                            if (ilItem is LicenseBasedLineItem)
                            {
                                AzureBillingDataL.Text += "<b>Offer Name:</b> " + sOfferNameToDisplay + "<br />";
                                AzureBillingDataL.Text += "<b>Offer Id:</b> " + sOfferIdToDisplay + "<br />";
                                AzureBillingDataL.Text += "<b>List Price:</b> " + dListPrice.ToString("N") + "<br />";
                                AzureBillingDataL.Text += "<b>ERP Price:</b> " + dERPPrice.ToString("N") + "<br />";
                            }
                            */

                            if (sDollarPrice != "n/a")
                            {
                                AzureBillingDataL.Text += "<b>PCToBCExchangeRate:</b> " + sDollarPrice + "<br />";
                            }
                            AzureBillingDataL.Text += "<br />";

                            sCSVLine += sDollarPrice.Replace(",", "") + ",";

                            AzureBillingDataL.Text += "<font color='#DF0000'><b>Quantity:</b></font> " + sQuantityToShow + "<br />";
                            AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup:</b></font> " + sMarkup + "%<br />";
                            AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Unit Price:</b></font> " + sRPCPPerUP + " DKK<br />";
                            AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Total Price:</b></font> " + sRPCP + " DKK<br />";
                            AzureBillingDataL.Text += "<font color='#DF0000'><b>Rackpeople Markup Diff:</b></font> " + sRPCPDiff + " DKK<br />";


                            sCSVLine += sQuantityToShow.Replace(",", "") + ",";
                            sCSVLine += sMarkup.Replace(",", "") + ",";
                            sCSVLine += sRPCPPerUP.Replace(",", "") + ",";
                            sCSVLine += sRPCP.Replace(",", "") + ",";
                            sCSVLine += sRPCPDiff.Replace(",", "");
                            sCSVFile.Add(sCSVLine);

                            sCustomerCSVLine += sQuantityToShow.Replace(",", "") + ",";
                            sCustomerCSVLine += sRPCP.Replace(",", "");

                            string sCSVTypeFile = "";
                            if (rbtnSeats.Checked == true)
                            {
                                sCSVTypeFile = "SEATS";
                                //sCSVTypeFile = "USAGE";
                            }
                            if (rtbnUsage.Checked == true)
                            {
                                sCSVTypeFile = "USAGE";

                                /*
                                if (sUnitType != "")
                                {
                                    sCSVTypeFile = "USAGE";
                                }
                                else
                                {
                                    sCSVTypeFile = "SEATS";
                                }
                                */
                            }

                            if (sCSVTypeFile == "USAGE")
                            {
                                sCustomerCSVUsageFile.Add(sCustomerCSVLine);
                            }
                            if (sCSVTypeFile == "SEATS")
                            {
                                sCustomerCSVSeatsFile.Add(sCustomerCSVLine);
                            }


                            AzureBillingDataL.Text += "<br />";
                            iCount++;


                        }

                        if (sRPInvoiceType == "usage")
                        {
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TM" + sCust + "#", dCustMSUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TC" + sCust + "#", dCustRPUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TD" + sCust + "#", dCustRPUDiffAmountSeats.ToString("N"));
                        }
                        else
                        {
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TM" + sCust + "#", dCustMSUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TMET" + sCust + "#", dCustMSUTotalAmountExcTax.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TC" + sCust + "#", dCustRPUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TD" + sCust + "#", dCustRPUDiffAmount.ToString("N"));
                        }

                        // sums
                        if (sRPInvoiceType == "usage")
                        {
                            dTCustMSUTotalAmount += dCustMSUTotalAmount;
                            dTCustMSUTotalAmountExcTax += dCustMSUTotalAmountExcTax;
                            dTCustRPUTotalAmount += dCustRPUTotalAmount;
                            dTCustRPUDiffAmount += dCustRPUDiffAmountSeats;
                            dTCustRPUMarkup += dCustRPUMarkup;
                        }
                        else
                        {
                            dTCustMSUTotalAmount += dCustMSUTotalAmount;
                            dTCustMSUTotalAmountExcTax += dCustMSUTotalAmountExcTax;
                            dTCustRPUTotalAmount += dCustRPUTotalAmount;
                            dTCustRPUDiffAmount += dCustRPUDiffAmount;
                            dTCustRPUMarkup += dCustRPUMarkup;
                        }

                        AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#LINESNUM#", (iCount - 1).ToString());

                        if (sRPInvoiceType == "usage")
                        {
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALMS#", dCustMSUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRP#", dCustRPUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRPDIFF#", dCustRPUDiffAmount.ToString("N"));
                        }
                        else
                        {
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALMS#", dCustMSUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRP#", dCustRPUTotalAmount.ToString("N"));
                            AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#TOTALRPDIFF#", dCustRPUDiffAmount.ToString("N"));
                        }

                        if (iCount > 0)
                        {
                            if (sAction == "Data")
                            {
                                PushDataToNavB.Enabled = true;
                                PushDataToNavB.Visible = true;
                                PushingDataL.Text = "";
                            }

                            if ((sAction == "BC") && ((sCustomerVATIdSingle == "ALL") || (sCustomerVATIdSingle == sCustId)))
                            {
                                PushDataToNavB.Enabled = false;
                                PushDataToNavB.Visible = false;
                                PushingDataL.Text = "Data pushed to BC.";

                                if (sCustomerVATIdSingle == sCustId)
                                {
                                    PushingDataL.Text = "Customer " + sCust + " (" + sCustVatNo + ") pushed to BC.";
                                }
                            }
                        }

                        if ((sAction == "BC") && ((sCustomerVATIdSingle == "ALL") || (sCustomerVATIdSingle == sCustId)))
                        {
                            if (iInvoiceLinesCount > 0)
                            {
                                // get real invoice line number
                                int iAzureInvoiceLinesCount = 0;
                                int iCompleteInvoiceLinesCount = 0;
                                if (rtbnUsage.Checked == true)
                                {
                                    foreach (PostSalesInvoiceLine sil in InvoiceLinesList)
                                    {
                                        if (sil.Document_No != "")
                                        {
                                            iAzureInvoiceLinesCount++;
                                        }
                                    }

                                    iCompleteInvoiceLinesCount = iInvoiceLinesCount - iAzureInvoiceLinesCount + 8;
                                }
                                else
                                {
                                    iCompleteInvoiceLinesCount = iInvoiceLinesCount + 3;
                                }

                                order.SalesLines = new PostSalesInvoiceLine[iCompleteInvoiceLinesCount];
                                for (int i = 0; i < iCompleteInvoiceLinesCount; i++)
                                {
                                    order.SalesLines[i] = new PostSalesInvoiceLine();
                                }

                                // post invoice now
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

                                int iOrderLinesCount = 0;

                                // add comment zero zero
                                order.SalesLines[iOrderLinesCount].itemId = "";
                                order.SalesLines[iOrderLinesCount].lineType = "";
                                order.SalesLines[iOrderLinesCount].lineObjectNumber = "";
                                order.SalesLines[iOrderLinesCount].quantity = 0;
                                order.SalesLines[iOrderLinesCount].unitPrice = 0;
                                order.SalesLines[iOrderLinesCount].description = "Name: " + sCust;
                                iOrderLinesCount++;

                                order.SalesLines[iOrderLinesCount].itemId = "";
                                order.SalesLines[iOrderLinesCount].lineType = "";
                                order.SalesLines[iOrderLinesCount].lineObjectNumber = "";
                                order.SalesLines[iOrderLinesCount].quantity = 0;
                                order.SalesLines[iOrderLinesCount].unitPrice = 0;
                                order.SalesLines[iOrderLinesCount].description = "CSP Id: " + sCustId;
                                iOrderLinesCount++;

                                order.SalesLines[iOrderLinesCount].itemId = "";
                                order.SalesLines[iOrderLinesCount].lineType = "";
                                order.SalesLines[iOrderLinesCount].lineObjectNumber = "";
                                order.SalesLines[iOrderLinesCount].quantity = 0;
                                order.SalesLines[iOrderLinesCount].unitPrice = 0;
                                order.SalesLines[iOrderLinesCount].description = "";
                                iOrderLinesCount++;

                                foreach (PostSalesInvoiceLine sil in InvoiceLinesList)
                                {
                                    bool bProcessLine = true;
                                    if (rtbnUsage.Checked == true)
                                    {
                                        if (sil.Document_No != "")
                                        {
                                            bProcessLine = false;
                                        }
                                    }

                                    if (bProcessLine == true)
                                    {
                                        order.SalesLines[iOrderLinesCount].itemId = sil.itemId;
                                        order.SalesLines[iOrderLinesCount].lineType = sil.lineType;
                                        order.SalesLines[iOrderLinesCount].lineObjectNumber = sil.lineObjectNumber;
                                        order.SalesLines[iOrderLinesCount].description = sil.description;
                                        order.SalesLines[iOrderLinesCount].unitPrice = sil.unitPrice;
                                        order.SalesLines[iOrderLinesCount].quantity = sil.quantity;
                                        iOrderLinesCount++;
                                    }
                                }

                                // add only one usage line
                                if (rtbnUsage.Checked == true)
                                {
                                    decimal dFinalPrice = 0;
                                    foreach (PostSalesInvoiceLine sil in InvoiceLinesList)
                                    {
                                        if (sil.Document_No != "")
                                        {
                                            dFinalPrice += sil.quantity * sil.unitPrice;
                                        }
                                    }

                                    if (sItem315Id == "")
                                    {
                                        sItem315Id = GetItemId("2050.020");
                                    }

                                    // item
                                    order.SalesLines[iOrderLinesCount].itemId = sItem315Id;

                                    // hardcoded
                                    order.SalesLines[iOrderLinesCount].lineType = "Item";

                                    // 310
                                    order.SalesLines[iOrderLinesCount].lineObjectNumber = "2050.020"; // 315

                                    // quantity
                                    order.SalesLines[iOrderLinesCount].quantity = 1;

                                    // unit price
                                    order.SalesLines[iOrderLinesCount].unitPrice = dFinalPrice;

                                    order.SalesLines[iOrderLinesCount].description = "Summarized Azure consumption for USAGE_" + sInvoiceId;

                                    iOrderLinesCount++;

                                    // add comment zero
                                    order.SalesLines[iOrderLinesCount].itemId = "";
                                    order.SalesLines[iOrderLinesCount].lineType = "";
                                    order.SalesLines[iOrderLinesCount].lineObjectNumber = "";
                                    order.SalesLines[iOrderLinesCount].quantity = 0;
                                    order.SalesLines[iOrderLinesCount].unitPrice = 0;
                                    order.SalesLines[iOrderLinesCount].description = "";
                                    iOrderLinesCount++;

                                    // add comment one
                                    order.SalesLines[iOrderLinesCount].itemId = "";
                                    order.SalesLines[iOrderLinesCount].lineType = "";
                                    order.SalesLines[iOrderLinesCount].lineObjectNumber = "";
                                    order.SalesLines[iOrderLinesCount].quantity = 0;
                                    order.SalesLines[iOrderLinesCount].unitPrice = 0;
                                    order.SalesLines[iOrderLinesCount].description = "Azure consumption is now summarized.";
                                    iOrderLinesCount++;

                                    // add comment two
                                    order.SalesLines[iOrderLinesCount].itemId = "";
                                    order.SalesLines[iOrderLinesCount].lineType = "";
                                    order.SalesLines[iOrderLinesCount].lineObjectNumber = "";
                                    order.SalesLines[iOrderLinesCount].quantity = 0;
                                    order.SalesLines[iOrderLinesCount].unitPrice = 0;
                                    order.SalesLines[iOrderLinesCount].description = "Please find you data in customer portal:";
                                    iOrderLinesCount++;

                                    // add comment three
                                    order.SalesLines[iOrderLinesCount].itemId = "";
                                    order.SalesLines[iOrderLinesCount].lineType = "";
                                    order.SalesLines[iOrderLinesCount].lineObjectNumber = "";
                                    order.SalesLines[iOrderLinesCount].quantity = 0;
                                    order.SalesLines[iOrderLinesCount].unitPrice = 0;
                                    order.SalesLines[iOrderLinesCount].description = "https://portal.rackpeople.com";
                                    iOrderLinesCount++;
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
                            }
                        }

                        // save customer csv file
                        if (sAction == "BC")
                        {
                            if (((sCustomerVATIdSingle == "ALL") || (sCustomerVATIdSingle == sCustId)))
                            {
                                // save usage
                                try
                                {
                                    string sCustomerCSVPathName = sCustVatNo + "_USAGE_" + sInvoiceId + ".csv";
                                    string sPath = HttpContext.Current.Server.MapPath("~") + "CSV\\" + sCustomerCSVPathName;
                                    using (var w = new StreamWriter(sPath))
                                    {
                                        foreach (string sLine in sCustomerCSVUsageFile)
                                        {
                                            w.WriteLine(sLine);
                                            w.Flush();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }

                                // save seats
                                try
                                {
                                    string sCustomerCSVPathName = sCustVatNo + "_SEATS_" + sInvoiceId + ".csv";
                                    string sPath = HttpContext.Current.Server.MapPath("~") + "CSV\\" + sCustomerCSVPathName;
                                    using (var w = new StreamWriter(sPath))
                                    {
                                        foreach (string sLine in sCustomerCSVSeatsFile)
                                        {
                                            w.WriteLine(sLine);
                                            w.Flush();
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

            AzureBillingDataL.Text += "</div>";

            // save csv file for all customers
            if (sAction == "BC")
            {
                try
                {
                    string sCSVFileType = "USAGE";
                    if (rbtnSeats.Checked == true) sCSVFileType = "SEATS";
                    string sCSVName = "AllCustomers_" + sCSVFileType + "_" + sInvoiceId + ".csv";
                    string sPath = HttpContext.Current.Server.MapPath("~") + "CSV\\" + sCSVName;
                    using (var w = new StreamWriter(sPath))
                    {
                        foreach (string sLine in sCSVFile)
                        {
                            w.WriteLine(sLine);
                            w.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            if (sRPInvoiceType == "usage")
            {
                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTM#", dTCustMSUTotalAmount.ToString("N"));
                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTC#", dTCustRPUTotalAmount.ToString("N"));
                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTD#", dTCustRPUDiffAmount.ToString("N"));
            }
            else
            {
                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTM#", dTCustMSUTotalAmount.ToString("N"));
                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTMET#", dTCustMSUTotalAmountExcTax.ToString("N"));
                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTC#", dTCustRPUTotalAmount.ToString("N"));
                AzureBillingDataL.Text = AzureBillingDataL.Text.Replace("#SUMTD#", dTCustRPUDiffAmount.ToString("N"));
            }

            lastscriptdiv.InnerHtml += "</script>";

            // close markups
            ClientScript.RegisterStartupScript(GetType(), "HideCustomers", "window.onload = function() { toogleMarkup(); }", true);

        }

        private string DoesCustomerExists(string filter)
        {
            string sResult = "n/a";
            string sCustomerNo = "n/a";
            string sCustomerCSP2 = "n/a";
            string sCustomerCSP3 = "n/a";

            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/ODataV4/Company('RackPeople ApS')/CustomerDetails?$filter=Microsoft_CSP_ID eq '" + filter + "'") as HttpWebRequest;
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
                            foreach (var cust in sExport.value)
                            {
                                sCustomerNo = cust.No;
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
                sCustomerNo = "n/a";
                sResult = "n/a";
            }

            if (sCustomerNo == "n/a")
            {
                try
                {
                    //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;

                    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/ODataV4/Company('RackPeople ApS')/CustomerDetails?$filter=Microsoft_CSP_ID2 eq '" + filter + "'") as HttpWebRequest;
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
                                foreach (var cust in sExport.value)
                                {
                                    sCustomerNo = cust.No;
                                    sCustomerCSP2 = "yes";
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
                    sCustomerNo = "n/a";
                    sResult = "n/a";
                }
            }

            if (sCustomerNo == "n/a")
            {
                try
                {
                    //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;

                    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/ODataV4/Company('RackPeople ApS')/CustomerDetails?$filter=Microsoft_CSP_ID3 eq '" + filter + "'") as HttpWebRequest;
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
                                foreach (var cust in sExport.value)
                                {
                                    sCustomerNo = cust.No;
                                    sCustomerCSP3 = "yes";
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
                    sCustomerNo = "n/a";
                    sResult = "n/a";
                }
            }

            if (sCustomerNo != "n/a")
            {
                try
                {
                    //System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;

                    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/customers?$filter=number eq '" + sCustomerNo + "'") as HttpWebRequest;
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
                                    sResult = cust.number + "ђ" + cust.id + "ђ" + cust.displayName + "ђ" + sCustomerCSP2 + "ђ" + sCustomerCSP3;
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
            }

            return sResult;
        }

        public async void AzureBillingDataB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (rbtnSeats.Checked == true)
            {
                await GetInvoiceData("Seats", "Data", "ALL");
            }

            if (rtbnUsage.Checked == true)
            {
                await GetInvoiceData("Usage", "Data", "ALL");
            }
        }

        protected async void PushDataToNavB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (rbtnSeats.Checked == true)
            {
                await GetInvoiceData("Seats", "BC", "ALL");
            }

            if (rtbnUsage.Checked == true)
            {
                await GetInvoiceData("Usage", "BC", "ALL");
            }

            // close markups
            ClientScript.RegisterStartupScript(GetType(), "HideCustomers", "window.onload = function() { toogleMarkup(); }", true);
        }

        protected void Unnamed_TextChanged1(object sender, EventArgs e)
        {
            if (Page.IsPostBack == true)
            {
                System.Web.UI.WebControls.TextBox tb = (System.Web.UI.WebControls.TextBox)sender;
                AttributeCollection ac = tb.Attributes;
                string sCustId = ac["CustId"];
                string sCustName = ac["CustName"];
                string sCustComment = tb.Text;

                string sCustomerCommentsFile = "CUSTOMERSComments.xml";

                if (sCustId != "")
                {
                    // update customer
                    UpdateCustomerXml(sCustId, sCustComment, sCustomerCommentsFile);
                }

                lastscriptdiv.InnerHtml = "<script>";
                lastscriptdiv.InnerHtml += "var lnk_obj = document.getElementById('cclink');";
                lastscriptdiv.InnerHtml += "var lnk_tbl = document.getElementById('customercomments');";
                lastscriptdiv.InnerHtml += "lnk_tbl.style.display = 'inline';";
                lastscriptdiv.InnerHtml += "lnk_obj.innerHTML = 'Close Customer Comments';";
                lastscriptdiv.InnerHtml += "</script>";
            }
        }

        protected void Unnamed_TextChanged(object sender, EventArgs e)
        {
            if (Page.IsPostBack == true)
            {
                System.Web.UI.WebControls.TextBox tb = (System.Web.UI.WebControls.TextBox)sender;
                AttributeCollection ac = tb.Attributes;
                string sCustId = ac["CustId"];
                string sProdId = ac["ProdId"];
                string sCustName = ac["CustName"];
                string sCustMarkup = tb.Text;

                string sMarkupFile = "MARKUPSeats.xml";
                string sMarkupType = "MARKUPSeats";
                if (rbtnSeats.Checked == true)
                {
                    sMarkupFile = "MARKUPSeats.xml";
                    MarkupType.Text = "SEATS Type: MARKUP";
                    sMarkupType = "MARKUPSeats";
                }
                if (rtbnUsage.Checked == true)
                {
                    MarkupType.Text = "USAGE Type: MARKUP";
                    sMarkupFile = "MARKUPUsage.xml";
                    sMarkupType = "MARKUPUsage";
                }

                if (sCustId != "")
                {
                    if (sProdId != "Customer")
                    {
                        // update product/subscription
                        string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                        System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                        dbConn.Open();
                        string sSql = "UPDATE[dbo].[MPCMarkups] ";
                        sSql += "SET [Markup] = " + sCustMarkup + " ";
                        sSql += "WHERE [ProductId] = '" + sProdId + "' AND [CustomerId] = '" + sCustId + "' AND [MarkupType]  = '" + sMarkupType + "'";
                        string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                        if (sDBResult != "DBOK")
                        {
                            PushingDataL.Text += sDBResult + " <br />";
                        }
                        dbConn.Close();
                    }
                    else
                    {
                        // update customer
                        UpdateXml(sCustId, sCustMarkup, sMarkupFile);

                        // update all customer's products
                        string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                        System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                        dbConn.Open();
                        string sSql = "UPDATE[dbo].[MPCMarkups] ";
                        sSql += "SET [Markup] = " + sCustMarkup + " ";
                        sSql += "WHERE [CustomerId] = '" + sCustId + "' AND [MarkupType]  = '" + sMarkupType + "'";
                        string sDBResult = InsertUpdateDatabase(sSql, dbConn);
                        if (sDBResult != "DBOK")
                        {
                            PushingDataL.Text += sDBResult + " <br />";
                        }
                        else
                        {
                            // update fields
                            HandleCustomersData();
                        }
                        dbConn.Close();
                    }

                    lastscriptdiv.InnerHtml = "<script>";
                    lastscriptdiv.InnerHtml += "var lnk_obj = document.getElementById('tmlink');";
                    lastscriptdiv.InnerHtml += "var lnk_tbl = document.getElementById('resellermarkup');";
                    lastscriptdiv.InnerHtml += "lnk_tbl.style.display = 'inline';";
                    lastscriptdiv.InnerHtml += "lnk_obj.innerHTML = 'Close Reseller Percentage';";
                    lastscriptdiv.InnerHtml += "</script>";
                }
            }
        }

        protected void rbtnSeats_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnSeats.Checked == true)
            {
                LBFileInfo.Visible = false;
            }
            else
            {
                LBFileInfo.Visible = false;
            }

            HandleCustomersData();
        }

        protected void rtbnUsage_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnSeats.Checked == true)
            {
                LBFileInfo.Visible = false;
            }
            else
            {
                LBFileInfo.Visible = false;
            }

            HandleCustomersData();
        }

        protected void InvoiceCommentTB_TextChanged(object sender, EventArgs e)
        {
            // update comment field
            string sCommentFile = "MARKUPSeats.xml";
            if (rbtnSeats.Checked == true)
            {
                sCommentFile = "MARKUPSeats.xml";
            }
            if (rtbnUsage.Checked == true)
            {
                sCommentFile = "MARKUPUsage.xml";
            }
            UpdateXml("D87883D1-AECE-48DE-8109-394F3A7E3EC2", InvoiceCommentTB.Text, sCommentFile);
        }
    }
}
