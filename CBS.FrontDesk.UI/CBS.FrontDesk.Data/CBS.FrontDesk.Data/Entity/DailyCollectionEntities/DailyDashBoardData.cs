using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
 
 
        // Main financial summary data
        public class FinancialSummary
        {
            public int Year { get; set; }
            public int Month { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalCollectedAmount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalFeeCollected { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalCollectorShare { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalBranchShare { get; set; }

            public int TotalCashInCount { get; set; }
            public int TotalMembersServed { get; set; }
            public string MostFrequentSaver { get; set; }
            public string TopSaverName { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TopSaverAmount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal AverageCollectionPerSaver { get; set; }

            public int DaysWorked { get; set; }
            public int TotalSubscriptionCount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalSubscriptionAmount { get; set; }

            public int TotalCashOutCount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalCashOutAmount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal NetBalance { get; set; }

            [DisplayFormat(DataFormatString = "{0:P2}")]
            public decimal CollectorSharePercentage { get; set; }

            [DisplayFormat(DataFormatString = "{0:P2}")]
            public decimal BranchSharePercentage { get; set; }

            // Highest performers
            public string HighestCashInSaverName { get; set; }
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal HighestCashInAmount { get; set; }

            public string HighestCashOutSaverName { get; set; }
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal HighestCashOutAmount { get; set; }

            public string HighestCashInBranchName { get; set; }
            public string HighestCashInBranchCode { get; set; }
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal HighestCashInBranchAmount { get; set; }

            public string HighestCashOutBranchName { get; set; }
            public string HighestCashOutBranchCode { get; set; }
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal HighestCashOutBranchAmount { get; set; }

            // Related collections
            public List<DailySummary> DailySummaries { get; set; } = new List<DailySummary>();
            public List<Transaction> Transactions { get; set; } = new List<Transaction>();
            public List<TopSaver> TopSavers { get; set; } = new List<TopSaver>();
            public List<TopCollector> TopCollectors { get; set; } = new List<TopCollector>();
            public List<TopBranch> TopBranches { get; set; } = new List<TopBranch>();
            public List<BranchDashboard> BranchDashboards { get; set; } = new List<BranchDashboard>();
        }

        // Daily summary data
        public class DailySummary
        {
            public DateTime Date { get; set; }
            public int CashInCount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal CashInAmount { get; set; }

            public int CashOutCount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal CashOutAmount { get; set; }

            public int SubscriptionCount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal SubscriptionAmount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal CollectorCommission { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal BranchCommission { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalFeeCollected { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalCollectorShare { get; set; }
        }

        // Transaction details
        public class Transaction
        {
            public DateTime Date { get; set; }

            [Required]
            public string MemberReference { get; set; }

            [Required]
            public string MemberName { get; set; }

            [Required]
            public string OperationType { get; set; } // CashIn, CashOut, OnboardingFee

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal Amount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal Fee { get; set; }

            public string AccountNumber { get; set; }

            [Required]
            public string TransactionReference { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal CollectorShare { get; set; }

            public string BranchName { get; set; }
            public string BranchCode { get; set; }
            public string BranchId { get; set; }

            // Computed properties
            public bool IsCashIn => OperationType == "CashIn";
            public bool IsCashOut => OperationType == "CashOut";
            public bool IsOnboardingFee => OperationType == "OnboardingFee";
            public decimal BranchShare => Fee - CollectorShare;
        }

        // Top savers ranking
        public class TopSaver
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Code { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalAmount { get; set; }
        }

        // Top collectors ranking
        public class TopCollector
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Code { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalAmount { get; set; }
        }

        // Top branches ranking
        public class TopBranch
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Code { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalAmount { get; set; }
        }

        // Branch dashboard data
        public class BranchDashboard
        {
            public string BranchId { get; set; }
            public string BranchCode { get; set; }
            public string BranchName { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalCollectedAmount { get; set; }

            public int TotalCashInCount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalFeeCollected { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalCollectorShare { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalBranchShare { get; set; }

            public int TotalMembersServed { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal AverageCollectionPerSaver { get; set; }

            public int TotalSubscriptionCount { get; set; }

            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal TotalSubscriptionAmount { get; set; }

            // Computed properties
            public decimal CollectorSharePercentage =>
                TotalFeeCollected > 0 ? (TotalCollectorShare / TotalFeeCollected) * 100 : 0;

            public decimal BranchSharePercentage =>
                TotalFeeCollected > 0 ? (TotalBranchShare / TotalFeeCollected) * 100 : 0;
        }

        // Enums for better type safety
        public enum OperationType
        {
            CashIn,
            CashOut,
            OnboardingFee
        }

        public enum TransactionStatus
        {
            Success,
            Pending,
            Failed
        }
    }
 
