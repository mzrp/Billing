using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using TeleBilling_v02_.Models;
using TeleBilling_v02_.Models.DisplayModels;
using TeleBilling_v02_.Repository;
using TeleBilling_v02_.Repository.Navision;

namespace TeleBilling_v02_.Controllers
{
    public class AgreementController : Controller
    {
        IAgreementRepository agreementRepository;
        IFileRepository fileRepository;
        ICustomerInfo2Repository customerInfo2Repository;
        public AgreementController()
        {
            this.agreementRepository = new AgreementRepository(new DBModelsContainer());
            this.fileRepository = new FileRepository(new DBModelsContainer());

            //CustomerInfo2_Service service = new CustomerInfo2_Service();
            //service.Credentials= new NetworkCredential("rpnavapi", "Telefon1", "Gowingu");
            //this.customerInfo2Repository = new CustomerInfo2Repository(service);
        }
        public AgreementController(IAgreementRepository agreementRepository, IFileRepository fileRepository, ICustomerInfo2Repository customerInfo2Repository)
        {
            this.agreementRepository = agreementRepository;
            this.fileRepository = fileRepository;
            this.customerInfo2Repository = customerInfo2Repository;
        }

        // POST: Agreement
        [HttpPost]
        public ActionResult ViewAllAgreements(FormCollection collection)
        {
            var agreements = agreementRepository.GetAgreements();

            if (!string.IsNullOrEmpty(collection["item.Status"]))
            {
                string sAllIds = collection["item.Status"];
                sAllIds = "," + sAllIds + ",";

                foreach (var agreement in agreements)
                {
                    using (var db = new DBModelsContainer())
                    {
                        agreement.Status = false;
                        if (sAllIds.IndexOf("," + agreement.Id.ToString() + ",") != -1)
                        {
                            agreement.Status = true;
                        }

                        var result = db.AgreementSet.Where(x => x.Id == agreement.Id).FirstOrDefault();
                        result.Status = agreement.Status;

                        db.SaveChanges();
                    }
                }
            }

            return View(agreements);
        }

        // GET: Agreement
        public ActionResult ViewAllAgreements(string s="", string st="")
        {
            var agreements = agreementRepository.GetAgreements();
            if(agreements == null)
            {
                return HttpNotFound();
            }

            if (st == "active")
            {
                agreements = agreements.Where(x => x.Status == true);
            }

            if (st == "inactive")
            {
                agreements = agreements.Where(x => x.Status == false);
            }

            if (s == "cust")
            {
                agreements = agreements.OrderBy(x => x.Customer_name);
            }

            if (s == "dt")
            {
                agreements = agreements.OrderBy(x => x.Date);
            }

            return View(agreements);
        }

        public ActionResult ViewAgreementDetails(int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);

            return View(agreement);
        }

        public ActionResult ViewAgreementZones(int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);
            var agreementsZones = agreementRepository.GetAgreementZones(agreementId).ToList();

