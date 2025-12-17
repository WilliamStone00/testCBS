using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2
{
    //public class JournalApproval
    //{
    //    public string Reference { get; set; }
    //    public string SourceBranchId { get; set; }
    //    public string  BranchId { get; set; }
    //    public string Approver { get; set; }
    //}


    public class JournalApprovalResponse
    {
        public string Reference { get; set; }
        public string BranchId { get; set; }
        public string TicketId { get; set; }
        public string TicketState { get; set; }
        public string JournalId { get; set; }
        public string Message { get; set; }
    }
   

    public class JournalApproval
    {
        // ===== Journal Header Information =====
        public string Reference { get; set; }
        public string SourceBranchId { get; set; }
        public string BranchId { get; set; }
        public string Approver { get; set; }
        public string ApprovalName { get; set; }
        public string ApprovedByName { get; set; }
        public string DestinationBranchId { get; set; }
        public string TicketType { get; set; }

        // ===== Journal Line Details =====
        public List<DestinationJournalLineApproval> Lines { get; set; }


       
        public bool Approve { get; set; }
        public string Reason { get; set; }
    }

    public class DestinationJournalLineApproval
    {
        public string AffiliateAccountId { get; set; }
        public string DrCr { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
    }
    public class RejectJournal
    {
        public string Id { get; set; }

        public string comment { get; set;}
    }




}
