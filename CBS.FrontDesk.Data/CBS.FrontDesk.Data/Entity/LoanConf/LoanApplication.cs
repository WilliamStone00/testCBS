

using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
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
        public bool IsChargesInclussive { get; set; }
        public bool IsOverRightOldLoanInterestAndBalance { get; set; }
        public bool StopInterestCalculation { get; set; }
        public DateTime? DateInterestCalaculationWasStoped { get; set; }
        public string StopedBy { get; set; }
        public decimal NewInterest { get; set; }
        public decimal NewBalance { get; set; }
        public decimal NewVAT { get; set; }
        public decimal NewPenalty { get; set; }

        public string Id { get; set; }
        [Required]
        public string LoanProductId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public bool IsThereGuarantor { get; set; }
        public bool IsThereCollateral { get; set; }
        public decimal InterestRate { get; set; }
        public decimal VatRate { get; set; }
        public string LoanId { get; set; }

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
        public decimal ShareAccountCoverageAmount { get; set; }
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
        [Required]
        public string LoanTarget { get; set; }//Employee, Government, Groups etc
        [Required]
        public string LoanCategory { get; set; }//Main OR Special Saving Facilities
        public bool IsApproved { get; set; }
        public bool IsDisbursed { get; set; }
        public string ApprovalComment { get; set; }
        public bool IsInterestWaiverApplied { get; set; }
        public decimal InterestWaiverPercentage { get; set; }
        public bool IsChargesApplied { get; set; }
        public decimal ChargesPercentage { get; set; }
        public string LoanApplicationType { get; set; }
        public int NumberOfDaysToApplyCharges { get; set; }
        public decimal OldLoanAmount { get; set; }
        public decimal OldLoanCapital { get; set; }
        public decimal OldLoanInterest { get; set; }
        public decimal OldLoanVat { get; set; }
        public decimal OldLoanPenalty { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RestructuredBalance { get; set; }
        public string InterestMethod { get; set; }
        public string InterestType { get; set; }
        public int GracePeriod { get; set; }
        public decimal GracePeriodRate { get; set; }
        public decimal InsuranceCoverageRate { get; set; }
        public decimal ShareAccountCoverageRate { get; set; }
        public bool IsGuaranteeProvided { get; set; }
        public bool IsCollateralProvided { get; set; }
        public DateTime DateOfPayment { get; set; }
        public bool IsIninitalProcessingFeePaid { get; set; }
        public bool IsPaidAllFeeUpFront { get; set; }
        public bool IsPaidFeeBeforeProcessing { get; set; }
        public bool IsPaidFeeAfterProcessing { get; set; }

        public string CustomerName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public bool RequiredDownPaymentCoverageRate { get; set; }

        public decimal PreferenceShareAccountCoverageAmount { get; set; }
        public decimal DepositAccountCoverageAmount { get; set; }
        public decimal SalaryAccountCoverageAmount { get; set; }
        public decimal TermDeposiAccountCoverageAmount { get; set; }
        public bool IsPreferenceShareAccountCoverageAmount { get; set; }
        public bool IsDepositAccountCoverageAmount { get; set; }
        public bool IsTermDeposiAccountCoverageAmount { get; set; }


        public decimal DownPaymentCoverageAmountProvided { get; set; }
        public bool ApplyInterestToThisLoan { get; set; }
        public bool ApplyFeeToThisLoan { get; set; }


        public bool? IsUpload { get; set; }

        public bool IsInterestPaidUpFront { get; set; } = true;

        public bool IsInterestRunning { get; set; } = true;

        public List<string> FeeIds { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }
        public List<LoanApplicationCollateral> Collateras { get; set; }
        public List<LoanGuarantor> Guarantors { get; set; }
        public List<LoanCommiteeValidationHistory> LoanCommiteeValidations { get; set; }
        public List<DocumentAttachedToLoan> DocumentAttachedToLoans { get; set; }
        public virtual LoanPurpose LoanPurpose { get; set; }
        public List<OTPNotification> OTPNotifications { get; set; }
        public List<LoanApplicationFee> LoanApplicationFees { get; set; }
        public IndividualCustomerProfile Customer { get; set; }



        public decimal ProcessingFee { get; set; }
       






        public List<Loan> Loans { get; set; }

        public LoanApplication()
        {
            // Initialize double properties to 0
            Amount = 0;
            InterestRate = 0;
            VatRate = 0;
            CollateralCoverageRate = 0;
            ShareAccountCoverageAmount = 0;
            SavingAccountCoverageRate = 0;
            TotalLoanRiskCoverage = 0;
            LoanDuration = 0;
            GracePeriodAfterMaturityDate = 15;
            GracePeriodBeforeFirstPayment = 1;
            AmortizationType = "Constant_Amortization";
            LoanApplicationType = "Normal";
        }
    }



    public class AddLoanApplicationCommand
    {
        public bool StopInterestCalculation { get; set; }

        [Required(ErrorMessage = "Loan target is required.")]
        [StringLength(50, ErrorMessage = "Loan target must be less than 50 characters.")]
        public string LoanTarget { get; set; } //Employee, Government, Group, Company, Individual, etc.
        [Required(ErrorMessage = "Loan category is required.")]
        [StringLength(50, ErrorMessage = "Loan category must be less than 50 characters.")]
        public string LoanCategory { get; set; } //Main_Loan OR Special_Saving_Facilities
        [Required(ErrorMessage = "Loan product category is required")]
        public string LoanProductCategoryId { get; set; } //Main_Loan OR Special_Saving_Facilities

        [Required(ErrorMessage = "Fee IDs are required.")]
        //[MinLength(1, ErrorMessage = "At least one Fee ID is required.")]
        public List<string> FeeIds { get; set; }

        [Required(ErrorMessage = "Loan product ID is required.")]
        [StringLength(50, ErrorMessage = "Loan product ID must be less than 50 characters.")]
        public string LoanProductId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "Amount must be zero or greater.")]
        [ValidateAmount]
        public decimal Amount { get; set; }

        public bool IsOverRightOldLoanInterestAndBalance { get; set; }

        [Required(ErrorMessage = "New Overide Interest amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "New balance must be zero or greater.")]
        public decimal NewBalance { get; set; }

        [Required(ErrorMessage = "New Overide Interest amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "New interest must be zero or greater.")]
        public decimal NewInterest { get; set; }

        [Required(ErrorMessage = "New Overide Vat amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "New VAT must be zero or greater.")]
        public decimal NewVAT { get; set; }
        [Required(ErrorMessage = "New Overide Penalty amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "New penalty must be zero or greater.")]
        public decimal NewPenalty { get; set; }
        public decimal VatRate { get; set; }
        public bool IsThereGuarantor { get; set; }


        public bool IsThereCollateral { get; set; }

        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100.")]
        public decimal InterestRate { get; set; }

        public bool IsInterestPaidUpFront { get; set; } = true;

        [Required(ErrorMessage = "Repayment cycle is required.")]
        [StringLength(50, ErrorMessage = "Repayment cycle must be less than 50 characters.")]
        public string RepaymentCircle { get; set; }

        [Required(ErrorMessage = "Loan type is required.")]
        [StringLength(50, ErrorMessage = "Loan type must be less than 50 characters.")]
        public string LoanType { get; set; }
        [Required(ErrorMessage = "Select loan term.")]
        public string LoanTermId { get; set; }

        [Required(ErrorMessage = "Loan duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Loan duration must be at least 1 Month.")]
        public int LoanDuration { get; set; }
        public bool IsPaidFeeBeforeProcessing { get; set; }
        public bool IsPaidFeeAfterProcessing { get; set; }

        //[Required(ErrorMessage = "First installment date is required.")]
        //[DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        //[FutureDate(ErrorMessage = "First installment date must be in the future.")]
        //public DateTime FirstInstallmentDate { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        [StringLength(50, ErrorMessage = "Customer ID must be less than 50 characters.")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Economic activity ID is required.")]
        [StringLength(50, ErrorMessage = "Economic activity ID must be less than 50 characters.")]
        public string EconomicActivityId { get; set; }

        [StringLength(50, ErrorMessage = "Amortization type must be less than 50 characters.")]
        public string AmortizationType { get; set; } = "Constant_Amortization";

        [Range(0, int.MaxValue, ErrorMessage = "Grace period before the first payment must be a non-negative number.")]
        public int GracePeriodBeforeFirstPayment { get; set; } = 1;

        [Required(ErrorMessage = "Grace period after maturity date is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Grace period after maturity date must be at least 1 Month.")]
        public int GracePeriodAfterMaturityDate { get; set; } = 15;

        public bool RequiredDownPaymentCoverageRate { get; set; }

        [Required(ErrorMessage = "Loan application type is required.")]
        [StringLength(50, ErrorMessage = "Loan application type must be less than 50 characters.")]
        public string LoanApplicationType { get; set; } = "Normal";

        [ValidateLoanId]
        public string LoanId { get; set; }

        [Range(0, 100, ErrorMessage = "Collateral coverage rate must be between 0 and 100.")]
        public decimal CollateralCoverageRate { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Share account coverage amount must be a non-negative number.")]
        public decimal ShareAccountCoverageAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Preference share account coverage amount must be a non-negative number.")]
        public decimal PreferenceShareAccountCoverageAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Deposit account coverage amount must be a non-negative number.")]
        public decimal DepositAccountCoverageAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Salary account coverage amount must be a non-negative number.")]
        public decimal SalaryAccountCoverageAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Term deposit account coverage amount must be a non-negative number.")]
        public decimal TermDeposiAccountCoverageAmount { get; set; } = 0;

        public bool IsPreferenceShareAccountCoverageAmount { get; set; }

        public bool IsDepositAccountCoverageAmount { get; set; }
        public bool IsSalryAccount { get; set; }

        public bool IsTermDeposiAccountCoverageAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Saving account coverage rate must be between 0 and 100.")]
        public decimal SavingAccountCoverageRate { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Salary account coverage rate must be between 0 and 100.")]
        public decimal SalaryAccountCoverageRate { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Guarantor saving account coverage rate must be between 0 and 100.")]
        public decimal GuaratorSavingAccountCoverageRate { get; set; } = 0;

        [Required(ErrorMessage = "Loan purpose ID is required.")]
        [StringLength(50, ErrorMessage = "Loan purpose ID must be less than 50 characters.")]
        public string LoanPurposeId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Down payment coverage amount provided must be a non-negative number.")]
        public decimal DownPaymentCoverageAmountProvided { get; set; } = 0;

        [StringLength(50, ErrorMessage = "Branch ID must be less than 50 characters.")]
        public string BranchId { get; set; }

        public bool IsInterestWaiverApplied { get; set; }

        [Range(0, 100, ErrorMessage = "Interest waiver percentage must be between 0 and 100.")]
        public decimal InterestWaiverPercentage { get; set; }

        public bool ApplyInterestToThisLoan { get; set; }

        public bool ApplyFeeToThisLoan { get; set; }

        [Range(0, 100, ErrorMessage = "Charges percentage must be between 0 and 100.")]
        public decimal ChargesPercentage { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Number of days to apply charges must be a non-negative number.")]
        public int NumberOfDaysToApplyCharges { get; set; }
        public OldLoanPayment OldLoanPayment { get; set; }
        public AddLoanApplicationCommand()
        {
            Amount = 0;
            InterestRate = 0;
            CollateralCoverageRate = 0;
            ShareAccountCoverageAmount = 0;
            SavingAccountCoverageRate = 0;
            LoanDuration = 0;
            GracePeriodAfterMaturityDate = 15;
            GracePeriodBeforeFirstPayment = 1;
            AmortizationType = "Constant_Amortization";
            LoanApplicationType = "Normal";
            LoanCategory = "Main_Loan";
            OldLoanPayment=new OldLoanPayment();
        }
    }
    public class OldLoanPayment
    {
        public decimal Amount { get; set; }
        public decimal Capital { get; set; }
        public decimal VAT { get; set; }
        public decimal Interest { get; set; }
        public decimal Penalty { get; set; }
        public string LoanId { get; set; }
    }
    // Custom Validation Attributes
    public class ValidateAmountAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var command = (AddLoanApplicationCommand)validationContext.ObjectInstance;
            var amount = (decimal)value;

            // Check if 'IsOverRightOldLoanInterestAndBalance' is not checked and the amount is zero
            if (!command.IsOverRightOldLoanInterestAndBalance && amount == 0)
            {
                return new ValidationResult("Amount cannot be zero if 'Override' is not checked.");
            }

            // Check if 'LoanApplicationType' is Refinance and 'IsOverRightOldLoanInterestAndBalance' is checked, and the amount is zero
            if (command.LoanApplicationType == "Refinancing" && command.IsOverRightOldLoanInterestAndBalance && amount == 0)
            {
                return new ValidationResult("Please Enter Amount. Amount cannot be zero if 'Loan Application Type' is Refinancing and 'Override' is checked.");
            }

            return ValidationResult.Success;
        }
    }


    public class RequiredIfOverRightOldLoanAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var command = (AddLoanApplicationCommand)validationContext.ObjectInstance;
            if (command.IsOverRightOldLoanInterestAndBalance && (decimal)value == 0)
            {
                return new ValidationResult("This field is required if 'Override' is checked.");
            }
            return ValidationResult.Success;
        }
    }
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.Now;
            }
            return false;
        }
    }
    public class ValidateLoanIdAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var command = (AddLoanApplicationCommand)validationContext.ObjectInstance;

            if (command.LoanApplicationType == "Refinancing" ||
                command.LoanApplicationType == "Rescheduling" ||
                command.LoanApplicationType == "Restructuring")
            {
                if (string.IsNullOrEmpty(command.LoanId) || command.LoanId=="0")
                {
                    return new ValidationResult("LoanId is required when LoanApplicationType is Refinancing, Rescheduling, or Restructuring.");
                }
            }

            return ValidationResult.Success;
        }
    }
    public class GetAllLoanQuery
    {
        public string QueryParam { get; set; }
        public bool IsByBranch { get; set; }
        public string BranchId { get; set; }
    }
    public class GetAllLoanByCustomerIdQuery
    {
        public string CustomerId { get; set; }
        public string QueryParameter { get; set; }
    }
    public class LoanData
    {
        public decimal DueAmount { get; set; }
        public decimal Principal { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal Tax { get; set; }
        public decimal Penalty { get; set; }
        public string Id { get; set; }
        public decimal VatRate { get; set; }
    }

    public class Loan
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public decimal Principal { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestForcasted { get; set; }
        public decimal InterestRate { get; set; }
        public decimal LastPayment { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
        public decimal DueAmount { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal LastCalculatedInterest { get; set; }
        public decimal AccrualInterestPaid { get; set; }
        public decimal TotalPrincipalPaid { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPaid { get; set; }
        public decimal FeePaid { get; set; }
        public decimal Fee { get; set; }
        public decimal Penalty { get; set; }
        public decimal PenaltyPaid { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime FirstInstallmentDate { get; set; }
        public DateTime NextInstallmentDate { get; set; }
        public DateTime LoanDate { get; set; }
        public bool IsLoanDisbursted { get; set; }
        public string DisbursmentStatus { get; set; }
        public DateTime LastInterestCalculatedDate { get; set; }
        public DateTime LastRefundDate { get; set; }
        public DateTime LastEventData { get; set; }
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
        public string LoanType { get; set; }
        public string BranchCode { get; set; }
        public string LoanId { get; set; } = "N/A";
        public string CustomerName { get; set; }
        public int LoanDuration { get; set; }
        public virtual LoanApplication LoanApplication { get; set; }
        public string LoanJourneyStatus { get; set; }
        public decimal VatRate { get; set; }
        public string LoanTarget { get; set; }//Employee, Government, Group, Company, Individual etc
        public string LoanCategory { get; set; }//Main_Loan OR Special_Saving_Facilities
        public string AccountNumber { get; set; }
        public bool IsUpload { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RestructuredBalance { get; set; }
        public OldLoanPayment OldLoanPayment { get; set; }
        public decimal DeliquentInterest { get; set; }
        public int AdvancedPaymentDays { get; set; }
        public int DeliquentDays { get; set; }
        public decimal AdvancedPaymentAmount { get; set; }
        public decimal DeliquentAmount { get; set; }
        public string LoanStructuringStatus { get; set; }
        public DateTime LoanStructuringDate { get; set; }
        public decimal OldCapital { get; set; }
        public decimal OldInterest { get; set; }
        public decimal OldVAT { get; set; }
        public decimal OldPenalty { get; set; }
        public decimal OldBalance { get; set; }
        public decimal OldDueAmount { get; set; }
        public string DeliquentStatus { get; set; }
        public bool StopInterestCalculation { get; set; } = false;
        public string StoppedBy { get; set; } = "Normal";
        public DateTime? DateInterestWastStoped { get; set; } = DateTime.MinValue;
        public DateTime? LastDeliquecyProcessedDate { get; set; } // Nullable to support unprocessed loans

        public string BranchName { get; set; }
        public int NumberOfInstallments { get; set; }
        public string RepaymentCycle { get; set; }
        public string LoanDurarion { get; set; }
        public IndividualCustomerProfile IndividualCustomer { get; set; }
        public PaginationMetadata PaginationMetadata { get; set; }
        public List<FileDownloadInfo> FileDownloadInfos { get; set; }
        public InitiateLoanDownloadCommand InitiateLoanDownloadCommand { get; set; }
        public virtual ICollection<Refund> Refunds { get; set; }
        public virtual ICollection<LoanAmortization> LoanAmortizations { get; set; }
        public List<DisburstedLoan> DisburstedLoans { get; set; }
        public List<DailyInterestCalculation> DailyInterestCalculations { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? MigrationDate { get; set; } = DateTime.MinValue;
        public bool InterestMustBePaidUpFront { get; set; }
        public decimal InterestAmountUpfront { get; set; }
        public string LoanDeliquencyConfigurationId { get; set; }
        public decimal Savings { get; set; }
        public decimal OShares { get; set; }
        public decimal PShares { get; set; }
        public decimal Deposit { get; set; }
        public decimal Salary { get; set; }
        public decimal Shortee { get; set; }
        public decimal Co_Obligor { get; set; }
        public decimal Co_OperationGurantor { get; set; }
        public decimal OtherGuaranteeFund { get; set; }
        public decimal TotalFundGuranteed { get; set; }
        public decimal PercentageOfLiquidityCoverage { get; set; }
        public decimal PercentageOfCollateralCoverage { get; set; }
        public decimal PercentageOfOverAllCoverage { get; set; }
        public LoanDeliquencyConfiguration LoanDeliquencyConfiguration { get; set; }
        public string LoanDeliquencyConfigurationName { get; set; }

    }
    public class MembersLoanDto
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public decimal Principal { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal Tax { get; set; }
        public decimal Penalty { get; set; }
        public string LoanDate { get; set; }
        public bool IsLoanDisbursed { get; set; }
        public string CustomerId { get; set; }
        public string LoanStatus { get; set; }
        public string BranchCode { get; set; }
        public string CustomerName { get; set; }
        public string MaturityDate { get; set; }
        public int NumberOfInstallments { get; set; }
        public string LoanType { get; set; }
        public decimal DueAmount { get; set; }
        public string RepaymentCycle { get; set; }
        public string LoanJourneyStatus { get; set; }
    }
    public class InitiateLoanDownloadCommand
    {
        public bool IsByBranch { get; set; }
        [Required(ErrorMessage = "Please select a branch.")]
        public string BranchId { get; set; }
        public bool IsUnpaidOnly { get; set; }
        public bool IsMigratedLoan { get; set; }
        public string QueryParameter { get; set; }
        public string FullName { get; set; }
        public string UserId { get; set; }
        public string BranchName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StrStartDate { get; set; }
        public string StrEndDate { get; set; }
        public InitiateLoanDownloadCommand()
        {
            IsUnpaidOnly = true;
        }
    }
    public class DownloadF8Filter
    {
        /// <summary>
        /// The branch ID for which the member account balances should be exported.
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// The list of account types to include in the export (e.g., Saving, Deposit, MemberShare).
        /// </summary>
        public List<string> AccountTypes { get; set; }
    }
    public class FileDownloadInfo
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string DownloadPath { get; set; }
        public string FileType { get; set; }
        public string FullPath { get; set; }
        public string Size { get; set; }
        public string UserId { get; set; }
        public string TransactionType { get; set; }
        public string UserName { get; set; }
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public DateTime DateInitiated { get; set; }

    }
    public class FileDownloadDto
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string ErrorMessage { get; set; }
    }
    /// <summary>
    /// Query to retrieve paginated, filtered, and sortable loan data for DataTable.
    /// </summary>
    public class GetLoansDataTableQuery
    {
        /// <summary>
        /// DataTable options containing pagination, sorting, and search parameters.
        /// </summary>
        public DataTableOptions DataTableOptions { get; set; }

        /// <summary>
        /// Optional filter to retrieve loans from a specific branch.
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// Optional filter to retrieve loans for a specific customer or member.
        /// </summary>
        public string MemberId { get; set; }

        /// <summary>
        /// Optional loan status filter. 
        /// Possible values: "Open", "Closed", "Refinanced", "Restructured", "Rescheduled".
        /// Use "all" to include all statuses.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Optional start date to filter loans based on the loan creation date.
        /// Only loans created on or after this date will be included.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date to filter loans based on the loan creation date.
        /// Only loans created on or before this date will be included.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Optional filter to retrieve loans based on delinquency status.
        /// Possible values: "Current", "Delinquent", or "all".
        /// </summary>
        public string DeliquentStatus { get; set; }
    }
    public class GetLoanApplicationsDataTableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string MemberId { get; set; }
        public string BranchId { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected, or All

        // New fields added
        public string LoanCategory { get; set; } // Main, Special Saving Facilities, etc.
        public string LoanTarget { get; set; } // Employee, Government, Groups, etc.
        public string ApprovalStatus { get; set; } // Approved, Pending Approval, Rejected, All
    }


    public class DataTableLoan
    {
        public string DisbursementDate { get; set; }
        public string MaturityDate { get; set; }
        public decimal Principal { get; set; }

        public decimal InterestRate { get; set; }
        public decimal AccrualInterest { get; set; }
        public decimal Fee { get; set; }
        public decimal Fines { get; set; }
        public decimal Tax { get; set; }
        public decimal Penalty { get; set; }
        public decimal DueAmount { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
        public decimal LastPayment { get; set; }
        public string LoanStatus { get; set; }
        public bool IsCurrentLoan { get; set; }
        public string CustomerId { get; set; }
        public string Id { get; set; }
        public string LoanJourneyStatus { get; set; }
    }
    public class DailyInterestCalculation
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string BranchId { get; set; }
        public decimal InterestCalculated { get; set; }
        public decimal PreviouseBalance { get; set; }
        public decimal NewBalance { get; set; }
        public decimal InterestRate { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal VatRate { get; set; }
        public decimal CalculatedVat { get; set; }
        public DateTime Date { get; set; }
        public Loan Loan { get; set; }
    }
    public class DisburstedLoan
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public DateTime DisbursmentDate { get; set; }
        public string DisbursedBy { get; set; }
        public string DisbursementStatus { get; set; }
        public string Comment { get; set; }
        public Loan Loan { get; set; }
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
        public decimal Balance { get; set; }

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

