using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SalaryManagement
{
    public class StandingOrderDataTableQuery
    {
        public DataTableOptions Options { get; set; }
        public StandingOrderDataTableQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string SourceAccountType { get; set; }
        public string DestinationAccountType { get; set; }
        public string Frequency { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
