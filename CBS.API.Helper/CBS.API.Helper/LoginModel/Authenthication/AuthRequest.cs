using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CBS.API.Helper
{
    public class GoodPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(ErrorMessage ?? "Password is required.");
            }

            string password = value.ToString();
            FLoginChangePassword model = validationContext.ObjectInstance as FLoginChangePassword;

            if (model != null && !string.IsNullOrWhiteSpace(model.UserName) && password.IndexOf(model.UserName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new ValidationResult(ErrorMessage ?? "Password must not contain the username.");
            }

            if (!IsGoodPassword(password))
            {
                return new ValidationResult(ErrorMessage ?? "Password must contain at least 8 characters, including uppercase, lowercase, digit, and special characters. It must not contain serial numbers or the username.");
            }

            return ValidationResult.Success;
        }

        private bool IsGoodPassword(string password)
        {
            // Check if password contains at least 8 characters, including uppercase, lowercase, digit, and special characters
            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"))
            {
                return false;
            }

            // Check if password contains serial numbers
            if (ContainsSerialNumbers(password))
            {
                return false;
            }

            return true;
        }

        private bool ContainsSerialNumbers(string password)
        {
            // Generate a list of possible serial numbers
            List<string> serialPatterns = GenerateSerialPatterns();

            // Check if any of the serial patterns are present in the password
            foreach (string pattern in serialPatterns)
            {
                if (password.Contains(pattern))
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> GenerateSerialPatterns()
        {
            List<string> patterns = new List<string>();

            // Generate numeric patterns of 3 and 4 digits
            for (int i = 0; i <= 9; i++)
            {
                for (int length = 3; length <= 4; length++)
                {
                    string pattern = new string(Convert.ToChar('0' + i), length);
                    patterns.Add(pattern);
                }
            }

            // Add common ascending patterns
            for (int i = 0; i <= 6; i++)
            {
                string pattern = $"{i}{i + 1}{i + 2}";
                patterns.Add(pattern);
                pattern = $"{i}{i + 1}{i + 2}{i + 3}";
                patterns.Add(pattern);
            }

            // Add common descending patterns
            for (int i = 9; i >= 3; i--)
            {
                string pattern = $"{i}{i - 1}{i - 2}";
                patterns.Add(pattern);
                pattern = $"{i}{i - 1}{i - 2}{i - 3}";
                patterns.Add(pattern);
            }

            return patterns;
        }

    }
    //public class AuthRequest
    //{
    //    [Required(ErrorMessage = "Username is required.")]
    //    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
    //    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain alphanumeric characters and underscores.")]
    //    public string UserName { get; set; }

    //    [Required(ErrorMessage = "Password is required.")]
    //    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    //    public string Password { get; set; }
    //}
    public class AuthRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(17, MinimumLength = 3, ErrorMessage = "Username must be between 6 and 17 characters.")]
        // Updated RegularExpression to allow alphabets, digits, underscores, and dots
        [RegularExpression(@"^[a-zA-Z0-9_.@]+$", ErrorMessage = "Username can only contain alphanumeric characters, underscores, @ and dots.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }
        public GeoLocationResponse GeoLocationResponse { get; set; }
    }
    public class GeoLocationResponse
    {
        public string Ip { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Loc { get; set; }
        public string Org { get; set; }
        public string Timezone { get; set; }
        public string Readme { get; set; }
    }
    public class FLoginChangePassword
    {
        [Required(ErrorMessage = "Password is required.")]
        [GoodPassword(ErrorMessage = "Password must contain at least 8 characters, including uppercase, lowercase, digit, and special characters, and must not contain serial numbers or the username.")]
        [StringLength(15, ErrorMessage = "Password must be a maximum of 10 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [StringLength(15, ErrorMessage = "Confirm Password must be a maximum of 10 characters.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Text)]
        public string UserName { get; set; }

        [DataType(DataType.Text)]
        public string FullName { get; set; }

        public Guid UserId { get; set; }
    }

}
