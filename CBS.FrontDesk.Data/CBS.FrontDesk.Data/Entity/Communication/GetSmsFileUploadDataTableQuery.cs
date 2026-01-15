using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public sealed class GetSmsFileUploadDataTableQuery
    {
        public DataTableOptions Options { get; set; }
        public GetSmsFileUploadDataTableQuery() { Options = new DataTableOptions(); }
        public string BranchId { get; set; }
        public string UploadedBy { get; set; }
        public string FileType { get; set; }
        public string FileStatus { get; set; }


        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Global search text (DataTables search box).
        /// </summary>
        public string SearchTerm { get; set; }



    }
}
