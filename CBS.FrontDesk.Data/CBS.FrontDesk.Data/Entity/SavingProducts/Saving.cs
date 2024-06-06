using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
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

    public class SavingConfiguration
    {
        public CloseFeeParameter CloseFeeParameter { get; set; } = new CloseFeeParameter();
        public ReopenFeeParameter ReopenFeeParameter { get; set; } = new ReopenFeeParameter();
        public ManagementFeeParameter ManagementFeeParameter { get; set; } = new ManagementFeeParameter();
        public EntryFeeParameter EntryFeeParameter { get; set; } = new EntryFeeParameter();

        public List<CloseFeeParameter> CloseFeeParameters { get; set; } = new List<CloseFeeParameter>();
        public List<ReopenFeeParameter> ReopenFeeParameters { get; set; } = new List<ReopenFeeParameter>();
        public List<ManagementFeeParameter> ManagementFeeParameters { get; set; } = new List<ManagementFeeParameter>();
        public List<EntryFeeParameter> EntryFeeParameters { get; set; } = new List<EntryFeeParameter>();
        public DepositLimit DepositLimit { get; set; } = new DepositLimit();
        public TransferLimit TransferLimit { get; set; } = new TransferLimit();
        public SavingProduct SavingProduct { get; set; } = new SavingProduct();
        public WithdrawalLimit WithdrawalLimit { get; set; } = new WithdrawalLimit();
        public Teller Teller { get; set; } = new Teller();
        public SystemConfigForSaving SystemConfigForSaving { get; set; } = new SystemConfigForSaving();
        public List<DepositLimit> DepositLimits { get; set; } = new List<DepositLimit>();
        public List<TransferLimit> TransferLimits { get; set; } = new List<TransferLimit>();
        public List<SavingProduct> SavingProducts { get; set; } = new List<SavingProduct>();
        public List<WithdrawalLimit> WithdrawalLimits { get; set; } = new List<WithdrawalLimit>();
        public List<Teller> Tellers { get; set; } = new List<Teller>();
        public List<SystemConfigForSaving> SystemConfigForSavings { get; set; } = new List<SystemConfigForSaving>();
        public List<SavingProductFee> SavingProductFees { get; set; } = new List<SavingProductFee>();
        public SavingProductFee SavingProductFee { get; set; } = new SavingProductFee();

        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
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
        public decimal HeadOfficeShare { get; set; }
        public decimal PartnerShare { get; set; }
        public decimal SourceBrachOfficeShare { get; set; }
        public decimal DestinationBranchOfficeShare { get; set; }
        public string EventAttributForDepositFormFee { get; set; }
        public string EventAttributForDepositFee { get; set; }
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

        public virtual ICollection<FeePolicy> FeePolicies { get; set; }

    }
    public class FeePolicy
    {
        public string Id { get; set; }
        [Required]
        public string FeeId { get; set; }
        [Required]
        public decimal AmountFrom { get; set; }
        [Required]
        public decimal AmountTo { get; set; }
        public decimal Value { get; set; }
        public decimal Charge { get; set; }
        public virtual OperationFee Fee { get; set; }
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
        Saving,
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
        public string MemberRegistrationFeePolicyId { get; set; }
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

    public class SavingProduct
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
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
        [Required]
        public string ChartOfAccountIdInterestAccount { get; set; }
        [Required]
        public string ChartOfAccountIdInterestExpenseAccount { get; set; }
        [Required]
        public string ChartOfAccountIdCommissionAccount { get; set; }
        [Required]
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
        public SavingProduct()
        {
            AllowInterbranchTransfter = true;
            AllowInterbranchWithdrawal = true;
            AllowShareing = false;
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
        public decimal HeadOfficeShare { get; set; }
        public decimal PartnerShare { get; set; }
        public decimal SourceBrachOfficeShare { get; set; }
        public decimal DestinationBranchOfficeShare { get; set; }
        public bool MustNotifyOnWithdrawal { get; set; }
        public string BankId { get; set; }
        public SavingProduct Product { get; set; }
    }
    public class Teller
    {
        public string id { get; set; }
        public bool isPrimary { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string code { get; set; }
        [Required]
        public string bankId { get; set; }
        [Required]
        public string branchId { get; set; }
        public decimal MinimumAmountToManage { get; set; } = 0m;
        public decimal MaximumAmountToManage { get; set; } = 0m;
        public decimal MinimumDepositAmount { get; set; } = 0m;
        public decimal MaximumDepositAmount { get; set; } = 0m;
        public decimal MinimumWithdrawalAmount { get; set; } = 0m;
        public decimal MaximumWithdrawalAmount { get; set; } = 0m;
        public decimal MinimumTransferAmount { get; set; } = 0m;
        public decimal MaximumTransferAmount { get; set; } = 0m;
        public Branch Branch { get; set; }
        public bool inUseStatus { get; set; }
        public bool activeStatus { get; set; }
        public List<TransactionHistory> Transactions { get; set; }

        // Constructor
        public Teller()
        {
            MinimumAmountToManage = 0m;
            MaximumAmountToManage = 0m;
            MinimumDepositAmount = 0m;
            MaximumDepositAmount = 0m;
            MinimumWithdrawalAmount = 0m;
            MaximumWithdrawalAmount = 0m;
            MinimumTransferAmount = 0m;
            MaximumTransferAmount = 0m;
        }
    }
    public class OpeningOfTheDay
    {
        public List<CashReplenishmentPrimaryTeller>  CashReplenishmentPrimaryTellers { get; set; } = new List<CashReplenishmentPrimaryTeller>();
        public CashReplenishmentPrimaryTeller CashReplenishmentPrimaryTeller { get; set; } = new CashReplenishmentPrimaryTeller();

        public SubTellerProvissioning SubTellerProvissioning { get; set; } = new SubTellerProvissioning();
        public PrimaryTellerProvissioning PrimaryTellerProvissioning { get; set; } = new PrimaryTellerProvissioning();

        public OpenningOfDayRequest OpenningOfDayRequest { get; set; } = new OpenningOfDayRequest();
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

        public CurrencyNotes CurrencyNotes { get; set; }

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

    }
    public class AddCustomerAccount
    {
        [Required]
        public string productId { get; set; }
        [Required]
        public string customerId { get; set; }

        public string bankId { get; set; }

        public string branchId { get; set; }
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
        public string Naration { get; set; }
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; } = "N/A";
        public string BranchId { get; set; }
        public Branch Branch { get; set; }
        public string BankId { get; set; }
        public Teller Teller { get; set; }
    }

    public class AddOtherTransactionCommand
    {
        public string EnventName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string EventCode { get; set; }
        public string Direction { get; set; }
        public string TransactionType { get; set; }//Income Or Expenses
        public string SourceType { get; set; }//Cash_Collection Or Member_Account
        public string Naration { get; set; }
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public CurrencyNotes CurrencyNotesRequest { get; set; }
    }
}
