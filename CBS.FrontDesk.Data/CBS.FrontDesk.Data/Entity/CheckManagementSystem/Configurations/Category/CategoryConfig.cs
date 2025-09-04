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
        public string BranchID { get; set; }
        public string Name { get; set; } = string.Empty; // Unique per institution
        public decimal BasePrice { get; set; } // >= 0     
        public int NumberOfPages { get; set; } // 25, 50, 100 etc.
        public int ValidityPeriodInMonths { get; set; } // 12, 24 etc.
        public int IssuanceLimitPerCustomerType { get; set; } // per customer type
        public bool IsActive { get; set; } = true; // active/inactive toggle
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
