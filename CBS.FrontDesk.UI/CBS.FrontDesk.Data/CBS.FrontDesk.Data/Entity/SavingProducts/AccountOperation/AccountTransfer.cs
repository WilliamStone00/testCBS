using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation
{
    public class AccountTransfer
    {
        public double amount { get; set; }
        public string senderAccountNumber { get; set; }
        public string receiverAccountNumber { get; set; }
        public string sourceDetails { get; set; }
        public string note { get; set; }
    }
}
