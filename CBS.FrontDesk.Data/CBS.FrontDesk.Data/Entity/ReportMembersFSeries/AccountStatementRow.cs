using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.ReportData;
using CBS.FrontDesk.Data.Entity.LoanConf;
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
        public string PeriodFrom { get; set; }
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
        public string Year { get; set; }
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

        public decimal NetBalance { get; set; }
        public decimal BlockedAmount { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalBlockedAmount { get; set; }
        public decimal TotalActualBalance { get; set; }

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
        public string Boldbalance { get; set; }
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
        public string Logo { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        public string LoanAccountNumber { get; set; }
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string LoanType { get; set; }
        public decimal LoanAmount { get; set; }
        public string MembersReference { get; set; }
        public string MembersName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTellephone { get; set; }
        public string repaymentparameterAccount { get; set; }

        public string Comment { get; set; }
        public string Date { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentChannel { get; set; }

        public decimal Amount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Penalty { get; set; }
        public decimal Tax { get; set; }

        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string Status { get; set; }
        public string Year { get; set; }

        public decimal Balance { get; set; }
        public bool IsComplete { get; set; }

        public DateTimeOffset DateOfPayment { get; set; }

        // Summary
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalPenalty { get; set; }
        public decimal TotalPaid { get; set; }
    }

    public class LoanSituationRow
    {
        public decimal TotalOutstanding { get; set; }
        public decimal ActiveLoans { get; set; }
        public decimal DelinquentLoans { get; set; }
        public decimal PerAmount { get; set; }

        // Header Information    
        public string ReportTitle { get; set; }
        public string BankName { get; set; }
        public string Logo { get; set; }
        public string Year { get; set; }
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string Periodfrom { get; set; }
        public string PeriodTo { get; set; }
        public string PrintedBy { get; set; }
        public string PrintedOn { get; set; }

        public string BranchTellephone { get; set; }
        public string BranchAddress { get; set; }
        public string LoanAccount { get; set; }

        public string LoanId { get; set; }
        public string ContractCodeId { get; set; }


        // Portfolio Summary
        public int TotalLoans { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public decimal AverageInterestRate { get; set; }
        public decimal DelinquencyRate { get; set; }
        public string LoanAccountNumber { get; set; }
        public decimal LoanAmount { get; set; }
        // Loan Details
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public string LoanType { get; set; }
        
        public decimal OutstandingBalance { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Tax { get; set; }
        public decimal Vat { get; set; }
        public string Status { get; set; }
        public int DaysPastDue { get; set; }      
   
        public DateTime LoanDate { get; set; }
        public DateTime LastRepaymentDate { get; set; }
        public decimal LoanBalance { get; set; }
        public decimal LoanRepaymentAmount { get; set; }
        public decimal Interest { get; set; }
        public int Deliquencedays { get; set; }
        public decimal DeliquenceAmount { get; set; }
        public decimal DelInterest { get; set; }
        public decimal AdvancedPaymentAmount { get; set; }
        public decimal DueAmount { get; set; }

        public string ContractCode { get; set; }
        public int Instalment { get; set; }
        public int NumberOfInstallment { get; set; }
        
        public decimal Balance { get; set; }
        public decimal Principal { get; set; }
        public decimal Penalty { get; set; }
        public decimal TotalRepayment { get; set; }

        public decimal LastRepaymentAmount { get; set; }
        public DateTime NextRepaymentDate { get; set; }
       
        public int DelDays { get; set; }
        public decimal DelAmount { get; set; }

        public string MemberReference { get; set; }
        public string MembersName { get; set; }
        public string MembersBranch { get; set; }
        public string MemberBranchCode { get; set; }
        public string CNI { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
       
        

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
        public decimal TotalOutstanding { get; set; }
        public decimal ActiveLoans { get; set; }
        public decimal DelinquentLoans { get; set; }
        public decimal parAmount { get; set; }
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
        public string Id { get; set; }
        public string TransactionReference { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime AccountingDate { get; set; }

        public decimal Balance { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Amount { get; set; }
        public string Operation { get; set; }
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string DepositorName { get; set; }
        public bool IsDepositDoneByAccountOwner { get; set; }
        public string DepositorIDNumber { get; set; }
        public string DepositorTelephone { get; set; }
        public string DepositorIDIssueDate { get; set; }
        public string DepositorIDExpiryDate { get; set; }
        public string DepositorIDPlaceOfIssue { get; set; }
        public string DepositorNote { get; set; }

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

    public class LoanRepaymentData
    {
        public LoanDetails LoanDetails { get; set; }
        public List<RepaymentSchedule> RepaymentSchedule { get; set; }
        public List<LoanRepayment> ActualRepayments { get; set; }
        public ReportParameters ReportPeriod { get; set; }
    }

    // Loan History end point response  
    public class ReportResponse
    {
        public ReportData Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
        public bool Success { get; set; }
    }

    public class ReportData
    {
        public int ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
       

        public string MemberReference { get; set; }

        public object MemberSituation { get; set; }
        public object AccountSituation { get; set; }
        public object AccountStatement { get; set; }
        public object LoanRepayment { get; set; }

        public LoanHistory LoanHistory { get; set; }
    }
    public class LoanHistory
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public LoanMetrics Metrics { get; set; }
        public List<LoanItem> Loans { get; set; }
    }

    public class LoanMetrics
    {
        public int TotalLoans { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int ActiveLoans { get; set; }
        public int DelinquentLoans { get; set; }
        public decimal ParAmount { get; set; }
    }
    public class LoanItem
    {
        public string LoanId { get; set; }
        public string ContractCode { get; set; }
        public string LoanType { get; set; }
        public string Status { get; set; }
        public string AccountNumber { get; set; }
        public string loanAccountNumber { get; set; }
        public decimal  LoanAmount { get; set; }

        public decimal Balance { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Vat { get; set; }
        public decimal Penalty { get; set; }
        public decimal TotalRepayment { get; set; }

        public decimal LastRepaymentAmount { get; set; }
        public DateTime LastRepaymentDate { get; set; }
        public DateTime NextRepaymentDate { get; set; }
        public DateTime LoanDate { get; set; }

        public int DelDays { get; set; }
        public int Installment { get; set; }
        public decimal DelAmount { get; set; }
        public decimal DelInterest { get; set; }
        public decimal DueAmount { get; set; }

        public string MemberReference { get; set; }
        public string MembersName { get; set; }
        public string MembersBranch { get; set; }
        public string MemberBranchCode { get; set; }

        public DateTime DisbursementDate { get; set; }
    }

    //Loan Repayment
    public class LoanSituationReportDto
    {
        public int ReportType { get; set; }

        public DateTimeOffset GeneratedAt { get; set; }

        public string MemberReference { get; set; }

        public LoanRepaymentReportDto LoanRepayment { get; set; }
    }

    public class LoanRepaymentReportDto
    {
        public int Mode { get; set; }

        public string LoanId { get; set; }

        public DateTimeOffset DateFrom { get; set; }
        public DateTimeOffset DateTo { get; set; }

        public object Loan { get; set; }

        public List<LoanRepaymentTransactionDto> RepaymentLines { get; set; }

        public LoanRepaymentSummaryDto Summary { get; set; }
    }

    public class LoanRepaymentTransactionDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string CustomerId { get; set; }

        public string Comment { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentChannel { get; set; }

        public decimal Amount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Penalty { get; set; }
        public decimal Tax { get; set; }
        public decimal LoanAmount { get; set; }
        public string LoanAccountNumber { get; set; }
        public string LoanType { get; set; }

        public string BranchId { get; set; }
        public string BankId { get; set; }

        public string completeStatus { get; set; }
        public decimal Balance { get; set; }
        public bool IsComplete { get; set; }

        public DateTimeOffset DateOfPayment { get; set; }
    }

    public class LoanRepaymentSummaryDto
    {
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalPenalty { get; set; }
        public decimal TotalPaid { get; set; }
    }

    // end of loan repayment 




    // loanrepayment - Account Statement - Loan History
    public class MemberFinancialReport
    {
        public int ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
        

        public string MemberReference { get; set; }
        public object MemberSituation { get; set; }   // null in JSON

        public AccountSituation AccountSituation { get; set; }

        public object AccountStatement { get; set; }  // null
        public object LoanRepayment { get; set; }     // null
        public object LoanHistory { get; set; }       // null
    }

    public class AccountSituation
    {
        public DateTime OperationDate { get; set; }
        public List<AccountSnapshot> Accounts { get; set; }
        public AccountSummary Summary { get; set; }
    }

    public class AccountSnapshot
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }

        public decimal Balance { get; set; }
        public decimal BlockedAmount { get; set; }
        public decimal ActualBalance { get; set; }

        public DateTime SnapshotDate { get; set; }
        public DateTime LastTransactionDate { get; set; }
        public decimal LastTransactedAmount { get; set; }
    }

    public class AccountSummary
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalBlockedAmount { get; set; }
        public decimal TotalActualBalance { get; set; }
    }

    // penalties ==========================
    public class PenaltyReportRow
    {
        // Header Information
        public string ReportTitle { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }
        public string Currency { get; set; }
        public string Logo { get; set; }

        // Customer Information
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Telephone { get; set; }
        public string CustomerAddress { get; set; }
        public string Email { get; set; }

        // Account Information
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }

        // Loan Information
        public string LoanNumber { get; set; }
        public string LoanType { get; set; }
        public decimal LoanAmount { get; set; }
        public DateTime LoanDisbursementDate { get; set; }
        public DateTime LoanMaturityDate { get; set; }
        public decimal LoanOutstandingBalance { get; set; }

        // Penalty Information
        public string PenaltyType { get; set; } // Late Payment, Early Withdrawal, Overdraft, Minimum Balance
        public string PenaltyCode { get; set; }
        public string PenaltyDescription { get; set; }
        public decimal PenaltyRate { get; set; }
        public string PenaltyRateDisplay { get { return PenaltyRate.ToString("0.00") + "%"; } }
        public decimal PrincipalAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TotalAmount { get { return PrincipalAmount + PenaltyAmount; } }

        // Overdue Information
        public int DaysOverdue { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PenaltyDate { get; set; }
        public DateTime? ActualPaymentDate { get; set; }

        // Transaction Information
        public string TransactionReference { get; set; }
        public string TransactionType { get; set; }
        public string PenaltyTransactionType { get; set; } // Assessment, Waiver, Collection

        // Status Information
        public string Status { get; set; } // Pending, Paid, Waived, Written Off
        public bool IsCollected { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string CollectionMethod { get; set; } // Cash, Transfer, Deduction
        public bool IsWaived { get; set; }
        public string WaiverReason { get; set; }
        public DateTime? WaiverDate { get; set; }
        public string WaivedBy { get; set; }

        // Summary Fields (for footer)
        public decimal? TotalPenaltyAmount { get; set; }
        public decimal? TotalPrincipalAmount { get; set; }
        public int? TotalTransactions { get; set; }
        public decimal? AveragePenaltyRate { get; set; }
        public int? TotalOverdueDays { get; set; }

        // Formatted Display Properties
        public string PeriodFromDisplay { get { return PeriodFrom.ToString("dd/MM/yyyy"); } }
        public string PeriodToDisplay { get { return PeriodTo.ToString("dd/MM/yyyy"); } }
        public string DueDateDisplay { get { return DueDate.ToString("dd/MM/yyyy"); } }
        public string PenaltyDateDisplay { get { return PenaltyDate.ToString("dd/MM/yyyy"); } }
        public string PrintedOnDisplay { get { return PrintedOn.ToString("dd/MM/yyyy HH:mm:ss"); } }
        public string LoanDisbursementDateDisplay { get { return LoanDisbursementDate.ToString("dd/MM/yyyy"); } }
        public string LoanMaturityDateDisplay { get { return LoanMaturityDate.ToString("dd/MM/yyyy"); } }
        public string CollectionDateDisplay { get { return CollectionDate?.ToString("dd/MM/yyyy") ?? ""; } }
        public string WaiverDateDisplay { get { return WaiverDate?.ToString("dd/MM/yyyy") ?? ""; } }
        public string ActualPaymentDateDisplay { get { return ActualPaymentDate?.ToString("dd/MM/yyyy") ?? ""; } }

        // Amount Display Properties
        public string PrincipalAmountDisplay { get { return PrincipalAmount.ToString("N1"); } }
        public string PenaltyAmountDisplay { get { return PenaltyAmount.ToString("N1"); } }
        public string TotalAmountDisplay { get { return TotalAmount.ToString("N1"); } }
        public string LoanAmountDisplay { get { return LoanAmount.ToString("N1"); } }
        public string LoanOutstandingBalanceDisplay { get { return LoanOutstandingBalance.ToString("N1"); } }
        public string TotalPenaltyAmountDisplay { get { return TotalPenaltyAmount?.ToString("N1") ?? "0.0"; } }
        public string TotalPrincipalAmountDisplay { get { return TotalPrincipalAmount?.ToString("N1") ?? "0.0"; } }

        // Additional Fields
        public string Remarks { get; set; }
        public string AssessmentBy { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string CollectionReference { get; set; }
        public string ChargeCode { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrenceFrequency { get; set; } // Daily, Weekly, Monthly
        public int RecurrenceCount { get; set; }
        public bool IsActive { get; set; }

        // Grace Period Information
        public int GracePeriodDays { get; set; }
        public bool IsWithinGracePeriod { get; set; }

        // For grouping and sorting
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }

        public List<PenaltyDetail> PenaltyDetails { get; set; }
        public PenaltySummary Summary { get; set; }
    }
    public class PenaltyDetail
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public string LoanNumber { get; set; }
        public string PenaltyType { get; set; }
        public decimal PenaltyRate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PenaltyDate { get; set; }
        public string TransactionReference { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
    public class PenaltySummary
    {
        public decimal TotalPenaltyAmount { get; set; }
        public decimal TotalPrincipalAmount { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AveragePenaltyRate { get; set; }
        public int TotalOverdueDays { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal WaivedAmount { get; set; }
        public decimal OutstandingAmount { get { return TotalPenaltyAmount - CollectedAmount - WaivedAmount; } }
    }
    public class VatDetail
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionReference { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerTaxId { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public string VatType { get; set; }
        public string DocumentType { get; set; }
    }
    // V
    public class VatReportRow
    {
        // Header Information
        public string ReportTitle { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }
        public string Currency { get; set; }
        public string VatRate { get; set; }
        public string Logo { get; set; }

        // Customer/Tax Payer Information
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Telephone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerTaxId { get; set; }
        public string TaxRegistrationNumber { get; set; }

        // Transaction Information
        public DateTime TransactionDate { get; set; }
        public string TransactionReference { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceType { get; set; } // Sales, Purchase, Service
        public string InvoiceDescription { get; set; }

        // Amount Information
        public decimal TaxableAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public decimal ZeroRatedAmount { get; set; }

        // VAT Details
        public string VatType { get; set; } // Output VAT, Input VAT
        public string VatCategory { get; set; } // Standard, Reduced, Zero-rated, Exempt
        public bool IsVatInclusive { get; set; }
        public decimal VatRatePercentage
        {
            get
            {
                if (decimal.TryParse(VatRate?.Replace("%", ""), out decimal rate))
                    return rate;
                return 18.0m; // Default
            }
        }

        // Status
        public string Status { get; set; } // Paid, Due, Reversed
        public bool IsPosted { get; set; }
        public DateTime? PostingDate { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Supplier/Customer Details
        public string SupplierName { get; set; }
        public string SupplierTaxId { get; set; }
        public string SupplierAddress { get; set; }

        // Transaction Type
        public string TransactionType { get; set; } // Sale, Purchase, Refund
        public string ServiceType { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        // Summary Fields (for footer)
        public decimal? TotalTaxableAmount { get; set; }
        public decimal? TotalVatAmount { get; set; }
        public decimal? TotalAmountSummary { get; set; }
        public int? TotalTransactions { get; set; }
        public decimal? TotalExemptAmount { get; set; }
        public decimal? TotalZeroRatedAmount { get; set; }

        // Formatted Display Properties
        public string PeriodFromDisplay { get { return PeriodFrom.ToString("dd/MM/yyyy"); } }
        public string PeriodToDisplay { get { return PeriodTo.ToString("dd/MM/yyyy"); } }
        public string TransactionDateDisplay { get { return TransactionDate.ToString("dd/MM/yyyy"); } }
        public string PrintedOnDisplay { get { return PrintedOn.ToString("dd/MM/yyyy HH:mm:ss"); } }
        public string PostingDateDisplay { get { return PostingDate?.ToString("dd/MM/yyyy") ?? ""; } }
        public string PaymentDateDisplay { get { return PaymentDate?.ToString("dd/MM/yyyy") ?? ""; } }

        // Amount Display Properties
        public string TaxableAmountDisplay { get { return TaxableAmount.ToString("N1"); } }
        public string VatAmountDisplay { get { return VatAmount.ToString("N1"); } }
        public string TotalAmountDisplay { get { return TotalAmount.ToString("N1"); } }
        public string ExemptAmountDisplay { get { return ExemptAmount.ToString("N1"); } }
        public string ZeroRatedAmountDisplay { get { return ZeroRatedAmount.ToString("N1"); } }
        public string TotalTaxableAmountDisplay { get { return TotalTaxableAmount?.ToString("N1") ?? "0.0"; } }
        public string TotalVatAmountDisplay { get { return TotalVatAmount?.ToString("N1") ?? "0.0"; } }
        public string TotalAmountSummaryDisplay { get { return TotalAmountSummary?.ToString("N1") ?? "0.0"; } }

        // Additional Fields
        public string DocumentType { get; set; } // Invoice, Receipt, Credit Note, Debit Note
        public string ReferenceDocument { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public bool IsReconciled { get; set; }
        public string ReconciliationReference { get; set; }
        public List<VatDetail> VatDetails { get; set; }
        public VatSummary Summary { get; set; }
    }

    // Supporting classes for VAT data


    public class VatSummary
    {
        public decimal TotalTaxableAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
        public decimal OutputVat { get; set; }
        public decimal InputVat { get; set; }
        public decimal NetVatPayable { get { return OutputVat - InputVat; } }
        public decimal TotalExemptAmount { get; set; }
        public decimal TotalZeroRatedAmount { get; set; }
    }


    public class InterestReportRow
    {
        // Header Information
        public string ReportTitle { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string PrintedBy { get; set; }
        public DateTime PrintedOn { get; set; }
        public string Currency { get; set; }
        public string Logo { get; set; }

        // Customer Information
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Telephone { get; set; }
        public string CustomerAddress { get; set; }

        // Account Information
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }

        // Interest Calculation Details
        public decimal InterestRate { get; set; }
        public string InterestRateDisplay { get { return InterestRate.ToString("0.00") + "%"; } }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get { return PrincipalAmount + InterestAmount; } }

        // Calculation Period
        public DateTime CalculatedFrom { get; set; }
        public DateTime CalculatedTo { get; set; }
        public int DaysCount { get; set; }

        // Transaction Information
        public DateTime TransactionDate { get; set; }
        public string TransactionReference { get; set; }
        public string TransactionType { get; set; }
        public string TransactionDescription { get; set; }

        // Status
        public string Status { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostingDate { get; set; }

        // Loan Information (if applicable)
        public string LoanNumber { get; set; }
        public string LoanType { get; set; }
        public decimal LoanAmount { get; set; }

        // Summary Fields (for footer)
        public decimal? TotalInterestAmount { get; set; }
        public decimal? TotalPrincipalAmount { get; set; }
        public int? TotalTransactions { get; set; }
        public decimal? AverageInterestRate { get; set; }

        // Formatted Display Properties
        public string PeriodFromDisplay { get { return PeriodFrom.ToString("dd/MM/yyyy"); } }
        public string PeriodToDisplay { get { return PeriodTo.ToString("dd/MM/yyyy"); } }
        public string CalculatedFromDisplay { get { return CalculatedFrom.ToString("dd/MM/yyyy"); } }
        public string CalculatedToDisplay { get { return CalculatedTo.ToString("dd/MM/yyyy"); } }
        public string TransactionDateDisplay { get { return TransactionDate.ToString("dd/MM/yyyy"); } }
        public string PrintedOnDisplay { get { return PrintedOn.ToString("dd/MM/yyyy HH:mm:ss"); } }

        // Amount Display Properties
        public string PrincipalAmountDisplay { get { return PrincipalAmount.ToString("N1"); } }
        public string InterestAmountDisplay { get { return InterestAmount.ToString("N1"); } }
        public string TotalAmountDisplay { get { return TotalAmount.ToString("N1"); } }
        public string TotalInterestAmountDisplay { get { return TotalInterestAmount?.ToString("N1") ?? "0.0"; } }
        public string TotalPrincipalAmountDisplay { get { return TotalPrincipalAmount?.ToString("N1") ?? "0.0"; } }

        // Additional Fields
        public string InterestType { get; set; } // Normal, Penalty, Overdue
        public string CalculationMethod { get; set; } // Simple, Compound, Flat
        public string Frequency { get; set; } // Daily, Monthly, Quarterly, Annual
        public bool IsTaxable { get; set; }
        public decimal? TaxAmount { get; set; }
        public string TaxAmountDisplay { get { return TaxAmount?.ToString("N1") ?? "0.0"; } }

        // For grouping and sorting
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public List<InterestDetail> InterestDetails { get; set; }
        public InterestSummary Summary { get; set; }
    }

    // Supporting classes for interest data
    public class InterestDetail
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public decimal InterestRate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public DateTime CalculatedFrom { get; set; }
        public DateTime CalculatedTo { get; set; }
        public int DaysCount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionReference { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public string LoanNumber { get; set; }
        public string InterestType { get; set; }
    }

    public class InterestSummary
    {
        public decimal TotalInterestAmount { get; set; }
        public decimal TotalPrincipalAmount { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageInterestRate { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal NetInterestAmount { get { return TotalInterestAmount - TotalTaxAmount; } }
    }

    public class MemberSituationMainRpt
    {
        public string Logo { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }

        public decimal TotalBalance { get; set; }
        public decimal TotalBlockedAmount { get; set; }
        public decimal TotalLiquidSavings { get; set; }
        public decimal Actual { get; set; }
        public decimal LoanAndCoverageGapAmount { get; set; }
        public decimal TotalLoanBalance { get; set; }
        public string LoanCount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal SavingsAgainstLoanRatio { get; set; }
        public string LoanRiskLevel { get; set; }
        public string LoanRecommendation { get; set; }

        public string Telephone { get; set; }
        public string PrintedBy { get; set; }
        public string Address { get; set; }
        public string CNI { get; set; }
        public string Year { get; set; }
        public List<AccountSnapshot> AccountSituations { get; set; }
        public List<LoanSituationRow> LoanHistories { get; set; }
    }

    public class MemberSituationBackendResponse
    {
        public int ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
       
        public string MemberReference { get; set; }
        public MemberSituationBackend MemberSituation { get; set; }
    }

    public class MemberSituationBackend
    {
        public DateTime OperationDate { get; set; }
        public List<AccountBackendDto> Accounts { get; set; }
        public List<LoanBackendDto> Loans { get; set; }
        public MemberSituationSummary Summary { get; set; }
    }

    public class AccountBackendDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal BlockedAmount { get; set; }
        public decimal ActualBalance { get; set; }
        public decimal lastTransactedAmount { get; set; }
        public DateTime SnapshotDate { get; set; }
    }

    public class LoanBackendDto
    {
        public string LoanId { get; set; }
        public string ContractCode { get; set; }
        public string LoanType { get; set; }
        public string Status { get; set; }
        public decimal Balance { get; set; }
        public string LoanAccount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Penalty { get; set; }
        public decimal TotalRepayment { get; set; }
        public decimal LastRepaymentAmount { get; set; }
        public DateTime LastRepaymentDate { get; set; }
        public DateTime NextRepaymentDate { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DisbursementDate { get; set; }
        public int DelDays { get; set; }
        public decimal DelAmount { get; set; }
        public decimal DelInterest { get; set; }
        public string MemberReference { get; set; }
        public string MembersName { get; set; }
        public string MembersBranch { get; set; }
        public string MemberBranchCode { get; set; }
    }
    public class MemberSituationSummary
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalBlockedAmount { get; set; }
        public decimal TotalLiquidSavings { get; set; }

        public decimal Actual { get; set; }

        public decimal LoanAndCoverageGapAmount { get; set; }
        public decimal TotalLoanBalance { get; set; }

        public int LoansCount { get; set; }
        public decimal TotalPaid { get; set; }

        public decimal SavingsAgainstLoanRatio { get; set; }

        public string LoanRiskLevel { get; set; }
        public string LoanRecommendation { get; set; }
    }

}



