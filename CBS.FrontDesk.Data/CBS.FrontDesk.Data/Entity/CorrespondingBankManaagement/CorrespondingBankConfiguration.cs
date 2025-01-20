using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement
{
    public class CorrespondingBankConfiguration
    {


        public List<BankZoneBranchDto> BankZoneBranchs { get; set; } = new List<BankZoneBranchDto>();
        public BankZoneBranch BankZoneBranch { get; set; } = new BankZoneBranch();
        public List<CorrespondingBank> CorrespondingBanks { get; set; } = new List<CorrespondingBank>();
        public CorrespondingBank CorrespondingBank { get; set; } = new CorrespondingBank();
        public BankingZone BankingZone { get; set; } = new BankingZone();
        public CorrespondingBankBranch CorrespondingBankBranch { get; set; } = new CorrespondingBankBranch();
        public List<BankingZoneDto> BankingZones { get; set; } = new List<BankingZoneDto>();
        public List<CorrespondingBankBranchDto> CorrespondingBankBranches { get; set; } = new List<CorrespondingBankBranchDto>();

        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
        public BankZoneBranchObj BankZoneBranchObj { get; set; } = new BankZoneBranchObj();
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
        public string BranchName { get; set; }
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
        public object ConvertTOelementTobePosted()
        {
            return new { this.Name, this.Code, this.InstitionType, this.HeadOfficeTelephone, this.HeadOfficeLocation, this.Email };
        }
    }

    public class CorrespondingBankBranchDto
    {

        public string Id { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BranchName { get; set; }
        public string TownName { get; set; }
        public string FocalPointContact { get; set; }
        public string FocalPointName { get; set; }
    }


    public class LocationDto
    {

        public CountryDto Country { get; set; }
        public List<RegionDto> Regions { get; set; }

        public List<DivisionDto> Divisions { get; set; }
        public List<SubdivisionDto> Subdivisions { get; set; }
        public List<TownDto> Towns { get; set; }
    }

    public class RegionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CountryId { get; set; }


    }
    public class CountryDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }


    }

    public class DivisionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RegionId { get; set; }
        public string CountryId { get; set; }
    }
    public class SubdivisionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string DivisionId { get; set; }

    }
    public class TownDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubdivisionId { get; set; }


    }

    public class BankZoneBranchDto
    {
        public string Id { get; set; }
        public string BankingZoneName { get; set; } // Identifier for the associated Banking Zone
        public string Type { get; set; } // Bank or Branche
        public string BranchName { get; set; } // Identifier for the specific branch
        public string BankName { get; set; }
    }


    public class BankZoneBranch
    {
        public string Id { get; set; }
        public string BankingZoneId { get; set; } // Identifier for the associated Banking Zone
        public string Type { get; set; } // Bank or Branche
        public string BranchId { get; set; } // Identifier for the specific branch
    }
    public class BankZoneBranchObj
    {
        public string Id { get; set; }
        public string BankingZoneId { get; set; } // Identifier for the associated Banking Zone
        public string Type { get; set; } // Bank or Branche
        public List<string> BranchId { get; set; } // Identifier for the specific branch
    }



    public class BankingZone
    {
        public string Id { get; set; } // Unique identifier for the banking zone
        public string Code { get; set; } // Unique code representing the zone
        public string Name { get; set; } // Name of the banking zone
        public string LocationType { get; set; } // Type of location (e.g., Urban, Rural)
        public List<string> LocationId { get; set; } // Identifier for the location related to this zone
    }

    public class Branch3ppBranch
    {

        public string Code { get; set; } // Unique code representing the zone
        public string Name { get; set; } // Name of the banking zone
        public string Type { get; set; } // Type of location (e.g., Urban, Rural)
    }
    public class BankingZoneDto
    {
        public string Id { get; set; } // Unique identifier for the banking zone
        public string Code { get; set; } // Unique code representing the zone
        public string Name { get; set; } // Name of the banking zone
        public string LocationType { get; set; } // Type of location (e.g., Urban, Rural)
        public string LocationId { get; set; } // Identifier for the location related to this zone

    }
}
