using CBS.FrontDesk.Data.UserManagement.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Menu
{
    //public class Role
    //{
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public List<RoleClaim> roleClaims { get; set; }
    //}

    public class RoleClaim
    {
        public int id { get; set; }
        public string roleId { get; set; }
        public string claimType { get; set; }
        public string claimValue { get; set; }
        public string actionId { get; set; }
        public string pageId { get; set; }
    }

    public class RolePermission
    {
        public string id { get; set; }
        public string menuMasterId { get; set; }
        public string roleId { get; set; }
        public bool create { get; set; }
        public bool read { get; set; }
        public bool delete { get; set; }
        public bool update { get; set; }
        public bool download { get; set; }
        public bool upload { get; set; }
        public Role role { get; set; }
        public string menuMaster { get; set; }
    }

    public class MenuMaster
    {
        public string Id { get; set; }
        public string MenuText { get; set; }
        public string ParentId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string MenuGroup { get; set; }
        public string IconClass { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }
        public List<UserMenu> UserMenus { get; set; }
        public List<RolePermission> RolePermissions { get; set; }
    }


    public class UserAllowedIP
    {
        public string userId { get; set; }
        public string ipAddress { get; set; }
    }

    public class UserClaim
    {
        public string userId { get; set; }
        public string claimType { get; set; }
        public string claimValue { get; set; }
        public string actionId { get; set; }
        public string pageId { get; set; }
    }

    public class UserMenu
    {
        public string id { get; set; }
        public string menuMasterId { get; set; }
        public string userID { get; set; }
        public bool create { get; set; }
        public bool read { get; set; }
        public bool delete { get; set; }
        public bool update { get; set; }
        public bool download { get; set; }
        public bool upload { get; set; }
        public UserManagement.User user { get; set; }
        public string menuMaster { get; set; }
    }


}
