using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMappingV2;
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
     
        public string Scope { get; set; }
        public string AffiliateId { get; set; }
        public string BranchId { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }// Pending / Approved / Rejected / etc.
        public string ChangeType { get; set; }            // CreateRequest / UpdateRequest
        public string UpdateReference { get; set; }       // Target account Id or Code (for UpdateRequest)

        // ---- Names (multilingual) ----
        public string Name { get; set; }

        // ---- Identity & meta ----
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string Class { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public bool IncludeDeleted { get; set; }
        public string Language { get; set; }
    }

    public class PendingAccountDto
    {
        public string Id { get; set; } 
        public string Scope { get; set; } 
        public string AffiliateId { get; set; }
        public string AffiliateName { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string AffiliateAccountId { get; set; }
        public string AffiliateAccountName { get; set; }
        public string AffiliateAccountCode { get; set; }
        public string HoPcmfAccountId { get; set; }
        public string HoPcmfAccountName { get; set; }
        public string HoPcmfAccountCode { get; set; }
        public string UpdateReference { get; set; }
        public string ChangeType { get; set; }// UpdateRequest Or CreateRequest

        public string CodeOld { get; set; }
        public string AccountUpdateKind { get; set; }//We can update code , Name (En or Fr or both)
        public string NameEnOld { get; set; } 
        public string NameFrOld { get; set; }
        public string Code { get; set; } 
        public string NameEn { get; set; } 
        public string NameFr { get; set; } 
        public string Class { get; set; } 
        public string Name { get; set; }  // lang-aware

        public string ParentId { get; set; }
        public string PathSeed { get; set; }
        public int? DepthSeed { get; set; }
        public string ParentCode { get; set; }


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
        public string ParentAccountNumber { get; set; }
        public string CodeOld { get; set; }
        public string AccountUpdateKind { get; set; }//We can update code , Name (En or Fr or both)
        public string NameEnOld { get; set; }
        public string NameFrOld { get; set; }

        public bool PostingAllowed { get; set; }
        public bool RequiresMapping { get; set; } = false;
        public string Notes { get; set; }
        public string Language { get; set; }
        public string UpdateReference { get; set; }
        public string ChangeType { get; set; }// UpdateRequest Or CreateRequest

        public PendingAccountRequest()
        {
            
        }

        // Constructor that converts from AffiliateAccountDto
        public PendingAccountRequest(AffiliateAccountDto affiliateAccountDto, string userLanguage=null,  string parentAccountNumber = null)
        {

            UpdateReference = affiliateAccountDto.Id;
            Scope = "Affiliate";
            IsOrigin = false;
            AccountUpdateKind = "None";
            AffiliateId = affiliateAccountDto.AffiliateId;
            BranchId = null;
            AffiliateAccountId = affiliateAccountDto.Id;
            HoPcmfAccountId = affiliateAccountDto.HoPcmfAccountId;
            Code = affiliateAccountDto.Code;
            CodeOld = affiliateAccountDto.Code;
            NameEn = affiliateAccountDto.NameEn;
            NameEnOld = affiliateAccountDto.NameEn;
            NameFr = affiliateAccountDto.NameFr;
            NameFrOld = affiliateAccountDto.NameFr;
            Class = affiliateAccountDto.Class;
            ParentId = affiliateAccountDto.ParentId;
            ParentAccountNumber = parentAccountNumber;
            PostingAllowed = affiliateAccountDto.PostingAllowed;
            RequiresMapping = false;
            Notes = $"Created from affiliate account: {affiliateAccountDto.Name}";
            Language = userLanguage ??  "en";
            ChangeType = "UpdateRequest";
        }

        public  PendingAccountRequest(BranchAccountResponse branchAccount, string userLanguage = null, bool isOrigin = false, string parentAccountNumber = null)
        {

            UpdateReference = branchAccount.Id;
            Scope = branchAccount.Scope ?? "Branch";
            IsOrigin = isOrigin;
            AccountUpdateKind = "None";
            AffiliateId = branchAccount.BranchId;
            BranchId = branchAccount.BranchId;
            AffiliateAccountId = branchAccount.AffiliateAccountId;
            HoPcmfAccountId = null;
            Code = branchAccount.Code;
            CodeOld = branchAccount.Code;
            NameEn = branchAccount.NameEn;
            NameEnOld = branchAccount.NameEn;
            NameFr = branchAccount.NameFr;
            NameFrOld = branchAccount.NameFr;
            Class = branchAccount.Class;
            ParentId = branchAccount.ParentId;
            ParentAccountNumber = null;
            PostingAllowed = branchAccount.PostingAllowed;
            RequiresMapping = branchAccount.RequiresMapping;
            Notes = branchAccount.Notes ?? $"Created from branch account: {branchAccount.Name}";
            Language = userLanguage ?? branchAccount.Language ?? "en";
            ChangeType = "UpdateRequest";

        }
    }

    public class RequestAction
    {
        public string Id { get; set; }
        public string ApprovalNotes { get; set; }
        public string RejectionReason { get; set; }
        public string Language { get; set; }
        public string ActionType { get; set; }
    }

}

