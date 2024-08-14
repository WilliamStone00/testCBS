using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using System.Web.Mvc;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.Config;
using DocumentFormat.OpenXml.Bibliography;

namespace CBS.BusinessService
{

    public class FileDownloadServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;

        public FileDownloadServices(BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
        }

        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial, bool isByBranch = true, string branchid = "N/A")
        {
            var pagginationResource = new PagginationResource
            {
                OrderBy = "CustomerName",
                PageSize = dataTableOptions.pageSize,
                Skip = dataTableOptions.skip,
                IsByBranch = isByBranch,
                BranchId = branchid,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
            };
            var membersAccountSummaries = await GetAllMembersAccountSummaries(pagginationResource);
            // Handle potential null values safely
            var paginationMetadata = membersAccountSummaries?.FirstOrDefault()?.PaginationMetadata ?? new PaginationMetadata
            {
                TotalCount = 0
            };

            Func<Task<List<MembersAccountSummaryDto>>> getDataFunc = async () => (await GetDataTable(membersAccountSummaries.ToList()));
            var dataTable = await GenerateDataTable(dataTableOptions, getDataFunc, paginationMetadata.TotalCount);
            return dataTable;

        }

        public async Task<CustomDataTable> GenerateDataTable(DataTableOptions dataTableOptions, Func<Task<List<MembersAccountSummaryDto>>> getDataFunc, int totalRecords)
        {
            List<MembersAccountSummaryDto> data = (await getDataFunc()).ToList();
            dataTableOptions.recordsTotal = totalRecords;
            var dataTable = new CustomDataTable(
                Convert.ToInt32(dataTableOptions.draw),
                totalRecords,
                dataTableOptions.recordsFiltered,
                data,
                dataTableOptions
            );

            return dataTable;
        }
        public async Task<List<MembersAccountSummaryDto>> GetDataTable(List<MembersAccountSummary> loans)
        {

            var loanDtos = loans.Select(source => new MembersAccountSummaryDto
            {
                MemberName = source.MemberName,
                MemberReference = source.MemberReference,
                BranchCode = source.BranchCode,
                Saving = source.Saving,
                PreferenceShare = source.PreferenceShare,
                Share = source.Share,
                Deposit = source.Deposit,
                Loan = source.Loan,
                Gav = source.Gav,
                DailyCollection = source.DailyCollection,
                TotalBalance = source.TotalBalance,
                NetBalance = source.NetBalance,
            }).ToList();
            return loanDtos;


        }
        public async Task<IEnumerable<MembersAccountSummary>> GetAllMembersAccountSummaries(PagginationResource resource)
        {
            try
            {

                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.GetAllMembersPagginatedSummaryAccounts}?{queryString}";
                var response = await _transactionApiHelper.GetAsync<ResponseObject<List<MembersAccountSummary>>>(fullUrl);
                var data = new List<MembersAccountSummary>();
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
        public async Task<IEnumerable<FileDownloadInfo>> GetAllFileDownloadInfo()
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<FileDownloadInfo>>>(APICallHelper.GetAllBulkDownloadInfosLoan);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileDownloadInfo>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<FileDownloadInfo>> GetAllFileDownloadInfoPerUser()
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<FileDownloadInfo>>>(string.Format(APICallHelper.GetAllBulkDownloadInfosLoanPerUser, GetUserID()));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileDownloadInfo>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<FileDownloadDto> DownloadFile(string fileId)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<FileDownloadDto>>(string.Format(APICallHelper.DownloadLoanFile, fileId));

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


        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.BulkDownloadDeleteAndGetLoan, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(inResponse, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<FileDownloadInfo> GetFileDownloadInfo(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<FileDownloadInfo>>(string.Format(APICallHelper.BulkDownloadDeleteAndGetLoan, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> InitiateBulkDownloadBranch(InitiateLoanDownloadCommand model)
        {
            try
            {
//                {
//                    "isByBranch": true,
//  "branchId": "string",
//  "isUnpaidOnly": true,
//  "queryParameter": "string"
//}


                model.IsByBranch = true;
                model.BranchId = model.BranchId;
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<FileDownloadInfo>>(APICallHelper.InitiateBulkDownloadIndividualAccountBalances, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Bulk Download", MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Bulk Download", MessagesResults.Failed,
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
