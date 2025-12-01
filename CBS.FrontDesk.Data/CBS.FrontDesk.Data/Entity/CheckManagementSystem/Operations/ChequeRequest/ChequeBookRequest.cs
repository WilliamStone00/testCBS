using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest
{
    // DTO the UI / API sends (matches your "match" object exactly)
    public class ChequeBookRequest
    {
        public string CustomerId { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string BankId { get; set; }
        public string SubscriptionPaymentAccount { get; set; }
        public string CheckBookAccount { get; set; }
        public string CheckBookCategoryId { get; set; }   // maps to CategoryId on entity
        public string CheckBookCategoryName { get; set; }   // maps to CategoryId on entity
        public string RequestNote { get; set; }

        // Notifications
        public bool NotifyOnApproval { get; set; }
        public bool NotifyOnClearance { get; set; }
        public bool NotifyOnRejection { get; set; }
        public bool NotifyOnPayment { get; set; }
        public bool NotifyOnAnyTransaction { get; set; }
        public bool SubscriptionAmount { get; set; }
        public bool RecurentAmount { get; set; }

        // Pricing & Books
        public decimal BasePrice { get; set; }
        public int NumberOfPages { get; set; }
        public bool AutomaticRenewal { get; set; }

        // Subscription
        public int SubscriptionDurationInMonths { get; set; }
        public bool IsRecurringSubscription { get; set; }
        public decimal? RecurringAmount { get; set; }

        // additional values when sent ack
        public string Id { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalNote { get; set; }
    }




    public class ChequeRequestQuery
    {
        public DataTableOptions Options { get; set; }
        public ChequeRequestQuery() { Options = new DataTableOptions(); }
        public string CustomerId { get; set; }

        // Category
        public string CategoryId { get; set; }


        // Location / bank
        public string BranchId { get; set; }
        public string BankId { get; set; }

        // Status & approval
        public string Status { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Notification flags
        public bool NotifyOnApproval { get; set; }
        public bool NotifyOnClearance { get; set; }
        public bool NotifyOnRejection { get; set; }
        public bool NotifyOnPayment { get; set; }
        public bool NotifyOnAnyTransaction { get; set; }

        // Renewal & validity
        public bool AutomaticRenewal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Approval
    {
        public string id { get; set; }
        public string approvalNote { get; set; }
        public string action { get; set; }

    }

}
