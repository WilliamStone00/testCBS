using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.ChequeClearance
{
    public class ChequeClearanceService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public ChequeClearanceService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");

            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<OptionRequest>> GetClearanceAsync()
        {
            try
            {
                // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<OptionRequest>>>(APICallHelper.GetAllChequeClearance);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<OptionRequest>();
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

        public async Task<OptionRequest> GetClearanceByIdAsync(string ClearanceId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.GetChequeClearanceById, ClearanceId);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<OptionRequest>>(formattedUrl);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }
        public async Task<ExecutionMessages> SubmitFileActionAsync(ClearanceValidation model)
        {
            try
            {
                string url;
                object payload;

                switch (model.Mode?.ToLower())
                {
                    case "approve":
                        url = APICallHelper.ApproveUploadedFile;
                        payload = new { ChequeClearanceId = model.ChequeClearanceId, approvalStatement = model.Statement };
                        break;
                    case "review":
                        url = APICallHelper.ReviewUploadedFile;
                        payload = new { ChequeClearanceId = model.ChequeClearanceId, reviewerStatement = model.Statement };
                        break;
                    case "reject":
                        url = APICallHelper.DenyUploadedFile;
                        payload = new { ChequeClearanceId = model.ChequeClearanceId, rejectionStatement = model.Statement };
                        break;
                    case "disburse":
                        url = APICallHelper.DenyUploadedFile;
                        payload = new { ChequeClearanceId = model.ChequeClearanceId, disburseStatement = model.Statement };
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
