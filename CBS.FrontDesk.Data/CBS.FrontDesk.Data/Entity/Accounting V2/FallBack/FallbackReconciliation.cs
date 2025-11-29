using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.FallBack
{
    public class FallbackLogResponse
    {
        public string id { get; set; }
        public string branchId { get; set; }
        public string requestedAffiliateAccountIdOrBranchCode { get; set; }
        public string requestedBranchAccountCode { get; set; }
        public string supposedGlAccountId { get; set; }
        public string supposedGlAccountNumber { get; set; }
        public string supposedGlAccountName { get; set; }
        public string fallbackBranchAccountId { get; set; }
        public string fallbackBranchAccountNumber { get; set; }
        public bool isBranchAccountAutoCreated { get; set; }
        public string fallbackBranchAccountNameFr { get; set; }
        public string fallbackBranchAccountNameEn { get; set; }
        public decimal amount { get; set; }
        public string operationCode { get; set; }
        public string reference { get; set; }
        public string resolutionTips { get; set; }
        public bool isResolved { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? resolvedAt { get; set; }
        public string resolvedByUserId { get; set; }
        public string branchName { get; set; }
        public DateTime? createdOn { get; set; }
        public string fallbackBranchAccountName { get; set; }
        public DateTime? resolvedOn { get; set; }
        public string resolvedBy { get; set; }
    }

    public class FallbackLogQuery
    {
        public string BranchId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Status { get; set; }
        public string OperationCode { get; set; }
        public string Reference { get; set; }
        public string SupposedGlContains { get; set; }
        public string FallbackGlContains { get; set; }
        public DataTableOptions DataTable { get; set; }
        public FallbackLogQuery() { DataTable = new DataTableOptions(); }
    }

    public class ReconcileFallbackRequest
    {
        public string Id { get; set; }

        // Source (readonly)
        public string SourceGlAccountNumber { get; set; }
        public decimal SourceAmount { get; set; }
        public string SourceSit { get; set; }
        public string SourceDescription { get; set; }

        // Destination (editable)
        public string DestinationGlAccountNumber { get; set; }
        public decimal DestinationAmount { get; set; }
        public string DestinationSit { get; set; }
        public string DestinationDescription { get; set; }
    }

    public class ResolveFallbackRequest
    {
        public string Id { get; set; }
        public string SourceGlAccountNumber { get; set; }
        public decimal SourceAmount { get; set; }
        public string SourceSit { get; set; }
        public string SourceDescription { get; set; }
        public string DestinationGlAccountNumber { get; set; }
        public decimal DestinationAmount { get; set; }
        public string DestinationSit { get; set; }
        public string DestinationDescription { get; set; }
    }

    // Add these classes to your existing FallBack entity file

    public class FallbackExportRecord
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string OperationCode { get; set; }
        public string Reference { get; set; }
        public string SupposedGlAccountNumber { get; set; }
        public string SupposedGlAccountName { get; set; }
        public string FallbackBranchAccountNumber { get; set; }
        public string FallbackBranchAccountName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string ResolutionTips { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string ResolvedBy { get; set; }
        public bool IsBranchAccountAutoCreated { get; set; }
        public string AutoCreatedStatus { get; set; }
        public string RequestedAffiliateAccountIdOrBranchCode { get; set; }
        public string RequestedBranchAccountCode { get; set; }
    }

    public class FallbackExportRequest
    {
        public List<FallbackExportRecord> Data { get; set; }
        public int TotalRecords { get; set; }
        public FallbackExportOptions ExportOptions { get; set; }
        public FallbackLogQuery Filters { get; set; }
    }

    public class FallbackExportOptions
    {
        public string Format { get; set; } = "excel";
        public string ReportType { get; set; } = "detailed";
        public bool IncludeStatistics { get; set; } = true;
        public string FileName { get; set; } = "Fallback_Reconciliation_Report";
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class FallbackExportResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }
    }

}

