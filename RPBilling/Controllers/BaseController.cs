using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Threading;

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

            String userName = "";

            /*
            try
            {
                //userName = this.User.Identity.Name;

                WebClient webclient = new WebClient();
                Stream myStream = webclient.OpenRead(@"https://billing.gowingu.net/UserIdentity.aspx");
                StreamReader sr = new StreamReader(myStream);
                string sUserIdentity = sr.ReadToEnd();
                myStream.Close();

                // f43f4edb-7436-4561-89a0-d08c543767c0#$#Milan Zivic#$#<token>#$#2023-02-02 13:40:28
                string[] sUserIdentityArray = sUserIdentity.Split(new string[] { "#$#" }, StringSplitOptions.None);
                userName = sUserIdentityArray[1];

            }
            catch(Exception ex)
            {
                ex.ToString();
                userName = "";
            }

            string sDesc = format;
            if (userName == "")
            {
                sDesc = char.ToUpper(format[0]) + format.Substring(1);
            }
            */

            string sDesc = format;

            var audit = new Models.Audit();
            audit.ObjectId = record.AuditRecordId;
            audit.ObjectType = record.AuditRecordType;
            audit.Description = String.Format(String.Format("{0} {1}", userName, sDesc), args); ;
            audit.Created = DateTime.Now;
            db.Audits.Add(audit);
        }
    }
}