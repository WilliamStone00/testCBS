
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
   
    public class CashCeilingRequestServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public CashCeilingRequestServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_CashCeilingRequest, id));
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
        public async Task<IEnumerable<CashCeilingRequest>> GetCashCeilingRequests(GetAllCashCeilingRequestsQuery allCashCeilingRequestsQuery)
        {
            try
            {
                var queryString = ToQueryString(allCashCeilingRequestsQuery);
                var fullUrl = $"{APICallHelper.GetAllCashCeilingRequest}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<CashCeilingRequest>>>(fullUrl);
                var data = new List<CashCeilingRequest>();
                if (response.ApiResponseData != null)
                {
                    data = response.ApiResponseData.Data;
                }
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<CashCeilingRequest>> GetCashCeilingRequests(string userId,string Status,string requestType)
        {
            try
            {
                GetAllCashCeilingRequestsQuery allCashCeilingRequestsQuery = new GetAllCashCeilingRequestsQuery { BranchId=GetBranchID(), Status=Status, UserId=userId, RequestType=requestType };
                var queryString = ToQueryString(allCashCeilingRequestsQuery);
                var fullUrl = $"{APICallHelper.GetAllCashCeilingRequest}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<CashCeilingRequest>>>(fullUrl);
                var data = new List<CashCeilingRequest>();
                if (response.ApiResponseData != null)
                {
                    data = response.ApiResponseData.Data;
                }
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<CashCeilingRequest> GetCashCeilingRequest(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<CashCeilingRequest>>(string.Format(APICallHelper.Get_CashCeilingRequest, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data= cusResponseObject.ApiResponseData.Data;
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(AddCashCeilingRequestCommand model)
        {
            try
            {
               
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<CashCeilingRequest>>(APICallHelper.CreateCashCeilingRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.CashoutRequestAmount}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.CashoutRequestAmount}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ValidateRequest(ValidationCashCeilingRequestCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<CashCeilingRequest>>(APICallHelper.ValidateCashCeilingRequest, model);
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
        public async Task<ExecutionMessages> Update(AddCashCeilingRequestCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<CashCeilingRequest>>(APICallHelper.Update_CashCeilingRequest, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.CashoutRequestAmount}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.CashoutRequestAmount}", MessagesResults.Failed,
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
