using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public class SmsUploadDetailDto
    {
        public int RowNumber { get; set; }

        // These 3 are REQUIRED fields in your requirement
        public string MemberReference { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverName { get; set; }

        // Final resolved SMS message (template replaced)
        public string Message { get; set; }

        // Validation tracking
        public bool IsValid { get; set; }
        public string ValidationError { get; set; }
    }
}
