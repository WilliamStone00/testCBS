using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public class SmsFileUploadSendSummaryDto
    {
        public string FileUploadId { get; set; }
        public string FileCode { get; set; }

        public int TotalRows { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }

        public List<string> Errors { get; set; }
    }
}
