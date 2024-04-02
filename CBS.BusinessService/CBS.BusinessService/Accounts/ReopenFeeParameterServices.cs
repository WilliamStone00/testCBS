using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public class ReopenFeeParameterServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public ReopenFeeParameterServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objReopenFeeParameter = await GetReopenFeeParameter(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_ReopenFeeParameter, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"ReopenFeeParameter", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objReopenFeeParameter, false, $"ReopenFeeParameter", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<ReopenFeeParameter>> GetReopenFeeParameters()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<ReopenFeeParameter>>>(APICallHelper.GetAllReopenFeeParameter);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<ReopenFeeParameter>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ReopenFeeParameter> GetReopenFeeParameter(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<ReopenFeeParameter>>(string.Format(APICallHelper.Get_Update_Delete_ReopenFeeParameter, id));
                if (cusResponseObject.IsSuccess)
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
        public async Task<ExecutionMessages> Create(ReopenFeeParameter model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.bankId = GetBankID();
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<ReopenFeeParameter>>(APICallHelper.CreateReopenFeeParameter, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"ReopenFeeParameter", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "ReopenFeeParameter", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(ReopenFeeParameter model)
        {
            try
            {

                var ReopenFeeParameter = await GetReopenFeeParameter(model.id);
                if (ReopenFeeParameter != null)
                {
                    ReopenFeeParameter.reopenFeeFlat = model.reopenFeeFlat;
                    ReopenFeeParameter.productId = model.productId;
                    ReopenFeeParameter.reopenFeeRate = model.reopenFeeRate;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<ReopenFeeParameter>>(string.Format(APICallHelper.Get_Update_Delete_ReopenFeeParameter, model.id), ReopenFeeParameter);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"ReopenFeeParameter", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "ReopenFeeParameter", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
