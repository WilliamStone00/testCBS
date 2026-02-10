using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation
{
    public class Reconciliation
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string OperationBy { get; set; }
        public string OperationCode { get; set; }
        public string BranchId { get; set; }
        public DateTime AccountingDate { get; set; }
        public string PostMode { get; set; }
        public string Status { get; set; }
        public int Attempts { get; set; }
        public string LastError { get; set; }
        public DateTime? NextRetryAtUtc { get; set; }
        public string RequestJson { get; set; }
        public string DestinationUrl { get; set; }
        public string BranchName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsInterBranch { get; set; }
        public DateTime? StartAccountingDate { get; set; }

        public DateTime? EndAccountingDate { get; set; }

    }

    public class OperationDetailsFilter
    {

        public DataTableOptions Options { get; set; }
        public OperationDetailsFilter()
        {
            Options = new DataTableOptions();
        }
        public string BranchId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string OperationCode { get; set; }
        public bool IsInterBranch { get; set; }
    }


    public class ReconciliationQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public ReconciliationQuery()
        {
            DataTableOptions = new DataTableOptions();
        }

        // 🔍 FILTERS (moved here)
        public string Reference { get; set; }
        public string MemberReference { get; set; }
        public string OperationCode { get; set; }
        public string Status { get; set; }
        public string BranchId { get; set; }
        public string CorrelationId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
    public class ReconciliationQuerys
    {


        public string Reference { get; set; } = null;
        public string OperationCode { get; set; }
        public string BranchId { get; set; } = null;
        public string Status { get; set; }

        public DateTime? StartAccountingDate { get; set; }

        public DateTime? EndAccountingDate { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }


    }

    //************************************ Get Detail ********************************************//
    public class ReconciliationDetails
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string OperationCode { get; set; }
        public string BranchId { get; set; }
        public DateTime AccountingDate { get; set; }
        public string PostMode { get; set; }
        public string CorrelationId { get; set; }
        public string MemberReference { get; set; }
        public string AuxillaryReference { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string OperationBy { get; set; }
        public string Status { get; set; }
        public int Attempts { get; set; }
        public string LastError { get; set; }
        public DateTime? NextRetryAtUtc { get; set; }
        public string DestinationUrl { get; set; }
        public string ExternalApplicationName { get; set; }
        public string ReceiptId { get; set; }
        public string TempJournalId { get; set; }
        public string ReconciledJournalId { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public string RequestJson { get; set; } = null;
        public string ResponseJson { get; set; } = null;

        public string UpStatus { get; set; }
        public string UpStage { get; set; }
    }

    public class StatisticsDetails
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string Reference { get; set; }

        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }

        public string AuxiliaryRef { get; set; }
        public string Narrative { get; set; }

        public string PostMode { get; set; }
        public DateTime AccountingDate { get; set; }

        public string Memo { get; set; }
        public string MemberReference { get; set; }

        public string TillName { get; set; }

        public List<StatisticsDetails> Entry { get; set; }

    }



        public class PushRequest
        {
            public string TrackerId { get; set; }
            public string PusherName { get; set; }
            public string BranchId { get; set; }
            public string BranchName { get; set; }
            public string BranchCode { get; set; }
        }


    




        //************************************ RESPONSE summary ********************************************//

        public class ReconciliationData
        {
            public bool Success { get; set; }
            public string AccountingDate { get; set; }
            public string BranchInformation { get; set; }
            public DateTime LastSyncTime { get; set; }
            public Statistics Statistics { get; set; }
            public ReconciliationStatus ReconciliationStatus { get; set; }
            public string BranchCode { get; set; }
            public string BranchId { get; set; }

            public bool IsInterBranch { get; set; }
            public string OperationCode { get; set; }
        }

        public class Statistics
        {
            public OperationSummary TotalOperations { get; set; }
            public OperationSummary LocalCashInOperations { get; set; }
            public OperationSummary LocalCashOutOperations { get; set; }
            public OperationSummary InterBranchCashOutOperations { get; set; }
            public OperationSummary InterBranchCashInOperations { get; set; }
            public OperationSummary LocalLoanApprovalOperations { get; set; }
            public OperationSummary LocalLoanDisbursementOperations { get; set; }
            public OperationSummary InterBranchLoanApprovalOperations { get; set; }
            public OperationSummary InterBranchLoanDisbursementOperations { get; set; }
            public OperationSummary LocalManualentries { get; set; }
            public OperationSummary InterBranchManualentries { get; set; }
            public CashOperations CashOperations { get; set; }
            public LoanOperations LoanOperations { get; set; }
        }

        public class OperationSummary
        {
            public int Count { get; set; }
            public long Volume { get; set; }
            public string Currency { get; set; }
        }

        public class CashOperations
        {
            public CashOperationDetail MaxCashIn { get; set; }
            public CashOperationDetail MaxCashOut { get; set; }
        }
        public class LoanOperations
        {
            public CashOperationDetail MaxLoanApproval { get; set; }
            public CashOperationDetail MaxLoanDisbursement { get; set; }
        }

        public class CashOperationDetail
        {
            public long Amount { get; set; }
            public string Currency { get; set; }
            public EmployeeInfo PerformedBy { get; set; }
            public string TransactionId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class EmployeeInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public string EmployeeNumber { get; set; }
        }

        public class ReconciliationStatus
        {
            public int TotalTransactions { get; set; }
            public int ReconciledTransactions { get; set; }
            public int UnreconciledTransactions { get; set; }
            public double ReconciliationRate { get; set; }
        }


    
}
