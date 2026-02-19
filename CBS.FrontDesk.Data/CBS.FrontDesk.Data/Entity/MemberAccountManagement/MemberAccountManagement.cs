using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAccountManagement
{

    public class ManageAccountStatusCommand
    {
        public string Id { get; set; }

        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }

        public string MemberName { get; set; }
        public string MemberReference { get; set; }

        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        public AccountManagementAction Action { get; set; }
        public string Status { get; set; }

        public string RequestedByName { get; set; }
        public DateTime? RequestedOn { get; set; }

        public string DecidedByName { get; set; }
        public DateTime? DecidedOn { get; set; }

        public DateTime? AppliedOn { get; set; }
        public DateTime? EffectiveDate { get; set; }

        public string RequestReason { get; set; }
        public string DecisionNote { get; set; }
        public string BlockReason { get; set; }

        public decimal Amount { get; set; }

        public string ApplyError { get; set; }
        public string TraceLog { get; set; }
        public string CorrelationId { get; set; }
    }
    public class Decission
    {
        public string WorkflowId { get; set; }
        public string Decision { get; set; }
        public string DecisionNote { get; set; }

    }

    public enum AccountManagementAction
    {
        Activate,
        Freeze,
        Unfreeze,

        BlockFull,
        UnblockFull,

        BlockPartial,
        UnblockPartial,

        Close,
        Reopen,

        MarkDormant,
        ReactivateDormant,

        // --- Operation-level control ---
        BlockCashIn,
        OpenCashIn,

        BlockWithdrawal,
        OpenWithdrawal,

        BlockTransfer,
        OpenTransfer
    }


    public enum AccountWorkflowStatus
    {
        Draft,
        PendingApproval,
        Approved,
        Rejected,
        Cancelled,
        Applied,
        Failed
    }

    public class ManageAccountmanaQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public ManageAccountmanaQuery() { DataTableOptions = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string Status { get; set; }
        public string  MemberReference { get; set; }
        public string  Action { get; set; }
    }

}
