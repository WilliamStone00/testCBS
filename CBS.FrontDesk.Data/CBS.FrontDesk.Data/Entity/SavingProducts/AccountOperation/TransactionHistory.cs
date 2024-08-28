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
        public string CustomerId { get; set; }
        public string ExternalReference { get; set; }
        public bool IsExternalOperation { get; set; }
        public string ExternalApplicationName { get; set; }
        public string Currency { get; set; }
        public decimal Credit { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string OperationType { get; set; }//Debit or Credit
        public string Status { get; set; } = "PENDING";
        public string TransactionReference { get; set; }
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
        public decimal WithrawalFormCharge { get; set; }
        public decimal OperationCharge { get; set; }
        public decimal WithdrawalChargeWithoutNotification { get; set; }
        public decimal CloseOfAccountCharge { get; set; }
        public string FeeType { get; set; }
        public string SourceType { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string TellerId { get; set; }
        public string ReceiptTitle { get; set; }
        public string CreatedBy { get; set; }
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

    public class TellerProvioningHistory
    {
        public string Id { get; set; }
        public string TellerId { get; set; }
        public string UserIdInChargeOfThisTeller { get; set; }
        public string ProvisionedBy { get; set; }
        public bool IsCashReplenished { get; set; }
        public decimal ReplenishedAmount { get; set; }
        public DateTime? OpenedDate { get; set; }
        public DateTime? ClossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal OpenOfDayAmount { get; set; } = 0;
        public string ReferenceId { get; set; }
        public string CloseOfReferenceId { get; set; }
        public bool IsRequestedForCashReplenishment { get; set; }
        public decimal CashAtHand { get; set; } = 0;
        public decimal EndOfDayAmount { get; set; } = 0;
        public decimal AccountBalance { get; set; } = 0;
        public decimal LastOPerationAmount { get; set; } = 0;
        public string LastOperationType { get; set; }
        public decimal PreviouseBalance { get; set; }
        public string LastUserID { get; set; }
        public string SubTellerComment { get; set; }
        public string Note { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string ClossedStatus { get; set; }
        public string PrimaryTellerComment { get; set; }
        public string PrimaryTellerConfirmationStatus { get; set; }
        public string DailyTellerId { get; set; }
        public virtual Teller Teller { get; set; }
        public string InitialPrinting { get; set; }
        public bool IsPrimaryTeller { get; set; }
        // Opening Notes and Coins Counts
        public int OpeningNote10000 { get; set; }
        public int OpeningNote5000 { get; set; }
        public int OpeningNote2000 { get; set; }
        public int OpeningNote1000 { get; set; }
        public int OpeningNote500 { get; set; }
        public int OpeningCoin500 { get; set; }
        public int OpeningCoin100 { get; set; }
        public int OpeningCoin50 { get; set; }
        public int OpeningCoin25 { get; set; }
        public int OpeningCoin10 { get; set; }
        public int OpeningCoin5 { get; set; }
        public int OpeningCoin1 { get; set; }

        // Closing Notes and Coins Counts
        public int ClosingNote10000 { get; set; }
        public int ClosingNote5000 { get; set; }
        public int ClosingNote2000 { get; set; }
        public int ClosingNote1000 { get; set; }
        public int ClosingNote500 { get; set; }
        public int ClosingCoin500 { get; set; }
        public int ClosingCoin100 { get; set; }
        public int ClosingCoin50 { get; set; }
        public int ClosingCoin25 { get; set; }
        public int ClosingCoin10 { get; set; }
        public int ClosingCoin5 { get; set; }
        public int ClosingCoin1 { get; set; }

        // Calculated Properties for Opening Denominations
        public decimal OpeningTotal10000 => OpeningNote10000 * 10000;
        public decimal OpeningTotal5000 => OpeningNote5000 * 5000;
        public decimal OpeningTotal2000 => OpeningNote2000 * 2000;
        public decimal OpeningTotal1000 => OpeningNote1000 * 1000;
        public decimal OpeningTotal500 => OpeningNote500 * 500;
        public decimal OpeningTotalCoin500 => OpeningCoin500 * 500;
        public decimal OpeningTotalCoin100 => OpeningCoin100 * 100;
        public decimal OpeningTotalCoin50 => OpeningCoin50 * 50;
        public decimal OpeningTotalCoin25 => OpeningCoin25 * 25;
        public decimal OpeningTotalCoin10 => OpeningCoin10 * 10;
        public decimal OpeningTotalCoin5 => OpeningCoin5 * 5;
        public decimal OpeningTotalCoin1 => OpeningCoin1 * 1;

        // Calculated Properties for Closing Denominations
        public decimal ClosingTotal10000 => ClosingNote10000 * 10000;
        public decimal ClosingTotal5000 => ClosingNote5000 * 5000;
        public decimal ClosingTotal2000 => ClosingNote2000 * 2000;
        public decimal ClosingTotal1000 => ClosingNote1000 * 1000;
        public decimal ClosingTotal500 => ClosingNote500 * 500;
        public decimal ClosingTotalCoin500 => ClosingCoin500 * 500;
        public decimal ClosingTotalCoin100 => ClosingCoin100 * 100;
        public decimal ClosingTotalCoin50 => ClosingCoin50 * 50;
        public decimal ClosingTotalCoin25 => ClosingCoin25 * 25;
        public decimal ClosingTotalCoin10 => ClosingCoin10 * 10;
        public decimal ClosingTotalCoin5 => ClosingCoin5 * 5;
        public decimal ClosingTotalCoin1 => ClosingCoin1 * 1;
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        // Total Opening and Closing Amounts
        public decimal TotalOpeningAmount =>
            OpeningTotal10000 + OpeningTotal5000 + OpeningTotal2000 + OpeningTotal1000 + OpeningTotal500 +
            OpeningTotalCoin500 + OpeningTotalCoin100 + OpeningTotalCoin50 + OpeningTotalCoin25 +
            OpeningTotalCoin10 + OpeningTotalCoin5 + OpeningTotalCoin1;

        public decimal TotalClosingAmount =>
            ClosingTotal10000 + ClosingTotal5000 + ClosingTotal2000 + ClosingTotal1000 + ClosingTotal500 +
            ClosingTotalCoin500 + ClosingTotalCoin100 + ClosingTotalCoin50 + ClosingTotalCoin25 +
            ClosingTotalCoin10 + ClosingTotalCoin5 + ClosingTotalCoin1;

    }

    public class PrimaryTellerProvisioningHistory
    {

        public string Id { get; set; }
        public string TellerId { get; set; }
        public string ReferenceId { get; set; }
        public string CloseOfDayReferenceId { get; set; }
        public string UserIdInChargeOfThisTeller { get; set; }
        public string ProvisionedBy { get; set; }
        public DateTime? OpenedDate { get; set; }
        public string PrimaryTellerId { get; set; }
        public DateTime? ClossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal OpenOfDayAmount { get; set; } = 0;
        public decimal CashReplenishmentAmount { get; set; } = 0;
        public string ReplenishmentReferenceNumber { get; set; }
        public bool IsCashReplenishment { get; set; }
        public decimal CashAtHand { get; set; } = 0;
        public decimal EndOfDayAmount { get; set; } = 0;
        public decimal AccountBalance { get; set; } = 0;
        public string DailyTellerId { get; set; }
        public decimal PreviouseBalance { get; set; }
        public string LastUserID { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string ClossedStatus { get; set; }
        public string PrimaryTellerComment { get; set; }
        public virtual Teller Teller { get; set; }
        // Opening Notes and Coins Counts
        public int OpeningNote10000 { get; set; }
        public int OpeningNote5000 { get; set; }
        public int OpeningNote2000 { get; set; }
        public int OpeningNote1000 { get; set; }
        public int OpeningNote500 { get; set; }
        public int OpeningCoin500 { get; set; }
        public int OpeningCoin100 { get; set; }
        public int OpeningCoin50 { get; set; }
        public int OpeningCoin25 { get; set; }
        public int OpeningCoin10 { get; set; }
        public int OpeningCoin5 { get; set; }
        public int OpeningCoin1 { get; set; }

        // Closing Notes and Coins Counts
        public int ClosingNote10000 { get; set; }
        public int ClosingNote5000 { get; set; }
        public int ClosingNote2000 { get; set; }
        public int ClosingNote1000 { get; set; }
        public int ClosingNote500 { get; set; }
        public int ClosingCoin500 { get; set; }
        public int ClosingCoin100 { get; set; }
        public int ClosingCoin50 { get; set; }
        public int ClosingCoin25 { get; set; }
        public int ClosingCoin10 { get; set; }
        public int ClosingCoin5 { get; set; }
        public int ClosingCoin1 { get; set; }

        // Calculated Properties for Opening Denominations
        public decimal OpeningTotal10000 => OpeningNote10000 * 10000;
        public decimal OpeningTotal5000 => OpeningNote5000 * 5000;
        public decimal OpeningTotal2000 => OpeningNote2000 * 2000;
        public decimal OpeningTotal1000 => OpeningNote1000 * 1000;
        public decimal OpeningTotal500 => OpeningNote500 * 500;
        public decimal OpeningTotalCoin500 => OpeningCoin500 * 500;
        public decimal OpeningTotalCoin100 => OpeningCoin100 * 100;
        public decimal OpeningTotalCoin50 => OpeningCoin50 * 50;
        public decimal OpeningTotalCoin25 => OpeningCoin25 * 25;
        public decimal OpeningTotalCoin10 => OpeningCoin10 * 10;
        public decimal OpeningTotalCoin5 => OpeningCoin5 * 5;
        public decimal OpeningTotalCoin1 => OpeningCoin1 * 1;

        // Calculated Properties for Closing Denominations
        public decimal ClosingTotal10000 => ClosingNote10000 * 10000;
        public decimal ClosingTotal5000 => ClosingNote5000 * 5000;
        public decimal ClosingTotal2000 => ClosingNote2000 * 2000;
        public decimal ClosingTotal1000 => ClosingNote1000 * 1000;
        public decimal ClosingTotal500 => ClosingNote500 * 500;
        public decimal ClosingTotalCoin500 => ClosingCoin500 * 500;
        public decimal ClosingTotalCoin100 => ClosingCoin100 * 100;
        public decimal ClosingTotalCoin50 => ClosingCoin50 * 50;
        public decimal ClosingTotalCoin25 => ClosingCoin25 * 25;
        public decimal ClosingTotalCoin10 => ClosingCoin10 * 10;
        public decimal ClosingTotalCoin5 => ClosingCoin5 * 5;
        public decimal ClosingTotalCoin1 => ClosingCoin1 * 1;

        // Total Opening and Closing Amounts
        public decimal TotalOpeningAmount =>
            OpeningTotal10000 + OpeningTotal5000 + OpeningTotal2000 + OpeningTotal1000 + OpeningTotal500 +
            OpeningTotalCoin500 + OpeningTotalCoin100 + OpeningTotalCoin50 + OpeningTotalCoin25 +
            OpeningTotalCoin10 + OpeningTotalCoin5 + OpeningTotalCoin1;

        public decimal TotalClosingAmount =>
            ClosingTotal10000 + ClosingTotal5000 + ClosingTotal2000 + ClosingTotal1000 + ClosingTotal500 +
            ClosingTotalCoin500 + ClosingTotalCoin100 + ClosingTotalCoin50 + ClosingTotalCoin25 +
            ClosingTotalCoin10 + ClosingTotalCoin5 + ClosingTotalCoin1;

        public decimal LastOPerationAmount { get; set; }
        public string LastOperationType { get; set; }
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
