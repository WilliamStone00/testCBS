using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{
    public class ToggleRemoteCloseDto
    {
        public string TellerId { get; set; }
        public bool Enable { get; set; }
    }
    //public class LinkCollectorTransitCommand
    //{
    //    public string TellerId { get; set; }

    //    public string LikedMemberReference { get; set; }

    //    public bool IsLinkedToCollectorTransit { get; set; }
    //}

    public class SavingConfiguration
    {
        public CloseFeeParameter CloseFeeParameter { get; set; } = new CloseFeeParameter();
        public ReopenFeeParameter ReopenFeeParameter { get; set; } = new ReopenFeeParameter();
        public ManagementFeeParameter ManagementFeeParameter { get; set; } = new ManagementFeeParameter();
        public EntryFeeParameter EntryFeeParameter { get; set; } = new EntryFeeParameter();
        //List<>
        //public List<ProductAccountingChart> CloseFeeParameters { get; set; } = new List<ProductAccountingChart>();
        public List<CloseFeeParameter> CloseFeeParameters { get; set; } = new List<CloseFeeParameter>();
        public List<ReopenFeeParameter> ReopenFeeParameters { get; set; } = new List<ReopenFeeParameter>();
        public List<ManagementFeeParameter> ManagementFeeParameters { get; set; } = new List<ManagementFeeParameter>();
        public List<EntryFeeParameter> EntryFeeParameters { get; set; } = new List<EntryFeeParameter>();
        public DepositLimit DepositLimit { get; set; } = new DepositLimit();
        public TransferLimit TransferLimit { get; set; } = new TransferLimit();
        public SavingProduct SavingProduct { get; set; } = new SavingProduct();
        public WithdrawalLimit WithdrawalLimit { get; set; } = new WithdrawalLimit();
        public Teller Teller { get; set; } = new Teller();
        public InitializeAccountCommand InitializeAccountCommand { get; set; } = new InitializeAccountCommand();
        public SystemConfigForSaving SystemConfigForSaving { get; set; } = new SystemConfigForSaving();
        public List<DepositLimit> DepositLimits { get; set; } = new List<DepositLimit>();
        public List<TransferLimit> TransferLimits { get; set; } = new List<TransferLimit>();
        public List<SavingProduct> SavingProducts { get; set; } = new List<SavingProduct>();
        public List<WithdrawalLimit> WithdrawalLimits { get; set; } = new List<WithdrawalLimit>();
        public List<Teller> Tellers { get; set; } = new List<Teller>();
        public List<SystemConfigForSaving> SystemConfigForSavings { get; set; } = new List<SystemConfigForSaving>();
        public List<SavingProductFee> SavingProductFees { get; set; } = new List<SavingProductFee>();
        public SavingProductFee SavingProductFee { get; set; } = new SavingProductFee();
        public MobileMoneyTellerConfigurationCommand MobileMoneyTellerConfiguration { get; set; } = new MobileMoneyTellerConfigurationCommand();
        public List<ChartofAccountInfo> chartofAccountInfos { get; set; } = new List<ChartofAccountInfo>();
        public List<AccountProduct> AccountProducts { get; set; } = new List<AccountProduct>();
        public List<ProductAccountingChart> ProductAccountingCharts { get; set; } = new List<ProductAccountingChart>();
        public List<AccountingRuleEntry> AccountingRuleEntries { get; set; } = new List<AccountingRuleEntry>();
        public bool SavingDepositProductIsChargable { get; set; }
        public bool SavingWithdrwalProductIsChargable { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
    }

    public class AccountProduct
    {
        public string Id { get; set; }
        public string OperationEvent { get; set; }
        public string AccountNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

    }

    public class ProductAccountingChart
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public List<AccountProduct> AccountingChart { get; set; } = new List<AccountProduct>();
    }
    public class Sharing
    {
        public decimal HeadOfficeShare { get; set; }
        public decimal PartnerShare { get; set; }
        public decimal SourceBrachOfficeShare { get; set; }
        public decimal DestinationBranchOfficeShare { get; set; }
    }
    public class DepositLimit : Sharing
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string DepositType { get; set; }
        public string InterDepositOperationType { get; set; }
        public bool IsConfiguredForShareing { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public string BankId { get; set; }
        public decimal CamCCULShare { get; set; }
        public decimal FluxAndPTMShare { get; set; }
        public decimal HeadOfficeShare { get; set; }
        public decimal PartnerShare { get; set; }
        public decimal SourceBrachOfficeShare { get; set; }
        public decimal DestinationBranchOfficeShare { get; set; }
        public string EventAttributForDepositFormFee { get; set; }
        public string EventAttributForDepositFee { get; set; }
        public decimal SourceBrachOfficeShareCMoney { get; set; }
        public decimal DestinationBranchOfficeShareCMoney { get; set; }
        public decimal CamCCULShareCMoney { get; set; }
        public decimal FluxAndPTMShareCMoney { get; set; }
        public decimal HeadOfficeShareCMoney { get; set; }
        public string Path { get; set; }

        public SavingProduct Product { get; set; }
    }
    public class TransferLimit : Sharing
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public string transferType { get; set; }
        [Required]

        public decimal minAmount { get; set; }
        [Required]

        public decimal maxAmount { get; set; }
        [Required]

        public decimal transferFeeRate { get; set; }
        [Required]

        public decimal transferFeeFlat { get; set; }
        public string bankId { get; set; }
        public decimal CamCCULShare { get; set; }
        public decimal FluxAndPTMShare { get; set; }
        public decimal HeadOfficeShare { get; set; }
        public decimal SourceBrachOfficeShareCMoney { get; set; }
        public decimal DestinationBranchOfficeShareCMoney { get; set; }
        public decimal CamCCULShareCMoney { get; set; }
        public decimal FluxAndPTMShareCMoney { get; set; }
        public decimal HeadOfficeShareCMoney { get; set; }
        public string Path { get; set; }

        public SavingProduct product { get; set; }

    }
    public class ReopenFeeParameter : Sharing
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public decimal reopenFeeFlat { get; set; }
        [Required]
        public decimal reopenFeeRate { get; set; }
        public string bankId { get; set; }
        public SavingProduct product { get; set; }

    }
    public class ManagementFeeParameter : Sharing
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public decimal managementFeeFlat { get; set; }
        [Required]
        public decimal managementFeeRate { get; set; }
        [Required]
        public string managementFeeFrequency { get; set; }
        public string bankId { get; set; }
        public SavingProduct product { get; set; }

    }
    public class EntryFeeParameter : Sharing
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public decimal entryFeeRate { get; set; }
        [Required]
        public decimal entryFeeFlat { get; set; }
        public string bankId { get; set; }
        public SavingProduct product { get; set; }

    }





    public class OldLoanAccountingMaping
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Loan Type Name is required.")]
        [StringLength(50, ErrorMessage = "Loan Type Name cannot exceed 50 characters.")]
        public string LoanTypeName { get; set; }

        [Required(ErrorMessage = "GL for VAT is required.")]
        [StringLength(36, ErrorMessage = "GL for VAT must not exceed 36 characters.")]
        [RegularExpression("^[a-zA-Z0-9-]+$", ErrorMessage = "GL for VAT must contain only alphanumeric characters and dashes.")]
        public string ChartOfAccountIdForVAT { get; set; }

        [Required(ErrorMessage = "GL for Interest is required.")]
        [StringLength(36, ErrorMessage = "GL for Interest must not exceed 36 characters.")]
        [RegularExpression("^[a-zA-Z0-9-]+$", ErrorMessage = "GL for Interest must contain only alphanumeric characters and dashes.")]
        public string ChartOfAccountIdForInterest { get; set; }

        [Required(ErrorMessage = "GL for Capital is required.")]
        [StringLength(36, ErrorMessage = "GL for Capital must not exceed 36 characters.")]
        [RegularExpression("^[a-zA-Z0-9-]+$", ErrorMessage = "GL for Capital must contain only alphanumeric characters and dashes.")]
        public string ChartOfAccountIdForCapital { get; set; }

        [StringLength(36, ErrorMessage = "GL for Provision More Than One Year must not exceed 36 characters.")]
        [RegularExpression("^[a-zA-Z0-9-]+$", ErrorMessage = "GL for Provision More Than One Year must contain only alphanumeric characters and dashes.")]
        public string ChartOfAccountIdForProvisionMoreThanOneYear { get; set; }

        [StringLength(36, ErrorMessage = "GL for Provision More Than Two Years must not exceed 36 characters.")]
        [RegularExpression("^[a-zA-Z0-9-]+$", ErrorMessage = "GL for Provision More Than Two Years must contain only alphanumeric characters and dashes.")]
        public string ChartOfAccountIdForProvisionMoreThanTwoYear { get; set; }

        [StringLength(36, ErrorMessage = "GL for Provision More Than Three Years must not exceed 36 characters.")]
        [RegularExpression("^[a-zA-Z0-9-]+$", ErrorMessage = "GL for Provision More Than Three Years must contain only alphanumeric characters and dashes.")]
        public string ChartOfAccountIdForProvisionMoreThanThreeYear { get; set; }

        [StringLength(36, ErrorMessage = "GL for Provision More Than Four Years must not exceed 36 characters.")]
        [RegularExpression("^[a-zA-Z0-9-]+$", ErrorMessage = "GL for Provision More Than Four Years must contain only alphanumeric characters and dashes.")]
        public string ChartOfAccountIdForProvisionMoreThanFourYear { get; set; }
    }


    public class OperationFee
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string FeeType { get; set; }//Percentage Or Range Or Flat
        public bool IsAppliesOnHoliday { get; set; }
        public decimal MaximumRateAboveMaximumRange { get; set; }
        public decimal MaximumExtraCharge { get; set; }
        public bool IsMoralPerson { get; set; }
        [Required]
        public string OperationFeeType { get; set; }//MemberShip Or Operation

        public List<FeePolicy> FeePolicies { get; set; }

    }
    public class FeePolicy
    {
        public string Id { get; set; }
        [Required]
        public string FeeId { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
        public decimal Value { get; set; }
        public decimal Charge { get; set; }
        [Required]
        public string BranchId { get; set; }
        [Required]
        public string BankId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string EventCode { get; set; }
        public bool IsCentralised { get; set; }
        public OperationFee Fee { get; set; }
        public FeePolicy()
        {
            IsCentralised = false;
            AmountFrom = 0;
            AmountTo = 0;
            Value = 0;
            Charge = 0;
        }
    }
    public class SavingProductFee
    {
        public string Id { get; set; }
        [Required]
        public string FeeId { get; set; }
        [Required]
        public string SavingProductId { get; set; }
        [Required]
        public string FeeType { get; set; }//Withdrawal, Tranfer Or Cash-in
        [Required]
        public string FeePolicyType { get; set; }//Local Or Inter_Branch
        public virtual OperationFee Fee { get; set; }
        public virtual SavingProduct SavingProduct { get; set; }
        public static List<SelectListItem> GetFeeTypeList()
        {
            var feeTypes = Enum.GetValues(typeof(FeeType)).Cast<FeeType>();

            var feeTypeList = new List<SelectListItem>();

            foreach (var feeType in feeTypes)
            {
                feeTypeList.Add(new SelectListItem
                {
                    Text = feeType.ToString(),
                    Value = feeType.ToString()
                });
            }

            return feeTypeList;
        }

    }
    // Define an enum for FeeType
    public enum FeeType
    {
        Withdrawal,
        Transfer,
        Deposit, 
        Cmoney_SWN
    }
    public class CloseFeeParameter : Sharing
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public decimal closeFeeFlat { get; set; }
        [Required]
        public decimal closeFeeRate { get; set; }
        [Required]
        public string bankId { get; set; }
        public SavingProduct product { get; set; }

    }
    public class TagPISCollectionProfileCommand
    {
        public string CustomerId { get; set; }
        public bool TagAsPISCollectionProfile { get; set; } // true = tag, false = untag
    }
    public class MemberAccountActivation
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public decimal EntranceFee { get; set; }
        public decimal ByeLawFee { get; set; }
        public decimal LoanPolicyFee { get; set; }
        public decimal BuildingContribution { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime DatePaid { get; set; } = DateTime.MaxValue;
        public decimal TotalFee { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string MemberAccountActivationPolicyId { get; set; }


        public bool NotifyBeforeWithdrawal { get; set; }


        public virtual MemberRegistrationFeePolicy MemberRegistrationFeePolicy { get; set; }

        // Constructor
        public MemberAccountActivation()
        {
            MemberRegistrationFeePolicy = new MemberRegistrationFeePolicy();
        }
    }
    public class ResetPinCode
    {
        public string Phone { get; set; }
    }
    public class MemberRegistrationFeePolicy
    {
        public string Id { get; set; }
        [Required]
        public string PolicyName { get; set; }
        public decimal MinimumEntranceFee { get; set; }
        public decimal MaximumEntrancenFee { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public decimal MinimumByeLawsFee { get; set; }
        public decimal MaximumByeLawsFee { get; set; }
        public decimal MinimumLoanPolicyFee { get; set; }
        public decimal MaximumLoanPolicyFee { get; set; }
        public decimal MinimumBuildingContributionFee { get; set; }
        public decimal MaximumBuildingContribution { get; set; }
        public decimal YearBuildingContributionFee { get; set; }
        [Required]

        public string LegalForm { get; set; }//Physical_Person Or Moral_Person
        public string AccountTypeForYearyDeductionOfBuildingContribution { get; set; }//Saving, Share, Deposit
        public string EventCodeEntranceFee { get; set; }
        public string EventCodeByeLawsFee { get; set; }
        public string EventCodeLoanPolicyFee { get; set; }
        public string EventCodeBuildingContributionFee { get; set; }
        public string BankId { get; set; }
        public string Action { get; set; }
        public string ServiceOption { get; set; }
        public ICollection<MemberAccountActivation> MemberAccountActivations { get; set; }

        // Constructor
        public MemberRegistrationFeePolicy()
        {
            IsActive = true;
            MemberAccountActivations = new List<MemberAccountActivation>();
        }
    }
    public sealed class UpdateSavingProductAccountingV2MappingCommand
    {
        public string Id { get; set; }

        // Toggle – if provided: when true we expect at least the principal account id
        public bool? IsAccountingV2Mapping { get; set; }

        public string AccountingV2PrincipalAmountChartOfAccountId { get; set; }
        public string AccountingV2InterestAmountChartOfAccountId { get; set; }
        public string AccountingV2TransitChartOfAccountId { get; set; }
        public string AccountingV2CivilServantsSalarySourceChartofAccountId { get; set; }
        public string AccountingV2CivilServantsSalaryDestinationPayableChartofAccountId { get; set; }
        public string AccountingV2CivilServantsSalarySourceSalaryChartofaccountId { get; set; }
        public string AccountingV2CivilServantsSalaryDestinationSalaryProductChartofAccountId { get; set; }

        public string AccountingV2PrivateInstitutionSalarySourceChartofAccountId { get; set; }
        public string AccountingV2PrivateInstitutionSalaryDestinationPayableChartofAccountId { get; set; }
        public string AccountingV2PrivateInstitutionSalarySourceSalaryChartofAccountId { get; set; }
        public string AccountingV2PrivateInstitutionDestinationSalaryProductChartofAccountId { get; set; }

        public string AccountingV2PayOutNoneMemberSalaryChartofAccountId { get; set; }
        public string AccountingV2PayOutCashTillChartofAccountId { get; set; }
        public string AccountingV2SuspenseChartOfAccountId { get; set; }
    }
    public class SavingProduct
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
        public string AccountNuber { get; set; }
        public string AccountManagementPositionId { get; set; }

        [Required]
        public decimal MinAmount { get; set; }
        [Required]
        public decimal MaxAmount { get; set; }
        [Required]
        public string InterestAccrualFrequency { get; set; }
        [Required]
        public string PostingFrequency { get; set; }
        public bool IsUsedForTellerProvisioning { get; set; }
        public bool IsWithdrawalAllowedDirectlyFromthisAccount { get; set; } = false;
        public bool IsDepositAllowedDirectlyTothisAccount { get; set; } = false;
        public decimal MinimumAccountBalancePhysicalPerson { get; set; } = 0;
        public decimal MinimumAccountBalanceMoralPerson { get; set; } = 0;
        public bool AutoAddToMember { get; set; } = false;
        public bool CanPeformTransferMobileApp { get; set; }
        public bool CanPeformCashinMobileApp { get; set; }
        public bool CanPeformCashOutMobileApp { get; set; }
        public bool ActivateSavingWithdrawalNotificationForMobileApp { get; set; }
        public bool CanPeformTransfer3PP { get; set; }
        public bool CanPeformCashin3PP { get; set; }
        public bool CanPeformCashOut3PP { get; set; }
        public bool ActivateForMobileApp { get; set; }
        public bool ActivateFor3PPApp { get; set; }
        [Required(ErrorMessage = "Product Category is required.")]
        [StringLength(50, ErrorMessage = "Product Category must not exceed 50 characters.")]
        public string ProductCategory { get; set; }
        public bool OTPControl { get; set; }
        public decimal WithdrawalFormSavingFormFeeFor3PP { get; set; }
        public string EventCodeWithdrawalFormSavingFormFeeFor3PP { get; set; }
        public string TransitChartofAccountId { get; set; }
        public string SuspenseChartOfAccountId { get; set; }
        public string MemberDeficitChartofAccountId { get; set; }
        public bool AutoVerifyRemittanceSender { get; set; }
        public bool AutoVerifyRemittanceReceiver { get; set; }
        public decimal MinimumOpeningBalanceMoralPerson { get; set; }
        public decimal MinimumOpeningBalancePhysicalPerson { get; set; }
        public bool RequiredOpeningBalanceMoralPerson { get; set; }
        public bool RequiredOpeningBalancePhysicalPerson { get; set; }
        public bool CanPayInInstallmentPhysicalPerson { get; set; }
        public bool CanPayInInstallmentMoralPerson { get; set; }
        public int DisplayOrder { get; set; }
        public bool AllowInterbranchWithdrawal { get; set; } = false;
        public bool AllowShareing { get; set; } = false;
        public bool AllowInterbranchDeposit { get; set; } = false;
        public bool AllowInterbranchTransfter { get; set; } = false;
        public bool IsCapitalizeInterest { get; set; }
        [Required]
        public string CurrencyId { get; set; }
        public bool ActiveStatus { get; set; }
        public bool IsTermProduct { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ChartOfAccountIdPricipalAccount { get; set; }
        public string ChartOfAccountIdInterestAccount { get; set; }
        public string ChartOfAccountIdInterestExpenseAccount { get; set; }
        public string ChartOfAccountIdCashInCommission { get; set; }
        public string ChartOfAccountIdCashOutCommission { get; set; }
        public string ChartOfAccountIdHeadOfficeShareCashInCommission { get; set; }
        public string ChartOfAccountIdFluxAndPTMShareCashInCommission { get; set; }
        public string ChartOfAccountIdCamCCULShareCashInCommission { get; set; }
        public string ChartOfAccountIdHeadOfficeShareCashOutCommission { get; set; }
        public string ChartOfAccountIdFluxAndPTMShareCashOutCommission { get; set; }
        public string ChartOfAccountIdCamCCULShareCashOutCommission { get; set; }
        public string ChartOfAccountIdHeadOfficeShareTransferCommission { get; set; }
        public string ChartOfAccountIdFluxAndPTMShareTransferCommission { get; set; }
        public string ChartOfAccountIdCamCCULShareTransferCommission { get; set; }
        public string ChartOfAccountIdHeadOfficeShareCMoneyTransferCommission { get; set; }
        public string ChartOfAccountIdFluxAndPTMShareCMoneyTransferCommission { get; set; }
        public string ChartOfAccountIdCamCCULShareCMoneyTransferCommission { get; set; }
        public string ChartOfAccountIdSourceCMoneyTransferCommission { get; set; }
        public string ChartOfAccountIdDestinationCMoneyTransferCommission { get; set; }
        public string CivilServantsSalarySourceChartofAccountId { get; set; }
        public string CivilServantsSalaryDestinationPayableChartofAccountId { get; set; }
        public string CivilServantsSalarySourceSalaryChartofaccountId { get; set; }
        public string CivilServantsSalaryDestinationSalaryProductChartofAccountId { get; set; }
        public string PrivateInstitutionSalarySourceChartofAccountId { get; set; }
        public string PrivateInstitutionSalaryDestinationPayableChartofAccountId { get; set; }
        public string PrivateInstitutionSalarySourceSalaryChartofAccountId { get; set; }
        public string PrivateInstitutionDestinationSalaryProductChartofAccountId { get; set; }

        public string ChartOfAccountIdLiassonAccount { get; set; }
        [Required]
        public string ChartOfAccountIdSavingFee { get; set; }
        [Required]
        public string ChartOfAccountIdWithrawalFee { get; set; }
        [Required]
        public string ChartOfAccountIdTransferFee { get; set; }
        [Required]
        public string EventCodePhysicalPersonWithdrawalFormFee { get; set; }
        [Required]
        public string EventCodeMoralPersonWithdrawalFormFee { get; set; }
        [Required]
        public string EventCodeAdvanceOfSalaryFormFee { get; set; }

        [Required]
        public string AccountType { get; set; }
        [Required]
        public List<CloseFeeParameter> CloseFeeParameters { get; set; }
        public List<EntryFeeParameter> EntryFeeParameters { get; set; }
        public List<ManagementFeeParameter> ManagementFeeParameters { get; set; }
        public List<ReopenFeeParameter> ReopenFeeParameters { get; set; }
        public List<DepositLimit> CashDepositParameters { get; set; }
        public List<WithdrawalLimit> WithdrawalParameters { get; set; }
        public List<TransferLimit> TransferParameters { get; set; }
        public string UpdateOption { get; set; }
        public decimal MinDailySaverSharesOpeningBalancePerson { get; set; }
        public decimal MinDailySaverSharesOpeningBalanceMoral { get; set; }

        // Toggle – if provided: when true we expect at least the principal account id
        public bool? IsAccountingV2Mapping { get; set; }

        public string AccountingV2PrincipalAmountChartOfAccountId { get; set; }
        public string AccountingV2InterestAmountChartOfAccountId { get; set; }
        public string AccountingV2TransitChartOfAccountId { get; set; }

        public string AccountingV2CivilServantsSalarySourceChartofAccountId { get; set; }
        public string AccountingV2CivilServantsSalaryDestinationPayableChartofAccountId { get; set; }
        public string AccountingV2CivilServantsSalarySourceSalaryChartofaccountId { get; set; }
        public string AccountingV2CivilServantsSalaryDestinationSalaryProductChartofAccountId { get; set; }

        public string AccountingV2PrivateInstitutionSalarySourceChartofAccountId { get; set; }
        public string AccountingV2PrivateInstitutionSalaryDestinationPayableChartofAccountId { get; set; }
        public string AccountingV2PrivateInstitutionSalarySourceSalaryChartofAccountId { get; set; }
        public  string AccountingV2PrivateInstitutionDestinationSalaryProductChartofAccountId { get; set; }

        public string AccountingV2PayOutNoneMemberSalaryChartofAccountId { get; set; }
        public string AccountingV2PayOutCashTillChartofAccountId { get; set; }
        public string AccountingV2SuspenseChartOfAccountId { get; set; }
        public SavingProduct()
        {
            AllowInterbranchTransfter = true;
            AllowInterbranchWithdrawal = true;
            AllowShareing = false;
            ActiveStatus = true;
            DisplayOrder = 1;
            IsDepositAllowedDirectlyTothisAccount = true;
            IsWithdrawalAllowedDirectlyFromthisAccount = true;
            AllowInterbranchDeposit = true;
        }
    }
    public class WithdrawalLimit : Sharing
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal PhysicalPersonWithdrawalFormFee { get; set; } = 0;
        public decimal MoralPersonWithdrawalFormFee { get; set; } = 0;
        public int NotificationPeriodInMonths { get; set; }
        public string WithdrawalType { get; set; }
        public decimal CamCCULShare { get; set; }
        public decimal FluxAndPTMShare { get; set; }
        public decimal HeadOfficeShare { get; set; }
        public decimal SourceBrachOfficeShare { get; set; }
        public decimal DestinationBranchOfficeShare { get; set; }
        public bool MustNotifyOnWithdrawal { get; set; }
        public string BankId { get; set; }
        public decimal SourceBrachOfficeShareCMoney { get; set; }
        public decimal DestinationBranchOfficeShareCMoney { get; set; }
        public decimal CamCCULShareCMoney { get; set; }
        public decimal FluxAndPTMShareCMoney { get; set; }
        public decimal HeadOfficeShareCMoney { get; set; }
        public string Path { get; set; }

        public SavingProduct Product { get; set; }
    }

    public class LinkCollectorToTellerCommand
    {
        /// <summary>The Teller Id we are linking/unlinking.</summary>
        public string TellerId { get; set; }

        /// <summary>Member reference to link the teller with.</summary>
        public string MemberReference { get; set; } 

        /// <summary>User Id of the Daily Collector being linked.</summary>
        public string DailyCollectorUserId { get; set; }

        /// <summary>True = link; False = unlink.</summary>
        public bool IsLinked { get; set; }

        /// <summary>Optional comment to store when linking.</summary>
        public string LinkedComment { get; set; }

        /// <summary>Optional comment to store when unlinking.</summary>
        public string UnlinkedComment { get; set; }
    }
    public class Teller
    {

        [Required]
        public string id { get; set; }

        [Required]
        public bool isPrimary { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string name { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 10 characters.")]
        public string code { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Bank ID can only contain alphanumeric characters and hyphens.")]
        public string bankId { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Branch ID can only contain alphanumeric characters and hyphens.")]
        public string branchId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "TellerType cannot exceed 50 characters.")]
        public string TellerType { get; set; } // VirtualTeller, PhysicalTeller, DailyCollectorTeller, NoneCashTeller
        public bool AllowRemoteTellerClose { get; set; }
        public string LikedMemberReference { get; set; }
        public bool IsLinkedToCollectorTransit { get; set; }


        public bool PerformCashIn { get; set; }
        public bool ShowbalancesOnCloseOfDay { get; set; }
        public decimal BalanceInitialization { get; set; }
        public bool ShowbalancesOnOpenOfDay { get; set; }

        public bool PerformCashOut { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Event Code can only contain alphanumeric characters and hyphens.")]
        public string EventCode { get; set; }

        public bool PerformTransfer { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "OperationType cannot exceed 20 characters.")]
        public string OperationType { get; set; } // Cash, NoneCash

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinimumAmountToManage must be a positive number.")]
        public decimal MinimumAmountToManage { get; set; } = 0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MaximumAmountToManage must be a positive number.")]
        public decimal MaximumAmountToManage { get; set; } = 0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinimumDepositAmount must be a positive number.")]
        public decimal MinimumDepositAmount { get; set; } = 0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MaximumDepositAmount must be a positive number.")]
        public decimal MaximumDepositAmount { get; set; } = 0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinimumWithdrawalAmount must be a positive number.")]
        public decimal MinimumWithdrawalAmount { get; set; } = 0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MaximumWithdrawalAmount must be a positive number.")]
        public decimal MaximumWithdrawalAmount { get; set; } = 0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinimumTransferAmount must be a positive number.")]
        public decimal MinimumTransferAmount { get; set; } = 0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MaximumTransferAmount must be a positive number.")]
        public decimal MaximumTransferAmount { get; set; } = 0m;
        public string AccountNumber { get; set; }

        public Branch Branch { get; set; }

        public bool inUseStatus { get; set; }

        public bool activeStatus { get; set; }
        public string OperationEventCode { get; set; }
        public string MobileMoneyUserKeepingThePhone { get; set; }
        public string MobileMoneyFloatNumber { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Minimum alert balance must be a positive number.")]
        public decimal MobileMoneyMinimumBalanceAlertLevel { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Maximum alert balance must be a positive number.")]

        public decimal MobileMoneyMaximumBalanceAlertLevel { get; set; }
        [Required]
        public string FromAuxillaryAccountNumber_A { get; set; }
        [Required]
        public string ToBranchFloatAccountNumberAuxillary_A { get; set; }
        [Required]
        public string FromHeadOfficeAccountNumber_B { get; set; }
        [Required]
        public string ToBranchFloatAccountNumberHeadOffice_B { get; set; }
        [Required]
        public string FromBranchAccountNumber_C { get; set; }
        [Required]
        public string ToBranchFloatAccountNumberBranch_C { get; set; }
        [Required]
        public string FromBranchFloatAccountNumber_D { get; set; }
        [Required]
        public string ToHeadOfficeFloatAccountNumber_D { get; set; }
        public string PhoneNumberToRecieveAlert { get; set; }
        public string MobileMoneyAlertMessageInFrench { get; set; }
        public string MobileMoneyAlertMessageInEnglish { get; set; }
        public bool IsBlockedDueToCashCeilling { get; set; }
        public DateTime DateOfBlocked { get; set; }
        public string Blockedby { get; set; }
        public string Comment { get; set; }
        public InitializeAccountCommand InitializeAccountCommand { get; set; } = new InitializeAccountCommand();
        public List<TransactionHistory> Transactions { get; set; }
        public string MapMobileMoneyToNoneMemberMobileMoneyReference { get; set; }



        /// <summary>General free-form comment about the teller.</summary>

        /// <summary>Comment provided when the teller was linked.</summary>
        public string LinkedComment { get; set; }

        /// <summary>Comment provided when the teller was unlinked.</summary>
        public string UnlinkedComment { get; set; }

        /// <summary>User who linked this teller.</summary>
        public string LinkedBy { get; set; }

        /// <summary>User who unlinked this teller.</summary>
        public string UnlinkedBy { get; set; }

        /// <summary>Date when this teller was linked (default = MinValue).</summary>
        public DateTime LinkedDate { get; set; } = DateTime.MinValue;

        /// <summary>Date when this teller was unlinked (default = MinValue).</summary>
        public DateTime UnlinkedDate { get; set; } = DateTime.MinValue;

        // --- Navigation Properties ---
        public virtual ICollection<OtherTransaction> OtherTransactions { get; set; }
        public virtual ICollection<PrimaryTellerProvisioningHistory> PrimaryTellerProvisioningHistories { get; set; }
        public virtual ICollection<CashReplenishmentPrimaryTeller> CashReplenishmentPrimaryTellers { get; set; }
        public virtual ICollection<CashReplenishmentSubTeller> CashReplenishmentSubTellers { get; set; }

        public Teller()
        {
            MinimumAmountToManage = 0m;
            MaximumAmountToManage = 1;
            MinimumDepositAmount = 0m;
            MaximumDepositAmount = 1;
            MinimumWithdrawalAmount = 0m;
            MaximumWithdrawalAmount = 1;
            MinimumTransferAmount = 0m;
            MaximumTransferAmount = 1;
            MobileMoneyMinimumBalanceAlertLevel = 0m;
            MobileMoneyMaximumBalanceAlertLevel = 0m;
        }
    }
    //public class Tellerxxx
    //{

    //    [Required]
    //    public string id { get; set; }

    //    [Required]
    //    public bool isPrimary { get; set; }

    //    [Required]
    //    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    //    public string name { get; set; }

    //    [Required]
    //    [StringLength(10, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 10 characters.")]
    //    public string code { get; set; }

    //    [Required]
    //    [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Bank ID can only contain alphanumeric characters and hyphens.")]
    //    public string bankId { get; set; }

    //    [Required]
    //    [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Branch ID can only contain alphanumeric characters and hyphens.")]
    //    public string branchId { get; set; }

    //    [Required]
    //    [StringLength(50, ErrorMessage = "TellerType cannot exceed 50 characters.")]
    //    public string TellerType { get; set; } // VirtualTeller, PhysicalTeller, DailyCollectorTeller, NoneCashTeller
    //    public bool AllowRemoteTellerClose { get; set; }
    //    public string LikedMemberReference { get; set; }
    //    public bool IsLinkedToCollectorTransit { get; set; }


    //    public bool PerformCashIn { get; set; }
    //    public bool ShowbalancesOnCloseOfDay { get; set; }
    //    public decimal BalanceInitialization { get; set; }
    //    public bool ShowbalancesOnOpenOfDay { get; set; }

    //    public bool PerformCashOut { get; set; }

    //    [Required]
    //    [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Event Code can only contain alphanumeric characters and hyphens.")]
    //    public string EventCode { get; set; }

    //    public bool PerformTransfer { get; set; }

    //    [Required]
    //    [StringLength(20, ErrorMessage = "OperationType cannot exceed 20 characters.")]
    //    public string OperationType { get; set; } // Cash, NoneCash

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MinimumAmountToManage must be a positive number.")]
    //    public decimal MinimumAmountToManage { get; set; } = 0m;

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MaximumAmountToManage must be a positive number.")]
    //    public decimal MaximumAmountToManage { get; set; } = 0m;

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MinimumDepositAmount must be a positive number.")]
    //    public decimal MinimumDepositAmount { get; set; } = 0m;

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MaximumDepositAmount must be a positive number.")]
    //    public decimal MaximumDepositAmount { get; set; } = 0m;

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MinimumWithdrawalAmount must be a positive number.")]
    //    public decimal MinimumWithdrawalAmount { get; set; } = 0m;

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MaximumWithdrawalAmount must be a positive number.")]
    //    public decimal MaximumWithdrawalAmount { get; set; } = 0m;

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MinimumTransferAmount must be a positive number.")]
    //    public decimal MinimumTransferAmount { get; set; } = 0m;

    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "MaximumTransferAmount must be a positive number.")]
    //    public decimal MaximumTransferAmount { get; set; } = 0m;
    //    public string AccountNumber { get; set; }

    //    public Branch Branch { get; set; }

    //    public bool inUseStatus { get; set; }

    //    public bool activeStatus { get; set; }
    //    public string OperationEventCode { get; set; }
    //    public string MobileMoneyUserKeepingThePhone { get; set; }
    //    public string MobileMoneyFloatNumber { get; set; }
    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "Minimum alert balance must be a positive number.")]
    //    public decimal MobileMoneyMinimumBalanceAlertLevel { get; set; }
    //    [Required]
    //    [Range(0, double.MaxValue, ErrorMessage = "Maximum alert balance must be a positive number.")]

    //    public decimal MobileMoneyMaximumBalanceAlertLevel { get; set; }
    //    [Required]
    //    public string FromAuxillaryAccountNumber_A { get; set; }
    //    [Required]
    //    public string ToBranchFloatAccountNumberAuxillary_A { get; set; }
    //    [Required]
    //    public string FromHeadOfficeAccountNumber_B { get; set; }
    //    [Required]
    //    public string ToBranchFloatAccountNumberHeadOffice_B { get; set; }
    //    [Required]
    //    public string FromBranchAccountNumber_C { get; set; }
    //    [Required]
    //    public string ToBranchFloatAccountNumberBranch_C { get; set; }
    //    [Required]
    //    public string FromBranchFloatAccountNumber_D { get; set; }
    //    [Required]
    //    public string ToHeadOfficeFloatAccountNumber_D { get; set; }
    //    public string PhoneNumberToRecieveAlert { get; set; }
    //    public string MobileMoneyAlertMessageInFrench { get; set; }
    //    public string MobileMoneyAlertMessageInEnglish { get; set; }
    //    public bool IsBlockedDueToCashCeilling { get; set; }
    //    public DateTime DateOfBlocked { get; set; }
    //    public string Blockedby { get; set; }
    //    public string Comment { get; set; }
    //    public InitializeAccountCommand InitializeAccountCommand { get; set; } = new InitializeAccountCommand();
    //    public List<TransactionHistory> Transactions { get; set; }
    //    public string MapMobileMoneyToNoneMemberMobileMoneyReference { get; set; }



    //    /// <summary>General free-form comment about the teller.</summary>

    //    /// <summary>Comment provided when the teller was linked.</summary>
    //    public string LinkedComment { get; set; }

    //    /// <summary>Comment provided when the teller was unlinked.</summary>
    //    public string UnlinkedComment { get; set; }

    //    /// <summary>User who linked this teller.</summary>
    //    public string LinkedBy { get; set; }

    //    /// <summary>User who unlinked this teller.</summary>
    //    public string UnlinkedBy { get; set; }

    //    /// <summary>Date when this teller was linked (default = MinValue).</summary>
    //    public DateTime LinkedDate { get; set; } = DateTime.MinValue;

    //    /// <summary>Date when this teller was unlinked (default = MinValue).</summary>
    //    public DateTime UnlinkedDate { get; set; } = DateTime.MinValue;



    //    // --- Account & Status ---
    //    public bool IsPrimary { get; set; }
    //    public bool ActiveStatus { get; set; }

  

    //    // --- Navigation Properties ---
    //    public virtual ICollection<OtherTransaction> OtherTransactions { get; set; }
    //    public virtual ICollection<PrimaryTellerProvisioningHistory> PrimaryTellerProvisioningHistories { get; set; }
    //    public virtual ICollection<CashReplenishmentPrimaryTeller> CashReplenishmentPrimaryTellers { get; set; }
    //    public virtual ICollection<CashReplenishmentSubTeller> CashReplenishmentSubTellers { get; set; }


    //    public bool InUseStatus { get; set; }
    //    public Teller()
    //    {
    //        MinimumAmountToManage = 0m;
    //        MaximumAmountToManage = 1;
    //        MinimumDepositAmount = 0m;
    //        MaximumDepositAmount = 1;
    //        MinimumWithdrawalAmount = 0m;
    //        MaximumWithdrawalAmount = 1;
    //        MinimumTransferAmount = 0m;
    //        MaximumTransferAmount = 1;
    //        MobileMoneyMinimumBalanceAlertLevel = 0m;
    //        MobileMoneyMaximumBalanceAlertLevel = 0m;
    //    }
    //}
    public class InitializeAccountCommand
    {
        public string TellerId { get; set; }
        public decimal Amount { get; set; }
        public decimal GLBalance { get; set; }
        public decimal OperationBalance { get; set; }
        public bool Confirmed { get; set; }
        public string Note { get; set; }
    }

    public class MobileMoneyTellerConfigurationCommand
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Please provide a 16-digit account number for processing M/O transactions for non-members.")]
        public string MapMobileMoneyToNoneMemberMobileMoneyReference { get; set; }

        [Required(ErrorMessage = "Operation Event Code is required.")]
        public string OperationEventCode { get; set; }

        [Required(ErrorMessage = "Designated User of the Phone is required.")]
        [StringLength(100, ErrorMessage = "The Name must be a maximum of 100 characters long.")]
        public string MobileMoneyUserKeepingThePhone { get; set; }

        [Required(ErrorMessage = "Mobile Money Float Number is required.")]
        [StringLength(50, ErrorMessage = "The float number must be a maximum of 50 characters long.")]
        public string MobileMoneyFloatNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum Balance Alert Level must be a non-negative value.")]
        public decimal MobileMoneyMinimumBalanceAlertLevel { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum Balance Alert Level must be a non-negative value.")]
        public decimal MobileMoneyMaximumBalanceAlertLevel { get; set; }

        [StringLength(20, ErrorMessage = "Auxiliary Account Number A must be a maximum of 20 characters long.")]
        public string FromAuxillaryAccountNumber_A { get; set; }

        [StringLength(20, ErrorMessage = "Branch Float Account Number Auxiliary A must be a maximum of 20 characters long.")]
        public string ToBranchFloatAccountNumberAuxillary_A { get; set; }

        [StringLength(20, ErrorMessage = "Head Office Account Number B must be a maximum of 20 characters long.")]
        public string FromHeadOfficeAccountNumber_B { get; set; }

        [StringLength(20, ErrorMessage = "Branch Float Account Number Head Office B must be a maximum of 20 characters long.")]
        public string ToBranchFloatAccountNumberHeadOffice_B { get; set; }

        [StringLength(20, ErrorMessage = "Branch Account Number C must be a maximum of 20 characters long.")]
        public string FromBranchAccountNumber_C { get; set; }

        [StringLength(20, ErrorMessage = "Branch Float Account Number C must be a maximum of 20 characters long.")]
        public string ToBranchFloatAccountNumberBranch_C { get; set; }

        [StringLength(20, ErrorMessage = "Branch Float Account Number D must be a maximum of 20 characters long.")]
        public string FromBranchFloatAccountNumber_D { get; set; }

        [StringLength(20, ErrorMessage = "Head Office Float Account Number D must be a maximum of 20 characters long.")]
        public string ToHeadOfficeFloatAccountNumber_D { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumberToRecieveAlert { get; set; }

        [StringLength(500, ErrorMessage = "Alert message in French must be a maximum of 500 characters long.")]
        public string MobileMoneyAlertMessageInFrench { get; set; }

        [StringLength(500, ErrorMessage = "Alert message in English must be a maximum of 500 characters long.")]
        public string MobileMoneyAlertMessageInEnglish { get; set; }

        [StringLength(20, ErrorMessage = "Account Number must be a maximum of 20 characters long.")]
        public string AccountNumber { get; set; }
        public string Option { get; set; }
        public string BranchCode { get; set; }

    }
    public class OpeningOfTheDay
    {
        public List<CashReplenishmentPrimaryTeller> CashReplenishmentPrimaryTellers { get; set; } = new List<CashReplenishmentPrimaryTeller>();
        public CashReplenishmentPrimaryTeller CashReplenishmentPrimaryTeller { get; set; } = new CashReplenishmentPrimaryTeller();

        public SubTellerProvissioning SubTellerProvissioning { get; set; } = new SubTellerProvissioning();
        public PrimaryTellerProvissioning PrimaryTellerProvissioning { get; set; } = new PrimaryTellerProvissioning();

        public OpenningOfDayRequest OpenningOfDayRequest { get; set; } = new OpenningOfDayRequest();
        public CloseOfDayRequest CloseOfDayRequest { get; set; } = new CloseOfDayRequest();
        public TellerProvioningHistory TellerProvioningHistory { get; set; } = new TellerProvioningHistory();
        public List<TellerProvioningHistory> TellerProvioningHistories { get; set; } = new List<TellerProvioningHistory>();

        public string Option { get; set; }

    }
    public class OpenningOfDayRequest
    {
        [Required]
        public decimal InitialAmount { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string Comment { get; set; }
        [Required]
        public CurrencyNotes CurrencyNotes { get; set; }
        public DateTime? AccountingDay { get; set; }

    }

    public class CloseOfDayRequest
    {
        [Required]
        public CurrencyNotes CurrencyNotes { get; set; }
        [Required]
        public string Comment { get; set; }
        [Required]
        public decimal CashAtHand { get; set; }
        [Required]
        public string ClossedStatus { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public DateTime? AccountingDay { get; set; }

    }
    public class OpeningOfTheDayResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public double initialAmount { get; set; }
        public double minAlertBalance { get; set; }
        public double maxAlertBalance { get; set; }
        public bool isPrimary { get; set; }
        public string userId { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public DateTime modifiedDate { get; set; }
        public string modifiedBy { get; set; }
    }
    public class SubTellerProvissioning
    {
        public string id { get; set; }
        [Required]
        public string primaryTellerId { get; set; }
        [Required]

        public decimal initialAmount { get; set; }
        [Required]

        public string subTellerId { get; set; }
        [Required]
        public string userId { get; set; }
        [Required]
        public string bankId { get; set; }
        public string branchId { get; set; }
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
    }
    public class PrimaryTellerProvissioning
    {
        public string ReplenishmentId { get; set; }
        public string Note { get; set; }
        public decimal Amount { get; set; }
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
    }

    public class SystemConfigForSaving
    {
        public string id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string description { get; set; }
    }
    public class SavingConfigurationAggregates
    {
        public List<StringValues> interestCalculationFrequencies { get; set; } = new List<StringValues>();
        public List<StringValues> postingFrequencies { get; set; } = new List<StringValues>();
        public List<StringValues> managementFeeFrequencies { get; set; } = new List<StringValues>();
        public List<StringValues> depositTypes { get; set; } = new List<StringValues>();
        public List<StringValues> withdrawalTypes { get; set; } = new List<StringValues>();
        public List<StringValues> transferTypes { get; set; } = new List<StringValues>();
        public List<StringValues> transactionTypes { get; set; } = new List<StringValues>();
        public List<StringValues> withdrawalLimitTypes { get; set; } = new List<StringValues>();
        public List<StringValues> transferLimitTypes { get; set; } = new List<StringValues>();
        public List<StringValues> depositLimitTypes { get; set; } = new List<StringValues>();
        public List<StringValues> termDepositDurations { get; set; } = new List<StringValues>();
        public List<StringValues> freeQuencies { get; set; } = new List<StringValues>();
        public List<StringValues> currencies { get; set; } = new List<StringValues>();
        public List<StringValues> operationAccounts { get; set; } = new List<StringValues>();
        public List<StringValues> primaryTellerEODStatuses { get; set; } = new List<StringValues>();
        public List<StringValues> accountantEODStatuses { get; set; } = new List<StringValues>();
        public List<StringValues> Statuses { get; set; } = new List<StringValues>();
        public List<StringValues> HolidayTypes { get; set; } = new List<StringValues>();
        public List<StringValues> RecurrencePatterns { get; set; } = new List<StringValues>();
        public List<StringValues> DayOfWeeks { get; set; } = new List<StringValues>();
        public List<StringValues> RemittanceTypes { get; set; } = new List<StringValues>();

    }
    public class AddCustomerAccount
    {
        [Required]
        public string productId { get; set; }
        [Required]
        public string customerId { get; set; }

        public string bankId { get; set; }

        public string branchId { get; set; }
        public string CustomerName { get; set; }
        public bool IsRemoveAccount { get; set; }

    }
    public class WithdrawalNotification
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountId { get; set; }
        public DateTime NotificationDate { get; set; } = DateTime.MinValue;
        public DateTime DateOfIntendedWithdrawal { get; set; } = DateTime.MinValue;
        public DateTime GracePeriodDate { get; set; } = DateTime.MinValue;
        [Required]
        public decimal AmountRequired { get; set; }
        [Required]
        public string ReasonForWithdrawal { get; set; }//LoanRepayment Or Consumption, Others
        public decimal AccountBalance { get; set; }
        public decimal LoanBalance { get; set; }
        [Required]
        public string Purpose { get; set; }
        public bool IsNotificationPaid { get; set; }
        public bool IsExpired { get; set; }
        public decimal FormNotificationCharge { get; set; }
        public bool IsWithdrawalDone { get; set; }
        [Required]
        public string ApprovalStatus { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.MinValue;
        public string ApprovedByName { get; set; }
        [Required]
        public string ApprovalComment { get; set; }
        public string TransactionReference { get; set; }
        public DateTime DateWithdrawalWasDone { get; set; } = DateTime.MinValue;
        public DateTime DateFormFeeWasPaid { get; set; } = DateTime.MinValue;
        public string TellerId { get; set; }
        public decimal Total { get; set; }
        public string TellerName { get; set; }
        public string TellerCaise { get; set; }
        public string TellerId_fee { get; set; }
        public string TellerName_fee { get; set; }
        public string TellerCaise_fee { get; set; }
        public string ServiceClearName { get; set; }
        public string InitiatingBranchId { get; set; }
        public string MemberBranchId { get; set; }
        [Required]
        public int DateOfIntendedWithdrawalYear { get; set; }
        [Required]
        public int DateOfIntendedWithdrawalMonth { get; set; }
        [Required]
        public int DateOfIntendedWithdrawalDay { get; set; }
        [Required]
        public int GracePeriodDateYear { get; set; }
        [Required]
        public int GracePeriodDateMonth { get; set; }
        [Required]
        public int GracePeriodDateDay { get; set; }
        public Account Account { get; set; }
        public CustomerAccount CustomerAccount { get; set; }
        public IndividualProfile Customer { get; set; }
    }
    public class WithdrawalNotificationForm
    {
        public WithdrawalNotification WithdrawalNotification { get; set; }
        public List<WithdrawalNotification> WithdrawalNotifications { get; set; }
        public Account Account { get; set; }
        public string Option { get; set; }
        public IndividualProfile Customer { get; set; }
        public ChargesWaived ChargesWaived { get; set; }
        public List<ChargesWaived> ChargesWaiveds { get; set; }

    }
    public class CashDeskWithdrawalNotificationCommand
    {
        public string Id { get; set; }
    }
    public class ChargesWaived
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public decimal NormalCharge { get; set; }
        [Required]
        public decimal CustomCharge { get; set; }
        public DateTime DateOfWaiverRequest { get; set; }
        public DateTime DateOfWaived { get; set; }
        public bool IsWaiverDone { get; set; }
        public string TellerId { get; set; }
        public string TellerCaise { get; set; }
        public string TellerName { get; set; }
        public string WaiverInitiator { get; set; }
        public string TransactionReference { get; set; }
        [Required]
        public string Comment { get; set; }

    }
    public class AccountMigrationCommand
    {
        public string ProductId { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public List<Data> Accounts { get; set; }
    }
    public class Data
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string BranchCode { get; set; }
        public decimal OpeningBalance { get; set; }

    }

    public class MemberAccountUpload
    {
        [Required]
        public string BranchId { get; set; }

        [Required]
        public string ProductId { get; set; }


        [Required]
        public HttpPostedFileBase File { get; set; }
    }
    public class OtherTransaction
    {


        public string Id { get; set; }
        public string TransactionReference { get; set; }
        public string EnventName { get; set; }
        [Required]
        public string EventCode { get; set; }
        public string Description { get; set; }
        [Required]
        public string MemberName { get; set; }
        public string TellerId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Direction { get; set; }
        [Required]
        public string TransactionType { get; set; }//Income Or Expenses
        [Required]
        public string SourceType { get; set; }//Cash_Collection Or Member_Account
        public string Narration { get; set; }
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; } = "N/A";
        public string BranchId { get; set; }
        public Branch Branch { get; set; }
        public string BankId { get; set; }
        public string AmountInWord { get; set; }
        public string ReceiptTitle { get; set; }
        public Teller Teller { get; set; }
        public DateTime DateOfOperation { get; set; }
        public string CNI { get; set; }
        public string TelephoneNumber { get; set; }
        [Required]
        public string ExternalBranchId { get; set; }


    }

    public class TransactionReversal
    {
        public List<TransactionHistory> Transactions { get; set; }
        public ReversalRequest ReversalRequest { get; set; }
        public List<ReversalRequest> ReversalRequests { get; set; }
        public GetAllReversalRequestQuery GetAllReversalRequestQuery { get; set; }
        public AddReversalRequestCommand AddReversalRequestCommand { get; set; }
        public ApprovedReversalRequestCommand ApprovedReversalRequestCommand { get; set; }
        public ValidationReversalRequestCommand ValidationReversalRequestCommand { get; set; }
        public CashCompletionOfReversalCommand CashCompletionOfReversalCommand { get; set; }
        public string Option { get; set; }
        // Empty constructor
        public TransactionReversal()
        {
            ReversalRequest = new ReversalRequest();
            ReversalRequests = new List<ReversalRequest>();
            GetAllReversalRequestQuery = new GetAllReversalRequestQuery();
            AddReversalRequestCommand = new AddReversalRequestCommand();
            ApprovedReversalRequestCommand = new ApprovedReversalRequestCommand();
            ValidationReversalRequestCommand = new ValidationReversalRequestCommand();
            CashCompletionOfReversalCommand = new CashCompletionOfReversalCommand();
            Transactions = new List<TransactionHistory>();
        }
    }

    public class ReversalRequest
    {
        public string Id { get; set; }
        public string TransactionReference { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // "Pending", "Approved", "Rejected","Validated"
        public string InitiatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string ValidatedBy { get; set; }
        public string DebitDirection { get; set; }
        public string ApprovedComment { get; set; }
        public string ValidationComment { get; set; }
        public bool IsValidated { get; set; }
        public bool IsAppoved { get; set; }
        public bool RequestStatus { get; set; }
        public DateTime ValidationDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DateTreated { get; set; }
        public string TreatedTellerName { get; set; }
        public string TreatedTellerCode { get; set; }
        public string TreatedUserName { get; set; }
        public string BranchId { get; set; }
        public string CustomerId { get; set; }
        public string IncidentNote { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string TellerId { get; set; }
        public Branch Branch { get; set; } = new Branch();
        public Account Account { get; set; } = new Account();
        public List<TransactionHistory> Transactions { get; set; } = new List<TransactionHistory>();
        public Teller Teller { get; set; } = new Teller();
    }
    public class GetAllReversalRequestQuery
    {
        public string QueryString { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public bool IsBranch { get; set; }
        public bool IsByDate { get; set; }
        public string BranchId { get; set; }
    }
    public class AddReversalRequestCommand
    {
        public string TransactionId { get; set; }
        public string Reason { get; set; }
        public string IncidentNote { get; set; }
    }
    public class ApprovedReversalRequestCommand
    {
        public string Status { get; set; } // "Pending", "Approved", "Rejected","Validated"
        public string ApprovedComment { get; set; }
        public string Id { get; set; }
    }
    public class ValidationReversalRequestCommand
    {
        public string Status { get; set; } // "Pending", "Approved", "Rejected","Validated"
        public string ValidationComment { get; set; }
        public string Id { get; set; }
    }
    public class CashCompletionOfReversalCommand
    {
        public string Id { get; set; }
    }
    public class AddOtherTransactionCommand
    {

        public decimal Amount { get; set; }
        public string Direction { get; set; }
        public string Name { get; set; }
        public string Naration { get; set; }
        public bool IsOtherSource { get; set; }
        public string OtherSourceOfAccountId { get; set; }
        public string TransactionType { get; set; }//Income Or Expenses
        public string SourceType { get; set; }//Cash_Collection Or Member_Account
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string ExternalBranchId { get; set; }
        public List<AccountAmountCollection> AccountAmountCollections { get; set; }
        public CurrencyNotes CurrencyNotesRequest { get; set; }
        public string CNI { get; set; }
        public string TelephoneNumber { get; set; }
      
    }
    public class AccountAmountCollection
    {

        public string AccountId { get; set; }   // Generated from rule
        public decimal Amount { get; set; }
        public string Naration { get; set; }

    }
    public class AddOtherTransactionMobileMoneyCommand
    {
        public decimal Amount { get; set; }
        public string CustomerName { get; set; }
        public string SourceType { get; set; }//MobileMoneyMTN Or MobileMoneyORANGE
        public string CNI { get; set; }
        public string TelephoneNumber { get; set; }
        public string BookingDirection { get; set; }//Deposit, Withdrawal
        public string OperationType { get; set; }//MobileMoney
        public bool IsCashOperation { get; set; }
        public string MemberReference { get; set; }
        public string TellerCode { get; set; }
        public CurrencyNotes CurrencyNotesRequest { get; set; }
    }
    public class AddMembersNoneCashOperationCommand
    {
        public decimal Amount { get; set; }
        public string ChartOfAccountId { get; set; }
        public string BookingDirection { get; set; }
        public string Note { get; set; }
        public string MemberReference { get; set; }
    }
    public class AddNoneCashMobileMoneyCommand
    {
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(100, ErrorMessage = "Customer Name cannot be longer than 100 characters.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Operator Type is required.")]
        [RegularExpression("MobileMoneyMTN|MobileMoneyORANGE", ErrorMessage = "Operator Type must be either 'MobileMoneyMTN' or 'MobileMoneyORANGE'.")]
        public string SourceType { get; set; }

        [Required(ErrorMessage = "Telephone Number is required.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Telephone Number must be a valid 9-digit number.")]
        public string TelephoneNumber { get; set; }

        [Required(ErrorMessage = "Operation Type is required.")]
        [StringLength(50, ErrorMessage = "Operation Type cannot be longer than 50 characters.")]
        public string OperationType { get; set; }

        [Required(ErrorMessage = "Member Reference is required.")]
        [StringLength(10, ErrorMessage = "Member Reference cannot be longer than 10 characters.")]
        public string MemberReference { get; set; }

        [StringLength(20, ErrorMessage = "Teller Code cannot be longer than 20 characters.")]
        public string TellerCode { get; set; }

        [StringLength(20, ErrorMessage = "Receiver Account Number cannot be longer than 20 characters.")]
        public string ReceiverAccountNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Charges must be greater than or equal to zero.")]
        public decimal Charges { get; set; }
        [Required(ErrorMessage = "The request note is required.")]
        public string Note { get; set; }
    }

}
