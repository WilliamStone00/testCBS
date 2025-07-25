using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
        public async Task<IEnumerable<FileUploadDto>> GetFileUploads()
        {
            try
            {

                var couApiResponse = await _dailySavingApiHelper.GetAsync<ResponseObject<List<FileUploadDto>>>(string.Format(APICallHelper.GetAllDailySavingMigrationFile, "SalaryAnalysisExtract"));
                if (couApiResponse.IsSuccess)
                {
                    var data = couApiResponse.ApiResponseData.Data;
                    if (!IsHeadOffice())
                    {
                        return data.Where(x => x.BranchId == GetBranchID());
                    }
                    return data;


                }
                return new List<FileUploadDto>();
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
                // Validate input file
                if (model.ExcelFile == null)
                {
                    return GetExecutionMessages(null, false, "File", MessagesResults.Failed,
                        ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
                        "No file was selected. Please upload a valid file.");
                }

                // Prepare additional parameters
                var additionalParams = new Dictionary<string, string>
        {
            { "UploadFileId", "model.UploadFileId" }
        };

                // Prepare file list
                var httpPostedFileBases = new List<HttpPostedFileBase> { model.ExcelFile };

                // Send request to API
                var response = await _dailySavingApiHelper.PostFilesAndParamsAsync<ServiceResponse<bool>>(
                    APICallHelper.DailySavingMigrationFileExecution, additionalParams, httpPostedFileBases);

                // Process API response
                if (response.ApiResponseData == null)
                {
                    if (response.IsSuccess)
                    {
                        return GetExecutionMessages(response, true, null, MessagesResults.Success,
                                 ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                                 "File uploaded successfully. Your data is being processed.");

                    }

                }

                // Handle failed response with user-friendly message
                return GetExecutionMessages(model, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                    $"File upload failed. {response.Message ?? "Please try again later or contact support."}");
            }
            catch (Exception ex)
            {
                // Handle exception with a detailed message
                return GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex,
                    "An error occurred while uploading the file. Please try again later or contact support.");
            }
        }



  
    }

}
