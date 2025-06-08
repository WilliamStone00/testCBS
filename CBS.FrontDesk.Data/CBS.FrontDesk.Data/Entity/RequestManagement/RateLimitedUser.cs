using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{
    public class CheckRateLimitBlockQuery
    {
        public string IpOrUser { get; set; }
        public string Username { get; set; }
    }
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
        // New fields added
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }

        public string BranchCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Cidr { get; set; }
        public string Action { get; set; }
        public BlockRequest BlockRequest { get; set; } = new BlockRequest();
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
        // New fields added
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }

        public string BranchCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }

    }

    public class BlockRequest
    {
        /// <summary>
        /// The IP address of the user to be blocked.
        /// Required. Must be a valid IPv4 or IPv6 address format.
        /// </summary>
        [Required(ErrorMessage = "User IP address is required.")]
        [RegularExpression(@"^(([0-9]{1,3}\.){3}[0-9]{1,3}|([a-fA-F0-9:]+))$", ErrorMessage = "Invalid IP address format.")]
        public string UserIP { get; set; }

        /// <summary>
        /// The unique identifier of the user.
        /// Required. Should not exceed 100 characters.
        /// </summary>
        [Required(ErrorMessage = "User is required.")]
        [StringLength(100, ErrorMessage = "User ID cannot exceed 100 characters.")]
        public string UserId { get; set; }

        /// <summary>
        /// The reason for blocking the user.
        /// Required. Should be descriptive and within a 500-character limit.
        /// </summary>
        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; }

        /// <summary>
        /// The number of days to block the user.
        /// Must be a positive integer between 1 and 365.
        /// </summary>
        [Range(1, 365, ErrorMessage = "Block duration must be between 1 and 365 days.")]
        public int NumberOfDays { get; set; }
    }
    public class WafDashboardData
    {
        public SummaryStats Summary { get; set; }
        public List<BlockTrendPoint> BlockTrends { get; set; }
        public List<PieChartEntry> BlockReasons { get; set; }
        public List<PieChartEntry> SuspiciousPathByBranch { get; set; }
        public List<BarChartEntry> BlocksByBranch { get; set; }
        public List<BlockedUserEntry> BlockedUsers { get; set; }
    }
    public class SummaryStats
    {
        public int ActiveBlocks { get; set; }
        public string TopCountry { get; set; }
        public int BlockedIps { get; set; }
        public string TopReason { get; set; }
    }
    public class BlockTrendPoint
    {
        public int Day { get; set; }          // 1–30
        public int TSCCount { get; set; }
        public int NVCCount { get; set; }
    }
    public class PieChartEntry
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }
    public class BarChartEntry
    {
        public string Branch { get; set; }
        public int BlockCount { get; set; }
    }
    public class BlockedUserEntry
    {
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public string Country { get; set; }
        public string Branch { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
