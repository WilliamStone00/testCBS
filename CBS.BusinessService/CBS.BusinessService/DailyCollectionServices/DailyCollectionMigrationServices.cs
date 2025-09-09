using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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
              
                // Prepare additional parameters
                var additionalParams = new Dictionary<string, string>{
                 { "formFile", model.ExcelFile.FileName }  };
                var urlString = string.Format(APICallHelper.DailySavingMigrationFileExecution, model.BranchId, model.CollectorId,model.CollectorName);
               var response = await _dailySavingApiHelper.UploadFileToApiAsync<DailySaverUploadResult>(model.ExcelFile, "formFile", urlString, additionalParams);

            
       
                // Process API response
                if (response == null)
                {
                    return GetExecutionMessages(model, false, "Response", MessagesResults.Failed,
                        ExecutionProcessOption.ApiResponseNull, SystemMessageStatus.Failed.ToString(), null,
                        "No response received from server. Please try again later or contact support.");
                }

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    // Check the actual response data
                    if (response.ApiResponseData != null)
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
                var response = await _dailySavingApiHelper.UploadFileToApiAsync<ManualEntryDailyCollectorUploadSummaryDto>(model.ExcelFile, "file", urlString, additionalParams);



                // Process API response
                if (response == null)
                {
                    return GetExecutionMessages(model, false, "Response", MessagesResults.Failed,
                        ExecutionProcessOption.ApiResponseNull, SystemMessageStatus.Failed.ToString(), null,
                        "No response received from server. Please try again later or contact support.");
                }

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    // Check the actual response data
                    if (response.ApiResponseData != null)
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
