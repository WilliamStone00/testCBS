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
        public int Year { get; set; }
        public string Status { get; set; }
    }
}
