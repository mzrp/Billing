
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace RackPeople.BillingAPI.Models
{

using System;
    using System.Collections.Generic;
    
public partial class TeleBillingImport
{

    public long Id { get; set; }

    public string Number { get; set; }

    public string Destination { get; set; }

    public int DestinationType { get; set; }

    public long NumberOfCalls { get; set; }

    public long DurationOfCalls { get; set; }

    public System.DateTime Imported { get; set; }

    public Nullable<System.DateTime> SentToNAV { get; set; }

    public long VendorId { get; set; }

    public string Direction { get; set; }

    public Nullable<decimal> Price { get; set; }

}

}
