using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanCommitee
{
    public class LoanCommiteeGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal MinimumLoanAmount { get; set; }
        public decimal MaximumLoanAmount { get; set; }
        public int NumberOfMembers { get; set; }
        public int NumberToApprovalsToValidationALoan { get; set; }
        public string CommiteeLeaderUserId { get; set; }
        public bool Status { get; set; }
        public List<LoanCommiteeMember> LoanCommiteeMembers { get; set; }
        public LoanCommiteeGroup()
        {
            NumberToApprovalsToValidationALoan = 3;
            NumberOfMembers = 3;
            MinimumLoanAmount = 1000;
            MaximumLoanAmount = 10000000;
            Status = true;
        }
    }
}
