using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole
{
    public class MemberAdjustmentRequestDetailsDto
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string AccountId { get; set; }

        // Proposed New Values
        public string NewFirstName { get; set; }
        public string NewLastName { get; set; }
        public decimal NewBalance { get; set; }
        public string NewCustomerId { get; set; }
        public string NewMemberStatus { get; set; }
        public bool NewStatus { get; set; }
        public string NewMemberShipStatus { get; set; }
        public string NewMemberCategory { get; set; }
        public decimal NewAccountStatus { get; set; }

        // Original Snapshot (optional for display)
        public string OldFirstName { get; set; }
        public string OldLastName { get; set; }
        public decimal OldBalance { get; set; }
        public string OldCustomerId { get; set; }
        public string OldMemberStatus { get; set; }
        public bool OldStatus { get; set; }
        public string OldMemberShipStatus { get; set; }
        public string OldMemberCategory { get; set; }
        public decimal OldAccountStatus { get; set; }


        public bool isMemberProfileModified { get; set; }
        public bool isMemberAccountModified { get; set; }

        public string Reason { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; }

        public string Status { get; set; } // Pending, Approved, Rejected
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string RejectionReason { get; set; }
    }
}
