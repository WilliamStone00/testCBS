using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class PaginatedResource : ResourceParameter
    {
        public PaginatedResource() : base("TransactionDate")
        {
        }
        public string BranchId { get; set; }
        public bool IsByBranch { get; set; }
    }
    public class TransactionTrackerDatatableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public string Id { get; set; }
        public string CommandDataType { get; set; }
        public string CommandJsonObject { get; set; }
        public string TransactionReferenceId { get; set; }
        public bool HasPassed { get; set; }
        public int NumberOfRetry { get; set; }
        public DateTime DatePassed { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DestinationUrl { get; set; }
        public string SourceUrl { get; set; }
        public string UserFullName { get; set; }
        public string BranchOffice { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ErrorOrSuccessMessage { get; set; }
    }
    public class TransactionTracker
    {
        public PaginationMetadata PaginationMetadata { get; set; }
        public string Id { get; set; }
        public string CommandDataType { get; set; }
        public string CommandJsonObject { get; set; }
        public string TransactionReferenceId { get; set; }
        public bool HasPassed { get; set; }
        public int NumberOfRetry { get; set; }
        public DateTime DatePassed { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DestinationUrl { get; set; }
        public string SourceUrl { get; set; }
        public string UserFullName { get; set; }
        public string BranchOffice { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ErrorOrSuccessMessage { get; set; }
        public static class CommandDataTypeObject
        {
            public const string AddErrorRequestCommand = "AddErrorRequestCommand";
            public const string AddTransferEventCommand = "AddTransferEventCommand";
            public const string AddTransferToNonMemberEventCommand = "AddTransferToNonMemberEventCommand";
            public const string AddTransferWithdrawalEventCommand = "AddTransferWithdrawalEventCommand";
            public const string AssignBookingDirectionTrialBalance4ColumnCommand = "AssignBookingDirectionTrialBalance4ColumnCommand";
            public const string AutomatedEventEntryCommand = "AutomatedEventEntryCommand";
            public const string AutoPostingEventCommand = "AutoPostingEventCommand";
            public const string BranchToBranchTransferCommand = "BranchToBranchTransferCommand";
            public const string BulkLoanRefundPostingFromLedgerCommand = "BulkLoanRefundPostingFromLedgerCommand";
            public const string CashClearingTransferCommandFromCashRequisition = "CashClearingTransferCommandFromCashRequisition";
            public const string CashClearingTransferFromBankDepositCommand = "CashClearingTransferFromBankDepositCommand";
            public const string CashInitializationCommand = "CashInitializationCommand";
            public const string CashRequisitionCommand = "CashRequisitionCommand";
            public const string CleanAccountAndAccountingEntriesCommand = "CleanAccountAndAccountingEntriesCommand";
            public const string CloseOfDayEventCommand = "CloseOfDayEventCommand";
            public const string ClosingOfMemberAccountCommand = "ClosingOfMemberAccountCommand";
            public const string DailyCollectionConfirmationPostingEventCommand = "DailyCollectionConfirmationPostingEventCommand";
            public const string DailyCollectionMonthlyCommisionEventCommand = "DailyCollectionMonthlyCommisionEventCommand";
            public const string DailyCollectionMonthlyPayableEventCommand = "DailyCollectionMonthlyPayableEventCommand";
            public const string DailyCollectionPostingEventCommand = "DailyCollectionPostingEventCommand";
            public const string DeleteAccountPostingCommand = "DeleteAccountPostingCommand";
            public const string InternalAutoPostingEventCommand = "InternalAutoPostingEventCommand";
            public const string IPosting = "IPosting";
            public const string LoanApprovalPostingCommand = "LoanApprovalPostingCommand";
            public const string LoanDisbursementPostingCommand = "LoanDisbursementPostingCommand";
            //MobileMoneyTopUpCommand
            public const string MakeTransferPosting = "MakeTransferPosting";
            public const string GenericPostingCommand = "GenericPostingCommand";
            public const string MobileMoneyTopUpCommand = "MobileMoneyTopUpCommand";
            public const string LoanDisbursementRefinancingPostingCommand = "LoanDisbursementRefinancingPostingCommand";
            public const string LoanRefundPostingCommand = "LoanRefundPostingCommand";
            public const string MakeAccountPostingCommand = "MakeAccountPostingCommand";
            public const string MakeBulkAccountPostingCommand = "MakeBulkAccountPostingCommand";
            public const string MakeLoanRefinancingCommand = "MakeLoanRefinancingCommand";
            public const string MakeNonCashAccountAdjustmentCommand = "MakeNonCashAccountAdjustmentCommand";
            public const string MakeRemittanceDepositCommand = "MakeRemittanceDepositCommand";
            public const string ManualPostingEventCommand = "ManualPostingEventCommand";
            public const string MobileMoneyCollectionOperationCommand = "MobileMoneyCollectionOperationCommand";
            public const string MobileMoneyManagementPostingCommand = "MobileMoneyManagementPostingEventCommand";
            public const string MobileMoneyOperationCommand = "MobileMoneyOperationCommand";
            public const string OpeningOfDayEventCommand = "OpeningOfDayEventCommand";
            public const string OpenOfDayTransaction = "OpenOfDayTransaction";
            public const string RetrieveBalanceSheetCommand = "RetrieveBalanceSheetCommand";
            public const string ReverseAccountingEntryCommand = "ReverseAccountingEntryCommand";
            public const string SalaryPayrollActivation = "SalaryPayrollActivation";
            public const string UpdateAccountingEntryCommand = "UpdateAccountingEntryCommand";

            public const string DepositPostingEventCommand = "DepositPostingEventCommand";
            public const string MakeRemittanceCommand = "MakeRemittanceCommand";

            public static string InternalAutoPostingCommand = "InternalAutoPostingCommand";

            public static string AccountBalanceInitialization = "AccountBalanceInitialization";







        }
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    }
    public class TransactionTrackerConfiguration
    {
        //
        public List<TransactionTracker> TransactionTrackers { get; set; } = new List<TransactionTracker>();
        public TransactionTracker TransactionTracker { get; set; } = new TransactionTracker();
        public QueryModel QueryModel { get; set; }
    }
}
