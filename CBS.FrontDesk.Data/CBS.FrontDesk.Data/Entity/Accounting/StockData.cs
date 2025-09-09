using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class StockData
    {
        public string Id { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountManagementPosition { get; set; }
        public string  ChartOfAccountManagementPositionId { get; set; }

    }
}
