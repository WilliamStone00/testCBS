using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{
    public class RateLimitedUser
    {

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string BlockType { get; set; } // "User-Based", "IP-Based"
        public string Reason { get; set; } // "Exceeded limit", etc.
        public DateTime BlockedAt { get; set; }
        public DateTime BlockEndTime { get; set; }
        public string BlockedBy { get; set; }
        public bool IsActive => DateTime.UtcNow <= BlockEndTime;

        public DateTime Timestamp { get; set; }
        public string Location { get; set; }
        public string BranchName { get; set; }
        public string BranchId { get; set; }
    }
    public class BlockUserCommand
    {
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string ComputerName { get; set; }
        public string BlockType { get; set; }
        public DateTime BlockEndTime { get; set; }
        public string BlockedBy { get; set; }
        public string Location { get; set; }
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public string Reason { get; set; }
    }

}
