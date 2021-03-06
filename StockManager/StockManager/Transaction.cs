//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockManager
{
    using System;
    using System.Collections.Generic;
    
    public partial class Transaction
    {
        public int TransID { get; set; }
        public string Type { get; set; }
        public Nullable<int> NoOfShares { get; set; }
        public Nullable<decimal> PricePerShare { get; set; }
        public Nullable<int> PID { get; set; }
        public Nullable<int> CID { get; set; }
        public Nullable<System.DateTime> DateTime { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Portfolio Portfolio { get; set; }
    }
}
