using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RackPeople.BillingAPI.Models
{
    public class ImportNumberRange
    {
        public long NumberSeriesId;
        public long AgreementId;
        public List<long> Numbers = new List<long>();
    }
}