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
    public class WithdrawalLimitServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public WithdrawalLimitServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objWithdrawalLimit = await GetWithdrawalLimit(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_WithdrawalLimits, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objWithdrawalLimit, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<WithdrawalLimit>> GetWithdrawalLimits()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<WithdrawalLimit>>>(APICallHelper.GetAllWithdrawalLimits);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<WithdrawalLimit>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<WithdrawalLimit> GetWithdrawalLimit(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<WithdrawalLimit>>(string.Format(APICallHelper.Get_Update_Delete_WithdrawalLimits, id));
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
        public async Task<ExecutionMessages> Create(WithdrawalLimit model)
        {
            try
            {

                model.BankId=GetBankID();
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<WithdrawalLimit>>(APICallHelper.CreateWithdrawalLimits, model);
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
        public async Task<ExecutionMessages> Update(WithdrawalLimit model)
        {
            try
            {

                var WithdrawalLimit = await GetWithdrawalLimit(model.Id);
                if (WithdrawalLimit != null)
                {
                    WithdrawalLimit.WithdrawalType = model.WithdrawalType;
                    WithdrawalLimit.MinAmount = model.MinAmount;
                    WithdrawalLimit.MaxAmount = model.MaxAmount;
                    WithdrawalLimit.SourceBrachOfficeShare = model.SourceBrachOfficeShare;
                    WithdrawalLimit.DestinationBranchOfficeShare = model.DestinationBranchOfficeShare;
                    WithdrawalLimit.HeadOfficeShare = model.HeadOfficeShare;
                    WithdrawalLimit.FluxAndPTMShare = model.FluxAndPTMShare;
                    WithdrawalLimit.CamCCULShare = model.CamCCULShare;
                    WithdrawalLimit.PhysicalPersonWithdrawalFormFee = model.PhysicalPersonWithdrawalFormFee;
                    WithdrawalLimit.NotificationPeriodInMonths = model.NotificationPeriodInMonths;
                    WithdrawalLimit.MustNotifyOnWithdrawal = model.MustNotifyOnWithdrawal;
                    WithdrawalLimit.MoralPersonWithdrawalFormFee = model.MoralPersonWithdrawalFormFee;

                    WithdrawalLimit.CamCCULShareCMoney = model.CamCCULShareCMoney;
                    WithdrawalLimit.DestinationBranchOfficeShareCMoney = model.DestinationBranchOfficeShareCMoney;
                    WithdrawalLimit.FluxAndPTMShareCMoney = model.FluxAndPTMShareCMoney;
                    WithdrawalLimit.HeadOfficeShareCMoney = model.HeadOfficeShareCMoney;
                    WithdrawalLimit.SourceBrachOfficeShareCMoney = model.FluxAndPTMShare;
                    //WithdrawalLimit.Product = null;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<WithdrawalLimit>>(string.Format(APICallHelper.Get_Update_Delete_WithdrawalLimits, model.Id), WithdrawalLimit);
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
