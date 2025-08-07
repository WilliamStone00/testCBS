using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class BulkCashOperationFileDetails
    {
        public string BranchCode { get; set; }
        public string MemberReference { get; set; }
        public string AccountType { get; set; }
        public string MemberName { get; set; }
        public decimal Amount { get; set; }
    }

 
}
