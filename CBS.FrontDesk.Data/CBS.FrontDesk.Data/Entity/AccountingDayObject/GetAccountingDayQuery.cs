using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingDayObject
{
    public class GetAccountingDayQuery
    {
        public DateTime Date { get; set; }
        public string QueryParameter { get; set; }
        public bool ByBranch { get; set; }
    }
}
