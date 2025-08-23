using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole
{
     public class MemberAdjustmentHistoryDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string FieldAdjusted { get; set; } // e.g., "Penalty"
        public decimal OldValue { get; set; }
        public decimal NewValue { get; set; }
        public string Reason { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
        public string ApprovalStatus { get; set; } // Approved, Rejected
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string AdjustmentRequestId { get; set; }
    }
    public class MemberAdjustmentRequestDetailsWithHistoryDto
    {
        public MemberAdjustmentRequestDetailsDto Request { get; set; }
        public List<MemberAdjustmentHistoryDto> History { get; set; }
    }
}
