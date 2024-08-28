using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{

    using System;
    using System.ComponentModel.DataAnnotations;

    public class OpenOrCloseOfAccountingDayCommand
    {
        [Required(ErrorMessage = "The Date is required.")]
        [DataType(DataType.Date)]
        [FutureDateValidation(ErrorMessage = "The Date cannot be greater than today's date.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "At least one branch must be specified.")]
        public List<BranchListing> Branches { get; set; }

        [Required(ErrorMessage = "IsCentraliseOpening is required.")]
        public bool IsCentraliseOpening { get; set; }

        [Required(ErrorMessage = "The action (Open or Close) must be specified.")]
        [RegularExpression("Open|Close", ErrorMessage = "The action must be either 'Open' or 'Close'.")]
        public string OpenOrCloseAccountingDay { get; set; }
    }

    public class FutureDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date > DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage ?? "The Date cannot be in the future.");
                }
            }

            return ValidationResult.Success;
        }
    }





}
