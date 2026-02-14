
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FileUpload;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
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
        public async Task<FileDownloadDto> DownloadExecutionFile(string fileId)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.PostAsync<ResponseObject<FileDownloadDto>>(APICallHelper.DownloadReexecution,new { fileUploadId = fileId});

                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return null;

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

                var fileUploads = await GetUploadForAnalysis(allFileUploadSalaryFileActivatedQuery, path);
                    stringValues = (from a in fileUploads
                                    
                                    select new StringValues
                                    {
                                        Text = $"[Code: {a.FileUploadId}] [Name: {a.FileName}] [Type: {a.FileType}] [Date: {a.UploadedOn.ToString("MMM/yyy")}]",
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

        public async Task<IEnumerable<FileUploadDto>> GetUploadDtosAsyncByStatus(
            GetAllFileUploadSalaryFileActivatedQuery query,
            string path)
        {
            try
            {
                var queryString = ToQueryString(query);
                var fullUrl = $"{APICallHelper.GetAllSalaryUploadByFileBaseOnStatus}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<FileUploadDto>>>(fullUrl);

                if (response?.ApiResponseData?.Data == null)
                    return Enumerable.Empty<FileUploadDto>();

                var data = response.ApiResponseData.Data;
                var isHeadOffice = IsHeadOffice();

                // Head Office: see all files
                if (isHeadOffice) return data;

                // Branch users: see only files uploaded by *their* branch
                var myBranchId = GetBranchID();
                if (string.IsNullOrWhiteSpace(myBranchId))
                    return Enumerable.Empty<FileUploadDto>();

                return data.Where(f =>
                    !string.IsNullOrWhiteSpace(f.BranchId) &&
                    f.BranchId.Equals(myBranchId, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<FileUploadDto>> GetUploadForAnalysis(
    GetAllFileUploadSalaryFileActivatedQuery query,
    string path)
        {
            try
            {
                // Build URL and call API
                var queryString = ToQueryString(query);
                var fullUrl = $"{APICallHelper.GetAllSalaryUploadByFileBaseOnStatus}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<FileUploadDto>>>(fullUrl);

                // Nothing returned from API
                if (response?.ApiResponseData?.Data == null)
                    return Enumerable.Empty<FileUploadDto>();

                var data = response.ApiResponseData.Data;

                // If you're at Head Office, show EVERYTHING
                var isHeadOffice = IsHeadOffice();
                if (isHeadOffice)
                    return data;

                // Otherwise, apply branch/private/activation visibility rules
                var myBranchId = GetBranchID() ?? string.Empty;

                var visible = data.Where(f =>
                {
                    // Always allow own-branch uploads (private or public, activated or not)
                    var isOwnBranch = !string.IsNullOrWhiteSpace(f.BranchId) &&
                                      f.BranchId.Equals(myBranchId, StringComparison.OrdinalIgnoreCase);

                    if (isOwnBranch)
                        return true;

                    // For other branches: must be public (not private) AND activated
                    var isPublicAndActivated = !f.PrivateView && f.IsAvalaibleForExecution;

                    return isPublicAndActivated;
                });

                return visible;
            }
            catch
            {
                // You can log here if needed
                throw;
            }
        }

        public List<FileUploadDto> GetUploadForAnalysis(List<FileUploadDto> fileUploads)
        {
            try
            {
                // Build URL and call API

                 fileUploads = fileUploads.Where(x => x.FileType!="Analysis").ToList();
                // If you're at Head Office, show EVERYTHING
                var isHeadOffice = IsHeadOffice();
                if (isHeadOffice)
                    return fileUploads;

                // Otherwise, apply branch/private/activation visibility rules
                var myBranchId = GetBranchID() ?? string.Empty;

                var visible = fileUploads.Where(f =>
                {
                    // Always allow own-branch uploads (private or public, activated or not)
                    var isOwnBranch = !string.IsNullOrWhiteSpace(f.BranchId) && f.BranchId.Equals(myBranchId, StringComparison.OrdinalIgnoreCase);

                    if (isOwnBranch)
                        return true;

                    // For other branches: must be public (not private) AND activated
                    var isPublicAndActivated = !f.PrivateView && f.IsAvalaibleForExecution;

                    return isPublicAndActivated;
                });

                return visible.ToList();
            }
            catch
            {
                // You can log here if needed
                throw;
            }
        }
        public List<FileUploadDto> GetFileUploads(List<FileUploadDto> fileUploads)
        {
            try
            {
                var data = fileUploads.Where(x=>x.FileCategory=="SalaryAnalysisExtract").ToList();
                if (!IsHeadOffice())
                {
                    return data.Where(x=>x.BranchId==GetBranchID()).ToList();
                }
                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
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
                var fields = new Dictionary<string, string>
                {
                    ["SalaryType"] = model.SalaryType,
                    ["BranchId"] = model.BranchId,
                    ["StandingOrderSourceChartOfAccountId"] = model.StandingOrderSourceChartOfAccountId ?? string.Empty,
                    ["PrivateView"] = model.PrivateView.ToString() // "True"/"False"
                };

                var response = await _transactionApiHelper
                    .UploadFileAsync<ServiceResponse<SalaryUploadModelSummaryDto>>(
                        file: model.File,
                        apiUrl: APICallHelper.CreateSalaryUpload,
                        fields: fields
                    );

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }

                GetExecutionMessages(model, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> ReexecuteSalaryUploadFile(AddSalaryUploadModelCommand model)
        {
            try
            {
                var fields = new Dictionary<string, string>  { };

                var response = await _transactionApiHelper
                    .UploadFileAsync<ServiceResponse<bool>>(
                        file: model.File,
                        apiUrl: APICallHelper.ReexecuteSalaryFileUpload,
                        fileFieldName:"formFile",
                        fields: fields
                    );

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }

                GetExecutionMessages(model, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        //public async Task<ExecutionMessages> UploadFile(AddSalaryUploadModelCommand model)
        //{
        //    try
        //    {

        //        var response = await _transactionApiHelper.UploadSalaryFileAsync<ServiceResponse<SalaryUploadModelSummaryDto>>(model.File, model.SalaryType, APICallHelper.CreateSalaryUpload);
        //        if (response.IsSuccess)
        //        {
        //            // Successful creation
        //            GetExecutionMessages(response, true, null, MessagesResults.Success,
        //                ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
        //            return ExecutionMessage;
        //        }
        //        else
        //        {
        //            // Failed creation
        //            GetExecutionMessages(model, false, null, MessagesResults.Failed,
        //                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
        //            SystemMessageStatus.Failed.ToString(), ex);
        //    }
        //    return ExecutionMessage;
        //}



    }

}
