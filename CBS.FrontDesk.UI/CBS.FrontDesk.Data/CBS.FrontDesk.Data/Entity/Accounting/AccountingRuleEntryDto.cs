using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountingRuleEntryData
    {
        public string Id { get; set; }
        public string AccountingRuleEntryName { get; set; }
        public string Determinant { get; set; }
        public string CounterPart { get; set; }

        public string DeterminantAccountNumber { get; set; }
        public string CounterPartAccountNumber { get; set; }
        public string BookingDirection { get; set; }

    }
    public class AccountingRuleEntryDto
    {
        public string Id { get; set; }
        public string AccountingRuleEntryName { get; set; }
        public string BookingDirection { get; set; }
        public string OperationEventAttributeName { get; set; }
        public string OperationEventName { get; set; }
        public string DebitAccountLabel { get; set; }
        public string CreditAccountLabel { get; set; }

        public string DebitAccountNumber { get; set; }
        public string CreditAccountNumber { get; set; }

    }
}
