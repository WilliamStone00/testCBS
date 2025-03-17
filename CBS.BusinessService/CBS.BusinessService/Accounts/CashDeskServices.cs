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
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.Accounting;
using Irony.Parsing;
using System.Web.Mvc;
using CBS.BusinessService.Application;
using DocumentFormat.OpenXml.Bibliography;
using CBS.FrontDesk.Data.Entity.MemberNoneCashOperationsP;

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
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly RemittanceServices _remittanceServices;
        private readonly AccountServices _accountServices;

        public CashDeskServices(UserManagementServices userManagementServices = null, IndividualProfileServices individualProfileServices = null, BranchServices branchServices = null, ApiCallerHelper branchConfigApiHelper = null, LoanServices loanServices = null, LoanApplicationServices loanApplicationServices = null, RemittanceServices remittanceServices = null, AccountServices accountServices = null)
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _userManagementServices = userManagementServices;
            _individualProfileServices = individualProfileServices;
            _branchServices = branchServices;
            _loanServices = loanServices;
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _remittanceServices=remittanceServices;
            _accountServices=accountServices;
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

        public async Task<IndividualProfile> GetCustomer(string id)
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
        private async Task<List<LoanApplicationFee>> GetLoanApplicationFeesPending(string customerId)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanApplicationFee>>>(string.Format(APICallHelper.LoanApplicationFeesPending, customerId));
                if (cusResponseObject.ApiResponseData != null)
                {
                    var loanApplications = cusResponseObject.ApiResponseData.Data.ToList();
                    return loanApplications;
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
                    AccountType = t.AccountType != null ? t.AccountType : "None Member's Account",
                    Amount = t.OriginalDepositAmount,
                    Telephone = c.Phone,
                    Address = c.Address,
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
                    CustomerName = c.FirstName + " " + c.LastName,
                    Fee = t.OriginalDepositAmount - t.Amount,
                    Note = t.Note,
                    OperationType = t.OperationType,
                    PreviousBalance = t.PreviousBalance,
                    Tax = t.Tax,
                    TellerName = t.Teller.name,
                    TransactionRef = t.TransactionReference,
                    TransactionType = t.Operation,
                    AccountName = t.Account != null ? t.Account.AccountName : "None Member",
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
                    ProductName = t.Account != null ? t.Account.Product.Name : "None Member",
                    RecieverName = "",
                    SenderName = "",
                    RecievingBranch = "",
                    SendingBranch = "",
                    SourceBranchCommission = t.SourceBranchCommission,
                    SourceType = t.SourceType,
                    Status = t.Status,
                    ReceiptTitle = t.ReceiptTitle,
                    BarCode = BarCodeHelper.GenerateBarcodeImage($"{c.CustomerId}-{t.TransactionReference}-{t.OriginalDepositAmount}"),

                };
                return rpt;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        public OtherTransactionDto MapToDto(Branch branch, OtherTransaction otherTransaction)
        {
            return new OtherTransactionDto
            {
                TransactionReference = otherTransaction.TransactionReference,
                EnventName = otherTransaction.EnventName,
                Description = otherTransaction.Description,
                TellerCode = otherTransaction.TellerId,
                Amount = otherTransaction.Amount,
                Debit = otherTransaction.Debit,
                Credit = otherTransaction.Credit,
                AccountNumber = otherTransaction.AccountNumber,
                Direction = otherTransaction.Direction,
                TransactionType = otherTransaction.TransactionType,
                SourceType = otherTransaction.SourceType,
                Naration = otherTransaction.Narration,
                AmountInWord = otherTransaction.AmountInWord,
                ReceiptTitle = otherTransaction.ReceiptTitle,
                CustomerId = otherTransaction.CustomerId,
                CreatedDate = otherTransaction.DateOfOperation, // Assuming CreatedDate and DateOfOPeration are the same
                ModifiedDate = DateTime.Now, // Assuming ModifiedDate is now
                EventCode = otherTransaction.EventCode,
                MemberName = otherTransaction.MemberName, // Assuming MemberName is the Name in OtherTransaction
                Logo = branch.LogoUrl,
                BranchName = branch.Name,
                BranchCode = branch.BranchCode,
                BranchAddress = branch.Address,
                BranchTelephone = branch.Telephone,
                HeadOfficeName = branch.IsHeadOffice ? branch.Name : null,
                HeadOfficeAddress = branch.IsHeadOffice ? branch.Address : null,
                HeadOfficeTelephone = branch.IsHeadOffice ? branch.HeadOfficeTelehoneNumber : null,
                HeadOfficeEmail = branch.IsHeadOffice ? branch.Email : null,
                HeadOfficeWebSite = branch.IsHeadOffice ? branch.WebSite : null,
                HeadOfficeInitial = branch.IsHeadOffice ? branch.BankInitial : null,
                HeadOfficeCode = branch.IsHeadOffice ? branch.Id : null, // Assuming HeadOfficeCode is Branch ID
                DateOfOPeration = otherTransaction.DateOfOperation,
                TellerName = otherTransaction.Teller?.name // Assuming Teller has a Name property
            };
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
        public static List<BulkDeposit> FilterByAmountGreaterThanZero(List<BulkDeposit> deposits)
        {

            return deposits.Where(deposit => deposit.Amount > 0 || deposit.Fee > 0 || deposit.Interest > 0 || deposit.Penalty > 0).ToList();
        }
        public async Task<ExecutionMessages> BulkDeposi(List<BulkDeposit> bulkDeposits1)
        {
            try
            {
                var bulkDeposits = FilterByAmountGreaterThanZero(bulkDeposits1);

                if (bulkDeposits.FirstOrDefault().OperationType == "Withdrawal")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, IsCashOperation = true, OperationType = "Withdrawal" };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.MakeWithdrawal, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                //SavingWithdrawalFormFee
                else if (bulkDeposits.FirstOrDefault().OperationType == "WithdrawalSWS")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, IsCashOperation = true, OperationType = "Withdrawal" };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.MakeWithdrawal, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "SavingWithdrawalFormFee")
                {
                    var cash = new CashDeskWithdrawalNotificationCommand { Id = bulkDeposits.FirstOrDefault().AccountNumber };
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<PaymentReceipt>>(string.Format(APICallHelper.PayinSavingWithdrawalNotification, cash.Id), cash);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                //LoanRepayment
                else if (bulkDeposits.FirstOrDefault().OperationType == "CashIn")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, IsCashOperation = true, OperationType = "Deposit" };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "RemittanceIN")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, IsCashOperation = true, OperationType = "Deposit", Id=bulkDeposits.FirstOrDefault().RemittanceId, DepositType="RemittanceIN" };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }



                //MobileMoneyNoneCashIn
                else if (bulkDeposits.FirstOrDefault().OperationType == "RemittanceOUT")
                {
                    string N = "N/A";
                    var remOut = bulkDeposits.FirstOrDefault();
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, IsCashOperation = true, OperationType = "RemittanceOUT", Id=remOut.RemittanceId, OTP=remOut.OTP, ReceiverPhoneNumber=remOut.ReceiverPhoneNumber, DepositType=N, Period=N, ReceiverAddress=remOut.ReceiverAddress, ReceiverCNI=remOut.ReceiverCNI, ReceiverCNIDateOfExpiration=remOut.ReceiverCNIDateOfExpiration, ReceiverCNIDateOfIssue=remOut.ReceiverCNIDateOfIssue, ReceiverCNIPlcaceOfIssue=remOut.ReceiverCNIPlcaceOfIssue, ReceiverName=remOut.ReceiverName, RemittanceAmount=remOut.RemittanceAmount, RemittanceDate=remOut.RemittanceDate, SenderAddress=remOut.SenderAddress, SenderName=remOut.SenderName, SenderPhoneNumber=remOut.SenderPhoneNumber, SenderSecretCode=remOut.SenderSecretCode };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.MakeWithdrawal, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "CashInMomocashCollection")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, IsCashOperation = false, OperationType = "Deposit", DepositType = "CashInMomocashCollection" };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                //else if (bulkDeposits.FirstOrDefault().OperationType == "MemberNoneCash")
                //{
                //    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, IsCashOperation = false, OperationType = "Deposit", DepositType = "MemberNoneCash" };
                //    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                //    if (response.ApiResponseData != null)
                //    {
                //        var transaction = response.ApiResponseData.Data;
                //        Branch branch = RetrieveBranchFromSession();
                //        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                //        HttpContext.Current.Session["rptSource"] = rptSource;
                //        GetExecutionMessages(response, true, null, MessagesResults.Success,
                //            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                //        return ExecutionMessage;
                //    }
                //    else
                //    {
                //        // Failed creation
                //        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                //            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                //    }
                //}
                //
                else if (bulkDeposits.FirstOrDefault().OperationType == "LoanRepaymentMomocashCollection")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, DepositType = "LoanRepaymentMomocashCollection", IsCashOperation = false, OperationType = "Deposit" };

                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "LoanRepaymentByLocalAccountNoneCash")
                {



                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits,  DepositType = "LoanRepaymentByLocalAccountNoneCash", IsCashOperation = false, OperationType = "Deposit"
                    , LoanToBeRefundeds= bulkDeposits.FirstOrDefault().LoanToBeRefundeds, AccountToBeDebiteds=bulkDeposits.FirstOrDefault().AccountToBeDebiteds


                    };
                  
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "LoanRepayment")
                {
                    var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, DepositType = "LoanRepayment", IsCashOperation = true, OperationType = "Deposit", };

                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                    if (response.ApiResponseData != null)
                    {
                        var transaction = response.ApiResponseData.Data;
                        Branch branch = RetrieveBranchFromSession();
                        var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "LoanFee")
                {
                    if (IsSinglePeriodKind(bulkDeposits))
                    {
                        var BulkOperation = new BulkOperation { BulkOperations = bulkDeposits, DepositType = "LoanFeePayment", Period = bulkDeposits.FirstOrDefault().Period, IsCashOperation = true, OperationType = "Deposit" };
                        var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.BulkDeposit, BulkOperation);
                        if (response.ApiResponseData != null)
                        {
                            var transaction = response.ApiResponseData.Data;
                            Branch branch = RetrieveBranchFromSession();
                            var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);
                            HttpContext.Current.Session["rptSource"] = rptSource;
                            GetExecutionMessages(response, true, null, MessagesResults.Success,
                                ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                            return ExecutionMessage;
                        }
                        else
                        {
                            // Failed creation
                            GetExecutionMessages(null, false, $"Loan Fee Payment", MessagesResults.Failed,
                                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                        }
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, $"Loan Fee Payment", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Only one kind of payment can be done at an instant. Its Either Before OR After Payment. Please update your select.");
                    }


                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "OtherCashIn")
                {
                    var a = bulkDeposits.FirstOrDefault();
                    var addOtherTransaction = new AddOtherTransactionCommand
                    {
                        AccountNumber = a.AccountNumber,
                        Amount = a.Amount,
                        CurrencyNotesRequest = a.currencyNotes,
                        CustomerId = (a.SourceType == "Member_Account" || (!string.IsNullOrEmpty(a.CustomerId) && a.SourceType != "Member_Account"))
                 ? a.CustomerId
                 : "N/A",
                        Direction = "",
                        Name = a.Period,
                        Naration = a.Note=string.IsNullOrEmpty(a.Note) ? "N/A": a.Note,
                        EnventName = a.EventCode,
                        EventCode = a.EventCode,
                        SourceType = a.SourceType,
                        TransactionType = "Income"
                    };

                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<OtherTransaction>>(APICallHelper.CreateOtherTransaction, addOtherTransaction);
                    if (response.ApiResponseData != null)
                    {
                        Branch branch = RetrieveBranchFromSession();
                        var rpt = MapToDto(branch, response.ApiResponseData.Data);
                        var rptSource = new List<OtherTransactionDto>();
                        rptSource.Add(rpt);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }

                else if (bulkDeposits.FirstOrDefault().OperationType == "MobileMoney")
                {
                    var a = bulkDeposits.FirstOrDefault();
                    var addOtherTransaction = new AddOtherTransactionMobileMoneyCommand
                    {
                        Amount = a.Amount,
                        CurrencyNotesRequest = a.currencyNotes,
                        MemberReference = a.CustomerId,
                        CustomerName = a.MemberName,
                        CNI = a.CNI,
                        IsCashOperation = true,
                        TellerCode = a.TellerCode != null ? a.TellerCode : "N/A",
                        SourceType = a.SourceType,
                        BookingDirection = a.BookingDirection,
                        OperationType = a.BookingDirection,
                        TelephoneNumber = a.TelephoneNumber
                    };

                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<OtherTransaction>>(APICallHelper.CreateOtherTransactionMobileMoney, addOtherTransaction);
                    if (response.ApiResponseData != null)
                    {
                        Branch branch = RetrieveBranchFromSession();
                        var rpt = MapToDto(branch, response.ApiResponseData.Data);
                        var rptSource = new List<OtherTransactionDto>();
                        rptSource.Add(rpt);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "MobileMoneyNoneCashIn")
                {



                    var data = bulkDeposits.FirstOrDefault();
                    var addNoneCashMobileMoneyCommand = new AddNoneCashMobileMoneyCommand { Amount=data.Amount, Charges=data.Fee, CustomerName=data.MemberName, MemberReference=data.CustomerId, OperationType=data.OperationType, ReceiverAccountNumber=data.AccountNumber, SourceType=data.SourceType, TelephoneNumber=data.TelephoneNumber, TellerCode=data.TellerCode, Note=data.Note };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.MobileMoneyNoneCashCashIn, addNoneCashMobileMoneyCommand);
                    if (response.ApiResponseData != null)
                    {

                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else if (bulkDeposits.FirstOrDefault().OperationType == "OtherCashInExpense")
                {
                    var a = bulkDeposits.FirstOrDefault();
                    var addOtherTransaction = new AddOtherTransactionCommand
                    {
                        AccountNumber = "N/A",
                        Amount = a.Amount,
                        CurrencyNotesRequest = a.currencyNotes,
                        CustomerId = a.SourceType == "Member_Account" ? a.CustomerId : "N/A",
                        Direction = "N/A",
                        Name = a.Period,
                        Naration = a.Note,
                        EnventName = a.EventCode,
                        EventCode = a.EventCode,
                        SourceType = a.SourceType,
                        TransactionType = "Expense"
                    };
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<OtherTransaction>>(APICallHelper.CreateOtherTransaction, addOtherTransaction);
                    if (response.ApiResponseData != null)
                    {
                        Branch branch = RetrieveBranchFromSession();
                        var rpt = MapToDto(branch, response.ApiResponseData.Data);
                        var rptSource = new List<OtherTransactionDto>();
                        rptSource.Add(rpt);
                        HttpContext.Current.Session["rptSource"] = rptSource;
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(null, false, null, MessagesResults.Failed,
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
        // Function to check if only one kind of period exists in the list
        public bool IsSinglePeriodKind(List<BulkDeposit> bulkDeposits)
        {
            // Create a HashSet to store unique periods
            HashSet<string> uniquePeriods = new HashSet<string>();

            // Iterate through each BulkDeposit and add its period to the HashSet
            foreach (var operation in bulkDeposits)
            {
                uniquePeriods.Add(operation.Period);
            }

            // If the HashSet contains only one element, return true, else return false
            return uniquePeriods.Count == 1;
        }
        public async Task<CashDesk> GetOtherCashDeskTransactions()
        {
            try
            {

                var Accounts = new List<CustomerAccount> { };
                var customer = new IndividualProfile { CustomerId = "N/A", name = "None Member" };
                var branch = await _branchServices.GetBranch(GetBranchID());
                //await _individualProfileServices.GetAllIndividualProfileLight();
                decimal amountRequested = 0;
                var cashDesk = new CashDesk { Branch = branch, Accounts = Accounts, BulkDeposit = new BulkDeposit { Amount = amountRequested }, BulkDeposits = BuidObject(Accounts), OtherTransaction = new OtherTransaction(), Customer = customer, LoanId = null, CustomerId = customer.CustomerId };
                return cashDesk;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<CashDesk> GetOtherCashDeskMobileMoney()
        {
            try
            {

                var Accounts = new List<CustomerAccount> { };
                var customer = new IndividualProfile { CustomerId = "N/A", name = "None Member" };
                var branch = await _branchServices.GetBranch(GetBranchID());
                //await _individualProfileServices.GetAllIndividualProfileLight();
                decimal amountRequested = 0;
                var cashDesk = new CashDesk { Branch = branch, Accounts = Accounts, BulkDeposit = new BulkDeposit { Amount = amountRequested }, BulkDeposits = BuidObject(Accounts), Customer = customer, LoanId = null, CustomerId = customer.CustomerId };
                return cashDesk;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
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
                    var branch = await _branchServices.GetBranch(customer.BranchId);
                    decimal amountRequested = 0;
                    customer.name = $"{customer.FirstName} {customer.LastName}";
                    var cashDesk = new CashDesk { Branch = branch, Accounts = Accounts, BulkDeposit = new BulkDeposit { Amount = amountRequested }, BulkDeposits = BuidObject(Accounts), Customer = customer, LoanId = null, CustomerId = customerId };
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


        public async Task<CashDesk> GetAccountByAccountNumberSearch(string customerId, string path)
        {
            try
            {

                var cusResponseObject = await GetCustomerAccounts(customerId);
                if (cusResponseObject == null)
                {
                    return null;
                }
                if (cusResponseObject.Any())
                {
                    var Accounts = cusResponseObject;
                    var customer = new IndividualProfile();
                    var branch = await _branchServices.GetBranch(Accounts.FirstOrDefault().branchId);
                    var loans = new List<Loan>();
                    var WithdrawalNotifications = new List<WithdrawalNotification>();
                    var loanApplicationFees = new List<LoanApplicationFee>();
                    decimal amountRequested = 0;
                    if (path == "F5")
                    {
                        //loans = (from a in await _loanServices.GetLoanByCustomerID(new GetAllLoanByCustomerIdQuery { CustomerId = customerId, QueryParameter = "Open" }) select a).ToList();

                    }
                    else if (path == "repayment")
                    {
                        loans = (from a in await _loanServices.GetLoanByCustomerID(new GetAllLoanByCustomerIdQuery { CustomerId = customerId, QueryParameter = "Open" }) select a).ToList();

                    }

                    else if (path == "withdrawalnotification")
                    {

                        if (Accounts != null)
                        {
                            var savingAccount = Accounts.FirstOrDefault(x => x.accountType == "Saving");
                            if (savingAccount != null && savingAccount.WithdrawalNotifications != null)
                            {
                                WithdrawalNotifications = savingAccount.WithdrawalNotifications.Where(x => x.IsNotificationPaid == false).ToList();
                            }
                        }
                    }
                    else if (path == "loanapplicationfeepayment")
                    {

                        loanApplicationFees = await GetLoanApplicationFeesPending(customerId);
                    }
                    //Biossing@1234_
                    else if (path == "cashout")
                    {
                        amountRequested = Accounts.FirstOrDefault(x => x.accountType == "Saving")?.WithdrawalNotifications.FirstOrDefault(x => x.IsNotificationPaid)?.AmountRequired ?? 0;

                    }
                    customer.name = $"{Accounts.FirstOrDefault().customerName}";
                    customer.CustomerId = customerId;
                    var cashDesk = new CashDesk { Branch = branch, Accounts = Accounts, BulkDeposit = new BulkDeposit { Amount = amountRequested, CheckNumber = "N/A", CheckName = "N/A" }, BulkDeposits = BuidObject(Accounts), Customer = customer, LoanId = null, CustomerId = customerId, Loans = loans.ToList(), WithdrawalNotifications = WithdrawalNotifications, LoanApplicationFees = loanApplicationFees };
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

        public async Task<CashDesk> GetCashDeskRemittance(Remittance remittance = null, string remittanceId = null)
        {
            try
            {
                // Fetch remittance if only the ID is provided
                if (remittance == null && !string.IsNullOrEmpty(remittanceId))
                {
                    remittance = await _remittanceServices.GetRemittance(remittanceId);
                }

                if (remittance == null)
                {
                    return null;
                }

                // Retrieve related data
                var account = await _accountServices.GetMemberAccounByAccounId(remittance.AccountId);
                var accounts = new List<Account> { account };
                var customer = await _individualProfileServices.GetSingleCustomer(account.CustomerId);
                var branch = await _branchServices.GetBranch(remittance.SourceBranchId);

                // Populate customer name
                customer.name = $"{customer.FirstName} {customer.LastName}";

                // Populate CashDesk object
                var cashDesk = new CashDesk
                {

                    Branch = branch,
                    MemberAccounts = accounts,
                    BulkDeposit = new BulkDeposit
                    {
                        Amount = remittance.Amount, // Handles 0 by default if not set in the Remittance object
                        CheckNumber = "N/A",
                        CheckName = "N/A",
                        IsPaid=remittance.Status=="Paid" ? true : false,
                        Fee = remittance.Status=="Paid" ? 0 : remittance.Fee
                    },
                    BulkDeposits = BuidObject(accounts, remittance),
                    Customer = customer,
                    LoanId = null,
                    CustomerId = customer.CustomerId,
                    RemittanceId = remittance.Id,
                    Remittance = remittance,
                    GenerateRemittanceOTPCommand=new GenerateRemittanceOTPCommand { ReceiverPhoneNumber=remittance.ReceiverPhoneNumber, RemittanceReference=remittance.TransactionReference },
                    SavingProduct=account.Product
                };

                return cashDesk;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<List<MembersLoanDto>> GetMembersLoans(string customerId, string queryParameter)
        {
            try
            {

                var loans = (from loan in await _loanServices.GetLoanByCustomerID(new GetAllLoanByCustomerIdQuery { CustomerId = customerId, QueryParameter = queryParameter })
                             select new MembersLoanDto
                             {
                                 Id = loan.Id,
                                 LoanApplicationId = loan.LoanApplicationId,
                                 Principal = loan.Principal,
                                 LoanAmount = loan.LoanAmount,
                                 InterestRate = loan.InterestRate,
                                 Paid = loan.Paid,
                                 Balance = loan.Balance,
                                 AccrualInterest = loan.AccrualInterest,
                                 Tax = loan.Tax,
                                 Penalty = loan.Penalty,
                                 LoanDate = loan.LoanDate.ToString("dd/MM/yyyy hh:mm:ss"),
                                 IsLoanDisbursed = loan.IsLoanDisbursted,
                                 CustomerId = loan.CustomerId,
                                 DueAmount = loan.DueAmount,
                                 LoanStatus = loan.LoanStatus,
                                 BranchCode = loan.BranchCode,
                                 CustomerName = loan.CustomerName,
                                 MaturityDate = loan.MaturityDate.ToString("dd/MM/yyyy hh:mm:ss"),
                                 NumberOfInstallments = loan.NumberOfInstallments,
                                 LoanType = loan.LoanType,
                                 RepaymentCycle = loan.RepaymentCycle

                             }).ToList();

                return loans;
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
                    var branch = await _branchServices.GetBranch(customer.BranchId);
                    customer.name = $"{customer.FirstName} {customer.LastName}";
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

        //public async Task<CashDesk> GetMembers()
        //{
        //    try
        //    {

        //        var cusResponseObject = await _individualProfileServices.GetMembers();
        //        if (cusResponseObject != null)
        //        {
        //            var customer = cusResponseObject;
        //            var cashDesk = new CashDesk { Customers = cusResponseObject.ToList() };
        //            return cashDesk;
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public List<BulkDeposit> BuidObject(List<CustomerAccount> accounts)
        {
            if (accounts.Any())
            {
                var selected = accounts.Select(a => new BulkDeposit
                {
                    AccountNumber = a.accountNumber,
                    AccountType = a.product.Name,
                    Amount = 0,
                    Balance = a == null ? 0 : a.balance,
                    currencyNotes = new CurrencyNotes(),
                    CustomerId = a.customerId,
                    Fee = 0,
                    Interest = 0,
                    ProductId=a.productId,
                    LoanId = null,
                    Penalty = 0,
                    Total = 0
                }).ToList();
                return selected;
            }
            else
            {
                // Create a default BulkDeposit object
                var defaultBulkDeposit = new BulkDeposit
                {
                    AccountNumber = "N/A",
                    AccountType = "N/A",
                    Amount = 0,
                    Balance = 0,
                    currencyNotes = new CurrencyNotes(),
                    CustomerId = "N/A",
                    Fee = 0,
                    Interest = 0,
                    LoanId = null,
                    Penalty = 0,
                    Total = 0
                };

                // Return a list containing the default BulkDeposit object
                return new List<BulkDeposit> { defaultBulkDeposit };

            }

        }
        public List<BulkDeposit> BuidObject(List<Account> accounts, Remittance remittance)
        {
            if (accounts.Any())
            {

                if (remittance.Status=="Paid")
                {
                    var selected = accounts.Select(a => new BulkDeposit
                    {
                        AccountNumber = a.AccountNumber,
                        AccountType = remittance.RemittanceType,
                        Amount = remittance.ChargeType!="Exclussive" ? remittance.Amount-remittance.Fee : remittance.Amount,
                        Balance = a == null ? 0 : a.Balance,
                        currencyNotes = new CurrencyNotes(),
                        CustomerId = a.CustomerId,
                        Fee = 0,
                        Interest = 0,
                        LoanId = null,
                        Penalty = 0,
                        Total = 0,
                        RemittanceId=remittance.Id
                    }).ToList();
                    return selected;
                }
                else
                {
                    var selected = accounts.Select(a => new BulkDeposit
                    {
                        AccountNumber = a.AccountNumber,
                        AccountType = remittance.RemittanceType,
                        Amount = remittance.Amount,
                        Balance = a == null ? 0 : a.Balance,
                        currencyNotes = new CurrencyNotes(),
                        CustomerId = a.CustomerId,
                        Fee = remittance.Fee,
                        Interest = 0,
                        LoanId = null,
                        Penalty = 0,
                        Total = 0,
                        RemittanceId=remittance.Id
                    }).ToList();
                    return selected;
                }

            }
            else
            {
                // Create a default BulkDeposit object
                var defaultBulkDeposit = new BulkDeposit
                {
                    AccountNumber = "N/A",
                    AccountType = "N/A",
                    Amount = 0,
                    Balance = 0,
                    currencyNotes = new CurrencyNotes(),
                    CustomerId = "N/A",
                    Fee = 0,
                    Interest = 0,
                    LoanId = null,
                    Penalty = 0,
                    Total = 0
                };

                // Return a list containing the default BulkDeposit object
                return new List<BulkDeposit> { defaultBulkDeposit };

            }

        }

        public async Task<SelectList> LoadMembersAccountByMemberReference(string customerid)
        {
            try
            {
                var customerAccounts = await GetCustomerAccounts(customerid);
                return LoadAccountsToList(customerAccounts);
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        private SelectList LoadAccountsToList(List<CustomerAccount> customerAccounts)
        {
            var defaultSelectedValue = "default-value";
            if (customerAccounts != null)
            {

                if (customerAccounts.Any())
                {
                    var charOfAccount = customerAccounts.Select(a => new StringValues
                    {
                        Text = $"[{a.accountNumber}]-[{a.accountType}], Balance: [{a.balance.ToString("#,##0.0")}]",
                        Value = a.accountNumber
                    });


                    return new SelectList(charOfAccount.ToList(), "Value", "Text", defaultSelectedValue);
                }

            }
            defaultSelectedValue = "Member not found";
            return new SelectList(new List<CustomerAccount>(), "Value", "Text", defaultSelectedValue);

        }
        public List<StringValues> LoadMembersToList(List<IndividualProfile> individuals)
        {
            var values = individuals.Select(a => new StringValues
            {
                Text = $"[{a.CustomerId}] [{a.FirstName} {a.LastName}] [{a.branch}]",
                Value = a.CustomerId
            });
            return values.ToList();

        }
    }

}
