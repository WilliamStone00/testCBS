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

namespace CBS.BusinessService.Accounts
{

    public class AccountServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;
        public AccountServices()
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions)
        {
            Func<Task<List<CustomerAccountDto>>> getDataFunc = async () => (await GetCustomersAccounts()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<CustomerAccountDto>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<CustomerAccountDto>> GetCustomersAccounts()
        {
            try
            {
                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(APICallHelper.GetAllIndividualProfile);
                var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(APICallHelper.GetAllAccounts);
                var data = (from a in individualProfiles.ApiResponseData.Data
                            join ca in accounts.ApiResponseData.Data on a.customerId equals ca.customerId
                            select MapCustomersToAccounts(a, ca)).ToList();
                return data;
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
                .OrderBy(t => t.createdDate) // Order transactions by date in descending order
                .Select(transaction => new TransactionHistoryExport
                {
                    accountHolderName =transaction.account.accountName,
                    Date = transaction.createdDate,
                    originalAmount = transaction.originalDepositAmount,
                    accountNumber = transaction.accountNumber,
                    transactionType = transaction.transactionType,
                    operationDirection = transaction.operationType,
                    transactionRef = transaction.transactionRef,
                    previousBalance = transaction.previousBalance,
                    note = transaction.note,
                    senderAccountId = transaction.senderAccountId,
                    receiverAccountId = transaction.receiverAccountId,
                    depositorIdNumber = transaction.depositorIdNumber,
                    depositorName = transaction.depositorName,
                    depositorIdIssueDate = transaction.depositorIdIssueDate,
                    depositorIdExpiryDate = transaction.depositorIdExpiryDate,
                    balance = transaction.balanceBroughtForward,
                    fee = transaction.fee,
                    feeType = transaction.feeType, Operation= transaction.Operation,
                    teller = transaction.teller.name, customerReferenceNumber= transaction.account.customerId, newAmount= transaction.amount, productName= transaction.account.product.name
                })
                .ToList();
        }
       
        public async Task<IEnumerable<StringValues>> SourceAndDestinationAccount()
        {
            try
            {
                var accounts = from a in await GetCustomersAccounts() select new StringValues {
                    Text =$"{a.accountNumber}-{a.productName}-{a.customerName}", Value = a.accountNumber,
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
                productName = caAccount.product.name,
                profileType = "Individual"
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
        public async Task<ExecutionMessages> MakeInitialDeposit(AccountDepositRequest model)
        {
            try
            {
                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                model.depositType = "CASH_INITIAL_DEPOSIT";
                if (IsCurrencySumValid(model.currencyNotes, model.amount))
                {
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<TransactionRespose>>(string.Format(APICallHelper.InitialDeposit, model.accountNumber), model);
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
        public async Task<ExecutionMessages> Deposit(DepositRequest model)
        {
            try
            {
                if (IsCurrencySumValid(model.currencyNotes,model.amount))
                {
                    model.bankId = GetBankID();
                    model.branchId = GetBranchID();
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionRespose>>(APICallHelper.MakeDepoit, model);
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
        public async Task<ExecutionMessages> Transfer(TransferRequest model)
        {
            try
            {
               
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionRespose>>(APICallHelper.MakeTransfer, model);
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
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Withdrawal(WithdrawalRequest model)
        {
            try
            {
                if (IsCurrencySumValid(model.currencyNotes, model.amount))
                {
                    model.bankId = GetBankID();
                    model.branchId = GetBranchID();
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionRespose>>(APICallHelper.MakeWithdrawal, model);
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

                        var customer = await GetCustomer(cusResponseObject.ApiResponseData.Data.customerId);
                        cusResponseObject.ApiResponseData.Data.Customer = customer;
                        customer.name = $"{customer.firstName} {customer.lastName}";
                        var balance = await GetCustomerBalance(cusResponseObject.ApiResponseData.Data.customerId);
                        var Accounts = await GetCustomerAccounts(cusResponseObject.ApiResponseData.Data.customerId);
                        var transactionHistories = await GetCustomerTransactionsByAccountNumber(cusResponseObject.ApiResponseData.Data.accountNumber);
                        account.AccountBalance = balance;
                        account.Accounts = Accounts;
                        account.AccountActivationRequest = new AccountDepositRequest
                        {
                            accountNumber = account.accountNumber,
                            amount = 0,
                        };
                        account.DepositRequest = new DepositRequest
                        {
                            accountNumber = account.accountNumber,
                            amount = 0,
                            note = string.Empty, currencyNotes = new CurrencyNotes(),
                                depositType = string.Empty, 
                        };
                        account.WithdrawalRequest = new WithdrawalRequest
                        {
                            accountNumber = account.accountNumber,
                            amount = 0,
                            note = string.Empty,
                            withDrawalType = string.Empty,
                        };
                        account.TransferRequest = new TransferRequest
                        {
                            receiverAccountNumber = string.Empty,
                            amount = 0,
                            note = string.Empty,
                            senderAccountNumber = account.accountNumber, transferType = string.Empty,
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
