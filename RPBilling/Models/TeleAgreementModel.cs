using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RackPeople.BillingAPI.Models
{
    public partial class TeleAgreement: Auditable
    {
        override public long AuditRecordId
        {
            get { return this.Id; }
        }

        public override string AuditRecordType
        {
            get
            {
                return "TeleAgreement";
            }
        }
    }
}