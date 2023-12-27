using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanCommentry
    {
        public string id { get; set; }
        public string loanId { get; set; }
        public string comment { get; set; }
        public string organizationId { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }

    }

}
