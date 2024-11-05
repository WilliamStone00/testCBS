using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation
{
    public class AccountDepositRequest
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        public int amount { get; set; }
        [Required(ErrorMessage = "Account number is required")]
        public string accountNumber { get; set; }
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
        public string bankId { get; set; }
        public string branchId { get; set; }
        [Required(ErrorMessage = "Teller is required")]
        public string tellerId { get; set; }
        public string depositType { get; set; }
       

    }
    public class CashDesk
    {
        public string CustomerId { get; set; }
        public string LoanId { get; set; }
        public Depositer Depositer { get; set; } = new Depositer();
        public BulkDeposit BulkDeposit { get; set; } = new BulkDeposit();
        public PrintDate PrintDate { get; set; } = new PrintDate();
        public Branch Branch { get; set; } = new Branch();
        public List<BulkDeposit> BulkDeposits { get; set; } = new List<BulkDeposit>();
        public List<WithdrawalNotification> WithdrawalNotifications { get; set; } = new List<WithdrawalNotification>();
        public IndividualProfile Customer { get; set; } = new IndividualProfile();
        public List<LoanApplicationFee> LoanApplicationFees { get; set; } = new List<LoanApplicationFee>();
        public List<CustomerAccount> Accounts { get; set; } = new List<CustomerAccount>();
        public List<Loan> Loans { get; set; } = new List<Loan>();
        public List<Refund> Refunds { get; set; } = new List<Refund>();
        public List<IndividualProfile> Customers { get; set; } = new List<IndividualProfile>();
        public List<TransactionHistory> Transactions { get; set; } = new List<TransactionHistory>();
        public OtherTransaction OtherTransaction { get; set; }
        public AddOtherTransactionMobileMoneyCommand AddOtherTransactionMobileMoneyCommand { get; set; } = new AddOtherTransactionMobileMoneyCommand();
        public List<OtherTransaction> OtherTransactions { get; set; } = new List<OtherTransaction>();
        public string Action { get; set; }
        public string ServiceOption { get; set; }
    }
    public class PrintDate
    {
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string CustomerID { get; set; }
        public string ReportType { get; set; }
    }
    public class  BulkOperation
    {
        public List<BulkDeposit> BulkOperations { get; set; } = new List<BulkDeposit>();
        public string DepositType { get; set; }
        public string Period { get; set; }
        public string OperationType { get; set; }
        public bool IsCashOperation { get; set; }

        public BulkOperation()
        {
            DepositType = "Normal";
        }//Normal, LoanFeePayment, Disbursment, LoanRepayment

    }

    public class BulkDeposit
    {
        public string AccountNumber { get; set; }
        public decimal Fee { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal Penalty { get; set; }
        public decimal Interest { get; set; }
        public decimal Total { get; set; }
        public string AccountType { get; set; }
        public string LoanId { get; set; }
        public string LoanApplicationId { get; set; }
        public string Note { get; set; }
        public string EventCode { get; set; }
        public string SourceType { get; set; }
        public string OperationType { get; set; }
        public string TelephoneNumber { get; set; }
        public string CNI { get; set; }
        public string Period { get; set; }
        public bool IsSWS { get; set; } = false;
        public string CheckNumber { get; set; }
        public string BookingDirection { get; set; }
        public string CheckName { get; set; }
        public string MemberName { get; set; }
        public bool isDepositDoneByAccountOwner { get; set; }
        public bool IsChargesInclussive { get; set; } = false;
        public string PaymentMethod { get; set; }
        public decimal Tax { get; set; }
        public decimal Principal { get; set; }
        public string PaymentChannel { get; set; }

        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
        public Depositer Depositer { get; set; } = new Depositer();
        public OtherTransaction OtherTransaction { get; set; } = new OtherTransaction();
        public BulkDeposit()
        {
            Fee = 0;
            Amount = 0;
            Balance = 0;
            Penalty = 0;
            Interest = 0;
            Total = 0;
        }
    }
    public class Depositer
    {
        public string DepositorName { get; set; }
        public string DepositerNote { get; set; }
        public string DepositerTelephone { get; set; }
        public string DepositorIDNumber { get; set; }
        public string DepositorIDIssueDate { get; set; }
        public string DepositorIDExpiryDate { get; set; }
        public string DepositorIDNumberPlaceOfIssue { get; set; }
    }
    public class WithdrawalRequest
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int amount { get; set; }
        [Required(ErrorMessage = "Account number is required")]
        public string accountNumber { get; set; }
        [Required(ErrorMessage = "Withdrawal type is required")]
        public string withDrawalType { get; set; }
  
        public string note { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
        [Required(ErrorMessage = "Teller is required")]
        public string tellerId { get; set; }
        public CurrencyNotes currencyNotes { get; set; }=new CurrencyNotes();
    }
    public class DepositRequest
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int amount { get; set; }
        [Required(ErrorMessage = "Account number is required")]
        public string accountNumber { get; set; }

        [Required(ErrorMessage = "Deposit type is required")]
        public string depositType { get; set; }
        public string note { get; set; }
        public bool isDepositDoneByAccountOwner { get; set; }=true;
        public string depositerNote { get; set; }

        public string depositerTelephone { get; set; }
        public string depositorIDNumber { get; set; }
        public string depositorName { get; set; }
        public string depositorIDIssueDate { get; set; }
        public string depositorIDExpiryDate { get; set; }
        public string depositorIDNumberPlaceOfIssue { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }

        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Tax { get; set; }
        public decimal Penalty { get; set; }
        public string LoanId { get; set; }


        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
    }
    public class LoanRepaymentCommand
    {
        public decimal Amount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Tax { get; set; }
        public decimal Penalty { get; set; }
        public string LoanId { get; set; }
        public string AccountNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentChannel { get; set; }
        public string DepositType { get; set; }
        public string Note { get; set; }
        public CurrencyNotes CurrencyNotes { get; set; }
        public bool IsDepositDoneByAccountOwner { get; set; }
        public string DepositerTelephone { get; set; }
        public string DepositorIDNumberPlaceOfIssue { get; set; }
        public string DepositerNote { get; set; }
        public string DepositorIDNumber { get; set; }
        public string DepositorName { get; set; }
        public string DepositorIDIssueDate { get; set; }
        public string DepositorIDExpiryDate { get; set; }

    }
    public class TransferRequest
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int Amount { get; set; }
        [Required(ErrorMessage = "Sender account number is required")]
        public string SenderAccountNumber { get; set; }
        [Required(ErrorMessage = "Receiver account number is required")]
        public string ReceiverAccountNumber { get; set; }
        public string Note { get; set; }
        public TransferRequest()
        {
            Note = "Commencing transfer process now. Initiating transfer from [Sender's Account] to [Receiver's Account]. Thank you for your patience.";
        }
    }
    public class TransferConfirmation
    {
        [Required]
        public string TransferId { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string Note { get; set; }
        public TransferConfirmation()
        {
            Status="Approved";
            Note= "After careful review, I've examined this operation and am pleased to grant my approval.";
        }
    }
    public class Transfer
    {
        public string Id { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string SourceAccountType { get; set; }
        public string DestinationAccountType { get; set; }
        public decimal Charges { get; set; }
        public decimal Tax { get; set; }
        public string TransactionRef { get; set; }
        public string TransactionType { get; set; }
        public decimal SourceCommision { get; set; }
        public decimal DestinationCommision { get; set; }
        public bool IsInterBranchOperation { get; set; }
        public decimal Amount { get; set; }
        public string SourceType { get; set; }
        public string Status { get; set; }
        public string ApprovedByUserName { get; set; }
        public string InitiatedByUSerName { get; set; }
        public DateTime DateOfInitiation { get; set; } = DateTime.Now;
        public DateTime DateOfApproval { get; set; } = DateTime.MinValue;
        public string InitiatorComment { get; set; }
        public string ValidatorComment { get; set; }
        public string BranchId { get; set; }
        public string AccountId { get; set; }
        public string TellerId { get; set; }
        public string SourceBrachId { get; set; }
        public string DestinationBrachId { get; set; }
        public string SourceAccountName { get; set; }
        public string DestinationAccountName { get; set; }
        public string SenderName { get; set; }
        public string RecieverName { get; set; }

        public string SourceBranchName { get; set; }
        public string DestinationBranchName { get; set; }
        public virtual Account Account { get; set; }
        public virtual Teller Teller { get; set; }
 
    }

    //
    public class CurrencyNotes
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int note10000 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int note5000 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int note2000 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int note1000 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int note500 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int coin500 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int coin100 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int coin50 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int coin25 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int coin10 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int coin5 { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(0, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int coin1 { get; set; }


    }
    public class CurrencyNotesDto
    {
        public string Id { get; set; }
        public string DinominationType { get; set; }
        public string Denomination { get; set; }
        public int Value { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string ReferenceId { get; set; }
    }

}
