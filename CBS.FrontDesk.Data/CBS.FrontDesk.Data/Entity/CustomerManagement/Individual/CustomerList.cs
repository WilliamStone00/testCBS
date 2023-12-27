using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement.Individual
{
    public class CustomerList
    {
        public string customerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string gender { get; set; }
        public string name { get; set; }
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
        public string pin { get; set; }
        public int attempts { get; set; }
        public bool isUseOnlineMobileBanking { get; set; }
        public string loginState { get; set; }
        public string packageId { get; set; }
        public string divisionId { get; set; }
        public string branchId { get; set; }
        public string economicActivitesId { get; set; }
        public string bankId { get; set; }
        public string organisationId { get; set; }
        public string subDivisionId { get; set; }
        public string taxIdentificationNumber { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public int loanCycle { get; set; }
        public bool active { get; set; }
        public string status { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public string branch { get; set; }
        public string town { get; set; }
    }

   

}
