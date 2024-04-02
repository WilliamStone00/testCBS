using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class UpdateLoanApplicationStatusCommand
    {
        public string Id { get; set; }
        [Required]
        public string ApprovalStatus { get; set; }
        [Required]
        public string ApprovalComment { get; set; }
        [Required]
        public string OTPCode { get; set; }
        public UpdateLoanApplicationStatusCommand()
        {
            ApprovalComment = "Having received confirmation from the loan committee, I hereby approve this loan.";
        }
    }
    public class OTPNotification
    {
        public string Id { get; set; }
        public string OPTCode { get; set; }
        public DateTime InitializedDate { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public string Status { get; set; }//Approved Or Expired
        public string LoanApplicationId { get; set; }
        public string InitiatedBy { get; set; }
        public string CustomerId { get; set; }
        public LoanApplication LoanApplication { get; set; }

    }
    public class AddOTPNotificationCommand
    {
        public string LoanApplicationId { get; set; }
        public string CustomerId { get; set; }

    }
    public class LoanApplication
    {
        public string Id { get; set; }
        public string LoanProductId { get; set; }
        public decimal Amount { get; set; }
        public bool IsThereGuarantor { get; set; }
        public bool IsThereCollateral { get; set; }
        public decimal InterestRate { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal VatRate { get; set; }
        public int NumberOfRepayment { get; set; }
        public string RepaymentCircle { get; set; }
        public string LoanType { get; set; }
        public int LoanDuration { get; set; }
        public DateTime FirstInstallmentDate { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime DisbursementDate { get; set; }
        public string CustomerId { get; set; }
        public string EconomicActivityId { get; set; }
        public string DisburstmentType { get; set; }
        public bool IsDisbursted { get; set; }
        public string AmortizationType { get; set; }
        public int GracePeriodBeforeFirstPayment { get; set; }
        public int GracePeriodAfterMaturityDate { get; set; }
        public string Status { get; set; }
        public decimal CollateralCoverageRate { get; set; }
        public decimal ShareAccountCoverageRate { get; set; }
        public decimal SavingAccountCoverageRate { get; set; }
        public decimal SalaryAccountCoverageRate { get; set; }
        public decimal GuaratorSavingAccountCoverageRate { get; set; }
        public string LoanPurposeId { get; set; }
        public decimal TotalLoanRiskCoverage { get; set; }
        public string OrganizationId { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string ApprovalStatus { get; set; }
        public string LoanManager { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDisbursed { get; set; }
        public string ApprovalComment { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }
        public virtual ICollection<LoanApplicationCollateral> Collateras { get; set; }
        public virtual ICollection<LoanGuarantor> Guarantors { get; set; }
        public virtual ICollection<LoanCommiteeValidationHistory> LoanCommiteeValidations { get; set; }
        public virtual ICollection<DocumentAttachedToLoan> DocumentAttachedToLoans { get; set; }
        public virtual LoanPurpose LoanPurpose { get; set; }
        public virtual ICollection<OTPNotification> OTPNotifications { get; set; }

        public LoanApplication()
        {
            // Initialize double properties to 0
            Amount = 0;
            InterestRate = 0;
            ProcessingFee = 0;
            VatRate= 0;
            CollateralCoverageRate = 0;
            ShareAccountCoverageRate = 0;
            SavingAccountCoverageRate = 0;
            TotalLoanRiskCoverage = 0;
            LoanDuration = 0;
            GracePeriodAfterMaturityDate = 15;
            GracePeriodBeforeFirstPayment = 1;
            AmortizationType = "Constant_Amortization";
        }
    }
    public class Loan
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public decimal Principal { get; set; }
        public decimal InterestForcasted { get; set; }
        public decimal InterestRate { get; set; }
        public decimal LastPayment { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
        public decimal DueAmount { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal AccrualInterestPaid { get; set; }
        public decimal AccrualInterestBalance { get; set; }
        public decimal TotalPrincipalPaid { get; set; }
        public decimal PrincipalBalance { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPaid { get; set; }
        public decimal TaxBalance { get; set; }
        public decimal FeePaid { get; set; }
        public decimal FeeBalance { get; set; }
        public decimal Penalty { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal PenaltyBalance { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime FirstInstallmentDate { get; set; }
        public DateTime NextInstallmentDate { get; set; }
        public DateTime LoanDate { get; set; }
        public bool IsLoanDisbursted { get; set; }
        public DateTime LastInterestCalculatedDate { get; set; }
        public DateTime LastRefundDate { get; set; }
        public string CustomerId { get; set; }
        public string LoanManager { get; set; }
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

        public string LoanId { get; set; }
        public decimal Amount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Tax { get; set; }
        public decimal Penalty { get; set; }
        public string Comment { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentChannel { get; set; }
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string PaymentMode { get; set; }
        public string RepaymentType { get; set; }
        public decimal Paid { get; set; }
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
        public string LoanId { get; set; }
        public decimal Amount { get; set; }//The loan amount that is submited for simulation
        public int Sno { get; set; }
        public decimal BegginingBalance { get; set; }
        public decimal Principal { get; set; }
        public decimal PrincipalPaid { get; set; }
        public decimal Interest { get; set; }
        public decimal PreviouseInterest { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal TotalDue { get; set; }
        public decimal Balance { get; set; }
        public decimal Fee { get; set; }
        public decimal Annuity { get; set; }
        public string Description { get; set; }
        public decimal Penalty { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal Tax { get; set; }
        public bool PreviousInstallmentDue { get; set; }
        public decimal TaxPaid { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public DateTime DateOfPayment { get; set; }
        public string Status { get; set; }
        public string MethodOfPayment { get; set; }
        public bool IsRescheduled { get; set; }
        public bool IsCompleted { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public Loan Loan { get; set; }
        public List<RefundDetail> RefundDetails { get; set; }
    }

    public class LoanParameters
    {
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanDuration { get; set; } // Represents the duration value (int)
        public string LoanDurationType { get; set; } //Represents the duration type (Months, Days, Weeks, Years)
        public string RepaymentCycle { get; set; }//Daily, Weekly, Biweekly, Monthly, Bimonthly, Quarterly, Every4Months, SemiAnnual, Every9Months, Yearly, LumpSum
        public DateTime RepaymentStartDate { get; set; }
        public string StrRepaymentStartDate { get; set; }
        public string InterestCalculationPeriod { get; set; }//Daily, Weekly, Monthly, Yearly etc
        public decimal Penalty { get; set; }
        public decimal Tax { get; set; }
        public string AmortizationType { get; set; }
        public decimal Fee { get; set; }
        public int NumberOfInstallments { get; set; }
        public bool IsSimulation { get; set; }
        public LoanParameters()
        {
            // Set default values
            Amount = 1000000;
            InterestRate = 1;
            RepaymentStartDate = DateTime.Now.AddDays(30);
            LoanDuration = 10;
            LoanDurationType = "Months"; // Default to Months if not provided
            RepaymentCycle = "Monthly"; // Default to Monthly if not provided
            InterestCalculationPeriod = "Daily"; // Default to Daily if not provided
            AmortizationType = "Constant_Amortization";
            Penalty = 0;
            Tax = 0;
            Fee = 0;
            IsSimulation = true;
        }
    }

}

