//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockManagerService
{
    using System;
    using System.Collections.Generic;
    
    public partial class TimeAlert
    {
        public int alertID { get; set; }
        public System.TimeSpan time { get; set; }
    
        public virtual Alert Alert { get; set; }
    }
}
