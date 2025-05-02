using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.BulkOPerations;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.BulkOperations
{
    public  class BulkOperationService : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;

        public BulkOperationService(BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
        }




        public async Task<ExecutionMessages> GenerateLoanAmortizationSchedule(BulkOperationRangeBetweenAccountSimulationCommand command)
        {
            try
            {
                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ResponseObject<CreateBulkOperationSimulation>>(APICallHelper.SimulateBulkOperationRangeBetweenAccountSimulation, command);
                if (response.IsSuccess)
                {
                    HttpContext.Current.Session["bulk_operation"] = response.ApiResponseData.Data;
                    // Successful creation
                    GetExecutionMessages(response, true, $"bulk_operation", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(response, false, "bulk_operation", MessagesResults.Failed,
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


        public async Task<CustomDataTable> GetBulkOperationDataTableAsync(GetBulkOperationDataTableQuery loansDataTableQuery, string searchCriterial)
        {
            loansDataTableQuery.DataTableOptions.searchValue = searchCriterial;
            loansDataTableQuery.DataTableOptions.search = searchCriterial;
            loansDataTableQuery.DataTableOptions.sortColumnName = "LoanDate";
            if (!IsHeadOffice())
            {
                loansDataTableQuery.BranchId = GetBranchID();
            }
            // Make API call to fetch the DataTable result
            /*  var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                  APICallHelper.LoaDataTablePaggination,
                  loansDataTableQuery
              );*/

            var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<MemberAccountsBulkOperations>>>(
               APICallHelper.GetAllBulkOperations
           );

            

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {

                //For Now
                var customisedCustomDataTable = new CustomDataTable(
                    draw: Convert.ToInt32(loansDataTableQuery.DataTableOptions.draw),
                    recordsTotal: couApiResponse.ApiResponseData.Data.Count,
                    recordsFiltered: couApiResponse.ApiResponseData.Data.Count,
                    data: couApiResponse.ApiResponseData.Data, // No data
                    dataTableOptions: loansDataTableQuery.DataTableOptions
                );

                return customisedCustomDataTable;
                //return couApiResponse.ApiResponseData.Data;
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



    }
}
