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
        public decimal amount { get; set; }
        public decimal originalDepositAmount { get; set; }
        public string accountId { get; set; }
        public string accountNumber { get; set; }
        public Account account { get; set; }
        public string transactionType { get; set; }
        public string operationType { get; set; } // Debit or Credit
        public string status { get; set; }
        public string transactionRef { get; set; }
        public decimal tax { get; set; }
        public decimal previousBalance { get; set; }
        public string note { get; set; }
        public string senderAccountId { get; set; }
        public string receiverAccountId { get; set; }
        public string depositorIdNumber { get; set; }
        public string depositorName { get; set; }
        public string depositorIdIssueDate { get; set; }
        public string depositorIdExpiryDate { get; set; }
        public decimal balanceBroughtForward { get; set; }
        public string productId { get; set; }
        public decimal fee { get; set; }
        public DateTime createdDate { get; set; }
        public string feeType { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        public string currencyNotesId { get; set; }
        public string tellerId { get; set; }
        //public CurrencyNotesDto currencyNotes { get; set; }
        //public TellerDto teller { get; set; }
        //public List<TellerOperationDto> tellerOperations { get; set; }
    }

}
