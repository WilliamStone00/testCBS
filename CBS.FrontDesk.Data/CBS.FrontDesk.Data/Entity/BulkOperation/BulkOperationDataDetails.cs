using System;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class BulkOperationDataDetails
    {

        public string Id { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string SourceAccountNumber { get; set; }
        public string SourceAccountType { get; set; }
        public decimal SourceAccountBalance { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string DestinationAccountType { get; set; }
        public decimal DestinationBalance { get; set; }
        public decimal AmountToDebit { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string ApprovalStatusBadge { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NetBalance { get; set; }
        public string BulkOperationReference { get; set; }
        public string TransactionReference { get; set; }
        public string TransferStatus { get; set; }
        public string TransferMessage { get; set; }
        public DateTime TransferDate { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

    }
}
