using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Models
{
    public class WafDecisionResult
    {
        public bool IsBlocked { get; set; }
        public string Reason { get; set; }
        public int HttpStatus { get; set; } = 403;
    }
}