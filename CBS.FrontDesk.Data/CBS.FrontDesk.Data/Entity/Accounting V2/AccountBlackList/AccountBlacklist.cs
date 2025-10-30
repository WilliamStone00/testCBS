using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AccountBlackList
{
    public class AccountBlacklist
    {
        public string TrialBallanceAcc { get; set; }
        public string BranchID { get; set; }
        public string Menu { get; set; }
        public string branchAccountId { get; set; }
        public bool isAffiliate { get; set; }
    }




}
