using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BranchToBranchTransfer
    {
        [Required]
        public string FromAccountId { get; set; }

        public string Accountinfor { get; set; }
        public string Balance { get; set; }

        public string ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }
        public string ReferenceId { get; set; }
        public string TransferBy { get; set; }
        public string TransferDate { get; set; }
        public CurrencyNotesRequest CurrencyNotesRequest { get; set; }

        public BranchTransfer ConvertToTransferData()
        {
            return new BranchTransfer
            {
                FromAccountId = FromAccountId,
                ToAccountId = ToAccountId,
                CurrencyNotesRequest= CurrencyNotesRequest,
                Amount = CurrencyNotesRequest.GetAmountValue(),
          
                ReferenceId = ReferenceId,
            };
        }


    }
    public class BranchToBranchTransferCommand  
    {
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string ToAccountId { get; set; }
        public string FromAccountId { get; set; }
        public CurrencyNotesRequest CurrencyNotesRequest { get; set; }

    }
    public class BranchTransfer
    {

        public string ToAccountId { get; set; }
        public string FromAccountId { get; set; }

        public decimal Amount { get; set; }

        public CurrencyNotesRequest CurrencyNotesRequest { get; set; }
        public string ReferenceId { get; set; }


    }


  

    public class CurrencyNotesRequest
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

        public decimal GetAmountValue()
        {
            return (this.note10000 * 10000) +
                                  (this.note5000 * 5000) +
                                  (this.note2000 * 2000) +
                                  (this.note1000 * 1000) +
                                  (this.note500 * 500) +
                                  (this.coin500 * 500) +
                                  (this.coin100 * 100) +
                                  (this.coin50 * 50) +
                                  (this.coin25 * 25) +
                                  (this.coin10 * 10) +
                                  (this.coin5 * 5) +
                                  this.coin1;
        }
    }
}
 