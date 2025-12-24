using CBS.FrontDesk.Data.Entity.DataTable;
using Newtonsoft.Json;
using System;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem
{
    public class CategoryConfig
    {
        public string id { get; set; } = null;
        public string branchID { get; set; } = null;
        public string bankId { get; set; } = null;
        public string name { get; set; }  
        public decimal issuanceFee { get; set; }
        public int? numberOfPages { get; set; }
        public int numberofCheckBooks { get; set; }
        public int validityPeriodInMonths { get; set; } 
        public int issuanceLimitPerCustomerType { get; set; }
		public decimal? maxTransactionAmount { get; set; }
		public bool isActive { get; set; } = false;
        public bool isCentralised { get; set; }
        public int? maxIssuancePerYear { get; set; }
        public decimal? renewalFee { get; set; }
        public decimal? basePrice { get; set; }
        public string modifiedBy { get; set; }
        public string modifiedDate { get; set; }
        public string createdDate { get; set; }
        public string createdBy { get; set; }

    }

    public class CategoryConfigQuery
    {
        public DataTableOptions Options { get; set; }
        public CategoryConfigQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string Name { get; set; }
        public bool IsCentralised { get; set; }
        public decimal BasePrice { get; set; }
        public int NumberOfPages { get; set; }
        public int ValidityPeriodInMonths { get; set; }
        public int IssuanceLimitPerCustomerType { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CategoryDataTableRequest
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("BranchId")]
        public string BranchId { get; set; }

        [JsonProperty("DataTableOptions")]
        public DataTableOptions DataTableOptions { get; set; }
    }

    //public class DataTableOptionss
    //{
    //    [JsonProperty("draw")]
    //    public int draw { get; set; }

    //    [JsonProperty("start")]
    //    public int start { get; set; }

    //    [JsonProperty("length")]
    //    public int length { get; set; }

    //    [JsonProperty("sortColumnName")]
    //    public string sortColumnName { get; set; }

    //    [JsonProperty("sortColumnDirection")]
    //    public string sortColumnDirection { get; set; }
    //}
}
