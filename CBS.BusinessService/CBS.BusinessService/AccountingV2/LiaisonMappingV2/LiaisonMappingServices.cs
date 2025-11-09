using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.LiaisonMappingV2
{
	public class LiaisonMappingServices : BaseService
	{
		private readonly ApiCallerHelper _accountingV2ConfigApiHelper;

		public LiaisonMappingServices()
		{
			_accountingV2ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
		}

		public async Task<ExecutionMessages> Delete(string id)
		{
			try
			{
				var inResponse = await _accountingV2ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteLiaisonMapping, id));
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
		public async Task<IEnumerable<LiaisonMapping>> GetAll()
		{
			try
			{
				if (IsHeadOffice())
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<LiaisonMapping>>>(APICallHelper.GetAllLiaisonMapping);
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<LiaisonMapping>();
				}
				else
				{
					var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<LiaisonMapping>>>(string.Format(APICallHelper.GetAllLiaisonMapping, GetBranchID()));
					if (couApiResponse.ApiResponseData != null)
					{
						return couApiResponse.ApiResponseData.Data;
					}
					return new List<LiaisonMapping>();
				}
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<IEnumerable<LiaisonMapping>> GetAll(string branchId)
		{
			try
			{
				var couApiResponse = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<List<LiaisonMapping>>>(string.Format(APICallHelper.GetAllLiaisonMapping, branchId));
				if (couApiResponse.ApiResponseData != null)
				{
					return couApiResponse.ApiResponseData.Data;
				}
				return new List<LiaisonMapping>();
			}
			catch (Exception ex)
			{
				// Log and handle exception
				throw;
			}
		}

		public async Task<LiaisonMapping> GetById(string id)
		{
			try
			{
				var cusResponseObject = await _accountingV2ConfigApiHelper.GetAsync<ResponseObject<LiaisonMapping>>(string.Format(APICallHelper.GetLiaisonMappingById, id));
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
		public async Task<ExecutionMessages> Create(LiaisonMapping model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PostAsync<ServiceResponse<LiaisonMapping>>(APICallHelper.CreateLiaisonMapping, model);
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
		public async Task<ExecutionMessages> Update(LiaisonMapping model)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PutAsync<ServiceResponse<LiaisonMapping>>(string.Format(APICallHelper.UpdateLiaisonMapping, model.Id), model);
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


		public async Task<CustomDataTable> GetLiaisonMappingDataTable(LiaisonMappingQuery query)
		{
			try
			{
				var response = await _accountingV2ConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
					APICallHelper.LiaisonMappingDatatable, query);

				// ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
				if (!response.IsSuccess)
				{
					throw new Exception($"API call failed: {response.Message}");
				}

				if (response.ApiResponseData == null)
				{
					throw new Exception("API returned null data");
				}

				return response.ApiResponseData.Data;
			}
			catch (Exception ex)
			{
				// Log the original exception
				System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

				// Re-throw to trigger fallback
				throw new Exception($"Liaison Mapping service unavailable: {ex.Message}", ex);
			}
		}


	}
}
