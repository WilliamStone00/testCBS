using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Queries
{

    public class AccountingV2ReportsFilter
    {
        [JsonProperty("from")]
        public DateTime From { get; set; }

        [JsonProperty("to")]
        public DateTime To { get; set; }

        [JsonProperty("dateFrom")]
        public DateTime DateFrom { get; set; }

        [JsonProperty("dateTo")]
        public DateTime DateTo { get; set; }

        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("consolidated")]
        public bool Consolidated { get; set; }

        [JsonProperty("selectedBranchIds")]
        public List<string> SelectedBranchIds { get; set; }

        [JsonProperty("excludeLiaisonInternal")]
        public bool ExcludeLiaisonInternal { get; set; }

        [JsonProperty("sourceMode")]
        public string SourceMode { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("accountNumbers")]
        public List<string> AccountNumbers { get; set; }

        [JsonProperty("accountingYear")]
        public string AccountingYear { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }

        [JsonProperty("sorting")]
        public Sorting Sorting { get; set; }
    }

    public class Paging
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

    public class Sorting
    {
        [JsonProperty("sortBy")]
        public string SortBy { get; set; } = null;

        [JsonProperty("sortDir")]
        public string SortDir { get; set; } = null;
    }

}
