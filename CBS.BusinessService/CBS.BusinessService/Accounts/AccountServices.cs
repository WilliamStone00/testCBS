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
using CBS.BusinessService.Config;
using System.IO;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using CBS.FrontDesk.Data;
using Account = CBS.FrontDesk.Data.Entity.SavingProducts.Account;

namespace CBS.BusinessService.Accounts
{

    public class AccountServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly UserManagementServices _userManagementServices;
        private readonly ApiCallerHelper _BranchConfigApiHelper;
        private readonly IndividualProfileServices _individualProfileServices;

        private readonly BranchServices _branchServices;
        public AccountServices(UserManagementServices userManagementServices = null, IndividualProfileServices individualProfileServices = null, BranchServices branchServices = null, ApiCallerHelper branchConfigApiHelper = null)
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _userManagementServices = userManagementServices;
            _individualProfileServices = individualProfileServices;
            _branchServices = branchServices;
            _BranchConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        //public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string path)
        //{
        //    Func<Task<List<CustomerAccountDto>>> getDataFunc = async () => (await GetCustomersAccounts(path)).ToList();
        //    var dataTable = await DatatableHelper.GenerateDataTable<CustomerAccountDto>(dataTableOptions, getDataFunc);
        //    return dataTable;
        //}

        public async Task<CustomDataTable> GetDataTableSearch(DataTableOptions dataTableOptions, string searchCriterial)
        {
            //CustomerSearch
            var customers = await GetCustomersAccounts("all");
            Func<Task<List<CustomerAccountDto>>> getDataFunc = async () => (await CustomerSearch(customers.ToList(), searchCriterial)).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<CustomerAccountDto>(dataTableOptions, getDataFunc);
            return dataTable;
        }




