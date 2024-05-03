using System;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity
{




    public class MenuMaster
    {
        public int Id { get; set; }
        [Required]
        public string MenuText { get; set; }
        [Required]

        public string ParentId { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }
        [Required]

        public string MenuGroup { get; set; }
        [Required]

        public string IconClass { get; set; }
        [Required]

        public string Description { get; set; }
        public bool IsVisible { get; set; } = true;
        public string Action { get; set; }
        public string ServiceOption { get; set; }
        public MenuMaster()
        {
            IsVisible = true;
        }
    }

    public class DatabaseMenus
    {

        public int MenuMasterId { get; set; }
        public Guid UserID { get; set; }
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }
        public string FullName { get; set; }
        public string ParentName { get; set; }
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



    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);


    




}
