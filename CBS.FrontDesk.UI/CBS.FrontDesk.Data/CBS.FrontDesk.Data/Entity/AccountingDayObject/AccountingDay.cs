using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{
    public class AccountingDay
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public DateTime Date { get; set; }
        public string StrDate { get; set; }
        public bool IsClosed { get; set; }
        public string ClosedBy { get; set; }
        public string OpenedBy { get; set; }
        public string StrClosedAt { get; set; }
        public string StrOpenedAt { get; set; }
        public string Note { get; set; } = "Opening of Accounting Day.";
        public DateTime ReOpenedDate { get; set; } = DateTime.MinValue;

        public DateTime? ClosedAt { get; set; }
        public DateTime? OpenedAt { get; set; }
        public bool IsCentralized { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
    }
}
