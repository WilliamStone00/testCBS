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

namespace CBS.BusinessService
{
  
    public class LoanServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());

        }

        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial)
        {
            var customerParam = new CustomerResource
            {
                OrderBy = "CustomerId",
                PageSize = dataTableOptions.pageSize,
                Skip = dataTableOptions.skip,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
            };
            var loans = await GetLoans(customerParam);
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


        public async Task<IEnumerable<Loan>> GetLoans(CustomerResource resource)
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
                DisbursementDate = loan.DisbursementDate.ToString("yyyy-MM-dd"),
                MaturityDate = loan.MaturityDate.ToString("yyyy-MM-dd"),
                Principal = loan.Principal,
                InterestRate = loan.InterestRate,
                AccrualInterest = loan.AccrualInterest,
                Fee = loan.Fee,
                Penalty = loan.Penalty,
                DueAmount = loan.DueAmount,
                Paid = loan.Paid,
                Balance = loan.Balance,
                LastPayment = loan.LastPayment,
                LoanStatus = loan.LoanStatus,
                IsCurrentLoan = loan.IsCurrentLoan,
                Id = loan.Id, CustomerId=loan.CustomerId
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
                    return couApiResponse.ApiResponseData.Data.Where(x=>!x.IsLoanDisbursted);
                }
                return new List<Loan>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<Loan>> GetLoanByCustomerID(string customerId)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Loan>>>(string.Format(APICallHelper.GetAllLoanByCustomerId, customerId));
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
        public async Task<IEnumerable<Loan>> GetCustomerCurrentLoans(string customerId)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<Loan>>>(string.Format(APICallHelper.GetAllLoanByCustomerId, customerId));
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

    }

}
