using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission
{

    public sealed class DailyCollectorCommissionScomand
    {
        public string Id { get; set; }

        public string BranchId { get; set; } = null;

        public string CollectorId { get; set; } = null;

        public bool IsCentralised { get; set; }

        public bool IsActive { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string CollectorMemberReference { get; set; } = null;

        public List<DailyCollectorCommissionShareRule> Rules { get; set; } = new List<DailyCollectorCommissionShareRule>();
    }

    public class DailyCollectorCommissionShareConfig
    {
        public string Id { get; set; } 

        public string BranchId { get; set; }
        public string CollectorId { get; set; }

        public string BranchName { get; set; }
        public string CollectorName { get; set; }
        public string ScopeLevel { get; set; }

        public bool IsCentralised { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string CollectorMemberReference { get; set; }
        // Navigation Property
        public List<DailyCollectorCommissionShareRule> Rules { get; set; }
            = new List<DailyCollectorCommissionShareRule>();

        // Audit (optional but recommended)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
    }


    public class DailyCollectorCommissionShareRule
    {
       public string Stakeholder { get; set; } = null;

        public decimal Percentage { get; set; }
        // Navigation
        public DailyCollectorCommissionShareConfig Config { get; set; } = null;
    }

    public sealed class DailyCollectorCommissionShareReponse
    {
        public string Id { get; set; } = null;
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CollectorName { get; set; }
        public string CollectorId { get; set; }
        public bool IsCentralised { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string CollectorMemberReference { get; set; }
        public decimal TotalPercentage { get; set; }
        public string Stakeholders { get; set; } = null;   // e.g. "COLLECTOR:60 | BRANCH:40"
        public int RuleCount { get; set; }
        public string ScopeLevel { get; set; }
    }
    public class DaillycollectorCommissionConfigQuery
    {
        public DataTableOptions Options { get; set; }
     
        public string BranchId { get; set; }
        public string CollectorId { get; set; }

        public bool? IsCentralised { get; set; }
        public bool? IsActive { get; set; }

        // NEW: Scope filter (CENTRALISED | BRANCH_DEFAULT | COLLECTOR_BRANCH)
        public string ScopeLevel { get; set; }
        // Exact date filter
        public DateTime? EffectiveOn { get; set; }

        // Date range filter
        public DateTime? EffectiveFromStart { get; set; }
        public DateTime? EffectiveFromEnd { get; set; }

        public string Search { get; set; }

    }
}
