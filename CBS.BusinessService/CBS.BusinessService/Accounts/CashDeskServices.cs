using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;

using CBS.FrontDesk.Data.Entity.CustomerManagement;
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
using CBS.FrontDesk.Data.Entity.LoanConf;

namespace CBS.BusinessService.Accounts
{

    public class CashDeskServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly UserManagementServices _userManagementServices;
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
            _loanServices = loanServices;
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
                    Fee = t.OriginalDepositAmount-t.Amount,
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
                    ProductName = t.Account.Product.name,
                    RecieverName = "",
                    SenderName = "",
                    RecievingBranch = "",
                    SendingBranch = "",
                    SourceBranchCommission = t.SourceBranchCommission,
                    SourceType = t.SourceType,
                    Status = t.Status, 
                    ReceiptTitle=t.ReceiptTitle,
                    BarCode = BarCodeHelper.GenerateBarcodeImage($"{c.customerId}-{t.TransactionReference}-{t.OriginalDepositAmount}"),

                };
                return rpt;

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

        public async Task<ExecutionMessages> BulkDeposi(List<BulkDeposit> bulkDeposits)
        {
            try
            {
                if (bulkDeposits.FirstOrDefault().OperationType == "Withdrawal")
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

        public async Task<CashDesk> GetAccountByAccountNumberSearch(string customerId, string path)
        {
            try
            {

                var cusResponseObject = await GetCustomerAccounts(customerId);
                if (cusResponseObject.Any())
                {
                    var Accounts = cusResponseObject;
                    var customer = await GetCustomer(customerId);
                    var branch = await _branchServices.GetBranch(customer.branchId);
                    var loans=new List<Loan>();
                    if (path== "repayment" || path == "F5")
                    {
                        loans = (from a in await _loanServices.GetLoanByCustomerID(customer.customerId) select a).ToList();
                        
                    }
                    customer.name = $"{customer.firstName} {customer.lastName}";
                    var cashDesk = new CashDesk { Branch = branch, Accounts = Accounts, BulkDeposit = new BulkDeposit(), BulkDeposits = BuidObject(Accounts), Customer = customer, LoanId = null, CustomerId = customerId, Loans = loans.ToList() };
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
                    var cashDesk = new CashDesk { Customers = cusResponseObject.ToList() };
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
                AccountType = a.product.name,
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
