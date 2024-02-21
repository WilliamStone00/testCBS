using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BankingOperations
    {
        public CashApprovalResponse CashApprovalResponse { get; set; } = new CashApprovalResponse();
    public CashInfusion CashInfusion { get; set; } = new CashInfusion();
        public CashReplenimentRequest CashReplenimentRequest { get; set; } = new CashReplenimentRequest();
        public ManualAccountingEntry ManualAccountingEntry { get; set; } = new ManualAccountingEntry();
        public List<ManualAccountingEntry> ManualAccountingEntries { get; set; } = new List<ManualAccountingEntry>();
        public List<CashReplenimentRequest> listOfCashReplenimentRequest { get; set; } = new List<CashReplenimentRequest>();

        public string ManualOperationCode { get; set; }
        public string ManualOperationName { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
    }
}
