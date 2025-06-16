using BusinessServices;
using CBS.API.Helper;

using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using CBS.FrontDesk.Data.Entity.DailyCollectorManagement;
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
  

    public class AgentAccountServices : BaseService
    {
        private readonly ApiCallerHelper _dailyCollectionApiHelper;

        public AgentAccountServices( )
        {
            _dailyCollectionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["DailyCollectionBaseUrl"].ToString());
            
        }
 

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var AgentAccount = await GetAgentAccountById(id);
                var inResponse = await _dailyCollectionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_AgentAccount, id));
                if (inResponse.ApiResponseData != null && inResponse.IsSuccess)
                {

                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"AgentId: {AgentAccount.AgentId} BranchId:{AgentAccount.BranchId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(AgentAccount, false, $"AgentId: {AgentAccount.AgentId} BranchId:{AgentAccount.BranchId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }


        public async Task<IEnumerable<AgentAccount>> GetAgentAccounts()
        {
            try
            {

                var apiResponse = await _dailyCollectionApiHelper.GetAsync<ResponseObject<List<AgentAccount>>>(APICallHelper.GetAllAgentAccount);
                if (apiResponse.IsSuccess)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<AgentAccount>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

 
        public async Task<AgentAccount> GetAgentAccountById(string id)
        {
            try
            {
                var cusResponseObject = await _dailyCollectionApiHelper.GetAsync<ResponseObject<AgentAccount>>(string.Format(APICallHelper.Get_Update_Delete_AgentAccount, id));
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

  
        public async Task<ExecutionMessages> Create(AgentAccount model)
        {
            try
            {
      
      
                var response = await _dailyCollectionApiHelper.PostAsync<ServiceResponse<AgentAccount>>(APICallHelper.CreateAgentAccount, model);
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
