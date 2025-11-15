using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.FileUpload
{
    public class AffiliateFileUploadResponse
    {
        public int? TotalRows { get; set; }
        public int? ProcessedRows { get; set; }
        public int? SkippedRows { get; set; }
        public int? TotalAffiliateAccounts { get; set; }
        public int? AutoMatchedAccounts { get; set; }
        public int? RequiresMappingAccounts { get; set; }
    }

    public class BranchFileUploadResponse
    {
        public int? TotalRows { get; set; }
        public int? ProcessedRows { get; set; }
        public int? SkippedRows { get; set; }
        public int? totalBranchAccounts { get; set; }
        public int? branchAccountsCreated { get; set; }
        public int? branchAccountsUpdated { get; set; }
        public int? branchAccountsRequiresMapping { get; set; }
        public int? trialBalanceStagingCreated { get; set; }
        public int? awaitingCorrespondenceCreated { get; set; }
    }

    public class FileUpload
    {
        public bool isAffiliate { get; set; } 
        public string affiliateId { get; set; } = null;
        public string branchId { get; set; } = null;
        public HttpPostedFileBase file { get; set; }
    }

    public class AccountwaitingCorrespondanceQuery
    {
        public DataTableOptions Options { get; set; }
        
        public string Scope { get; set; }
        public string BranchId { get; set; }
        public string AffiliateId { get; set; }
        public string Code { get; set; }
        public string Class { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public bool IncludeDeleted { get; set; } = false;
        public string Language { get; set; }
    }

    public class Accountwaiting
    {
        public string Id { get; set; }
        public string Scope { get; set; }
        public string BranchId { get; set; }
        public string AffiliateId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }
        public string Name { get; set; }
        public string HoPcmfAccountId { get; set; }
        public string AffiliateToHoCorrespondenceId { get; set; }
        public bool RequiresMapping { get; set; }
        public string AffiliateAccountId { get; set; }
        public string BranchAccountId { get; set; }
        public string Path { get; set; }
        public int Depth { get; set; }
        public bool PostingAllowed { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string Language { get; set; } = null;
    }

    public class AddCORRESPONDANCE
    {
        public string Scope { get; set; }
        public string Type { get; set; }
        public string BranchId { get; set; }
        public string Name { get; set; }
        public string BranchAccountId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string SourceAffiliateAccountId { get; set; }
        public string HoPcmfAccountId { get; set; }
        public string Language { get; set; }
        public string RequestedByName { get; set; }
        public string RequestedByUserId { get; set; }
    }


    public class AF_BRCorrespondance
    {
        //AffiliateAccount
        public string HoPcmfAccountId { get; set; }
        // branchAcccount 
        public string BranchAccountId { get; set; }
        public string branchId { get; set; }

        // used for both
        public string AffiliateAccountId { get; set; }
        public string Rationale { get; set; }
        public string Language { get; set; }
    }

    public class Affiliate_Correspondance
    {
        public string HoPcmfAccountId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string Rationale { get; set; }
        public string Language { get; set; }
    }

    public class Branch_BRCorrespondance
    {
        public string BranchAccountId { get; set; }
        public string branchId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string Rationale { get; set; }
        public string Language { get; set; }
    }

    
    public class CorrespondanceRequestQuery
    {
        public DataTableOptions Options { get; set; }


        public string Type { get; set; }
        public string BranchId { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public string CorrelationId { get; set; }
        public bool IncludeDeleted { get; set; }
        public string Language { get; set; }
    }



    public sealed class CorrespondenceRequestDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Rationale { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public string RequestedByUserId { get; set; }
        public string RequestedByName { get; set; }
        public string ActionedByUserId { get; set; }
        public string ActionedByName { get; set; }
        public DateTime? ActionedDate { get; set; }
        public string RejectionReason { get; set; }

        public string BranchId { get; set; }
        public string BranchAccountId { get; set; }
        public string AffiliateAccountId { get; set; }
        public string SourceAffiliateAccountId { get; set; }
        public string HoPcmfAccountId { get; set; }

        public string BranchAccountCode { get; set; }
        public string BranchAccountName { get; set; }
        public string AffiliateAccountCode { get; set; }
        public string AffiliateAccountName { get; set; }
        public string SourceAffiliateAccountCode { get; set; }
        public string SourceAffiliateAccountName { get; set; }
        public string HoPcmfAccountCode { get; set; }
        public string HoPcmfAccountName { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }

    //A = approve & R = reject
    public class CorrespondenceApproveAndRejection
    {
        /// <summary>
        /// Correspondence request identifier.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Optional note / comment for the approval or rejection.
        /// (Shown in Swagger as "note")
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Preferred language code (e.g. "en", "fr").
        /// (Shown in Swagger as "language")
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Optional RejectionReason / comment for the approval or rejection.
        /// (Shown in Swagger as "rejectionReason")
        /// </summary>
        public string RejectionReason { get; set; }

        public string RequestBy { get; set; }
        public string Type { get; set; }// Approval or Rejection

        /*    /// <summary>
            /// User Id of the staff who actioned the correspondence.
            /// Filled server-side from the auth token.
            /// </summary>
            [JsonIgnore]
            public string ActionedByUserId { get; set; }

            /// <summary>
            /// Display name of the staff who actioned the correspondence.
            /// Filled server-side from the auth token.
            /// </summary>
            [JsonIgnore]
            public string ActionedByName { get; set; }*/
    }

}
