using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque
{
    public class CounterCheques
    {
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string BranchId { get; set; }
        // For displaying in the list
        public string CheckLeafId { get; set; }
        public string Id { get; set; }
        public string CheckNumber { get; set; }
        public DateTime IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public string Status { get; set; }
        public string ClientName { get; set; } // For better display
      //  public string BranchName { get; set; } // For better display
    }

    // DTO for the modal action form
    public class CounterChequeActionDto
    {
        [Required]
        public string CounterChequeId { get; set; }
        [Required]
        public string Motive { get; set; }
        public string Action { get; set; } // "Review", "Validate", or "Reject"
    }

    // Query object for the server-side DataTable
    public class CounterChequeQuery : GetFilesForDataTableQuery // Re-using for consistency
    {
        // We can add specific filters for this module if needed
    }
}
