using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class HolyDay : IValidatableObject
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "The start date is required.")]
        [DataType(DataType.Date, ErrorMessage = "The start date must be a valid date.")]
        public string DateFromStr { get; set; }
        public DateTime DateFrom { get; set; }

        [Required(ErrorMessage = "The end date is required.")]
        [DataType(DataType.Date, ErrorMessage = "The end date must be a valid date.")]
        public string DateToStr { get; set; }
        public DateTime DateTo { get; set; }

        [Required(ErrorMessage = "Branch ID is required unless the configuration is centralized.")]
        public string BranchId { get; set; }

        public bool IsCentralisedConfiguration { get; set; }

        [Required(ErrorMessage = "The event name is required.")]
        [MaxLength(100, ErrorMessage = "The event name can have a maximum of 100 characters.")]
        public string EventName { get; set; }

        [MaxLength(500, ErrorMessage = "The description can have a maximum of 500 characters.")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public bool IsNormalOperationDay { get; set; }

        public Branch Branch { get; set; }

        /// <summary>
        /// Validates that the class properties adhere to business rules.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection of validation results.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            // Rule: End date must be on or after the start date.
            if (DateTo < DateFrom)
            {
                validationResults.Add(new ValidationResult(
                    "The end date must be greater than or equal to the start date.",
                    new[] { nameof(DateTo) }));
            }

            // Rule: Branch ID is required if the configuration is not centralized.
            if (!IsCentralisedConfiguration && string.IsNullOrWhiteSpace(BranchId))
            {
                validationResults.Add(new ValidationResult(
                    "Branch must be specified unless the configuration is centralized.",
                    new[] { nameof(BranchId) }));
            }

            return validationResults;
        }
    }


}
