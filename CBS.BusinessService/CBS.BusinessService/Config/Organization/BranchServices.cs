using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Config
{
    public class BranchServices : BaseService
    {
        private readonly ApiCallerHelper _BranchConfigApiHelper;

        public BranchServices()
        {
            _BranchConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objBranch = await GetBranch(id);
                var inResponse = await _BranchConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Branch, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objBranch.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objBranch, false, $"{objBranch.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions)
        {
            Func<Task<List<Branch>>> getDataFunc = async () => (await GetCountries()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<Branch>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<Branch>> GetCountries()
        {
            try
            {
                var couApiResponse = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                return couApiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        private async Task<Branch> GetBranch(string id)
        {
            try
            {
                var cusResponseObject = await _BranchConfigApiHelper.GetAsync<ResponseObject<Branch>>(string.Format(APICallHelper.Get_Update_Delete_Branch, id));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(Branch model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _BranchConfigApiHelper.PostAsync<ServiceResponse<Branch>>(APICallHelper.CreateBranch, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, null);
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
