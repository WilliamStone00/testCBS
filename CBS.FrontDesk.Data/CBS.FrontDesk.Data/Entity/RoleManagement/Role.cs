using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity
{

    public class PermissionMenuLoader
    {
        public int MenuMasterId { get; set; }
        public string MenuText { get; set; }
        public int ParentId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string MenuGroup { get; set; }
        public string Description { get; set; }
        public string ParentName { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
        public bool Download { get; set; }
        public bool Upload { get; set; }
        public bool IsAllMenus { get; set; }
        public bool IsAllCRUD { get; set; }

    }

    public class PermissionMenuLoaderDto
    {
        [Required]
        public string roleID { get; set; }
        [Required]
        public string userID { get; set; }
        public List<int> MenuMasterId { get; set; }
        public List<PermissionMenuLoader> PermissionMenuLoaders { get; set; }
        public string Action { get; set; }
        public string ServiceOption { get; set; }
    }

  
  
    public class RolePermission
    {
        public int menuMasterId { get; set; }
        public string id { get; set; }
        public string userID { get; set; }
        public string roleID { get; set; }
        public string roleName { get; set; }
        public string fullName { get; set; }
        public bool create { get; set; }
        public bool read { get; set; }
        public bool delete { get; set; }
        public bool update { get; set; }
        public bool download { get; set; }
        public bool upload { get; set; }
        public string menuText { get; set; }
        public int parentId { get; set; }
        public string controllerName { get; set; }
        public string actionName { get; set; }
        public string menuGroup { get; set; }
        public string iconClass { get; set; }
        public string description { get; set; }
        public bool isVisible { get; set; }

    }
    public class DeleteUserPermissionCommand
    {
        public DeleteUserPermissionCommand(List<string> ids)
        {
            Ids = ids;
        }

        public List<string> Ids { get; set; }
    }
    public class UserPermission
    {
        public Guid Id { get; set; }
        public int MenuMasterId { get; set; }
        public Guid UserID { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
        public bool Download { get; set; }
        public bool Upload { get; set; }
        public UserDto User { get; set; }
        public MenuMaster MenuMaster { get; set; }

    }
    public class PermissionRequest
    {
        public string Id { get; set; }
        public int MenuMasterId { get; set; }
        public string MenuText { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
        public bool Download { get; set; }
        public bool Upload { get; set; }
    }
    
    public class UserPermissionRequestCommand
    {
        public Guid userID { get; set; }
        public List<PermissionRequest> userPermissionRequests { get; set; }
    }
    public class RolePermissionRequestCommand
    {
        public Guid roleID { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();
        public List<PermissionRequest> rolePermissionRequests { get; set; } = new List<PermissionRequest>();
    }
    public class RolePermissionRequestModel
    {
        public Guid roleID { get; set; }
        public string roleName { get; set; }
        public List<Permission> Permissions { get; set; }
    }

    public class RolePermissionManagement
    {
        public List<Role> Roles { get; set; }
        public Role Role { get; set; }
        public List<RolePermission> RolePermissions { get; set; }
    }
    public class DeleteRolePermission
    {
        public List<string> Ids { get; set; }
    }

    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsTeller { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
