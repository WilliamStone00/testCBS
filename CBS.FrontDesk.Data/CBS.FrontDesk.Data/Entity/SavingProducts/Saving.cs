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

    public class DepositLimit
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public string depositType { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal minAmount { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal maxAmount { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal depositFeeRate { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal depositFeeFlat { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }
        public SavingProduct product { get; set; }
    }
    public class TransferLimit
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public string transferType { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal minAmount { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal maxAmount { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal transferFeeRate { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal transferFeeFlat { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public SavingProduct product { get; set; }

    }
    public class ReopenFeeParameter
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal reopenFeeFlat { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal reopenFeeRate { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public SavingProduct product { get; set; }

    }
    public class ManagementFeeParameter
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal managementFeeFlat { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal managementFeeRate { get; set; }
        [Required]
        public string managementFeeFrequency { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public SavingProduct product { get; set; }

    }
    public class EntryFeeParameter
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal entryFeeRate { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal entryFeeFlat { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public SavingProduct product { get; set; }

    }
    public class CloseFeeParameter
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal closeFeeFlat { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal closeFeeRate { get; set; }
        [Required]
        public string bankId { get; set; }
        public string branchId { get; set; }
        public SavingProduct product { get; set; }

    }

    public class SavingProduct
    {
        public string id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string code { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public decimal minAmount { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
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
        public string ChartOfAccountIdPricipalSavingAccount { get; set; }
        [Required]
        public string ChartOfAccountIdInterestSavingAccount { get; set; }
        public string ChartOfAccountIdInterestSavingExpenseAccount { get; set; }
        public string ChartOfAccountIdSavingFee { get; set; }
        public string ChartOfAccountIdWithrawalFee { get; set; }
        public string ChartOfAccountIdTransferFee { get; set; }
        public string ChartOfAccountIdManagementFee { get; set; }
        public string ChartOfAccountIdClossingFee { get; set; }
        public string ChartOfAccountIdRepoeningFee { get; set; }
        [Required]
        public string bankId { get; set; }
        public List<CloseFeeParameter> CloseFeeParameters { get; set; }
        public List<EntryFeeParameter> EntryFeeParameters { get; set; }
        public List<ManagementFeeParameter> ManagementFeeParameters { get; set; }
        public List<ReopenFeeParameter> ReopenFeeParameters { get; set; }
        public List<DepositLimit> CashDepositParameters { get; set; }
        public List<WithdrawalLimit> WithdrawalParameters { get; set; }
        public List<TransferLimit> TransferParameters { get; set; }
    }
    public class WithdrawalLimit
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]

        public decimal minAmount { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]

        public decimal maxAmount { get; set; }
        [Required]
        public string withdrawalType { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]

        public decimal withdrawalFeeRate { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]

        public decimal withdrawalFeeFlat { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
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
        [Required]
   
        public double MinAmount { get; set; }
        [Required]
        public double MaxAmount { get; set; }
        public bool inUseStatus { get; set; }
        public string inUsedByUserId { get; set; }
        public bool activeStatus { get; set; }
        public List<TransactionHistory> Transactions { get; set; }
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
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]

        public decimal initialAmount { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]

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
        //

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
