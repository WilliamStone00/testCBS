using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{


    public class LoanProduct
    {
        public string Id { get; set; }

        [Required]
        public string ProductCode { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string LoanInterestPeriod { get; set; } //Per Day, Per Week, Per Month, Per Year

        public string LoanInterestType { get; set; } //Rate Or Flate

        [Required]
        public double MinimumInterestRate { get; set; }

        [Required]
        public double MaximumInterestRate { get; set; }

        [Required]
        public double DefaultInterestRate { get; set; }

        public string LoanDurationPeriod { get; set; } //Days, Weeks, Months, Years

        [Required]
        public int MinimumDurationPeriod { get; set; }
        [Required]
        public string LoanTerm { get; set; }


        [Required]
        public int MaximumDurationPeriod { get; set; }

        [Required]
        public bool RequiresGuarantor { get; set; }

        [Required]
        public bool IsRequiredCollateral { get; set; }
        public bool StartGeneratingInterestAfterDisbustment { get; set; }

        public List<string> RepaymentCycles { get; set; }

        public List<string> RefundOrders { get; set; }

        [Required]
        public int DefaultDurationsPeriod { get; set; }

        [Required]
        public int MinimumNumberOfRepayment { get; set; }

        [Required]
        public int MaximumNumberOfRepayment { get; set; }

        [Required]
        public int DefaultNumberOfRepayment { get; set; }
        [Required]
        public string TaxId { get; set; }
        public decimal LoanMinimumAmount { get; set; }
        [Required]
        public string Description { get; set; }

        public decimal LoanMaximumAmount { get; set; }
        public bool BlockedSavingAccount { get; set; }
        public bool BlockedGuarantorAccount { get; set; }
        public bool BlockedSalaryAccount { get; set; }
        public decimal DefaultLoanAmount { get; set; }
        public bool IsRequiredShareAccount { get; set; }
        public bool IsRequiredSalaryccount { get; set; }
        public bool IsRequiredSavingAccount { get; set; }
        public bool IsRequresRegisteredPublicAuthority { get; set; }
        public bool IsRequredIrrivocableSalaryTransfer { get; set; }
        public bool IsInterestDeductedUpFront { get; set; }
        public bool IsFeeDeductedUpFront { get; set; }
        public decimal MinimumSavingAccountBalanceRateForTheRequestAmount { get; set; }
        public decimal MaximumSavingAccountBalanceRateForTheRequestAmount { get; set; }
        public decimal MinimumSalaryAccountBalanceRateForTheRequestAmount { get; set; }
        public decimal MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount { get; set; }
        public decimal MinimumShareAccountBalanceForTheRequestAmount { get; set; }
        public decimal MaximumShareAccountBalanceForTheRequestAmount { get; set; }
        public decimal MinimumInspectionFeeRate { get; set; }
        public decimal MaximumInspectionFeeRate { get; set; }
        public decimal DefaultInspectionFeeRate { get; set; }
        public double MinimumCollateralPercentage { get; set; }

        public bool IsInterestWaiverApplied { get; set; }
        public decimal MinimumInterestWaiver { get; set; }
        public decimal MaximumInterestWaiver { get; set; }
        public bool IsChargesApplied { get; set; }
        public decimal MinimumChargesToAppliedInPercentage { get; set; }
        public decimal MaximumChargesToAppliedPercentage { get; set; }
        public string ChargesAreAppliedToInterestOrBalance { get; set; }//Interest Or Balance
        public int ChargesStopAfterHowManyDaysFromStart { get; set; }//In days
        public decimal DefaultChargeToAppliedPercentage { get; set; }
        public decimal MinimumChargesStartDayAfterLoanDueDate { get; set; }
        public decimal MaximumChargesStartDayAfterLoanDueDate { get; set; }
        public decimal DefaulChargesStartDayAfterLoanDueDate { get; set; } = 60;
        public double MaximumCollateralPercentage { get; set; }

        public double DefaultCollateralPercentage { get; set; }

        public bool ActiveStatus { get; set; }

        public bool HasTopUp { get; set; }

        public decimal MaxTopUpLoanAmount { get; set; }

        public decimal MinTopUpLoanAmount { get; set; }

        public decimal TopUpAmount { get; set; }

        public bool IsEarlyPartialRepaymentFeeRate { get; set; }

        public double EarlyPartialRepaymentFee { get; set; }
        public decimal MinimumProcessingFeeRate { get; set; }
        public decimal MaximumProcessingFeeRate { get; set; }
        public decimal DefaultProcessingFeeRate { get; set; }

        public double EarlyTotalRepaymentFee { get; set; }

        public bool IsEarlyTotalRepaymentFeeRate { get; set; }

        public double FirstRepaymentAmount { get; set; }

        public bool CalculateInterestOnEachRepaymentOnProRatabase { get; set; }

        public string HowShoudInterestBeCahrgedInLoanSchedule { get; set; }

        public string HowShoudPrincipalBeCahrgedInLoanSchedule { get; set; }

        public string LoanScheduleDescription { get; set; }

        public string ChartOfAccountIdForPrincipalAmount { get; set; }
        public string ChartOfAccountIdForLoanTransition { get; set; }
        public string ChartOfAccountIdForAccrualInterest { get; set; }
        public string ChartOfAccountIdForPenalty { get; set; }
        public string ChartOfAccountIdForFee { get; set; }
        public string ChartOfAccountIdForTax { get; set; }
        public string ChartOfAccountIdForWriteOffPrincipal { get; set; }
        public string ChartOfAccountIdForProvisionOnPrincipal { get; set; }
        public Penalty Penalty { get; set; }
       

        public Tax Tax { get; set; }

        public List<LoanProductFee> LoanProductFeeJoins { get; set; }

        public List<Penalty> Penalties { get; set; }

        public List<LoanProductRepaymentCycle> LoanProductRepaymentCycles { get; set; } //Daily, Weekly, Biweekly, Monthly, Bimonthly, Quarterly, Every 4 Months, Semi-Annual, Every 9 Months, Yearly, Lump-Sum

        public List<LoanProductRepaymentOrder> LoanProductRepaymentOrders { get; set; }

        public List<LoanApplication> LoanApplications { get; set; }

        public List<LoanProductCollateral> LoanProductCollaterals { get; set; }

        public List<LoanProductMaturityPeriodExtension> LoanProductMaturityPeriodExtensions { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string UpdateOption { get; set; }

        public LoanProduct()
        {
            MinimumInterestRate = 0;
            MaximumInterestRate = 0;
            DefaultInterestRate = 0;
            MinimumDurationPeriod = 0;
            MaximumDurationPeriod = 0;
            DefaultDurationsPeriod = 0;
            MinimumNumberOfRepayment = 0;
            MaximumNumberOfRepayment = 0;
            DefaultNumberOfRepayment = 0;
          
            LoanMinimumAmount = 0;
            LoanMaximumAmount = 0;
            MinimumProcessingFeeRate=0;
            DefaultProcessingFeeRate = 0;
            MaximumProcessingFeeRate = 0;
            DefaultLoanAmount = 0;
            FirstRepaymentAmount = 0;
            MinimumCollateralPercentage = 0;
            MaximumCollateralPercentage = 0;
            DefaultCollateralPercentage = 0;
            MaxTopUpLoanAmount = 0;
            MinTopUpLoanAmount = 0;
            TopUpAmount = 0;
            EarlyPartialRepaymentFee = 0;
            EarlyTotalRepaymentFee = 0;
            RepaymentCycles = new List<string>();
            RefundOrders = new List<string>();
            Penalty = new Penalty();
            LoanProductFeeJoins = new List<LoanProductFee>();
            Penalties = new List<Penalty>();
            LoanProductRepaymentCycles = new List<LoanProductRepaymentCycle>();
            LoanProductRepaymentOrders = new List<LoanProductRepaymentOrder>();
            LoanApplications = new List<LoanApplication>();
            LoanProductCollaterals = new List<LoanProductCollateral>();
            LoanProductMaturityPeriodExtensions = new List<LoanProductMaturityPeriodExtension>();
        }
    }
    public class AddLoanDisbumentCommand
    {
        public string LoanId { get; set; }
        public string AccountNumber { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string Comment { get; set; }
    }
    public class LoanProductMaturityPeriodExtension
    {
        public string Id { get; set; }
        public bool ExternLoanAfterMaturityPeriod { get; set; }
        public string MaturityPeriodLoanInterestType { get; set; }
        public string CalculateInterestOn { get; set; }
        public double InterestRate { get; set; }
        public string LoanProductId { get; set; }
        public int RecurringPeriod { get; set; }
        public string RecurringPeriodType { get; set; }
        public bool IncludePenalTyFee { get; set; }
        public bool KeepLoanStatusAsPAssedMaturityEvenAfterLoanIsExterneded { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }
    }

    
    public class LoanProductRepaymentOrder
    {
        public string Id { get; set; }
        public int RepaymentOrder { get; set; }
        public string RepaymentType { get; set; }
        public string LoanProductId { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }

    }
    public class LoanProductRepaymentCycle
    {
        public string Id { get; set; }
        public string RepaymentCycle { get; set; }
        public string LoanProductId { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }

    }
    public class LoanProductPenalty
    {
        public string Id { get; set; }
        public string LoanProductId { get; set; }
        public string PenaltyId { get; set; }
        public double PenaltyAmount { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }
        public virtual Penalty Penalty { get; set; }
    }
    public class LoanProductFee
    {
        public string Id { get; set; }
        public string LoanProductId { get; set; }
        public string FeeId { get; set; }
        public double FeeAmount { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }
        public virtual Fee Fee { get; set; }
    }

    public class LoanProductConfigurationAgregates
    {
        public List<Tax> Taxes { get; set; }=new List<Tax>();
        public List<Fee> Fees { get; set; }=new List<Fee>();
        public List<Penalty> Penalties { get; set; }= new List<Penalty>();
        public List<GurantiPack> GuranteePackes { get; set; } = new List<GurantiPack>();
        public List<DocumentPack> DocumentPackes { get; set; } = new List<DocumentPack>();
        public List<FundingLine> FundingLines { get; set; } = new List<FundingLine>();
        public List<InstallmentType> InstallmentTypes { get; set; }= new List<InstallmentType>();
        public List<FeeRange> FeeRanges { get; set; } = new List<FeeRange>();
        
    }
    public class LoanProductEnumAgregates
    {
        public List<StringValues> LoanInterestMethods { get; set; } = new List<StringValues>();
        public List<StringValues> LoanInterestPeriods { get; set; } = new List<StringValues>();
        public List<StringValues> LoanInterestTypes { get; set; } = new List<StringValues>();
        public List<StringValues> LoanDurationPeriods { get; set; } = new List<StringValues>();
        public List<StringValues> CalculateInterestOn { get; set; } = new List<StringValues>();
        public List<StringValues> RepaymentCycles { get; set; } = new List<StringValues>();
        public List<StringValues> LoanStatuses { get; set; } = new List<StringValues>();
        public List<StringValues> RefundOrders { get; set; } = new List<StringValues>();
        public List<StringValues> LoanPurposes { get; set; } = new List<StringValues>();
        public List<StringValues> LoanTypes { get; set; } = new List<StringValues>();
        public List<StringValues> PaymentModes { get; set; } = new List<StringValues>();
        public List<StringValues> LoanApplicationStatus { get; set; } = new List<StringValues>();
        public List<StringValues> PenaltyTypes { get; set; } = new List<StringValues>();
        public List<StringValues> YesOrNo { get; set; } = new List<StringValues>();
        public List<StringValues> LoanCommiteeValidationStatuses { get; set; } = new List<StringValues>();
        public List<StringValues> AmortizationTypes { get; set; } = new List<StringValues>();
        public List<StringValues> DisbursmentStatuses { get; set; } = new List<StringValues>();
        public List<StringValues> LoanTerms { get; set; } = new List<StringValues>();
        public List<StringValues> LoanTargets { get; set; } = new List<StringValues>();
        public List<StringValues> LoanCategories { get; set; } = new List<StringValues>();
        public List<StringValues> LoanDeliquenciesStatus { get; set; } = new List<StringValues>();

        //LoanDeliquenciesStatus
        //ApprovalStatus
    }

}
