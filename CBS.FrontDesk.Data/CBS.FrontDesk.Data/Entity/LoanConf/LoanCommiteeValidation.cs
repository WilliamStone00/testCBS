using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanCommiteeValidation
    {
        public string id { get; set; }
        public string loanApplicationId { get; set; }
        public string loanCommiteeMemberId { get; set; }
        public string comment { get; set; }
        public string approvalState { get; set; }

    }

}
