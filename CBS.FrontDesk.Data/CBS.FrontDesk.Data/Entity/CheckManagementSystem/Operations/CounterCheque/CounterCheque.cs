using CBS.FrontDesk.Data.Entity.DataTable;
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
        public string CustomerId { get; set; }
        // For displaying in the list
        public string CheckLeafId { get; set; }
        public string Id { get; set; }
        public string CheckNumber { get; set; }
        public DateTime? IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public string Status { get; set; }
      
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

    
    public class CounterChequeQuery
    {
        public CounterChequeQuery()
        {
            DataTableOptions = new DataTableOptions();
        }

        public DataTableOptions DataTableOptions { get; set; }

        public string Id { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string BranchId { get; set; }
        
    }


    public class ChequeRequestDto
    {
        public string Id { get; set; }              
        public string CustomerId { get; set; }      
        public decimal Amount { get; set; }         
        public string AccountNumber { get; set; }   
        public string BranchId { get; set; }        
        public string CheckLeafId { get; set; }     
        public string CheckNumber { get; set; }    
        public DateTime? IssuedOn { get; set; }      
        public string IssuedBy { get; set; }        
    }

}
