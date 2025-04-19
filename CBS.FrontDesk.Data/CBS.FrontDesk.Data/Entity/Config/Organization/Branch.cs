using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Data.Entity.Migrations.Model.UpdateDatabaseOperation;
using System.Resources;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Branch
    {
        public string Id { get; set; }

        public bool IsHavingBank { get; set; } //""
       [Required]
        public string BranchCode { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Telephone { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        [Required]
        public string Address { get; set; }
        public string BankId { get; set; }
        public string Capital { get; set; }
        [Required]
        public string RegistrationNumber { get; set; }


        public string LogoUrl { get; set; }
        [Required]
        public string ImmatriculationNumber { get; set; }
        [Required]
        public string TaxPayerNUmber { get; set; }
        public bool ActiveStatus { get; set; }
        public bool IsHeadOffice { get; set; }
        [Required]
        public string PBox { get; set; }
        [Required]
        public string WebSite { get; set; }
        [Required]
        public string DateOfCreation { get; set; }
        [Required]
        public string BankInitial { get; set; }
        public string Motto { get; set; }
        [Required]
        public string HeadOfficeTelehoneNumber { get; set; }
        public string ImageVirtualPath { get; set; }
        [Required]
        public string HeadOfficeAddress { get; set; }
        public Bank Bank { get; set; }
        public List<SubDivision> Subdivisions { get; set; }
        public List<Town> Towns { get; set; }
        // New display property
        public string DisplayName => $"{BranchCode} ({Name})";
        public Branch()
        {
            ImageVirtualPath = "~/AppFiles/Images/noimage.jpg";
        }
    }
    public class BranchDropdownDto
    {

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
