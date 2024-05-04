using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;

namespace CBS.BusinessService.Config.Localization
{
   
    public class CountryServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;

        public CountryServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objCountry = await GetCountry(id);
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_Country,id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objCountry.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objCountry, false, $"{objCountry.Name}", MessagesResults.Failed,
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
            Func<Task<List<Country>>> getDataFunc = async () => (await GetCountries()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<Country>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<Country>> GetCountries()
        {
            try
            {
                var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<Country>>>(APICallHelper.GetAllCountry);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Country>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Country> GetCountry(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<Country>>(string.Format(APICallHelper.Get_Update_Delete_Country, id));
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
        public async Task<ExecutionMessages> Create(Country model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _bankConfigApiHelper.PostAsync<ServiceResponse<Country>>(APICallHelper.CreateCountry, model);
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
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
        public async Task<ExecutionMessages> Update(Country model)
        {
            try
            {

                var country = await GetCountry(model.Id);
                if (country != null)
                {
                    country.Name = model.Name;
                    country.Code = model.Code;
                    var response = await _bankConfigApiHelper.PutAsync<ServiceResponse<Country>>(string.Format(APICallHelper.Get_Update_Delete_Country, model.Id), country);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
