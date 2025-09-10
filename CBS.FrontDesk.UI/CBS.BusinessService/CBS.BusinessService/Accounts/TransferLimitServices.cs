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
    public class TransferLimitServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public TransferLimitServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objTransferLimit = await GetTransferLimit(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_TransferLimits, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Transfer Limit", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objTransferLimit, false, $"Transfer Limit", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<TransferLimit>> GetTransferLimits()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<TransferLimit>>>(APICallHelper.GetAllTransferLimits);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<TransferLimit>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<TransferLimit> GetTransferLimit(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<TransferLimit>>(string.Format(APICallHelper.Get_Update_Delete_TransferLimits, id));
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
        public async Task<ExecutionMessages> Create(TransferLimit model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.bankId = GetBankID(); 
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<TransferLimit>>(APICallHelper.CreateTransferLimits, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Transfer Limit", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Transfer Limit", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(TransferLimit model)
        {
            try
            {

                var TransferLimit = await GetTransferLimit(model.id);
                if (TransferLimit != null)
                {
                    TransferLimit.transferType = model.transferType;
                    TransferLimit.minAmount = model.minAmount;
                    TransferLimit.maxAmount = model.maxAmount;
                    TransferLimit.transferFeeFlat = model.transferFeeFlat;
                    TransferLimit.transferFeeRate = model.transferFeeRate;
                    TransferLimit.productId = model.productId;
                    TransferLimit.SourceBrachOfficeShare = model.SourceBrachOfficeShare;
                    TransferLimit.DestinationBranchOfficeShare = model.DestinationBranchOfficeShare;
                    TransferLimit.HeadOfficeShare = model.HeadOfficeShare;
                    TransferLimit.CamCCULShare = model.CamCCULShare;
                    TransferLimit.FluxAndPTMShare = model.FluxAndPTMShare;


                    TransferLimit.CamCCULShareCMoney = model.CamCCULShareCMoney;
                    TransferLimit.DestinationBranchOfficeShareCMoney = model.DestinationBranchOfficeShareCMoney;
                    TransferLimit.FluxAndPTMShareCMoney = model.FluxAndPTMShareCMoney;
                    TransferLimit.HeadOfficeShareCMoney = model.HeadOfficeShareCMoney;
                    TransferLimit.SourceBrachOfficeShareCMoney = model.SourceBrachOfficeShareCMoney;

                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<TransferLimit>>(string.Format(APICallHelper.Get_Update_Delete_TransferLimits, model.id), TransferLimit);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Transfer Limit", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "Transfer Limit", MessagesResults.Failed,
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
