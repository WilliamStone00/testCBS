

using System;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace CBS.FrontDesk.Service
{
    public class CustomRole : RoleProvider
    {


        private AuthenticationServices _userServices;

        public CustomRole()
        {
         
            _userServices = new AuthenticationServices();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param Name="username"></param>
        /// <param Name="roleName"></param>
        /// <returns></returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            var userRoles = GetRolesForUser(username);
            return userRoles.Contains(roleName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param Name="username"></param>
        /// <returns></returns>
        /// 
        //public override string[] GetRolesForUser(string username)
        //{
        //    string[] s = _roleServices.GetUserRole(username);
        //    return s;
        //}
        public override string[] GetRolesForUser(string username)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return null;
            }

            //var userRoles = new string[] { };
            //userRoles = _userServices.GetUserRole(username);
            //return userRoles;
            return null;
        }
        #region Overrides of Role Provider

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }


        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        #endregion

    }





}