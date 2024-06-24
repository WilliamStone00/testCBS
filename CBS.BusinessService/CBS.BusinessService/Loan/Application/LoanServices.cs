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

namespace CBS.BusinessService
{

    public class LoanServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;

        public LoanServices(BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
        }

        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial, bool isByBranch = true)
        {
            var loanResource = new LoanResource
            {
                OrderBy = "CustomerId",
                PageSize = dataTableOptions.pageSize,
                Skip = dataTableOptions.skip,
                IsByBranch = isByBranch,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
            };
            var loans = await GetLoans(loanResource);
            // Handle potential null values safely
            var paginationMetadata = loans?.FirstOrDefault()?.PaginationMetadata ?? new PaginationMetadata
            {
                TotalCount = 0
            };

            Func<Task<List<DataTableLoan>>> getDataFunc = async () => (await GetDataTable(loans.ToList()));
            var dataTable = await GenerateDataTable(dataTableOptions, getDataFunc, paginationMetadata.TotalCount);
            return dataTable;

        }

        public async Task<CustomDataTable> GenerateDataTable(DataTableOptions dataTableOptions, Func<Task<List<DataTableLoan>>> getDataFunc, int totalRecords)
        {
            List<DataTableLoan> data = (await getDataFunc()).ToList();
            dataTableOptions.recordsTotal = totalRecords;
            //var filteredData = DatatableHelper.FilterData(data, dataTableOptions);
            // Construct CustomDataTable using pagination metadata
            var dataTable = new CustomDataTable(
                Convert.ToInt32(dataTableOptions.draw),
                totalRecords,
                dataTableOptions.recordsFiltered,
                data,
                dataTableOptions
            );

            return dataTable;
        }


        public async Task<IEnumerable<Loan>> GetLoans(LoanResource resource)
        {
            try
            {

                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.GelLoansSearchByAnyCriterialQuery}?{queryString}";
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Loan>>>(fullUrl);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<List<DataTableLoan>> GetDataTable(List<Loan> loans)
        {

            var loanDtos = loans.Select(loan => new DataTableLoan
            {
                DisbursementDate = loan.DisbursementDate.ToString("yyyy-MM-dd, hh:mm:ss"),
                MaturityDate = loan.MaturityDate.ToString("yyyy-MM-dd"),
                Principal = loan.Principal,
                InterestRate = loan.InterestRate,
                AccrualInterest = loan.AccrualInterest,
                Fee = loan.Fee,
                Tax = loan.Tax,
                Fines=loan.Penalty,
                Penalty = loan.Penalty,
                DueAmount = loan.DueAmount,
                Paid = loan.Paid,
                Balance = loan.Balance,
                LastPayment = loan.LastPayment,
                LoanStatus = loan.LoanStatus,
                IsCurrentLoan = loan.IsCurrentLoan,
                Id = loan.Id,
                CustomerId = loan.CustomerId
            }).ToList();
            return loanDtos;


        }
        public async Task<ExecutionMessages> ApprovePendingDisbursement(AddLoanDisbumentCommand model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanApplication>>(APICallHelper.Disbursed, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Disbursement", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Disbursement", MessagesResults.Failed,
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
        public async Task<IEnumerable<Loan>> GetPendingDisbursementLoans()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Loan>>>(APICallHelper.GetLoans);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data.Where(x => !x.IsLoanDisbursted);
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<FileDownloadInfoLoan>> GetAllFileDownloadInfoLoan()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FileDownloadInfoLoan>>>(APICallHelper.GetAllBulkDownloadInfosLoan);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileDownloadInfoLoan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<FileDownloadInfoLoan>> GetAllFileDownloadInfoLoanPerUser()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FileDownloadInfoLoan>>>(string.Format(APICallHelper.GetAllBulkDownloadInfosLoanPerUser, GetUserID()));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FileDownloadInfoLoan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.BulkDownloadDeleteAndGetLoan, id));
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
        public async Task<FileDownloadInfoLoan> GetFileDownloadInfoLoan(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FileDownloadInfoLoan>>(string.Format(APICallHelper.BulkDownloadDeleteAndGetLoan, id));
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
        public async Task<ExecutionMessages> InitiateBulkDownloadLoansBranch(InitiateLoanDownloadCommand model)
        {
            try
            {
                model.IsByBranch = true;
                model.StartDate = GetDateTime(model.StrStartDate);
                model.EndDate = GetDateTime(model.StrEndDate);
                model.EndDate = GetDateTime(model.StrEndDate);
                model.UserId = GetUserID();
                model.FullName = GetUserFullName();
                model.BranchName = GetBranchName();
                model.BranchId = GetBranchID();
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<FileDownloadInfoLoan>>(APICallHelper.InitiateBulkDownloadLoans, model);
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
        public async Task<IEnumerable<Loan>> GetLoanByCustomerID(GetAllLoanByCustomerIdQuery loanByCustomerIdQuery)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<List<Loan>>>(APICallHelper.GetAllLoanByCustomerId, loanByCustomerIdQuery);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<Loan> GetLoan(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<Loan>>(string.Format(APICallHelper.GetLoan, id));
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
        public async Task<Loan> GetLoanWithCustomerAndBranch(string customerId)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<Loan>>(string.Format(APICallHelper.GetLoan, customerId));
                if (cusResponseObject.IsSuccess && cusResponseObject.ApiResponseData != null)
                {
                    var loan = cusResponseObject.ApiResponseData.Data;
                    var individualCustomerProfile = await _individualProfileServices.GetCustomerLight(loan.CustomerId);
                    loan.IndividualCustomer = individualCustomerProfile;
                    return loan;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }

        }

    }

}
