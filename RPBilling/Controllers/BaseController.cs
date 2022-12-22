using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RackPeople.BillingAPI.Controllers
{
    public class BaseController : ApiController {
        /// <summary>
        /// Returns the NAV api credentials
        /// </summary>
        /// <returns></returns>
        protected NetworkCredential GetNetworkCredentials() {
            var credentials = new NetworkCredential("rpnavapi", "Telefon1");
            return credentials;
        }

        /// <summary>
        /// Writes an audit entry, and saves it right away
        /// </summary>
        /// <param name="record"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected void Audit(Models.Auditable record, string format, params object[] args) {
            var db = new Models.BillingEntities();
            this.Audit(db, record, format, args);
            db.SaveChanges();
        }

        /// <summary>
        /// Add an audit entry to the passed context
        /// </summary>
        /// <param name="db"></param>
        /// <param name="record"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected void Audit(Models.BillingEntities db, Models.Auditable record, string format, params object[] args) {
            String userName = this.User.Identity.Name;
            if (String.IsNullOrEmpty(userName)) {
                userName = "Guest";
            }

            var audit = new Models.Audit();
            audit.ObjectId = record.AuditRecordId;
            audit.ObjectType = record.AuditRecordType;
            audit.Description = String.Format(String.Format("{0} {1}", userName, format), args); ;
            audit.Created = DateTime.Now;
            db.Audits.Add(audit);
        }
    }
}