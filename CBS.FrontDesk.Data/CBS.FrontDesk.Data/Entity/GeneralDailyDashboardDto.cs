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
        public int NumberOfInterBranchCashIn { get; set; }
        public int NumberOfInterBranchCashOut { get; set; }
        public decimal VolumeOfInterBranchCashIn { get; set; }
        public decimal VolumeOfInterBranchCashOut { get; set; }
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
        public decimal MobileMoneyCashOut { get; set; }
        public int NumberOfCashOutMTN { get; set; }

        public int NumberOfCashOutOrange { get; set; }
        public int NumberOfCashInMTN { get; set; }
        public int NumberOfLoanFee { get; set; }
        public int NumberOfLoanDisbursementFee { get; set; }
        public int NumberOfCashInOrange { get; set; }
        public decimal OrangeMoneyCashOut { get; set; }

        public decimal OrangeMoney { get; set; }
        public decimal DailyCollectionCashOut { get; set; }
        public decimal DailyCollectionCashIn { get; set; }
        public int NumberOfDailyCollectionCashOut { get; set; }
        public int NumberOfDailyCollectionCashIn { get; set; }

        public decimal MomocashCollection { get; set; }
        public decimal Transfer { get; set; }
        public int NumberOfTransfer { get; set; }
        public decimal PrimaryTillOpenOfDayBalance { get; set; }

        public decimal SubTillTillOpenOfDayBalance { get; set; }
        public DateTime Date { get; set; }
        public DateTime AccountingDate { get; set; }
        public decimal SubTillBalance { get; set; }
        public decimal PrimaryTillBalance { get; set; }
        public decimal CashReplenishmentSubTill { get; set; }
        public decimal CashReplenishmentPrimaryTill { get; set; }
        public int NumberOfBranches { get; set; }
    }

    public class CustomerDashboardStatistics
    {
        public int TotalNumberOfPhysical { get; set; }
        public int TotalNumberOfMoral { get; set; }
        public int TotalBranches { get; set; }
        public int TotalMembers { get; set; }
        public List<CustomerDashboardStatisticsByBranch> CustomerStatisticsByBranches { get; set; }
    }
    public class CustomerDashboardStatisticsByBranch
    {
        public int NumberOfPhysical { get; set; }
        public int NumberOfMoral { get; set; }
        public int NumberOfMembers { get; set; }
        public string BranchId { get; set; }
    }
    public class AccountDashboardStatistics
    {
        public int TotalNumberOfAccounts { get; set; }
        public int TotalBranches { get; set; }
        public int TotalMembers { get; set; }
        public int TotalNumberOfActiveAccounts { get; set; }
        public int TotalNumberOfInActiveAccounts { get; set; }
        public string BranchId { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalBalanceWithoutBlocked { get; set; }
        public decimal TotalBlockedAmount { get; set; }
        public List<AccountDashboardStatisticsPerAccount> StatisticPerAccounts { get; set; }
    }
    public class AccountDashboardStatisticsPerAccount
    {
        public int NumberOfAccounts { get; set; }
        public int NumberOfActiveAccounts { get; set; }
        public int NumberOfInActiveAccounts { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal BlockedAmount { get; set; }
        public decimal BalanceWithoutBlocked { get; set; }
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
    public class DashboardQueryParameter
    {
        public string BranchId { get; set; }
        public string QueryParameter { get; set; }
    }
    public class MainDashboardMembers
    {
        public int TotalMembers { get; set; }
        public string BranchName { get; set; }
        public int TotalPhysicalMembers { get; set; }
        public int TotalMoralMembers { get; set; }
        public int TotalBranches { get; set; }
    }
    public class MainDashboardOrdinaryAccounts
    {
        public int TotalAccounts { get; set; }
        public int TotalActieAccounts { get; set; }
        public int TotalInactiveAccounts { get; set; }
        public int TotalOrdinaryShares { get; set; }
        public decimal TotalVolumeOfOrdinaryShares { get; set; }
        public int TotalPreferenceShares { get; set; }
        public decimal TotalVolumeOfPreferenceShares { get; set; }
        public int TotalSavings { get; set; }
        public decimal TotalVolumeOfSavings { get; set; }
        public int TotalDeposits { get; set; }
        public decimal TotalVolumeOfDeposits { get; set; }
        public int TotalDailyCollections { get; set; }
        public decimal TotalVolumeOfDailyCollections { get; set; }
        public int TotalGav { get; set; }
        public decimal TotalVolumeOfGav { get; set; }
        public decimal TotalVolumeOfBlockedAccounts { get; set; }
        public string BranchName { get; set; }
        public int TotalBranches { get; set; }
        public int TotalMembers { get; set; }
        public int TotalNumberOfActiveAccounts { get; set; }
        public int TotalNumberOfInActiveAccounts { get; set; }
        public string BranchId { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalBalanceWithoutBlocked { get; set; }
        public decimal TotalBlockedAmount { get; set; }
    }

    public class AccountingDashboardStatistics
    {
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public decimal Balance { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string DashboardAccountType { get; set; }
    }
    public class MainAccountingDashboardStatistics
    {
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string DashboardAccountType { get; set; }
        public decimal CashInHandBalance { get; set; }
        public decimal CashInBankBalance { get; set; }
        public decimal TotalSharesBalance { get; set; }
        public decimal PreferenceShareBalance { get; set; }
        public decimal OrdinarySharesBalance { get; set; }
        public decimal DepositBalance { get; set; }
        public decimal SavingsBalance { get; set; }
        public decimal GavBalance { get; set; }
        public decimal DailyCollectionsBalance { get; set; }
        public decimal MTNMobileMoneyBalance { get; set; }
        public decimal MTNMobileMoneyMasterBalance { get; set; }
        public decimal OrangeMoneyMasterBalance { get; set; }
        public decimal OrangeMoneyBalance { get; set; }
        public decimal TotalExpenseBalance { get; set; }
        public decimal TotalIncomeBalance { get; set; }
        public decimal TotalLiquidity { get; set; }
    }
    public class LoanMainDashboard
    {
        public int TotalNumberOfLoans { get; set; }
        public decimal TotalVolumeOfLoanGranted { get; set; }
        public decimal TotalRemainingBalance { get; set; }
    }
}
