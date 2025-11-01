using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.MobileMoneyV2;
using CBS.FrontDesk.Data.Entity.AndriodApp;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.MobileMoneyV2
{

	public class MobileMoneyServices : BaseService
	{
		private readonly ApiCallerHelper _accountingV2ConfigApiHelper;

		public MobileMoneyServices()
		{
			_accountingV2ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
		}

		public async Task<ExecutionMessages> Delete(string id)
		{
			try
			{
				var inResponse = await _accountingV2ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteMobileMoneyV2, id));
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
		public async Task<IEnumerable<MobileMoney>> GetAll()
		{
			try
			{
				if (IsHeadOffice())
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<MobileMoney>>>(APICallHelper.GetAllMobileMoneyV2);
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<MobileMoney>();
				} else
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<MobileMoney>>>(string.Format(APICallHelper.GetAllMobileMoneyV2, GetBranchID()));
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<MobileMoney>();
				}
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<IEnumerable<MobileMoney>> GetAll(string branchId)
		{
			try
			{
				var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<MobileMoney>>>(string.Format(APICallHelper.GetAllMobileMoneyV2, branchId));
				if (couApiResponse.ApiResponseData != null)
				{
					return couApiResponse.ApiResponseData.Data;
				}
				return new List<MobileMoney>();
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<MobileMoney> GetById(string id)
		{
			try
			{
				var cusResponseObject = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<MobileMoney>>(string.Format(APICallHelper.GetMobileMoneyByIdV2, id));
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
		public async Task<ExecutionMessages> Create(MobileMoney model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PostAsync<ServiceResponse<MobileMoney>>(APICallHelper.CreateMobileMoneyV2, model);
				if (response.ApiResponseData != null)
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
		public async Task<ExecutionMessages> Update(MobileMoney model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PostAsync<ServiceResponse<MobileMoney>>(string.Format(APICallHelper.UpdateMobileMoneyV2, model.Id), model);
				if (response.ApiResponseData != null)
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
