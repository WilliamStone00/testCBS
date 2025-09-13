using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanProductCollateral
    {

        public string Id { get; set; }
        public string CollateralId { get; set; }
        public string LoanProductId { get; set; }
        public string LoanProductCollateralTag { get; set; }
        public decimal MinimumValueRate { get; set; }
        public decimal MaximumValueRate { get; set; }
        public Collateral Collateral { get; set; }
        public LoanProduct LoanProduct { get; set; }
    }
}
