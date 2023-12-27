using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanDeliquencyConfiguration
    {
        public string Id { get; set; }
        public int DaysFrom { get; set; }
        public int DaysTo { get; set; }
        public string Status { get; set; }//Normal,Bad loan, Due loan, Over due loan, unracoverable loan, Write off loan
        public string Name { get; set; }// Par 0-30, Par 31-60, Par 61-90, Par 91-120
        public string ActionToPerform { get; set; }//Send email, Call customer, Call recovery agent, Others
        public bool SendSMStoClient { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string OrganizationId { get; set; }
        public string AccountingRuleId { get; set; }
    }
}
