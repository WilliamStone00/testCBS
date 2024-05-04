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
    public class ManagementFeeParameterServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public ManagementFeeParameterServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objManagementFeeParameter = await GetManagementFeeParameter(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_ManagementFeeParameter, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"ManagementFeeParameter", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objManagementFeeParameter, false, $"ManagementFeeParameter", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<ManagementFeeParameter>> GetManagementFeeParameters()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<ManagementFeeParameter>>>(APICallHelper.GetAllManagementFeeParameter);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<ManagementFeeParameter>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ManagementFeeParameter> GetManagementFeeParameter(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<ManagementFeeParameter>>(string.Format(APICallHelper.Get_Update_Delete_ManagementFeeParameter, id));
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
        public async Task<ExecutionMessages> Create(ManagementFeeParameter model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.bankId = GetBankID();
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<ManagementFeeParameter>>(APICallHelper.CreateManagementFeeParameter, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"ManagementFeeParameter", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "ManagementFeeParameter", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(ManagementFeeParameter model)
        {
            try
            {

                var ManagementFeeParameter = await GetManagementFeeParameter(model.id);
                if (ManagementFeeParameter != null)
                {
                    ManagementFeeParameter.managementFeeFrequency = model.managementFeeFrequency;
                    ManagementFeeParameter.productId = model.productId;
                    ManagementFeeParameter.managementFeeFlat = model.managementFeeFlat;
                    ManagementFeeParameter.managementFeeRate = model.managementFeeRate;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<ManagementFeeParameter>>(string.Format(APICallHelper.Get_Update_Delete_ManagementFeeParameter, model.id), ManagementFeeParameter);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"ManagementFeeParameter", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "ManagementFeeParameter", MessagesResults.Failed,
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
