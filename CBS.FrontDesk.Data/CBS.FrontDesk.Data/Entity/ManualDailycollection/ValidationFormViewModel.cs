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
        [Required]
        public string ApprovedBy { get; set; }
        public string ApprovalStatement { get; set; }
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
