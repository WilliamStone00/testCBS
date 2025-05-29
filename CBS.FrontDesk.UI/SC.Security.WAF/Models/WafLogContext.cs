using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Models
{
    public class WafLogContext
    {
        public string Ip { get; set; }
        public string Username { get; set; }
        public string Path { get; set; }
        public string Reason { get; set; }
        public bool IsBlocked { get; set; }
        public string FullUrl { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}