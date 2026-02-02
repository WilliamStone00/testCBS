using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ReportMembersFSeries
{// ReportDTOs.cs
    public class AccountStatementRow
    {
        // Bank Information
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Tell { get; set; }
        public string Bp { get; set; }
        public string ReportHeader { get; set; }

        // Period Information
        public string  PeriodFrom { get; set; }
        public string PeriodTo { get; set; }

        // Account Information
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string Currency { get; set; }
        public decimal BegginingBalance { get; set; }

        // Customer Information
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string PoBox { get; set; }

        // Transaction Details
        public string Date { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string Representative { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string Balance { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        // Summary Information
        public string Total { get; set; }
        public string AcountBalanceOn { get; set; }
        public string TotalOperation { get; set; }
        public string TotalDebit { get; set; }
        public string TotalCredit { get; set; }

        // Print Information
        public string PrintedBy { get; set; }
        public string PrintedOn { get; set; }
        public string  Year { get; set; }
    }

    public class TransactionStaement
    {
        // Coins
        public int Coin1 { get; set; }
        public int Coin5 { get; set; }
        public int Coin10 { get; set; }
        public int Coin25 { get; set; }
        public int Coin50 { get; set; }
        public int Coin100 { get; set; }
        public int Coin500 { get; set; }

        // Notes
        public int Note500 { get; set; }
        public int Note1000 { get; set; }
        public int Note2000 { get; set; }
        public int Note5000 { get; set; }
        public int Note10000 { get; set; }

        // Amounts
        public decimal Amount { get; set; }
        public decimal OriginalDepositAmount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Tax { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal SourceBranchCommission { get; set; }
        public decimal DestinationBranchCommission { get; set; }
        public decimal Balance { get; set; }
        public decimal Fee { get; set; }
        public decimal Charges { get; set; }
        public decimal ClosingBalance { get; set; }
        public string OpeningBalance { get; set; }

        // Transaction info
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string OperationType { get; set; }
        public string Status { get; set; }
        public string TransactionRef { get; set; }
        public string Reference { get; set; }
        public string Representative { get; set; }
        public DateTime TransactionDate { get; set; }

        // Branch
        public string SendingBranch { get; set; }
        public string RecievingBranch { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }

        // People
        public string SenderName { get; set; }
        public string RecieverName { get; set; }
        public string DepositorName { get; set; }
        public string DepositorIDNumber { get; set; }
        public string DepositerTelephone { get; set; }
        public string DepositorIDIssueDate { get; set; }
        public string DepositorIDExpiryDate { get; set; }
        public string DepositorIDNumberPlaceOfIssue { get; set; }
        public string DepositerNote { get; set; }
        public bool IsDepositDoneByAccountOwner { get; set; }

        // Inter-branch
        public bool IsInterBrachOperation { get; set; }
        public string InterBrachOperation { get; set; }

        // Product / Fees
        public string ProductName { get; set; }
        public string FeeType { get; set; }
        public string SourceType { get; set; }

        // Cashier
        public string TellerName { get; set; }
        public string CashierName { get; set; }

        // Organization
        public string Logo { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }

        // Account / Customer
        public string AccountType { get; set; }
        public string AccountName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }

        // Address
        public string Address { get; set; }
        public string Town { get; set; }
        public string Village { get; set; }
        public string Country { get; set; }

        // Misc
        public string ExpireDate { get; set; }
        public string DeliveryDate { get; set; }
        public string Telephone { get; set; }
        public string Key { get; set; }
        public string BarCode { get; set; }
        public string AmountInWord { get; set; }
        public string ReceiptTitle { get; set; }
        public string Description { get; set; }

        // Report
        public string ReportTitle { get; set; }
        public string Year { get; set; }
        public string PrintedBy { get; set; }
        public string Printedfrom { get; set; }
        public string PrintedTo { get; set; }
        public string PrintedOn { get; set; }
        public string BalanceasOf { get; set; }
        public string TotalOperation { get; set; }
        public string TotalDebit { get; set; }
        public string TotalCredit { get; set; }

        // Date / Time (string as in XSD)
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }

        // Identification
        public string CNI { get; set; }
        public string Currreccy { get; set; }
    }


    public class MemberSituationRow
    {
        // Header Information
        public string ReportTitle { get; set; }
        public bool Status { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }

        // Account Information
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string AccountStatus { get; set; }
        public DateTime LastTransactionDate { get; set; }

        // Loan Information
        public string LoanNumber { get; set; }
        public string LoanType { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string LoanStatus { get; set; }

        // Summary
        public int TotalAccounts { get; set; }
        public int TotalLoans { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }

        public string Total { get; set; }
    }

    public class LoanRepaymentRow
    {
        // Header Information
        public string ReportTitle { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }

        // Loan Information
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime MaturityDate { get; set; }

        // Repayment Details
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalDue { get; set; }
        public decimal InterestDue { get; set; }
        public decimal TotalDue { get; set; }
        public DateTime ActualPaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; }
        public decimal OutstandingBalance { get; set; }

        // Summary
        public decimal TotalPrincipalPaid { get; set; }
        public decimal TotalInterestPaid { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
    }

    public class LoanSituationRow
    {
        // Header Information
        public string ReportTitle { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }

        // Portfolio Summary
        public int TotalLoans { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public decimal AverageInterestRate { get; set; }
        public decimal DelinquencyRate { get; set; }

        // Loan Details
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public string LoanType { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal InterestRate { get; set; }
        public string LoanStatus { get; set; }
        public int DaysPastDue { get; set; }

        // Risk Indicators
        public string RiskCategory { get; set; }
        public decimal CollateralValue { get; set; }
        public string OfficerName { get; set; }
    }

    public class MemberAccount
    {
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
    }

    public class MemberLoan
    {
        public string LoanNumber { get; set; }
        public string LoanType { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    public class TransactionSummary
    {
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
    }

    public class LoanDetails
    {
        public string LoanNumber { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string LoanProductCode { get; set; }
        public string LoanProductName { get; set; }
        public string LoanType { get; set; } // Personal, Business, Mortgage, etc.
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalRepayable { get; set; }
        public int LoanTermMonths { get; set; }
        public int LoanTermYears { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime FirstPaymentDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal TotalRepaid { get; set; }
        public decimal TotalDue { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public string LoanStatus { get; set; } // Active, Closed, Defaulted, Written-off
        public string LoanOfficerId { get; set; }
        public string LoanOfficerName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string CollateralType { get; set; }
        public decimal CollateralValue { get; set; }
        public string GuarantorName { get; set; }
        public string GuarantorPhone { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public string PurposeOfLoan { get; set; }
        public string RepaymentFrequency { get; set; } // Monthly, Weekly, Daily
        public int InstallmentsPaid { get; set; }
        public int InstallmentsRemaining { get; set; }
        public decimal LatePaymentPenalty { get; set; }
        public decimal EarlyRepaymentFee { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public decimal LastPaymentAmount { get; set; }
        public int DaysPastDue { get; set; }
        public decimal ArrearsAmount { get; set; }
    }

    public class RepaymentSchedule
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PrincipalDue { get; set; }
        public decimal InterestDue { get; set; }
        public decimal PenaltyDue { get; set; }
        public decimal TotalDue { get; set; }
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal CumulativePrincipal { get; set; }
        public decimal CumulativeInterest { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string PaymentStatus { get; set; } // Pending, Paid, Partial, Overdue
        public bool IsPaid { get; set; }
        public int DaysLate { get; set; }
        public string ReceiptNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime ActualPaymentDate { get; set; }
        public string CollectedBy { get; set; }
        public string Remarks { get; set; }
    }

    public class LoanRepayment
    {
        public string ReceiptNumber { get; set; }
        public string TransactionReference { get; set; }
        public string LoanNumber { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal OtherChargesPaid { get; set; }
        public string PaymentMethod { get; set; } // Cash, Cheque, Transfer, Mobile
        public string PaymentMode { get; set; } // Full, Partial, Advance
        public string ChequeNumber { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string ReceivingOfficerId { get; set; }
        public string ReceivingOfficerName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Status { get; set; } // Cleared, Pending, Reversed
        public string Remarks { get; set; }
        public string InstallmentPeriod { get; set; } // Jan 2024, Feb 2024
        public bool IsAdvancePayment { get; set; }
        public bool IsLatePayment { get; set; }
        public int DaysLate { get; set; }
        public decimal LatePaymentFee { get; set; }
        public DateTime ValueDate { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime VerificationDate { get; set; }
        public string GLAccount { get; set; }
        public string TransactionType { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
    }


    public class LoanPortfolioItem
    {
        public string LoanNumber { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; } // Individual, Corporate, SME
        public string LoanProductCode { get; set; }
        public string LoanProductName { get; set; }
        public string LoanType { get; set; }
        public string LoanCategory { get; set; } // Short-term, Medium-term, Long-term
        public decimal LoanAmount { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int LoanTermMonths { get; set; }
        public string LoanStatus { get; set; } // Active, Closed, Non-performing, Written-off
        public string PerformanceStatus { get; set; } // Performing, Watch, Substandard, Doubtful, Loss
        public int DaysPastDue { get; set; }
        public decimal ArrearsAmount { get; set; }
        public string RiskCategory { get; set; } // Low, Medium, High
        public string RiskGrade { get; set; }
        public decimal ProvisionRate { get; set; }
        public decimal ProvisionAmount { get; set; }
        public string LoanOfficerId { get; set; }
        public string LoanOfficerName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Region { get; set; }
        public string Sector { get; set; } // Agriculture, Trade, Manufacturing, Services
        public string CollateralType { get; set; }
        public decimal CollateralValue { get; set; }
        public decimal LTVRatio { get; set; } // Loan-to-Value ratio
        public decimal DSR { get; set; } // Debt Service Ratio
        public DateTime LastPaymentDate { get; set; }
        public decimal LastPaymentAmount { get; set; }
        public decimal TotalRepaid { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public int InstallmentsPaid { get; set; }
        public int InstallmentsRemaining { get; set; }
        public string Currency { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public decimal NextPaymentAmount { get; set; }
        public bool HasGuarantor { get; set; }
        public string GuarantorName { get; set; }
        public string PurposeOfLoan { get; set; }
        public string SecurityDetails { get; set; }
        public DateTime LastReviewDate { get; set; }
        public DateTime NextReviewDate { get; set; }
        public decimal InterestAccrued { get; set; }
        public decimal InterestSuspended { get; set; }
    }

    public class LoanPortfolioSummary
    {
        // Counts
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int ClosedLoans { get; set; }
        public int DisbursedLoans { get; set; }
        public int ApprovedLoans { get; set; }
        public int PendingLoans { get; set; }
        public int RejectedLoans { get; set; }

        // Amounts
        public decimal TotalPortfolioValue { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public decimal TotalDisbursedAmount { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public decimal TotalRepaidAmount { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public decimal TotalInterestAccrued { get; set; }

        // Performance Metrics
        public int PerformingLoans { get; set; }
        public int NonPerformingLoans { get; set; }
        public int WatchListLoans { get; set; }
        public int SubstandardLoans { get; set; }
        public int DoubtfulLoans { get; set; }
        public int LossLoans { get; set; }

        // Performance Amounts
        public decimal PerformingAmount { get; set; }
        public decimal NonPerformingAmount { get; set; }
        public decimal WatchListAmount { get; set; }
        public decimal SubstandardAmount { get; set; }
        public decimal DoubtfulAmount { get; set; }
        public decimal LossAmount { get; set; }

        // Averages
        public decimal AverageLoanSize { get; set; }
        public decimal AverageInterestRate { get; set; }
        public decimal AverageLoanTerm { get; set; }
        public decimal AverageOutstanding { get; set; }

        // Ratios
        public decimal PortfolioAtRisk { get; set; }
        public decimal NPLRatio { get; set; } // Non-Performing Loan Ratio
        public decimal ProvisionCoverageRatio { get; set; }
        public decimal YieldRate { get; set; }
        public decimal CostOfRisk { get; set; }

        // Provisions
        public decimal TotalProvisionRequired { get; set; }
        public decimal TotalProvisionHeld { get; set; }
        public decimal ProvisionShortfall { get; set; }

        // Delinquency
        public int CurrentLoans { get; set; }
        public int Days1_30PastDue { get; set; }
        public int Days31_60PastDue { get; set; }
        public int Days61_90PastDue { get; set; }
        public int DaysOver90PastDue { get; set; }

        // Delinquency Amounts
        public decimal CurrentAmount { get; set; }
        public decimal PastDue1_30Amount { get; set; }
        public decimal PastDue31_60Amount { get; set; }
        public decimal PastDue61_90Amount { get; set; }
        public decimal PastDueOver90Amount { get; set; }

        // Portfolio Distribution
        public int AgricultureLoans { get; set; }
        public int TradeLoans { get; set; }
        public int ManufacturingLoans { get; set; }
        public int ServicesLoans { get; set; }
        public int OtherSectorLoans { get; set; }

        public decimal AgricultureAmount { get; set; }
        public decimal TradeAmount { get; set; }
        public decimal ManufacturingAmount { get; set; }
        public decimal ServicesAmount { get; set; }
        public decimal OtherSectorAmount { get; set; }

        // By Loan Type
        public int PersonalLoans { get; set; }
        public int BusinessLoans { get; set; }
        public int MortgageLoans { get; set; }
        public int MicrofinanceLoans { get; set; }

        public decimal PersonalLoansAmount { get; set; }
        public decimal BusinessLoansAmount { get; set; }
        public decimal MortgageLoansAmount { get; set; }
        public decimal MicrofinanceLoansAmount { get; set; }

        // Branch Performance
        public Dictionary<string, decimal> BranchPortfolio { get; set; }
        public Dictionary<string, int> BranchLoanCounts { get; set; }

        // Date Information
        public DateTime AsOfDate { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
    }

    public class LoanDelinquencyData
    {
        // Delinquency by Age
        public int Current { get; set; }
        public int Days1_30 { get; set; }
        public int Days31_60 { get; set; }
        public int Days61_90 { get; set; }
        public int Days91_180 { get; set; }
        public int DaysOver180 { get; set; }

        // Delinquency Amounts
        public decimal CurrentAmount { get; set; }
        public decimal Days1_30Amount { get; set; }
        public decimal Days31_60Amount { get; set; }
        public decimal Days61_90Amount { get; set; }
        public decimal Days91_180Amount { get; set; }
        public decimal DaysOver180Amount { get; set; }

        // Totals
        public int TotalDelinquentLoans { get; set; }
        public decimal TotalDelinquentAmount { get; set; }
        public decimal TotalPortfolioAmount { get; set; }

        // Ratios
        public decimal DelinquencyRateByCount { get; set; }
        public decimal DelinquencyRateByAmount { get; set; }
        public decimal PortfolioAtRiskRate { get; set; }

        // Trend Analysis
        public decimal DelinquencyRate30DaysAgo { get; set; }
        public decimal DelinquencyRate60DaysAgo { get; set; }
        public decimal DelinquencyRate90DaysAgo { get; set; }
        public decimal Change30Days { get; set; }
        public decimal Change60Days { get; set; }
        public decimal Change90Days { get; set; }

        // Recovery Performance
        public int LoansRecoveredThisMonth { get; set; }
        public decimal AmountRecoveredThisMonth { get; set; }
        public int LoansWrittenOffThisMonth { get; set; }
        public decimal AmountWrittenOffThisMonth { get; set; }
        public int LoansRestructuredThisMonth { get; set; }
        public decimal AmountRestructuredThisMonth { get; set; }

        // Provision Analysis
        public decimal RequiredProvision { get; set; }
        public decimal ActualProvision { get; set; }
        public decimal ProvisionAdequacyRatio { get; set; }

        // By Product
        public Dictionary<string, DelinquencyByProduct> DelinquencyByProduct { get; set; }

        // By Branch
        public Dictionary<string, DelinquencyByBranch> DelinquencyByBranch { get; set; }

        // By Loan Officer
        public Dictionary<string, DelinquencyByOfficer> DelinquencyByOfficer { get; set; }

        // Top Delinquent Accounts
        public List<TopDelinquentLoan> TopDelinquentLoans { get; set; }
    }

    // Supporting classes for LoanDelinquencyData
    public class DelinquencyByProduct
    {
        public string ProductName { get; set; }
        public int TotalLoans { get; set; }
        public int DelinquentLoans { get; set; }
        public decimal DelinquencyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DelinquentAmount { get; set; }
    }

    public class DelinquencyByBranch
    {
        public string BranchName { get; set; }
        public int TotalLoans { get; set; }
        public int DelinquentLoans { get; set; }
        public decimal DelinquencyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DelinquentAmount { get; set; }
    }

    public class DelinquencyByOfficer
    {
        public string OfficerName { get; set; }
        public int TotalLoans { get; set; }
        public int DelinquentLoans { get; set; }
        public decimal DelinquencyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DelinquentAmount { get; set; }
    }

    public class TopDelinquentLoan
    {
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public string LoanOfficer { get; set; }
        public string Branch { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int DaysPastDue { get; set; }
        public decimal ArrearsAmount { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public string LoanStatus { get; set; }
        public string RiskCategory { get; set; }
    }


    // Simplified DTOs for Quick Implementation
    public class SimpleLoanDetails
    {
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string LoanStatus { get; set; }
    }

    public class SimpleRepaymentSchedule
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalDue { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentStatus { get; set; }
    }

    public class SimpleLoanRepayment
    {
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class SimpleLoanPortfolioItem
    {
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string LoanStatus { get; set; }
        public int DaysPastDue { get; set; }
    }

    public class SimpleLoanPortfolioSummary
    {
        public int TotalLoans { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public int DelinquentLoans { get; set; }
        public decimal DelinquencyRate { get; set; }
    }

    public class SimpleLoanDelinquencyData
    {
        public int CurrentLoans { get; set; }
        public int PastDueLoans { get; set; }
        public decimal PastDueAmount { get; set; }
        public decimal DelinquencyRate { get; set; }
    }



// 1. Unique Wrapper for the "data" object
public class FinancialReportResponseDto
    {
        [JsonProperty("reportType")]
        public int ReportType { get; set; }

        [JsonProperty("accountStatement")]
        public AccountStatementResponseDto AccountStatement { get; set; }
        public AccountStatementSummary summary { get; set; }
    }

    // 2. Unique Wrapper for the "accountStatement" object
    public class AccountStatementResponseDto
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("openingBalance")]
        public decimal OpeningBalance { get; set; }

        // This maps the JSON list "accountStatements" to the C# property "Transactions"
        [JsonProperty("accountStatements")]
        public List<TransactionRaw> Transactions { get; set; }
        public AccountStatementSummary summary { get; set; }
    }

    // 3. Updated Transaction Item with MAPPINGS (Crucial!)
    public class TransactionRaw
    {
        // Map JSON "transactionId" to C# "Id"
        [JsonProperty("transactionId")]
        public string Id { get; set; }

        [JsonProperty("transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty("transactionDate")]
        public DateTime CreatedDate { get; set; }

        // Map JSON "balanceAfter" to C# "Balance"
        [JsonProperty("balanceAfter")]
        public decimal Balance { get; set; }

        // Map JSON "balanceBefore" to C# "PreviousBalance"
        [JsonProperty("balanceBefore")]
        public decimal PreviousBalance { get; set; }

        [JsonProperty("debit")]
        public decimal Debit { get; set; }

        [JsonProperty("credit")]
        public decimal Credit { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }

        [JsonProperty("depositorName")]
        public string DepositorName { get; set; }

        // These fields are NOT in the JSON Transaction object, 
        // so they will remain null. You must handle nulls in your UI logic.
        public Branchs Branch { get; set; }
        public Account Account { get; set; }
        public string CustomerId { get; set; }
        public string DepositerTelephone { get; set; }
        public string Currency { get; set; }
        public string Logi { get; set; }
    }

    // Keep your existing Branchs and Account classes as they were...
    public class Branchs
    {
        public string Name { get; set; }
        public string BranchCode { get; set; }
        public string Telephone { get; set; }
        public string PBox { get; set; }
    }

    public class Account
    {
        public string CustomerName { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
    }

    public class AccountStatementSummary
    {
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public int TransactionsCount { get; set; }
        public decimal ClosingBalance { get; set; }
        public string OpeningBalance { get; set; }
        public decimal YearOpeningBalance { get; set; }
        public decimal PeriodOpeningBalance { get; set; }
    }

    // New API Response Model
    public class AccountStatementApiResponse
    {
        public AccountStatementData Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
        public bool Success { get; set; }
    }

    public class AccountStatementData
    {
        public int ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
        public FinancialReportFilter Filter { get; set; }
        public string MemberReference { get; set; }
        public AccountStatement AccountStatement { get; set; }
        // ... other properties as needed
    }

    public class AccountStatement
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<AccountStatementTransaction> AccountStatements { get; set; }
        public AccountStatementSummary Summary { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal YearOpeningBalance { get; set; }
        public decimal PeriodOpeningBalance { get; set; }
    }

    public class AccountStatementTransaction
    {
        public string TransactionId { get; set; }
        public string TransactionReference { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime AccountingDate { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalCharges { get; set; }
        public string TransactionType { get; set; }
        public string OperationType { get; set; }
        public string Operation { get; set; }
        public string Status { get; set; }
        public string ExternalReference { get; set; }
        public bool IsExternalOperation { get; set; }
        public string ExternalApplicationName { get; set; }
        public string SourceBranchId { get; set; }
        public string DestinationBranchId { get; set; }
        public bool IsInterBranchOperation { get; set; }
        public string TellerId { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Note { get; set; }
        public string ReceiptTitle { get; set; }
        public decimal Balance { get; set; }
    }

   

    public class MemberSituationData
    {
        public List<MemberAccount> MemberAccounts { get; set; }
        public List<MemberLoan> MemberLoans { get; set; }
        public TransactionSummary TransactionSummary { get; set; }
        public ReportParameters ReportPeriod { get; set; }
    }

    public class LoanRepaymentData
    {
        public LoanDetails LoanDetails { get; set; }
        public List<RepaymentSchedule> RepaymentSchedule { get; set; }
        public List<LoanRepayment> ActualRepayments { get; set; }
        public ReportParameters ReportPeriod { get; set; }
    }

    public class LoanSituationData
    {
        public List<LoanPortfolioItem> LoanPortfolio { get; set; }
        public LoanPortfolioSummary PortfolioSummary { get; set; }
        public LoanDelinquencyData DelinquencyData { get; set; }
        public ReportParameters ReportPeriod { get; set; }
    }
}
