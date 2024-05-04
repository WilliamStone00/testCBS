using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
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
    public class DepositLimitServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public DepositLimitServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objDepositLimit = await GetDepositLimit(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_DepositLimit, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Depositlimit", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objDepositLimit, false, $"Depositlimit", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<DepositLimit>> GetDepositLimits()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<DepositLimit>>>(APICallHelper.GetAllDepositLimits);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<DepositLimit>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<DepositLimit> GetDepositLimit(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<DepositLimit>>(string.Format(APICallHelper.Get_Update_Delete_DepositLimit, id));
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
        public async Task<ExecutionMessages> Create(DepositLimit model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.bankId = GetBankID();
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<DepositLimit>>(APICallHelper.CreateDepositLimit, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Depositlimit", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Depositlimit", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(DepositLimit model)
        {
            try
            {

                var DepositLimit = await GetDepositLimit(model.id);
                if (DepositLimit != null)
                {
                    DepositLimit.depositType = model.depositType;
                    DepositLimit.minAmount = model.minAmount;
                    DepositLimit.maxAmount = model.maxAmount;
                    DepositLimit.depositFeeFlat = model.depositFeeFlat;
                    DepositLimit.depositFeeRate = model.depositFeeRate;
                    DepositLimit.productId = model.productId;
                    DepositLimit.SourceBrachOfficeShare = model.SourceBrachOfficeShare;
                    DepositLimit.DestinationBranchOfficeShare = model.DestinationBranchOfficeShare;
                    DepositLimit.HeadOfficeShare = model.HeadOfficeShare;

                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<DepositLimit>>(string.Format(APICallHelper.Get_Update_Delete_DepositLimit, model.id), DepositLimit);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Depositlimit", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "Depositlimit", MessagesResults.Failed,
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
