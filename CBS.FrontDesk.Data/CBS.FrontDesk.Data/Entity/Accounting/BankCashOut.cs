using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BankCashOut
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Balance { get; set; }
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
         
        public string ApprovedDate { get; set; }
        //public string ApprovedBy { get; set; }
        public string IssuedBy { get; set; }
        public string BranchOffice { get; set; }
        public string FileUpload { get; set; }
        [Required]
        public HttpPostedFileBase UploadedFile{ get; set; }
        [Required]
        public DateTime ValueDate { get; set; }
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
                AccountId = cashOut.AccountId,
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


    public class BankCashOutDto
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
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