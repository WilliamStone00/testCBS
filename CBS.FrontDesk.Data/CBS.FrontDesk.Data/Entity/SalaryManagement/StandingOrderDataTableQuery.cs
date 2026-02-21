using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SalaryManagement
{
    public class StandingOrderDataTableQuery
    {
        public DataTableOptions Options { get; set; }
        public StandingOrderDataTableQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string SourceAccountType { get; set; }
        public string DestinationAccountType { get; set; }
        public string Frequency { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public class StandingOrderExport
        {
            public string Id { get; set; }
            public string MemberId { get; set; }
            public string MemberName { get; set; }
            public decimal Amount { get; set; }
            public string SourceAccountType { get; set; }
            public string DestinationAccountType { get; set; }
            public string Purpose { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsActive { get; set; }
            public bool IsAutomatic { get; set; }
            public string Frequency { get; set; }
            public string Priority { get; set; }
            public string BranchId { get; set; }
            public string BranchCode { get; set; }
            public string BranchName { get; set; }
            public string UserName { get; set; }
            public bool ExternalAccount { get; set; }
            public string ExternalAccountNumber { get; set; }
            public string ExternalAccountHolderName { get; set; }
            public string PersonalNote { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CreatedBy { get; set; }
            public DateTime ModifiedDate { get; set; }
            public string ModifiedBy { get; set; }
        }

        public class ExportTableRequest
        {
            public List<StandingOrderExport> Data { get; set; }
            public int TotalRecords { get; set; }
            public ExportOptions ExportOptions { get; set; }
        }

        public class ExportOptions
        {
            public string FileName { get; set; }
            public string Format { get; set; } // excel, csv, pdf
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public bool IncludeSummary { get; set; }
            public string ExportType { get; set; } // filtered, all
        }

    }
}
