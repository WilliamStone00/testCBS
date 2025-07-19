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




    public class CollectorDto
    {
        // 👤 Collector Info
        public string collectorName { get; set; }
        public string amount { get; set; }

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


    public class PayDailyCollectorCommission 
    {
        /// <summary>
        /// The ID of the daily collector who is to receive the commission.
        /// </summary>
        public string DailyCollectorId { get; set; }

        public string MemberReference { get; set; }

        /// <summary>
        /// The ID of the branch processing the payment.
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// Total amount to be distributed across stakeholders.
        /// </summary>
        public decimal TotalAmountToShare { get; set; }

        /// <summary>
        /// List of shared breakdowns with percentage and amount per stakeholder.
        /// </summary>
        public List<SharedAmountBreakdown> SharedAmounts { get; set; } 
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
