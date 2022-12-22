using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace RackPeople.BillingAPI.Models
{
    public partial class Product: Auditable
    {
        override public long AuditRecordId {
            get { return (long)this.Id; }
        }

        public override string AuditRecordType
        {
            get
            {
                return "HostingProduct";
            }
        }
    }
}