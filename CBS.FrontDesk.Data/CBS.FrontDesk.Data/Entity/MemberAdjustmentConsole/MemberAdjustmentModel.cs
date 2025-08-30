using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole
{
    public class MemberAdjustmentModel
    {
        public string MemberId { get; set; }
        public string BranchId { get; set; }
        public string AccountId { get; set; }
        public string NewFirstName { get; set; }
        public string OldFirstName { get; set; }
        public string NewLastName { get; set; }
        public string OldLastName { get; set; }

        public string AdjustmentType { get; set; }
        public string BalanceSenseDifference { get; set; }

        public decimal NewBalance { get; set; }
        public string NewMemberId { get; set; }
        public string NewMemberStatus { get; set; }
        public string OldMemberStatus { get; set; }
        public bool NewStatus { get; set; }
        public bool OldStatus { get; set; }
        public string NewMemberShipStatus { get; set; }
        public string OldMemberShipStatus { get; set; }
        public string NewMemberCategory { get; set; }
        public string OldMemberCategory { get; set; }
        public bool NewAccountStatus { get; set; }
        public string Reason { get; set; }
        public string RequestedBy { get; set; }
        public bool IsMemberProfileModified { get; set; }
        public bool IsMemberAccountModified { get; set; }
        public decimal OldBalance { get; set; }
        public bool AccountStatus { get; set; }

        public List<CustomerAccount> CustomerAccounts { get; set; }
    }
}
