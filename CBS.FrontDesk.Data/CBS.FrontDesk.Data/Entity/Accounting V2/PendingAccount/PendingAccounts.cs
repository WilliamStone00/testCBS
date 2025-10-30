using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.PendingAccount
{
    public class PendingAccountQuery
    {
        public DataTableOptions Options { get; set; }
        public PendingAccountQuery() { Options = new DataTableOptions(); }
        public string Scope { get; set; }
        public string AffiliateId { get; set; }
        public string BranchId { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public string Class { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public bool? IncludeDeleted { get; set; }
        public string Language { get; set; }
    }

    public class PendingAccountDto
    {
        public string Id { get; set; }
        public string Scope { get; set; }
        public string AffiliateId { get; set; }
        public string BranchId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string HoPcmfAccountId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string PathSeed { get; set; }
        public int? DepthSeed { get; set; }
        public bool PostingAllowed { get; set; }
        public bool RequiresMapping { get; set; }
        public string Reference { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public string RequestedBy { get; set; }
        public DateTime? RequestedAtUtc { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string RejectedBy { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public string RejectionReason { get; set; }
        public string Notes { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string Language { get; set; }
    }

    public class PendingAccountRequest
    {
        public string Id { get; set; }
        public string Scope { get; set; }
        public bool IsOrigin { get; set; } = false;
        public string AffiliateId { get; set; }
        public string BranchId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string HoPcmfAccountId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }
        public string ParentId { get; set; }
        public bool PostingAllowed { get; set; }
        public bool RequiresMapping { get; set; } = false;
        public string Notes { get; set; }
        public string Language { get; set; }
    }

    public class requestAction
    {
        public string Id { get; set; }
        public string ApprovalNotes { get; set; }
        public string RejectionReason { get; set; }
        public string Language { get; set; }
        public string  ActionType { get; set; }
    }

}

