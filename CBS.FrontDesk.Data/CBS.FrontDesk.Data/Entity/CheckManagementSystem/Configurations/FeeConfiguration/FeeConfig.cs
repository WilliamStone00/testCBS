using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration
{
    public class FeeConfig
    {
        public string Id { get; set; }
        public bool IsCentralized { get; set; } = false;
        public string BranchId { get; set; }
        public string BranchName { get; set; } = null;
      
        public string Description { get; set; }
        public string FeeType { get; set; }            // e.g. "CheckFee"

        public bool AcceptPercentage { get; set; }
        public double? PercentageApplied { get; set; }

        public bool AcceptRange { get; set; }
        public List<Range> FeeTypeRanges { get; set; } = new List<Range>();

        public bool IsActive { get; set; } = true;
    }
}
