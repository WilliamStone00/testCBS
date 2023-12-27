using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.UserManagement.Roles
{
   
    public class Role
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<RoleClaim> roleClaims { get; set; }
    }
}
