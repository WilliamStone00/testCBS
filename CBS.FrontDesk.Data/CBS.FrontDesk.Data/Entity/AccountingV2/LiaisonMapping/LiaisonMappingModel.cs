using CBS.FrontDesk.Data.Entity.DataTable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping
{
    public class LiaisonMappingModel
    {
        public string Id { get; set; }

        public string BranchId { get; set; }

        public string BranchName { get; set; }

        public string CounterpartyBranchId { get; set; }

        public string CounterpartyBranchName { get; set; }

        public string DueFromAssetAccountId { get; set; }

        public string DueFromAssetAccountName { get; set; }

        public string DueToLiabilityAccountId { get; set; }

        public string DueToLiabilityAccountName { get; set; }
    }

    public class LiaisonMappingDto
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CounterpartyBranchId { get; set; }
        public string CounterpartyBranchName { get; set; }
        public string DueFromAssetAccountId { get; set; }
        public string DueFromAssetAccountName { get; set; }
        public string DueToLiabilityAccountId { get; set; }
        public string DueToLiabilityAccountName { get; set; }
    }

    public class GetLiaisonMappingsDataTableQuery
    {
        [JsonProperty("options")] // correspond à "options" (minuscule)
        public DataTableOptions Options { get; set; }

        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("counterpartyBranchId")]
        public string CounterpartyBranchId { get; set; }

        [JsonProperty("dueFromAssetAccountId")]
        public string DueFromAssetAccountId { get; set; }

        [JsonProperty("dueToLiabilityAccountId")]
        public string DueToLiabilityAccountId { get; set; }

        [JsonProperty("sourceBranchId")]
        public string SourceBranchId { get; set; }

        [JsonProperty("targetBranchId")]
        public string TargetBranchId { get; set; }

        [JsonProperty("createdFromUtc")]
        public DateTime? CreatedFromUtc { get; set; }

        [JsonProperty("createdToUtc")]
        public DateTime? CreatedToUtc { get; set; }

        [JsonProperty("includeDeleted")]
        public bool IncludeDeleted { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CreateOrUpdateLiaisonMappingCommand
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string SourceBranchId { get; set; }
        public string CounterpartyBranchId { get; set; }
        public string DueFromAssetAccountId { get; set; }
        public string DueToLiabilityAccountId { get; set; }
        public string TargetBranchId { get; set; }
    }

    public class BranchAccount
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string AffiliateAccountId { get; set; }
        public string AffiliateAccountName { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public bool PostingAllowed { get; set; }
        public int Depth { get; set; }
        public string Path { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public List<BranchAccount> Children { get; set; } = new List<BranchAccount>();
    }

}
