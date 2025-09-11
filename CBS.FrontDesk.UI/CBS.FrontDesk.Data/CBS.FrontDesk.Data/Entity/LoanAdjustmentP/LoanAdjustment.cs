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

        // === New Values to Apply ===
        public decimal? NewLoanAmount { get; set; }
        public decimal? NewBalance { get; set; }
        public decimal? NewInterest { get; set; }
        public decimal? NewVat { get; set; }
        public decimal? NewPenalty { get; set; }

        public decimal? NewVatRate { get; set; }
        public decimal? NewIntRate { get; set; }

        public DateTime? NewLoanDate { get; set; }
        public DateTime? NewDisbursementDate { get; set; }
        public string NewLoanStatus { get; set; }
        public DateTime? NewNextInstallmentDate { get; set; }
        public decimal NewPaid { get; set; }

        // === Old Values for Audit/Comparison ===
        public decimal OldLoanAmount { get; set; }
        public decimal OldBalance { get; set; }
        public decimal OldInterest { get; set; }
        public decimal OldVat { get; set; }
        public decimal OldPenalty { get; set; }

        public decimal OldVatRate { get; set; }
        public decimal OldIntRate { get; set; }

        public DateTime OldLoanDate { get; set; }
        public DateTime OldDisbursementDate { get; set; }
        public string OldLoanStatus { get; set; }
        public DateTime OldNextInstallmentDate { get; set; }
        public decimal OldPaid { get; set; }

        // === Justification ===
        public string Reason { get; set; }
        public string RequestedBy { get; set; }

        public DateTime? RequestDate { get; set; }
    }
    public class LoanAdjustmentHistoryDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string FieldAdjusted { get; set; } // e.g., "Penalty"

        public string OldValue { get; set; } // ✅ Changed from decimal to string
        public string NewValue { get; set; } // ✅ Changed from decimal to string

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
        [Required]
        public string Id { get; set; }

        [Required]
        public string LoanId { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required]
        public string BranchId { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        [Display(Name = "Branch Code")]
        public string BranchCode { get; set; }

        // 🔹 Proposed New Values
        [Display(Name = "New Loan Amount")]
        public decimal NewLoanAmount { get; set; }

        [Display(Name = "New Balance")]
        public decimal NewBalance { get; set; }

        [Display(Name = "New Interest")]
        public decimal NewInterest { get; set; }

        [Display(Name = "New VAT")]
        public decimal NewVat { get; set; }

        [Display(Name = "New Penalty")]
        public decimal NewPenalty { get; set; }

        [Display(Name = "New VAT Rate (%)")]
        public decimal NewVatRate { get; set; }

        [Display(Name = "New Interest Rate (%)")]
        public decimal NewIntRate { get; set; }

        [Display(Name = "New Due Amount")]
        public decimal NewDueAmount { get; set; }

        [Display(Name = "New Loan Date")]
        [DataType(DataType.Date)]
        public DateTime? NewLoanDate { get; set; }

        [Display(Name = "New Disbursement Date")]
        [DataType(DataType.Date)]
        public DateTime? NewDisbursementDate { get; set; }

        [Display(Name = "New Loan Status")]
        public string NewLoanStatus { get; set; }

        [Display(Name = "New Next Installment Date")]
        [DataType(DataType.Date)]
        public DateTime? NewNextInstallmentDate { get; set; }

        [Display(Name = "New Total Paid")]
        public decimal NewPaid { get; set; }

        // 🔸 Original Snapshot (for audit/log)
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

        [Display(Name = "Old Loan Date")]
        [DataType(DataType.Date)]
        public DateTime? OldLoanDate { get; set; }

        [Display(Name = "Old Disbursement Date")]
        [DataType(DataType.Date)]
        public DateTime? OldDisbursementDate { get; set; }

        [Display(Name = "Old Loan Status")]
        public string OldLoanStatus { get; set; }

        [Display(Name = "Old Next Installment Date")]
        [DataType(DataType.Date)]
        public DateTime? OldNextInstallmentDate { get; set; }

        [Display(Name = "Old Total Paid")]
        public decimal? OldPaid { get; set; }

        // 🔐 Request Info
        [Required]
        [MinLength(10, ErrorMessage = "Reason must be at least 10 characters.")]
        public string Reason { get; set; }

        [Required]
        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Display(Name = "Requested Date")]
        [DataType(DataType.DateTime)]
        public DateTime RequestedDate { get; set; }

        // 📝 Approval Info
        [Display(Name = "Request Status")]
        public string Status { get; set; } // Pending, Approved, Rejected

        [Display(Name = "Approved By")]
        public string ApprovedBy { get; set; }

        [Display(Name = "Approval Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ApprovedDate { get; set; }

        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }
    }

}
