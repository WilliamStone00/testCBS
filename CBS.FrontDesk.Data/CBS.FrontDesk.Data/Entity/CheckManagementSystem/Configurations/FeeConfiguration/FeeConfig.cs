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
}
