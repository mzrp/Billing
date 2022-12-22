using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RackPeople.BillingAPI.Models
{
    public interface IAuditable
    {
        /// <summary>
        /// Returns the id of the record that is being audited
        /// </summary>
        /// <returns></returns>
        long AuditRecordId { get; }

        /// <summary>
        /// Returns the type of the record
        /// </summary>
        string AuditRecordType { get; }
    }

    abstract public class Auditable: IAuditable
    {
        [JsonIgnore]
        public abstract long AuditRecordId { get; }

        [JsonIgnore]
        public virtual string AuditRecordType {
            get { return this.GetType().Name;  }
        }

        public Audit[] AuditHistory {
            get {
                var db = new BillingEntities();
                var history = from audit in db.Audits
                              where audit.ObjectType == this.AuditRecordType && 
                                    audit.ObjectId == this.AuditRecordId
                              select audit;

                return history.ToArray();
            }
        }
    }
}