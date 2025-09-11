using BusinessServices;
using CBS.API.Helper;

using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity;
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
  

    public class AgentDashBoardServices : BaseService
    {
        private readonly ApiCallerHelper _dailyCollectionApiHelper;

        public AgentDashBoardServices( )
        {
            _dailyCollectionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["DailyCollectionBaseUrl"].ToString());
            
        }
 

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var AgentDashBoard = await GetAgentDashBoardById(id);
                var inResponse = await _dailyCollectionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_AgentDashBoard, id));
                if (inResponse.ApiResponseData != null && inResponse.IsSuccess)
                {

                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"BranchId: {AgentDashBoard.branchId} AgentId: {AgentDashBoard.agentId}  ", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(AgentDashBoard, false, $"BranchId: {AgentDashBoard.branchId} AgentId: {AgentDashBoard.agentId}  ", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }


        public async Task<IEnumerable<AgentDashBoard>> GetAgentDashBoards()
        {
            try
            {

                var apiResponse = await _dailyCollectionApiHelper.GetAsync<ResponseObject<List<AgentDashBoard>>>(APICallHelper.GetAllAgentDashBoard);
                if (apiResponse.IsSuccess)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<AgentDashBoard>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

 
        public async Task<AgentDashBoard> GetAgentDashBoardById(string id)
        {
            try
            {
                var cusResponseObject = await _dailyCollectionApiHelper.GetAsync<ResponseObject<AgentDashBoard>>(string.Format(APICallHelper.Get_Update_Delete_AgentDashBoard, id));
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

  
        public async Task<ExecutionMessages> Create(AgentDashBoard model)
        {
            try
            {
      
      
                var response = await _dailyCollectionApiHelper.PostAsync<ServiceResponse<AgentDashBoard>>(APICallHelper.CreateAgentDashBoard, model);
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
