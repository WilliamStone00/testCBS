using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;

namespace CBS.FrontDesk.Data.ReportDataSetDto
{
    public class TransactionReportDS
    {
        public decimal Amount { get; set; }
        public decimal OriginalDepositAmount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string OperationType { get; set; }
        public string Status { get; set; }
        public string TransactionRef { get; set; }
        public string SendingBranch { get; set; }
        public string RecievingBranch { get; set; }
        public decimal Tax { get; set; }
        public string Operation { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal SourceBranchCommission { get; set; }
        public decimal DestinationBranchCommission { get; set; }
        public string Note { get; set; }
        public string AmountInWord { get; set; }
        public string SenderName { get; set; }
        public string RecieverName { get; set; }
        public string DepositorIDNumber { get; set; }
        public string DepositerTelephone { get; set; }
        public bool IsDepositDoneByAccountOwner { get; set; }
        public string DepositorName { get; set; }
        public string DepositorIDIssueDate { get; set; }
        public string DepositorIDExpiryDate { get; set; }
        public string DepositorIDNumberPlaceOfIssue { get; set; }
        public string DepositerNote { get; set; }
        public bool IsInterBrachOperation { get; set; }
        public string InterBrachOperation { get; set; }
        public decimal Balance { get; set; }
        public string ProductName { get; set; }
        public decimal Fee { get; set; }
        public string FeeType { get; set; }
        public string SourceType { get; set; }
        public int Coin1 { get; set; }
        public int Coin5 { get; set; }
        public int Coin10 { get; set; }
        public int Coin25 { get; set; }
        public int Coin50 { get; set; }
        public int Coin100 { get; set; }
        public int Coin500 { get; set; }
        public int Note500 { get; set; }
        public int Note1000 { get; set; }
        public int Note2000 { get; set; }
        public int Note5000 { get; set; }
        public int Note10000 { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TellerName { get; set; }
        public string CashierName { get; set; }
        public string Logo { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
        public string AccountType { get; set; }
        public string AccountName { get; set; }
        public string CustomerName { get; set; }
        public string ExpireDate { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string Village { get; set; }
        public string Country { get; set; }
        public string DeliveryDate { get; set; }
        public decimal Charges { get; set; }
        public string Telephone { get; set; }
        public string Key { get; set; }
        public string BarCode { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal OpeningBalance { get; set; }
        public string ReceiptTitle { get; set; }
    }
    public class OtherTransactionDto
    {
        public string TransactionReference { get; set; }
        public string EnventName { get; set; }
        public string Description { get; set; }
        public string TellerCode { get; set; }
        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string AccountNumber { get; set; }
        public string Direction { get; set; }
        public string TransactionType { get; set; }
        public string SourceType { get; set; }
        public string Naration { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string EventCode { get; set; }
        public string MemberName { get; set; }
        public string Logo { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
        public DateTime DateOfOPeration { get; set; }
        public string TellerName { get; set; }
        public string AmountInWord { get; set; }
        public string ReceiptTitle { get; set; }

    }
    public class OpenningAnclClossingTillDto: HeadOffice
    {
        public string UserIdInChargeOfThisTeller { get; set; }
        public string ProvisionedBy { get; set; }
        public string OpeningReference { get; set; }
        public bool IsCashReplenished { get; set; }
        public string TellerType { get; set; }
        public string IsPrimary { get; set; }
        public string ClossingReference { get; set; }
        public decimal ReplenishedAmount { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime ClossedDate { get; set; } = new DateTime(1900, 1, 1);
        public decimal OpenOfDayAmount { get; set; } = 0;
        public decimal AmountReplenished { get; set; }
        public bool IsRequestedForCashReplenishment { get; set; }
        public decimal CashAtHand { get; set; } = 0;
        public decimal EndOfDayAmount { get; set; } = 0;
        public decimal AccountBalance { get; set; } = 0;
        public decimal LastOPerationAmount { get; set; } = 0;
        public string LastOperationType { get; set; }
        public decimal PreviouseBalance { get; set; }
        public string TellerComment { get; set; }
        public string ClossedStatus { get; set; }
        public string PrimaryTellerComment { get; set; }
        public string PrimaryTellerConfirmationStatus { get; set; }
        public string TellerName { get; set; }
    }
    public class TillOpenAndClossingDS: HeadOffice
    {
        public string UserIdInChargeOfThisTeller { get; set; }
        public string ProvisionedBy { get; set; }
        public bool IsCashReplenished { get; set; }
        public decimal ReplenishedAmount { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime ClossedDate { get; set; }
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
        public string SubTellerComment { get; set; }
        public string Note { get; set; }
        public string ClossedStatus { get; set; }
        public string TillName { get; set; }
        public string InitialPrinting { get; set; } = "Initial Printing";

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
        public decimal TotalOpeningAmount { get; set; }
        public decimal TotalClosingAmount { get; set; }

    }

    public class HeadOffice
    {
        public string Logo { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
    }
}
