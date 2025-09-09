using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    public class ManualDailyCollectionFileDetail
    {

        public string memberReference { get; set; }
        //public string AccountNumber { get; set; }
        public string memberName { get; set; }
        public string memberBranchName { get; set; }
        public decimal amount { get; set; }
        public string dailyCollectorName { get; set; }
    }
}
