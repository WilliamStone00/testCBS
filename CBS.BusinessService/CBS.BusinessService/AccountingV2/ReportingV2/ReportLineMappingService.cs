using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.ReportingV2
{
	public class ReportLineMappingService : BaseService
	{
		private readonly ApiCallerHelper _accountingV2ConfigApiHelper;

		public ReportLineMappingService()
		{
			_accountingV2ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
		}

		public async Task<ExecutionMessages> Delete(string id)
		{
			try
			{
				var inResponse = await _accountingV2ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteReportLineMapping, id));
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
		public async Task<IEnumerable<ReportLineMapping>> GetAll()
		{
			try
			{
				if (IsHeadOffice())
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportLineMapping>>>(APICallHelper.GetAllReportLineMapping);
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<ReportLineMapping>();
				}
				else
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportLineMapping>>>(string.Format(APICallHelper.GetAllReportLineMapping, GetBranchID()));
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<ReportLineMapping>();
				}
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<IEnumerable<ReportLineMapping>> GetAll(string branchId)
		{
			try
			{
				var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportLineMapping>>>(string.Format(APICallHelper.GetAllReportLineMapping, branchId));
				if (couApiResponse.ApiResponseData != null)
				{
					return couApiResponse.ApiResponseData.Data;
				}
				return new List<ReportLineMapping>();
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<ReportLineMapping> GetById(string id)
		{
			try
			{
				var cusResponseObject = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<ReportLineMapping>>(string.Format(APICallHelper.GetReportLineMappingById, id));
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
		public async Task<ExecutionMessages> Create(ReportLineMapping model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CreateReportLineMapping, model);
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
		public async Task<ExecutionMessages> Update(ReportLineMapping model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PutAsync<ServiceResponse<ReportLineMapping>>(string.Format(APICallHelper.UpdateReportLineMapping, model.Id), model);
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
