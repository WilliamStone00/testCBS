
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Accounts
{
    public class TransactionResversalServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public TransactionResversalServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_GetReversalRequest, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(inResponse, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public List<SelectListItem> GetReasons()
        {
            // Populate a more extensive list of predefined reasons for transaction reversal (Cash Desk Member Operations)
            var reasons = new List<SelectListItem>
    {
        new SelectListItem { Text = "Transaction Error - Incorrect amount processed", Value = "Transaction Error - Incorrect amount processed" },
        new SelectListItem { Text = "Customer Request - Cancellation of member transaction", Value = "Customer Request - Cancellation of member transaction" },
        new SelectListItem { Text = "Overcharged - Member charged more than the allowed amount", Value = "Overcharged - Member charged more than the allowed amount" },
        new SelectListItem { Text = "Refund Request - Member requests refund due to an issue", Value = "Refund Request - Member requests refund due to an issue" },
        new SelectListItem { Text = "System Error - System failure during member transaction", Value = "System Error - System failure during member transaction" },
        new SelectListItem { Text = "Duplicate Transaction - Same transaction processed twice for member", Value = "Duplicate Transaction - Same transaction processed twice for member" },
        new SelectListItem { Text = "Authorization Failure - Member’s transaction not authorized", Value = "Authorization Failure - Member’s transaction not authorized" },
        new SelectListItem { Text = "Fraud Detection - Suspicious activity detected in member account", Value = "Fraud Detection - Suspicious activity detected in member account" },
        new SelectListItem { Text = "Incorrect Member Account - Transaction linked to wrong member account", Value = "Incorrect Member Account - Transaction linked to wrong member account" },
        new SelectListItem { Text = "Transaction Reversal - Member account debited incorrectly", Value = "Transaction Reversal - Member account debited incorrectly" },
        new SelectListItem { Text = "Member Funds Error - Insufficient funds in member account", Value = "Member Funds Error - Insufficient funds in member account" },
        new SelectListItem { Text = "Transaction Timeout - Member transaction failed due to timeout", Value = "Transaction Timeout - Member transaction failed due to timeout" },
        new SelectListItem { Text = "Payment Method Error - Invalid payment method used by member", Value = "Payment Method Error - Invalid payment method used by member" },
        new SelectListItem { Text = "Cashier Input Error - Incorrect data entered by cashier for member", Value = "Cashier Input Error - Incorrect data entered by cashier for member" },
        new SelectListItem { Text = "Member Balance Error - Incorrect member balance calculated", Value = "Member Balance Error - Incorrect member balance calculated" },
        new SelectListItem { Text = "Reversal Request - Member requests reversal due to an operational issue", Value = "Reversal Request - Member requests reversal due to an operational issue" },
        new SelectListItem { Text = "Account Mismatch - Member’s account information mismatch", Value = "Account Mismatch - Member’s account information mismatch" },
        new SelectListItem { Text = "Deposit Error - Incorrect deposit recorded for member", Value = "Deposit Error - Incorrect deposit recorded for member" },
        new SelectListItem { Text = "Withdrawal Error - Incorrect withdrawal processed for member", Value = "Withdrawal Error - Incorrect withdrawal processed for member" },
        new SelectListItem { Text = "Currency Mismatch - Currency used does not match member account", Value = "Currency Mismatch - Currency used does not match member account" },
        new SelectListItem { Text = "Other - Specify reason in incident note for member operation", Value = "Other - Specify reason in incident note for member operation" }
    };

            return reasons;
        }

        public async Task<IEnumerable<ReversalRequest>> GetReversalRequests(GetAllReversalRequestQuery getAllReversalRequestQuery)
        {
            try
            {
                getAllReversalRequestQuery.DateFrom = getAllReversalRequestQuery.DateFrom ?? "N/A";
                getAllReversalRequestQuery.DateTo = getAllReversalRequestQuery.DateTo ?? "N/A";
                var couApiResponse = await _transactionApiHelper.PostAsync<ServiceResponse<List<ReversalRequest>>>(APICallHelper.GetAllReversalRequest, getAllReversalRequestQuery);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<ReversalRequest>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ReversalRequest> GetReversalRequest(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<ReversalRequest>>(string.Format(APICallHelper.Delete_GetReversalRequest, id));
                if (cusResponseObject.IsSuccess)
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
        public async Task<ExecutionMessages> Create(AddReversalRequestCommand model)
        {
            try
            {
                
                // Make an API call to create an individual profile
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<ReversalRequest>>(APICallHelper.CreateReversalRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Validate(ValidationReversalRequestCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<ReversalRequest>>(APICallHelper.ValidateReversalRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Approve(ApprovedReversalRequestCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<ReversalRequest>>(APICallHelper.ApprovedReversalRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> TreateTransaction(CashCompletionOfReversalCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<ReversalRequest>>(APICallHelper.TreatRequestReversalRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
    }

}
