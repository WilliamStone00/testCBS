using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Bank
    {
        public string Id { get; set; }
        public string BankCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string OrganizationId { get; set; } // Foreign key
        public string Capital { get; set; }
        public string RegistrationNumber { get; set; }
        public string LogoUrl { get; set; }
        public string WaterMarkUrl { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string TaxPayerNUmber { get; set; }
        public string PBox { get; set; }
        public string WebSite { get; set; }
        public string DateOfCreation { get; set; }
        public string BankInitial { get; set; }
        public string Motto { get; set; }
        public string CustomerServiceContact { get; set; }
        public string CategoryInformation { get; set; }
        public string ShortHeaderInfo { get; set; }
        public string Fax { get; set; }
        public string RegistrationInformation { get; set; }
        public Organization Organization { get; set; }
        public List<Branch> Branches { get; set; }
        public List<SubDivision> Subdivisions { get; set; }
        public List<Town> Towns { get; set; }
        public Bank()
        {
            LogoUrl = "~/AppFiles/Images/noimage.jpg";
            WaterMarkUrl = "~/AppFiles/Images/noimage.jpg";

        }
    }

}
