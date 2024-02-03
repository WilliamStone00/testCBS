using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{

    public class AccountingRuleEntryDto
    {
        public string Id { get; set; }
        public string AccountingRuleEntryName { get; set; }
        public string BookingDirection { get; set; }
        public string OperationEventAttributeName { get; set; }
        public string OperationEventName { get; set; }
        public string DebitAccountLabel { get; set; }
        public string CreditAccountLabel { get; set; }
         
    }
}
