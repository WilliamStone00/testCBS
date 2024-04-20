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
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.UserManagement;
using System.Web;
using CBS.BusinessService.UserManagement;
using CBS.BusinessService.Config;

namespace CBS.BusinessService.Accounts
{

    public class CashDeskServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly UserManagementServices _userManagementServices;
        private readonly ApiCallerHelper _BranchConfigApiHelper;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly LoanServices _loanServices;
        private readonly BranchServices _branchServices;
        public CashDeskServices(UserManagementServices userManagementServices = null, IndividualProfileServices individualProfileServices = null, BranchServices branchServices = null, ApiCallerHelper branchConfigApiHelper = null, LoanServices loanServices = null)
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _userManagementServices = userManagementServices;
            _individualProfileServices = individualProfileServices;
            _branchServices = branchServices;
            _BranchConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _loanServices = loanServices;
        }

        public async Task<IEnumerable<CustomerAccountDto>> GetCustomersAccounts(string path = null)
        {
            try
            {
                if (path == "all")
                {
                    // Fetch individual profiles and accounts for head office
                    var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(APICallHelper.GetAllIndividualProfile);
                    var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(APICallHelper.GetAllAccounts);
                    var branchesx = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                    var branches = branchesx.ApiResponseData.Data;
                    // Join individual profiles with accounts and map to DTOs
                    var data = (from a in individualProfiles.ApiResponseData.Data
                                join ca in accounts.ApiResponseData.Data on a.customerId equals ca.customerId
                                join b in branches on a.branchId equals b.Id
                                select MapCustomersToAccounts(a, ca, b)).ToList();

                    // Filter data based on the 'path' parameter
                    var distinctData = data.GroupBy(x => x.customerId).Select(g => g.First());
                    return distinctData;
                }
                else
                {
                    // Fetch individual profiles and accounts for the branch
                    var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(string.Format(APICallHelper.GetAllIndividualProfileByBranch, GetBranchID()));
                    var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(string.Format(APICallHelper.GetAllAccountsByBranchIdQuery, GetBranchID()));

                    // Join individual profiles with accounts and map to DTOs
                    var data = (from a in individualProfiles.ApiResponseData.Data
                                join ca in accounts.ApiResponseData.Data on a.customerId equals ca.customerId
                                select MapCustomersToAccounts(a, ca, RetrieveBranchFromSession())).ToList();

                    // Filter data based on the 'path' parameter
                    var distinctData = data.GroupBy(x => x.customerId).Select(g => g.First());
                    return distinctData;
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<CustomerAccountDto>> CustomerSearch(List<CustomerAccountDto> customers, string criteria)
        {
            try
            {
                // Convert criteria to lowercase for case-insensitive search
                string lowerCriteria = criteria.ToLower();

                // Filter customers based on the criteria
                var filteredCustomers = customers.Where(c =>
                    c.phone.ToLower().Contains(lowerCriteria) ||
                    c.customerId.ToLower().Contains(lowerCriteria) ||
                    c.customerName.ToLower().Contains(lowerCriteria) ||
                    c.customerCode.ToLower().Contains(lowerCriteria)
                ).ToList();

                return filteredCustomers;
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
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        private CustomerAccountDto MapCustomersToAccounts(IndividualProfile a, CustomerAccount caAccount, Branch b)
        {
            return new CustomerAccountDto
            {
                customerName = $"{a.firstName} {a.lastName}",
                status = caAccount.status,
                phone = a.phone,
                customerId = a.customerId,
                accountNumber = caAccount.accountNumber,
                accountId = caAccount.id,
                productId = caAccount.productId,
                productName = caAccount.product.name,
                BranchId = caAccount.branchId,
                BranchName = b.Name,
                customerCode = a.customerCode,
                createdDate = caAccount.createdDate.ToString(),
            };
        }

        private async Task<IndividualProfile> GetCustomer(string id)
        {
            try
            {
                var cusResponseObject = await _customerApiHelper.GetAsync<ResponseObject<IndividualProfile>>(string.Format(APICallHelper.GetCustomerByID, id));
                if (cusResponseObject.ApiResponseData!=null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public TransactionReportDS MaprptSource(TransactionHistory t, Branch b, User u, IndividualProfile c)
        {
            try
            {

                t.currencyNote = CurrencyMapper.MapToCurrencyNotesRequest(t.currencyNotes);

                var rpt = new TransactionReportDS
                {
                    AccountNumber = t.AccountNumber,
                    AccountType = t.Account.AccountType,
                    Amount = t.OriginalDepositAmount,
                    Telephone = c.phone,
                    Address = c.address,
                    Charges = t.Fee,
                    TransactionDate = t.CreatedDate,
                    Note500 = t.currencyNote.note500,
                    Note2000 = t.currencyNote.note2000,
                    Note1000 = t.currencyNote.note1000,
                    Note5000 = t.currencyNote.note5000,
                    Note10000 = t.currencyNote.note10000,
                    Coin500 = t.currencyNote.coin500,
                    Coin100 = t.currencyNote.coin100,
                    AmountInWord = t.AmountInWord,
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
                    Coin1 = t.currencyNote.coin1,
                    Coin5 = t.currencyNote.coin5,
                    Coin10 = t.currencyNote.coin10,
                    Coin25 = t.currencyNote.coin25,
                    Coin50 = t.currencyNote.coin50,
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
                    TransactionType = t.Operation,
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
                    Status = t.Status, 
                };
                return rpt;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<TransactionReportDS> MaprptSource(List<TransactionHistory> transactions, Branch b, IndividualProfile c)
        {
            try
            {
                List<TransactionReportDS> reports = new List<TransactionReportDS>();
                List<TransactionReportDS> results = new List<TransactionReportDS>();
                decimal openingBalance = transactions.OrderBy(t => t.CreatedDate).ToList().FirstOrDefault()?.PreviousBalance ?? 0;
                //decimal closingBalance = openingBalance;

                foreach (TransactionHistory t in transactions)
                {
                    if (t.currencyNote == null)
                    {
                        t.currencyNote = new CurrencyNotes();
                    }

                    //closingBalance += t.Credit - t.Debit;

                    TransactionReportDS rpt = new TransactionReportDS
                    {
                        AccountNumber = t.AccountNumber,
                        AccountType = t.Account.AccountType,
                        Amount = t.OriginalDepositAmount,
                        TransactionDate = t.CreatedDate,
                        Note500 = t.currencyNote.note500,
                        Note2000 = t.currencyNote.note2000,
                        Note1000 = t.currencyNote.note1000,
                        Note5000 = t.currencyNote.note5000,
                        Note10000 = t.currencyNote.note10000,
                        Coin500 = t.currencyNote.coin500,
                        Coin100 = t.currencyNote.coin100,
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
                        Coin1 = t.currencyNote.coin1,
                        Coin5 = t.currencyNote.coin5,
                        Coin10 = t.currencyNote.coin10,
                        Coin25 = t.currencyNote.coin25,
                        Coin50 = t.currencyNote.coin50,
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
                        TransactionType = t.Operation,
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
                        Status = t.Status,
                        OpeningBalance = openingBalance,
                        ClosingBalance = t.Account.Balance
                    };

                    reports.Add(rpt);
                }
                results = reports.OrderBy(t => t.TransactionDate).ToList();
                return results;
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
                model.amount = ComputeDenomination(model.currencyNotes);
                if (model.amount <= 0)
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered be greater than 0");
                    return ExecutionMessage;
                }
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

        public Branch RetrieveBranchFromSession()
        {
            Branch branch = HttpContext.Current.Session["BranchObject"] as Branch; // Retrieve the Branch object from session
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
        public async Task<ExecutionMessages> BulkDeposi(List<BulkDeposit> bulkDeposits)
        {
            try
            {
                if (bulkDeposits.FirstOrDefault().OperationType== "Withdrawal")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.MakeWithdrawal, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                        User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                        var rpt = MaprptSource(response.ApiResponseData.Data, branch, user, profile);
                        var rptSource = new List<TransactionReportDS>();
                        rptSource.Add(rpt);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, $"Deposit", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, $"Bulk deposit", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "Loan")
                {

                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "CashIn")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.BulkDeposit, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                        User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                        var rpt = MaprptSource(response.ApiResponseData.Data, branch, user, profile);
                        var rptSource = new List<TransactionReportDS>();
                        rptSource.Add(rpt);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, $"Deposit", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, $"Bulk deposit", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else
                {

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

        public async Task<ExecutionMessages> Deposit(DepositRequest model)
        {
            try
            {
                model.amount = ComputeDenomination(model.currencyNotes);
                if (model.amount <= 0)
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered be greater than 0");
                    return ExecutionMessage;
                }
                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                model.depositType = "CASH";
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.MakeDepoit, model);
                if (response.ApiResponseData != null)
                {
                    var transaction = response.ApiResponseData.Data;
                    Branch branch = RetrieveBranchFromSession();
                    IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                    User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                    var rpt = MaprptSource(response.ApiResponseData.Data, branch, user, profile);
                    var rptSource = new List<TransactionReportDS>();
                    rptSource.Add(rpt);
                    HttpContext.Current.Session["rptSource"] = rptSource;
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
                    var rpt = MaprptSource(response.ApiResponseData.Data, branch, user, profile);
                    var rptSource = new List<TransactionReportDS>();
                    rptSource.Add(rpt);
                    HttpContext.Current.Session["rptSource"] = rptSource;
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


                var model = new WithdrawalRequest { accountNumber = deposit.accountNumber, amount = deposit.amount, note = deposit.note, currencyNotes = deposit.currencyNotes, withDrawalType = "CASH" };
                model.amount = ComputeDenomination(model.currencyNotes);
                if (model.amount <= 0)
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered be greater than 0");
                    return ExecutionMessage;
                }
                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.MakeWithdrawal, model);
                if (response.ApiResponseData != null)
                {
                    var transaction = response.ApiResponseData.Data;
                    Branch branch = RetrieveBranchFromSession();
                    IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                    User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                    var rpt = MaprptSource(response.ApiResponseData.Data, branch, user, profile);
                    var rptSource = new List<TransactionReportDS>();
                    rptSource.Add(rpt);
                    HttpContext.Current.Session["rptSource"] = rptSource;
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
                model.Amount = ComputeDenomination(model.CurrencyNotes);
                if (model.Amount <= 0)
                {
                    GetExecutionMessages(model, false, $"{model.Amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered be greater than 0");
                    return ExecutionMessage;
                }
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TransactionHistory>>(APICallHelper.MakeLoanRepayment, model);
                if (response.ApiResponseData != null)
                {
                    var transaction = response.ApiResponseData.Data;
                    Branch branch = RetrieveBranchFromSession();
                    IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                    User user = await RetrieveUserFromSession(transaction.Teller.inUsedByUserId);
                    var rpt = MaprptSource(response.ApiResponseData.Data, branch, user, profile);
                    var rptSource = new List<TransactionReportDS>();
                    rptSource.Add(rpt);
                    HttpContext.Current.Session["rptSource"] = rptSource;
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
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<CashDesk> GetAccountByAccountNumberSearch(string customerId)
        {
            try
            {

                var cusResponseObject = await GetCustomerAccounts(customerId);
                if (cusResponseObject.Any())
                {
                    var Accounts = cusResponseObject;
                    var customer = await GetCustomer(customerId);
                    var branch = await _branchServices.GetBranch(customer.branchId);
                    var loans = await _loanServices.GetLoanByCustomerID(customer.customerId);
                    customer.name = $"{customer.firstName} {customer.lastName}";
                    var cashDesk = new CashDesk { Branch = branch, Accounts = Accounts, BulkDeposit = new BulkDeposit(), BulkDeposits = BuidObject(Accounts), Customer = customer, LoanId = null, CustomerId = customerId, Loans= loans.ToList() };
                    return cashDesk;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        
        public async Task<CashDesk> GetMember(string customerId)
        {
            try
            {

                var cusResponseObject = await GetCustomer(customerId);
                if (cusResponseObject != null)
                {
                    var customer = cusResponseObject;
                    var branch = await _branchServices.GetBranch(customer.branchId);
                    customer.name = $"{customer.firstName} {customer.lastName}";
                    var cashDesk = new CashDesk { Branch = branch, Accounts = null, BulkDeposit = new BulkDeposit(), BulkDeposits = new List<BulkDeposit>(), Customer = customer, LoanId = null, CustomerId = customerId };
                    return cashDesk;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<CashDesk> GetMembers()
        {
            try
            {

                var cusResponseObject = await _individualProfileServices.GetMembers();
                if (cusResponseObject != null)
                {
                    var customer = cusResponseObject;
                    var cashDesk = new CashDesk { Customers= cusResponseObject.ToList() };
                    return cashDesk;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<BulkDeposit> BuidObject(List<CustomerAccount> accounts)
        {
            return accounts.Select(a => new BulkDeposit
            {
                AccountNumber = a.accountNumber,
                AccountType = a.accountType,
                Amount = 0,
                Balance = a.balance,
                currencyNotes = new CurrencyNotes(),
                CustomerId = a.customerId,
                Fee = 0,
                Interest = 0,
                LoanId = null,
                Penalty = 0,
                Total = 0
            }).ToList();
        }

    }

}
