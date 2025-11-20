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
    }
}
