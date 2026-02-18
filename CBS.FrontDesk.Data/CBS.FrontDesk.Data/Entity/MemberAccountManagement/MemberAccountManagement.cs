using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAccountManagement
{
  
        public class ManageAccountStatusCommand
        {

            public string Id { get; set; }
            public string AccountId { get; set; }
            public string CurrentStatus { get; set; }
            public string ActionType { get; set; }
            public decimal BlockedAmount { get; set; }
            public string Reason { get; set; }
        }
        public class ManageAccountmanaQuery
        {
            public DataTableOptions Options { get; set; }
            public ManageAccountmanaQuery() { Options = new DataTableOptions(); }

        }
    
}
