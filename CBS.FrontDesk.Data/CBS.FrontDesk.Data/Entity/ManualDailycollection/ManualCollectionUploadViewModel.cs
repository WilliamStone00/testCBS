using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    public class ManualCollectionUploadViewModel
    {
        [Required(ErrorMessage = "Please select a branch.")]
        public string BranchId { get; set; }

        // This will hold the composite key "collectorId|userId"
        [Required(ErrorMessage = "Please select a collector.")]
        public string CollectorAndUser { get; set; }

        [Required(ErrorMessage = "Please select a file to upload.")]
        public HttpPostedFileBase UploadedFile { get; set; }
    }

    public class GetFilesForDataTableQuery
    {
        // This holds the standard DataTable parameters (draw, start, length, etc.)
        public DataTableOptions Options { get; set; }

        // --- Basic Filters (Already Included) ---
        public string StatusFilter { get; set; } // e.g., "Pending", "Approved"
        public DateTime? StartDate { get; set; } // For UploadedOn date range
        public DateTime? EndDate { get; set; }   // For UploadedOn date range      
    
        public string BranchId { get; set; }
        public string CollectorId { get; set; }
        public string UploadedByUserId { get; set; }
        public string GlobalSearch { get; set; }
        public string FileCategory { get; set; }
        public bool ShowAll { get; set; } = false;

        public string FileName { get; set; }

        public GetFilesForDataTableQuery()
        {
            // Ensure Options is never null
            Options = new DataTableOptions();
        }
    }
}

