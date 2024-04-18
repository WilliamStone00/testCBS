using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanCommitee
{
    public class LoanCommiteeMember
    {

        public string Id { get; set; }
        public string LoanCommiteeGroupId { get; set; }
        public string UserId { get; set; }
        public virtual LoanCommiteeGroup LoanCommiteeGroup { get; set; }
        public virtual User User { get; set; }
        public List<LoanCommiteeValidationHistory> LoanCommiteeValidationHistories { get; set; }
    }

}
