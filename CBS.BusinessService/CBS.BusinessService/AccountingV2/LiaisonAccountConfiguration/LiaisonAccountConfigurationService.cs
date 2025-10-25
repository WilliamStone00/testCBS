using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonAccountConfiguration;
using CBS.FrontDesk.Data.Entity.Config; // adjust namespace
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.LiaisonAccountConfiguration
{
    public class LiaisonAccountConfigurationService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;

        public LiaisonAccountConfigurationService()
        {
            _apiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
        }

        public async Task<IEnumerable<LiaisonAccountConfigurationDto>> GetLiaisonAccountMappings()
        {
            var response = await _apiHelper.GetAsync<ResponseObject<List<LiaisonAccountConfigurationDto>>>(APICallHelper.GetAllLiaisonMappings);
            return response.IsSuccess ? response.ApiResponseData.Data : new List<LiaisonAccountConfigurationDto>();
        }

        public async Task<LiaisonAccountConfigurationDto> GetLiaisonAccountMapping(string id)
        {
            var response = await _apiHelper.GetAsync<ResponseObject<LiaisonAccountConfigurationDto>>(string.Format(APICallHelper.GetLiaisonMapping, id));
            return response.IsSuccess ? response.ApiResponseData.Data : null;
        }

        public async Task<ExecutionMessages> Create(LiaisonAccountConfigurationDto model)
        {
            var response = await _apiHelper.PostAsync<ServiceResponse<LiaisonAccountConfigurationDto>>(APICallHelper.CreateLiaisonMapping, model);
            GetExecutionMessages(response, response.IsSuccess, null,
                response.IsSuccess ? MessagesResults.Success : MessagesResults.Failed,
                ExecutionProcessOption.DefaultSuccessdMessages,
                response.IsSuccess ? SystemMessageStatus.Success.ToString() : SystemMessageStatus.Failed.ToString(),
                null, response.Message);
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> Update(LiaisonAccountConfigurationDto model)
        {
            var response = await _apiHelper.PutAsync<ServiceResponse<LiaisonAccountConfigurationDto>>(string.Format(APICallHelper.UpdateLiaisonMapping, model.Id), model);
            GetExecutionMessages(response, response.IsSuccess, null,
                response.IsSuccess ? MessagesResults.Success : MessagesResults.Failed,
                ExecutionProcessOption.DefaultSuccessdMessages,
                response.IsSuccess ? SystemMessageStatus.Success.ToString() : SystemMessageStatus.Failed.ToString(),
                null, response.Message);
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            var response = await _apiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteLiaisonMapping, id));
            GetExecutionMessages(response, response.IsSuccess, null,
                response.IsSuccess ? MessagesResults.Success : MessagesResults.Failed,
                ExecutionProcessOption.DefaultSuccessdMessages,
                response.IsSuccess ? SystemMessageStatus.Success.ToString() : SystemMessageStatus.Failed.ToString(),
                null, response.Message);
            return ExecutionMessage;
        }
    }
}
