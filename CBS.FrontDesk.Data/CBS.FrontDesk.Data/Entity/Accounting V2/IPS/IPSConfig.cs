using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.IPS
{
   

    public class IPSConfig
    {
        public string Id { get; set; }
        public int Year { get; set; }
        public int AgeLimit { get; set; }
        public decimal LoanCoverageLimit { get; set; }
        public decimal SavingsCoverageLimit { get; set; }
        public decimal PremiumRateLoan { get; set; }
        public decimal PremiumRateSavings { get; set; }
        public bool ExcludeGroups { get; set; }
        public int MaxDelinquencyDays { get; set; }
        public bool IsActive { get; set; }

        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        public string LifeSavingsClaimDebitAccountId { get; set; }
        public string LifeSavingsClaimCreditAccountId { get; set; }
        public string LoanProtectionDebitAccountId { get; set; }
        public string LoanProtectionCreditAccountId { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class IPSConfigQuery
    {
   
        public DataTableOptions Options { get; set; }
        public IPSConfigQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; }
        public int? Year { get; set; }
        public bool IsActive { get; set; }
        public int? AgeLimitFrom { get; set; }
        public int? AgeLimitTo { get; set; }
        public int? MaxDelinquencyDaysFrom { get; set; }
        public int? MaxDelinquencyDaysTo { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public bool IncludeDeleted { get; set; }
    }
}

