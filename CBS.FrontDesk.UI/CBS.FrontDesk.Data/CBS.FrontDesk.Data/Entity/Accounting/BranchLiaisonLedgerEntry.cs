using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{

    public class BranchLiaisonLedgerEntryServiceResponse
    {
        public List<BranchLiaisonLedgerEntry> Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
    }
    public class BranchLiaisonLedgerEntry
    {
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string AccountId { get; set; }

        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal TotalDrAmount { get; set; }
        public decimal TotalCrAmount { get; set; }
        public decimal Balance { get; set; }
    }
}
