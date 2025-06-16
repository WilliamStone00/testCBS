using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionData
{

    public class CommissionSetting
    {
        public string Id { get; set; }
        public string OperationType { get; set; } //SUB,DEP,WDR,LRP
        public bool IsProportional { get; set; } // Determine where the commission is shared in a give ratio or is a fixed rate
        public int AmountPerTransaction { get; set; } // If IsProportional then contains the amount to be given to the agent
        public string ProviderType { get; set; }   // can be any of the above DestinationBranch,League,AgentBranch,HeadOffice
        public double CommissionProviderShare { get; set; }
        public double AgentShare { get; set; }
        public string BranchId { get; set; }// contains a value if scope is not global
        public string AgentId { get; set; }// contains a value if scope Agent
        public bool IsInterbranch { get; set; } // Indicate if the commission is done from an interbranch transaction

        public bool IsGlobal { get; set; } //  Determine the scope of the configuration
        public bool IsBranch { get; set; } //  Determine the scope of the configuration
        public bool IsAgent { get; set; } //  Determine the scope of the configuration
    }
    public class CommissionSettingDto
    {
        public string Id { get; set; }
        public string AgentShare { get; set; }
        public string BranchShare { get; set; }
        public string LeagueShare { get; set; }
        public string ServiceProviderShare { get; set; }
        public string BranchId { get; set; }
        public string AgentId { get; set; }
        public string AgentNames{ get; set; }
        public string BranchName { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsBranch { get; set; }
    }
}
