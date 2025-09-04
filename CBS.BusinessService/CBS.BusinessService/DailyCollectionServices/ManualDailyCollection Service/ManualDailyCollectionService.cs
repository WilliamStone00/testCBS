using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service
{
    public class ManualDailyCollectionService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly string _baseApiUrl;
        private readonly string _baseCustomerApiUrl;
        private readonly IndividualProfileServices _individualProfileServices;

        public ManualDailyCollectionService(IndividualProfileServices individualProfileServices)
        {
            _baseApiUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"]?.ToString();
            _baseCustomerApiUrl = ConfigurationManager.AppSettings["CustomerBaseUrl"]?.ToString();
            _apiHelper = new ApiCallerHelper(_baseApiUrl);
            _customerApiHelper = new ApiCallerHelper(_baseCustomerApiUrl);
            _individualProfileServices = individualProfileServices;
        }

        #region Upload
        public async Task<ApiResponse<ServiceResponse<FileUploadResponse>>> UploadManualEntryFileAsync(HttpPostedFileBase file, string branch, string CollectorId)
        {
            try
            {
                if (file == null)
                {
                    return new ApiResponse<ServiceResponse<FileUploadResponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "No file provided."
                    };
                }

                // Correctly pass CollectorId in the API endpoint
                var result = await _apiHelper.UploadBulkCashPaymentFileAsync<ServiceResponse<FileUploadResponse>>(
                    file, string.Format(APICallHelper.ManualEntryUpload, branch, CollectorId)
                );
                return result;
            }
            catch (Exception ex)
            {
                // Log exception (ex) here
                return new ApiResponse<ServiceResponse<FileUploadResponse>>
                {
                    IsSuccess = false,
                    ApiResponseData = null,
                    Message = $"Error while uploading file: {ex.Message}"
                };
            }
        }

        #endregion

        #region Read
        public async Task<FileUploadResponse> GetFileDetailsByIdAsync(string fileUploadId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUploadId)) return null;

                var response = await _apiHelper.GetAsync<ResponseObject<FileUploadResponse>>(
                                  APICallHelper.GetFileById.Replace("{fileId}", fileUploadId)
                              );

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log exception
                return null;
            }
        }


        public async Task<List<FileUploadResponse>> GetAllFilesAsync()
        {
            try
            {
                var response = await _apiHelper.GetAsync<ResponseObject<List<FileUploadResponse>>>(
                    APICallHelper.GetAllFiles
                );

                return response?.ApiResponseData?.Data ?? new List<FileUploadResponse>();
            }
            catch (Exception ex)
            {
                // Log exception
                return new List<FileUploadResponse>();
            }
        }
        #endregion


        #region Delete
        public async Task<ExecutionMessages> DeleteFileByIdAsync(string fileUploadId)
        {
            try
            {
                var response = await _apiHelper.DeleteAsync<ResponseObject<object>>(
                    APICallHelper.DeleteManualEntryFile.Replace("{fileId}", fileUploadId)
                );



                if (response != null && response.ApiResponseData != null && response.ApiResponseData.StatusCode == 200)
                {
                    GetExecutionMessages(response, true, fileUploadId, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(),
                        null, response.Message);

                    return ExecutionMessage;
                }
                else
                {
                    GetExecutionMessages(fileUploadId, false, "ManualDailyCollection", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                        null, response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
            }

            return ExecutionMessage;
        }
        #endregion

        #region Daily Collectors Dropdown
        /// <summary>
        /// Retrieves a list of customers specifically identified as 'Daily Collectors'
        /// and formats them for use in a dropdown list.
        /// </summary>
        /// <param name="branchId">Optional: A branch ID to filter the collectors by.</param>
        /// <returns>A list of SelectListItem objects, perfect for a dropdown.</returns>
        public async Task<IEnumerable<SelectListItem>> GetDailyCollectorsAsSelectListAsync(string branchId = null)
        {
            try
            {
                var response = await _customerApiHelper.GetAsync<ResponseObject<List<CustomerBasicInfosDto>>>(
                    $"{APICallHelper.GetAllDailyCollectorBasicInfos}?customerType=DailyCollection"
                );

                // 🔹 FIX: Changed && to || (otherwise it will throw if ApiResponseData is null)
                if (response?.ApiResponseData == null || response.ApiResponseData.Data == null)
                {
                    return new List<SelectListItem>();
                }

                var collectorList = response.ApiResponseData.Data;

                var selectList = collectorList.Select(c => new SelectListItem
                {
                    Value = c.CustomerId,
                    Text = $"{c.FullName} ({c.CustomerId})"
                }).ToList();

                return selectList;
            }
            catch (Exception ex)
            {
                // Log exception
                return new List<SelectListItem>();
            }
        }
        #endregion


        // In ManualDailyCollectionService.cs

        /// <summary>
        /// Gets a list of uploaded files filtered by their processing status.
        /// </summary>
        /// <param name="status">The status to filter by (e.g., "Pending", "Approved").</param>
        public async Task<IEnumerable<FileUploadSummary>> GetFilesByStatusAsync(string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    // Return empty list if no status is provided
                    return Enumerable.Empty<FileUploadSummary>();
                }

                // Assumes your APICallHelper has a constant like: GetFilesByStatus = "api/files/status/{0}"
                string url = string.Format(APICallHelper.GetFilesByStatus, status);

                var response = await _apiHelper.GetAsync<ResponseObject<List<FileUploadSummary>>>(url);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }

                return Enumerable.Empty<FileUploadSummary>();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw;
            }
        }

        /// <summary>
        /// Submits a validation request for a specific file.
        /// </summary>
        /// <param name="validationRequest">The DTO containing the validation details.</param>
        public async Task<ExecutionMessages> ValidateFileAsync(FileValidationRequest validationRequest)
        {
            try
            {
                // Assumes your APICallHelper has a constant like: ValidateFile = "api/files/validate"
                var url = APICallHelper.ValidateFile;

                var response = await _apiHelper.PostAsync<ResponseObject<bool>>(url, validationRequest);

                if (response != null && response.IsSuccess && response.ApiResponseData.Data)
                {
                    GetExecutionMessages(validationRequest, true, "File Validation", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(validationRequest, false, "File Validation", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(validationRequest, false, "File Validation", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }


    }
}
