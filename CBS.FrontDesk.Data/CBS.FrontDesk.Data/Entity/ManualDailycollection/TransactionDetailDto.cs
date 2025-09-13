using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    public class TransactionDetailDto
    {
        public string Id { get; set; }
        public string MemberBranchCode { get; set; }
        public string MemberBranchId { get; set; }
        public string MemberBranchName { get; set; }
        public decimal Amount { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string AccountNumber { get; set; }
        public string DailyCollectorName { get; set; }
        public string UploadBy { get; set; }
        public string ManualEntryDailyCollectorId { get; set; }
        public string ProcessingStatus { get; set; }
        public string TreatementStatus { get; set; }
        public DateTime? TreatementDate { get; set; }
        public DateTime? AccountingDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ManualEntryDailyCollectionDetailDataTableOptions : DataTableOptions
    {
        // Inherits start, draw, length, etc.
    }
     
    public class GetManualEntryDailyCollectionDetailDataTableQuery
    {
        public GetManualEntryDailyCollectionDetailDataTableQuery()
        {
            Options = new ManualEntryDailyCollectionDetailDataTableOptions();
        }

        public ManualEntryDailyCollectionDetailDataTableOptions Options { get; set; }

        public string MemberBranchCode { get; set; }
        public string MemberBranchId { get; set; }
        public string MemberBranchName { get; set; }
        public decimal? Amount { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string AccountNumber { get; set; }
        public string DailyCollectorName { get; set; }
        public string UploadBy { get; set; }
        public string ManualEntryDailyCollectorId { get; set; }
        public string ProcessingStatus { get; set; }
        public string TreatementStatus { get; set; }
        public DateTime? TreatementDate { get; set; }
        public DateTime? AccountingDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }



}
