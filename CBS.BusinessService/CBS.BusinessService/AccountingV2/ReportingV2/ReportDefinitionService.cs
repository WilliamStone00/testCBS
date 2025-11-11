using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportDefinition;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.ReportingV2
{
	public class ReportDefinitionService : BaseService
	{
		private readonly ApiCallerHelper _accountingV2ConfigApiHelper;

		public ReportDefinitionService()
		{
			_accountingV2ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
		}

		public async Task<ExecutionMessages> Delete(string id)
		{
			try
			{
				var inResponse = await _accountingV2ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteReportDefinition, id));
				if (inResponse.ApiResponseData != null)
				{

					GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
						ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
					return ExecutionMessage;

				}
				else
				{
					// Handle failure scenario
					GetExecutionMessages(null, false, null, MessagesResults.Failed,
						ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
				}
			}
			catch (Exception ex)
			{
				// Log and handle exception
			}
			return ExecutionMessage;
		}
		public async Task<IEnumerable<ReportDefinition>> GetAll()
		{
			try
			{
				if (IsHeadOffice())
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportDefinition>>>(APICallHelper.GetAllReportDefinition);
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<ReportDefinition>();
				}
				else
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportDefinition>>>(string.Format(APICallHelper.GetAllReportDefinition, GetBranchID()));
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<ReportDefinition>();
				}
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<IEnumerable<ReportDefinition>> GetAll(string branchId)
		{
			try
			{
				var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportDefinition>>>(string.Format(APICallHelper.GetAllReportDefinition, branchId));
				if (couApiResponse.ApiResponseData != null)
				{
					return couApiResponse.ApiResponseData.Data;
				}
				return new List<ReportDefinition>();
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<ReportDefinition> GetById(string id)
		{
			try
			{
				var cusResponseObject = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<ReportDefinition>>(string.Format(APICallHelper.GetReportDefinitionById, id));
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
		public async Task<ExecutionMessages> Create(ReportDefinition model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CreateReportDefinition, model);
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
		public async Task<ExecutionMessages> Update(ReportDefinition model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PutAsync<ServiceResponse<ReportDefinition>>(string.Format(APICallHelper.UpdateReportDefinition, model.Id), model);
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
					GetExecutionMessages(null, false, "", MessagesResults.Failed,
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
