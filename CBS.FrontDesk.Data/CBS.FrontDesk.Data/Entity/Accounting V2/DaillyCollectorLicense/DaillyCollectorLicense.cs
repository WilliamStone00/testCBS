using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense
{
    // DailyCollectorLicense.cs
    public class DailyCollectorLicense
    {
        public string Id { get; set; }
        public string CollectorUserId { get; set; }
        public string CollectorUserName { get; set; }
        public string CollectorCode { get; set; }
        public string BranchId { get; set; }
        public bool IsLicensed { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Message { get; set; }
        public string LicenseCode { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsRevoked { get; set; }
        public string BoundDeviceId { get; set; }
        public int MaxDevicesAllowed { get; set; }
        public int DevicesActivatedCount { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    // GenerateLicenseRequest.cs
    public class GenerateLicenseRequest
    {
        public string CollectorUserId { get; set; }
        public string BranchId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool DeactivateExistingActive { get; set; }
        public string DeviceId { get; set; }
    }

    // RevokeLicenseRequest.cs
    public class RevokeLicenseRequest
    {
        public string LicenseId { get; set; }
        public string Reason { get; set; }
    }

    // DeactivateLicenseRequest.cs
    public class DeactivateLicenseRequest
    {
        public string LicenseId { get; set; }
        public string Reason { get; set; }
    }

    // ReactivateLicenseRequest.cs
    public class ReactivateLicenseRequest
    {
        public string LicenseId { get; set; }
        public string Reason { get; set; }
    }

    // ActivateLicenseRequest.cs
    public class ActivateLicenseRequest
    {
        public string CollectorUserId { get; set; }
        public string LicenseCode { get; set; }
    }

    // CheckLicenseStatusRequest.cs
    public class CheckLicenseStatusRequest
    {
        public string CollectorUserId { get; set; }
        public string LicenseCode { get; set; }
    }

    // LicenseQuery.cs
    public class LicenseQuery
    {
        public string BranchId { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public DateTime? ExpiryFrom { get; set; }
        public DateTime? ExpiryTo { get; set; }
        public DataTableOptions dataTableOptions { get; set; }
        public LicenseQuery() { dataTableOptions = new DataTableOptions(); }
    }

    // Add this new model for the unified action request
    public class LicenseActionRequest
    {
        public string LicenseId { get; set; }
        public string ActionType { get; set; } // "Revoke", "Deactivate", "Reactivate", "Extend"
        public string Reason { get; set; }
        public DateTime? NewExpiryDate { get; set; }
    }

    // Keep existing models but add ExtendLicenseRequest
    public class ExtendLicenseRequest
    {
        public string LicenseId { get; set; }
        public DateTime NewExpiryDate { get; set; }
        public string Reason { get; set; }
    }
}
