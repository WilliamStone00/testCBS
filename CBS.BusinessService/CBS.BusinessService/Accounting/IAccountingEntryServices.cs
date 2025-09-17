using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.BusinessService
{
    public interface IAccountingEntryServices
    {
        BranchServices branchServices { get; }

        Task<ExecutionMessages> CashReplenishmentRequest(CashInfusion model);
        Task<bool> CheckIfTransactionReferenceIdExist(string Id);
        Task<ExecutionMessages> CreateApprovalRequest(CashApprovalResponse model, bool HasError);
        Task<IExecutionMessages> CreateBankCashTransaction(BankCashOut model);
        Task<IExecutionMessages> CreateBranchToBranchTransferTransaction(BranchToBranchTransfer model);
        Task<IExecutionMessages> CreateCashClearingTransaction(CashClearing model);
        Task<ExecutionMessages> CreateManualAccountingEntry(ManualAccountingEntry model);
        Task<ExecutionMessages> CreateTransactionReversalRequest(TransactionReversalRequest model);
        Task<ExecutionMessages> DepositNotificationApprovalRequest(Approval model);
        Task<ExecutionMessages> DepositNotificationApprovalRequest(DepositNotificationApproval model);
        Task<ExecutionMessages> DepositNotificationRequest(DepositNotification model);
        Task<List<AccountingEntryDto>> GetAccountingEntriesDtoByReferceId(string reference);
        Task<List<AccountingEntry>> GetAccountingEntriesByReferceId(string reference);
        Task<List<AccountingEntry>> GetAllAccountingEntries();
        Task<List<AccountingEntry>> GetAllAccountingEntriesForAnAccountPerBranch(string branchId, string accountId);
        Task<List<CashReplenimentRequestDto>> GetAllCashReplenimentRequest();
        Task<List<CashReplenimentRequest>> GetAllCashReplenimentRequestByBranch(bool VALUE);
        Task<List<CashReplenimentRequest>> GetAllCashReplenimentRequestByBranch(string Id);
        Task<List<CashReplenimentRequestDto>> GetAllCashRequestApprovalQuery();
        Task<List<DepositNotificationDto>> GetAllDepositNotificationRequest();
        Task<List<TransactionReversalDetailRequestDto>> GetAllTransasctionReversalRequest();
        Task<BankTransaction> GetBankTransactionByReferenceId(string referenceId);
        Task<IEnumerable<Branch>> GetBranches();
        Task<List<CashRoot>> GetCashReplenimentCurrentOpenOfDayHistoryRequestId();
        Task<CashReplenimentRequest> GetCashReplenimentReferenceRequest(string Id);
        Task<CashReplenimentRequest> GetCashReplenimentRequest(string Id);
        Task<CashReplenimentRequest> GetCashReplenishmentRequestIdReference(string Id);
        Task<DepositNotificationDto> GetDepositNotificationRequest(string Id);
        Task<TransactionReversalDetailRequestDto> GetTransactionReversalRequestByReferenceId(string Id);
        Task<TransactionReversalDetailRequestDto> GetTransasctionReversalRequestById(string Id);
        Task<List<AccountingEntry>> GetTrialBalance4ColumnEntries(TrialBalance4Column trialBalance);
        Task<User> GetUser(string userid);
        Task<IEnumerable<User>> GetUserList();
        Task<List<UsersNotification>> GetUserNotificationRequest();
        Task<List<UsersNotification>> GetUserNotificationRequestByBranchId(string branchID);
        Task<List<AccountingEntry>> RetrieveAccountingEntries(JEQuery model);
        Task<List<AccountingEntry>> RetrieveAccountingEntries(SystemQuery model);
        Task<List<ModelBalanceSheetAssets>> RetrieveBalanceSheetColumnEntries(SystemQuery model);
        Task<List<BranchLiaisonLedgerEntry>> RetrieveBranchLiaisonAccountingEntries(SystemQuery model);
 
        Task<List<LiaisonLedgerEntry>> RetrieveLiasonAccountingEntries(SystemQuery model);
        Task<List<TrialBalance4ColumnDto>> RetrieveTrialBalance4ColumnEntries(SystemQuery model);
        Task<List<TrialBalance6ColumnDto>> RetrieveTrialBalance6ColumnEntries(SystemQuery model);
        Task<ExecutionMessages> TransactionReversalRequest(TransactionReversalRequest model);
        Task<ExecutionMessages> TransactionReversalRequestApproval(TransactionReversalRequestApproval model);
        Task<ExecutionMessages> Update(CashInfusion model);
        Task<ExecutionMessages> UpdateDepositNotificationRequest(DepositNotification model);
        Task<IExecutionMessages> UploadBankDepositTransactionReciept(UploadBankReciept model);
        Task<ExecutionMessages> UploadFiles(AddDocumentUploadedCommand documentRequest);
    }
}