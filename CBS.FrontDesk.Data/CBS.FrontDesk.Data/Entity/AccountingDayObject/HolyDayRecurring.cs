using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;
    using System;

    public class HolyDayRecurring : IValidatableObject
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Branch is required if the holiday is not global.")]
        public string BranchId { get; set; }
        [Required(ErrorMessage = "Recurring holiday name is required.")]
        public string Name { get; set; }

        public bool IsGlobal { get; set; } // If true, the holiday configuration applies globally to all branches.

        [Required(ErrorMessage = "Holiday Type is required. Please specify a valid holiday type such as Weekend, Public Holiday, etc.")]
        [EnumDataType(typeof(HolidayType), ErrorMessage = "Invalid Holiday Type. Accepted values are Weekend, Recurring, Special Event, Public Holiday, Lockdown.")]
        public string HolidayType { get; set; }

        [EnumDataType(typeof(RecurrencePattern), ErrorMessage = "Invalid Recurrence Pattern. Accepted values are Daily, Weekly, Monthly, Yearly.")]
        public string RecurrencePattern { get; set; }

        [MaxLength(100, ErrorMessage = "Recurring Day can have a maximum of 100 characters.")]
        [RegularExpression(@"^([A-Za-z]+(,|\s)?)+$", ErrorMessage = "Recurring Day must include valid day names separated by commas, e.g., Monday, Tuesday.")]
        public string RecurringDay { get; set; }

        [Range(1, 12, ErrorMessage = "Month must be a value between 1 (January) and 12 (December).")]
        public int Month { get; set; }

        [Range(1, 31, ErrorMessage = "Day of the Month must be a value between 1 and 31.")]
        public int DayOfMonth { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(500, ErrorMessage = "Description can have a maximum of 500 characters.")]
        public string Description { get; set; }

        public Branch Branch { get; set; }
        public bool ExcludeRecurringDay { get; set; }
        public bool ExludeDayAndMonth { get; set; }
        /// <summary>
        /// Validates the class properties based on custom rules.
        /// </summary>
        /// <param name="validationContext">Validation context.</param>
        /// <returns>A collection of validation results.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            // Rule: Branch must be specified if the holiday is not global.
            if (!IsGlobal && string.IsNullOrWhiteSpace(BranchId))
            {
                validationResults.Add(new ValidationResult(
                    "Branch must be specified when the holiday is branch-specific.",
                    new[] { nameof(BranchId) }));
            }

            // Rule: Ensure the specified day of the month is valid for the specified month.
            if (!ExludeDayAndMonth) // Only validate Day and Month if ExcludeDayAndMonth is false.
            {
                if (Month > 0 && DayOfMonth > 0)
                {
                    try
                    {
                        var date = new DateTime(2024, Month, DayOfMonth); // Using a valid year as reference.
                    }
                    catch
                    {
                        validationResults.Add(new ValidationResult(
                            $"Day {DayOfMonth} is not valid for the selected month {Month}.",
                            new[] { nameof(DayOfMonth) }));
                    }
                }
            }

            return validationResults;
        }

    }
    public enum HolidayType
    {
        Weekend,
        Recurring,
        SpecialEvent,
        PublicHoliday,
        Lockdown
    }

    public enum RecurrencePattern
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

}
