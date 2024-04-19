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
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal OriginalDepositAmount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string OperationType { get; set; }//Debit or Credit
        public string Status { get; set; } = "PENDING";
        public string TransactionRef { get; set; }
        public decimal Tax { get; set; }
        public string Operation { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal SourceBranchCommission { get; set; }
        public decimal DestinationBranchCommission { get; set; }
        public string Note { get; set; }
        public string SenderAccountId { get; set; }
        public string ReceiverAccountId { get; set; }
        public string DepositorIDNumber { get; set; }
        public string DepositerTelephone { get; set; }
        public bool IsDepositDoneByAccountOwner { get; set; }
        public string DepositorName { get; set; }
        public string DepositorIDIssueDate { get; set; }
        public string DepositorIDExpiryDate { get; set; }
        public string DepositorIDNumberPlaceOfIssue { get; set; }
        public string DepositerNote { get; set; }
        public bool IsInterBrachOperation { get; set; }
        public string SourceBrachId { get; set; }
        public string DestinationBrachId { get; set; }
        public string AmountInWord { get; set; }
        public decimal Balance { get; set; }
        public string ProductId { get; set; }
        public decimal Fee { get; set; }
        public string FeeType { get; set; }
        public string SourceType { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string TellerId { get; set; }
        public virtual ICollection<TellerOperation> TellerOperations { get; set; }
        public virtual Teller Teller { get; set; }
        public virtual Account Account { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CurrencyNotesDto> currencyNotes { get; set; }
        public CurrencyNotes currencyNote { get; set; }

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
        public DateTime Date { get; set; }
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
        public decimal credit { get; set; }
        public decimal debit { get; set; }
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
        public string InterBrachOperation { get; set; }
        public string SourceBranch { get; set; }
        public string DestinationBranch { get; set; }
        public decimal SourceShare { get; set; }
        public decimal DestinationShare { get; set; }
    }

}
