using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    public class FileUploadSummary
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileHash { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        public string FileUploadId { get; set; }
        public string FileCategory { get; set; }
        public string SalaryProcessingStatus { get; set; }
        public bool IsAvalaibleForExecution { get; set; }
        public string FileCode { get; set; }
        public string FileType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string CollectorName { get; set; }
    }

    public class FileValidationRequest
    {
        [Required]
        public string FileUploadId { get; set; }

        public string ApprovedBy { get; set; }

        [Required(ErrorMessage = "Reviewer statement is required.")]
        public string ReviewerStatement { get; set; }

        public string ApprovalStatement { get; set; }
    }
    /// <summary>
    /// Represents the complete request payload sent from the File Validation
    /// page's DataTable to the LoadFiles controller action.
    /// It combines the standard DataTable options with your custom filters.
    /// </summary>
    public class FileValidationRequestDto
    {
       
        public DataTableOptions DataTableOptions { get; set; }

        public string StatusFilter { get; set; }
    }
}
