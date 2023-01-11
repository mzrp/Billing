using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RackPeople.BillingAPI.NAVProductService;
using System.Web.Http.Cors;
//using WebApi.OutputCache.V2;
using RackPeople.BillingAPI.Models;
using Newtonsoft.Json;
using RackPeople.BillingAPI.Services;
using System.IO;

namespace RackPeople.BillingAPI.Controllers
{
    public class BCProducts
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        public List<BCProduct> value { get; set; }
    }

    public class BCProduct
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
    public class NavProductsController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        private BCProducts GetAllProducts()
        {
            BCProducts AllBCProducts = new BCProducts();
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

                    var webRequestAUTH = WebRequest.Create("https://api.businesscentral.dynamics.com/v2.0/74df0893-eb0e-4e6e-a68a-c5ddf3001c1f/RP-Test/api/v2.0/companies(9453c722-de43-ed11-946f-000d3ad96c72)/items") as HttpWebRequest;
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
                                AllBCProducts = JsonConvert.DeserializeObject<BCProducts>(sExportAsJson);
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

            return AllBCProducts;
        }

        [Route("api/nav/products")]
        //[CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 0)]
        public IEnumerable<Models.NavProduct> Get()
        {
            var entities = new List<Models.NavProduct>();

            BCProducts AllBCProducts = GetAllProducts();

            try {
                
                foreach (var p in AllBCProducts.value) {
                    var entity = new Models.NavProduct();
                    if (p.number.Length == 8)
                    {
                        entity.Number = p.number.Replace(".", "");
                        entity.Name = p.displayName;
                        entity.UnitPrice = p.unitCost;
                        entity.UnitType = p.baseUnitOfMeasureCode;
                        entities.Add(entity);
                    }
                }
            }
            catch (Exception) {
                // @todo Handle this exception
            }

            entities = entities.OrderBy(o => o.Name).ToList();

            return entities;
        }

        [Route("api/nav/productprices")]
        //[CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 0)]
        public IHttpActionResult GetPrices()
        {
            var result = new List<String>();

            BCProducts AllBCProducts = GetAllProducts();

            var subscriptions = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);

            try
            {
                bool bDBChanged = false;

                foreach (var p in AllBCProducts.value)
                {
                    bool bProductFound = false;
                    bool bProductPriceChanged = false;
                    foreach (var subscription in subscriptions)
                    {
                        foreach (var subproduct in subscription.Products)
                        {
                            if (subproduct.NavProductNumber == p.number)
                            {
                                bProductFound = true;
                                if (subproduct.NavPrice != p.unitCost)
                                {
                                    decimal oldPrice = subproduct.NavPrice;
                                    var src = subscription.Products.First(pid => pid.Id == subproduct.Id);
                                    src.NavPrice = p.unitCost;

                                    if (src.UnitPrice == oldPrice)
                                    {
                                        src.UnitPrice = p.unitCost;
                                    }

                                    db.Entry(src).State = System.Data.Entity.EntityState.Modified;
                                    
                                    bProductPriceChanged = true;
                                    bDBChanged = true;
                                    result.Add(String.Format("Product Id:{0} prices changed in NAV: Subscription: '{1}' Old subscription price: '{2}' New subscription price: '{3}'", p.number, subscription.NavCustomerName, oldPrice.ToString(), p.unitCost.ToString()));

                                    break;
                                }
                            }
                        }
                    }

                    if ((bProductFound == true) && (bProductPriceChanged == false))
                    {
                        result.Add(String.Format("Product Id:{0} price: '{1}' accurate for all subscriptions.", p.number, p.unitCost));
                    }

                    if (bDBChanged == true)
                    {
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                // @todo Handle this exception
            }

            // Render
            return Ok(result);
        }
    }
}
