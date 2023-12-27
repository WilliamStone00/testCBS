using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class GroupProfile
    {
        public string groupName { get; set; }
        public string meetingDay { get; set; }
        public DateTime dateOfEstablishment { get; set; }
        public DateTime dateOfBirth { get; set; }
        public int groupCycle { get; set; }
        public string groupType { get; set; }
        public string email { get; set; }
        public string countryId { get; set; }
        public string regionId { get; set; }
        public string townId { get; set; }
        public string address { get; set; }
        public string zipCode { get; set; }
        public string homePhone { get; set; }
        public string personalPhone { get; set; }
        public string photoSource { get; set; }
        public string package { get; set; }
        public string divisionId { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }
        public string organisationId { get; set; }
        public string subDivisionId { get; set; }
        public int loanCycle { get; set; }
    }


}
