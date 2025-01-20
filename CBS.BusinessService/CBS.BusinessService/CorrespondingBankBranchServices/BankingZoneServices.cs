using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Services
{

    public class BankingZoneServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;

        public BankingZoneServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objBankingZone = await GetBankingZone(id);
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_BankingZone, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objBankingZone.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objBankingZone, false, $"{objBankingZone.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        //public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions)
        //{
        //    Func<Task<List<BankingZone>>> getDataFunc = async () => (await GetBankingZone()).ToList();
        //    var dataTable = await DatatableHelper.GenerateDataTable<BankingZone>(dataTableOptions, getDataFunc);
        //    return dataTable;
        //}
        public async Task<List<BankingZoneDto>> GetBankingZone()
        {
            try
            {
                var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<BankingZoneDto>>>(APICallHelper.GetAllBankingZone);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<BankingZoneDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<BankingZone> GetBankingZone(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<BankingZone>>(string.Format(APICallHelper.Get_Update_Delete_BankingZone, id));
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
     
        public async Task<ExecutionMessages> Create(BankingZone model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _bankConfigApiHelper.PostAsync<ServiceResponse<BankingZone>>(APICallHelper.CreateBankingZone, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name} banking zone or banking zone {model.Code}   has been created successfully", MessagesResults.Success,
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
        public async Task<ExecutionMessages> Update(BankingZone model)
        {
            try
            {

                var BankingZone = await GetBankingZone(model.Id);
                if (BankingZone != null)
                {
                    BankingZone.Name = model.Name;
                    BankingZone.Code = model.Code;
                    var response = await _bankConfigApiHelper.PutAsync<ServiceResponse<BankingZone>>(string.Format(APICallHelper.Get_Update_Delete_BankingZone, model.Id), BankingZone);
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

 