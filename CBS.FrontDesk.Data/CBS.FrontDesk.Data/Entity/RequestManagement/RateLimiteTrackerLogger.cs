using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{
    public class RateLimiteTrackerLogger
    {
        public string Id { get; set; }
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string ComputerName { get; set; }
        public string Path { get; set; }
        public int StatusCode { get; set; }
        public string Location { get; set; }
        public string WarningMessage { get; set; }
        public string BlockType { get; set; }
        public DateTime BlockEndTime { get; set; }
        public string Reason { get; set; }
        public bool IsBlocked { get; set; }

        // New fields added
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }

        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsAuthenticated { get; set; } // ✅ New
        public string FullUrl { get; set; }
    }
    public class LogRateLimitTrackerCommand
    {
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string ComputerName { get; set; }
        public string Path { get; set; }
        public int StatusCode { get; set; }
        public string Location { get; set; }
        public string WarningMessage { get; set; }
        public string BlockType { get; set; }
        public DateTime BlockEndTime { get; set; }
        public string Reason { get; set; }
        public bool IsBlocked { get; set; }
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
        public bool IsAuthenticated { get; set; } // ✅ New
        public string FullUrl { get; set; }

    }
    public class GetRateLimitTrackerDataTableQuery
    {
        public DataTableOptions Options { get; set; }

        // Optional Filters
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string BlockType { get; set; }
        public string Path { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
