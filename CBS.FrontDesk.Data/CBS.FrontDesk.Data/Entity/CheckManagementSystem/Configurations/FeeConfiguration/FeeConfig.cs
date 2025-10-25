using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration
{
    public class FeeConfig
    {
        public string id { get; set; } = null;
        public bool isCentralized { get; set; } = false;
        public string branchId { get; set; }
        public string branchName { get; set; } = null;
        public string branchCode { get; set; } = null;


        public string description { get; set; } = null;
        public string feeType { get; set; }            // e.g. "CheckFee"

        public bool acceptPercentage { get; set; }
        public double? percentageApplied { get; set; }

        public bool acceptStaticAmount { get; set; }
        public double? staticAmount { get; set; }

        public bool acceptRange { get; set; }
        public List<Range> feeTypeRanges { get; set; } = new List<Range>();



        public bool IsActive { get; set; } = true;
    }

    public class FeeConfigQuery
    {
        public DataTableOptions Options { get; set; }
        public FeeConfigQuery() { Options = new DataTableOptions(); }

        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string FeeType { get; set; }

        public bool? AcceptStaticAmount { get; set; }
        public bool? AcceptPercentage { get; set; }
        public bool? AcceptRange { get; set; }
        public bool? IsCentralized { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
