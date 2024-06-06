using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{
    public class TrialBalanceReference
    {
        public string Id { get; set; }
 
        public string ChartOfAccountId { get; set; }

        //public string OperationSide { get; set; }

        public string AccountInfo { get; set; }

        public string StatementModelId { get; set; }

    }
}
