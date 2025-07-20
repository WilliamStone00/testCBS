using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.DailyCollectorManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Migration
{
    public class AccountMigrationServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public AccountMigrationServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<List<AccountInitDto>> MigrationGLReconciliationAccountList(GetInfoDto model)
        {
            string _baseUrl =   APICallHelper.MigrationGLReconciliationAccountList;

            var apiResponse = await _savingConfigApiHelper.PostAsync<ResponseObject<List<AccountInitDto>>>(_baseUrl, model);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new List<AccountInitDto>();
            }
        }


        public async Task<ExecutionMessages> MigrationGLReconciliation(InitInfoDto model)
        {
            try
            {
                var response = await _savingConfigApiHelper.PostAsync<ResponseObject<AccountInitDto>>(APICallHelper.MigrationGLReconciliationInit, model);

                if (response.ApiResponseData != null)
                {
                    // Successful operation
                    GetExecutionMessages(response, true, null, MessagesResults.Success, ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    // Failed operation
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

       
    
        public async Task<CollectorSalarySummaryDto> GetAccountList(string id)
        {
            //{branchId}/{agentId}/{month}/{operationType}
            string _baseUrl = string.Format(APICallHelper.MigrationGLReconciliationAccount, id);
            var apiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<CollectorSalarySummaryDto>>(_baseUrl);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new CollectorSalarySummaryDto();
            }
        }
    }
}