            return View(agreementsZones);
        }

        /*
        public ActionResult ViewDetails(int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);

            try
            {
                System.Web.HttpContext.Current.Session["sesAgreementId"] = agreementId.ToString();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            var priceFile_ZoneRecords = agreement.CSVFile.ZoneRecords.ToList();
            var agreements_ZoneRecords = agreement.ZoneRecords.ToList();
            AgreementDisplay display = new AgreementDisplay
            {
                Customer_cvr = agreement.Customer_cvr,
                Customer_name = agreement.Customer_name,
                Description = agreement.Description,
                Status = agreement.Status,
                Date = agreement.Date,
                Subscriber_range_start = agreement.Subscriber_range_start,
                Subscriber_range_end = agreement.Subscriber_range_end
            };

            display.ZoneRecords = new List<AgreementZoneRecords>();
            foreach (ZoneRecords zone in priceFile_ZoneRecords)
            {
                if (agreements_ZoneRecords.Any(x => x.Name == zone.Name))
                {
                    ZoneRecords az = agreements_ZoneRecords.Find(x => x.Name == zone.Name);

                    AgreementZoneRecords d = new AgreementZoneRecords();
                    d.Id = zone.Id;
                    d.Country_code = zone.Country_code;
                    d.Name = zone.Name;
                    d.Minute_price_Supplier = zone.Minute_price;
                    d.Call_price_Supplier = zone.Call_price;
                    d.Minute_price_RP = az.Minute_price;
                    d.Call_price_RP = az.Call_price;

                    display.ZoneRecords.Add(d);
                }
            }
           
            return View(display);
        }
        */

        [HttpPost]
        public ActionResult UpdateCustomerDetails()
        {
            string sCustDescription = Request.Form["Description"];
            string sCustCVR = Request.Form["Customer_cvr"];
            string sCustName = Request.Form["Customer_name"];
            string sCustDate = Request.Form["Date"];
            string sCustStatus = Request.Form["Status"];
            string sCustRangeStart = Request.Form["Subscriber_range_start"];
            string sCustRangeEnd = Request.Form["Subscriber_range_end"];

            int agreementId = -1;
            if (System.Web.HttpContext.Current.Session["sesAgreementId"] != null)
            {
                agreementId = Convert.ToInt32(System.Web.HttpContext.Current.Session["sesAgreementId"]);
            }

            if (agreementId != -1)
            {
                using (var db = new DBModelsContainer())
                {
                    Agreement agreement = agreementRepository.GetAgreement(agreementId);

                    agreement.Status = false;
                    if (sCustStatus != null)
                    {
                        if (sCustStatus.ToLower() == "false")
                        {
                            agreement.Status = false;
                        }
                        if (sCustStatus.ToLower() == "true,false")
                        {
                            agreement.Status = true;
                        }
                    }

                    var result = db.AgreementSet.Where(x => x.Id == agreement.Id).FirstOrDefault();
                    result.Status = agreement.Status;
                    if (sCustDescription != null) result.Description = sCustDescription;
                    if (sCustCVR != null) result.Customer_cvr = sCustCVR;
                    if (sCustName != null) result.Customer_name = sCustName;
                    try
                    {
                        if (sCustDate != null)
                        {
                            // 08-04-2019 12:01:19
                            string sDay = sCustDate.Substring(0, 2);
                            string sMonth = sCustDate.Substring(3, 2);
                            string sYear = sCustDate.Substring(6, 4);
                            string sHour = sCustDate.Substring(11, 2);
                            string sMinute = sCustDate.Substring(14, 2);
                            string sSecond = sCustDate.Substring(17, 2);

                            DateTime dtNew = new DateTime(Convert.ToInt32(sYear), Convert.ToInt32(sMonth), Convert.ToInt32(sDay), Convert.ToInt32(sHour), Convert.ToInt32(sMinute), Convert.ToInt32(sSecond));

                            result.Date = dtNew;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                    if (sCustRangeStart != null) result.Subscriber_range_start = sCustRangeStart;
                    if (sCustRangeEnd != null) result.Subscriber_range_end = sCustRangeEnd;

                    db.SaveChanges();
                }
            }

            return RedirectToAction("ViewDetails", "Agreement", new { itemid = -1, agreementId = agreementId });
        }

        [HttpPost]
        public ActionResult ViewZoneDetails()
        {
            string sBulkValue = "-1";

            if (Request.Form["NewBulkCallPriceRP"] != null)
            {
                sBulkValue = Request.Form["NewBulkCallPriceRP"].ToString();

                // db edit now
                System.Web.HttpContext.Current.Session["sesBulkCallRPValue"] = sBulkValue;
            }
            else
            {
                System.Web.HttpContext.Current.Session["sesBulkCallRPValue"] = "n/a";
                sBulkValue = "-1";
            }

            int agreementId = -1;
            if (System.Web.HttpContext.Current.Session["sesAgreementId"] != null)
            {
                agreementId = Convert.ToInt32(System.Web.HttpContext.Current.Session["sesAgreementId"]);
            }

            if (agreementId != -1)
            {
                if (sBulkValue != "-1")
                {
                    decimal dBulkValue = -1;

                    try
                    {
                        dBulkValue = Convert.ToDecimal(sBulkValue);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        dBulkValue = -1;
                    }

                    if (dBulkValue != -1)
                    {
                        using (var db = new DBModelsContainer())
                        {
                            var ZoneRecordsSetResult = db.ZoneRecordsSet.Where(x => x.AgreementId == agreementId);
                            foreach (var ZoneRecordsSet in ZoneRecordsSetResult)
                            {
                                ZoneRecordsSet.Call_price = dBulkValue;
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }

            return RedirectToAction("ViewDetails", "Agreement", new { itemid = -1, agreementId = agreementId });
        }

        public ActionResult ViewDetails(int itemid, int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);
            int iItemId = itemid;

            if (itemid == -1)
            {
                // do something
            }

            try
            {
                System.Web.HttpContext.Current.Session["sesAgreementId"] = agreementId.ToString();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            var priceFile_ZoneRecords = agreement.CSVFile.ZoneRecords.ToList();
            var agreements_ZoneRecords = agreement.ZoneRecords.ToList();
            AgreementDisplay display = new AgreementDisplay
            {
                Customer_cvr = agreement.Customer_cvr,
                Customer_name = agreement.Customer_name,
                Description = agreement.Description,
                Status = agreement.Status,
                Date = agreement.Date,
                Subscriber_range_start = agreement.Subscriber_range_start,
                Subscriber_range_end = agreement.Subscriber_range_end
            };

            display.ZoneRecords = new List<AgreementZoneRecords>();
            foreach (ZoneRecords zone in priceFile_ZoneRecords)
            {
                if (agreements_ZoneRecords.Any(x => x.Name == zone.Name))
                {
                    ZoneRecords az = agreements_ZoneRecords.Find(x => x.Name == zone.Name);

                    AgreementZoneRecords d = new AgreementZoneRecords();
                    d.Id = zone.Id;
                    d.Country_code = zone.Country_code;
                    d.Name = zone.Name;
                    d.Minute_price_Supplier = zone.Minute_price;
                    d.Call_price_Supplier = zone.Call_price;
                    d.Minute_price_RP = az.Minute_price;
                    d.Call_price_RP = az.Call_price;
                    d.Customer_name = agreement.Customer_name;
                    d.Customer_cvr = agreement.Customer_cvr;

                    display.ZoneRecords.Add(d);
                }
            }

            return View(display);
        }

        public string GetBCToken()
        {
            string sResult = "n/a";

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                string sDate = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                sDate += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                sDate += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                sDate += DateTime.Now.AddMinutes(10).Hour.ToString().PadLeft(2, '0') + ":";
                sDate += DateTime.Now.AddMinutes(10).Minute.ToString().PadLeft(2, '0') + ":";
                sDate += DateTime.Now.AddMinutes(10).Second.ToString().PadLeft(2, '0') + ".000";

                string strSqlQuery = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[BCLoginLog] WHERE [TokenExpiresAt] > '" + sDate + "' ORDER BY Id DESC";
                System.Data.OleDb.OleDbDataReader oleReader;
                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSqlQuery, dbConn);
                oleReader = cmd.ExecuteReader();
                if (oleReader.Read())
                {
                    if (!oleReader.IsDBNull(1))
                    {
                        string sAuthToken = oleReader.GetString(1);
                        string sTokenType = oleReader.GetString(2);
                        int lExpiresIn = oleReader.GetInt32(3);
                        DateTime dExpiresAt = oleReader.GetDateTime(4);

                        if (DateTime.Now.AddMinutes(15) < dExpiresAt)
                        {
                            sResult = sAuthToken;
                        }
                    }
                }
                oleReader.Close();

                dbConn.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = "n/a";
            }

            return sResult;
        }

        private BCCustomers GetAllCustomers()
        {
            BCCustomers AllBCCustomers = new BCCustomers();

            string sAuthToken = GetBCToken();

            if (sAuthToken != "n/a")
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

                    var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Production/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/customers") as HttpWebRequest;
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
                                AllBCCustomers = JsonConvert.DeserializeObject<BCCustomers>(sExportAsJson);
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

            return AllBCCustomers;
        }

        private string GetCustomerName(string filter)
        {
            string sResult = "n/a";

            string sAuthToken = GetBCToken();

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
                                sResult = cust.displayName;
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



        public ActionResult CreateAgreement()
        {
            Agreement agreement = new Agreement();
            agreement.Date = DateTime.Now;
            //agreement.AgreementZoneRecords = fileRepository.GetFileByName(fileId).AgreementZoneRecords;
            //List<Supplier> list = agreementRepository.GetSuppliers().ToList();
            //ViewBag.SupplierList = new SelectList(list, "Id", "Name");

            int typeId = fileRepository.GetType("PriceFile").Id;
            List<CSVFile> fileList = fileRepository.GetCsvFileByTypeId(typeId).ToList();

            int iRemoveIPVision = -1;
            for(int i=0; i<fileList.Count; i++)
            {
                if (fileList[i].Name.IndexOf("IPVision") != -1)
                {
                    iRemoveIPVision = i;
                    break;
                }
            }
            if (iRemoveIPVision != -1)
            {
                fileList.RemoveAt(iRemoveIPVision);
            }

            ViewBag.FileList = new SelectList(fileList, "Id", "Name");

            //List<CustomerInfo2> customerList = customerInfo2Repository.GetCustomers();
            //ViewBag.CustomerList = new SelectList(customerList, "No", "Name");

            BCCustomers AllBCCustomers = GetAllCustomers();
            ViewBag.CustomerList = new SelectList(AllBCCustomers.value, "number", "displayName");

            string username = Session["UserName"].ToString();

            if (username == "")
            {
                agreement.UserId = 5;
            }
            else
            {
                var user = fileRepository.GetUser(username);
                if (user != null)
                {
                    agreement.UserId = user.Id;
                }
                else
                {
                    return Json(new { success = false, message = "User not found!!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            return View(agreement);
        }

        ICollection<ZoneRecords> zoneReords = new List<ZoneRecords>();
        
        [HttpPost]
        public ActionResult Create(Agreement model)
        {

            model.Customer_name = GetCustomerName(model.Customer_cvr);
            zoneReords= fileRepository.GetFileZoneDetails(model.CSVFileId).ToList();

            bool existed = agreementRepository.GetAgreements().ToList().Any(x=> Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(model.Subscriber_range_start)
                                                                             && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(model.Subscriber_range_end));
            
            if (zoneReords.Count >= 0) {

                if (zoneReords.Count > 0)
                {
                    foreach (ZoneRecords line in zoneReords)
                    {
                        ZoneRecords temp = new ZoneRecords
                        {
                            Name = line.Name,
                            Minute_price = line.Minute_price + ((line.Minute_price * 25) / 100),
                            Call_price = line.Call_price + ((line.Call_price * 25) / 100)
                        };

                        model.ZoneRecords.Add(temp);
                    }
                }

                using (var db = new DBModelsContainer())
                {
                    try
                    {
                        if (existed)
                        {
                            return Json(new { success = false, message = "the agreements is already in the database" }, JsonRequestBehavior.AllowGet);
                        }
                        db.AgreementSet.Add(model);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }

                return RedirectToAction("ViewAllAgreements");
            }
            return View("Error");
        }

        public ActionResult DeActivateAgreement(int agreementId)
        {
            return View();
        }
    }
}