        public async Task<IEnumerable<CustomerAccountDto>> GetCustomersAccountsForTransfter(string searchCriterial = "All")
        {
            try
            {
                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(APICallHelper.AllCustomers);
                var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(APICallHelper.GetAllAccounts);
                var branchesx = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                var branches = branchesx.ApiResponseData.Data;
                // Join individual profiles with accounts and map to DTOs
                var data = (from a in individualProfiles.ApiResponseData.Data
                            join ca in accounts.ApiResponseData.Data on a.CustomerId equals ca.customerId
                            join b in branches on a.BranchId equals b.Id
                            select MapCustomersToAccounts(a, ca, b)).ToList();

                // Filter data based on the 'path' parameter
                //var distinctData = data.GroupBy(x => x.customerId).Select(g => g.First());
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ExecutionMessages> MakeInitialDeposit(AccountDepositRequest model)
        {
            try
            {
                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                model.depositType = "CASH_INITIAL_DEPOSIT";
                if (!ComputeDenomination(model.currencyNotes, model.amount))
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

        public async Task UploadMembersAccount(AccountMigrationCommand accountMigrationCommand)
        {
            var branch = await _branchServices.GetBranch(accountMigrationCommand.BranchId);

            var cusResponseObject = await _transactionApiHelper.PostAsync<ResponseObject<bool>>(APICallHelper.AccountMigration, accountMigrationCommand);
            if (cusResponseObject.ApiResponseData != null && cusResponseObject.IsSuccess)
            {
                GetExecutionMessages(cusResponseObject.ApiResponseData.Data, true, $"{branch.Name} Members account migration", MessagesResults.Success,
                    ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, cusResponseObject.Message);
            }
            else
            {
                GetExecutionMessages(accountMigrationCommand, false, $"{branch.Name} Members account migration", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, cusResponseObject.Message);
            }


        }


        public async Task<AccountMigrationCommand> ExtractFile(MemberAccountUpload model)
        {
            if (model.File != null && model.File.ContentLength > 0)
            {
                // Check if the file is an Excel file
                if (Path.GetExtension(model.File.FileName).Equals(".xls") || Path.GetExtension(model.File.FileName).Equals(".xlsx"))
                {
                    try
                    {
                        using (var stream = model.File.InputStream)
                        {
                            // Call the method to read the Excel file and convert it to a list of Data objects
                            var dataList = ReadExcelFile(stream);
                            var branch = await _branchServices.GetBranch(model.BranchId);
                            var accountMigration = new AccountMigrationCommand { BranchId = branch.Id, BranchCode = branch.BranchCode, BankId = branch.Bank.Id, Accounts = dataList, ProductId = model.ProductId };
                            return accountMigration;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        // Handle the error gracefully
                        throw new InvalidOperationException("An error occurred while extracting data from the file.", ex);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Please upload a valid Excel file.");
                }
            }
            else
            {
                throw new InvalidOperationException("No file was uploaded.");
            }
        }


        //public async Task<ExecutionMessages> UploadMembersAccount(MemberAccountUpload model)
        //{
        //    if (model.File != null && model.File.ContentLength > 0)
        //    {
        //        // Check if the file is an Excel file
        //        if (Path.GetExtension(model.File.FileName).Equals(".xls") || Path.GetExtension(model.File.FileName).Equals(".xlsx"))
        //        {
        //            using (var stream = model.File.InputStream)
        //            {
        //                // Call the method to read the Excel file and convert it to a list of Data objects
        //                var dataList = ReadExcelFile(stream);
        //                var branch = await _branchServices.GetBranch(model.BranchId);
        //                var accountMigration = new AccountMigrationCommand { BranchId = branch.Id, BranchCode = branch.BranchCode, BankId = branch.Bank.Id, Accounts = dataList, ProductId = model.ProductId };
        //                var cusResponseObject = await _transactionApiHelper.PostAsync<ResponseObject<bool>>(APICallHelper.AccountMigration, accountMigration);
        //                if (cusResponseObject.ApiResponseData != null && cusResponseObject.IsSuccess)
        //                {
        //                    GetExecutionMessages(cusResponseObject.ApiResponseData.Data, true, $"{branch.Name} Members account migration", MessagesResults.Success,
        //                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, cusResponseObject.Message);
        //                    return ExecutionMessage;
        //                }
        //                // Failed creation
        //                return GetExecutionMessages(model, false, $"{branch.Name} Members account migration", MessagesResults.Failed,
        //                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, cusResponseObject.Message);
        //            }
        //        }
        //        else
        //        {
        //            return GetExecutionMessages(model, false, "Members account migration", MessagesResults.Failed, ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"Please upload a valid Excel file.");
        //        }
        //    }
        //    else
        //    {
        //        return GetExecutionMessages(model, false, "Members account migration", MessagesResults.Failed, ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "No file was uploaded.");

        //    }
        //}

        private List<Data> ReadExcelFile(Stream stream)
        {
            var dataList = new List<Data>();

            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var data = new Data
                    {
                        CustomerId = row.Cell(1).GetString(),
                        OpeningBalance = row.Cell(2).GetValue<decimal>()
                    };

                    dataList.Add(data);
                }
            }

            return dataList;
        }
        public async Task<IEnumerable<CustomerAccountDto>> GetCustomersAccounts(string path = null, string searchCriterial = "All")
        {
            try
            {
                if (path == "all")
                {
                    // Fetch individual profiles and accounts for head office
                    var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(string.Format(APICallHelper.SearchByAnyCriterialQuery, searchCriterial));
                    var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(APICallHelper.GetAllAccounts);
                    var branchesx = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                    var branches = branchesx.ApiResponseData.Data;
                    // Join individual profiles with accounts and map to DTOs
                    var data = (from a in individualProfiles.ApiResponseData.Data
                                join ca in accounts.ApiResponseData.Data on a.CustomerId equals ca.customerId
                                join b in branches on a.BranchId equals b.Id
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
                                join ca in accounts.ApiResponseData.Data on a.CustomerId equals ca.customerId
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

        public async Task<IEnumerable<CustomerAccount>> GetAllRemittanceAccounts(string branchid)
        {
            try
            {
                var accounts = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(string.Format(APICallHelper.GetAllRemittanceAccounts, branchid));
                if (accounts.ApiResponseData!=null)
                {
                    return accounts.ApiResponseData.Data;

                }
                return new List<CustomerAccount>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<SelectList> GetAllRemittanceAccountsDroupDown(List<CustomerAccount> customerAccounts, string transfterType)
        {
            try
            {
                var accounts = new List<CustomerAccount>();
                if (transfterType=="Local_Remittance")
                {
                    accounts=customerAccounts.Where(x => x.accountType==RemittanceTypes.TrustSoftCredit.ToString()).ToList();
                }
                else
                {
                    accounts=customerAccounts.Where(x => x.accountType!=RemittanceTypes.TrustSoftCredit.ToString()).ToList();
                }
                // Filter and map the data to the list of SelectListItem
                var values = accounts
                    .Select(x => new SelectListItem
                    {
                        Text = $"[Remittance Type: {x.accountType}], [Account Number: {x.accountNumber}], [Balance: {x.balance}]",
                        Value = x.accountType
                    })
                    .ToList();

                // Default selected value (adjust as per your needs)
                var defaultSelectedValue = "default-value";

                // Return the SelectList
                return new SelectList(values, "Value", "Text", defaultSelectedValue);

               
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while getting the loan products dropdown.");

                // Handle the exception accordingly
                throw; // Re-throw the exception after logging
            }
        }

        public async Task<CustomerAccount> GetRemittanceAccountByTypeQuery(string branchid, string accountType)
        {
            try
            {
                GetRemittanceAccountByTypeQuery getRemittanceAccountByType = new GetRemittanceAccountByTypeQuery { AccountType=accountType, BranchId=branchid };
                var apiResponse = await _transactionApiHelper.PostAsync<ResponseObject<CustomerAccount>>(APICallHelper.GetRemittanceAccount, getRemittanceAccountByType);
                if (apiResponse != null)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new CustomerAccount();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<List<TransactionHistory>> GetTransactionsAsync(GetAllTransactionsByDatesAndBranchQuery allTransactionsByDatesAndBranchQuery)
        {
            try
            {
                var apiResponse = await _transactionApiHelper.PostAsync<ResponseObject<List<TransactionHistory>>>(APICallHelper.GetTransactionsByQueryParameters, allTransactionsByDatesAndBranchQuery);
                if (apiResponse != null)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<TransactionHistory>();
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
        public async Task<IEnumerable<TransactionHistoryExport>> GetTransactionHistoryExports(List<TransactionHistory> transactionHistories)
        {
            try
            {

                var accounts = await MapTransactionHistoryToExport(transactionHistories);
                return accounts;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<List<TransactionHistoryExport>> MapTransactionHistoryToExport(List<TransactionHistory> transactionHistories)
        {
            var branches = (await _branchServices.GetBranches()).ToDictionary(b => b.Id); // Dictionary for quick lookups

            return transactionHistories
                .Select(transaction =>
                {
                    branches.TryGetValue(transaction.BranchId, out var branch); // Fast lookup

                    return new TransactionHistoryExport
                    {
                        MemberName = transaction.Account?.CustomerName,
                        AccountNumber = transaction.AccountNumber,
                        AccountType = transaction.Account?.AccountType,
                        CustomerReference = transaction.CustomerId,
                        Date = transaction.CreatedDate,
                        AccountingDate = transaction.AccountingDate,
                        Amount = transaction.Amount,
                        Fee = transaction.Fee,
                        Balance = transaction.Balance,
                        NewBalance = transaction.PreviousBalance,
                        Reference = transaction.TransactionReference,
                        TellerName = transaction.DailyTeller?.UserName,
                        TellerBranchName = transaction?.Branch?.Name,
                        Note = transaction.Note,
                        ThirdPartyName = transaction.DepositorName,
                        Operation = transaction.Operation,
                        WithdrawalFormCharge = transaction.WithrawalFormCharge,
                        OperationCharge = transaction.OperationCharge,
                        Debit = transaction.Debit,
                        Credit = transaction.Credit,
                        InterBranch = transaction.IsInterBrachOperation ? "Yes" : "No",
                        BankName = branch?.Bank?.Name ?? "Unknown Bank",
                        BranchCode = branch?.BranchCode ?? transaction?.Branch?.BranchCode,
                        BrnachName = branch?.Name ?? transaction?.Branch?.Name,
                        BranchTel = branch?.Telephone ?? transaction?.Branch?.Telephone
                    };
                })
                .OrderBy(t => t.Date) // Sort at the end
                .ToList();
        }



        public async Task<IEnumerable<StringValues>> SourceAndDestinationAccount()
        {
            try
            {
                var accounts = from a in await GetCustomersAccountsForTransfter()
                               select new StringValues
                               {
                                   Text = $"{a.accountNumber}-{a.productName}-{a.customerName} [{a.BranchName}]",
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
        public async Task<Account> GetMemberAccounByAccounId(string accountid)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<Account>>(string.Format(APICallHelper.GetMemberAccountByAccountID, accountid));
                if (cusResponseObject.ApiResponseData==null)
                {
                    return new Account();

                }
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<AccountBalance> GetAccountBalanceByAccountNumber(string accountNumber)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<AccountBalance>>(string.Format(APICallHelper.GetAccountBalanceByAccountNumber, accountNumber));
                if (cusResponseObject.ApiResponseData==null)
                {
                    return new AccountBalance();

                }
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<EndOfTheDay> GetTellerAccount(GetTellerAccountBalanceQuery getTellerAccountBalanceQuery, bool isOpen = true)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.PostAsync<ResponseObject<TellerProvioningHistory>>(APICallHelper.GetTellerAccountInfo, getTellerAccountBalanceQuery);
                if (cusResponseObject.ApiResponseData != null)
                {
                    if (isOpen)
                    {
                        var closeOfDayRequest = Mapper(cusResponseObject.ApiResponseData.Data, false);
                        var endofDay = new EndOfTheDay { CloseOfDayRequest = closeOfDayRequest, Teller = cusResponseObject.ApiResponseData.Data.Teller, CashAtHand = closeOfDayRequest.CashAtHand, HasError = false, ErrorMessage = null, AccountingDay = cusResponseObject.ApiResponseData.Data.OpenedDate };
                        return endofDay;
                    }
                    else
                    {
                        if (getTellerAccountBalanceQuery.IsPrimary)
                        {
                            var closeOfDayRequest = Mapper(cusResponseObject.ApiResponseData.Data, false);
                            var endofDay = new EndOfTheDay { CloseOfDayRequest = closeOfDayRequest, Teller = cusResponseObject.ApiResponseData.Data.Teller, CashAtHand = closeOfDayRequest.CashAtHand, HasError = false, ErrorMessage = null, AccountingDay = cusResponseObject.ApiResponseData.Data.OpenedDate };
                            return endofDay;
                        }
                        else
                        {
                            var closeOfDayRequest = Mapper(ResetDenominations(cusResponseObject.ApiResponseData.Data), false);
                            var endofDay = new EndOfTheDay { CloseOfDayRequest = closeOfDayRequest, Teller = cusResponseObject.ApiResponseData.Data.Teller, CashAtHand = closeOfDayRequest.CashAtHand, HasError = false, ErrorMessage = null, AccountingDay = cusResponseObject.ApiResponseData.Data.OpenedDate };
                            return endofDay;
                        }
                    }


                }
                return new EndOfTheDay { CashAtHand = 0, HasError = true, ErrorMessage = $"{cusResponseObject.Message}" };
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public TellerProvioningHistory ResetDenominations(TellerProvioningHistory tellerProvioningHistory)
        {
            // Reset Opening Notes and Coins Counts
            tellerProvioningHistory.OpeningNote10000 = 0;
            tellerProvioningHistory.OpeningNote5000 = 0;
            tellerProvioningHistory.OpeningNote2000 = 0;
            tellerProvioningHistory.OpeningNote1000 = 0;
            tellerProvioningHistory.OpeningNote500 = 0;
            tellerProvioningHistory.OpeningCoin500 = 0;
            tellerProvioningHistory.OpeningCoin100 = 0;
            tellerProvioningHistory.OpeningCoin50 = 0;
            tellerProvioningHistory.OpeningCoin25 = 0;
            tellerProvioningHistory.OpeningCoin10 = 0;
            tellerProvioningHistory.OpeningCoin5 = 0;
            tellerProvioningHistory.OpeningCoin1 = 0;

            // Reset Closing Notes and Coins Counts
            tellerProvioningHistory.ClosingNote10000 = 0;
            tellerProvioningHistory.ClosingNote5000 = 0;
            tellerProvioningHistory.ClosingNote2000 = 0;
            tellerProvioningHistory.ClosingNote1000 = 0;
            tellerProvioningHistory.ClosingNote500 = 0;
            tellerProvioningHistory.ClosingCoin500 = 0;
            tellerProvioningHistory.ClosingCoin100 = 0;
            tellerProvioningHistory.ClosingCoin50 = 0;
            tellerProvioningHistory.ClosingCoin25 = 0;
            tellerProvioningHistory.ClosingCoin10 = 0;
            tellerProvioningHistory.ClosingCoin5 = 0;
            tellerProvioningHistory.ClosingCoin1 = 0;

            // Optionally reset other denomination-related properties if needed
            return tellerProvioningHistory;
        }

        public CloseOfDayRequest Mapper(TellerProvioningHistory history, bool isOpen)
        {
            if (isOpen)
            {
                var data = new CloseOfDayRequest
                {
                    Amount = history.CashAtHand,
                    CashAtHand = history.CashAtHand,
                    ClossedStatus = history.ClossedStatus,
                    Comment = history.SubTellerComment,
                    CurrencyNotes = new CurrencyNotes
                    {
                        coin1 = history.OpeningCoin1,
                        coin10 = history.OpeningCoin10,
                        coin100 = history.OpeningCoin100,
                        coin25 = history.OpeningCoin25,
                        coin5 = history.OpeningCoin5,
                        coin50 = history.OpeningCoin50,
                        coin500 = history.OpeningCoin500,
                        note1000 = history.OpeningNote1000,
                        note10000 = history.OpeningNote10000,
                        note2000 = history.OpeningNote2000,
                        note500 = history.OpeningNote500,
                        note5000 = history.OpeningNote5000
                    },
                };
                return data;
            }
            else
            {
                var data = new CloseOfDayRequest
                {
                    Amount = history.CashAtHand,
                    CashAtHand = history.CashAtHand,
                    ClossedStatus = history.ClossedStatus,
                    Comment = history.SubTellerComment,
                    CurrencyNotes = new CurrencyNotes
                    {
                        coin1 = history.ClosingCoin1,
                        coin10 = history.ClosingCoin10,
                        coin100 = history.ClosingCoin100,
                        coin25 = history.ClosingCoin25,
                        coin5 = history.ClosingCoin5,
                        coin50 = history.ClosingCoin50,
                        coin500 = history.ClosingCoin500,
                        note1000 = history.ClosingNote1000,
                        note10000 = history.ClosingNote10000,
                        note2000 = history.ClosingNote2000,
                        note500 = history.ClosingNote500,
                        note5000 = history.ClosingNote5000
                    },
                };
                return data;
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
        public async Task<List<TransactionHistory>> GetCustomerTransactionsByCustomerNumberAndByDates(PrintDate printDate)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.PostAsync<ResponseObject<List<TransactionHistory>>>(APICallHelper.GetAllTransactionsByDatesAndCustomerIDQuery, printDate);
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
        private CustomerAccountDto MapCustomersToAccounts(IndividualProfile a, CustomerAccount caAccount, Branch b)
        {
            return new CustomerAccountDto
            {
                customerName = $"{a.FirstName} {a.LastName}",
                status = caAccount.status,
                phone = a.Phone,
                customerId = a.CustomerId,
                accountNumber = caAccount.accountNumber,
                accountId = caAccount.id,
                productId = caAccount.productId,
                productName = caAccount.product.Name,
                BranchId = caAccount.branchId,
                BranchName = b.Name,
                customerCode = a.CustomerCode,
                createdDate = caAccount.createdDate.ToString(),
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
                    CashierName = u.firstName + " " + u.lastName,
                    Coin1 = t.currencyNote.coin1,
                    Coin5 = t.currencyNote.coin5,
                    Coin10 = t.currencyNote.coin10,
                    Coin25 = t.currencyNote.coin25,
                    Coin50 = t.currencyNote.coin50,
                    Credit = t.Credit,
                    Debit = t.Debit,
                    Balance = t.Balance,
                    CustomerName = c.FirstName + " " + c.LastName,
                    Fee = t.Fee,
                    Note = t.Note,
                    OperationType = t.OperationType,
                    PreviousBalance = t.PreviousBalance,
                    Tax = t.Tax,
                    TellerName = t.Teller.name,
                    TransactionRef = t.TransactionReference,
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
                    ProductName = t.Account.Product?.Name ?? "N/A",
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
                    if (t.currencyNotes == null)
                    {
                        t.currencyNote = new CurrencyNotes();
                    }
                    else
                    {
                        t.currencyNote = CurrencyMapper.MapToCurrencyNotesRequest(t.currencyNotes);
                    }


                    //closingBalance += t.Credit - t.Debit;

                    TransactionReportDS rpt = new TransactionReportDS
                    {
                        AccountNumber = t.AccountNumber,
                        AccountType = t.Account.AccountType,
                        Amount = t.Amount,
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
                        Charges = t.Fee,
                        Debit = t.Debit,
                        Balance = t.Balance,
                        CustomerName = c.FirstName + " " + c.LastName,
                        Fee = t.Fee,
                        Note = t.Note,
                        OperationType = t.OperationType,
                        PreviousBalance = t.PreviousBalance,
                        Tax = t.Tax,
                        TellerName = t.Teller.name,
                        TransactionRef = t.TransactionReference,
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
                        ProductName = t.Account.Product.Name,
                        RecieverName = "",
                        SenderName = "",
                        RecievingBranch = "",
                        SendingBranch = "",
                        SourceBranchCommission = t.SourceBranchCommission,
                        SourceType = t.SourceType,
                        Status = t.Status,
                        OpeningBalance = openingBalance,
                        ClosingBalance = t.Account.Balance,
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
        public async Task<ExecutionMessages> Deposit(DepositRequest model)
        {
            try
            {
                if (!ComputeDenomination(model.currencyNotes, model.amount))
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
                    User user = await RetrieveUserFromSession(transaction.CreatedBy);
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
                    if (model.Status == "Approved")
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        IndividualProfile profile = await RetrieveCustomerFromSession(transaction.Account.CustomerId);
                        User user = await RetrieveUserFromSession(transaction.CreatedBy);
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
                        GetExecutionMessages(response, true, $"Transfer Canceled", MessagesResults.Success,
                          ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }

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
                if (!ComputeDenomination(model.currencyNotes, model.amount))
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
                    User user = await RetrieveUserFromSession(transaction.CreatedBy);
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
                if (!ComputeDenomination(model.CurrencyNotes, Convert.ToInt32(model.Amount)))
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
                    User user = await RetrieveUserFromSession(transaction.CreatedBy);
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
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_Account, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<CustomerAccount> GetAccount(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<CustomerAccount>>(string.Format(APICallHelper.GetAccount, id));
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return new CustomerAccount();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
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
                        customer.name = $"{customer.FirstName} {customer.LastName}";
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


    public class MemberAccountJob : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly ApiCallerHelper _BranchConfigApiHelper;
        private readonly BranchServices _branchServices;
        // Parameterless constructor required by Hangfire
        public MemberAccountJob()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
            _BranchConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());

        }

        public async Task<AccountMigrationCommand> ExtractFile(MemberAccountUpload model)
        {
            if (model.File != null && model.File.ContentLength > 0)
            {
                // Check if the file is an Excel file
                if (Path.GetExtension(model.File.FileName).Equals(".xls") || Path.GetExtension(model.File.FileName).Equals(".xlsx"))
                {
                    try
                    {
                        using (var stream = model.File.InputStream)
                        {
                            // Call the method to read the Excel file and convert it to a list of Data objects
                            var dataList = ReadExcelFile(stream);
                            var branch = await _branchServices.GetBranch(model.BranchId);
                            var accountMigration = new AccountMigrationCommand { BranchId = branch.Id, BranchCode = branch.BranchCode, BankId = branch.Bank.Id, Accounts = dataList, ProductId = model.ProductId };
                            return accountMigration;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        // Handle the error gracefully
                        throw new InvalidOperationException("An error occurred while extracting data from the file.", ex);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Please upload a valid Excel file.");
                }
            }
            else
            {
                throw new InvalidOperationException("No file was uploaded.");
            }
        }

        public async Task UploadMembersAccountx(AccountMigrationCommand accountMigrationCommand)
        {
            var branch = await _branchServices.GetBranch(accountMigrationCommand.BranchId);

            var cusResponseObject = await _transactionApiHelper.PostAsync<ResponseObject<bool>>(APICallHelper.AccountMigration, accountMigrationCommand);
            if (cusResponseObject.ApiResponseData != null && cusResponseObject.IsSuccess)
            {
                GetExecutionMessages(cusResponseObject.ApiResponseData.Data, true, $"{branch.Name} Members account migration", MessagesResults.Success,
                    ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, cusResponseObject.Message);
            }
            else
            {
                GetExecutionMessages(accountMigrationCommand, false, $"{branch.Name} Members account migration", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, cusResponseObject.Message);
            }


        }

        public async Task<ExecutionMessages> UploadMembersAccount(AccountMigrationCommand accountMigrationCommand)
        {
            try
            {




                var cusResponseObject = await _transactionApiHelper.PostAsync<ResponseObject<bool>>(APICallHelper.AccountMigration, accountMigrationCommand);
                var branch = await _branchServices.GetBranch(accountMigrationCommand.BranchId);
                if (cusResponseObject.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"{branch.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, cusResponseObject.Message);
                }
                else
                {
                    GetExecutionMessages(accountMigrationCommand, false, $"{branch.Name} Members account migration", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, cusResponseObject.Message);
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

        private List<Data> ReadExcelFile(Stream stream)
        {
            var dataList = new List<Data>();

            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var data = new Data
                    {
                        CustomerId = row.Cell(1).GetString(),
                        CustomerName = $"{row.Cell(2).GetString()} {row.Cell(3).GetString()}",
                        BranchCode = row.Cell(4).GetString(),
                        OpeningBalance = row.Cell(5).GetValue<decimal>()
                    };

                    dataList.Add(data);
                }
            }

            return dataList;
        }


    }

}
