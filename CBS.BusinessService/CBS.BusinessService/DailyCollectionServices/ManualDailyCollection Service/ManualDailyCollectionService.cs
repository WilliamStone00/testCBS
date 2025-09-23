//using BusinessServices;
//using CBS.API.Helper;
//using CBS.BusinessService.CustomerManagement;
//using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
//using CBS.FrontDesk.Data.Entity;
//using CBS.FrontDesk.Data.Entity.CustomerManagement;
//using CBS.FrontDesk.Data.Entity.DataTable;
//using CBS.FrontDesk.Data.Entity.ManualDailycollection;
//using CBS.FrontDesk.Data.Message;
//using CBS.FrontDesk.Helper;
//using DocumentFormat.OpenXml.Presentation;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.UI.WebControls;

//namespace CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service
//{
//    public class ManualDailyCollectionService : BaseService
//    {
//        private readonly ApiCallerHelper _apiHelper;
//        private readonly ApiCallerHelper _customerApiHelper;
//        private readonly string _baseApiUrl;
//        private readonly string _baseCustomerApiUrl;
//        private readonly IndividualProfileServices _individualProfileServices;

//        public ManualDailyCollectionService(IndividualProfileServices individualProfileServices)
//        {
//            _baseApiUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"]?.ToString();
//            _baseCustomerApiUrl = ConfigurationManager.AppSettings["CustomerBaseUrl"]?.ToString();
//            _apiHelper = new ApiCallerHelper(_baseApiUrl);
//            _customerApiHelper = new ApiCallerHelper(_baseCustomerApiUrl);
//            _individualProfileServices = individualProfileServices;
//        }

//        #region Upload
//        public async Task<ApiResponse<ServiceResponse<FileUploadResponse>>> UploadManualEntryFileAsync(HttpPostedFileBase file, string branch, string CollectorId)
//        {
//            try
//            {
//                if (file == null)
//                {
//                    return new ApiResponse<ServiceResponse<FileUploadResponse>>
//                    {
//                        IsSuccess = false,
//                        ApiResponseData = null,
//                        Message = "No file provided."
//                    };
//                }

//                // Correctly pass CollectorId in the API endpoint
//                var result = await _apiHelper.UploadBulkCashPaymentFileAsync<ServiceResponse<FileUploadResponse>>(
//                    file, string.Format(APICallHelper.ManualEntryUpload, branch, CollectorId)
//                );
//                return result;
//            }
//            catch (Exception ex)
//            {
//                // Log exception (ex) here
//                return new ApiResponse<ServiceResponse<FileUploadResponse>>
//                {
//                    IsSuccess = false,
//                    ApiResponseData = null,
//                    Message = $"Error while uploading file: {ex.Message}"
//                };
//            }
//        }

//        #endregion

//        #region Read
//        public async Task<FileUploadResponse> GetFileDetailsByIdAsync(string fileUploadId)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(fileUploadId)) return null;

//                var response = await _apiHelper.GetAsync<ResponseObject<FileUploadResponse>>(
//                                  APICallHelper.GetFileById.Replace("{fileId}", fileUploadId)
//                              );

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                // Log exception
//                return null;
//            }
//        }


//        public async Task<List<FileUploadResponse>> GetAllFilesAsync()
//        {
//            try
//            {
//                var response = await _apiHelper.GetAsync<ResponseObject<List<FileUploadResponse>>>(
//                    APICallHelper.GetAllFiles
//                );

//                return response?.ApiResponseData?.Data ?? new List<FileUploadResponse>();
//            }
//            catch (Exception ex)
//            {
//                // Log exception
//                return new List<FileUploadResponse>();
//            }
//        }
//        #endregion


//        #region Delete
//        public async Task<ExecutionMessages> DeleteFileByIdAsync(string fileUploadId)
//        {
//            try
//            {
//                var response = await _apiHelper.DeleteAsync<ResponseObject<object>>(
//                    APICallHelper.DeleteManualEntryFile.Replace("{fileId}", fileUploadId)
//                );



//                if (response != null && response.ApiResponseData != null && response.ApiResponseData.StatusCode == 200)
//                {
//                    GetExecutionMessages(response, true, fileUploadId, MessagesResults.Success,
//                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(),
//                        null, response.Message);

