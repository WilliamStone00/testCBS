using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest
{
    public class ChequeBookRequest
    {
        // Properties for Creating/Updating a Request
        [Required]
        public string customerId { get; set; } = null;
        public string bankId { get; set; }
        public string branchId { get; set; } = null;
        public string customerName { get; set; } = null;
        public string branchName { get; set; } = null;

        [Required]
        public string subscriptionPaymentAccount { get; set; } = null;

        [Required]
        public string checkBookAccount { get; set; } = null;
        [Required]
        public string categoryId { get; set; }
        public string categoryName { get; set; } // Often populated for display
        public string requestNote { get; set; }

        // Notification options
        public bool notifyOnApproval { get; set; }
        public bool notifyOnClearance { get; set; }
        public bool notifyOnRejection { get; set; }
        public bool notifyOnPayment { get; set; }
        public bool notifyOnAnyTransaction { get; set; }

        public bool automaticRenewal { get; set; }
        public decimal transactionAmount { get; set; }
        public decimal feeAmount { get; set; }
        public int numberofCheckBooks { get; set; }
        public int numberOfPages { get; set; }

        // Properties for Displaying/Listing Requests (populated by the backend)
        public string Id { get; set; } = null;
        public string status { get; set; }
        //  public DateTime? requestDate { get; set; }
        public DateTime? approvalDate { get; set; }
          public string approvalNote { get; set; }
    }

    // In your Data/Entity folder


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

}
