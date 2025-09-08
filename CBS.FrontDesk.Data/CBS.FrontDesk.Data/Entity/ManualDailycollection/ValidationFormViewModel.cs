using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    // This is the GENERIC object sent from the Controller to the Service. It is CORRECT.
    public class ValidationDto
    {
        [Required]
        public string ManualEntryDailyCollectorId { get; set; }
        public string ApprovedBy { get; set; }
        [Required(ErrorMessage = "A statement is required.")]
        public string Statement { get; set; } // Renamed for clarity
        public string Mode { get; set; }
    }

    // These are the SMALL, SPECIFIC objects the backend API expects.
    // We will create these inside our service method.

    public class ApprovePayload
    {
        public string ManualEntryDailyCollectorId { get; set; }
        public string approvalStatement { get; set; }
    }

    public class ReviewPayload
    {
        public string ManualEntryDailyCollectorId { get; set; }
        public string reviewerStatement { get; set; }
    }

    public class RejectPayload
    {
        public string ManualEntryDailyCollectorId { get; set; }
        public string rejectionStatement { get; set; }
    }

}
