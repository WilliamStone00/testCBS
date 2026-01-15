
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
            HttpPostedFileBase file,
            string defaultMessageTemplate = null)
        {
            if (file == null || file.ContentLength <= 0)
                throw new ArgumentException("File is required.", nameof(file));

            // Many ApiCallerHelpers accept: (file, url, extraFormFields)
            // If yours does not, you can create a new helper method (UploadFileWithFormFieldsAsync).
            var formFields = new System.Collections.Generic.Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(defaultMessageTemplate))
                formFields["DefaultMessageTemplate"] = defaultMessageTemplate;

            return await _communicationApiHelper.UploadFileWithFormFieldsAsync<ServiceResponse<SmsUploadPreviewSummaryDto>>(
                file,
                APICallHelper.SmsUpload_Preview,
                formFields);
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
            if (string.IsNullOrWhiteSpace(command.SenderService))
                throw new ArgumentException("SenderService is required.", nameof(command.SenderService));
            if (string.IsNullOrWhiteSpace(command.Title))
                throw new ArgumentException("Title is required.", nameof(command.Title));

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
