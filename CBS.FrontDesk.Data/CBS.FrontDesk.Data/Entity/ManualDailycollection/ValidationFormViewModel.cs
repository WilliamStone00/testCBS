using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    public class ValidationFormViewModel
    {
        public string FileUploadId { get; set; }
        public string ApprovedBy { get; set; } // user who approves
        public string Mode { get; set; } // "validate" or "review"
    }

    public class ValidationDto
    {
        [Required]
        public string FileUploadId { get; set; }

        // This property holds the name of the user performing the action
        public string ApprovedBy { get; set; }

        // This holds the text from the textarea (for Approve, Review, or Deny)
        [Required(ErrorMessage = "A statement is required.")]
        public string Statement { get; set; }

        // This property tells the service which action to perform
        public string Mode { get; set; } // "validate", "review", or "deny"
    }

    public class ReviewDto
    {
        [Required]
        public string FileUploadId { get; set; }
        [Required]
        public string ApprovedBy { get; set; }
        public string ReviewerStatement { get; set; }
    }

}
