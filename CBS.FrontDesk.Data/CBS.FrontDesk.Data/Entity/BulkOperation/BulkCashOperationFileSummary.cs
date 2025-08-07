
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class BulkCashOperationFileSummary
    {
        public string FileUploadId { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalMembers { get; set; }
        public string UploadedBy { get; set; }
        public List<BulkCashOperationFileDetails> BulkCashOperationFileDetails { get; set; }
    }
}