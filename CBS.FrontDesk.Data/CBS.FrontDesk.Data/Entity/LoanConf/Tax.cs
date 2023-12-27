using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
 
    public class Tax
    {
        public string id { get; set; }
        public string name { get; set; }
        public double value { get; set; }
        public string periodicity { get; set; }
        public bool isMandatory { get; set; }
        public string description { get; set; }
        public string AccountingRuleId { get; set; }
    }

}
