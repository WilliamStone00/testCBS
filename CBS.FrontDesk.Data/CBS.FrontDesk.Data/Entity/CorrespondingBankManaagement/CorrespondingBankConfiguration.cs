using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement
{
    public class CorrespondingBankConfiguration
    {
        public CorrespondingBank CorrespondingBank { get; set; }
        public CorrespondingBankBranch CorrespondingBankBranch { get; set; }
        public List<CorrespondingBank> CorrespondingBanks { get; set; }
        public List<CorrespondingBankBranchDto> CorrespondingBankBranches { get; set; }

        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";

    }

    public class CorrespondingBankBranch
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
       
            public string Id { get; set; }
            public string RegionId { get; set; }
            public string DivisionId { get; set; }
            public string SubdivisionId { get; set; }
            public string TownId { get; set; }
            public string ThirdPartyInstitutionId { get; set; }
            public string TownName { get; set; }
            public string FocalPointContact { get; set; }
            public string FocalPointName { get; set; }
   
    }

    public class CorrespondingBank
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string InstitionType { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeLocation { get; set; }
        public string Email { get; set; }
    }
 
    public class CorrespondingBankBranchDto
    {
    }
}
