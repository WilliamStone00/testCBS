using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
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
        public string GoogleAuthenticatorSecretKey { get; set; }
        public string BarcodeImageUrl { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBlocked { get; set; }
        public string ResetPasswordReason { get; set; }
        public bool ChangePasswordOnFirstLogin { get; set; }
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
        public string SessionRecoveryCode { get; set; }
        public bool IsDefaultSessionRecoveryCode { get; set; }
        public string NationalIdentityCardNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string PlaceOfIssue { get; set; }
        public DateTime LastDateOfPasswordChange { get; set; }
        public string ReasonForBlockingAccount { get; set; }


        public Bank Bank { get; set; }
        public Branch Brancch { get; set; }
        public string strlastLoginDate { get; set; }
        public ChangePassword ChangePassword { get; set; } = new ChangePassword();
        public ResetPassword ResetPassword { get; set; } = new ResetPassword();
        public UserSessionDto UserSession { get; set; } = new UserSessionDto();
        public List<UserPermission> UserPermissions { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public string ImageVirtualPath { get; set; }
        public MFAActivation MFAActivation { get; set; }
        public List<PermissionMenuLoader> PermissionMenuLoaders { get; set; }=new List<PermissionMenuLoader>();

        public User()
        {
            isActive = true;
            MFAActivation = new MFAActivation();
            ImageVirtualPath = "~/Appfiles/Images/p.jpg";
        }
    }
    public class GetAllUserSessionsDataTableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }

        // Searchable session fields
        public string UserName { get; set; }
        public string SessionCode { get; set; }
        public string FullName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Role { get; set; }
        public bool IsExpired { get; set; }
        // CreatedDate filter
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public GetAllUserSessionsDataTableQuery()
        {
            DataTableOptions=new DataTableOptions();
        }
    }
    public class UserLightDto
    {

        public string Id { get; set; }
        // === Personal Information ===
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePhoto { get; set; }
        public string Address { get; set; }
        public string FullName { get; set; }

        // === Security & Login ===
        public bool IsVerified { get; set; }
        public bool IsBlocked { get; set; }
        public int LoginAttempts { get; set; }
        public bool ChangePasswordOnFirstLogin { get; set; }
        public string LoginMethod { get; set; } // Password, MFA, OTP, etc.
        public bool IsGoogleAuthenticatorEnabled { get; set; }
        public string GoogleAuthenticatorSecretKey { get; set; }
        public string BarcodeImageUrl { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // === Account State ===
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUsedByAnotherUser { get; set; }
        public bool IsRoot { get; set; }
        public string SessionRecoveryCode { get; set; }

        // === Organization Info ===
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string OrganizationId { get; set; }

        // === Token Management ===
        public string RefreshToken { get; set; }
        public string UserToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public int ExpirationTime { get; set; }
        public DateTime ExpirationDate { get; set; }

        // === Email Verification ===
        public string EmailVerificationCode { get; set; }
        public DateTime EmailVerificationExpiry { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class GetAllUsersDataTableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsVerified { get; set; }

        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public GetAllUsersDataTableQuery()
        {
            DataTableOptions = new DataTableOptions();
        }
    }
    public class ChangePassword
    {
        [Required]
        [Display(Name = "Enter user Name")]
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
        [Required(ErrorMessage = "Username is required.")]
        public string userName { get; set; }

        public string password { get; set; }

        [Required(ErrorMessage = "Reset reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string ResetPasswordReason { get; set; }
    }

    public class GetUserSessionBySessionCodeQuery
    {
        public string SessionCode { get; set; }
        public string Username { get; set; }

    }
    public class UserSessionDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string SessionCode { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string FullName { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Role { get; set; }
        public string ErrorMessage { get; set; }
        public int NumberOfSessionsOpen { get; set; }
        public string SessionRecoveryCode { get; set; }
        public bool IsDefaultSessionRecoveryCode { get; set; }
        public bool IsExpired { get; set; }

        public string SessionStatus { get; set; }
        public User User { get; set; }
        public UserDto UserAuthDto { get; set; }
        public List<PermissionNode> PermissionNodes { get; set; }

    }
    public class UserSessionDataTable
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string SessionCode { get; set; }
        public string FullName { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Role { get; set; }
        public string ErrorMessage { get; set; }
        public int NumberOfSessionsOpen { get; set; }
        public string SessionRecoveryCode { get; set; }
        public bool IsDefaultSessionRecoveryCode { get; set; }
        public bool IsExpired { get; set; }

        public string SessionStatus { get; set; }
    }

    public class MFAActivation
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        public Guid Id { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The MFA Code must be 6 characters long.")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "The MFA Code must be a 6-digit number.")]
        public string Code { get; set; }

        public bool Status { get; set; }

        public string FullName { get; set; }

        public string ReturnUrl { get; set; }
    }
    public class GenerateNewRecoveryCodeCommand
    {
        public string UserId { get; set; }


    }

}
