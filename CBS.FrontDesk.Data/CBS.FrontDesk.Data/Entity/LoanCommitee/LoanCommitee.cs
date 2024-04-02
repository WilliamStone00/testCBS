using CBS.FrontDesk.Data.Entity.LoanConf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanCommitee
{
    public class LoanCommitee
    {
      
        public LoanCommiteeGroup LoanCommiteeGroup { get; set; }
        public LoanCommiteeMember LoanCommiteeMember { get; set; }
        public List<LoanCommiteeGroup> LoanCommiteeGroups { get; set; }
        public List<LoanCommiteeMember> LoanCommiteeMembers { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public LoanCommitee()
        {
            LoanCommiteeGroup = new LoanCommiteeGroup();
            LoanCommiteeMember = new LoanCommiteeMember();
            LoanCommiteeGroups = new List<LoanCommiteeGroup>();
            LoanCommiteeMembers = new List<LoanCommiteeMember>();
        }

    }
    
}
