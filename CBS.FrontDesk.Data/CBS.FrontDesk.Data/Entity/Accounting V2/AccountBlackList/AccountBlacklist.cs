using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AccountBlackList
{
    public class AccountBlacklist
    {
        public string Id { get; set; }

        // Centralized? (affiliate-level) OR branch-mode
        public bool IsCentralized { get; set; } = true;

        // Scope ids (only one is relevant per mode)
        public string BranchID { get; set; }
        public string AffiliateId { get; set; }  // optional if you later expose it

        // Target menu (ManualEntries, OtherCashIn, ...)
        [Required] public string Menu { get; set; }

        // Account targets (only one relevant per mode)
        public string AffiliateAccountId { get; set; }
        public string BranchAccountId { get; set; }

        // Extra info
        public bool IsBlacklisted { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; }

        // Auditing (display only)
        public DateTime CreatedDate { get; set; }
    }

    public class AccountBlacklistresponse
    {
        public string Id { get; set; }
        public bool IsCentralized { get; set; }
        public string AffiliateId { get; set; }
        public string BranchId { get; set; }
        public string Menu { get; set; }
        public string BranchName { get; set; }
        public string AffiliateAccountName { get; set; }
        public string AccountNumber { get; set; }
        public string BranchAccountName { get; set; } // NOTE: Uppercase B to keep consistency
        public string Notes { get; set; }

        public string AffiliateAccountId { get; set; }
        public string BranchAccountId { get; set; }

        public bool IsBlacklisted { get; set; }
        public string Reason { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

    }

    public class AccountBlacklistQuery
    {
        public DataTableOptions Options { get; set; }
        public AccountBlacklistQuery() { Options = new DataTableOptions(); }

        public string BranchID { get; set; }
        public string Menu { get; set; }
        public string AffiliateAccountId { get; set; }
        public string BranchAccountId { get; set; }

        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
    }

}
