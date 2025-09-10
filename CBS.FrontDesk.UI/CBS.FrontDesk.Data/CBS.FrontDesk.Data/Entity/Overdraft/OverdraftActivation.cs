using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Overdraft
{
    public class OverdraftActivation
    {
        public string Id { get; set; }

        // ================================
        // 🧾 Account & Customer Info
        // ================================
        public string AccountId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }

        // ================================
        // 🏦 Branch Metadata
        // ================================
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }

        // ================================
        // 📌 Overdraft Config Reference
        // ================================
        public string OverdraftConfigId { get; set; }            // ✅ ID of the config used for validation

        // ================================
        // 💰 Overdraft Terms
        // ================================
        public decimal RequestedLimit { get; set; }

        public bool FlatInterestRate { get; set; }
        public bool PayAsYouGoInterest { get; set; }
        public decimal InterestRate { get; set; }

        public decimal PenaltyRate { get; set; }

        public string LinkedSalaryAccountId { get; set; }
        public List<string> RecoverySourceAccountIds { get; set; }

        public bool AutoRecoverOnDeposit { get; set; } = true;
        public bool IsSalaryOverdraftAccount { get; set; }

        // ================================
        // 🔁 Auto Provisioning
        // ================================
        public bool AutoProvisionEnabled { get; set; } = false;
        public int? AutoProvisionFrequencyInDays { get; set; }

        // ================================
        // 🚦 Workflow Status & Approval
        // ================================
        public string Status { get; set; }

        public string RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string RejectionReason { get; set; }
    }
    public class ApproveOverdraftServiceCommand
    {
        public string ActivationRequestId { get; set; }

        public OverdraftServiceActivationStatus Status { get; set; }
        // ✅ Status to set: Approved or Rejected

        public string Comment { get; set; }
        // ✅ Required when status is Rejected (used as rejection reason)

        public string ApprovedBy { get; set; }
        // ✅ Officer or system user approving/rejecting

        // ⏱️ Approval timestamp will be managed inside the handler
    }
    public enum OverdraftServiceActivationStatus
    {
        Pending,
        Approved,
        Rejected,
        Activated
    }
    public class GetOverdraftActivationRequestsDataTableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }

        // Optional filters
        public OverdraftServiceActivationStatus? Status { get; set; }
        public string BranchId { get; set; }
        public string CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
    public class SubmitOverdraftActivationRequestCommand
    {
        [Required(ErrorMessage = "Account ID is required.")]
        public string AccountId { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(100, ErrorMessage = "Customer Name cannot exceed 100 characters.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Branch ID is required.")]
        public string BranchId { get; set; }

        [Required(ErrorMessage = "Branch Code is required.")]
        public string BranchCode { get; set; }

        [Required(ErrorMessage = "Branch Name is required.")]
        public string BranchName { get; set; }

        [Required(ErrorMessage = "Overdraft Facility Config ID is required.")]
        public string OverdraftFacilityConfigId { get; set; }

        [StringLength(50, ErrorMessage = "Customer segment cannot exceed 50 characters.")]
        public string CustomerSegment { get; set; }

        [Required(ErrorMessage = "Requested limit is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Requested limit must be greater than zero.")]
        public decimal RequestedLimit { get; set; }

        public bool FlatInterestRate { get; set; }

        public bool PayAsYouGoInterest { get; set; }

        [Required(ErrorMessage = "Interest rate is required.")]
        [Range(0.01, 100.0, ErrorMessage = "Interest rate must be between 0.01 and 100.")]
        public decimal InterestRate { get; set; }

        [Range(0, 100.0, ErrorMessage = "Penalty rate must be between 0 and 100.")]
        public decimal PenaltyRate { get; set; }

        [StringLength(50, ErrorMessage = "Linked salary account number cannot exceed 50 characters.")]
        public string LinkedSalaryAccountId { get; set; }

        public List<string> RecoverySourceAccountIds { get; set; }

        public bool AutoRecoverOnDeposit { get; set; } = true;

        public bool IsSalaryOverdraftAccount { get; set; }

        public bool AutoProvisionEnabled { get; set; } = false;

        [Range(1, 365, ErrorMessage = "Provision frequency must be between 1 and 365 days.")]
        public int? AutoProvisionFrequencyInDays { get; set; }

        [Required(ErrorMessage = "Proposed expiry date is required.")]
        [DataType(DataType.Date)]
        public DateTime ProposedExpiryDate { get; set; }

        [Required(ErrorMessage = "Average salary amount is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Average salary must be greater than zero.")]
        public int AverageSalaryAmount { get; set; }

        public string Id { get; set; }
    }

}
