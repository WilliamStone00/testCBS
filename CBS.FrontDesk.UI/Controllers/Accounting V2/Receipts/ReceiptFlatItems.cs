using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Receipts;
using System;

public class ReceiptFlatItems:BankHeaderInformation
{
    public string Id { get; set; }
    
    public string Reference { get; set; }
    public decimal Dr { get; set; }
    public decimal Cr { get; set; }
    public string Number { get; set; }
    public string Title { get; set; }
    public DateTime date { get; set; }
    public string Memo { get; set; }
    public string OperationCode { get; set; }
    public string OperationLabel { get; set; }
    public string JournalHeaderId { get; set; }
    public string JournalReference { get; set; }
    public DateTime AccountingDate { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime IssuedAtLocal { get; set; }
    public string AmountInWords { get; set; }
    public string Payor { get; set; }
    public string MemberNumber { get; set; }
    public string MemberName { get; set; }
    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public string TellerName { get; set; }
    public string TillName { get; set; }
    public string IssuedBy { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal? CashInAmount { get; set; }
    public decimal? CashOutAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public bool IsInterBranch { get; set; }
    public string CounterpartyBranchId { get; set; }
    public string CounterpartyBranchCode { get; set; }
    public string CounterpartyBranchName { get; set; }
    public int PrintedCount { get; set; }
    public bool IsReprint { get; set; }

    // Newly added properties
    public string PrintedBy { get; set; }
    public string Logo { get; set; }    // now a logo path instead of byte[]
    public string Mode { get; set; }    // newly added

    public string PayloadDisplayTitle { get; set; }
    public string PayloadDescription { get; set; }
    public string PayloadLinesJson { get; set; }
    public string PayloadCashDenominationsJson { get; set; }
    public string PayloadExtraMetadataJson { get; set; }
    public string PayloadVersion { get; set; }
    public string PayloadHash { get; set; }

    // Flattened entries as JSON text
    public string EntriesJson { get; set; }
}
