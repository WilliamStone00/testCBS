using BusinessServices;
using CBS.API.Helper;

using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
 using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.DailyCollectionServices
{
  

    public class CommissionSettingServices : BaseService
    {
        private readonly ApiCallerHelper _dailyCollectionApiHelper;

        public CommissionSettingServices( )
        {
            _dailyCollectionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["DailyCollectionBaseUrl"].ToString());
            
        }
 

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var CommissionSetting = await GetCommissionSettingById(id);
                var inResponse = await _dailyCollectionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_CommissionSetting, id));
                if (inResponse.ApiResponseData != null && inResponse.IsSuccess)
                {

                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"BranchId: {CommissionSetting.BranchId} AgentId: {CommissionSetting.AgentId}  ", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(CommissionSetting, false, $"BranchId: {CommissionSetting.BranchId} AgentId: {CommissionSetting.AgentId}  ", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }


        public async Task<IEnumerable<CommissionSetting>> GetCommissionSettings()
        {
            try
            {

                var apiResponse = await _dailyCollectionApiHelper.GetAsync<ResponseObject<List<CommissionSetting>>>(APICallHelper.GetAllCommissionSettings);
                if (apiResponse.IsSuccess)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<CommissionSetting>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

 
        public async Task<CommissionSetting> GetCommissionSettingById(string id)
        {
            try
            {
                var cusResponseObject = await _dailyCollectionApiHelper.GetAsync<ResponseObject<CommissionSetting>>(string.Format(APICallHelper.Get_Update_Delete_CommissionSetting, id));
                if (cusResponseObject.ApiResponseData != null)
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

  
        public async Task<ExecutionMessages> Create(CommissionSetting model)
        {
            try
            {

                model.Id = "NO_ID";
                var response = await _dailyCollectionApiHelper.PostAsync<ServiceResponse<CommissionSetting>>(APICallHelper.CreateCommissionSetting, model);
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
        public async Task<ExecutionMessages> Update(CommissionSetting model)
        {
            try
            {

                var ZoneDto = await GetCommissionSettingById(model.Id);
                if (ZoneDto != null)
                {


                    var response = await _dailyCollectionApiHelper.PutAsync<ServiceResponse<CommissionSetting>>(string.Format(APICallHelper.Get_Update_Delete_CommissionSetting, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"CommissionSettingId:{model.Id}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.Id, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
