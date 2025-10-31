using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AccountBlackList
{
    public class AccountBlacklist
    {
        public string  Id { get; set; }
        public string TrialBallanceAcc { get; set; }
        public string BranchID { get; set; }
        public string Menu { get; set; }
        public string branchAccountId { get; set; }
        public string AffiliateAccountId { get; set; }
        public bool isAffiliate { get; set; }
    }

    public class AccountBlacklistresponse
    {
        public string Id { get; set; }
        public string TrialBallanceAcc { get; set; }
        public string BranchID { get; set; }
        public string Menu { get; set; }
        public string branchAccountId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string Notes { get; set; }
        public bool isAffiliate { get; set; }
        public DateTime? CreatedDate { get; set; }
       
    }

    public class AccountBlacklistQuery
    {
        public DataTableOptions Options { get; set; }
        public AccountBlacklistQuery() { Options = new DataTableOptions(); }

        public string TrialBallanceAcc { get; set; }
        public string BranchID { get; set; }
        public string BranchName { get; set; }
        public string Menu { get; set; }
        public string branchAccountId { get; set; }
        public string branchAccountName { get; set; }
        public string AffiliateAccountId { get; set; }
        public string AffiliateAccountName { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
    }

}
