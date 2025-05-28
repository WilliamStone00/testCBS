using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Models
{
    public class ContentScanResultDto
    {
        public bool IsMalicious { get; set; }
        public string Reason { get; set; }
        public string MatchedPattern { get; set; }
        public string RawContent { get; set; }
    }
}