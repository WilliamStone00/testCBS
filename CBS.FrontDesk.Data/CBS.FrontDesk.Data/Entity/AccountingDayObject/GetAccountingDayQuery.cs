using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;


namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{

    [DateRangeValidation]
    [BranchRequiredValidation]
    public class GetAccountingDayQuery
    {
        [Required(ErrorMessage = "Date From is required.")]
        public DateTime DateFrom { get; set; }

        [Required(ErrorMessage = "Date To is required.")]
        public DateTime DateTo { get; set; }

        [Required(ErrorMessage = "Query Parameter is required.")]
        public string QueryParameter { get; set; }

        public string BranchId { get; set; }

        public bool ByBranch => QueryParameter == "ByBranch";
        public List<AccountingDay> AccountingDays { get; set; }
        public AccountingDay AccountingDay { get; set; }
    }
    public class AccountingDayActionsCommand
    {
        public string Id { get; set; }
        public string Option { get; set; }

    }
    public class DateRangeValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (GetAccountingDayQuery)validationContext.ObjectInstance;

            if (model.DateFrom > model.DateTo)
            {
                return new ValidationResult("Date From must not be greater than Date To.");
            }

            return ValidationResult.Success;
        }
    }

    public class BranchRequiredValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (GetAccountingDayQuery)validationContext.ObjectInstance;

            if (model.QueryParameter == "ByBranch" && string.IsNullOrEmpty(model.BranchId))
            {
                return new ValidationResult("Branch is required when querying by branch is selected.");
            }

            return ValidationResult.Success;
        }
    }

}
