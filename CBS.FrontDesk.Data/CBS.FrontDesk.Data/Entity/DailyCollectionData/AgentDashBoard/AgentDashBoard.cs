using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionData 
{
    public class AgentDashBoard
    {
        public string kpi { get; set; }
        public string totalDailyCollection { get; set; }
        public string totalCashOut { get; set; }
        public string totalCashIn { get; set; }
        public string totalLoanRepayment { get; set; }
        public string agentId { get; set; }
        public string branchId { get; set; }
    }
}
