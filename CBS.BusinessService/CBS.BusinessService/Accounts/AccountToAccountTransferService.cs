using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.BusinessService.Accounts
{
    public class AccountToAccountTransferService : BaseService
    {
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly BranchServices _branchServices;
        private readonly AccountServices _accountServices;
        private readonly ApiCallerHelper _transactionApiHelper;
        public AccountToAccountTransferService(
            IndividualProfileServices individualProfileServices,
            BranchServices branchServices,
            AccountServices accountServices,
            UserManagementServices userManagementServices)
        {
            _individualProfileServices = individualProfileServices;
            _branchServices = branchServices;
            _accountServices = accountServices;
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        /// <summary>
        /// Builds the full AccountToAccountTransferDto with sender preloaded.
        /// </summary>
        public async Task<AccountToAccountTransferDto> GetAccountToAccountTransfer(string senderRef)
        {
            var senderInfo = await BuildCustomerTransferInfo(senderRef);
            if (senderInfo == null)
                return null;

            return new AccountToAccountTransferDto
            {
                Sender = new SenderInfoDto
                {
                    Customer = senderInfo.Customer,
                    Branch = senderInfo.Branch,
                    Accounts = senderInfo.Accounts
                },
                Receiver = new ReceiverInfoDto(), // to be filled later
                Transfer = new TransferRequest()
            };
        }

        /// <summary>
        /// Builds just the receiver info block (used after searching).
        /// </summary>
        public async Task<ReceiverInfoDto> GetReceiverInfo(string receiverRef)
        {
            return await BuildCustomerTransferInfo(receiverRef);
        }

        /// <summary>
        /// Shared logic for both sender and receiver lookup.
        /// </summary>
        private async Task<ReceiverInfoDto> BuildCustomerTransferInfo(string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber) || referenceNumber.Length != 10)
                return null;

            // 1. Customer
            var customer = await _individualProfileServices.GetSingleCustomer(referenceNumber);
            if (customer == null)
                return null;

            // 2. Branch
            var branch = await _branchServices.GetBranch(customer.BranchId);
            if (branch == null)
                return null;

            // 3. Accounts (exclude loans)
            var rawAccounts = await _accountServices.GetAllCustomerAccountsByCustomerId(customer.CustomerId);
            var accounts = rawAccounts
                .Where(a => !a.AccountType.ToLower().Contains("loan"))
                .ToList();

            if (!accounts.Any())
                return null;

            // 4. Format name for display
            customer.name = $"{customer.FirstName} {customer.LastName}";

            return new ReceiverInfoDto
            {
                Customer = customer,
                Branch = branch,
                Accounts = accounts
            };
        }
        public async Task<ExecutionMessages> SubmitTransferRequest(TransferRequest transferRequest)
        {
            try
            {
                if (transferRequest == null)
                {
                    return GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.MissingData, SystemMessageStatus.Failed.ToString(), null, "Transfer data is missing.");
                }
                return await Transfer(transferRequest); // Reuse your existing Transfer method
            }
            catch (Exception ex)
            {
                return GetExecutionMessages(transferRequest, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex, "Unexpected error during transfer.");
            }
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
        public Branch RetrieveBranchFromSession()
        {
            Branch branch = HttpContext.Current.Session["BranchObject"] as Branch; // Retrieve the Branch object from session
            if (branch != null)
            {
                return branch;
            }
            return null;
        }
        public async Task<ExecutionMessages> TransferConfirmation(TransferConfirmation model)
        {
            try
            {

                var response = await _transactionApiHelper.PostAsync<ServiceResponse<PaymentReceipt>>(APICallHelper.TransferConfirmation, model);
                if (response.ApiResponseData != null)
                {
                    if (model.Status == "Approved")
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

      

    }

}
