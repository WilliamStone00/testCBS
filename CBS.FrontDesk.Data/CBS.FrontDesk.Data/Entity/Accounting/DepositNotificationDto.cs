using System;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class DepositNotificationDto
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }

        public string ApprovalKey { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string IssuedBy { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsApproved { get; set; }
        public bool HasBankAccount { get; set; }

        public string BankAccountOwner { get; set; }
        public string BankAccountId { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime DepositDate { get; set; }
        public string ApprovedMessage { get; set; }
        public string BranchOffice { get; set; }
        public string BranchId { get; set; }
    }

    public class DepositNotification
    {
        public string  Id { get; set; }

        public string BankAccountOwner{ get; set; }
        public decimal Amount { get; set; }

        public bool HasBankAccount { get; set; }
        public string BankAccountId { get; set; }
        public string Message { get; set; }

        public Denomination CurrencyNotes { get; set; }

        public object ConvertToTransferData()
        {


            return new { Amount = CurrencyNotes.GetAmountValue(), Message = Message, BankAccountId = "Not Found", BankAccountOwner="XXXXXX" };
            
            
        }

    }
    

     public class UploadBankReciept
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string BankTransactionReference { get; set; }
        public DateTime ValueDate { get; set; }
        public HttpPostedFileBase UploadedFile { get; set; }
        public string FileUpload { get; set; }
        public UploadBankReciept()
        {
            FileUpload = "~/AppFiles/Images/Bank-deposit-slip-excel-template.jpg";
        }

        public UploadBankRecieptDto ConvertToUploadBankRecieptDto()
        {
            return new UploadBankRecieptDto
            {
                id = Id,
                bankTransactionReference = BankTransactionReference,
                comment = Description,
                filePath = FileUpload,
                ValueDate = ValueDate,
            };
        }
    }
    public class DepositNotificationApproval
    {

        public string Id { get; set; }
        public string BankAccountId { get; set; }
        public string BankAccountOwner { get; set; }
        public string ApprovedMessage { get; set; }
        public bool IsApproved { get; set; }
    }
    
}