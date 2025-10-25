using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfig
{
    public class BranchCashConfigManagement
    {
        public BranchCashConfigDto BranchCashConfig { get; set; }
        public IEnumerable<BranchCashConfigDto> BranchCashConfigs { get; set; }
    }
}
