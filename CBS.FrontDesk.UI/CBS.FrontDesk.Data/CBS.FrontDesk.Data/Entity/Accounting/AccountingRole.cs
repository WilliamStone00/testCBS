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

    public class MFIChartOfAccount
    {
        public string id { get; set; }
        public string description { get; set; }
        public string rootDescription { get; set; }
        public string chartOfAccountId { get; set; }
        public string positionNumber { get; set; }
        public object level_Management { get; set; }
        public string accountNumber { get; set; }
        public string old_AccountNumber { get; set; }
        public string new_AccountNumber { get; set; }
    }


}
