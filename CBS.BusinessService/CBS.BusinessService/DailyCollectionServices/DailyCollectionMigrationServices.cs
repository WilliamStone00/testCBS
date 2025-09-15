using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using ClosedXML.Excel;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CBS.BusinessService.DailyCollectionServices
{
    public class DailyCollectionMigrationServices : BaseService
    {
        private readonly ApiCallerHelper _dailySavingApiHelper;

        public DailyCollectionMigrationServices()
        {
            _dailySavingApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["DailyCollectionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _dailySavingApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_DailySavingMigrationFile, id));
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
        public async Task<IEnumerable<DailySavingMigrationFileUploadDto>> GetFileUploads()
        {
            try
            {

                var couApiResponse = await _dailySavingApiHelper.GetAsync<ResponseObject<List<DailySavingMigrationFileUploadDto>>>(string.Format(APICallHelper.GetAllDailyOperationFileUploadsByProcessingStatus, "ProcessingStatus"));
                if (couApiResponse.IsSuccess)
                {
                    var data = couApiResponse.ApiResponseData.Data;
                    if (!IsHeadOffice())
                    {
                        return data.Where(x => x.BranchId == GetBranchID());
                    }
                    return data;


                }
                return new List<DailySavingMigrationFileUploadDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public DashboardViewModel GetDashboardSummary(List<SalaryExtractDto> salaryExtracts)
        {
            // Retrieve salary extracts from the service or database

            // Calculate KPIs
            var totalMembers = salaryExtracts.Count(); // Total number of members
            var totalNetSalary = salaryExtracts.Sum(s => s.NetSalary);
            var totalLoanRepayment = salaryExtracts.Sum(s => s.TotalLoanRepayment);
            var totalSavings = salaryExtracts.Sum(s => s.Saving);
            var totalCharges = salaryExtracts.Sum(s => s.Charges);
            var totalSalary = salaryExtracts.Sum(s => s.Salary);
            var totalRemainingSalary = salaryExtracts.Sum(s => s.RemainingSalary);
            var totalPreferenceShares = salaryExtracts.Sum(s => s.PreferenceShares);
            var totalDeposits = salaryExtracts.Sum(s => s.Deposit);
            var totalShares = salaryExtracts.Sum(s => s.Shares);
            var totalLoanInterest = salaryExtracts.Sum(s => s.LoanInterest);
            var totalStandingOrderAmount = salaryExtracts.Sum(s => s.StandingOrderAmount); // New KPI for Standing Order Amount

            // Map the summary data to a view model
            var model = new DashboardViewModel
            {
                TotalMembers = totalMembers,
                TotalNetSalary = totalNetSalary,
                TotalLoanRepayment = totalLoanRepayment,
                TotalSavings = totalSavings,
                TotalCharges = totalCharges,
                TotalSalary = totalSalary,
                TotalRemainingSalary = totalRemainingSalary,
                TotalPreferenceShares = totalPreferenceShares,
                TotalDeposits = totalDeposits,
                TotalShares = totalShares,
                TotalLoanInterest = totalLoanInterest,
                TotalStandingOrderAmount = totalStandingOrderAmount // Pass the Standing Order Amount to the view
            };

            return model;
        }


        public async Task<DailySavingMigrationFileUploadDto> GetFileUpload(string fileId)
        {
            try
            {
                var couApiResponse = await _dailySavingApiHelper.GetAsync<ResponseObject<DailySavingMigrationFileUploadDto>>(string.Format(APICallHelper.GetSalaryFileUploadByFileId, fileId));
                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return new DailySavingMigrationFileUploadDto { };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ExecutionMessages> UploadFile(UploadDailyCollectorData model)
        {
            try
            {
                // Validate input model
                if (model == null)
                {
                    return GetExecutionMessages(null, false, "Model", MessagesResults.Failed,
                        ExecutionProcessOption.ValidationError, SystemMessageStatus.Failed.ToString(), null,
                        "Invalid request data. Please try again.");
                }

                // Validate input file
                if (model.FormFile == null)
                {
                    return GetExecutionMessages(model, false, "File", MessagesResults.Failed,
                        ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
                        "No file was selected. Please upload a valid Excel file.");
                }

                // Validate file extension
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(model.FormFile.FileName)?.ToLowerInvariant();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return GetExecutionMessages(model, false, "File", MessagesResults.Failed,
                        ExecutionProcessOption.InvalidFileType, SystemMessageStatus.Failed.ToString(), null,
                        "Invalid file type. Please upload a valid Excel file (.xlsx or .xls).");
                }

                // Validate file size (e.g., max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (model.FormFile.ContentLength > maxFileSize)
                {
                    return GetExecutionMessages(model, false, "File", MessagesResults.Failed,
                        ExecutionProcessOption.FileSizeExceeded, SystemMessageStatus.Failed.ToString(), null,
                        "File size exceeds the maximum limit of 10MB. Please upload a smaller file.");
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(model.BranchId))
                {
                    return GetExecutionMessages(model, false, "BranchId", MessagesResults.Failed,
                        ExecutionProcessOption.ValidationError, SystemMessageStatus.Failed.ToString(), null,
                        "Branch ID is required. Please provide a valid Branch ID.");
                }

                if (string.IsNullOrWhiteSpace(model.CollectorId))
                {
                    return GetExecutionMessages(model, false, "CollectorId", MessagesResults.Failed,
                        ExecutionProcessOption.ValidationError, SystemMessageStatus.Failed.ToString(), null,
                        "Collector ID is required. Please provide a valid Collector ID.");
                }

                if (string.IsNullOrWhiteSpace(model.AccountId))
                {
                    return GetExecutionMessages(model, false, "AccountId", MessagesResults.Failed,
                        ExecutionProcessOption.ValidationError, SystemMessageStatus.Failed.ToString(), null,
                        "Account ID is required. Please provide a valid Collector ID.");
                }
                // Prepare additional parameters
                var additionalParams = new Dictionary<string, string>{
                 { "FormFile", model.FormFile.FileName }  };
                var urlString = string.Format(APICallHelper.DailySavingMigrationFileExecution, model.BranchId, model.CollectorId, model.CollectorName, model.AccountId);

                var response = await _dailySavingApiHelper. UploadFileToApiAsync< ServiceResponseDailySaverUploadResult>(model.FormFile, "formFile", urlString, additionalParams);




                // Process API response
                if (response == null)
                {
                    return GetExecutionMessages(model, false, "Response", MessagesResults.Failed,
                        ExecutionProcessOption.ApiResponseNull, SystemMessageStatus.Failed.ToString(), null,
                        "No response received from server. Please try again later or contact support.");
                }

                if ( response.Data != null)
                {
                    // Check the actual response data
                    if (response.Data != null)
                    {
                        return GetExecutionMessages(response, true, "Upload", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                            "File uploaded successfully. Your data is being processed.");
                    }
                    else
                    {
                        return GetExecutionMessages(model, false, "Processing", MessagesResults.Failed,
                            ExecutionProcessOption.ProcessingFailed, SystemMessageStatus.Failed.ToString(), null,
                            $"File upload processing failed. {response.Message ?? "Please verify your data and try again."}");
                    }
                }
                else
                {
                    // Handle failed response with user-friendly message
                    var errorMessage = !string.IsNullOrWhiteSpace(response.Message)
                        ? response.Message
                        : "File upload failed. Please try again later or contact support.";

                    return GetExecutionMessages(model, false, "Processing", MessagesResults.Failed,
                        ExecutionProcessOption.ProcessingFailed, SystemMessageStatus.Failed.ToString(), null,
                        $"File upload processing failed. {response.Message ?? "Please verify your data and try again."}");
                }
                }
            catch (ArgumentException argEx)
            {
                // Handle argument-specific exceptions
                return GetExecutionMessages(model, false, "Arguments", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), argEx,
                    "Invalid input provided. Please check your data and try again.");
            }
            catch (HttpRequestException httpEx)
            {
                // Handle HTTP-specific exceptions
                return GetExecutionMessages(model, false, "Network", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), httpEx,
                    "Network error occurred. Please check your connection and try again.");
            }
            catch (TaskCanceledException tcEx)
            {
                // Handle timeout exceptions
                return GetExecutionMessages(model, false, "Timeout", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), tcEx,
                    "Request timed out. Please try again later.");
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                return GetExecutionMessages(model, false, "General", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex,
                    "An unexpected error occurred while uploading the file. Please try again later or contact support.");
            }
        }
        public async Task<ExecutionMessages> PostAgentGLForInitialization(DailySaverUploadTempResult modelData)
        {
            var model = modelData.ToPostingCommand();
            var apiResponse = await _dailySavingApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.PostDailyCollectorGLForInitialization, model);
            if (apiResponse.IsSuccess)
            {
                GetExecutionMessages(apiResponse.ApiResponseData, true, $"Reconciliation was successfully done", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, apiResponse.Message);
                return ExecutionMessage;
            }
            else
            {
                GetExecutionMessages(apiResponse.ApiResponseData, true, $" Reconciliation was successfully done", MessagesResults.Success,
                   ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, apiResponse.Message);
                return ExecutionMessage;
            }
        }
        public async Task<ExecutionMessages> Create(DailySaverUpload model)
        {
            try
            {
                //model.bankId = GetBankID();
                // Make an API call to create an individual profile/api/v1/DailySaver/DailyCustomer/AddDailySaverMinimumCommand 
                var urlString = string.Format(APICallHelper.DailySavingMigrationFileExecution);
                var response = await _dailySavingApiHelper.PostAsync<ResponseObject<DailySaverUploadTempResult>>(urlString, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response.ApiResponseData, true, $" Successfully done", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $" Successfully done", MessagesResults.Failed,
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
 
        public async Task  UploadFileVoid(UploadDailyCollectorData model)
        {
              
                // Prepare additional parameters
                var additionalParams = new Dictionary<string, string>{
                 { "formFile", model.FormFile.FileName }  };
                var urlString = string.Format(APICallHelper.DailySavingMigrationFileExecution, model.BranchId, model.CollectorId, model.CollectorName, model.AccountId);
                var response = await _dailySavingApiHelper.UploadFileToApiAsync<DailySaverUploadResult>(model.FormFile, "formFile", urlString, additionalParams);
                if (response.Data != null)
                {
                    // Check the actual response data
                    if (response.Data != null)
                    {
                          GetExecutionMessages(response, true, "Upload", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                            "File uploaded successfully. Your data is being processed.");
                    }
                    else
                    {
                          GetExecutionMessages(model, false, "Processing", MessagesResults.Failed,
                            ExecutionProcessOption.ProcessingFailed, SystemMessageStatus.Failed.ToString(), null,
                            $"File upload processing failed. {response.Message ?? "Please verify your data and try again."}");
                    }
                }
                else
                {
                    // Handle failed response with user-friendly message
                    var errorMessage = !string.IsNullOrWhiteSpace(response.Message)
                        ? response.Message
                        : "File upload failed. Please try again later or contact support.";

                     GetExecutionMessages(model, false, "Upload", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                        errorMessage);
                }
     
        }

        public async Task<List<DailySaverRequest>> ReadExcelFileAsync(HttpPostedFileBase excelFile, string branchName, string BranchCode, string username, string accountId, string branchId, string collectorId)
        {
            var dailySavers = new List<DailySaverRequest>();

            try
            {
                using (var stream = excelFile.InputStream)
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        throw new InvalidOperationException("No worksheet found in the Excel file.");
                    }

                    var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                    if (lastRow < 2)
                    {
                        throw new InvalidOperationException("Excel file must contain at least a header row and one data row.");
                    }

                    // Skip header row, start from row 2
                    for (int row = 2; row <= lastRow; row++)
                    {
                        var currentRow = worksheet.Row(row);

                        // Skip empty rows
                        if (currentRow.IsEmpty()) continue;
                
                        var record = new DailySaverRequest
                        {
                            AccountNumber = GetCellValueAsString(currentRow.Cell(1)),
                            FirstName = GetCellValueAsString(currentRow.Cell(2)),
                            Username = username,
                            BranchCode = BranchCode,
                            BranchId = branchId,//GetCellValueAsString(currentRow.Cell(4)),
                            DailySaverId = PrepareDailySaverIDFormat(GetCellValueAsString(currentRow.Cell(1)), BranchCode),
                            BranchName = branchName,
                            IsNewCustomer = false, // Default value, adjust as needed
                            BankCode = "012",
                            AccountId = accountId,
                            Amount = GetCellValueAsDecimal(currentRow.Cell(5)),
                            HasBeenProcessed = false,
                            Id = branchId + "@" + collectorId + "@" + PrepareDailySaverIDFormat(GetCellValueAsString(currentRow.Cell(1)), BranchCode),
                            CreatedBy = username,
                            ModifiedBy = "NOT-SET",
                            CollectorId =collectorId
                            
                        };
                        dailySavers.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
               
                throw new InvalidOperationException($"Error processing Excel file: {ex.Message}", ex);
            }

            return dailySavers;
        }

        public string PrepareDailySaverIDFormat(string id, string branchCode)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID must not be null or empty.");

            if (string.IsNullOrWhiteSpace(branchCode) || branchCode.Length != 3)
                throw new ArgumentException("Branch code must be exactly 3 characters long.  " + branchCode);

            // Get last 5 characters of the ID or pad with '0' to the left if shorter
            string formattedIdPart = id.Length > 5
                ? id.Substring(id.Length - 5)
                : id.PadLeft(5, '0');

            // Combine branchCode + "DS" + 5-character ID
            return branchCode + "DS" + formattedIdPart;
        }
        public string GetCellValueAsString(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return string.Empty;

            return cell.GetString()?.Trim() ?? string.Empty;
        }

        // Basic version - returns 0 for invalid values
        public decimal GetCellValueAsDecimal(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return 0m;

            // Try to get as double first (Excel's native numeric type)
            if (cell.TryGetValue(out double doubleValue))
            {
                return Convert.ToDecimal(doubleValue);
            }

            // If not a number, try to parse the string representation
            var stringValue = cell.GetString()?.Trim();
            if (string.IsNullOrEmpty(stringValue))
                return 0m;

            // Try parsing as decimal with culture-invariant format
            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            // Try parsing with current culture (handles localized number formats)
            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }

            // If all parsing attempts fail, return 0
            return 0m;
        }
        public async Task<ExecutionMessages> UploadFile(UploadDailyCollectorOperationData model)
        {
            try
            {
                // Validate input model
                if (model == null)
                {
                    return GetExecutionMessages(null, false, "Model", MessagesResults.Failed,
                        ExecutionProcessOption.ValidationError, SystemMessageStatus.Failed.ToString(), null,
                        "Invalid request data. Please try again.");
                }

                // Validate input file
                if (model.ExcelFile == null)
                {
                    return GetExecutionMessages(model, false, "File", MessagesResults.Failed,
                        ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
                        "No file was selected. Please upload a valid Excel file.");
                }

                // Validate file extension
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(model.ExcelFile.FileName)?.ToLowerInvariant();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return GetExecutionMessages(model, false, "File", MessagesResults.Failed,
                        ExecutionProcessOption.InvalidFileType, SystemMessageStatus.Failed.ToString(), null,
                        "Invalid file type. Please upload a valid Excel file (.xlsx or .xls).");
                }

                // Validate file size (e.g., max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (model.ExcelFile.ContentLength > maxFileSize)
                {
                    return GetExecutionMessages(model, false, "File", MessagesResults.Failed,
                        ExecutionProcessOption.FileSizeExceeded, SystemMessageStatus.Failed.ToString(), null,
                        "File size exceeds the maximum limit of 10MB. Please upload a smaller file.");
                }

            

                if (string.IsNullOrWhiteSpace(model.CollectorId))
                {
                    return GetExecutionMessages(model, false, "CollectorId", MessagesResults.Failed,
                        ExecutionProcessOption.ValidationError, SystemMessageStatus.Failed.ToString(), null,
                        "Collector ID is required. Please provide a valid Collector ID.");
                }

                // Prepare additional parameters
                var additionalParams = new Dictionary<string, string>      {
            { "file", model.ExcelFile.FileName }
        };
                var urlString = string.Format(APICallHelper.ManualEntryCollectorUploadFileExecution, model.CollectorId);
                var response = await _dailySavingApiHelper.UploadFileToApiAsync<ManualEntryDailyCollectorUploadSummaryDto>(model.ExcelFile, "formFile", urlString, additionalParams);



                // Process API response
                if (response == null)
                {
                    return GetExecutionMessages(model, false, "Response", MessagesResults.Failed,
                        ExecutionProcessOption.ApiResponseNull, SystemMessageStatus.Failed.ToString(), null,
                        "No response received from server. Please try again later or contact support.");
                }

                if ( response.Data != null)
                {
                    // Check the actual response data
                    if (response.Data != null)
                    {
                        return GetExecutionMessages(response, true, "Upload", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                            "File uploaded successfully. Your data is being processed.");
                    }
                    else
                    {
                        return GetExecutionMessages(model, false, "Processing", MessagesResults.Failed,
                            ExecutionProcessOption.ProcessingFailed, SystemMessageStatus.Failed.ToString(), null,
                            $"File upload processing failed. {response.Message ?? "Please verify your data and try again."}");
                    }
                }
                else
                {
                    // Handle failed response with user-friendly message
                    var errorMessage = !string.IsNullOrWhiteSpace(response.Message)
                        ? response.Message
                        : "File upload failed. Please try again later or contact support.";

                    return GetExecutionMessages(model, false, "Upload", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                        errorMessage);
                }
            }
            catch (ArgumentException argEx)
            {
                // Handle argument-specific exceptions
                return GetExecutionMessages(model, false, "Arguments", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), argEx,
                    "Invalid input provided. Please check your data and try again.");
            }
            catch (HttpRequestException httpEx)
            {
                // Handle HTTP-specific exceptions
                return GetExecutionMessages(model, false, "Network", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), httpEx,
                    "Network error occurred. Please check your connection and try again.");
            }
            catch (TaskCanceledException tcEx)
            {
                // Handle timeout exceptions
                return GetExecutionMessages(model, false, "Timeout", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), tcEx,
                    "Request timed out. Please try again later.");
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                return GetExecutionMessages(model, false, "General", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex,
                    "An unexpected error occurred while uploading the file. Please try again later or contact support.");
            }
        }
    }

}
