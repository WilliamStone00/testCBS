using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class SubmitLossRequestCommand
    {
        public string CheckBookID { get; set; }
        public string CheckLeafID { get; set; }
        public string CustomerID { get; set; }
        public string BranchID { get; set; }
        public string LossReason { get; set; }
        public DateTime LossDate { get; set; }
        public string LossLocation { get; set; }
        public string Description { get; set; }
        public string ClientSuggestion { get; set; }
        public string LossReportedBy { get; set; }
        public string ThirdPartyName { get; set; }
        public string ThirdPartyNationalID { get; set; }
        public DateTime? ThirdPartyIDExpiry { get; set; }
        public DateTime? ThirdPartyDeliveryDate { get; set; }
        public string ThirdPartyIssueLocation { get; set; }
        public string RequestedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ApproveLossRequestCommand
    {
        public string RequestId { get; set; }
        public string ApprovedBy { get; set; }
    }

    // RejectLossRequestCommand.cs
    public class RejectLossRequestCommand
    {
        public string RequestId { get; set; }
        public string Reason { get; set; }
        public string RejectedBy { get; set; }
    }

    public class LossRequestQuery
    {
        public string BranchId { get; set; }
        public string Status { get; set; }
        public string CustomerId { get; set; }
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public DataTableOptions Options { get; set; }
    }

}
