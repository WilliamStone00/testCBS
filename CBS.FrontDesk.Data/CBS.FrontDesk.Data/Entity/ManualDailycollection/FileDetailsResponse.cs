using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    //public class FileDetailsResponse
    //{
    //    public string Id { get; set; }

    //    public string CollectorName { get; set; }

    //    public string BranchName { get; set; }

    //    public decimal TotalAmount { get; set; }

    //    public int TotalMember { get; set; }

    //    public string Status { get; set; }

    //    public string UploadedBy { get; set; }

    //    public string FileUploadId { get; set; }

    //    public List<TransactionDetail> Details { get; set; }
    //}

    //// This class represents one item in the "details" array
    //public class TransactionDetail
    //{
    //    public string MemberName { get; set; }

    //    public string AccountNumber { get; set; }

    //    public decimal Amount { get; set; }

    //    public string MemberBranchName { get; set; }
    //}

    public class FileDetailsResponse
    {
        public string Id { get; set; }
        public string BranchCode { get; set; }
        public string DailyCollectorTransitMemberReference { get; set; }
        public string CollectorId { get; set; }
        public string CollectorName { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalMember { get; set; }
        public int TotalBranchesCollected { get; set; } = 0;
        public string status { get; set; }
        public string UploadedBy { get; set; } = null;
        public DateTime? UploadDateTime { get; set; } = null;
        public string ReviewedBy { get; set; } = null;
        public DateTime? ReviewedDateTime { get; set; } = null;
        public string ReviewerStatement { get; set; } = null;
        public string ApprovedBy { get; set; } = null;
        public DateTime? ApprovalDateTime { get; set; } = null;
        public string ApprovalStatement { get; set; } = null;
        public DateTime? CashReceptionDate { get; set; }  = null;
        public string CashierName { get; set; } = null;
        public string CashierBranchId { get; set; } = null;
        public string CashierBranchName { get; set; } = null;
        public string CashierBranchCode { get; set; } = null;
        public decimal TotalCashInAmount { get; set; } 
        public string CashierComment { get; set; } = null;
        public bool CashInStatus { get; set; } = false;
        public string TellerId { get; set; } = null;
        public string TellerName { get; set; } = null;
        public string FileUploadId { get; set; } = null;
        public string BatchExecutionStatus { get; set; } = null;
        public DateTime? CreatedDate { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;
        public List<TransactionDetail> Details { get; set; } = new List<TransactionDetail>();
    }

    public class TransactionDetail
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
        public string UploadBy { get; set; } = null;
        public string ManualEntryDailyCollectorId { get; set; } = null;
        public string ProcessingStatus { get; set; } = null;
        public string TreatementStatus { get; set; } = null;
        public DateTime? TreatementDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

}
