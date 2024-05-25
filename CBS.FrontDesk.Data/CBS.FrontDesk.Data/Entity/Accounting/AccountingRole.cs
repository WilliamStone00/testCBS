using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountingRole
    {
        public string id { get; set; }
        public string ruleName { get; set; }
    }
    public class AccountingEventAttributs
    {
        public string EventCode { get; set; }
        public string AccountingRuleEntryName { get; set; }
    }
    public class GetEventAttributs
    {
        public string OpertionType { get; set; }
    }
   
}
