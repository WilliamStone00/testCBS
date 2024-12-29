using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement
{
    public class CorrespondingBankConfiguration
    {
        public CorrespondingBank CorrespondingBank { get; set; } = new CorrespondingBank();
        public CorrespondingBankBranch CorrespondingBankBranch { get; set; } = new CorrespondingBankBranch();
        public List<CorrespondingBank> CorrespondingBanks { get; set; } = new List<CorrespondingBank> ();
        public List<CorrespondingBankBranchDto> CorrespondingBankBranches { get; set; } = new List<CorrespondingBankBranchDto>();

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
        public string CorrespondingBankId { get; set; }

        public string Telephone { get; set; }
        public string FocalPointName { get; set; }
        public string FocalPointContact { get; set; }
        public string FocalPointFunction { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
    }

    public class CorrespondingBank
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string InstitionType { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeLocation { get; set; }
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
}
