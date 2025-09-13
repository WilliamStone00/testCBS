using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class OrganizationProfile
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string citizenship { get; set; }
        public string branchId { get; set; }
        public string gender { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string idNumber { get; set; }
        public string countryId { get; set; }
        public string regionId { get; set; }
        public string townId { get; set; }
        public string address { get; set; }
        public string zipCode { get; set; }
        public string homePhone { get; set; }
        public string personalPhone { get; set; }
        public string photoSource { get; set; }
        public bool isUseOnlineMobileBanking { get; set; }
        public string package { get; set; }
        public string divisionId { get; set; }
        public string economicActivitesId { get; set; }
        public string bankId { get; set; }
        public string organisationId { get; set; }
        public string subDivisionId { get; set; }
    }


}
