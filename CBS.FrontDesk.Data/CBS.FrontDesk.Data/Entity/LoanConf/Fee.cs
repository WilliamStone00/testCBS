using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class Fee
    {
        public string id { get; set; }
        public string nameOfFee { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public bool isRate { get; set; }
        public double rate { get; set; }
        public string accountingRuleId { get; set; }
        public string organizationId { get; set; }
        public string bankId { get; set; }
        public string branchId { get; set; }
    }
}
