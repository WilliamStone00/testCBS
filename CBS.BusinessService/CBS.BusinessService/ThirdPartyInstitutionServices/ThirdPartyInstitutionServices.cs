using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.ThirdPartyBankAccount;
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

namespace CBS.BusinessService.ThirdPartyInstitutionAccount
{
  
    public class ThirdPartyInstitutionServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;

        public ThirdPartyInstitutionServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objThirdPartyInstitution = await GetThirdPartyInstitution(id);
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_ThirdPartyInstitution, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objThirdPartyInstitution.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objThirdPartyInstitution, false, $"{objThirdPartyInstitution.Name}", MessagesResults.Failed,
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
            Func<Task<List<ThirdPartyInstitution>>> getDataFunc = async () => (await GetCountries()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<ThirdPartyInstitution>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<ThirdPartyInstitution>> GetCountries()
        {
            try
            {
                var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<ThirdPartyInstitution>>>(APICallHelper.GetAllThirdPartyInstitution);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<ThirdPartyInstitution>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ThirdPartyInstitution> GetThirdPartyInstitution(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<ThirdPartyInstitution>>(string.Format(APICallHelper.Get_Update_Delete_ThirdPartyInstitution, id));
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
        public async Task<ExecutionMessages> Create(ThirdPartyInstitution model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _bankConfigApiHelper.PostAsync<ServiceResponse<ThirdPartyInstitution>>(APICallHelper.CreateThirdPartyInstitution, model);
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
        public async Task<ExecutionMessages> Update(ThirdPartyInstitution model)
        {
            try
            {

                var ThirdPartyInstitution = await GetThirdPartyInstitution(model.Id);
                if (ThirdPartyInstitution != null)
                {
                    ThirdPartyInstitution.Name = model.Name;
                    ThirdPartyInstitution.Code = model.Code;
                    var response = await _bankConfigApiHelper.PutAsync<ServiceResponse<ThirdPartyInstitution>>(string.Format(APICallHelper.Get_Update_Delete_ThirdPartyInstitution, model.Id), ThirdPartyInstitution);
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
