using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RackPeople.BillingAPI.NAVCustomerService;
using System.Web.Http.Cors;
using RackPeople.BillingAPI.Policy;
//using WebApi.OutputCache.V2;
using RackPeople.BillingAPI.Models;
using Newtonsoft.Json;
using System.IO;
using RackPeople.BillingAPI.Services;

namespace RackPeople.BillingAPI.Controllers
{
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

    public class NavCustomersController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        private BCCustomers GetAllCustomers()
        {
            BCCustomers AllBCCustomers = new BCCustomers();
            BCService bcs = new BCService();

            string sAuthToken = bcs.GetBCToken();

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

        [Route("api/nav/customers")]
        //[CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 0)]
        public IEnumerable<Models.NavCustomer> Get()
        {
            var entities = new List<Models.NavCustomer>();

            BCCustomers AllBCCustomers = GetAllCustomers();

            try
            {
                foreach (var customer in AllBCCustomers.value)
                {
                    if (customer.displayName != "")
                    {
                        var entity = new Models.NavCustomer();
                        entity.Number = customer.number;
                        entity.Name = customer.displayName;
                        entities.Add(entity);
                    }
                }
            }
            catch (Exception)
            {
                // @todo Handle error
            }

            entities = entities.OrderBy(o => o.Name).ToList();

            return entities;
        }

        [Route("api/nav/customernames")]
        //[CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 0)]
        public IHttpActionResult GetNames()
        {
            var result = new List<String>();

            BCCustomers AllBCCustomers = GetAllCustomers();

            var subscriptions = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);

            // nav customers
            try
            {
                bool bDBChanged = false;
                foreach (var customer in AllBCCustomers.value)
                {
                    bool bSubscriptionFound = false;
                    bool bSubscriptionNameChanged = false;
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.NavCustomerId == customer.number)
                        {
                            bSubscriptionFound = true;
                            if (subscription.NavCustomerName != customer.displayName)
                            {
                                string sOldName = subscription.NavCustomerName;
                                subscription.NavCustomerName = customer.displayName;
                                db.Entry(subscription).State = System.Data.Entity.EntityState.Modified;
                                bSubscriptionNameChanged = true;
                                bDBChanged = true;
                                result.Add(String.Format("Customer Id:{0} name changed in NAV: Old name: '{1}' New name: '{2}'", subscription.NavCustomerId, sOldName, customer.displayName));
                                break;
                            }
                        }
                    }

                    if ((bSubscriptionFound == true) && (bSubscriptionNameChanged == false))
                    {
                        result.Add(String.Format("Customer Id:{0} name: '{1}' accurate.", customer.number, customer.displayName));
                    }
                }

                if (bDBChanged == true)
                {
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result.Add(ex.ToString());
            }

            // Render
            return Ok(result);
        }
    }
}
