using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RackPeople.BillingAPI.Models
{
    public partial class Subscription: Auditable
    {
        override public long AuditRecordId
        {
            get { return (long)this.Id; }
        }

        public override string AuditRecordType
        {
            get
            {
                return "HostingSubscription";
            }
        }

        /// <summary>
        /// Return the number of months in the billing cycle
        /// </summary>
        public int MonthsInBillingCycle
        {
            get
            {
                switch (this.BillingCycle) {
                    case "Quaterly":
                        return 3;
                    case "Biannually":
                        return 6;
                    case "Annually":
                        return 12;
                }

                return 1;
            }
        }

        /// <summary>
        /// Gets the next billing periode, based on first invoice and billing cycle
        /// </summary>
        public DateTime BillingPeriod 
        {
            get
            {
                var offset = this.PaymentTerms.HasValue ? this.PaymentTerms.Value : 30; // 26 
                var first = this.FirstInvoice.Date; // 2020-02-01 
                var today = DateTime.Today.Date; // 2020-05-06 

                // Calculate the expected period
                var periode = new DateTime(first.Year, first.Month, first.Day); // 2020-02-01 

                // If the the current invoice date is in the past, we need to go to the next avilable period
                var nextInvoice = periode.AddDays(-Math.Abs(offset)); // 2020-01-06

                while (nextInvoice < today)
                {
                    periode = periode.AddMonths(this.MonthsInBillingCycle); // 2020-03-01 2020-04-01 2020-05-01 2020-06-01
                    nextInvoice = periode.AddDays(-Math.Abs(offset)); // 2020-02-04 2020-03-06 2020-04-05 2020-05-06
                }

                return periode;
            }
        }

        public DateTime InvoiceDate
        {
            get
            {
                var offset = this.PaymentTerms.HasValue ? this.PaymentTerms.Value : 30; // 26
                var date = this.BillingPeriod.AddDays(-Math.Abs(offset)); // 2020-03-31 
                return date;
            }
        }

        /// <summary>
        /// Checks if the current date matches the next invoice, or if the invoice date has passed.
        /// This makes it possible for the api to pickup failed invoices.
        /// </summary>
        /// <returns></returns>
        public bool IsDue() {
            var today = DateTime.Today.Date;
            return today.Date.Equals(this.InvoiceDate.Date); // TODAY (2020-03-05) : DUE
        }

        /// <summary>
        /// Updates the billing cycle, and the next invoice date
        /// </summary>
        /// <param name="cycle"></param>
        public void UpdateBillingCycle(string cycle, DateTime firstInvoice) {
            this.BillingCycle = cycle;

            switch (cycle) {
                case "Annually":
                    this.NextInvoice = firstInvoice.AddYears(1);
                    break;
                case "Biannually":
                    this.NextInvoice = firstInvoice.AddMonths(6);
                    break;
                case "Quaterly":
                    this.NextInvoice = firstInvoice.AddMonths(3);
                    break;
                case "Monthly":
                    this.NextInvoice = firstInvoice.AddMonths(1);
                    break;
            }
        }

        public void UpdateBillingCycle(string cycle) {
            this.UpdateBillingCycle(cycle, this.FirstInvoice);
        }

        /// <summary>
        /// For when the billing cycle hasn't changed, but the first invoice data has changed.
        /// </summary>
        public void UpdateBillingCycle(DateTime firstInvoice) {
            this.UpdateBillingCycle(this.BillingCycle, firstInvoice);
        }

        /// <summary>
        /// For when the billing cycle hasn't changed, but the first invoice data has changed.
        /// </summary>
        public void UpdateBillingCycle() {
            this.UpdateBillingCycle(this.BillingCycle, this.FirstInvoice);
        }
    }
}