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
        public string id { get; set; }
        public string memberBranchCode { get; set; }
        public string memberBranchId { get; set; }
        public string memberBranchName { get; set; }
        public decimal amount { get; set; }
        public string memberName { get; set; }
        public string memberReference { get; set; }
        public string accountNumber { get; set; }
        public string dailyCollectorName { get; set; }
        public string uploadBy { get; set; }
        public string manualEntryDailyCollectorId { get; set; }
        public string processingStatus { get; set; }
        public string treatementStatus { get; set; }
        public DateTime treatementDate { get; set; }
        public DateTime accountingDate { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
       
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
