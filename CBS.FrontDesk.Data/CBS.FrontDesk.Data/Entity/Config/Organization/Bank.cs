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
        [Required]
        public string BankCode { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string Telephone { get; set; }
        [Required]
        public string Email { get; set; }
        public string Address { get; set; }
        public string Capital { get; set; }
        public string RegistrationNumber { get; set; }
        public string SignatureURL { get; set; }
        public string LogoUrl { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string TaxPayerNUmber { get; set; }
        public string PBox { get; set; }
        public string WebSite { get; set; }
        public string DateOfCreation { get; set; }
        public string BankInitial { get; set; }
        public string Motto { get; set; }
        public string OrganizationId { get; set; } 
        public Organization Organization { get; set; }
        public List<Branch> Branches { get; set; }
        public List<SubDivision> Subdivisions { get; set; }
        public List<Town> Towns { get; set; }
        public Bank()
        {
            LogoUrl = "~/AppFiles/Images/noimage.jpg";

        }
    }

}
