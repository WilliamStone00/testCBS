using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DepositLimit DepositLimit { get; set; }=new DepositLimit();
        public TransferLimit TransferLimit { get; set; } = new TransferLimit();
        public SavingProduct SavingProduct { get; set; } = new SavingProduct();
        public WithdrawalLimit WithdrawalLimit { get; set; }=new WithdrawalLimit();
        public Teller Teller { get; set; } = new Teller();
        public SystemConfigForSaving SystemConfigForSaving { get; set; }=new SystemConfigForSaving();
        public List<DepositLimit> DepositLimits { get; set; } = new List<DepositLimit>();
        public List<TransferLimit> TransferLimits { get; set; } = new List<TransferLimit>();
        public List<SavingProduct> SavingProducts { get; set; } = new List<SavingProduct>();
        public List<WithdrawalLimit> WithdrawalLimits { get; set; } = new List<WithdrawalLimit>();
        public List<Teller> Tellers { get; set; } = new List<Teller>();
        public List<SystemConfigForSaving> SystemConfigForSavings { get; set; } = new List<SystemConfigForSaving>();
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; }= "KEY";
    }
    public class Sharing
    {
        public decimal HeadOfficeShare { get; set; }
        public decimal PartnerShare { get; set; }
        public decimal SourceBrachOfficeShare { get; set; }
        public decimal DestinationBranchOfficeShare { get; set; }
    }
    public class DepositLimit: Sharing
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public string depositType { get; set; }
        [Required]

        public decimal minAmount { get; set; }
        [Required]

        public decimal maxAmount { get; set; }
        [Required]

        public decimal depositFeeRate { get; set; }
        [Required]

        public decimal depositFeeFlat { get; set; }
        public string bankId { get; set; }
        public SavingProduct product { get; set; }
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
        public decimal RegistrationFee { get; set; } = 0m;
        public decimal ClossingFee { get; set; } = 0m;
        public decimal ReopeningFee { get; set; } = 0m;
        public string BankId { get; set; }
        public string BranchId { get; set; }
        [Required]
        public string MemberAccountActivationPolicyId { get; set; }
        public bool NotifyBeforeWithdrawal { get; set; }
        public string AccountNumberToPeformWithdrawal { get; set; }
        public DateTime? NotificationEndDate { get; set; } = DateTime.MinValue;
        public DateTime? NotificationDate { get; set; } = DateTime.MinValue;
        public string CustomerNotification { get; set; }
        public MemberAccountActivationPolicy MemberAccountActivationPolicy { get; set; }

        // Constructor
        public MemberAccountActivation()
        {
            RegistrationFee = 0m;
            ClossingFee = 0m;
            ReopeningFee = 0m;
            MemberAccountActivationPolicy = new MemberAccountActivationPolicy();
        }
    }
    public class ResetPinCode
    {
        public string Phone { get; set; }
    }
    public class MemberAccountActivationPolicy
    {
        public string Id { get; set; }
        [Required]
        public string PolicyName { get; set; }
        public decimal MinimumRegistrationFee { get; set; } = 0m;
        public decimal MaximumRegistrationFee { get; set; } = 0m;
        public bool IsActive { get; set; }
        public decimal MinimumAccountClossingFee { get; set; } = 0m;
        public decimal MaximumAccountClossingFee { get; set; } = 0m;
        public decimal MinimumReopeningFee { get; set; } = 0m;
        public decimal MaximumReopeningFee { get; set; } = 0m;
        public string BankId { get; set; }
        public string Action { get; set; }
        public string ServiceOption { get; set; }
        public ICollection<MemberAccountActivation> MemberAccountActivations { get; set; }

        // Constructor
        public MemberAccountActivationPolicy()
        {
            MinimumRegistrationFee = 0m;
            MaximumRegistrationFee = 0m;
            MinimumAccountClossingFee = 0m;
            MaximumAccountClossingFee = 0m;
            MinimumReopeningFee = 0m;
            MaximumReopeningFee = 0m;
            MemberAccountActivations=new List<MemberAccountActivation>();
        }
    }

    public class SavingProduct
    {
        public string id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string code { get; set; }
        [Required]
        public decimal minAmount { get; set; }
        [Required]
        public decimal maxAmount { get; set; }
        [Required]
        public string interestAccrualFrequency { get; set; }
        [Required]
        public string postingFrequency { get; set; }
        public bool isUsedForTellerProvisioning { get; set; }
        
        public bool isCapitalizeInterest { get; set; }
        [Required]
        public string currencyId { get; set; }
        public bool activeStatus { get; set; }
        public bool isTermProduct { get; set; }
        [Required]
        public string description { get; set; }
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
    }
    public class WithdrawalLimit : Sharing
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]

        public decimal minAmount { get; set; }
        [Required]

        public decimal maxAmount { get; set; }
        [Required]
        public string withdrawalType { get; set; }
        [Required]

        public decimal withdrawalFeeRate { get; set; }
        [Required]

        public decimal withdrawalFeeFlat { get; set; }
        public string bankId { get; set; }
        public SavingProduct product { get; set; }
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
        public bool inUseStatus { get; set; }
        public string inUsedByUserId { get; set; }
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
        public SubTellerProvissioning SubTellerProvissioning { get; set; } = new SubTellerProvissioning();
        public PrimaryTellerProvissioning PrimaryTellerProvissioning { get; set; }=new PrimaryTellerProvissioning();
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
        public CurrencyNotes currencyNotes { get; set; }=new CurrencyNotes();
    }
    public class PrimaryTellerProvissioning
    {
        public string id { get; set; }
        public string primaryTellerId { get; set; }
        public int amount { get; set; }
        public string note { get; set; }
        public string userId { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }
        public CurrencyNotes currencyNotes { get; set; }=new CurrencyNotes();
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
        public List<StringValues> interestCalculationFrequencies { get; set; }=new List<StringValues>();
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
}
