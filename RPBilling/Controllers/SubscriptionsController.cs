using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RackPeople.BillingAPI.Models;
using System.Web.Http.Cors;

namespace RackPeople.BillingAPI.Controllers
{
    public class SubscriptionsController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        protected IQueryable<Subscription> activeSubscriptions {
            get {
                return db.Subscriptions.Include(x => x.Products).Where(x => x.Deleted == null);
            }
        }

        // GET: api/Subscriptions
        public IQueryable<Subscription> GetSubscriptions()
        {
            db.Configuration.ProxyCreationEnabled = false;

            // dave csv file now
            string sCSVData = "NavCustomerName,NavCustomerId,Description,BillingCycle,UnitAmount,NavPrice,UnitPrice\n";
            foreach(var singlesub in this.activeSubscriptions)
            {
                foreach (var subproduct in singlesub.Products) {
                    sCSVData += "\"" + singlesub.NavCustomerName + "\",";
                    sCSVData += "\"" + singlesub.NavCustomerId + "\",";
                    sCSVData += "\"" + singlesub.Description + "\",";
                    sCSVData += "\"" + singlesub.BillingCycle + "\",";

                    sCSVData += "\"" + subproduct.UnitAmount.ToString() + "\",";
                    sCSVData += "\"" + subproduct.NavPrice.ToString() + "\",";
                    sCSVData += "\"" + subproduct.UnitPrice.ToString() + "\"\n";
                }
            }
            
            try
            {
                string sCSVFilePath = System.Web.HttpContext.Current.Server.MapPath("~") + "\\RPBillingSubscriptions.csv";
                System.IO.File.WriteAllText(sCSVFilePath, sCSVData);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return this.activeSubscriptions;
        }

        // GET: api/Subscriptions/5
        [ResponseType(typeof(Subscription))]
        public IHttpActionResult GetSubscription(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            Subscription subscription = this.activeSubscriptions.Where(x => x.Id == id).FirstOrDefault();
            if (subscription == null)
            {
                return NotFound();
            }
            
            return Ok(subscription);
        }

        // PUT: api/Subscriptions/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSubscription(int id, Subscription subscription, string username = "n/a") {
            
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (id != subscription.Id) {
                return BadRequest();
            }

            // log save button
            //this.Audit(db, subscription, username + " saved subscription - FID: '{0}' NID: '{1}'.", subscription.FirstInvoice.ToString(), subscription.NextInvoice.ToString());

            // Find the original entry, and ensure only a few fields can be changed
            var org = db.Subscriptions.FirstOrDefault(e => e.Id == subscription.Id);
            if (org == null)
            {
                return NotFound();
            }

            string sResult = "";
            sResult += "0";

            try
            {

                sResult += "1";

                // Add each of the new products
                foreach (var product in subscription.Products.Where(p => p.Id == 0))
                {
                    if (product.NavProductNumber.Length == 7)
                    {
                        // 1010010 -> 1010.010
                        product.NavProductNumber = product.NavProductNumber.Substring(0, 4) + "." + product.NavProductNumber.Substring(4);
                    }

                    if (product.UnitType.Length == 7)
                    {
                        // 1010010 -> 1010.010
                        product.UnitType = product.UnitType.Substring(0, 4) + "." + product.UnitType.Substring(4);
                    }
                    org.Products.Add(product);
                    this.Audit(db, org, username + " added product '{0}: {1}'.", product.NavProductNumber, product.Description);
                }
                db.SaveChanges();

                sResult += "2";

                // Delete products no longer wanted
                var ids = subscription.Products.Select(e => e.Id);
                var obsolete = org.Products.Where(p => !ids.Contains(p.Id)).ToArray();
                foreach (var product in obsolete)
                {
                    db.Products.Remove(product);
                    this.Audit(db, org, username + " removed product '{0}: {1}'.", product.NavProductNumber, product.Description);
                }
                db.SaveChanges();

                sResult += "3";

                // Update modified projects
                foreach (var product in subscription.Products.Where(p => p.Id != 0))
                {
                    var src = org.Products.First(p => p.Id == product.Id);
                    src.Description = product.Description;
                    src.NavPrice = product.NavPrice;
                    src.UnitAmount = product.UnitAmount;

                    if (product.NavProductNumber.Length == 7)
                    {
                        // 1010010 -> 1010.010
                        product.NavProductNumber = product.NavProductNumber.Substring(0, 4) + "." + product.NavProductNumber.Substring(4);
                    }

                    if (product.UnitType.Length == 7)
                    {
                        // 1010010 -> 1010.010
                        product.UnitType = product.UnitType.Substring(0, 4) + "." + product.UnitType.Substring(4);
                    }

                    src.NavProductNumber = product.NavProductNumber;

                    src.UnitPrice = product.UnitPrice;
                    src.UnitType = product.UnitType;

                    db.Entry(src).State = EntityState.Modified;
                }
                db.SaveChanges();

                sResult += "4";

                // Save the changes made to the subscription, and detach 
                // it from the data context.
                db.Entry(org).State = EntityState.Modified;
                if (subscription.NavCustomerName != org.NavCustomerName)
                {
                    this.Audit(db, subscription, username + " changed customer from '{0}' to '{1}'.", org.NavCustomerName, subscription.NavCustomerName);
                }
                if (subscription.BillingCycle != org.BillingCycle)
                {
                    this.Audit(db, subscription, username + " changed billing cycle from '{0}' to '{1}'.", org.BillingCycle, subscription.BillingCycle);
                }

                if (subscription.PaymentTerms != org.PaymentTerms)
                {
                    this.Audit(db, subscription, username + " changed payment terms from '{0}' to '{1}'.", org.PaymentTerms, subscription.PaymentTerms);
                }

                if (subscription.Description != org.Description)
                {
                    this.Audit(db, subscription, username + " changed description terms from '{0}' to '{1}'.", org.Description, subscription.Description);
                }

                sResult += "5";

                org.NavCustomerName = subscription.NavCustomerName;
                org.NavCustomerId = subscription.NavCustomerId;
                org.BillingCycle = subscription.BillingCycle;

                bool bFirstInvoiceIsChanged = true;
                TimeSpan tsFirstInvoice = org.FirstInvoice.Subtract(subscription.FirstInvoice);
                if (org.FirstInvoice < subscription.FirstInvoice)
                {
                    tsFirstInvoice = subscription.FirstInvoice.Subtract(org.FirstInvoice);
                }
                if ((tsFirstInvoice.Days == 0) && (tsFirstInvoice.Hours <= 2))
                {
                    bFirstInvoiceIsChanged = false;
                }
                if (bFirstInvoiceIsChanged == true)
                {
                    org.FirstInvoice = subscription.FirstInvoice;
                }

                bool bNextInvoiceIsChanged = true;
                TimeSpan tsNextInvoice = org.NextInvoice.Subtract(subscription.NextInvoice);
                if (org.NextInvoice < subscription.NextInvoice)
                {
                    tsNextInvoice = subscription.NextInvoice.Subtract(org.NextInvoice);
                }
                if ((tsNextInvoice.Days == 0) && (tsNextInvoice.Hours <= 2))
                {
                    bNextInvoiceIsChanged = false;
                }
                if (bNextInvoiceIsChanged == true)
                {
                    org.NextInvoice = subscription.NextInvoice;
                }

                org.PaymentTerms = subscription.PaymentTerms;
                org.Description = subscription.Description;
                org.AdditionalText = subscription.AdditionalText;
                org.AdditionalRPText = subscription.AdditionalRPText;

                // log save button
                this.Audit(db, subscription, username + " saved subscription - FID: '{0}' NID: '{1}'.", org.FirstInvoice.ToString(), org.NextInvoice.ToString());

                db.SaveChanges();

                sResult += "6";

                sResult += "7";
            }
            catch (Exception ex)
            {
                sResult += ex.Message.ToString();
            }

            // Serve the changed subscription 
            db.Entry(org).Reload();
            return Ok(org);
        }

