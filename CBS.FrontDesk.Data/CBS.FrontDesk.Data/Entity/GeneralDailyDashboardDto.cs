using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{
    public class GeneralDailyDashboardDto
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public int NumberOfCashIn { get; set; }
        public int NumberOfCashOut { get; set; }
        public decimal TotalCashInAmount { get; set; }
        public decimal TotalCashOutAmount { get; set; }
        public int NewMembers { get; set; }
        public int ClosedAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int DormantAccounts { get; set; }
        public decimal LoanDisbursements { get; set; }
        public decimal LoanRepayments { get; set; }
        public decimal ServiceFeesCollected { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal Vat { get; set; }
        public decimal Penalties { get; set; }

        public decimal DailyExpenses { get; set; }
        public decimal OrdinaryShares { get; set; }
        public decimal PreferenceShares { get; set; }
        public decimal Savings { get; set; }
        public decimal Deposits { get; set; }
        public decimal CashInHand57 { get; set; }
        public decimal CashInHand56 { get; set; }
        public decimal MTNMobileMoney { get; set; }
        public int NumberOfCashOutMTN { get; set; }

        public int NumberOfCashOutOrange { get; set; }
        public int NumberOfCashInMTN { get; set; }
        public int NumberOfLoanFee { get; set; }
        public int NumberOfLoanDisbursementFee { get; set; }
        public int NumberOfCashInOrange { get; set; }

        public decimal OrangeMoney { get; set; }
        public decimal DailyCollectionCashOut { get; set; }
        public decimal DailyCollectionCashIn { get; set; }
        public decimal MomocashCollection { get; set; }
        public decimal Transfer { get; set; }
        public decimal PrimaryTillOpenOfDayBalance { get; set; }

        public decimal SubTillTillOpenOfDayBalance { get; set; }
        public DateTime Date { get; set; }
        public DateTime AccountingDate { get; set; }
        public decimal SubTillBalance { get; set; }
        public decimal PrimaryTillBalance { get; set; }
        public decimal CashReplenishmentSubTill { get; set; }
        public decimal CashReplenishmentPrimaryTill { get; set; }
    }

    public class CashOperationsDto
    {
        // Cash In and Cash Out operations
        public int NumberOfCashIn { get; set; }
        public int NumberOfCashOut { get; set; }
        public decimal TotalCashInAmount { get; set; }
        public decimal TotalCashOutAmount { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        // Cash operations specific to mobile money
        public int NumberOfCashOutMTN { get; set; }
        public int NumberOfCashOutOrange { get; set; }
        public int NumberOfCashInMTN { get; set; }
        public int NumberOfCashInOrange { get; set; }
        public decimal MTNMobileMoney { get; set; }
        public decimal OrangeMoney { get; set; }

        // Other cash collections and transfers
        public decimal DailyCollectionCashOut { get; set; }
        public decimal DailyCollectionCashIn { get; set; }
        //public decimal MomocashCollection { get; set; }
        //public decimal Transfer { get; set; }

        // Cash balances and replenishments
        public decimal CashInHand57 { get; set; }
        public decimal CashInHand56 { get; set; }
        public decimal PrimaryTillOpenOfDayBalance { get; set; }
        public decimal SubTillTillOpenOfDayBalance { get; set; }
        public decimal PrimaryTillBalance { get; set; }
        public decimal SubTillBalance { get; set; }
        public decimal CashReplenishmentSubTill { get; set; }
        public decimal CashReplenishmentPrimaryTill { get; set; }
        // Financial transactions for ordinary accounts
        public decimal LoanDisbursements { get; set; }
        public decimal LoanRepayments { get; set; }
        public decimal ServiceFeesCollected { get; set; }
        public DateTime Date { get; set; }
        public DateTime AccountingDate { get; set; }
    }

    public class OrdinaryAccountsDto
    {
        // Branch and account identifiers
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        // Member account statistics
        public int NewMembers { get; set; }
        public int ClosedAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int DormantAccounts { get; set; }

        // Account balance summaries
        public decimal OrdinaryShares { get; set; }
        public decimal PreferenceShares { get; set; }
        public decimal Savings { get; set; }
        public decimal Deposits { get; set; }

        // Date for the account operations
        public DateTime Date { get; set; }
    }
    public class GetDailyDashboardQuery
    {
        public string BranchId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

    }
}
