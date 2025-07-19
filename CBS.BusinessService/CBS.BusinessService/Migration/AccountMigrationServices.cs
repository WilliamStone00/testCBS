using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Data.Entity.DailyCollectorManagement;
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
        public async Task<AccountInitDto> MigrationGLReconciliation(InitInfoDto model)
        {
 
            var apiResponse = await _savingConfigApiHelper.PostAsync<ResponseObject<AccountInitDto>>(APICallHelper.MigrationGLReconciliationInit, model);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new AccountInitDto();
            }
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
