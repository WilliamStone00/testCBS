using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionData
{
    public class Agent
    {
  
        public string BranchId { get; set; }
        public string UserId { get; set; }
    }


    public class CollectorSalarySummaryDto
    {
        // 👤 Collector Info
        public string CollectorId { get; set; }
        public string CollectorName { get; set; }

        // 🏢 Branch Info
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        // 📅 Period & Operation Context
        public string Month { get; set; }                   // Format: YYYY-MM
        public string OperationType { get; set; }           // e.g. "CashIn", "LoanRepayment", "OnboardingFee"

        // 📊 Aggregated Per-Member Stats
        public List<MemberOperationSummary> MemberStats { get; set; }  

        // 📈 Summary Metrics
        public int TotalMembersWithActivity => MemberStats?.Count ?? 0;
        public decimal TotalValueCollected => MemberStats?.Sum(m => m.TotalActivityAmount) ?? 0m;

        public decimal TotalFeeCharged { get; set; }            // Total computed fees across all members
        public decimal TotalAmountToDistribute { get; set; }    // Net distributable value

        // 📤 Stakeholder Breakdown
        public List<SharedAmountBreakdown> SharedAmounts { get; set; } = new List<SharedAmountBreakdown>();
    }


    public class MemberOperationSummary
    {
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public decimal TotalActivityAmount { get; set; }         // E.g. total deposits, repayments
        public decimal FeeCharged { get; set; }                  // Fee assigned based on rules
        public DateTime LastTransactionDate { get; set; }
    }

    public class SharedAmountBreakdown
    {
        public string Stakeholder { get; set; } // e.g. "DailyCollector", "CamCCUL"
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}
