using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FileUpload;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Hangfire.Storage.Monitoring;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Accounting_V2.FilesUpload
{
    public class FileUploadService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _ExtractedDetails;
        public FileUploadService()
        {
            var baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);
            var cusbaseurl = ConfigurationManager.AppSettings["CustomerBaseUrl"];
        }

        public async Task<CustomDataTable> AccountwaitingDataTableAsync(AccountwaitingCorrespondanceQuery query)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.awaitingcorrespondancedatatable, query);

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

        public async Task<CustomDataTable> CorrespondanceDataTableAsync(CorespondanceQUERY query)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.Correspondancedatatable, query);

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

        public async Task<ApiResponse<ServiceResponse<FileUploadResponse>>> AffiliateUpload(FileUpload affiliateUpload)
        {
            try
            {
                if (affiliateUpload.file == null)
                {
                    return new ApiResponse<ServiceResponse<FileUploadResponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "❌ No file provided."
                    };
                }
                var endpoint = string.Format(APICallHelper.AffiliateUpload, Uri.EscapeDataString(affiliateUpload.affiliateId));
                var result = await _apiHelper.UploadBulkCashPaymentFileAsync<ServiceResponse<FileUploadResponse>>(affiliateUpload.file, endpoint);
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceResponse<FileUploadResponse>>
                {
                    IsSuccess = false,
                    ApiResponseData = null,
                    Message = $"❌ Error while uploading file: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<ServiceResponse<A_B_FileUploadResponse>>> BranchUpload(FileUpload BranchUpload)
        {
            try
            {
                if (BranchUpload.file == null)
                {
                    return new ApiResponse<ServiceResponse<A_B_FileUploadResponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "❌ No file provided."
                    };
                }

                var endpoint = $"{APICallHelper.BranchUpload}";
                var result = await _apiHelper.UploadBulkCashPaymentFileAsync<ServiceResponse<A_B_FileUploadResponse>>(BranchUpload.file, BranchUpload.branchId, endpoint);

                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceResponse<A_B_FileUploadResponse>>
                {
                    IsSuccess = false,
                    ApiResponseData = null,
                    Message = $"❌ Error while uploading file: {ex.Message}"
                };
            }
        }


        public async Task<List<Accountwaiting>> GetAllAsync()
        {
            try
            {
                var url = APICallHelper.GetAllawaitingcorrespondance;
                var response = await _apiHelper.GetAsync<ResponseObject<List<Accountwaiting>>>(url);
                return response?.ApiResponseData?.Data ?? new List<Accountwaiting>();
            }
            catch (Exception ex)
            {
                // Log ex
                return new List<Accountwaiting>();
            }
        }

        /// <summary>
        /// Gets the full details of a single file. Used for the read-only Details page/preview.
        /// </summary>
        public async Task<Accountwaiting> GetByIdAsync(string fileUploadId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUploadId)) return null;
                string url = APICallHelper.GetawaitingcorrespondanceById.Replace("{FileUploadId}", fileUploadId);
                var response = await _apiHelper.GetAsync<ResponseObject<Accountwaiting>>(url);
                return response?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                // Log ex
                return null;
            }
        }

        public async Task<ExecutionMessages> CreateAsync(Accountwaiting model)
        {
            try
            {

                var response = await _apiHelper.PostAsync<ServiceResponse<Accountwaiting>>(APICallHelper.createCorrespondanceRequest, model);

                // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(Accountwaiting model)
        {
            try
            {
                string Id = model.Id;
                string formattedUrl = string.Format(APICallHelper.Updateawaitingcorrespondance, Id);
                var response = await _apiHelper.PutAsync<ServiceResponse<CategoryConfig>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateAsync(string categoryId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.Deactivateawaitingcorrespondance, categoryId);
                var response = await _apiHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Category ID: {categoryId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Category deactivated successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate category.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> CorrespondenceValidationAsync(correspondanceR_A model)
        {
            try
            {

                string url;
                model.actionedByName = GetUserFullName();
                model.actionedByUserId = GetUserID();
                model.language = GetLanguage();

                url = APICallHelper.ApproveCorrespondence;
                // Expect a detailed object back from the API (adjust generic type to your response DTO)
                var response = await _apiHelper.PostAsync<ServiceResponse<CorrespondenceRequestDto>>(url, model);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    // success: attach returned details as the Data of ExecutionMessages
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Correspondence Action", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, "Success", null, "Action completed successfully.");
                }
                else
                {
                    // failure: bubble message
                    var message = response?.ApiResponseData?.Message ?? response?.Message ?? "Action failed";
                    GetExecutionMessages(model, false, "Correspondence Action", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, "Failed", null, message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Correspondence Action", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> RejectCorrespondenceAsync(correspondanceR_A model)
        {
            try
            {
                var payload = model.Id;
                var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.RejectCorrespondence, payload);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Reject Correspondence", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, "Success", null, "Correspondence rejected successfully.");
                }
                else
                {
                    var message = response?.ApiResponseData?.Message ?? response?.Message ?? "Reject operation failed";
                    GetExecutionMessages(model, false, "Reject Correspondence", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, "Failed", null, message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Reject Correspondence", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            return ExecutionMessage;
        }

        // Correspondance DETAILS 
        public async Task<CorrespondenceRequestDto> GetCorrespondanceByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetAffiliateById, encodedId);

                var response = await _apiHelper.GetAsync<ServiceResponse<CorrespondenceRequestDto>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}

