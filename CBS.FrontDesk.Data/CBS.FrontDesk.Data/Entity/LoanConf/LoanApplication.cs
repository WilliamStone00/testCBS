using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{

    public class LoanApplication
    {
        public string Id { get; set; }
        [Required]
        public string LoanProductId { get; set; }
        [Required]
        public string LoanType { get; set; }
        [Required]

        public double Amount { get; set; }
        [Required]

        public double InterestRate { get; set; }
        [Required]

        public double ProcessingFee { get; set; }
        [Required]

        public int NumberOfRepayment { get; set; }
        [Required]

        public string RepaymentCircle { get; set; }
        [Required]

        public int LoanDuration { get; set; }
        public DateTime FirstInstallmentDate { get; set; }

        [Required]

        public string CustomerId { get; set; }
        [Required]

        public string EconomicActivityId { get; set; }
        [Required]

        public int GracePeriod { get; set; }
        [Required]

        public double GracePeriodRate { get; set; }
        [Required]

        public double InsuranceCoverageRate { get; set; }
        [Required]

        public double CollateralCoverageRate { get; set; }
        [Required]

        public double ShareAccountCoverageRate { get; set; }
        [Required]

        public double SavingAccountCoverageRate { get; set; }
        [Required]

        public bool IsGuaranteeProvided { get; set; }
        public bool IsThereGuarantor { get; set; }
        public bool IsCollateralProvided { get; set; }
        [Required]

        public string LoanPurposeId { get; set; }
        [Required]

        public double TotalLoanRiskCoverage { get; set; }
        public string ApprovalStatus { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDisbursed { get; set; }
        public string ApprovalComment { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string OrganizationId { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public LoanProduct LoanProduct { get; set; }
        //public List<LoanApplicationCollateral> Collateras { get; set; }
        public List<LoanGuarantor> Guarantors { get; set; }
        public List<LoanCommiteeValidation> LoanCommiteeValidations { get; set; }
        public List<DocumentAttachedToLoan> DocumentAttachedToLoans { get; set; }
        public LoanPurpose LoanPurpose { get; set; }
        public EconomicActivity EconomicActivity { get; set; }
        public LoanApplication()
        {
            // Initialize double properties to 0
            Amount = 0.0;
            InterestRate = 0.0;
            ProcessingFee = 0.0;
            GracePeriodRate = 0.0;
            InsuranceCoverageRate = 0.0;
            CollateralCoverageRate = 0.0;
            ShareAccountCoverageRate = 0.0;
            SavingAccountCoverageRate = 0.0;
            TotalLoanRiskCoverage = 0.0;

            // Initialize int properties to 0
            NumberOfRepayment = 0;
            LoanDuration = 0;
            GracePeriod = 0;
        }
    }
    public class Loan
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Paid { get; set; }
        public decimal LastPayment { get; set; }
        public decimal Balance { get; set; }
        public decimal DueAmount { get; set; }
        public decimal Accrual { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalInterestPaid { get; set; }
        public decimal TotalPrincipalPaid { get; set; }
        public decimal BalanceInterest { get; set; }
        public decimal BalancePrincipal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FeePaid { get; set; }
        public decimal FeeOwed { get; set; }
        public decimal PenaltyAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime LastInterestCalculatedDate { get; set; }
        public DateTime LastRefundDate { get; set; }
        public string CustomerId { get; set; }
        public string LoanStatus { get; set; }
        public bool IsRestructured { get; set; }
        public string NewLoanId { get; set; }
        public bool IsWriteOffLoan { get; set; }
        public bool IsDeliquentLoan { get; set; }
        public bool IsCurrentLoan { get; set; }
        public DateTime MaturityDate { get; set; }
        public string OrganizationId { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public LoanApplication LoanApplication { get; set; }
        public List<Refund> Refunds { get; set; }
        public List<LoanAmortization> LoanAmortizations { get; set; }
    }
    public class Refund
    {

        public string Id { get; set; }
        public string LoanId { get; set; }
        public string CustomerId { get; set; }
        public string Comment { get; set; }
        public string PaymentMode { get; set; }
        public string RepaymentType { get; set; }
        public decimal Paid { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Penalty { get; set; }
        public decimal Tax { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }

        public DateTime DateOfPayment { get; set; }
        public Loan Loan { get; set; }
        public List<RefundDetail> RefundDetails { get; set; }

    }
    public class RefundDetail
    {

        public string Id { get; set; }
        public string RefundId { get; set; }
        public string LoanAmortizationId { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxAmountBalance { get; set; }
        public decimal Interest { get; set; }
        public decimal InterestBalance { get; set; }
        public decimal PrincipalBalance { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal PenaltyAmountBalance { get; set; }
        public decimal Balance { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }

        public Refund Refund { get; set; }
    }
    public class LoanAmortization
    {
        public string Id { get; set; }
        public int Sno { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string Description { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Fee { get; set; }
        public decimal Penalty { get; set; }
        public decimal Tax { get; set; }
        public decimal Due { get; set; }
        public decimal Paid { get; set; }
        public decimal PendingDue { get; set; }
        public decimal TotalDue { get; set; }
        public decimal PrincipalDue { get; set; }
        public decimal PrincipalBalance { get; set; }
        public decimal Balance { get; set; }
        public DateTime DateOfPayment { get; set; }
        public string Status { get; set; }
        public string MethodOfPayment { get; set; }
        public bool IsRescheduled { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string LoanId { get; set; }
        public Loan Loan { get; set; }
        public List<RefundDetail> RefundDetails { get; set; }
    }

    public class LoanParameters
    {
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanDuration { get; set; } // Represents the duration value
        public string LoanDurationType { get; set; } // Represents the duration type (Months, Days, Weeks, Years)
        public string RepaymentCycle { get; set; }
        public DateTime InterestStartDate { get; set; }
        public string InterestCalculationPeriod { get; set; }
        public string LoanInterestMethod { get; set; } //Flat Or Rate
        public decimal Penalty { get; set; }
        public decimal Tax { get; set; }
        public decimal Fee { get; set; }
        public bool IsSimulation { get; set; }
        public LoanParameters()
        {
            // Set default values
            Amount = 50000;
            InterestRate = 1;
            LoanDuration = 5;
            LoanDurationType = "Months"; // Default to Months if not provided
            RepaymentCycle = "Monthly"; // Default to Monthly if not provided
            InterestStartDate = DateTime.MinValue;
            InterestCalculationPeriod = "Daily"; // Default to Daily if not provided
            LoanInterestMethod = "Flat";
            Penalty = 0;
            Tax = 0;
            Fee = 0;
            IsSimulation = true;
        }
    }

}

