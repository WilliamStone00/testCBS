using CBS.FrontDesk.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;

namespace CBS.FrontDesk.Service
{
    public class CustomMembershipUser : MembershipUser
    {
        #region User Properties

        public string UserID { get; set; }
        public ICollection<string> Roles { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        public bool TwoFactorAuthentification { get; set; }
        #endregion

        public CustomMembershipUser(UserDto user):base("CustomMembership",string.Empty,user.id, string.Empty, string.Empty, string.Empty,user.isMFA, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now)
        {
            string[] roles = user.Roles.Select(role => role.RoleName).ToArray();
            UserID = user.id;
            Roles = roles;
            FullName = user.firstName;
            Email = user.email;
            Phonenumber = user.phoneNumber;
            TwoFactorAuthentification = user.isMFA; 
        }
    }
}