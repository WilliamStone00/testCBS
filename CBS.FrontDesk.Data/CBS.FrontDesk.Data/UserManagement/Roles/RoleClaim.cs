using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.UserManagement.Roles
{
    public class RoleClaim
    {
        public int id { get; set; }
        public string roleId { get; set; }
        public string claimType { get; set; }
        public string claimValue { get; set; }
        public string actionId { get; set; }
        public string pageId { get; set; }
    }
}
