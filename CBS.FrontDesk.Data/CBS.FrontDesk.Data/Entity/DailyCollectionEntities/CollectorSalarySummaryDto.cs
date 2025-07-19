using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
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
        public string MemberReference { get; private set; }

        public PayDailyCollectorCommission ConvertToPayDailyCollectorCommission()
        {
            PayDailyCollectorCommission payDailyCollectorCommission = new PayDailyCollectorCommission();
            payDailyCollectorCommission.DailyCollectorId = this.CollectorId;
            payDailyCollectorCommission.MemberReference = this.MemberReference;
            payDailyCollectorCommission.BranchId = this.BranchId;
            payDailyCollectorCommission.TotalAmountToShare = this.TotalAmountToDistribute;
            payDailyCollectorCommission.SharedAmounts = this.SharedAmounts;

            // Parse Month format: YYYY-MM
            if (!string.IsNullOrEmpty(this.Month))
            {
                var monthParts = this.Month.Split('-');
                if (monthParts.Length == 2)
                {
                    if (int.TryParse(monthParts[0], out int year) && int.TryParse(monthParts[1], out int month))
                    {
                        payDailyCollectorCommission.Year = year;
                        payDailyCollectorCommission.Month = month;
                    }
                    else
                    {
                        throw new FormatException($"Invalid month format: {this.Month}. Expected format: YYYY-MM");
                    }
                }
                else
                {
                    throw new FormatException($"Invalid month format: {this.Month}. Expected format: YYYY-MM");
                }
            }

            return payDailyCollectorCommission;
        }
    }
}
