using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;

using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using System.Web.Util;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.UserManagement;
using System.Web.Mvc;
using System.Web;
using CBS.BusinessService.UserManagement;

namespace CBS.BusinessService.Accounts
{

    public class AccountServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly UserManagementServices _userManagementServices;
        private readonly IndividualProfileServices _individualProfileServices;
        public AccountServices(UserManagementServices userManagementServices = null, IndividualProfileServices individualProfileServices = null)
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _userManagementServices = userManagementServices;
            _individualProfileServices = individualProfileServices;
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string path)
        {
            Func<Task<List<CustomerAccountDto>>> getDataFunc = async () => (await GetCustomersAccounts(path)).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<CustomerAccountDto>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<CustomerAccountDto>> GetCustomersAccounts(string path=null)
        {
            try
            {
                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(APICallHelper.GetAllIndividualProfile);
                var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(APICallHelper.GetAllAccounts);
                var data = (from a in individualProfiles.ApiResponseData.Data
                            join ca in accounts.ApiResponseData.Data on a.customerId equals ca.customerId
                            select MapCustomersToAccounts(a, ca)).ToList();

                if (path=="local")
                {
                    var dataaa=data.Where(x=>x.BranchId==GetBranchID());
                    return dataaa;
                }
                else
                {
                    return data;
                }
            
             
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<CustomerAccount>> GetAllAccounts()
        {
            try
            {
                var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(APICallHelper.GetAllAccounts);
                return accounts.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Account> GetTransactionsAsync()
        {
            try
            {

                var apiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<TransactionHistory>>>(APICallHelper.GetAllTransactions);
                if (apiResponse != null)
                {
                    var accounts = new Account { TransactionHistories = apiResponse.ApiResponseData.Data };
                    return accounts;
                }
                return new Account();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<TransactionHistory> GetTransactionsAsync(string transactionId)
        {
            try
            {

                var apiResponse = await _transactionApiHelper.GetAsync<ResponseObject<TransactionHistory>>(string.Format(APICallHelper.GetTransaction, transactionId));
                if (apiResponse != null)
                {
                    var transactionHistory = apiResponse.ApiResponseData.Data;
                    return transactionHistory;

                }
                return new TransactionHistory();
            }

            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public IEnumerable<TransactionHistoryExport> GetTransactionHistoryExports(List<TransactionHistory> transactionHistories)
        {
            try
            {

                var accounts = MapToTransactionHistoryExport(transactionHistories);
                return accounts;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public List<TransactionHistoryExport> MapToTransactionHistoryExport(List<TransactionHistory> transactions)
        {
            return transactions
                .OrderBy(t => t.CreatedDate) // Order transactions by date in ascending order
                .Select(transaction => new TransactionHistoryExport
                {
                    accountHolderName = transaction.Account?.AccountName ?? "-", // Use null coalescing operator
                    Date = transaction.CreatedDate,
                    originalAmount = transaction.OriginalDepositAmount,
                    accountNumber = transaction.AccountNumber,
                    transactionType = transaction.TransactionType,
                    operationDirection = transaction.OperationType,
                    transactionRef = transaction.TransactionRef,
                    previousBalance = transaction.PreviousBalance,
                    note = transaction.Note,
                    senderAccountId = transaction.SenderAccountId,
                    receiverAccountId = transaction.ReceiverAccountId,
                    depositorIdNumber = transaction.DepositorIDNumber,
                    depositorName = transaction.DepositorName,
                    depositorIdIssueDate = transaction.DepositorIDIssueDate,
                    depositorIdExpiryDate = transaction.DepositorIDExpiryDate,
                    balance = transaction.Balance,
                    InterBrachOperation = transaction.IsInterBrachOperation ? "Yes" : "No",
                    fee = transaction.Fee,
                    feeType = transaction.FeeType,
                    Operation = transaction.Operation,
                    teller = transaction.Teller?.name ?? "-", // Use null coalescing operator
                    customerReferenceNumber = transaction.Account?.CustomerId ?? "-", // Use null coalescing operator
                    newAmount = transaction.Amount,
                    productName = transaction.Account?.Product.name ?? "-",
                    DestinationBranch = transaction.DestinationBrachId,
                    DestinationShare = transaction.DestinationBranchCommission,
                    SourceShare = transaction.SourceBranchCommission,
                    SourceBranch = transaction.SourceBrachId,
                    credit = transaction.Credit,
                    debit = transaction.Debit
                    // Use null coalescing operator
                })
                .ToList();
        }


        public async Task<IEnumerable<StringValues>> SourceAndDestinationAccount()
        {
            try
            {
                var accounts = from a in await GetCustomersAccounts()
                               select new StringValues
                               {
                                   Text = $"{a.accountNumber}-{a.productName}-{a.customerName}",
                                   Value = a.accountNumber,
                               };
                return accounts;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<SavingConfigurationAggregates> GetSavingConfigurationAggregates()
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<SavingConfigurationAggregates>>(APICallHelper.GetAllConfigurationEnums);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new SavingConfigurationAggregates();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        private async Task<AccountBalance> GetCustomerBalance(string customerID)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<AccountBalance>>(string.Format(APICallHelper.GetCustomerBalance, customerID));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<TransactionHistory>> GetCustomerTransactionsByAccountNumber(string accountNumber)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<TransactionHistory>>>(string.Format(APICallHelper.GetTransactionHistoryByAccountNumber, accountNumber));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<TransactionHistory>> GetCustomerTransactionsByCustomerNumber(string customerNumber)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<TransactionHistory>>>(string.Format(APICallHelper.GetTransactionHistoryByCustomerNumber, customerNumber));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<TransactionHistory>> GetAllTransactions()
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<TransactionHistory>>>(APICallHelper.GetAllTransactions);
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<CustomerAccount>> GetCustomerAccounts(string customerID)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(string.Format(APICallHelper.GetCustomerAccounts, customerID));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        private CustomerAccountDto MapCustomersToAccounts(IndividualProfile a, CustomerAccount caAccount)
        {
            return new CustomerAccountDto
            {
                customerName = $"{a.firstName} {a.lastName}",
                status = caAccount.status,
                customerId = a.customerId,
                accountNumber = caAccount.accountNumber,
                accountId = caAccount.id,
                productId = caAccount.productId,
                productName = caAccount.product.name, BranchId=caAccount.branchId, createdDate=caAccount.createdDate.ToString(),
            };
        }

        private async Task<IndividualProfile> GetCustomer(string id)
        {
            try
            {
                var cusResponseObject = await _customerApiHelper.GetAsync<ResponseObject<IndividualProfile>>(string.Format(APICallHelper.GetCustomerByID, id));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public TransactionReportDS MapReportObject(TransactionHistory t, Branch b, User u, IndividualProfile c)
        {
            try
            {
                if (t.currencyNotes==null)
                {
                    t.currencyNotes = new CurrencyNotes();
                }
                var rpt = new TransactionReportDS
                {
                    AccountNumber = t.AccountNumber,
                    AccountType = t.Account.AccountType,
                    Amount = t.OriginalDepositAmount,
                    TransactionDate = t.CreatedDate,
                    Note500 = t.currencyNotes.note500,
                    Note2000 = t.currencyNotes.note2000,
                    Note1000 = t.currencyNotes.note1000,
                    Note5000 = t.currencyNotes.note5000,
                    Note10000 = t.currencyNotes.note10000,
                    Coin500 = t.currencyNotes.coin500,
                    Coin100 = t.currencyNotes.coin100,
                    FeeType = t.FeeType,
                    OriginalDepositAmount = t.OriginalDepositAmount,
                    HeadOfficeAddress = b.Bank.Address,
                    HeadOfficeInitial = b.Bank.BankInitial,
                    BranchCode = b.BranchCode,
                    HeadOfficeName = b.Bank.Name,
                    BranchName = b.Name,
                    HeadOfficeEmail = b.Bank.Email,
                    HeadOfficeCode = b.Bank.BankCode,
                    HeadOfficeTelephone = b.Bank.Telephone,
                    HeadOfficeWebSite = b.Bank.WebSite,
                    CashierName = u.firstName + " " + u.lastName,
                    Coin1 = t.currencyNotes.coin1,
                    Coin5 = t.currencyNotes.coin5,
                    Coin10 = t.currencyNotes.coin10,
                    Coin25 = t.currencyNotes.coin25,
                    Coin50 = t.currencyNotes.coin50,
                    Credit = t.Credit,
                    Debit = t.Debit,
                    Balance = t.Balance,
                    CustomerName = c.firstName + " " + c.lastName,
                    Fee = t.Fee,
                    Note = t.Note,
                    OperationType = t.OperationType,
                    PreviousBalance = t.PreviousBalance,
                    Tax = t.Tax,
                    TellerName = t.Teller.name,
                    TransactionRef = t.TransactionRef,
                    TransactionType = t.TransactionType,
                    AccountName = t.Account.AccountName,
                    BranchAddress = b.Address,
                    BranchTelephone = b.Telephone,
                    DepositerNote = t.DepositerNote,
                    DepositerTelephone = t.DepositerTelephone,
                    DepositorIDExpiryDate = t.DepositorIDExpiryDate,
                    DepositorIDIssueDate = t.DepositorIDIssueDate,
                    DepositorIDNumber = t.DepositorIDNumber,
                    DepositorIDNumberPlaceOfIssue = t.DepositorIDNumberPlaceOfIssue,
                    DepositorName = t.DepositorName,
                    DestinationBranchCommission = t.DestinationBranchCommission,
                    IsDepositDoneByAccountOwner = t.IsDepositDoneByAccountOwner,
                    IsInterBrachOperation = t.IsInterBrachOperation,
                    InterBrachOperation = t.IsInterBrachOperation ? "YES" : "NO",
                    Logo = b.Bank.LogoUrl,
                    Operation = t.Operation,
                    ProductName = t.Account.Product.name,
                    RecieverName = "",
                    SenderName = "",
                    RecievingBranch = "",
                    SendingBranch = "",
                    SourceBranchCommission = t.SourceBranchCommission,
                    SourceType = t.SourceType,
                    Status = t.Status
                };
                return rpt;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<ExecutionMessages> MakeInitialDeposit(AccountDepositRequest model)
        {
            try
            {
                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                model.depositType = "CASH_INITIAL_DEPOSIT";
                if (IsCurrencySumValid(model.currencyNotes, model.amount))
                {
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<TransactionHistory>>(string.Format(APICallHelper.InitialDeposit, model.accountNumber), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.amount}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Sum of notes and coins must be equal to deposit amount.");

                }
                // Make an API call to create an individual profile

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public bool IsCurrencySumValid(CurrencyNotes currencyNotes, int amount)
        {
            int totalNotesValue = currencyNotes.note10000 * 10000 +
                                  currencyNotes.note5000 * 5000 +
                                  currencyNotes.note2000 * 2000 +
                                  currencyNotes.note1000 * 1000 +
                                  currencyNotes.note500 * 500 +
                                  currencyNotes.coin500 * 500 +
                                  currencyNotes.coin100 * 100 +
                                  currencyNotes.coin50 * 50 +
                                  currencyNotes.coin25 * 25 +
                                  currencyNotes.coin10 * 10 +
                                  currencyNotes.coin5 * 5 +
                                  currencyNotes.coin1;

            return totalNotesValue == amount;
        }
        public Branch RetrieveBranchFromSession()
        {
            Branch branch =HttpContext.Current.Session["BranchObject"] as Branch; // Retrieve the Branch object from session
            if (branch != null)
            {
               return branch;
            }
           return null;
        }
        public async Task<User> RetrieveUserFromSession(string userId)
        {
            User user = await _userManagementServices.GetUser(userId); 
            if (user != null)
            {
                return user;
            }
            return null;
        }
        public async Task<IndividualProfile> RetrieveCustomerFromSession(string customerId)
        {
            IndividualCustomerProfile profile = await _individualProfileServices.GetCustomerLight(customerId);
            if (profile != null)
            {
                return profile.CustomerList;
            }
            return null;
        }
        public async Task<ExecutionMessages> Deposit(DepositRequest model)
        {
            try
            {
                if (IsCurrencySumValid(model.currencyNotes, model.amount))
                {
                    model.bankId = GetBankID();
                    model.branchId = GetBranchID();
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.MakeDepoit, model);
                    if (response.ApiResponseData!=null)
                    {
                        var transaction= response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                        User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                        var rpt = MapReportObject(response.ApiResponseData.Data, branch,user,profile);
                        HttpContext.Current.Session["ReportObject"] = rpt;
                        GetExecutionMessages(response, true, $"Deposit", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Sum of notes and coins must be equal to deposit amount.");

                }
                // Make an API call to create an individual profile

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Transfer(TransferRequest model)
        {
            try
            {

                var response = await _transactionApiHelper.PostAsync<ServiceResponse<Transfer>>(APICallHelper.TransferRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Amount}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.Amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> TransferConfirmation(TransferConfirmation model)
        {
            try
            {

                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.TransferConfirmation, model);
                if (response.ApiResponseData != null)
                {
                    var transaction = response.ApiResponseData.Data;
                    Branch branch = RetrieveBranchFromSession();
                    IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                    User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                    var rpt = MapReportObject(response.ApiResponseData.Data, branch, user, profile);
                    HttpContext.Current.Session["ReportObject"] = rpt;
                    GetExecutionMessages(response, true, $"Transfer confirmation", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Transfer Confirmation", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<Transfer> GetTransfer(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<Transfer>>(string.Format(APICallHelper.GetTransfer, id));
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return new Transfer();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Transfer>> GetTransfers()
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<Transfer>>>(APICallHelper.GetTransfers);
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return new List<Transfer>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Transfer>> GetPendingTransfers()
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<Transfer>>>(APICallHelper.GetPendingTransfers);
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return new List<Transfer>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        //LoanRepayment
        public async Task<ExecutionMessages> Withdrawal(DepositRequest deposit)
        {
            try
            {
                var model = new WithdrawalRequest { accountNumber = deposit.accountNumber, amount = deposit.amount, note = deposit.note, currencyNotes = deposit.currencyNotes, withDrawalType = deposit.depositType };
                if (IsCurrencySumValid(model.currencyNotes, model.amount))
                {
                    model.bankId = GetBankID();
                    model.branchId = GetBranchID();
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.MakeWithdrawal, model);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                        User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                        var rpt = MapReportObject(response.ApiResponseData.Data, branch, user, profile);
                        HttpContext.Current.Session["ReportObject"] = rpt;
                        GetExecutionMessages(response, true, $"Withdrawal", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Sum of notes and coins must be equal to deposit amount.");

                }
                // Make an API call to create an individual profile

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> LoanRepayment(DepositRequest deposit)
        {
            try
            {
                var model = new LoanRepaymentCommand
                {
                    AccountNumber = deposit.accountNumber,
                    Amount = deposit.amount,
                    Note = deposit.note,
                    CurrencyNotes = deposit.currencyNotes,
                    DepositType = "CASH",
                    Interest = deposit.Interest,
                    LoanId = deposit.LoanId,
                    Penalty = deposit.Penalty,
                    Principal = deposit.Principal,
                    Tax = deposit.Tax,
                    PaymentChannel = "Front_Desk",
                    PaymentMethod = "CASH",
                    DepositerNote = deposit.depositerNote,
                    DepositerTelephone = deposit.depositerTelephone,
                    DepositorIDExpiryDate = deposit.depositorIDExpiryDate,
                    DepositorIDNumber = deposit.depositorIDNumber,
                    DepositorName = deposit.depositorName,
                    DepositorIDIssueDate = deposit.depositorIDIssueDate,
                    DepositorIDNumberPlaceOfIssue = deposit.depositorIDNumberPlaceOfIssue,
                    IsDepositDoneByAccountOwner = deposit.isDepositDoneByAccountOwner
                };
                if (IsCurrencySumValid(deposit.currencyNotes, deposit.amount))
                {

                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.MakeLoanRepayment, model);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                        User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                        var rpt = MapReportObject(response.ApiResponseData.Data, branch, user, profile);
                        HttpContext.Current.Session["ReportObject"] = rpt;
                        GetExecutionMessages(response, true, $"Loan repayment", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{deposit.amount}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else
                {
                    GetExecutionMessages(model, false, $"{deposit.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Sum of notes and coins must be equal to deposit amount.");

                }
                // Make an API call to create an individual profile

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<Account> GetAccountByAccountNumber(string accountNumber)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<Account>>(string.Format(APICallHelper.MakeTrGetAccountByAccountNumber, accountNumber));
                if (cusResponseObject.IsSuccess)
                {
                    if (cusResponseObject.ApiResponseData.Data != null)
                    {
                        var account = cusResponseObject.ApiResponseData.Data;

                        var customer = await GetCustomer(cusResponseObject.ApiResponseData.Data.CustomerId);
                        cusResponseObject.ApiResponseData.Data.Customer = customer;
                        customer.name = $"{customer.firstName} {customer.lastName}";
                        var balance = await GetCustomerBalance(cusResponseObject.ApiResponseData.Data.CustomerId);
                        var Accounts = await GetCustomerAccounts(cusResponseObject.ApiResponseData.Data.CustomerId);
                        var transactionHistories = await GetCustomerTransactionsByAccountNumber(cusResponseObject.ApiResponseData.Data.AccountNumber);
                        account.AccountBalance = balance;
                        account.Accounts = Accounts;
                        account.AccountActivationRequest = new AccountDepositRequest
                        {
                            accountNumber = account.AccountNumber,
                            amount = 0,
                        };
                        account.DepositRequest = new DepositRequest
                        {
                            accountNumber = account.AccountNumber,
                            amount = 0,
                            note = string.Empty,
                            currencyNotes = new CurrencyNotes(),
                            depositType = string.Empty,
                        };
                        account.WithdrawalRequest = new WithdrawalRequest
                        {
                            accountNumber = account.AccountNumber,
                            amount = 0,
                            note = string.Empty,
                            withDrawalType = string.Empty,
                        };

                        account.TransferRequest = new TransferRequest
                        {
                            ReceiverAccountNumber = string.Empty,
                            Amount = 0,
                            Note = string.Empty,
                            SenderAccountNumber = account.AccountNumber,
                        };
                        account.TransactionHistories = transactionHistories;
                        return account;
                    }
                }

                return new Account();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }


    }

}
