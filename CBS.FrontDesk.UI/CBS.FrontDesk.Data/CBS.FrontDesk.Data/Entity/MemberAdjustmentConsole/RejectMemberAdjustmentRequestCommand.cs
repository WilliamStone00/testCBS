using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole
{
    public class RejectMemberAdjustmentRequestCommand
    {
        public string RequestId { get; set; }
        public string RejectedBy { get; set; }
        public string Reason { get; set; }
    }
}
