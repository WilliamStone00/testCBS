using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.UserManagement
{
    public class UserRole
    {
        public Guid userId { get; set; }
        public Guid roleId { get; set; }
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
    
}
