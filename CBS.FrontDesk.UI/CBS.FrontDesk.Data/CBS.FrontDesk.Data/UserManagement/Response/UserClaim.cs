using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.UserManagement
{
    public class UserClaim
    {
        public Guid userId { get; set; }
        public string claimType { get; set; }
        public string claimValue { get; set; }
        public string actionId { get; set; }
        public string pageId { get; set; }
    }
}
