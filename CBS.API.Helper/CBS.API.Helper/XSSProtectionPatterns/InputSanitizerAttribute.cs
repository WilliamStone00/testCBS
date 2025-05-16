using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Filter
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;

    public class InputSanitizerAttribute : ValidationAttribute
    {
        private readonly string _pattern = @"([';--])|(\b(SELECT|INSERT|DELETE|UPDATE|DROP|ALTER|CREATE|EXEC)\b)";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            string input = value.ToString();

            if (Regex.IsMatch(input, _pattern, RegexOptions.IgnoreCase))
            {
                return new ValidationResult("Invalid input detected.");
            }

            return ValidationResult.Success;
        }
    }

}