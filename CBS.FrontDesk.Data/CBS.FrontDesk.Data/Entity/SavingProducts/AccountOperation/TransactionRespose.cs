using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{
    public class TransactionRespose
    {
        public string id { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
        public string accountId { get; set; }
        public string accountNumber { get; set; }
        public string transactionRef { get; set; }
        public string transactionType { get; set; }
        public double tax { get; set; }
        public string sourceDetails { get; set; }
        public double previousBalance { get; set; }
        public string note { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public string senderAccountId { get; set; }
        public string receiverAccountId { get; set; }
    }

}
