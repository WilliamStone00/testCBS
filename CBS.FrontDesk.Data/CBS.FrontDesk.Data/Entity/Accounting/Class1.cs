using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class TrialBalanceFile
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string FilePath { get; set; }
        public string Size { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
    }
}
