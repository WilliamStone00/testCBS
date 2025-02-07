using System;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class UsersNotification  
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string BranchName { get; set; }
        public string Action { get; set; }
        public string ActionId { get; set; }
        public string ActionUrl { get; set; }
        public DateTime CreatedDate { get; set; }

        public string  BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool IsSeen { get; set; }
        public string Timestamp { get; set; }
        public string UserName { get; set; }

        public UsersNotification()
        {
            // Default constructor
        }
        public UsersNotification(string action, string actionUrl, string actionId)
        {
            Action = action;
            ActionUrl = actionUrl;
            ActionId = actionId;
        }
        public UsersNotification(string userName, string branchName, string action, DateTime timestamp, string actionUrl)
        {
            Id = userName;
            BranchName = branchName;
            Action = action;

            ActionUrl = actionUrl;
        }

      
    }
    public class DepositNotificationDto
    {
        public string Id { get; set; }
        public string Amount { get; set; }
    
        public string Temp3 { get; set; }
        public string ApprovalKey { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string IssuedBy { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsApproved { get; set; }
        public bool HasBankAccount { get; set; }
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }
        public string correpondingBranchId { get; set; }
        public string BankAccountId { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime DepositDate { get; set; }
        public string ApprovedMessage { get; set; }
        public string BranchOffice { get; set; }
        public string BranchId { get; set; }
        public bool IsOwner { get; set; }
        public bool HasAccount56 { get; set; }
    }

    public class DepositNotification
    {
        public string  Id { get; set; }

        public string BankAccountOwner{ get; set; }
        public decimal Amount { get; set; }

        public bool HasBankAccount { get; set; }
        public string BankAccountId { get; set; }
        public string Message { get; set; }

        public CurrencyNotesRequest CurrencyNotes { get; set; }
        public string Temp1 { get; set; }

        public object ConvertToTransferData(string branchID )
        {


            return new 
            { 
                Amount = CurrencyNotes.GetAmountValue(),
                Message = Message, 
                BankAccountId = BankAccountId == null ? "XXXXXX" : BankAccountId,
                BankAccountOwner = BankAccountId==null?"XXXXXX" : branchID,
                CurrencyNotesRequest =CurrencyNotes
            };
            
            
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
        public string CorrepondingBranchId { get; set; }
        public string Status { get; set; }
        public string ApprovedMessage { get; set; }
    }
    
}