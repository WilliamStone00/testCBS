using CBS.BusinessService.ThirdPartyBankAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{
    public class ThirdPartyBankConfiguration
    {
        public ThirdPartyBranch ThirdPartyBranch { get; set; } = new ThirdPartyBranch();
        public ThirdPartyInstitution ThirdPartyInstitution { get; set; } = new ThirdPartyInstitution();
        public List<ThirdPartyBranch> ThirdPartyBranchs { get; set; } = new List<ThirdPartyBranch>();
        public List<ThirdPartyInstitution> ThirdPartyInstitutions { get; set; } = new List<ThirdPartyInstitution>();
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
    }
}
