using CBS.FrontDesk.Data.Entity.DataTable;
using System;

namespace CBS.FrontDesk.Data.Entity.BulkOPerations
{
    public class MemberAccountsBulkOperations
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string SimulationType { get; set; }
        public decimal TotalVolume { get; set; }
        public int TotalMembers { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovalValidationDescription { get; set; }
        public DateTime ApprovalValidationDate { get; set; }

    }

    /// <summary>
    /// Query to retrieve paginated, filtered, and sortable bulk operation data for DataTable.
    /// </summary>
    public class GetBulkOperationDataTableQuery
    {
        /// <summary>
        /// DataTable options containing pagination, sorting, and search parameters.
        /// </summary>
        public DataTableOptions DataTableOptions { get; set; }

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
