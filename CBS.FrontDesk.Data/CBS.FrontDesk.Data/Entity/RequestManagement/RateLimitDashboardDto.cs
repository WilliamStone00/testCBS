using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{
    public class RateLimitDashboardDto
    {
        public List<KeyValuePair<string, int>> TopIps { get; set; }
        public List<KeyValuePair<string, int>> TopUsers { get; set; }
        public List<KeyValuePair<string, int>> TopPaths { get; set; }
        public List<KeyValuePair<string, int>> BlockedStatus { get; set; }
        public List<KeyValuePair<string, int>> HourlyRequests { get; set; }
    }

    public class GetRateLimitDashboardQuery 
    {
        public string BranchId { get; set; }
        public string QueryTopValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class GetRateLimitLogsByKeyQuery
    {
        public string Key { get; set; } // IP, Username, Path, or Blocked Status
        public string KeyType { get; set; } // "IP", "User", "Path", or "Blocked"
        public string BranchId { get; set; }
        public string QueryTopValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
