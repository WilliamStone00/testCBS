using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest
{
    public class ClearanceValidation
    {
       
        
            [Required]
            public string ChequeClearanceId { get; set; }
            public string ApprovedBy { get; set; }
            [Required(ErrorMessage = "A statement is required.")]
            public string Statement { get; set; } // Renamed for clarity
            public string Mode { get; set; }

            public string AccountNumber { get; set; }

            public bool DepositToAccount { get; set; }


    }
}
