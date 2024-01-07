using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class ChartOfAccount
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string LabelEn { get; set; }
        public string LabelFr { get; set; }
        public bool IsBalanceAccount { get; set; }
        public string ParentAccountNumber { get; set; }
        public string ParentAccountId { get; set; }

        
        

    }
}
