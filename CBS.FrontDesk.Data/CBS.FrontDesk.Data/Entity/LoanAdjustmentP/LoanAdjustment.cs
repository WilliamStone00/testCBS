using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.LoanAdjustmentP
{
    public class SubmitLoanAdjustmentRequestCommand
    {
        public string LoanId { get; set; }
        public string BranchId { get; set; }
        public string CustomerId { get; set; }
        public decimal OldVatRate { get; set; }
        public decimal OldIntRate { get; set; }
        public decimal NewVatRate { get; set; }
        public decimal NewIntRate { get; set; }
        public decimal? NewLoanAmount { get; set; }
        public decimal? NewBalance { get; set; }
        public decimal? NewInterest { get; set; }
        public decimal? NewVat { get; set; }
        public decimal? NewPenalty { get; set; }
        public string Reason { get; set; }
        public string RequestedBy { get; set; }
    }
    public class LoanAdjustmentHistoryDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string FieldAdjusted { get; set; } // e.g., "Penalty"
        public decimal OldValue { get; set; }
        public decimal NewValue { get; set; }
        public string Reason { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
        public string ApprovalStatus { get; set; } // Approved, Rejected
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string AdjustmentRequestId { get; set; }
    }
    public class GetLoanAdjustmentRequestsDataTableQuery
    {
        public DataTableOptions Options { get; set; }

        public string BranchId { get; set; }
        public string Status { get; set; }
        public string LoanId { get; set; }
        public string CustomerId { get; set; }
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


    }

    public class LoanAdjustmentRequestDetailsDto
    {
        public LoanAdjustmentRequestDto Request { get; set; }
        public List<LoanAdjustmentHistoryDto> History { get; set; } = new List<LoanAdjustmentHistoryDto>();
    }
    public class ApproveLoanAdjustmentRequestCommand
    {
        public string RequestId { get; set; }
        public string ApprovedBy { get; set; }
    }

    public class RejectLoanAdjustmentRequestCommand
    {
        public string RequestId { get; set; }
        public string RejectedBy { get; set; }
        public string Reason { get; set; }
    }
    public class GetLoanAdjustmentRequestQuery
    {
        public string RequestId { get; set; }

        public GetLoanAdjustmentRequestQuery(string requestId)
        {
            RequestId = requestId;
        }
    }

    public class LoanAdjustmentRequestDto
    {
        public string Id { get; set; }

        [Required]
        public string LoanId { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [Display(Name = "Member Name")]
        public string CustomerName { get; set; }

        [Required]
        public string BranchId { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        [Display(Name = "Branch Code")]
        public string BranchCode { get; set; }

        // === Proposed New Values ===
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Loan amount must be a non-negative number.")]
        [Display(Name = "New Loan Amount")]
        public decimal NewLoanAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Balance must be a non-negative number.")]
        [Display(Name = "New Balance")]
        public decimal NewBalance { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Interest must be a non-negative number.")]
        [Display(Name = "New Interest")]
        public decimal NewInterest { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "VAT must be a non-negative number.")]
        [Display(Name = "New VAT")]
        public decimal NewVat { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Penalty must be a non-negative number.")]
        [Display(Name = "New Penalty")]
        public decimal NewPenalty { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "VAT Rate must be between 0 and 100.")]
        [Display(Name = "New VAT Rate (%)")]
        public decimal NewVatRate { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Interest Rate must be between 0 and 100.")]
        [Display(Name = "New Interest Rate (%)")]
        public decimal NewIntRate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Due amount must be a non-negative number.")]
        [Display(Name = "New Due Amount")]
        public decimal NewDueAmount { get; set; }

        // === Original Snapshot (for display) ===
        [Display(Name = "Old Loan Amount")]
        public decimal OldLoanAmount { get; set; }

        [Display(Name = "Old Balance")]
        public decimal OldBalance { get; set; }

        [Display(Name = "Old Interest")]
        public decimal OldInterest { get; set; }

        [Display(Name = "Old VAT")]
        public decimal OldVat { get; set; }

        [Display(Name = "Old Penalty")]
        public decimal OldPenalty { get; set; }

        [Display(Name = "Old VAT Rate (%)")]
        public decimal OldVatRate { get; set; }

        [Display(Name = "Old Interest Rate (%)")]
        public decimal OldIntRate { get; set; }

        [Display(Name = "Old Due Amount")]
        public decimal OldDueAmount { get; set; }

        public string Reason { get; set; }

        [Required]
        public string RequestedBy { get; set; }

        [Display(Name = "Requested Date")]
        public DateTime RequestedDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } // Pending, Approved, Rejected

        [Display(Name = "Approved By")]
        public string ApprovedBy { get; set; }

        [Display(Name = "Approved Date")]
        public DateTime? ApprovedDate { get; set; }

        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }
    }

}
