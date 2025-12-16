using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear
{
    public class accountingyear
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }

        public int Year { get; set; }
        public string YearB { get; set; }
        public string Status { get; set; }
        public string StatusB { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime? OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        public  string AccountingYearId { get; set; }
    }


    public class AccoutingyearQuery
    {
        public DataTableOptions Options { get; set; }
        public AccoutingyearQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; } = null;
      
        public DateTime? StartDate { get; set; }    // For filtering creation date range
        public DateTime? EndDate { get; set; }

        public int Year { get; set; }

        public string Status { get; set; }

    }
}
