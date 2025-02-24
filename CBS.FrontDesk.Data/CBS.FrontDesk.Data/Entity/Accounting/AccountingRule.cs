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
        public string EventName { get; set; }
        public string System_Id { get; set; }
        public string BookingDirection { get; set; }
        public List<string> ListOfEligibleBranchId { get; set; }
        public bool IsValidationNeed { get; set; }
        public string MFI_ChartOfAccountId { get; set; }
        public string AccountNumber { get; set; }
        public double Amount { get; set; }
        public string AccountName { get; set; }
        public string EntryType { get; set; }
        public string LevelOfExecution { get; set; }
        public bool IsChainEntry { get; set; }
        public string AccountingEventRuleId { get; set; }
        public bool IsInterBranchTransaction { get; set; }
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
