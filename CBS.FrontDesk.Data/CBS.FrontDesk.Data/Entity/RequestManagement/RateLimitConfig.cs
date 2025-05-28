using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{
    public class RateLimitConfig
    {
        public string Id { get; set; }
        public int RequestLimit { get; set; }
        public int TimeWindowSeconds { get; set; }
        public int BlockDurationMinutes { get; set; }
        public int IPRequestLimit { get; set; }
        public int IPTimeWindowSeconds { get; set; }
        public List<string> WhitelistedHeaders { get; set; } = new List<string>();
        public string Action { get; set; }
    }


}
