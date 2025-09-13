using CBS.FrontDesk.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity 
{
    public class DailyCustomer
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string customerType { get; set; }
        public string gender { get; set; }
        public string matricule { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string bankCode { get; set; }
        public string branchName { get; set; }
        public string branchCode { get; set; }
        public string branchId { get; set; }
        public string economicActivitiesId { get; set; }
        public string bankId { get; set; }
        public string countryId { get; set; }
        public string regionId { get; set; }
        public string divisionId { get; set; }
        public string subDivisionId { get; set; }
        public string townId { get; set; }
        public string placeOfBirth { get; set; }
        public string occupation { get; set; }
        public string idNumberIssueDate { get; set; }
        public string idNumberIssueAt { get; set; }
        public string idNumber { get; set; }
        public string language { get; set; }
        public string maritalStatus { get; set; }
        public Resource resource { get; set; }
    }
}
