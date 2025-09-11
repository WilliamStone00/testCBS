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

namespace CBS.BusinessService.Config.Localization
{
    public class TownServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;

        public TownServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["SystemConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objTown = await GetTown(id);
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Town, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objTown.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objTown, false, $"{objTown.Name}", MessagesResults.Failed,
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
            Func<Task<List<Town>>> getDataFunc = async () => (await GetTowns()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<Town>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<Town>> GetTowns()
        {
            try
            {
                var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<Town>>>(APICallHelper.GetAllTown);
                return couApiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Town> GetTown(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<Town>>(string.Format(APICallHelper.Get_Update_Delete_Town, id));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(Town model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _bankConfigApiHelper.PostAsync<ServiceResponse<Town>>(APICallHelper.CreateTown, model);
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

        public async Task<List<Town>> GetTownsBySubDivisionId(string key)
        {
            var towns = await GetTowns();
            return towns.Where(x => x.SubdivisionId==key).ToList();
        }
    }
}
