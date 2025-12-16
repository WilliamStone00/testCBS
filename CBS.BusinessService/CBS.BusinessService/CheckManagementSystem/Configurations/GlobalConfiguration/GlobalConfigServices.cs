using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.GlobalConfig;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Configurations.GlobalConfiguration
{
	public class GlobalConfigServices : BaseService
	{
		private readonly ApiCallerHelper _apiCallerHelper;

		public GlobalConfigServices()
		{
			_apiCallerHelper =
				new ApiCallerHelper(ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"].ToString());
		}

		public async Task<IEnumerable<GlobalConfig>> GetAll()
		{
			try
			{
				var response = await _apiCallerHelper
					.GetAsync<ResponseObject<List<GlobalConfig>>>(APICallHelper.GetAllGlobalConfig);

				if (response.ApiResponseData != null)
				{
					return response.ApiResponseData.Data;
				}

				return new List<GlobalConfig>();
			}
			catch (Exception)
			{
				throw;
			}
		}


		public async Task<GlobalConfig> GetById(string id)
		{
			try
			{
				var response = await _apiCallerHelper
					.GetAsync<ResponseObject<GlobalConfig>>(
						string.Format(APICallHelper.GetGlobalConfigById, id));

				if (response.ApiResponseData != null)
				{
					return response.ApiResponseData.Data;
				}

				return null;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task<ExecutionMessages> Create(GlobalConfig model)
		{
			try
			{
				var response = await _apiCallerHelper
					.PostAsync<ServiceResponse<bool>>(APICallHelper.CreateGlobalConfig, model);

				if (response.IsSuccess)
				{
					GetExecutionMessages(response, true, null, MessagesResults.Success,
						ExecutionProcessOption.DefaultSuccessdMessages,
						SystemMessageStatus.Success.ToString(), null, response.Message);
				}
				else
				{
					GetExecutionMessages(model, false, null, MessagesResults.Failed,
						ExecutionProcessOption.DefaultFailedMessages,
						SystemMessageStatus.Failed.ToString(), null, response.Message);
				}
			}
			catch (Exception ex)
			{
				GetExecutionMessages(null, false, null, MessagesResults.Error,
					ExecutionProcessOption.TryCatch,
					SystemMessageStatus.Failed.ToString(), ex);
			}

			return ExecutionMessage;
		}


		public async Task<ExecutionMessages> Update(GlobalConfig model)
		{
			try
			{
				var response = await _apiCallerHelper
					.PutAsync<ServiceResponse<GlobalConfig>>(
						string.Format(APICallHelper.UpdateGlobalConfig, model.Id), model);

				if (response.IsSuccess)
				{
					GetExecutionMessages(response, true, null, MessagesResults.Success,
						ExecutionProcessOption.DefaultSuccessdMessages,
						SystemMessageStatus.Success.ToString(), null, response.Message);
				}
				else
				{
					GetExecutionMessages(null, false, null, MessagesResults.Failed,
						ExecutionProcessOption.DefaultFailedMessages,
						SystemMessageStatus.Failed.ToString(), null, response.Message);
				}
			}
			catch (Exception ex)
			{
				GetExecutionMessages(null, false, null, MessagesResults.Error,
					ExecutionProcessOption.TryCatch,
					SystemMessageStatus.Failed.ToString(), ex);
			}

			return ExecutionMessage;
		}


		public async Task<ExecutionMessages> Delete(string id)
		{
			try
			{
				var response = await _apiCallerHelper
					.DeleteAsync<ServiceResponse<bool>>(
						string.Format(APICallHelper.DeleteGlobalConfig, id));

				if (response.ApiResponseData != null)
				{
					GetExecutionMessages(response, true, null, MessagesResults.Success,
						ExecutionProcessOption.DefaultSuccessdMessages,
						SystemMessageStatus.Success.ToString(), null, response.Message);
				}
				else
				{
					GetExecutionMessages(null, false, null, MessagesResults.Failed,
						ExecutionProcessOption.DefaultFailedMessages,
						SystemMessageStatus.Failed.ToString(), null, response.Message);
				}
			}
			catch (Exception ex)
			{
				GetExecutionMessages(null, false, null, MessagesResults.Error,
					ExecutionProcessOption.TryCatch,
					SystemMessageStatus.Failed.ToString(), ex);
			}

			return ExecutionMessage;
		}

	}
}
