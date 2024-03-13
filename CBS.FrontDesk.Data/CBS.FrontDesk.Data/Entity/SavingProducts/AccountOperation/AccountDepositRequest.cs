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
        public CurrencyNotes currencyNotes { get; set; } = new CurrencyNotes();
    }
    public class TransferRequest
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Amount must be a positive whole number")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int amount { get; set; }
        [Required(ErrorMessage = "Sender account number is required")]
        public string senderAccountNumber { get; set; }
        [Required(ErrorMessage = "Receiver account number is required")]
        public string receiverAccountNumber { get; set; }
        [Required(ErrorMessage = "Source type is required")]
        public string transferType { get; set; }
       
        public string note { get; set; }
        [Required(ErrorMessage = "Teller is required")]
        public string tellerId { get; set; }
        [Required(ErrorMessage = "Currency is required")]
        public string currency { get; set; }
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
    
}
