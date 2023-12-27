using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    public class IndividualProfile
    {
        [Required]
        public string firstName { get; set; }
        [Required]
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }
        [Required]
        public string gender { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string idNumber { get; set; }
        [Required]
        public string countryId { get; set; }
        [Required]
        public string regionId { get; set; }
        [Required]
        public string townId { get; set; }
        [Required]
        public string address { get; set; }
        public string zipCode { get; set; }
        public string homePhone { get; set; }
        public string personalPhone { get; set; }
        public string photoSource { get; set; }
        public bool isUseOnlineMobileBanking { get; set; }
        public string packageId { get; set; }
        [Required]
        public string divisionId { get; set; }
        [Required]
        public string branchId { get; set; }
        public string secretQuestion { get; set; }
        public string secretAnswer { get; set; }
        [Required]
        public string economicActivitesId { get; set; }
        [Required]
        public string bankId { get; set; }
        [Required]
        public string organisationId { get; set; }
        [Required]
        public string subdivisionId { get; set; }
        [Required]
        public string taxIdentificationNumber { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public int loanCycle { get; set; }
        [Required]
        public List<string> accountType { get; set; }
    }


}
