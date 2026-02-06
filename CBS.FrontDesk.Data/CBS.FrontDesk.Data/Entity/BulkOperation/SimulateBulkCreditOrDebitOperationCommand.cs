using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class SimulateBulkCreditOrDebitOperationCommand
    {
        public string AccountChartId { get; set; }
        public string OperationType { get; set; }//CashIn or CashOut
        public string SimulationType { get; set; }
        public string Description { get; set; }
        public string AccountChart { get; set; }
        public string AccountingDate { get; set; }
        public string MainBranchName { get; set; }
        public string MainBranchCode { get; set; }
        public string MainBranchId { get; set; }
        public bool ShouldImpactAccounting { get; set; }
        
        public List<SimulateBulkOperationDetailCommandDto> SimulateBulkOperationDetails { get; set; }
    }

  
}
