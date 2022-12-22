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
using RackPeople.BillingAPI.Services;

namespace RackPeople.BillingAPI.Controllers
{
    public class TeleAgreementsController : BaseController
    {
        private BillingEntities db = new BillingEntities();

        protected IQueryable<TeleAgreement> activeAgreements
        {
            get
            {
                return db.TeleAgreements.Include(a => a.Numbers)
                                        .Include(a => a.Numbers.Select(n => n.Products))
                                        .Where(x => x.Deleted == null);
            }
        }
        
        public IQueryable<TeleAgreement> GetTeleAgreements()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return this.activeAgreements;
        }

        public IHttpActionResult GetTeleAgreement(long id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            TeleAgreement agreement = this.activeAgreements.FirstOrDefault(x => x.Id == id);
            if (agreement == null) {
                return NotFound();
            }

            return Ok(agreement);
        }

        [ResponseType(typeof(void))]
        public IHttpActionResult PutTeleAgreement(long id, TeleAgreement model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (id != model.Id) {
                return BadRequest();
            }

            if (!TeleAgreementsExists(id)) {
                return NotFound();
            }

            // Modify the original agreement
            var agreement = db.TeleAgreements.Find(id);
            if (agreement.Description != model.Description || agreement.NavCustomerId != model.NavCustomerId || agreement.NavCustomerName != model.NavCustomerName) {
                agreement.Description = model.Description;
                agreement.NavCustomerId = model.NavCustomerId;
                agreement.NavCustomerName = model.NavCustomerName;
                this.Audit(db, agreement, "Updated the agreement");
            }

            // Check for number series that needs to be deleted
            foreach (var number in agreement.Numbers.ToList()) {
                if (!model.Numbers.ToList().Exists(n => n.Id == number.Id)) {
                    db.TeleProducts.RemoveRange(number.Products);
                    db.TeleNumberSeries.Remove(number);
                    this.Audit(db, agreement, "Removed number series '{0}' from agreement", number.Description);
                }
            }

            // Add each of the number series or modify as needed
            foreach(var number in model.Numbers) {
                if (number.Id > 0) {
                    var original = db.TeleNumberSeries.Find(number.Id);
                    original.Description = number.Description;
                    original.Numbers = number.Numbers;

                    // Go through each of the existing products
                    var current = number.Products.Select(p => p.Id).ToList();
                    foreach(var product in original.Products.ToList()) {
                        if (!current.Contains(product.Id)) {
                            db.TeleProducts.Remove(product);
                            this.Audit(db, agreement, "Removed product '{0}' from number series '{1}'", product.Description, number.Description);
                        } else {
                            db.Entry(product).State = EntityState.Detached;
                        }
                    }

                    // Update or create products
                    foreach(var product in number.Products) {
                        if (product.Id > 0) {
                            db.Entry(product).State = EntityState.Modified;
                        } else {
                            original.Products.Add(product);
                            this.Audit(db, agreement, "Added product '{0}' to number series '{1}'", product.Description, number.Description);
                        }
                    }
                } else {
                    agreement.Numbers.Add(number);
                    this.Audit(db, agreement, "Added number series '{0}' to agreement", number.Description);
                }
            }

            // Save all changes
            db.SaveChanges();
            
            return Ok(agreement);
        }

        [ResponseType(typeof(TeleAgreement))]
        public IHttpActionResult PostTeleAgreement(TeleAgreement agreement) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            db.TeleAgreements.Add(agreement);
            

            try {
                db.SaveChanges();

                this.Audit(db, agreement, "Created the agreement");
                db.SaveChanges();
            }
            catch (DbUpdateException) {
                if (TeleAgreementsExists(agreement.Id)) {
                    return Conflict();
                }
                else {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = agreement.Id }, agreement);
        }

        // DELETE: api/TeleAgreements/5
        [ResponseType(typeof(TeleAgreement))]
        public IHttpActionResult DeleteTeleAgreement(long id) {
            TeleAgreement agreement = this.activeAgreements.FirstOrDefault(x => x.Id == id);
            if (agreement == null) {
                return NotFound();
            }

            agreement.Deleted = new DateTime();
            db.SaveChanges();

            return Ok(agreement);
        }

        [HttpGet]
        [Route("api/teleAgreements/parseImport")]
        public IHttpActionResult ParseImport(string recipients = "bogholderi@rackpeople.dk", bool dryRun = false) {
            var service = new TeleBillingImportService(db);
            var result = service.Execute(dryRun);

            var lines = new string[] { };
            var nav = new NAVService();
            if (result != null && result.Count() > 0) {
                lines = result.Select(e => String.Format("Ny faktura til {0} for aftale {1} er klar i NAV", e.CustomerName, e.Reference)).ToArray();
                if (dryRun) {
                    nav.SendEmailToBookkeeping(recipients, "Tele Billing Invoices (TEST)", lines.ToArray());
                }
                else {
                    nav.SendEmailToBookkeeping(recipients, "Tele Billing Invoices", lines.ToArray());
                }
            }

            // Create a result container
            var o = new Dictionary<string, object>();
            o.Add("recipients", recipients);
            o.Add("dryRun", dryRun == true ? "true" : "false");
            o.Add("lines", lines);
            if (dryRun) {
                o.Add("invoices", result);
            }

            return Ok(o);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TeleAgreementsExists(long id)
        {
            return db.TeleAgreements.Count(e => e.Id == id) > 0;
        }
    }
}