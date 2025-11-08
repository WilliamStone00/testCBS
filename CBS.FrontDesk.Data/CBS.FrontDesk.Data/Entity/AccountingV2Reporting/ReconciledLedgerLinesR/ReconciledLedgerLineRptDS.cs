using CBS.FrontDesk.Data.Entity.AccountingV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2Reporting.ReconciledLedgerLinesR
{
    public class ReconciledLedgerLineRptDS
    {
        public string Logo { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string HeadOfficeTelephone { get; set; }
        public string HeadOfficeEmail { get; set; }
        public string HeadOfficeWebSite { get; set; }
        public string HeadOfficeInitial { get; set; }
        public string HeadOfficeCode { get; set; }
        public string DrCr { get; set; }
        public decimal Amount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; } = null;
        public int Seq { get; set; }
        public string ReferenceNumber { get; set; } = null;
        public string AuxiliaryRef { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime OpearationDate { get; set; }
        public bool InterbranchStatus { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string OperationType { get; set; }
    }
}
