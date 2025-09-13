using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts
{
    public class TransactionRespose
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal OriginalDepositAmount { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public Account Account { get; set; }
        public string TransactionType { get; set; }
        public string OperationType { get; set; }//Debit or Credit
        public string Status { get; set; }
        public string TransactionRef { get; set; }
        public decimal Tax { get; set; }
        public DateTime createdDate { get; set; }
        public decimal PreviousBalance { get; set; }
        public string Note { get; set; }
        public string SenderAccountId { get; set; }
        public string ReceiverAccountId { get; set; }
        public string DepositorIDNumber { get; set; }
        public string DepositorIDNumberPlaceOfIssue { get; set; }
        public string DepositerNote { get; set; }
        public string DepositorName { get; set; }
        public string DepositorIDIssueDate { get; set; }
        public bool IsDepositDoneByAccountOwner { get; set; }
        public string DepositerTelephone { get; set; }
        public string DepositorIDExpiryDate { get; set; }
        public decimal BalanceBroughtForward { get; set; }
        public string ProductId { get; set; }
        public decimal Fee { get; set; }
        public string FeeType { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string CurrencyNotesId { get; set; }
        public string TellerId { get; set; }
        public CurrencyNotes CurrencyNotes { get; set; }
    }

}
