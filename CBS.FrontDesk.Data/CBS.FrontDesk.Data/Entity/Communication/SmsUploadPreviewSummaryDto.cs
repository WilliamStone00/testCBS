using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public class SmsUploadPreviewSummaryDto
    {
        public string FileUploadId { get; set; }
        public string FileCode { get; set; }

        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }

        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }

        public List<string> ValidationErrors { get; set; }
        public List<SmsUploadDetailDto> Rows { get; set; }
    }
}
