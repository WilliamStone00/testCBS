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
        public string Description { get; set; }
        public string SystemDescription { get; set; }
        public string System_Id { get; set; }
        public string BookingDirection { get; set; }
        public string MFI_ChartOfAccountId { get; set; }
        public string AccountNumber { get; set; }
        public double Amount { get; set; }
        public string AccountName { get; set; }
    }
    public class AccountingModelRule
    {
        public bool HasError { get; set; }
        public List<AccountingRule> AccountingRule { get; set; }
    }
    public class AccountingRuleDtos
    {
        public string RuleName { get; set; }
        public string SystemDescription { get; set; }
        public string System_Id { get; set; }

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AccountingRuleX
    {
        public string Description { get; set; }
        public string RuleName { get; set; }
        public string MFI_ChartOfAccountId { get; set; }
        public string BookingDirection { get; set; }
    }

    public class AccountingRuleXRoot
    {
        public List<AccountingRuleX> accountingRules { get; set; } = new List<AccountingRuleX>();
    }


}
