using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
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
        public double minAmount { get; set; }
        [Required]
        public double maxAmount { get; set; }
        [Required]
        public double feePercentage { get; set; }
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
        public double minAmount { get; set; }
        [Required]
        public double maxAmount { get; set; }
        [Required]
        public double feePercentage { get; set; }
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
        public double minAmount { get; set; }
        [Required]
        public double maxAmount { get; set; }
        [Required]
        public double yearlyInterestRate { get; set; }
        [Required]
        public string interestCalculationFrequency { get; set; }
        [Required]
        public string postingFrequency { get; set; }
        public List<DepositLimit> depositLimits { get; set; } = new List<DepositLimit>();
        public List<WithdrawalLimit> withdrawalLimits { get; set; } = new List<WithdrawalLimit>();
        public List<TransferLimit> transferLimits { get; set; } = new List<TransferLimit>();
        [Required]
        public double entryFee { get; set; }
        [Required]
        public double reopeningFee { get; set; }
        [Required]
        public double closingFee { get; set; }
        [Required]
        public double managementFee { get; set; }
        [Required]
        public string managementFeeFrequency { get; set; }
        [Required]
        public string bankId { get; set; }
        public bool isTerm { get; set; }
        public double interestRate { get; set; }
        public string createdBy { get; set; }
        public object accountOwnershipType { get; set; }
        public double alertBalance { get; set; }
        public bool isOverdraftAllowed { get; set; }
        public bool canExpire { get; set; }
        public string description { get; set; }
        public string modifiedBy { get; set; }
        public object deletedBy { get; set; }
        public int objectState { get; set; }
        public bool isDeleted { get; set; }
    }
    public class WithdrawalLimit
    {
        public string id { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public double minAmount { get; set; }
        [Required]
        public double maxAmount { get; set; }
        [Required]
        public string withdrawalType { get; set; }
        [Required]
        public double feePercentage { get; set; }
        [Required]
        public double tax { get; set; }
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
        public double minAlertBalance { get; set; }
        [Required]
        public double maxAlertBalance { get; set; }
        public string BranchName { get; set; }
        public string BankName { get; set; }
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
        public int initialAmount { get; set; }
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