        // POST: api/Subscriptions
        [ResponseType(typeof(Subscription))]
        public IHttpActionResult PostSubscription(Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure the next invoice date gets generated
            subscription.NextInvoice = subscription.FirstInvoice;
            db.Subscriptions.Add(subscription);

            try {
                db.SaveChanges();
                this.Audit(subscription, "created the subscription.");
            }
            catch (DbUpdateException) {
                if (SubscriptionExists(subscription.Id)) {
                    return Conflict();
                }
                else {
                    throw;
                }
            }

            // Fix projects
            try
            {
                var org = db.Subscriptions.FirstOrDefault(e => e.Id == subscription.Id);

                foreach (var product in subscription.Products)
                {
                    var src = org.Products.First(p => p.Id == product.Id);
                    src.Description = product.Description;
                    src.NavPrice = product.NavPrice;
                    src.UnitAmount = product.UnitAmount;

                    if (product.NavProductNumber.Length == 7)
                    {
                        // 1010010 -> 1010.010
                        product.NavProductNumber = product.NavProductNumber.Substring(0, 4) + "." + product.NavProductNumber.Substring(4);
                    }

                    if (product.UnitType.Length == 7)
                    {
                        // 1010010 -> 1010.010
                        product.UnitType = product.UnitType.Substring(0, 4) + "." + product.UnitType.Substring(4);
                    }

                    src.NavProductNumber = product.NavProductNumber;

                    src.UnitPrice = product.UnitPrice;
                    src.UnitType = product.UnitType;

                    db.Entry(src).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            catch( Exception ex)
            {
                ex.ToString();
            }

            return CreatedAtRoute("DefaultApi", new { id = subscription.Id }, subscription);
        }

        // DELETE: api/Subscriptions/5
        [ResponseType(typeof(Subscription))]
        public IHttpActionResult DeleteSubscription(int id)
        {
            Subscription subscription = db.Subscriptions.Find(id);
            if (subscription == null)
            {
                return NotFound();
            }

            subscription.Deleted = DateTime.Now;
            db.Entry(subscription).State = EntityState.Modified;

            this.Audit(db, subscription, "cancelled the subscription.");
            db.SaveChanges();

            return Ok(subscription);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SubscriptionExists(int id)
        {
            return db.Subscriptions.Count(e => e.Id == id) > 0;
        }
    }
}