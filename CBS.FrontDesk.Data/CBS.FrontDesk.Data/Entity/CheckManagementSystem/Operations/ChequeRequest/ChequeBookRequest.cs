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
        public string branchId { get; set; } = null;
        public string customerName { get; set; } = null;
        [Required]
        public string subscriptionPaymentAccountId { get; set; } = null;
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

        public bool automticRenewal { get; set; }

        // Properties for Displaying/Listing Requests (populated by the backend)
        public string Id { get; set; } = null;
        public string status { get; set; }
      //  public DateTime? requestDate { get; set; }
        public DateTime? approvalDate { get; set; }
        public DateTime? requestDate { get; set; }
        public string approvalNote { get; set; }
    }

    // In your Data/Entity folder

    public class ChequeRequestQuery
    {
        public DataTableOptions Options { get; set; }
        public string CustomerFilter { get; set; }
        public string CategoryFilter { get; set; }
        public string StatusFilter { get; set; }

        public ChequeRequestQuery() { Options = new DataTableOptions(); }
    }
}
