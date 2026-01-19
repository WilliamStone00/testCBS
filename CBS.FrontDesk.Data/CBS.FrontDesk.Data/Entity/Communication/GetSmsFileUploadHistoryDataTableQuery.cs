using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    /// <summary>
    /// DataTable query for SmsFileUploadHistory (Mongo).
    /// Supports advanced filtering + free-text search + paging + sorting.
    /// </summary>
    public sealed class GetSmsFileUploadHistoryDataTableQuery
    {
        public DataTableOptions Options { get; set; }
        public GetSmsFileUploadHistoryDataTableQuery() { Options = new DataTableOptions(); }
        // -------------------------
        // Filters (optional)
        // -------------------------
        public string FileUploadId { get; set; }

        public string FileCode { get; set; }

        public string BranchId { get; set; } // maps to BranchID in entity
        public string SentBy { get; set; }

        public string SenderService { get; set; }
        public string Purpose { get; set; }
        public string Title { get; set; }

        public bool? IsSentSuccessfully { get; set; }

        public DateTime StartDate { get; set; } // SentOn >= StartDate
        public DateTime EndDate { get; set; }   // SentOn <= EndDate

        // Overrides Options.searchValue if provided
        public string SearchTerm { get; set; }
    }
}
