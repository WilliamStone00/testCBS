using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV
{
    public class BranchCashConfigManagement
    {
        public BranchCashConfig BranchCashConfig { get; set; }
        public BranchCashConfigDetailsVm BranchCashConfigDetailsVm { get; set; }
        public List<BranchCashConfig> BranchCashConfigs { get; set; }
        public BranchCashConfigManagement()
        {
            BranchCashConfig= new BranchCashConfig();
            BranchCashConfigDetailsVm = new BranchCashConfigDetailsVm();
            BranchCashConfigs = new List<BranchCashConfig>();
        }
    }
}
