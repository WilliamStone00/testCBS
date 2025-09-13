using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class ConfirmBulkOperationCommand
    {
        public string BulkOperationSimulationId { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovalStatusDescription { get; set; }
        public string ApprovalBy { get; set; }
    }
}
