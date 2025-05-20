using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Linq;
using System.Security.Principal;

namespace CBS.FrontDesk.Service
{
    public class CustomPrincipal : IPrincipal
    {
       

        #region Identity Properties
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string[] Roles { get; set; }
        public string SessionID { get; set; }
        public string Phonenumber { get; set; }
        public string RefresherID { get; set; }
        public string Password { get; set; }
        public string SessionCode { get; set; }
        public string SessionIP { get; set; }
        public string SessionUserAgent { get; set; }
        public UserSessionDto  UserSession { get; set; }


        #endregion

        public IIdentity Identity
        {
            get; private set;
        }
        public bool IsAuthenticated { get; set; }

        public bool IsInRole(string role)
        {
            if (Roles.Any(r => role.Contains(r)))
            {
                return true;
            }
            else
            {
                return false;
            }

          
        }

        public CustomPrincipal(string username)
        {
            Identity = new GenericIdentity(username);
        }
       
    }
}