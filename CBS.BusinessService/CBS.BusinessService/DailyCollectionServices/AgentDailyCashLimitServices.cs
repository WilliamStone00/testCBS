using BusinessServices;
using CBS.API.Helper;

using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using CBS.FrontDesk.Data.Entity.DailyCollectorManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.DailyCollectionServices
{
  

    public class AgentDailyCashLimitServices : BaseService
    {
        private readonly ApiCallerHelper _dailyCollectionApiHelper;

        public AgentDailyCashLimitServices( )
        {
            _dailyCollectionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["DailyCollectionBaseUrl"].ToString());
            
        }
 

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var AgentDailyCashLimit = await GetAgentDailyCashLimitById(id);
                var inResponse = await _dailyCollectionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_AgentDailyCashLimit, id));
                if (inResponse.ApiResponseData != null && inResponse.IsSuccess)
                {

                    // Handle success scenario
                    GetExecutionMessages(AgentDailyCashLimit, false, $"branchId: {AgentDailyCashLimit.branchId} AgentId: {AgentDailyCashLimit.agentId} ", MessagesResults.Failed,
                   ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(AgentDailyCashLimit, false, $"branchId: {AgentDailyCashLimit.branchId} AgentId: {AgentDailyCashLimit.agentId} ", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }


        public async Task<IEnumerable<AgentDailyCashLimit>> GetAgentDailyCashLimits()
        {
            try
            {

                var apiResponse = await _dailyCollectionApiHelper.GetAsync<ResponseObject<List<AgentDailyCashLimit>>>(APICallHelper.GetAllAgentDailyCashLimit);
                if (apiResponse.IsSuccess)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<AgentDailyCashLimit>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

 
        public async Task<AgentDailyCashLimit> GetAgentDailyCashLimitById(string id)
        {
            try
            {
                var cusResponseObject = await _dailyCollectionApiHelper.GetAsync<ResponseObject<AgentDailyCashLimit>>(string.Format(APICallHelper.Get_Update_Delete_AgentDailyCashLimit, id));
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

  
        public async Task<ExecutionMessages> Create(AgentDailyCashLimit model)
        {
            try
            {
      
      
                var response = await _dailyCollectionApiHelper.PostAsync<ServiceResponse<AgentDailyCashLimit>>(APICallHelper.CreateAgentDailyCashLimit, model);
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

        public async Task<ExecutionMessages> Update(AgentDailyCashLimit model)
        {
            try
            {

                var ZoneDto = await GetAgentDailyCashLimitById(model.agentId);
                if (ZoneDto != null)
                {


                    var response = await _dailyCollectionApiHelper.PutAsync<ServiceResponse<OperationEvent>>(string.Format(APICallHelper.Get_Update_Delete_AgentDailyCashLimit, model.agentId), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Agent:{model.agentId}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.agentId, MessagesResults.Failed,
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
