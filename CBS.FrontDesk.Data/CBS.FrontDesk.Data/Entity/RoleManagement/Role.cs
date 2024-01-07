using CBS.FrontDesk.Data.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
        public bool Download { get; set; }
        public bool Upload { get; set; }
        public bool IsAllMenus { get; set; }
        public bool IsAllCRUD { get; set; }

    }

    public class RolePermission
    {
        public Guid Id { get; set; }
        public int MenuMasterId { get; set; }
        public Guid RoleId { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
        public bool Download { get; set; }
        public bool Upload { get; set; }
        public Role Role { get; set; }
        public MenuMaster MenuMaster { get; set; }

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
        public string roleName { get; set; }
        public List<Permission> Permissions { get; set; }
        public List<PermissionRequest> rolePermissionRequests { get; set; }
    }
    public class RolePermissionRequestModel
    {
        public Guid roleID { get; set; }
        public string roleName { get; set; }
        public List<Permission> Permissions { get; set; }
    }
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsTeller { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
