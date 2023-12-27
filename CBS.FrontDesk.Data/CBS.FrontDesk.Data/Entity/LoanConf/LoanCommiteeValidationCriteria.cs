using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanCommiteeValidationCriteria
    {

        public string id { get; set; }
        public string loanProductId { get; set; }
        public int committeeSize { get; set; }
        public int acceptedValueForApproval { get; set; }
        public bool canBeOverriddenBySupervisor { get; set; }
        public string supervisorId { get; set; }
        public int daysForAlert { get; set; }
    }

}
