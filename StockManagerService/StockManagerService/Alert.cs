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
    
    public partial class Alert
    {
        public Alert()
        {
            this.TimeAlerts = new HashSet<TimeAlert>();
        }
    
        public int alertID { get; set; }
        public int UID { get; set; }
        public int CID { get; set; }
        public string type { get; set; }
        public bool pending { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual PriceAlert PriceAlert { get; set; }
        public virtual ICollection<TimeAlert> TimeAlerts { get; set; }
        public virtual UserInfo UserInfo { get; set; }
    }
}
