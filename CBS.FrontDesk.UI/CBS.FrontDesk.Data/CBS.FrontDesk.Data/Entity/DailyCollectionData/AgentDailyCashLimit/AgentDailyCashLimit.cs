using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionData
{
    public class AgentDailyCashLimit
    {
        public string Id { get; set; }
        public string cashIn { get; set; }
        public string cashOut { get; set; }
        public string agentId { get; set; }
        public string branchId { get; set; }
        public bool Isglobal { get; set; }

        public bool IsAgent { get; set; }
        public bool IsBranch { get; set; }
    }
    public class AgentDailyCashLimitDto
    {
        public string cashIn { get; set; }
        public string cashOut { get; set; }
        public string agentId { get; set; }
        public string branchId { get; set; }
        public string agentNames { get; set; }
        public string branchNames { get; set; }
    }
}
