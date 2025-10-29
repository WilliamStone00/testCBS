using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2
{
    public class JournalApproval
    {
        public string Reference { get; set; }
        public string BranchId { get; set; }
        public string Approver { get; set; }
    }


    public class JournalApprovalResponse
    {
        public string Reference { get; set; }
        public string BranchId { get; set; }
        public string TicketId { get; set; }
        public string TicketState { get; set; }
        public string JournalId { get; set; }
        public string Message { get; set; }
    }


}
