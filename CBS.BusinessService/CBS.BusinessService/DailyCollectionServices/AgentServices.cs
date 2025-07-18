using BusinessServices;
using CBS.API.Helper;

using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.DailyCollectorManagement;
using CBS.FrontDesk.Data.Entity.DashBoards;
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
  

    public class AgentServices : BaseService
    {
        private readonly ApiCallerHelper _dailyCollectionApiHelper;
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public AgentServices( )
        {
            _dailyCollectionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["DailyCollectionBaseUrl"].ToString());
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }
 

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var Agent = await GetAgentById(id);
                var inResponse = await _dailyCollectionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Agent, id));
                if (inResponse.ApiResponseData != null && inResponse.IsSuccess)
                {

                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, inResponse.Message, MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(Agent, false, inResponse.Message, MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }


        public async Task<IEnumerable<Agent>> GetAgents()
        {
            try
            {

                var apiResponse = await _dailyCollectionApiHelper.GetAsync<ResponseObject<List<Agent>>>(APICallHelper.GetAllAgent);
                if (apiResponse.IsSuccess)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<Agent>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

 
        public async Task<Agent> GetAgentById(string id)
        {
            try
            {
                var cusResponseObject = await _dailyCollectionApiHelper.GetAsync<ResponseObject<Agent>>(string.Format(APICallHelper.Get_Update_Delete_Agent, id));
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

  
        public async Task<ExecutionMessages> Create(Agent model)
        {
            try
            {
      
      
                var response = await _dailyCollectionApiHelper.PostAsync<ServiceResponse<Agent>>(APICallHelper.CreateAgent, model);
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


        public async Task<FinancialReportData> GetAllActivitiesAsync(DailyCollectionActivitiesQuery salary)
        {
            //{branchId}/{agentId}/{month}/{operationType}
            string _baseUrl = string.Format(APICallHelper.GetCollectorsHistory, salary.CollectorId, salary.Year, salary.Month, salary.BranchId);
            var apiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<FinancialReportData>>(_baseUrl);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new FinancialReportData();
            }
        }
        public async Task<List<DailyCollectorInfo>> GetAgentActiveAgent(string branchId, int month , int year)
        {
            string _baseUrl = string.Format(APICallHelper.GetActiveDailyCollectors, branchId,month, year);

            var apiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<DailyCollectorInfo>>>(_baseUrl);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new List<DailyCollectorInfo>();
            }
        }
        public async Task<CollectorSalarySummaryDto> GetAgentActivitiesAsync(CollectorSalaryInfo salary)
        {
            //{branchId}/{agentId}/{month}/{operationType}
            string _baseUrl = string.Format(APICallHelper.GetAgentActivities, salary.BranchId, salary.CollectorId, salary.Month, salary.OperationType);
            var apiResponse = await _dailyCollectionApiHelper.GetAsync<ResponseObject<CollectorSalarySummaryDto>>(_baseUrl);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new CollectorSalarySummaryDto();
            }
        }

        public async Task<FinancialSummary> GetDailyCollectionActivitiesAsync(DailyCollectionDashboardActivitiesQuery model)
        {
            var salary = model.ConvertToDailyCollectionActivitiesQuery();
            //{branchId}/{agentId}/{month}/{operationType}
            string _baseUrl = string.Format(APICallHelper.GetCollectorsHistory, salary.BranchId, salary.CollectorId, salary.Month);
            var apiResponse = await _dailyCollectionApiHelper.GetAsync<ResponseObject<FinancialSummary>>(_baseUrl);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new FinancialSummary();
            }
        }

        public async Task<ResponseObject<bool>> PayAgentActivitiesAsync(PayDailyCollectorCommission modelData)
        {
    
            var apiResponse = await _savingConfigApiHelper.PostAsync<ResponseObject<bool>>(APICallHelper.PayDailyCollectorCommission, modelData);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData;
            }
            else
            {
                return new ResponseObject<bool>();
            }
        }
    }
}
