using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Queries
{
   

    public class AccountingV2ReportsFilterDemo2
    {
        [JsonProperty("dateFrom")]
        public string DateFrom { get; set; } = null;

        [JsonProperty("dateTo")]
        public string DateTo { get; set; } = null;

        [JsonProperty("branchId")]
        public string BranchId { get; set; } = null;

        [JsonProperty("consolidated")]
        public bool Consolidated { get; set; } = true;

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; } = null;

        [JsonProperty("accountNumbers")]
        public List<string> AccountNumbers { get; set; } = new List<string>();


        [JsonProperty("selectedBranchIds")]
        public List<string> SelectedBranchIds { get; set; }
            = new List<string> { "717373842079233" };

        [JsonProperty("excludeLiaisonInternal")]
        public bool ExcludeLiaisonInternal { get; set; } = true;

        [JsonProperty("sourceMode")]
        public string SourceMode { get; set; } = "Temp"; // 👈 Correct capital T

        [JsonProperty("language")]
        public string Language { get; set; } = "en";

        [JsonProperty("paging")]
        public Paging2 Paging { get; set; } = new Paging2();

        [JsonProperty("sorting")]
        public Sorting2 Sorting { get; set; } = new Sorting2();
    }

    public class Paging2
    {
        [JsonProperty("page")]
        public int Page { get; set; } = 0;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 0;
    }

    public class Sorting2
    {
        [JsonProperty("sortBy")]
        public string SortBy { get; set; } = "string";

        [JsonProperty("sortDir")]
        public string SortDir { get; set; } = "string";
    }

}
