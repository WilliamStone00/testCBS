using CBS.FrontDesk.Data.Entity.BulkOperation;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.BulkOPeration
{
    public class CreateBulkOperationSimulation
    {
        public int TotalMember { get; set; }
        public decimal OperationVolume { get; set; }
        public List<BulkOperationDataDetails> memberAccounts { get; set; }
    }
}
