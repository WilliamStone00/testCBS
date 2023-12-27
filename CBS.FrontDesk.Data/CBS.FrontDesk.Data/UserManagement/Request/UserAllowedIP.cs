using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.UserManagement
{
    public class UserAllowedIP
    {
        public Guid userId { get; set; }
        public string ipAddress { get; set; }
    }
}
