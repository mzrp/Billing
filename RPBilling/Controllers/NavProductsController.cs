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

namespace RackPeople.BillingAPI.Controllers
{
    public class NavProductsController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        [Route("api/nav/products")]
        //[CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 0)]
        public IEnumerable<Models.NavProduct> Get()
        {
            var service = new Vareoversigt_Service();
            service.Credentials = this.GetNetworkCredentials();

            var entities = new List<Models.NavProduct>();

            try {
                var filters = new Vareoversigt_Filter[] { };
                var products = service.ReadMultiple(filters, null, 0);
                
                foreach (var p in products) {
                    var entity = new Models.NavProduct();
                    entity.Number = p.No;
                    entity.Name = p.Description;
                    entity.UnitPrice = p.Unit_Price;
                    entity.UnitType = p.Base_Unit_of_Measure;
                    entities.Add(entity);
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

            var service = new Vareoversigt_Service();
            service.Credentials = this.GetNetworkCredentials();
            var subscriptions = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);

            try
            {
                var filters = new Vareoversigt_Filter[] { };
                var products = service.ReadMultiple(filters, null, 0);
                bool bDBChanged = false;

                foreach (var p in products)
                {
                    bool bProductFound = false;
                    bool bProductPriceChanged = false;
                    foreach (var subscription in subscriptions)
                    {
                        foreach (var subproduct in subscription.Products)
                        {
                            if (subproduct.NavProductNumber == p.No)
                            {
                                bProductFound = true;
                                if (subproduct.NavPrice != p.Unit_Price)
                                {
                                    decimal oldPrice = subproduct.NavPrice;
                                    var src = subscription.Products.First(pid => pid.Id == subproduct.Id);
                                    src.NavPrice = p.Unit_Price;

                                    if (src.UnitPrice == oldPrice)
                                    {
                                        src.UnitPrice = p.Unit_Price;
                                    }

                                    db.Entry(src).State = System.Data.Entity.EntityState.Modified;
                                    
                                    bProductPriceChanged = true;
                                    bDBChanged = true;
                                    result.Add(String.Format("Product Id:{0} prices changed in NAV: Subscription: '{1}' Old subscription price: '{2}' New subscription price: '{3}'", p.No, subscription.NavCustomerName, oldPrice.ToString(), p.Unit_Price.ToString()));

                                    break;
                                }
                            }
                        }
                    }

                    if ((bProductFound == true) && (bProductPriceChanged == false))
                    {
                        result.Add(String.Format("Product Id:{0} price: '{1}' accurate for all subscriptions.", p.No, p.Unit_Price));
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
