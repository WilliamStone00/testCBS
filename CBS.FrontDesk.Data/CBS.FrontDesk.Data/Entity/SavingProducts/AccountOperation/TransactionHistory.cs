using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
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
        public string Operation { get; set; }
        public Account account { get; set; }
        public string transactionType { get; set; }
        public string operationType { get; set; }
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
        public CurrencyNotes currencyNotes { get; set; }
        public Teller teller { get; set; }
        public List<TellerOperation> TellerOperations { get; set; }

    }
    public class TellerOperation
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string AccountID { get; set; }
        public string OperationType { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string EventName { get; set; }
        public string TransactionRef { get; set; }
        public string TransactionID { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string TellerID { get; set; }
        public string UserID { get; set; }
        public DateTime? Date { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public TransactionHistory Transaction { get; set; }

    }
    public class TransactionHistoryExport
    {
        public string accountHolderName { get; set; }
        public string accountNumber { get; set; }
        public string customerReferenceNumber { get; set; }
        public DateTime Date { get; set; }
        public decimal originalAmount { get; set; }
        public decimal fee { get; set; }
        public decimal newAmount { get; set; }
        public decimal previousBalance { get; set; }
        public decimal balance { get; set; }
      
        public string Operation { get; set; }
        public string feeType { get; set; }

        public string transactionType { get; set; }
        public string operationDirection { get; set; }
        public string transactionRef { get; set; }
        public string note { get; set; }
        public string senderAccountId { get; set; }
        public string receiverAccountId { get; set; }
        public string depositorIdNumber { get; set; }
        public string depositorName { get; set; }
        public string depositorIdIssueDate { get; set; }
        public string depositorIdExpiryDate { get; set; }
        public string productName { get; set; }
        public string teller { get; set; }
    }

}
