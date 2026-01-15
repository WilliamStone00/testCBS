using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public class SmsUploadPreviewCommand
    {
        /// <summary>
        /// Uploaded SMS excel file (XLSX/XLS).
        /// </summary>
        public IFormFile File { get; set; }

        /// <summary>
        /// Optional: if you want to override template per upload rather than reading "Message Template" column.
        /// If provided, extractor can fallback to this when template column is missing/empty.
        /// </summary>
        public string DefaultMessageTemplate { get; set; } // optional
    }
}
