using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeRequestService
{
    using CBS.API.Helper;
    using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
    using CBS.FrontDesk.Data.Message;
    using DocumentFormat.OpenXml.EMMA;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;

    public class ChequeRequestService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;

        public ChequeRequestService()
        {
            var baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<ExecutionMessages> CreateRequestAsync(ChequeBookRequest model)
        {
            try
            {
                model.bankId = "1";
                var response = await _apiHelper.PostAsync<ServiceResponse<ChequeBookRequest>>(APICallHelper.CreateChequeRequest, model);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Cheque Request", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, "Cheque Request", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Cheque Request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<List<ChequeBookRequest>> GetAllRequestsAsync()
        {
            var response = await _apiHelper.GetAsync<ResponseObject<List<ChequeBookRequest>>>(APICallHelper.GetAllChequeRequests);
            return response?.ApiResponseData?.Data ?? new List<ChequeBookRequest>();
        }

        public async Task<ChequeBookRequest> GetRequestByIdAsync(string requestId)
        {
            string url = string.Format(APICallHelper.GetChequeRequestById, requestId);
            var response = await _apiHelper.GetAsync<ResponseObject<ChequeBookRequest>>(url);
            return response?.ApiResponseData?.Data;
        }

        public async Task<ExecutionMessages> ApproveRequestAsync(string requestId, string approvalNote)
        {
            try
            {
                string url = string.Format(APICallHelper.ApproveChequeRequest, requestId);
                var payload = new { ApprovalNote = approvalNote, ApprovedBy = GetUserFullName() };
                var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(url, payload);
                if (response.IsSuccess && response.ApiResponseData.Data)
                {
                    GetExecutionMessages(null, true, $"Request ID: {requestId}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Request approved successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Request ID: {requestId}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex) 
            {
               
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> RejectRequestAsync(string requestId, string rejectionNote)
        {
            try
            {
                string url = string.Format(APICallHelper.RejectChequeRequest, requestId);
                var payload = new { RejectionNote = rejectionNote, RejectedBy = GetUserFullName() };
                var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(url, payload);
                if (response.IsSuccess && response.ApiResponseData.Data)
                {
                    GetExecutionMessages(null, true, $"Request ID: {requestId}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Request rejected successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Request ID: {requestId}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex) { /* ... Error Handling ... */ }
            return ExecutionMessage;
        }
    }
}
