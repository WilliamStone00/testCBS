using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BudgetManagement
{
   
    public class OrganizationalUnit
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParentUnitId { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public string Location { get; set; }
        public string Manager { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }     
    }
}
