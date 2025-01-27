using CBS.FrontDesk.Data.Entity.Accounting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity.VaultManagement
{
    public class Vault
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string Diamention { get; set; }
        public decimal MaximumCapacity { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PreviouseBalance { get; set; }
        public string EnryptedBalance { get; set; }
        public bool IsActive { get; set; }
        public decimal LastOperationAmount { get; set; }
        public string LastOperation { get; set; }
        public int ClosingNote10000 { get; set; }
        public int ClosingNote5000 { get; set; }
        public int ClosingNote2000 { get; set; }
        public int ClosingNote1000 { get; set; }
        public int ClosingNote500 { get; set; }
        public int ClosingCoin500 { get; set; }
        public int ClosingCoin100 { get; set; }
        public int ClosingCoin50 { get; set; }
        public int ClosingCoin25 { get; set; }
        public int ClosingCoin10 { get; set; }
        public int ClosingCoin5 { get; set; }
        public int ClosingCoin1 { get; set; }
        public List<VaultOperation> VaultOperations { get; set; }
        public List<VaultAuthorisedPerson> VaultAuthorisedPersons { get; set; }
        public AddVaultCommand AddVaultCommand { get; set; }
        public VaultInitializationCommand VaultInitializationCommand { get; set; }
        public Vault()
        {
            AddVaultCommand=new AddVaultCommand();
            VaultOperations=new List<VaultOperation>();
            VaultAuthorisedPersons=new List<VaultAuthorisedPerson>();
            VaultInitializationCommand=new VaultInitializationCommand();
        }
    }

  public class AddVaultCommand
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Vault name is required.")]
        [StringLength(100, ErrorMessage = "Vault name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Branch ID is required.")]
        public string BranchId { get; set; }

        [Required(ErrorMessage = "Branch code is required.")]
        [StringLength(50, ErrorMessage = "Branch code cannot exceed 50 characters.")]
        public string BranchCode { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; }

        [StringLength(100, ErrorMessage = "Dimension cannot exceed 100 characters.")]
        public string Diamention { get; set; }

        [Required(ErrorMessage = "Maximum capacity is required.")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Maximum capacity must be greater than zero.")]
        public decimal MaximumCapacity { get; set; }

        [Required(ErrorMessage = "IsActive status is required.")]
        public bool IsActive { get; set; }
    }


  

public class VaultInitializationCommand
    {
        [Required(ErrorMessage = "Amount in vault is required.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "Amount in vault cannot be negative.")]
        public decimal AmountInVault { get; set; }

        [Required(ErrorMessage = "Amount in hand is required.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "Amount in hand must be greater than 0.")]
        public decimal AmountInHand { get; set; }

        [Required(ErrorMessage = "Currency notes are required.")]
        public CurrencyNotesRequest CurrencyNote { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0.")]
        [CompareTotalAmounts(nameof(AmountInVault), nameof(AmountInHand), ErrorMessage = "Total amount must equal the sum of Amount in Vault and Amount in Hand.")]
        public decimal TotalAmount { get; set; }
        [Required(ErrorMessage = "Note is required.")]
        public string Note { get; set; }
        public bool CanProceed { get; set; }

        public VaultInitializationCommand()
        {
            CurrencyNote=new CurrencyNotesRequest();
        }
    }

    // Custom Validation Attribute for TotalAmount Comparison
    public class CompareTotalAmountsAttribute : ValidationAttribute
    {
        private readonly string _vaultAmountPropertyName;
        private readonly string _handAmountPropertyName;

        public CompareTotalAmountsAttribute(string vaultAmountPropertyName, string handAmountPropertyName)
        {
            _vaultAmountPropertyName = vaultAmountPropertyName;
            _handAmountPropertyName = handAmountPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var totalAmount = (decimal?)value;
            var vaultAmount = (decimal?)validationContext.ObjectType.GetProperty(_vaultAmountPropertyName)?.GetValue(validationContext.ObjectInstance);
            var handAmount = (decimal?)validationContext.ObjectType.GetProperty(_handAmountPropertyName)?.GetValue(validationContext.ObjectInstance);

            if (totalAmount.HasValue && vaultAmount.HasValue && handAmount.HasValue)
            {
                if (totalAmount.Value == vaultAmount.Value + handAmount.Value)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage);
        }
    }

}
