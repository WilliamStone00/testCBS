using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BankCashOut
    {
        public string Id { get; set; }
        [Required]
        public string FromAccountId { get; set; }
        
              public string FromAccountName { get; set; }
        public string Balance { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string ReferenceId { get; set; }
        public string TransactionType { get; set; }
        [Required]
        public string BankTransactionReference { get; set; }
        [Required]
        public string Description { get; set; }
        public string AmountRequested { get; set; }
          public string CreatedBy { get; set; }
        public string IssuedDate { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedDate { get; set; }
          public string ToAccountId { get; set; }
        public string ApprovedDate { get; set; }
        //public string ApprovedBy { get; set; }
        public string IssuedBy { get; set; }
        public string BranchOffice { get; set; }
        public string FileUpload { get; set; }
        [Required]
        public HttpPostedFileBase UploadedFile{ get; set; }
        [Required]
        public DateTime ValueDate { get; set; }
        public CurrencyNotesRequest CurrencyNotes { get; set; }

        public BankTransactionModel ConvertToTransferData()
        {
            try
            {
                if (true/*Convert.ToDecimal(Balance) - Amount > 0*/)
                {
                    return new BankTransactionModel
                    {
                        FromAccountId = FromAccountId,
                        ToAccountId = ToAccountId,
                        Amount = CurrencyNotes.GetAmountValue(),
                        ReferenceId = ReferenceId,
                        TransactionType = TransactionType,
                        BankTransactionReference = BankTransactionReference,
                        ValueDate = ValueDate.ToString(),
                        FileUpload = FileUpload,
                        Description = Description,
                        Id = Id,
                        CurrencyNotesRequest = CurrencyNotes,
                        Balance = Balance
                    };
                }
                else
                {
                    throw new Exception("The balance of bank account does not permit this cash out");
                }
            }
            catch (Exception ex)
            {

                throw(ex);
            }
            
        }
        public BankCashOut()
        {
            FileUpload = "~/AppFiles/Images/Bank-deposit-slip-excel-template.jpg";
        }
        //convert BankCashOut to BankCashOutDto
        public BankCashOutDto ConvertToBankCashOutDto(BankCashOut cashOut)
        {
            return new BankCashOutDto
            {
                Id = cashOut.Id,
                AccountId = cashOut.FromAccountId,
                Balance = cashOut.Balance,
                Amount = cashOut.Amount,

                ReferenceId = cashOut.ReferenceId,
                TransactionType = cashOut.TransactionType,
                BankTransactionReference = cashOut.BankTransactionReference,
                Description = cashOut.Description,
                ValueDate = cashOut.ValueDate,
                FileUpload = cashOut.FileUpload

            };
        }
    }

    

   
    public class CashClearing
    {
        public string CreatedDate { get; set; }
        public string TransferBy { get; set; }
        public string ToAccountId { get; set; }
        public string FromAccountId { get; set; }
        public string AccountInfo { get; set; }
        [Required]
        public string ExpectedAmount { get; set; }
       
        public decimal AmountExpected { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string ReferenceId { get; set; }
     
        public string Description { get; set; }
        [Required]
        public CurrencyNotesRequest CurrencyNotes { get; set; }
        public BranchTransfer ConvertToTransferData()
        {
            return new BranchTransfer
            {
                FromAccountId = FromAccountId,
                ToAccountId = ToAccountId,
                CurrencyNotesRequest = CurrencyNotes,
                Amount = CurrencyNotes.GetAmountValue(),
                ReferenceId = ReferenceId,
            };
        }

    }
    public class BankCashOutDto
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string Balance { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceId { get; set; }
        public string TransactionType { get; set; }
        [Required]
        public string BankTransactionReference { get; set; }
        [Required]
        public string Description { get; set; }

        public string FileUpload { get; set; }
    
        [Required]
        public DateTime ValueDate { get; set; }
         

    }
}