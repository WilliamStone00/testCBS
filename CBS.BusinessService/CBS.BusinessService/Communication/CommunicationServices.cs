
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.Communication;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Communication
{

    public class CommunicationServices : BaseService
    {
        private readonly ApiCallerHelper _communicationApiHelper;

        public CommunicationServices()
        {
            _communicationApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CommunicationBaseUrl"].ToString());

        }





        // =====================================================================================================
        // NEW: SMS UPLOAD (Preview + DataTable + Send + Download)
        // Controller: api/v1/SmsUpload
        // =====================================================================================================

        /// <summary>
        /// Uploads an SMS Excel file (XLS/XLSX) to preview parsed rows before sending.
        /// Endpoint: POST api/v1/SmsUpload/preview
        /// Content: multipart/form-data
        /// </summary>
        public async Task<ApiResponse<ServiceResponse<SmsUploadPreviewSummaryDto>>> PreviewSmsUploadAsync(
            HttpPostedFileBase file, string branchId = null, string defaultMessageTemplate = null, string senderService = null, string title = null, string purpose = null)
        {

            branchId = GetBranchID();

            if (file == null || file.ContentLength <= 0)
                throw new ArgumentException("File is required.", nameof(file));

            if (string.IsNullOrWhiteSpace(branchId))
                throw new ArgumentException("Branch is required.", nameof(title));
            if (string.IsNullOrWhiteSpace(purpose))
                throw new ArgumentException("Title is required.", nameof(purpose));
            if (string.IsNullOrWhiteSpace(defaultMessageTemplate))
                throw new ArgumentException("Message Template is required.", nameof(defaultMessageTemplate));
            if (string.IsNullOrWhiteSpace(senderService))
                throw new ArgumentException("SenderService is required.", nameof(senderService));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            // Many ApiCallerHelpers accept: (file, url, extraFormFields)
            // If yours does not, you can create a new helper method (UploadFileWithFormFieldsAsync).
            var formFields = new System.Collections.Generic.Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(defaultMessageTemplate))
                formFields["DefaultMessageTemplate"] = defaultMessageTemplate;
            if (!string.IsNullOrWhiteSpace(branchId))
                formFields["BranchId"] = branchId;
            if (!string.IsNullOrWhiteSpace(senderService))
                formFields["SenderService"] = senderService;
            if (!string.IsNullOrWhiteSpace(title))
                formFields["Title"] = title;
            if (!string.IsNullOrWhiteSpace(purpose))
                formFields["Purpose"] = purpose;

            return await _communicationApiHelper.UploadFileWithFormFieldsAsync<ServiceResponse<SmsUploadPreviewSummaryDto>>(
                file,
                APICallHelper.SmsUpload_Preview,
                formFields);
        }


        public async Task<SmsFileUploadDetailsDto> GetSmsUploadDetaisAsync(string fileUploadId)
        {
            if (fileUploadId == null)
                throw new ArgumentException("file Upload is required.", nameof(fileUploadId));
            try
            {

              var response=  await _communicationApiHelper.GetAsync<ServiceResponse<SmsFileUploadDetailsDto>>(string.Format(APICallHelper.SmsUpload_ById, fileUploadId));
                if (response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        /// <summary>
        /// Loads SMS uploads listing using DataTables request model.
        /// Endpoint: POST api/v1/SmsUpload/datatable
        /// Content: application/json
        /// </summary>
        public async Task<CustomDataTable> GetSmsUploadDataTableAsync(GetSmsFileUploadDataTableQuery query)
        {
            try
            {
                var response = await _communicationApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.SmsUpload_DataTable,
                    query);

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                throw new Exception($"service unavailable: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Loads SMS uploads  history listing using DataTables request model.
        /// Endpoint: POST api/v1/SmsUpload/history/datatable
        /// Content: application/json
        /// </summary>
        public async Task<CustomDataTable> GetSmsFileUploadHistoryDataTableAsync(GetSmsFileUploadHistoryDataTableQuery query)
        {
            try
            {
                var response = await _communicationApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.SmsUpload_History_DataTable,
                    query);

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                throw new Exception($"service unavailable: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends all SMS rows for a previously uploaded file (by FileUploadId),
        /// and writes history per row.
        /// Endpoint: POST api/v1/SmsUpload/send
        /// Content: application/json
        /// </summary>
        public async Task<ApiResponse<ServiceResponse<SmsFileUploadSendSummaryDto>>> SendSmsUploadAsync(SendSmsFileUploadCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (string.IsNullOrWhiteSpace(command.FileUploadId))
                throw new ArgumentException("FileUploadId is required.", nameof(command.FileUploadId));


            return await _communicationApiHelper.PostAsync<ServiceResponse<SmsFileUploadSendSummaryDto>>(
                APICallHelper.SmsUpload_Send,
                command);
        }

        /// <summary>
        /// Downloads an uploaded file by FileId.
        /// Endpoint: GET api/v1/SmsUpload/download/{fileId}
        /// Returns: FileDownloadDto (usually contains base64/file bytes + name + content type)
        /// </summary>
        public async Task<ApiResponse<ServiceResponse<SmsFileDownloadDto>>> DownloadSmsUploadFileByIdAsync(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                throw new ArgumentException("FileId is required.", nameof(fileId));

            // If your ApiCallerHelper doesn't have GetAsync<T>, add it.
            return await _communicationApiHelper.GetAsync<ServiceResponse<SmsFileDownloadDto>>(
                string.Format(APICallHelper.SmsUpload_DownloadById, fileId));
        }

    }
}
