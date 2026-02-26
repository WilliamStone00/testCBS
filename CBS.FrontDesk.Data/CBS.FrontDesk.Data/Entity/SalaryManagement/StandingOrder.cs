using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SalaryManagement
{
    public class StandingOrderCarrier
    {
        public string Action { get; set; }
        public List<StandingOrder> StandingOrders { get; set; }
        public AddOrUpdateStandingOrderCommand AddOrUpdateStandingOrderCommand { get; set; }
        public StandingOrder StandingOrder { get; set; }
        public IndividualProfile Customer { get; set; }

        public StandingOrderCarrier()
        {
            AddOrUpdateStandingOrderCommand=new AddOrUpdateStandingOrderCommand();
            Customer=new IndividualProfile();
            StandingOrders=new List<StandingOrder>();
            StandingOrder=new StandingOrder();
        }
    }
    public class StandingOrder
    {
        public string Id { get; set; }

        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccountType { get; set; }// Salary,Deposit,
        public string DestinationAccountType { get; set; } // Loan, Savings, OrdinaryShares, preferenceShare
        public string Purpose { get; set; }//Repay my loan, Cashin to savings, Cashin to ordinary share, Cashin to Preference shares
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutomatic { get; set; }

        public string Frequency { get; set; } // Monthly, Weekly, etc.
        public string Priority { get; set; } // High, Medium, Low
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string UserName { get; set; }
        public bool ExternalAccount { get; set; }
        public string ExternalAccountNumber { get; set; }
        public string ExternalAccountHolderName { get; set; }
        public string PersonalNote { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

    }

    public class GetStandingOrderByMemberIdQuery
    {
        /// <summary>
        /// Gets or sets the unique identifier of the CashReplenishment to be retrieved.
        /// </summary>
        public string MemberId { get; set; }
    }
    public class GetAllStandingOrdersQuery
    {
        public string BranchId { get; set; }
    }

    public class AddOrUpdateStandingOrderCommand
    {
        public string Id { get; set; }
        public string MemberName { get; set; }

        [Required(ErrorMessage = "Member reference is required.")]
        public string MemberId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Source Account Type is required.")]
        public string SourceAccountType { get; set; } // Radio buttons

        //[Required(ErrorMessage = "Destination Account Type is required.")]
        public string DestinationAccountType { get; set; } // Radio buttons

        [Required(ErrorMessage = "Purpose is required.")]
        public string Purpose { get; set; } // Radio buttons

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [EndDateAfterStartDate("StartDate", ErrorMessage = "End Date must be after Start Date.")]
        public DateTime? EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool IsAutomatic { get; set; }

        [Required(ErrorMessage = "Frequency is required.")]
        public string Frequency { get; set; } // Radio buttons

        [Required(ErrorMessage = "Priority is required.")]
        public string Priority { get; set; } // Radio buttons

        [Required]
        public bool ExternalAccount { get; set; }

        [RequiredIf(nameof(ExternalAccount), true, ErrorMessage = "External Account Number is required when External Account is enabled.")]
        public string ExternalAccountNumber { get; set; }

        [RequiredIf(nameof(ExternalAccount), true, ErrorMessage = "External Account Holder Name is required when External Account is enabled.")]
        public string ExternalAccountHolderName { get; set; }
        [Required(ErrorMessage = "Personal note is required.")]
        public string PersonalNote { get; set; }
    }
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly object _targetValue;

        public RequiredIfAttribute(string dependentProperty, object targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (property == null)
                return new ValidationResult($"Property {_dependentProperty} not found.");

            var dependentValue = property.GetValue(validationContext.ObjectInstance);

            if (dependentValue != null && dependentValue.Equals(_targetValue))
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
            }

            return ValidationResult.Success;
        }
    }

    // Custom validation for EndDate after StartDate
    public class EndDateAfterStartDateAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;

        public EndDateAfterStartDateAttribute(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            if (startDateProperty == null)
            {
                return new ValidationResult($"Unknown property: {_startDatePropertyName}");
            }

            var startDate = (DateTime?)startDateProperty.GetValue(validationContext.ObjectInstance);
            var endDate = (DateTime?)value;

            if (startDate != null && endDate != null && endDate <= startDate)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

        public sealed class StandingOrderUploadRowResult
        {
            public int RowNumber { get; set; }
            public string MemberId { get; set; }
            public string MemberName { get; set; }
            public decimal Amount { get; set; }
            public string Reason { get; set; }

            public bool Success { get; set; }
            public string Message { get; set; }
            public string StandingOrderId { get; set; } // optional
        }
        public sealed class RegisterStandingOrderUpload
        {
            public int RowNumber { get; set; }
            public string MemberId { get; set; }
            public string MemberName { get; set; }
            public decimal Amount { get; set; }
            public string Reason { get; set; }
        }
        public sealed class RegisterStandingOrderUploadMain
        {
            
            public string BranchId { get; set; }
            public List<RegisterStandingOrderUpload> Rows { get; set; }
        }

        public sealed partial class StandingOrderMemberRegistrationUploadSummary
        {
            public string FileCode { get; set; }
            public string BranchName { get; set; }
            public string UploadedBy { get; set; }

            public int TotalRows { get; set; }
            public int ValidRows { get; set; }
            public int InvalidRows { get; set; }
            public decimal TotalAmount { get; set; }
            public int CreatedStandingOrders { get; set; }
            public int FailedStandingOrders { get; set; }

            public List<StandingOrderMemberRegistrationUploadDetail> FileDetails { get; set; } =
                new List<StandingOrderMemberRegistrationUploadDetail>();
            public List<StandingOrderUploadRowResult> RowCreationResult { get; set; } =
                new List<StandingOrderUploadRowResult>();

            public List<string> ValidationErrors { get; set; } = new List<string>();
        }

        public sealed class StandingOrderMemberRegistrationUploadDetail
        {
            public int RowNumber { get; set; }

            public string MemberId { get; set; }          // Member Reference (required)
            public string MemberName { get; set; }        // Member Name (optional but recommended)
            public decimal Amount { get; set; }           // SO Amount (required)
            public string Reason { get; set; }            // "Loan", "Saving", "Loan repayment", etc.

            // Derived fields
            public bool ShouldCreateSO { get; set; }
            public string SourceAccountType { get; set; }       // Salary
            public string DestinationAccountType { get; set; }  // Loan / Savings
            public string Purpose { get; set; }                 // Repay my loan / Cash in to savings

            // If you want to create the actual SO payload directly:
            public CreateSOModelDto CreateSOModel { get; set; }
        }

        public sealed class CreateSOModelDto
        {
            public string MemberId { get; set; }
            public string MemberName { get; set; }
            public string SourceAccountType { get; set; }
            public string DestinationAccountType { get; set; }
            public decimal Amount { get; set; }
            public string Purpose { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsActive { get; set; }
            public bool IsAutomatic { get; set; }
            public string Frequency { get; set; }
            public string Priority { get; set; }
            public bool ExternalAccount { get; set; }
            public string ExternalAccountNumber { get; set; }
            public string ExternalAccountHolderName { get; set; }
            public string PersonalNote { get; set; }
        }

       
    }

}
