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

namespace RackPeople.BillingAPI.Controllers
{
    public class NavCustomersController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        [Route("api/nav/customers")]
        //[CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 0)]
        public IEnumerable<Models.NavCustomer> Get() {
            var entities = new List<Models.NavCustomer>();

            var service = new CustomerInfo2_Service();
            service.Credentials = this.GetNetworkCredentials();

            try 
            {
                var filters = new CustomerInfo2_Filter[] { };
                var customers = service.ReadMultiple(filters, null, 0);

                foreach (var customer in customers) {
                    var entity = new Models.NavCustomer();
                    entity.Number = customer.No;
                    entity.Name = customer.Name;
                    entities.Add(entity);
                }
            }
            catch (Exception) {
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

            var service = new CustomerInfo2_Service();
            service.Credentials = this.GetNetworkCredentials();
            var subscriptions = db.Subscriptions.Include("Products").Where(x => x.Deleted == null);

            // nav customers
            try
            {
                var filters = new CustomerInfo2_Filter[] { };
                var customers = service.ReadMultiple(filters, null, 0);
                bool bDBChanged = false;
                foreach (var customer in customers)
                {
                    bool bSubscriptionFound = false;
                    bool bSubscriptionNameChanged = false;
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.NavCustomerId == customer.No)
                        {
                            bSubscriptionFound = true;
                            if (subscription.NavCustomerName != customer.Name)
                            {
                                string sOldName = subscription.NavCustomerName;
                                subscription.NavCustomerName = customer.Name;
                                db.Entry(subscription).State = System.Data.Entity.EntityState.Modified;
                                bSubscriptionNameChanged = true;
                                bDBChanged = true;
                                result.Add(String.Format("Customer Id:{0} name changed in NAV: Old name: '{1}' New name: '{2}'", subscription.NavCustomerId, sOldName, customer.Name));
                                break;
                            }
                        }
                    }

                    if ((bSubscriptionFound == true) && (bSubscriptionNameChanged == false))
                    {
                        result.Add(String.Format("Customer Id:{0} name: '{1}' accurate.", customer.No, customer.Name));
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