//                    return ExecutionMessage;
//                }
//                else
//                {
//                    GetExecutionMessages(fileUploadId, false, "ManualDailyCollection", MessagesResults.Failed,
//                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
//                        null, response?.Message);
//                }
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(null, false, null, MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
//            }

//            return ExecutionMessage;
//        }
//        #endregion

//        #region Daily Collectors Dropdown
//        /// <summary>
//        /// Retrieves a list of customers specifically identified as 'Daily Collectors'
//        /// and formats them for use in a dropdown list.
//        /// </summary>
//        /// <param name="branchId">Optional: A branch ID to filter the collectors by.</param>
//        /// <returns>A list of SelectListItem objects, perfect for a dropdown.</returns>
//        public async Task<IEnumerable<SelectListItem>> GetDailyCollectorsAsSelectListAsync(string branchId = null)
//        {
//            try
//            {
//                var response = await _customerApiHelper.GetAsync<ResponseObject<List<CustomerBasicInfosDto>>>(
//                    $"{APICallHelper.GetAllDailyCollectorBasicInfos}?customerType=DailyCollection"
//                );

//                // 🔹 FIX: Changed && to || (otherwise it will throw if ApiResponseData is null)
//                if (response?.ApiResponseData == null || response.ApiResponseData.Data == null)
//                {
//                    return new List<SelectListItem>();
//                }

//                var collectorList = response.ApiResponseData.Data;

//                var selectList = collectorList.Select(c => new SelectListItem
//                {
//                    Value = c.CustomerId,
//                    Text = $"{c.FullName} ({c.CustomerId})"
//                }).ToList();

//                return selectList;
//            }
//            catch (Exception ex)
//            {
//                // Log exception
//                return new List<SelectListItem>();
//            }
//        }

//        #endregion


//        // In ManualDailyCollectionService.cs

//        /// <summary>
//        /// Gets a list of uploaded files filtered by their processing status.
//        /// </summary>
//        /// <param name="status">The status to filter by (e.g., "Pending", "Approved").</param>
//        public async Task<IEnumerable<FileUploadSummary>> GetFilesByStatusAsync(string status)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(status))
//                {
//                    // Return empty list if no status is provided
//                    return Enumerable.Empty<FileUploadSummary>();
//                }

//                // Assumes your APICallHelper has a constant like: GetFilesByStatus = "api/files/status/{0}"
//                string url = string.Format(APICallHelper.GetFilesByStatus, status);

//                var response = await _apiHelper.GetAsync<ResponseObject<List<FileUploadSummary>>>(url);

//                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
//                {
//                    return response.ApiResponseData.Data;
//                }

//                return Enumerable.Empty<FileUploadSummary>();
//            }
//            catch (Exception ex)
//            {
//                // Log the exception
//                throw;
//            }
//        }

//        /// <summary>
//        /// Submits a validation request for a specific file.
//        /// </summary>
//        /// <param name="validationRequest">The DTO containing the validation details.</param>
//        public async Task<ExecutionMessages> ValidateFileAsync(FileValidationRequest validationRequest)
//        {
//            try
//            {
//                // Assumes your APICallHelper has a constant like: ValidateFile = "api/files/validate"
//                var url = APICallHelper.ValidateFile;

//                var response = await _apiHelper.PostAsync<ResponseObject<bool>>(url, validationRequest);

//                if (response != null && response.IsSuccess && response.ApiResponseData.Data)
//                {
//                    GetExecutionMessages(validationRequest, true, "File Validation", MessagesResults.Success,
//                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
//                }
//                else
//                {
//                    GetExecutionMessages(validationRequest, false, "File Validation", MessagesResults.Failed,
//                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
//                }
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(validationRequest, false, "File Validation", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
//            }
//            return ExecutionMessage;
//        }


//    }
//}

// Location: ~/BusinessService/DailyCollectionServices/ManualDailyCollection_Service/ManualDailyCollectionService.cs

using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service
{
    public class ManualDailyCollectionService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _ExtractedDetails;



        public ManualDailyCollectionService()
        {
            var baseUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);

            var ExtractedbaseUrl = ConfigurationManager.AppSettings["ExtractedDetailsBaseUrl"];
            _ExtractedDetails = new ApiCallerHelper(ExtractedbaseUrl);

            var cusbaseurl = ConfigurationManager.AppSettings["CustomerBaseUrl"];
            _customerApiHelper = new ApiCallerHelper(cusbaseurl);
        }

        #region File Upload and Initial Actions

        /// <summary>
        /// Handles the multipart form data upload to the backend API.
        /// </summary>
        //public async Task<ApiResponse<ServiceResponse<FileUploadResponse>>> UploadFileAsync(HttpPostedFileBase file, string branchId, string collectorId, string userId)
        //{
        //    var additionalFields = new Dictionary<string, string>
        //    {
        //        { "BranchId", branchId },
        //        { "CollectorId", collectorId },
        //        { "UserId", userId }
        //    };
        //    return await _apiHelper.UploadFileToApiAsync<ServiceResponse<FileUploadResponse>>(
        //        file, "File", APICallHelper.ManualEntryUpload, additionalFields
        //    );
        //}

        public async Task<ApiResponse<ServiceResponse<FileUploadResponse>>> UploadManualEntryFileAsync(HttpPostedFileBase file,string branch,string collectorId,string userId,string AccountingDate)
        {
            try
            {
                if (file == null)
                {
                    return new ApiResponse<ServiceResponse<FileUploadResponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "❌ No file provided."
                    };
                }

                ///******* when Accounting date is updated on the endpoint add it to the endpoint beign sent here dont forget man****
               
                var endpoint = $"{APICallHelper.ManualEntryUpload}?collectorId={collectorId}&branchId={branch}&userId={userId}&accountingDate={AccountingDate}";
                var result = await _apiHelper.UploadBulkCashPaymentFileAsync<ServiceResponse<FileUploadResponse>>( file, endpoint);

             
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



        /// <summary>
        /// Calls the endpoint to process/extract a recently uploaded file.
        /// </summary>
        public async Task<ExecutionMessages> ExtractFileAsync(string fileUploadId){
        try
        {
            var url = APICallHelper.ExtractUploadedFile;
            var payload = new { fileUploadId = fileUploadId };

             var response = await _apiHelper.PostAsync<ServiceResponse<FileDetailsResponse>>(url, payload);

            if (response?.IsSuccess == true && response.ApiResponseData?.Data != null)
            {
                var data = response.ApiResponseData.Data;

                // do whatever you need with 'data' (e.g. save, render, map to viewmodel)
                GetExecutionMessages(
                    data,
                    true,
                    fileUploadId,
                    MessagesResults.Success,
                    ExecutionProcessOption.DefaultSuccessdMessages,
                    "Success",
                    null,
                    "File extracted successfully."
                );
            }
            else
            {
                GetExecutionMessages(
                    null,
                    false,
                    fileUploadId,
                    MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    "Failed",
                    null,
                    response?.ApiResponseData?.Message ?? response?.Message ?? "Extraction failed."
                );
            }
        }
        catch (Exception ex)
        {
            GetExecutionMessages(null, false, "Extract File", MessagesResults.Error, ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
        }

        return ExecutionMessage;
    }


        #endregion

        #region File Reading (Get, GetAll, GetById)

        /// <summary>
        /// Gets all uploaded files. Used for the initial generic 'list' view.
        /// </summary>
        public async Task<List<FileUploadResponse>> GetAllFilesAsync()
        {
            try
            {
                var url = APICallHelper.GetAllFiles;
                var response = await _apiHelper.GetAsync<ResponseObject<List<FileUploadResponse>>>(url);
                return response?.ApiResponseData?.Data ?? new List<FileUploadResponse>();
            }
            catch (Exception ex)
            {
                // Log ex
                return new List<FileUploadResponse>();
            }
        }

        /// <summary>
        /// Gets the full details of a single file. Used for the read-only Details page/preview.
        /// </summary>
        public async Task<FileUploadResponse> GetFileDetailsByIdAsync(string fileUploadId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUploadId)) return null;
                string url = APICallHelper.GetFileById.Replace("{FileUploadId}", fileUploadId);
                var response = await _apiHelper.GetAsync<ResponseObject<FileUploadResponse>>(url);
                return response?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                // Log ex
                return null;
            }
        }
                
        /// <summary>
        /// Gets data specifically for the server-side validation DataTable.
        /// </summary>
        public async Task<CustomDataTable> GetFilesForDataTableAsync(GetFilesForDataTableQuery query)
        {
            try
            {
                var url = APICallHelper.GetFileUploadsForDataTable;
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(url, query);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new CustomDataTable(
                    draw: Convert.ToInt32(query.Options.draw),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.Options
                );
            }
            catch (Exception ex)
            {
                return new CustomDataTable(
                    draw: Convert.ToInt32(query.Options.draw),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.Options
                );
            }
        }

        public async Task<CustomDataTable> GetextractedFilesForDataTableAsync(GetFilesForDataTableQuery query)
        {
            try
            {
                var url = APICallHelper.GetextracyedFileDetailsEndpoint;
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(url, query);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new CustomDataTable(
                    draw: Convert.ToInt32(query.Options.draw),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.Options
                );
            }
            catch (Exception ex)
            {
                return new CustomDataTable(
                    draw: Convert.ToInt32(query.Options.draw),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.Options
                );
            }
        }



        public async Task<FileDetailsResponse> GetExtractedDetailsAsync(string fileUploadId)
        {
            if (string.IsNullOrWhiteSpace(fileUploadId))
                return null;

            try
            {
                var endpoint = string.Format(APICallHelper.GetManualEntryCollectorById, fileUploadId);

                // We correctly expect a List<TransactionDetail> from the API.
                var response = await _apiHelper.GetAsync<ResponseObject<FileDetailsResponse>>(endpoint);

                // Check for a successful response and that the list contains items.
                // Because of our [JsonProperty] fix, the deserialization will now work correctly.
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    var detailsList = response.ApiResponseData.Data;
                    return detailsList;
                }

                // If the API call fails or the list is empty, return null.
                return null;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging.
                return null;
            }
        }

        #endregion

        #region File Validation Actions (Approve, Review, Reject)

        /// <summary>
        /// Submits a validation, review, or rejection action for a specific file.
        /// </summary>
        public async Task<ExecutionMessages> SubmitFileActionAsync(ValidationDto model)
        {
            try
            {
                string url;
                object payload;

                switch (model.Mode?.ToLower())
                {
                    case "approve":
                        url = APICallHelper.ApproveUploadedFile;
                        payload = new { manualEntryDailyCollectorId = model.ManualEntryDailyCollectorId, approvalStatement = model.Statement };
                        break;
                    case "review":
                        url = APICallHelper.ReviewUploadedFile;
                        payload = new { manualEntryDailyCollectorId = model.ManualEntryDailyCollectorId, reviewerStatement = model.Statement };
                        break;
                    case "reject":
                        url = APICallHelper.DenyUploadedFile;
                        payload = new { manualEntryDailyCollectorId = model.ManualEntryDailyCollectorId, rejectionStatement = model.Statement };
                        break;
                    default:
                        throw new ArgumentException("Invalid action mode specified.");
                }

                // CRITICAL CHANGE: We now expect a FileDetailsResponse back, not a boolean.
                var response = await _apiHelper.PostAsync<ServiceResponse<FileDetailsResponse>>(url, payload);

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


        #endregion

        #region File Deletion

        /// <summary>
        /// Deletes an uploaded file by its ID.
        /// </summary>
        //public async Task<ExecutionMessages> DeleteFileByIdAsync(string fileUploadId)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(fileUploadId))
        //        {
        //            GetExecutionMessages(null, false, "Delete File", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, "Failed", null, "File ID cannot be null.");
        //            return ExecutionMessage;
        //        }

        //        string url = APICallHelper.DeleteManualEntryFile.Replace("{fileId}", fileUploadId);
        //        var response = await _apiHelper.DeleteAsync<ServiceResponse<bool>>(url);

        //        if (response.IsSuccess && response.ApiResponseData.Data)
        //        {
        //            GetExecutionMessages(null, true, fileUploadId, MessagesResults.Success, ExecutionProcessOption.DeleteObject, "Success", null, "File deleted successfully.");
        //        }
        //        else
        //        {
        //            GetExecutionMessages(null, false, fileUploadId, MessagesResults.Failed, ExecutionProcessOption.DeleteObject, "Failed", null, response.ApiResponseData?.Message ?? response.Message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        GetExecutionMessages(null, false, "Delete File", MessagesResults.Error, ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
        //    }
        //    return ExecutionMessage;
        //}

        public async Task<ExecutionMessages> DeleteFileByIdAsync(string fileUploadId)
        {
            try
            {
                // 1) Validate input early
                if (string.IsNullOrWhiteSpace(fileUploadId))
                {
                    GetExecutionMessages(null, false, "Delete File", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, "Failed", null, "File ID cannot be null.");
                    return ExecutionMessage ?? new ExecutionMessages { Result = false, MessageString = "File ID cannot be null." };
                }

                // 2) Build API URL
                string url = APICallHelper.DeleteManualEntryFile.Replace("{fileId}", fileUploadId);

                // 3) Call API (this may return null or ApiResponseData may be null)
                var response = await _apiHelper.DeleteAsync<ServiceResponse<bool>>(url);

                // 4) Debug trace to help find shape of response (remove or replace with logger)
                System.Diagnostics.Trace.WriteLine($"DeleteFileByIdAsync: url={url} responseIsNull={(response == null)} ApiResponseDataIsNull={(response?.ApiResponseData == null)} message='{response?.Message}'");

                // 5) Defensive checks
                if (response == null)
                {
                    GetExecutionMessages(null, false, fileUploadId, MessagesResults.Failed, ExecutionProcessOption.DeleteObject, "Failed", null, "No response from API.");
                    return ExecutionMessage ?? new ExecutionMessages { Result = false, MessageString = "No response from API." };
                }

                // 6) Determine success carefully:
                var apiDataPresent = response.ApiResponseData != null;
                var apiDataIsTrue = apiDataPresent && (response.ApiResponseData.Data == true);

                // Interpret success:
                // - Prefer explicit ApiResponseData.Data == true
                // - If ApiResponseData missing but response.IsSuccess == true, treat as a success to preserve compatibility with older API shapes
                if (response.IsSuccess && (apiDataIsTrue || (!apiDataPresent && response.IsSuccess)))
                {
                    GetExecutionMessages(null, true, fileUploadId, MessagesResults.Success, ExecutionProcessOption.DeleteObject, "Success", null, "File deleted successfully.");
                }
                else
                {
                    var failMsg = response.ApiResponseData?.Message ?? response.Message ?? "API reported failure.";
                    GetExecutionMessages(null, false, fileUploadId, MessagesResults.Failed, ExecutionProcessOption.DeleteObject, "Failed", null, failMsg);
                }
            }
            catch (Exception ex)
            {
                // 7) Capture exception inside ExecutionMessage (uses your helper)
                GetExecutionMessages(null, false, "Delete File", MessagesResults.Error, ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            // 8) Always return a non-null ExecutionMessage so callers (controller) can read .Result/.MessageString safely
            return ExecutionMessage ?? new ExecutionMessages { Result = false, MessageString = "An unknown error occurred in DeleteFileByIdAsync." };
        }

        #endregion

        #region Helpers for Dropdowns

        public async Task<IEnumerable<SelectListItem>> GetCollectorsAsSelectListAsync(string branchId = null)
        {
            if (branchId==null)
            {
                branchId = "n/a";
            }
            var response = await _customerApiHelper.GetAsync<ResponseObject<List<CollectorDto>>>(string.Format(APICallHelper.GetDaillycollectors, branchId));

            if (response.IsSuccess && response.ApiResponseData?.Data != null)
            {
                return response.ApiResponseData.Data.Where(x=>x.BranchId==branchId).Select(c => new SelectListItem
                {
                    Value = $"{c.CustomerId}|{c.UserId}",
                    Text = $"{c.CustomerId}|{c.FullName}",
                }).ToList();
            }

            return new List<SelectListItem>();
        }


        // In ManualDailyCollectionService.cs

        #region File Validation Actions (Approve, Review, Reject)

        // ... (your existing SubmitFileActionAsync method) ...


        /// <summary>
        /// Rejects a file by calling a dedicated backend API endpoint.
        /// This is a direct action that does not require a statement.
        /// </summary>
        /// <param name="fileUploadId">The ID of the file to reject.</param>
        public async Task<ExecutionMessages> RejectFileAsync(string fileUploadId)
        {
            try
            {
                // Step 1: Validate the input
                if (string.IsNullOrWhiteSpace(fileUploadId))
                {
                    GetExecutionMessages(null, false, "Reject File", MessagesResults.Failed,
                        ExecutionProcessOption.ValidationError, SystemMessageStatus.Failed.ToString(), null, "File ID cannot be null or empty.");
                    return ExecutionMessage;
                }

                // Step 2: Define the URL and the payload for the API call
                // You will need to add this constant to your APICallHelper.cs
                // e.g., public static string RejectUploadedFile = "/api/v1/ManualEntryCollector/RejectUploadedFile";
                var url = APICallHelper.DenyUploadedFile;

                // The backend API might expect the ID in the body of the POST request
                var payload = new { FileUploadId = fileUploadId };

                // Step 3: Make the API call
                var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(url, payload);

                // Step 4: Handle the response from the API
                if (response.IsSuccess && response.ApiResponseData.Data)
                {
                    // Handle success
                    GetExecutionMessages(null, true, fileUploadId, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "File rejected successfully.");
                }
                else
                {
                    // Handle failure
                    GetExecutionMessages(null, false, fileUploadId, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to reject the file.");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the process
                GetExecutionMessages(null, false, "Reject File", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        #endregion

        #endregion
        ////////// ***************************** get extracted details data trable ************************

        //public async Task<CustomDataTable> GetManualEntryDetailsForDataTableAsync(GetManualEntryDailyCollectionDetailDataTableQuery query)
        //{
        //    try
        //    {
        //        var url = APICallHelper.GetManualEntryCollectorDetailsDataTable;
        //        var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(url, query);

        //        if (response.IsSuccess && response.ApiResponseData != null)
        //        {
        //            return response.ApiResponseData.Data;
        //        }

        //        return new CustomDataTable(
        //            draw: Convert.ToInt32(query.Options.draw),
        //            recordsTotal: 0,
        //            recordsFiltered: 0,
        //            data: new List<object>(),
        //            dataTableOptions: query.Options
        //        );
        //    }
        //    catch (Exception)
        //    {
        //        return new CustomDataTable(
        //            draw: Convert.ToInt32(query.Options.draw),
        //            recordsTotal: 0,
        //            recordsFiltered: 0,
        //            data: new List<object>(),
        //            dataTableOptions: query.Options
        //        );
        //    }
        //}

        /// <summary>
        /// Gets a server-side processed and filtered list of ALL transaction details.
        /// </summary>
        /// <param name="query">The query object containing all filters and DataTable options.</param>
        /// <returns>A CustomDataTable object ready for the controller to use.</returns>
        public async Task<CustomDataTable> GetManualEntryDetailsForDataTableAsync(GetManualEntryDailyCollectionDetailDataTableQuery query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(APICallHelper.GetManualEntryCollectorDetailsDataTable))
                    throw new InvalidOperationException("API endpoint for GetManualEntryCollectorDetailsDataTable is not configured.");

                var url = APICallHelper.GetManualEntryCollectorDetailsDataTable;

                // Post query to API; API should return ResponseObject<CustomDataTable>
                var response = await _ExtractedDetails.PostAsync<ResponseObject<CustomDataTable>>(url, query);

                if (response != null && response.IsSuccess && response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data ?? new CustomDataTable(
                        draw: Convert.ToInt32(query.Options?.draw ?? "1"),
                        recordsTotal: 0,
                        recordsFiltered: 0,
                        data: new List<object>(),
                        dataTableOptions: query.Options);
                }

                // If API returned non-success, return an empty structure with draw preserved
                return new CustomDataTable(
                    draw: Convert.ToInt32(query.Options?.draw ?? "1"),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.Options);
            }
            catch (Exception ex)
            {
                // Log the exception: ex.Message / stacktrace
                System.Diagnostics.Trace.TraceError("GetManualEntryDetailsForDataTableAsync error: " + ex);

                // Return an empty table, preserving draw if present
                return new CustomDataTable(
                    draw: Convert.ToInt32(query?.Options?.draw ?? "1"),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query?.Options);
            }
        }
    }
}