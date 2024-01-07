using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.UserManagement
{
    public class User
    {
        [Required]
        [Display(Name = "User Name")]
        public string userName { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string email { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string firstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string lastName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("password", ErrorMessage = "The password and confirmation password do not match.")]
        public string confirmPassword { get; set; }
        [Display(Name = "Phone number")]
        public string phoneNumber { get; set; }
        [Display(Name = "Active status")]
        public bool isActive { get; set; }
        [Display(Name = "Address")]
        public string address { get; set; }
        public Guid roleID { get; set; }
        [Required]
        [Display(Name = "Bank")]
        public string BankID { get; set; }
        [Required]
        [Display(Name = "Branch")]
        public string BranchID { get; set; }
        public string allowedIP { get; set; }
        public List<UserAllowedIP> userAllowedIPs { get; set; }=new List<UserAllowedIP>();
        public List<UserRole> userRoles { get; set; }=new List<UserRole>();
        public string Option { get; set; }
    }



    public class ChangePassword
    {
        [Required]
        [Display(Name = "Enter user name")]
        public string userName { get; set; }
        [Required]
        [Display(Name = "Enter old password")]
        public string oldPassword { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("password", ErrorMessage = "The password and confirmation password do not match.")]
        public string confirmPassword { get; set; }
    }

}
