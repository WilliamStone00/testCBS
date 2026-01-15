using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public  class SendSmsFileUploadCommand
    {
        public string FileUploadId { get; set; }
        ///// <summary>
        ///// Rows to send (usually from your preview result).
        ///// </summary>
        //public List<SmsUploadRowToSendDto> Rows { get; set; } = new();

        /// <summary>
        /// Optional fixed values applied to each SMS (if not already in Rows).
        /// </summary>
        public string SenderService { get; set; }
        public string Purpose { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Optional: if you want to override template per upload rather than reading "Message Template" column.
        /// If provided, extractor can fallback to this when template column is missing/empty.
        /// </summary>
        public string DefaultMessageTemplate { get; set; } // optional
    }
}
