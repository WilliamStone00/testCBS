using CBS.FrontDesk.Data.Entity.SalaryManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public class SmsFileUploadDetailsDto
    {
        public SmsFileUploadDto File { get; set; }

        public List<SmsUploadDetailDto> Rows { get; set; }

        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
    }
}
