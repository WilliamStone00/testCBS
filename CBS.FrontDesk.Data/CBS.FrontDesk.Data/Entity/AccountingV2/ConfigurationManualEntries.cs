using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2
{
    //public class ConfigurationManualEntries
    //{
    //    public string Id { get; set; }

    //    public string Type { get; set; } // Local or Inter Branch
    //    public string Command { get; set; } // Auto, Auto Approval, Auto Approval Source, Auto Approval Source Destination
    //    public string Description { get; set; }

    //    public bool UserRequiredApproval { get; set; } // Whether the user who initiated must approve
    //    public bool StatusIsActive { get; set; } // True = Active, False = Inactive
    //}
    public class ConfigurationManualEntries
    {
        public string Id { get; set; }
        public string Type { get; set; } // Local or Inter Branch
        public string Command { get; set; } // Auto, Auto Approval, Auto Approval Source, Auto Approval Source Destination
        public string Description { get; set; }
        public bool UserRequiredApproval { get; set; } // Whether the user who initiated must approve
        public string Status { get; set; } // "Active" or "Inactive"
    }

    public class ConfigurationRoot
    {
        public List<ConfigurationManualEntries> ConfigurationManualEntries { get; set; }
    }

}
