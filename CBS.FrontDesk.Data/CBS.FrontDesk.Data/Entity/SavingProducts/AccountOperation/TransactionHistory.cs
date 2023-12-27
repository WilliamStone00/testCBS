using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation
{
    public class TransactionHistory
    {
        public string id { get; set; }
        public double? amount { get; set; }
        public string status { get; set; }
        public string accountId { get; set; }
        public string accountNumber { get; set; }
        public string transactionRef { get; set; }
        public string transactionType { get; set; }
        public double? tax { get; set; }
        public double? previousBalance { get; set; }
        public string note { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public string senderAccountId { get; set; }
        public string receiverAccountId { get; set; }
        public string productId { get; set; }
        public double? balanceBroughtForward { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
    }
}
