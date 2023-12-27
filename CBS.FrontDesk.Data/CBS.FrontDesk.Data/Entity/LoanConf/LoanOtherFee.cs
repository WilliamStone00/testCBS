using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanOtherFee
    {
        public string id { get; set; }
        public string otherFeeId { get; set; }
        public string loanId { get; set; }
        public string description { get; set; }
        public double value { get; set; }
        public string organizationId { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }
    }

}
