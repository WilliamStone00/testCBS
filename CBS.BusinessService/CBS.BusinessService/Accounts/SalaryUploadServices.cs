
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CBS.BusinessService.Accounts
{
   
    public class SalaryUploadServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public SalaryUploadServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_SalaryUpload, id));
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
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<FileUploadDto>>>(string.Format(APICallHelper.GetAllSalaryUploadByFileCategory, "SalaryModelExtraction"));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileUploadDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ExecutionMessages> UpdateFileStatus(ActivateSalaryFileCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.ActivateSalaryUpload, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public async Task<FileDownloadDto> DownloadFile(string fileId)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<FileDownloadDto>>(string.Format(APICallHelper.DownloadSalaryUpload, fileId));

                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return new FileDownloadDto { ErrorMessage = couApiResponse.Message };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<SalaryUploadModel>> GetSalaryModelByBranchUsingFileId(string fileId)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<SalaryUploadModel>>>(string.Format(APICallHelper.GetSalaryModelForBranchByFileId, fileId));

                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<SalaryUploadModel>();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<SalaryUploadModel>> GetSalaryUploads(string FileUploadId)
        {
            try
            {
             
                GetSalaryUploadModelQuery allStandingOrdersQuery = new GetSalaryUploadModelQuery { FileUploadId=FileUploadId };
                var queryString = ToQueryString(allStandingOrdersQuery);
                var fullUrl = $"{APICallHelper.GetAllSalaryUploadByFileUploadId}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<SalaryUploadModel>>>(fullUrl);
                var data = new List<SalaryUploadModel>();
                if (response.ApiResponseData != null)
                {
                    data = response.ApiResponseData.Data;
                }
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<SalaryUploadModelWithBranchStatisticsDto> GetSalaryUploadModelWithBranchStatistics(string FileUploadId)
        {
            try
            {

                GetSalaryUploadModelWithBranchStatisticsQuery allStandingOrdersQuery = new GetSalaryUploadModelWithBranchStatisticsQuery { FileUploadId=FileUploadId, IncludeBranchStatistics=true };
                var queryString = ToQueryString(allStandingOrdersQuery);
                var fullUrl = $"{APICallHelper.GetAllSalaryUploadWithBranchstatisticsByFileUploadId}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<SalaryUploadModelWithBranchStatisticsDto>>(fullUrl);
                var data = new SalaryUploadModelWithBranchStatisticsDto();
                if (response.ApiResponseData != null)
                {
                    data = response.ApiResponseData.Data;
                }
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        
        public async Task<IEnumerable<StringValues>> GetValues(GetAllFileUploadSalaryFileActivatedQuery allFileUploadSalaryFileActivatedQuery, string path)
        {
            try
            {
                List<StringValues> stringValues;

                var fileUploads = await GetUploadDtosAsyncByStatus(allFileUploadSalaryFileActivatedQuery, path);
                    stringValues = (from a in fileUploads
                                    
                                    select new StringValues
                                    {
                                        Text = $"[Salary Code: {a.FileCode}] [Execution State: {a.FileCategory}] [Name: {a.FileName}]",
                                        Value = $"{a.Id}",
                                    }).ToList();
                return stringValues;
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                throw ex;
            }
        }

        public async Task<IEnumerable<FileUploadDto>> GetUploadDtosAsyncByStatus(GetAllFileUploadSalaryFileActivatedQuery query, string path)
        {
            try
            {
                var queryString = ToQueryString(query);
                var fullUrl = $"{APICallHelper.GetAllSalaryUploadByFileBaseOnStatus}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<FileUploadDto>>>(fullUrl);

                if (response?.ApiResponseData?.Data == null)
                    return Enumerable.Empty<FileUploadDto>();

                var data = response.ApiResponseData.Data;

                // Normalize path
                path = path?.ToLowerInvariant();

                // Filter by file type
                if (path == "cs_salary")
                {
                    data = data.Where(x => x.FileType?.ToLower().Contains("civil") == true).ToList();

                    // ❌ No branch filtering for cs_salary
                    return data;
                }
                else if (path == "ps_salary" || path == "pi_salary")
                {
                    data = data.Where(x => x.FileType?.ToLower().Contains("private") == true).ToList();

                    // ✅ Filter by branch if not head office
                    if (!IsHeadOffice())
                    {
                        var branchId = GetBranchID();
                        data = data.Where(x => x.BranchId == branchId).ToList();
                    }

                    return data;
                }
               // BranchCode, Acc Number, Names, GSalary
                // Fallback case — no filtering
                return data;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                throw;
            }
        }


        public async Task<FileUploadDto> GetFileUpload(string fileId)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<FileUploadDto>>(string.Format(APICallHelper.GetSalaryFileUploadByFileId, fileId));
                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return new FileUploadDto { };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ExecutionMessages> UploadFile(AddSalaryUploadModelCommand model)
        {
            try
            {
               
                var response = await _transactionApiHelper.UploadSalaryFileAsync<ServiceResponse<SalaryUploadModelSummaryDto>>(model.File, model.SalaryType, APICallHelper.CreateSalaryUpload);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        
       
       
    }

}
