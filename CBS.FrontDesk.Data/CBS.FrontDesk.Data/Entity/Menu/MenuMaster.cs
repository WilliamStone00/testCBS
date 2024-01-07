using System;

namespace CBS.FrontDesk.Data.Entity
{




    public class MenuMaster
    {
        public int Id { get; set; }
        public string MenuText { get; set; }
        public string ParentId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string MenuGroup { get; set; }
        public string IconClass { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }
    }

    public class DatabaseMenus
    {
        public int MenuMasterId { get; set; }
        public Guid UserID { get; set; }
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }
        public string FullName { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
        public bool Download { get; set; }
        public bool Upload { get; set; }
        public string MenuText { get; set; }
        public int ParentId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string MenuGroup { get; set; }
        public string IconClass { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }

    }
    public class Permission: DatabaseMenus
    {
        public Guid Id { get; set; }
        public bool IsAllRowSelected { get; set; }

    }








}
