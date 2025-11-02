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
        public DataTableOptions Options { get; set; }
        public FileListingQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime CreatedFromUtc { get; set; }
        public DateTime CreatedToUtc { get; set; }
        public bool IncludeDeleted { get; set; }

    }
}
