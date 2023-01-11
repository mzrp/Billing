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
        public IHttpActionResult PutSubscription(int id, Subscription subscription) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (id != subscription.Id) {
                return BadRequest();
            }

            // log save button
            this.Audit(db, subscription, "saved subscription - FID: '{0}' NID: '{1}'.", subscription.FirstInvoice.ToString(), subscription.NextInvoice.ToString());

            // Find the original entry, and ensure only a few fields can be changed
            var org = db.Subscriptions.FirstOrDefault(e => e.Id == subscription.Id);
            if (org == null) {
                return NotFound();
            }

            // Add each of the new products
            foreach(var product in subscription.Products.Where(p => p.Id == 0)) {
                product.NavProductNumber = product.NavProductNumber.Substring(0,4) + "." + product.NavProductNumber.Substring(4);
                org.Products.Add(product);
                this.Audit(db, org, "added product '{0}: {1}'.", product.NavProductNumber, product.Description);
            }
            db.SaveChanges();

            // Delete products no longer wanted
            var ids = subscription.Products.Select(e => e.Id);
            var obsolete = org.Products.Where(p => !ids.Contains(p.Id)).ToArray();
            foreach(var product in obsolete) {
                db.Products.Remove(product);
                this.Audit(db, org, "removed product '{0}: {1}'.", product.NavProductNumber, product.Description);
            }
            db.SaveChanges();

            // Update modified projects
            foreach (var product in subscription.Products.Where(p => p.Id != 0)) {
                var src = org.Products.First(p => p.Id == product.Id);
                src.Description = product.Description;
                src.NavPrice = product.NavPrice;
                src.NavProductNumber = product.NavProductNumber;
                src.UnitAmount = product.UnitAmount;
                src.UnitPrice = product.UnitPrice;
                src.UnitType = product.UnitType;
                db.Entry(src).State = EntityState.Modified;
            }
            db.SaveChanges();

            // Save the changes made to the subscription, and detach 
            // it from the data context.
            db.Entry(org).State = EntityState.Modified;
            if (subscription.NavCustomerName != org.NavCustomerName) {
                this.Audit(db, subscription, "changed customer from '{0}' to '{1}'.", org.NavCustomerName, subscription.NavCustomerName);
            }
            if (subscription.BillingCycle != org.BillingCycle) {
                this.Audit(db, subscription, "changed billing cycle from '{0}' to '{1}'.", org.BillingCycle, subscription.BillingCycle);
            }

            if (subscription.PaymentTerms != org.PaymentTerms) {
                this.Audit(db, subscription, "changed payment terms from '{0}' to '{1}'.", org.PaymentTerms, subscription.PaymentTerms);
            }

            if (subscription.Description != org.Description) {
                this.Audit(db, subscription, "changed description terms from '{0}' to '{1}'.", org.Description, subscription.Description);
            }

            org.NavCustomerName = subscription.NavCustomerName;
            org.NavCustomerId = subscription.NavCustomerId; 
            org.BillingCycle = subscription.BillingCycle;
            org.FirstInvoice = subscription.FirstInvoice;
            org.NextInvoice = subscription.NextInvoice;
            org.PaymentTerms = subscription.PaymentTerms;
            org.Description = subscription.Description;
            org.AdditionalText = subscription.AdditionalText;

            db.SaveChanges();

            // log save button
            this.Audit(db, subscription, "saved subscription - FID: '{0}' NID: '{1}'.", subscription.FirstInvoice.ToString(), subscription.NextInvoice.ToString());

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