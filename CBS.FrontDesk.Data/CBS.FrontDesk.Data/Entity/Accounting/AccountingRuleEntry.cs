
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
        public string AccountingRuleEntryName { get; set; }
       
        public string AccountingRuleId { get; set; }

 
        public int BookingDirection { get; set; }

   
        public string OperationEventAttributeId { get; set; }

 
        public string DebitAccountId { get; set; }

 
        public string CreditAccountId { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string OrganizationId { get; set; }
    }
}
