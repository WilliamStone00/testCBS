using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.UserManagement;

namespace CBS.FrontDesk.Data.Entity
{
   
    public class Claim
    {
        public string claimType { get; set; }
        public string claimValue { get; set; }
    }
    public class AddLogoutSessionCommand
    {
        public string UserId { get; set; }
    }
    public class UserDto
    {
        public string id { get; set; }
        public string userName { get; set; }
        public string firstName { get; set; }
        public int LoginAttempts { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public int expirationTime { get; set; }
        public DateTime expirationDate { get; set; }
        public string phoneNumber { get; set; }
        public string bearerToken { get; set; }
        public string refreshToken { get; set; }
        public string FullName { get; set; }
        public string password { get; set; }
        public bool isAuthenticated { get; set; }
        public bool isMFA { get; set; }
        public string SessionIP { get; set; }
        public string SessionUserAgent { get; set; }

        public bool IsBlocked { get; set; }
        public string SessionId { get; set; }
        public string SessionRecoveryCode { get; set; }
        public string SessionCode { get; set; }
        public bool ChangePasswordOnFirstLogin { get; set; }
        public string UserPreferedLanguage { get; set; }
        public string GoogleAuthenticatorSecretKey { get; set; }
        public bool IsVerified { get; set; }
        public string profilePhoto { get; set; }
        public string BankID { get; set; }
        public string BranchID { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public Bank Bank { get; set; } = new Bank();
        public Branch Branch { get; set; } = new Branch();
        public Organization Organization { get; set; }=new Organization();
        public List<Claim> claims { get; set; }=new List<Claim>();
        public List<UserRoleDto> Roles { get; set; }=new List<UserRoleDto>();
        public List<DatabaseMenus> Permissions { get; set; } = new List<DatabaseMenus>();
        public bool IsSuccess { get; set; }
        public List<PermissionNode> PermissionNodes { get; set; }

    }
   
    public class CustomSerializeModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string[] RoleName { get; set; }
        public string Phonenumber { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string SessionID { get; set; }
        public string SessionCode { get; set; }
        public string SessionIP { get; set; }
        public string SessionUserAgent { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
    }
    public class UserRoleDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
        public bool IsTeller { get; set; }
        public string branchId { get; set; }
    }
}
