using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportSection;
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
	public class ReportSectionService : BaseService
	{
		private readonly ApiCallerHelper _accountingV2ConfigApiHelper;

		public ReportSectionService()
		{
			_accountingV2ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
		}

		public async Task<ExecutionMessages> Delete(string id)
		{
			try
			{
				var inResponse = await _accountingV2ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteReportSection, id));
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
		public async Task<IEnumerable<ReportSection>> GetAll()
		{
			try
			{
				if (IsHeadOffice())
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportSection>>>(APICallHelper.GetAllReportSection);
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<ReportSection>();
				}
				else
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportSection>>>(string.Format(APICallHelper.GetAllReportSection, GetBranchID()));
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<ReportSection>();
				}
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<IEnumerable<ReportSection>> GetAll(string branchId)
		{
			try
			{
				var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<ReportSection>>>(string.Format(APICallHelper.GetAllReportSection, branchId));
				if (couApiResponse.ApiResponseData != null)
				{
					return couApiResponse.ApiResponseData.Data;
				}
				return new List<ReportSection>();
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<ReportSection> GetById(string id)
		{
			try
			{
				var cusResponseObject = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<ReportSection>>(string.Format(APICallHelper.GetReportSectionById, id));
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
		public async Task<ExecutionMessages> Create(ReportSection model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CreateReportSection, model);
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
		public async Task<ExecutionMessages> Update(ReportSection model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PutAsync<ServiceResponse<ReportSection>>(string.Format(APICallHelper.UpdateReportSection, model.Id), model);
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
