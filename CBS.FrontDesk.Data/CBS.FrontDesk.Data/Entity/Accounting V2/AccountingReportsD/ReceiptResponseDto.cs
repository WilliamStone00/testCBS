using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AccountingReportsD
{
    public  class ReceiptResponseDto
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Reference { get; set; }
        public string Number { get; set; }
        public string Title { get; set; }
        public string Memo { get; set; }
        public string OperationCode { get; set; }
        public string OperationLabel { get; set; }
        public string JournalHeaderId { get; set; }
        public string JournalReference { get; set; }
        public DateTime AccountingDate { get; set; }
        public DateTime IssuedAtUtc { get; set; }
        public DateTime IssuedAtLocal { get; set; }
        public string Payor { get; set; }
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string TellerName { get; set; }
        public string TillName { get; set; }
        public string IssuedBy { get; set; }
        public string Currency { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal? CashInAmount { get; set; }
        public decimal? CashOutAmount { get; set; }
        public decimal? NetAmount { get; set; }
        public List<ReceiptEntryDto> Entries { get; set; }
    }

    public class ReceiptEntryDto
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public decimal Dr { get; set; }
        public decimal Cr { get; set; }
    }
}
