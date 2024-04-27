using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.UserManagement
{
    public class User
    {
        public Guid id { get; set; }
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
        
        public string password { get; set; }
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
        public string status { get; set; }

        public string allowedIP { get; set; }
        public List<UserAllowedIP> userAllowedIPs { get; set; } = new List<UserAllowedIP>();
        public List<UserRole> userRoles { get; set; } = new List<UserRole>();
        public string Option { get; set; }
        public int LoginAttempts { get; set; }
        public bool IsGoogleAuthenticatorEnabled { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBlocked { get; set; }
        public bool ChangePasswordOnFirstLogin { get; set; }
        public string GoogleAuthenticatorSecretKey { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsRoot { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string profilePhoto { get; set; }
        public string provider { get; set; }

        public string name { get; set; }
        public string roleName { get; set; }
        public Bank Bank { get; set; }
        public Branch Brancch { get; set; }
        public string strlastLoginDate { get; set; }
        public ChangePassword ChangePassword { get; set; } = new ChangePassword();
        public ResetPassword ResetPassword { get; set; } = new ResetPassword();
        public List<UserClaim> userClaims { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public string ImageVirtualPath { get; set; }
        public User()
        {
            isActive = true;
            ImageVirtualPath = "~/Appfiles/Images/p.jpg";
        }
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
    public class ResetPassword
    {
        
        public string userName { get; set; }
        public string password { get; set; }
       
    }
}
