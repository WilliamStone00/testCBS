using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2
{
    public class JournalApproval
    {
        public string BranchId { get; set; }
        public string Reference { get; set; }
        public string State { get; set; }
        public string OperationCode { get; set; }
        public DateTime AccountingDate { get; set; }
        public List<JournalApprovalLine> JournalLines { get; set; }
        public string Remarks { get; set; }
        public string Id { get; set; }
        public string JournalHeaderId { get; set; }
        public string Notes { get; set; }
        public DateTime? OpenedAtUtc { get; set; }
        public string OperationType { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
    }

    public class JournalApprovalLine
    {
        
        public string Account { get; set; }
        public string DrCr { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
    }
}
