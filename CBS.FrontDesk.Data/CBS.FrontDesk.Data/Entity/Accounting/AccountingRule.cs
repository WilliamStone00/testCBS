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
        public string AccountingEntryRuleId { get; set; }
        public string BookingDirection { get; set; }
    }

    public class AccountingRuleDtos
    {
        public string Id { get; set; }
        public string RuleName { get; set; }
        public string DeterminantAccount { get; set; }
        public string BookingDirection { get; set; }
        public string BalancingAccount { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AccountingRuleX
    {
        public string ruleName { get; set; }
        public string accountingEntryRuleId { get; set; }
        public string bookingDirection { get; set; }
    }

    public class AccountingRuleXRoot
    {
        public List<AccountingRuleX> accountingRules { get; set; } = new List<AccountingRuleX>();
    }


}
