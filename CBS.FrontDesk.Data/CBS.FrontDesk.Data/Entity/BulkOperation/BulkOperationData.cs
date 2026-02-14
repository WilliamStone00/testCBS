using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class BulkOperationData
    {
        public string Id { get; set; }
        public string InitiatorBranchId { get; set; }
        public string InitiatorBranchName { get; set; }
        public string InitiatorBranchCode { get; set; }
        public string TransactionReference { get; set; }
        public string SimulationType { get; set; }
        public string Description { get; set; }
        public decimal TotalVolume { get; set; }
        public int TotalMembers { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovalStatusBadge { get; set; }
        public string FailedDescription { get; set; }
        public string ApprovalValidationDescription { get; set; }
        public DateTime ApprovalValidationDate { get; set; }
        public DateTime? AccountingDate { get; set; }
        public string BankCode { get; set; }
        public string BankId { get; set; }
        public string DestinationAccountId { get; set; }
        // public string EventCode { get; set; }
        public string BankName { get; set; }
        public string ApprovalBy { get; set; }
        public string BulkType { get; set; }
        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }
        public bool? ShouldImpactAccounting { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public List<BulkOperationDataDetails> BulkOperationSimulationDetails { get; set; }

    }

    public class GetAllSimulationDetailBySimulationIdRequestQuery
    {
        // Required
        public DataTableOptions Options { get; set; }
        public string SimulationId { get; set; }

        // Member Information
        public string MemberReference { get; set; }
        public string MemberName { get; set; }

        // Account Information
        public string SourceAccountNumber { get; set; }
        public string SourceAccountType { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string DestinationAccountType { get; set; }

        // Branch Information
        public string BranchId { get; set; }
        public string BranchCode { get; set; }

        // Status Information
        public string TransferStatus { get; set; }
        public string ApprovalStatus { get; set; }

        // Date Range Information
        public DateTime? TransferDateFrom { get; set; }
        public DateTime? TransferDateTo { get; set; }
        public DateTime? ApprovalDateFrom { get; set; }
        public DateTime? ApprovalDateTo { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }

        // Message Information
        public string TransferMessage { get; set; }

        // Additional helpful filters
        public string AccountType { get; set; } // For filtering both source and destination
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? IsSuccessful { get; set; }
        public string ExceptionCode { get; set; }
    }

    /// <summary>
    /// Query to retrieve paginated, filtered, and sortable bulk operation data for DataTable.
    /// </summary>
    public class GetBulkOperationDataTableQuery
    {
        /// <summary>
        /// DataTable options containing pagination, sorting, and search parameters.
        /// </summary>
        public DataTableOptions Options { get; set; }

        /// <summary>
        /// Optional filter to retrieve bulk operations from a specific branch.
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// Optional bulk operation status filter. 
        /// Possible values: "Open", "Closed", "Refinanced", "Restructured", "Rescheduled".
        /// Use "all" to include all statuses.
        /// </summary>
        public string Status { get; set; }

        public string SearchCriteria { get; set; }

        /// <summary>
        /// Optional start date to filter bulk operations based on the bulk operation creation date.
        /// Only bulk operations created on or after this date will be included.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date to filter bulk operations based on the bulk operation creation date.
        /// Only bulk operations created on or before this date will be included.
        /// </summary>
        public DateTime? EndDate { get; set; }

    }
}
