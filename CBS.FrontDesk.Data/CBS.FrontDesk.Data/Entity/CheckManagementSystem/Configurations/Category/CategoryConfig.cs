using CBS.FrontDesk.Data.Entity.DataTable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem
{
    public class CategoryConfig
    {
        public string Id { get; set; } = null;
        public string branchID { get; set; } = null;
        public string name { get; set; }  = string.Empty; 
        public decimal basePrice { get; set; } 
        public int numberOfPages { get; set; } 
        public int validityPeriodInMonths { get; set; } 
        public int issuanceLimitPerCustomerType { get; set; }
        public bool isActive { get; set; } = false;
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
