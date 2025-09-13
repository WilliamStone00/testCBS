using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DownLoadDTO
{
    public class FileDownloadDto
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        // Optional: for convenience/diagnostics
        public string SavedFullPath { get; set; }
        public string SavedRelativePath { get; set; } // e.g. /MembersExport/ALL/CustomerExport/<file>.xlsx
    }
}
