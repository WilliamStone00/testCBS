using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole
{
    public class ApproveMemberAdjustmentRequestCommand
    {
        public string RequestId { get; set; }
        public string ApprovedBy { get; set; }
    }
}
