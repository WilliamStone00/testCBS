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
    public class AuthRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

    }

public class GoodPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string password = value.ToString();

                if (!IsGoodPassword(password, validationContext.ObjectInstance))
                {
                    return new ValidationResult(ErrorMessage ?? "Password must contain at least 8 characters, including uppercase, lowercase, digit, and special characters, and must not contain serial numbers or the user name.");
                }
            }

            return ValidationResult.Success;
        }

        private bool IsGoodPassword(string password, object instance)
        {
            FLoginChangePassword model = instance as FLoginChangePassword;

            if (model != null && !string.IsNullOrWhiteSpace(model.UserName) && password.ToLower().Contains(model.UserName.ToLower()))
            {
                // Password contains the user name
                return false;
            }

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
            // Define a list of serial numbers to check against
            List<string> serialNumbers = new List<string> { "123", "234", "345", "456", "567", "678", "789", "890" };

            // Check if any of the serial numbers are present in the password
            foreach (string serial in serialNumbers)
            {
                if (password.Contains(serial))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class FLoginChangePassword
    {
        [Required(ErrorMessage = "Password is required")]
        [GoodPassword(ErrorMessage = "Password must contain at least 8 characters, including uppercase, lowercase, digit, and special characters, and must not contain serial numbers or the user name.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public Guid UserId { get; set; }
    }

}
