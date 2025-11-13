using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Queries
{

    public class AccountingV2ReportsFilter
    {
        [JsonProperty("from")]
        public DateTime From { get; set; } = DateTime.Parse("2025-09-01");

        [JsonProperty("to")]
        public DateTime To { get; set; } = DateTime.Parse("2025-11-30");

        [JsonProperty("branchId")]
        public string BranchId { get; set; } = "717373842079233";

        [JsonProperty("consolidated")]
        public bool Consolidated { get; set; } = false;

        [JsonProperty("selectedBranchIds")]
        public List<string> SelectedBranchIds { get; set; } = null;

        [JsonProperty("excludeLiaisonInternal")]
        public bool ExcludeLiaisonInternal { get; set; } = true;

        [JsonProperty("sourceMode")]
        public string SourceMode { get; set; } = "temp"; // ✅ lowercase to match API

        [JsonProperty("language")]
        public string Language { get; set; } = "en";

        [JsonProperty("search")]
        public string Search { get; set; } = "";

        [JsonProperty("paging")]
        public Paging Paging { get; set; } = new Paging();

        [JsonProperty("sorting")]
        public Sorting Sorting { get; set; } = new Sorting();

        [JsonProperty("accounts")]
        public List<string> Accounts { get; set; } = new List<string>();
    }

    public class Paging
    {
        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 100;
    }

    public class Sorting
    {
        [JsonProperty("sortColumn")]
        public string SortColumn { get; set; } = "AccountNumber";

        [JsonProperty("sortDirection")]
        public string SortDirection { get; set; } = "asc";
    }
}
