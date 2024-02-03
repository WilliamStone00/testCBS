using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{

    public class AccountType
    {
        public string name { get; set; }
        public string chartOfAccountIds { get; set; }
        public string operationAccountTypeId { get; set; } = "xxxxxxxx-xxxxxx-xxxx-xxxxx-xxx";
        public string operationAccountType { get; set; }

        public AccountType()
        {
        }
    }
}
