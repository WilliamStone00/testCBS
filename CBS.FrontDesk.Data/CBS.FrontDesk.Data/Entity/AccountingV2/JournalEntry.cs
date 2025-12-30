using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.AccountingV2
{

    public class JournalEntryPayload
    {
        public string OperationCode { get; set; }

        public string BranchId { get; set; }

        public string CounterpartyBranchId { get; set; } = null;

        public DateTime? AccountingDate { get; set; }

        public string PostMode { get; set; } = "HOLD_FOR_APPROVAL";
        public string Narration { get; set; }
        //----------dtails------------------
        public string Reference { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }


        public JournalPayload Payload { get; set; } = new JournalPayload();

        // Optional (not in JSON but kept for possible internal use)
        public string OperationType { get; set; }
        public string WorkTicket { get; set; }
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string CorrelationId { get; set; }
        public string ExternalApplicationName { get; set; }
        public string AuxiliaryReference { get; set; } = "AUX-IB-RECLASS-10";
    }

    public class JournalPayload
    {
        public string Memo { get; set; } = "Manual interbranch treatment (notify destination for completion";
        public bool AllowUnbalanced { get; set; } = false;
        public List<JournalEntryLine> Entries { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntryLine
    {
        public string AffiliateAccountId { get; set; }

        public string Naration { get; set; }

        public bool Dr { get; set; }
        public bool Cr { get; set; }
        public decimal Amount { get; set; }
    }





    public class ManualEntryresponnse
    {
        public string Reference { get; set; }
        public string OperationCode { get; set; }
        public string BranchId { get; set; }
        public string CounterpartyBranchId { get; set; }
        public DateTime AccountingDate { get; set; }
        public string PostMode { get; set; }
        public string CorrelationId { get; set; }
        public string Narration { get; set; }
        public string AffiliateAccountId { get; set; }
        public string AuxiliaryReference { get; set; }
        public string ExternalApplicationName { get; set; }
        public string Memo { get; set; }
        public bool AllowUnbalanced { get; set; }
        public string AccountId { get; set; }
        public string DrCr { get; set; }
        public decimal? Amount { get; set; }
        public string Remarks { get; set; }
        public string SourceModule { get; set; }
        public string PostedBy { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string Status { get; set; }
        public string TempJournalHeaderId { get; set; }
        public string ReconciledJournalHeaderId { get; set; }
    }









    public class JournalHead
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Narrative { get; set; } = null;
        public DateTime AccountingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsBalanced { get; set; }
        public string ExternalOperationType { get; set; } = null;
        public string PostMode { get; set; } = null;
        public string OperationCode { get; set; } 
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string Memo { get; set; } = null;
        public string Stage { get; set; }
        public string State { get; set; }
        public bool RequiresWorkflow { get; set; }
        public bool RequiresDestinationApproval { get; set; }
        public bool IsCashOperation { get; set; }
        public string MemberReference { get; set; } = null;
        public string TillName { get; set; } = null;
        public string CashierName { get; set; } = null;
        public string CashDenomsJson { get; set; } = null;
        public string CashTillId { get; set; } = null;
        public decimal CashDenomsTotal { get; set; }
       
        public DateTime? ClosedAtUtc { get; set; }
        public string TicketType { get; set; }


        public List<JournalLine> Lines { get; set; } 
        public List<JournalReconciliation> JournalReconciliations { get; set; } = null;
        public List<ReconciledLedgerLine> ReconciledLedgerLines { get; set; } = null;
        public List<WorkflowTicket> WorkflowTickets { get; set; } = null;
        public Receipt Receipt { get; set; } = null;

        public string Status { get; set; } = "RECEIVED";
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string CorrelationId { get; set; } = null;
        public string CounterpartyBranchId { get; set; } = null;
        public string CounterpartyBranchName { get; set; } = null;
        public string HeadOfficeBranchId { get; set; } = null;
        public string AuxiliaryRef { get; set; } = null;
        public bool IsInterBranch { get; set; }
        public string TicketSource { get; set; }
        public DateTime OpenedAtUtc { get; set; }
        public string ModifiedBy { get; set; }
        public string WorkflowTicketNotes { get; set; } = null;
        public string CreatedBy { get; set; }

        public string JournalStatus { get; set; }
        public string DailyOperator { get; set; }
        public string Source { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? InitiationDate { get; set; }
        public string InitiatedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string RejectedBy { get; set; }

        public string ApprovedByTrim { get; set; }
       
        public string InitiatedByTrim { get; set; }
       
        public string RejectedByTrim { get; set; }
        public string OperationCodeTrim { get; set; }


    }

    public class ExportWorkflowRequest
    {
        public List<WorkflowTicket> WorkflowData { get; set; }
        public ExportOptions ExportOptions { get; set; }
    }

    //public class WorkflowTicketQuery
    //{
    //    public DataTableOptions Options { get; set; }
    //    // Add other filter properties as needed
    //}


    public class ExportJournalRequest
    {
        public List<JournalHead> JournalData { get; set; }
        public ExportOptions ExportOptions { get; set; }
        public JournalEntryQuery Filters { get; set; }
    }

    public class ExportOptions
    {
        public string Format { get; set; }
        public bool IncludeSummary { get; set; }
        public string FileName { get; set; }
        public string ReportType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    // ==============================================
    // JOURNAL LINE
    // ==============================================
    public class JournalLine
    {
        public string Id { get; set; } 
        public string JournalHeaderId { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CounterpartyBranchId { get; set; }
        public string CounterpartyBranchName { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        public string DrCr { get; set; } 
        public decimal Amount { get; set; }
        public string Description { get; set; } = null;
        public string MemberId { get; set; } = null;
        public string LoanId { get; set; } = null;
        public string CustomerId { get; set; } = null;
       
        public string JournalReference { get; set; } = null;
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } 
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; } = null;

        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }

        public string AuxiliaryRef { get; set; }
    }

    // ==============================================
    // JOURNAL RECONCILIATION
    // ==============================================
    public class JournalReconciliation
    {
        public string BranchId { get; set; }    
        public string Reference { get; set; } 
        public string TempJournalId { get; set; } 
        public string ReconciledJournalId { get; set; } = null;
        public string Status { get; set; } 
        public string Notes { get; set; } = null;
        public string CorrelationId { get; set; } = null;
    }

    // ==============================================
    // RECONCILED LEDGER LINE
    // ==============================================
    public class ReconciledLedgerLine
    {

        public string BranchId { get; set; }
        //public string BranchAccountId { get; set; } = default!;
        //public BranchAccount? BranchAccount { get; set; }
        public string JournalHeaderId { get; set; } 
        public JournalHead Journal { get; set; } 
        public string DrCr { get; set; } 
        public decimal Amount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        //public decimal Balance { get; set; }
        public string Description { get; set; } = null;
        public int Seq { get; set; }
        public string ReferenceNumber { get; set; } = null;
        public string AuxiliaryRef { get; set; } 
        public DateTime EntryDate { get; set; }
        public string UserName { get; set; } = null;
        public bool InterbranchStatus { get; set; }
        public  string CounterpartyBranchId { get; set; } = null;
        public string CounterpartyBranchName { get; set; } = null;

        public string AccountNumber  { get; set; }
        public string AccountName { get; set; }
        public string BranchName { get; set; }

    }

    // ==============================================
    // WORKFLOW TICKET
    // ==============================================
    public class WorkflowTicket
    {
       public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string Reference { get; set; }
        public string State { get; set; } 
        public string OperationCode { get; set; } 
        public DateTime AccountingDate { get; set; }
        public List<JournalLine> JournalLines { get; set; }
        public JournalHead TempJournalHeader { get; set; } = new JournalHead();
        public string Remarks { get; set; } = null;
        public string Id { get; set; } 
        public string JournalHeaderId { get; set; }
        public string Notes { get; set; } = null;
        public DateTime? OpenedAtUtc { get; set; }
        public string OperationType { get; set; } = null;
        public DateTime? ClosedAtUtc { get; set; }
        //public string CounterpartyBranchId { get; set; }
        public string Approveby { get; set; }
        public string Rejectedby { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public string TicketType { get; set; }
        public decimal InitiatedAmount { get; set; }
        public bool IsInterBranch { get; set; }
        public string CreatedBy { get; set; }

       


    }
   

    // ==============================================
    // RECEIPT
    // ==============================================
    public class Receipt
    {
        public string BranchId { get; set; }
        public string Reference { get; set; } 
        public string JournalHeaderId { get; set; }
        public JournalHead Journal { get; set; } = null;
        public string Title { get; set; } = null;
        
        public string PayloadJson { get; set; } = null;
        public string Number { get; set; } = null;
        public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
        public string Payor { get; set; } = null;
        public string Memo { get; set; } = null;
        public DateTime AccountingDate { get; set; }
        public string OperationCode { get; set; } 
        public string Currency { get; set; } = null;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal? CashInAmount { get; set; }
        public decimal? CashOutAmount { get; set; }
        public decimal? NetAmount { get; set; }
        
        public string IssuedBy { get; set; } = null;
        public int PrintedCount { get; set; } = 1;
        public bool IsReprint { get; set; }
        public bool? IsInterBranch { get; set; }
        public string CounterpartyBranchId { get; set; } = null;
        public string PayloadVersion { get; set; } = null;
        public string PayloadHash { get; set; } = null; 
    }

    public class JournalEntryQuery
    {
        public DataTableOptions Options { get; set; }
        public JournalEntryQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; } = null;
        public string OperationCode { get; set; }
        public string Reference { get; set; } = null;
        public DateTime? StartAccountingDate { get; set; }
        public DateTime? EndAccountingDate { get; set; }
        public DateTime? StartDate { get; set; }    // For filtering creation date range
        public DateTime? EndDate { get; set; }
        public string TicketSource { get; set; }
        public string JournalStatus { get; set; }
        public string DailyOperator { get; set; }
        public bool IsInterbranch { get; set; }
        public string Source { get; set; }
    }


    public class GetFirlterData
    {
        public string BranchId { get; set; }
        public string JournalStatus { get; set; }
        public string Source { get; set; }
    }

    public class FilterResponse
    {
        public List<OperationCodeDto> OperationCodes { get; set; }
        public List<InitiatedByDto> InitiatedBy { get; set; }
        public List<ApprovedByDto> ApprovedBy { get; set; }
    }

    public class OperationCodeDto
    {
        public string Code { get; set; }
    }

    public class InitiatedByDto
    {
        public string Name { get; set; }
    }
    public class ApprovedByDto
    {
        public string Name { get; set; }
    }
    public class GetallWorkFlowTicketsQuery
    {

        // Optional filters

        public DataTableOptions DataTableOptions { get; set; }
        public string BranchId { get; set; }
        public string State { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string CounterpartyBranchId { get; set; }
        public string TicketType { get; set; }
    }
    //public class AccountDto
    //{
    //    public string Id { get; set; }
    //    public string Code { get; set; }
    //    public string AffiliateAccountId { get; set; }
    //    public string AffiliateAccountName { get; set; }
    //    public string Name { get; set; }
    //    public string Class { get; set; }
    //    public bool PostingAllowed { get; set; }
    //    public int Depth { get; set; }
    //    public string Path { get; set; }
    //    public DateTime CreatedDate { get; set; }
    //    public bool IsDeleted { get; set; }
    //    public List<AccountDto> Children { get; set; }
    //}
    public class AccountDto
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Code { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }

    }

   
       


}

