using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeCancelation
{
    // CancellationRequest.cs
    public class CancellationRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RequestType { get; set; } // "ChequeBook", "ChequeLeaf", "ChequeNumber"
        public string ChequeBookId { get; set; }
        public string ChequeLeafId { get; set; }
        public string ChequeNumber { get; set; }
        public string PageNumber { get; set; }

        // Requester Info
        public string RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; }
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
        public string BranchId { get; set; }
        public string BranchName { get; set; }

        // Request Details
        public string AccountNumber { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Reason { get; set; }
        public string AdditionalNotes { get; set; }
        public string AttachmentPath { get; set; }

        // Status & Workflow
        public CancellationStatus Status { get; set; } = CancellationStatus.Pending;
        public string ReviewedByUserId { get; set; }
        public string ReviewedByUserName { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string ReviewComments { get; set; }
        public bool IsAutoApproved { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }

    public enum CancellationStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3,
        Completed = 4
    }

    // DTOs for API
    //public class CreateCancellationRequestDto
    //{
    //    public string RequestType { get; set; }
    //    public string ChequeBookId { get; set; }
    //    public string ChequeLeafId { get; set; }
    //    public string ChequeNumber { get; set; }
    //    public string PageNumber { get; set; }
    //    public string Reason { get; set; }
    //    public string AdditionalNotes { get; set; }
    //}

    //public class ReviewRequestDto
    //{
    //    public string RequestId { get; set; }
    //    public bool IsApproved { get; set; }
       

    //    [Required]
    //    public string Comment { get; set; }
    //    [Required]
    //    public string Status { get; set; }
    //}

    public class CancellationRequestQuery
    {
        public string Status { get; set; }
        public string BranchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DataTableOptions DataTableOptions { get; set; }
    }
}
