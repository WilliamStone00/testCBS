using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.Overdraft;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Overdraft
{
    public class OverdraftActivationServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public OverdraftActivationServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.OverdraftServiceActivationSubmit, id));
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

        public async Task<IEnumerable<OverdraftActivation>> GetAll()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<OverdraftActivation>>>(APICallHelper.OverdraftServiceActivationDT);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<OverdraftActivation>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<CustomDataTable> GetDataTableAsync(GetOverdraftActivationRequestsDataTableQuery loansDataTableQuery)
        {
            loansDataTableQuery.DataTableOptions.sortColumnName = "RequestedAt";
            if (!IsHeadOffice())
            {
                loansDataTableQuery.BranchId=GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.OverdraftServiceActivationDT,
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
        public async Task<OverdraftActivation> Get(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<OverdraftActivation>>(string.Format(APICallHelper.OverdraftServiceActivationGetById, id));
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
        public async Task<ExecutionMessages> Create(OverdraftActivation model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.OverdraftServiceActivationSubmit, model);
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
        public async Task<ExecutionMessages> Approved(OverdraftActivation model)
        {
            try
            {

                var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.OverdraftServiceActivationApprove, model.Id);
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
