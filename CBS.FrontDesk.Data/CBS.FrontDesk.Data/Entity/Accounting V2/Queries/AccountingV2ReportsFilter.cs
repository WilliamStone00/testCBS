using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
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

        [JsonProperty("zeroInclusive")]
        public bool ZeroInclusive { get; set; }

        [JsonProperty("includeZeroGlAccount")]
        public bool IncludeZeroGlAccount { get; set; }
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




    public class ReceiptV2Filter
    {
        [JsonPropertyName("receiptId")]
        public string ReceiptId { get; set; } = null;

        [JsonPropertyName("journalHeaderId")]
        public string JournalHeaderId { get; set; } = null;



        [JsonPropertyName("reference")]
        public string Reference { get; set; } = null;

        [JsonPropertyName("receiptNumber")]
        public string ReceiptNumber { get; set; } = null;

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = "en";

        [JsonPropertyName("fromTemp")]
        public bool FromTemp { get; set; } = true;
    }
}
