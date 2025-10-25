using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
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

        public async Task<CustomDataTable> GetchequebokDataTableAsync(ChequeBookQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetChequeBooksDataTable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }

        //public async Task<CustomDataTable> GetChequeBooksDataTableAsync(ChequeBookQuery query)
        //{
        //    try
        //    {
        //        var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
        //            APICallHelper.GetChequeBooksDataTable, query);

        //        // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
        //        if (!response.IsSuccess)
        //        {
        //            throw new Exception($"API call failed: {response.Message}");
        //        }

        //        if (response.ApiResponseData == null)
        //        {
        //            throw new Exception("API returned null data");
        //        }

        //        return response.ApiResponseData.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the original exception
        //        System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

        //        // Re-throw to trigger fallback
        //        throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
        //    }
        //}
        public async Task<CheckbookDetail> GetChequebookDetail(string KEY)
        {
            try
            {

                var response = await _apiCallerHelper.GetAsync<ResponseObject<CheckbookDetail>>(
                   string.Format(APICallHelper.ChequebookDetail, KEY));
                return response.IsSuccess ? response.ApiResponseData.Data : null;
                
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }

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


        public async Task<CustomDataTable> GetClearanceDataTableAsync(ClearanceQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetClearanceDataTable, query);

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clearance API Error: {ex.Message}");
                throw new Exception($"Clearance service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ExecutionMessages> CreateAsync(OptionRequest model)
        {
            try
            {

                var response = await _apiCallerHelper.PostAsync<ServiceResponse<OptionRequest>>(APICallHelper.CreateFeeConfig, model);
                if (response != null && (response.IsSuccess))
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model?.ChequeClearanceId?? "FeeConfig", MessagesResults.Success, ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model?.ChequeClearanceId ?? "FeeConfig", MessagesResults.Failed, ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model?.ChequeClearanceId ?? "FeeConfig", MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(OptionRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.ChequeClearanceId))
                {
                    GetExecutionMessages(model, false, model?.ChequeClearanceId , MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or Id.");
                    return ExecutionMessage;
                }

                string url = string.Format(APICallHelper.UpdateFeeConfig, model.ChequeClearanceId);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<OptionRequest>>(url, model);
                if (response != null && (response.IsSuccess))
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model?.ChequeClearanceId, MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model?.ChequeClearanceId, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model?.ChequeClearanceId, MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }



    }
}
