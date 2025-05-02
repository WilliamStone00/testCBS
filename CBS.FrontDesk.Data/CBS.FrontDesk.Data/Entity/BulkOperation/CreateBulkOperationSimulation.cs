using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.BulkOPerations
{
    public class CreateBulkOperationSimulation
    {
        public int TotalMember { get; set; }
        public decimal OperationVolume { get; set; }
        public List<MemberAccountsBulkOperationDetails> memberAccounts { get; set; }
    }
}
