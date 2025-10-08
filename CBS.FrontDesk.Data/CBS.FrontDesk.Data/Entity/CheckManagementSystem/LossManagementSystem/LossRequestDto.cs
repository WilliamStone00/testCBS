using System;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class LossRequestDto
    {
        // Existing properties
        public string CheckBookID { get; set; }
        public string CheckLeafID { get; set; }
        public string CustomerID { get; set; }
        public string AccountID { get; set; }
        public string BranchID { get; set; }
        public string NationalIDNumber { get; set; }
        public DateTime LossDate { get; set; }
        public string LossReportedBy { get; set; }
        public string LossReason { get; set; }

        // New properties
        public string Status { get; set; }
        public string LossLocation { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
        public string ClientSuggestion { get; set; }

        // Third party properties
        public string ThirdPartyName { get; set; }
        public string ThirdPartyNationalID { get; set; }
        public DateTime? ThirdPartyIDExpiry { get; set; }
        public DateTime? ThirdPartyDeliveryDate { get; set; }
        public string ThirdPartyIssueLocation { get; set; }
    }

}