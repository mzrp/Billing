using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RackPeople.BillingAPI.Models
{
    public partial class TeleProduct : Auditable
    {
        public override long AuditRecordId
        {
            get
            {
                return this.Id;
            }
        }

        public override string AuditRecordType
        {
            get
            {
                return "TeleProduct";
            }
        }
    }
}