using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountingRule
    {
        public string Id { get; set; }
        public string RuleName { get; set; }
        public string OperationEventId { get; set; }
    }
}
