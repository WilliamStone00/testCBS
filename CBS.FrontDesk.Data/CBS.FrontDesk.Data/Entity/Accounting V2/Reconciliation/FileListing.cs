using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation
{
    public class FileListing
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string Category { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string StorageKey { get; set; }
        public string Checksum { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string ErrorJson { get; set; }
        public int TrialBalanceRowCount { get; set; }
        public int MemberBalanceRowCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

    }

    public class FileListingQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public FileListingQuery() { DataTableOptions = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        //public bool? IncludeDeleted { get; set; }

    }

    public class FileUploadData
    {
        public string Id { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string StorageKey { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string ErrorJson { get; set; } = string.Empty;
        public int TrialBalanceRowCount { get; set; }
        public int MemberBalanceRowCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public List<TrialBalanceRow> TrialBalanceRows { get; set; } = new List<TrialBalanceRow>();
        public List<MemberBalanceRow> MemberBalanceRows { get; set; } = new List<MemberBalanceRow>();

        public class TrialBalanceRow
        {
            public string Id { get; set; } = string.Empty;
            public string BranchId { get; set; } = string.Empty;
            public string SourceCode { get; set; } = string.Empty;
            public string SourceName { get; set; } = string.Empty;
            public decimal OpeningDebit { get; set; }
            public decimal OpeningCredit { get; set; }
            public decimal MovementDebit { get; set; }
            public decimal MovementCredit { get; set; }
            public decimal EndDebit { get; set; }
            public decimal EndCredit { get; set; }
            public string BranchAccountId { get; set; } = string.Empty;
            public bool IsReconciled { get; set; }
            public bool IsCurrentlyActive { get; set; }
            public string FileUploadId { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class MemberBalanceRow
        {
            public string Id { get; set; } = string.Empty;
            public string BranchId { get; set; } = string.Empty;
            public string MemberId { get; set; } = string.Empty;
            public string AccountType { get; set; } = string.Empty;
            public string AccountNumber { get; set; } = string.Empty;
            public string TrialBalanceStagingId { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
            public string BranchName { get; set; } = string.Empty;
            public string RecocilationStatus { get; set; } = string.Empty;
            public string ReconciledBy { get; set; } = string.Empty;
            public decimal Balance { get; set; }
            public string FileUploadId { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}

