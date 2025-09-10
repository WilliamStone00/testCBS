using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Accounting
{
  
    public class AccountingReconciliationServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public AccountingReconciliationServices(UserManagementServices userManagementServices = null)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
     
        public async Task<ExecutionMessages> GetTransactionTrackers(QueryFilter modelc)
        {
            try
            {
                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ResponseObject<List<TransactionTracker>>>(APICallHelper.RetrieveAllUnProcessedTransactionTracker, new { fromDate = modelc.FromDate, todate = modelc.ToDate, branchId = modelc.BranchId });
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response.ApiResponseData, true, $"TransactionTracker added and", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(modelc, false, "model.Id", MessagesResults.Failed,
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
        public List<TransactionTracker> MapToDtoOrdered(List<TransactionTracker> entities)
        {
            return entities

                .Select(ent => new TransactionTracker
                {
                    Id = ent.Id,
                    CommandDataType = ent.CommandDataType,
                    CommandJsonObject = ent.CommandJsonObject,
                    TransactionReferenceId = ent.TransactionReferenceId,
                    HasPassed = ent.HasPassed,
                    NumberOfRetry = ent.NumberOfRetry,
                    DatePassed = ent.DatePassed,
                    TransactionDate = ent.TransactionDate,
                    DestinationUrl = ent.DestinationUrl,
                    SourceUrl = ent.SourceUrl,
                    UserFullName = ent.UserFullName,
                    BranchId = ent.BranchId,
                    BranchCode = ent.BranchCode,
                    CreatedDate = ent.CreatedDate,
                    ErrorOrSuccessMessage = ent.ErrorOrSuccessMessage
                }).OrderByDescending(x => x.TransactionDate).ToList();
        }

        public async Task<CustomDataTable> GetDataTableAsync(GetCMoneyMemberActivationsDatatableQuery loansDataTableQuery)
        {
            loansDataTableQuery.DataTableOptions.sortColumnName = "ActivationDate";
            if (!IsHeadOffice())
            {
                loansDataTableQuery.ByBranch = true;
                loansDataTableQuery.BranchId = GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoaDataTablePagginationFroCmoney,
                loansDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(loansDataTableQuery.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: loansDataTableQuery.DataTableOptions
            );
        }
        public async Task<IExecutionMessages> PostPayload(object value)
        {
            try
            {
                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ResponseObject<List<TransactionTracker>>>(APICallHelper.ReExecutedFailedEntry, value);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response.ApiResponseData, true, $"TransactionTracker added and", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(value, false, "model.Id", MessagesResults.Failed,
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
        public async Task<IEnumerable<TransactionTracker>> GetTransactionTrackers(PaginatedResource resource, List<Branch> branches)
        {
            try
            {

                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.GetTransactionTrackerPaginated}?{queryString}";
                var individualProfiles = await _loanConfigApiHelper.GetAsync<ResponseObject<List<TransactionTracker>>>(fullUrl);
 
                var data = (from a in individualProfiles.ApiResponseData.Data
                            join b in branches on a.BranchId equals b.Id
                            select TransformToCustomerList(a, b)).ToList();

                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        private TransactionTracker TransformToCustomerList(TransactionTracker c, Branch b)
        {
            var customer = new TransactionTracker
            {
                // Copy all original properties
                Id = c.Id,
                CommandDataType = c.CommandDataType,
                CommandJsonObject = c.CommandJsonObject,
                TransactionReferenceId = c.TransactionReferenceId,
                HasPassed = c.HasPassed,
                NumberOfRetry = c.NumberOfRetry,
                DatePassed = c.DatePassed,
                TransactionDate = c.TransactionDate,
                DestinationUrl = c.DestinationUrl,
                SourceUrl = c.SourceUrl,
                UserFullName = c.UserFullName,
                CreatedDate = c.CreatedDate,
                ErrorOrSuccessMessage = c.ErrorOrSuccessMessage,

                // Branch-specific transformations
                BranchId = b?.Id,
                BranchCode = b?.BranchCode,
                BranchOffice=b?.Name

                // Apply any business logic transformations here
                // For example, format user name with branch info:
                // UserFullName = $"{c.UserFullName} ({b?.Name})"
            };
            return customer;
        }
        public async Task<CustomDataTable> GenerateDataTable(DataTableOptions dataTableOptions, Func<Task<List<TransactionTracker>>> getDataFunc)
        {
            List<TransactionTracker> data = (await getDataFunc()).ToList();
            var paginationMetadata =/* data?.FirstOrDefault()?.PaginationMetadata ??*/ new PaginationMetadata
            {
                TotalCount = 0
            };

            // Construct CustomDataTable using pagination metadata
            var dataTable = new CustomDataTable(
                Convert.ToInt32(dataTableOptions.draw),
                paginationMetadata.TotalCount,
                dataTableOptions.recordsFiltered,
                data,
                dataTableOptions
            );

            return dataTable;
        }
    }
}
