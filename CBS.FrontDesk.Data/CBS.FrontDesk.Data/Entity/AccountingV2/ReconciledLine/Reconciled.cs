using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.ReconciledLine
{
    public class Reconciled
    {
        public string Id { get; set; }

        public string BranchId { get; set; }

        public string BranchAccountId { get; set; }

        public string JournalHeaderId { get; set; }

        public string DrCr { get; set; }

        public decimal Amount { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }

        public decimal Balance { get; set; }

        public string Description { get; set; }

        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public int Seq { get; set; }

        public string ReferenceNumber { get; set; }

        public string AuxiliaryRef { get; set; }

        public DateTime EntryDate { get; set; }

        public string UserName { get; set; }

        public bool InterbranchStatus { get; set; }

        public string CounterpartyBranchId { get; set; }

        public string CounterpartyBranchName { get; set; }

        public string TempBranchAccountId { get; set; }

        public string BranchName { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int LineNum { get; set; }
    }


    public class ReconciledQuery
    {
        public DataTableOptions Options { get; set; }

        public ReconciledQuery()
        {
            Options = new DataTableOptions();
        }

        public string BranchId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string BranchAccountId { get; set; }

        public string ReferenceNumber { get; set; }

        public string AccountNumber { get; set; }

        public string AccountName { get; set; }

        public string UserName { get; set; }

        public string AuxiliaryRef { get; set; }

        public string CounterpartyBranchId { get; set; }

        public bool IsInterbranch { get; set; }
    }

}

