using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeCancelation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeCancelation
{
    public class ChequeCancellationService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public ChequeCancellationService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<CancellationRequest>> GetCancellationRequestsAsync()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<CancellationRequest>>>(APICallHelper.GetAllCancellationRequests);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<CancellationRequest>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching cancellation requests: {ex.Message}");
            }
        }

        public async Task<CancellationRequest> GetCancellationRequestByIdAsync(string id)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.GetCancellationRequestById, id);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<CancellationRequest>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching cancellation request: {ex.Message}");
            }
        }

        public async Task<ExecutionMessages> CreateAsync(CancellationRequest model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<CancellationRequest>>(APICallHelper.CreateCancellationRequest, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Cancellation Request", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, "Cancellation Request", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Cancellation Request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(CancellationRequest model)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.UpdateCancellationRequest, model.Id);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<CancellationRequest>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Cancellation Request", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, "Cancellation Request", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Cancellation Request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ReviewCancellationRequestAsync(string requestId, bool isApproved, string statement)
        {
            try
            {
                var payload = new { RequestId = requestId, IsApproved = isApproved, Statement = statement };
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.ReviewCancellationRequest, payload);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, "Cancellation Request", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? $"Request {(isApproved ? "approved" : "rejected")} successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, "Cancellation Request", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Cancellation Request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateCancellationRequestAsync(string requestId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeactivateCancellationRequest, requestId);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Cancellation Request ID: {requestId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Cancellation request deactivated successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Cancellation Request ID: {requestId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate cancellation request.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Cancellation Request ID: {requestId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<CustomDataTable> GetCancellationRequestsDataTableAsync(CancellationRequestQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetCancellationRequestsDataTable, query);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }

                return new CustomDataTable(
                    draw: Convert.ToInt32(query.DataTableOptions.draw),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.DataTableOptions
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching cancellation requests data: {ex.Message}");
            }
        }


        public async Task<ExecutionMessages> SubmitFileActionAsync(CancellationValidatiion model)
        {
            try
            {
                string url;
                object payload;

                switch (model.Mode?.ToLower())
                {
                    case "approve":
                        url = APICallHelper.ApproveUploadedFile;
                        payload = new { ChequeClearanceId = model.Id, approvalStatement = model.Statement };
                        break;
                    case "review":
                        url = APICallHelper.ReviewUploadedFile;
                        payload = new { ChequeClearanceId = model.Id, reviewerStatement = model.Statement };
                        break;
                    case "reject":
                        url = APICallHelper.DenyUploadedFile;
                        payload = new { ChequeClearanceId = model.Id, rejectionStatement = model.Statement };
                        break;
                    case "disburse":
                        url = APICallHelper.DenyUploadedFile;
                        payload = new { ChequeClearanceId = model.Id, disburseStatement = model.Statement };
                        break;
                    default:
                        throw new ArgumentException("Invalid action mode specified.");
                }

                // CRITICAL CHANGE: We now expect a FileDetailsResponse back, not a boolean.
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<FileDetailsResponse>>(url, payload);

                // We check for a successful response that contains data.
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    // We pass the ENTIRE returned object in the 'Data' property of ExecutionMessages.
                    GetExecutionMessages(response.ApiResponseData.Data, true, "File Action", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, "Success", null, "Action completed successfully.");
                }
                else
                {
                    GetExecutionMessages(model, false, "File Action", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, "Failed", null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "File Action", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }
            return ExecutionMessage;
        }
    }
}
