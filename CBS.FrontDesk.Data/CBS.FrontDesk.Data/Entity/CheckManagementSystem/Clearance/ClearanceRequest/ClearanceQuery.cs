using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest
{
    public class ClearanceQuery
    {
        public string BranchId { get; set; } = null;
        public string OperationType { get; set; } = null;
        public string Status { get; set; } = null;
        public string StartDate { get; set; } = null;
        public string EndDate { get; set; } = null;

        public DataTableOptions Options { get; set; } = null;


        public ClearanceQuery()
        {
            Options = new DataTableOptions();
        }
    }

    public class ChequeProcessingRequest1
    {
        public ChequeProcessingRequest1()
        {
            Options = new DataTableOptions();
        }
        public DataTableOptions Options { get; set; } = null;
       
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string ExternalCustomerName { get; set; }
        public string NationalIdNumber { get; set; }
        public string NationalIdType { get; set; }
        public DateTime? NationalIdCreationDate { get; set; }
        public DateTime? NationalIdExpirationDate { get; set; }
        public string BranchId { get; set; }
        public string CheckBookId { get; set; }
        public int CheckBookPageNumber { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ClearedDate { get; set; }
        public DateTime? RejectionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool External { get; set; }
        public bool IsInterBranch { get; set; }
        public bool DepositToAccount { get; set; }
        public bool SameDayProcessing { get; set; }
    }
}
