
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionData
{
    public class AgentAccount  
    {
        public string Id { get; set; }
        public double AccountBalance { get; set; }
        public string AgentId { get; set; }
        public string BranchId { get; set; }
    }

}
