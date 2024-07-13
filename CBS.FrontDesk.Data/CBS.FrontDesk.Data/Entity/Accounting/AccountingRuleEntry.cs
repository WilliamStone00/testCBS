
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
  
    public class AccountingRuleEntry
{
        public string Id { get; set; }
        public string  AccountingRuleEntryName { get; set; }
        public string BookingDirection { get; set; }
        public string  OperationEventAttributeId { get; set; }
        public string  OperationEventId { get; set; }
        public string DeterminationAccountId { get; set; }
        public string DeterminationManagementAccountId { get; set; }
        public string BalancingAccountId { get; set; }
        public bool HasManagementAccount { get; set; }
        public string BankId { get; set; }
        
                    public string Description { get; set; }
    }

    public class AccountingRuleEntryx
    {
        public string Id { get; set; }
        public string AccountingRuleEntryName { get; set; }
    }
}
