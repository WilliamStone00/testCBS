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

namespace CBS.BusinessService.MembersAccountSettings.policy
{
    public class MemberAccountActivationPolicyServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public MemberAccountActivationPolicyServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                //var objMemberAccountActivationPolicy = await GetMemberAccountActivationPolicy(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivationPolicy, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"MemberAccountActivationPolicy", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(id, false, $"MemberAccountActivationPolicy", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<MemberAccountActivationPolicy>> GetMemberAccountActivationPolicys()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<MemberAccountActivationPolicy>>>(APICallHelper.GetAllMemberAccountActivationPolicy);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<MemberAccountActivationPolicy>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<MemberAccountActivationPolicy> GetMemberAccountActivationPolicy(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<MemberAccountActivationPolicy>>(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivationPolicy, id));
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
        public async Task<ExecutionMessages> Create(MemberAccountActivationPolicy model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.BankId = GetBankID();
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<MemberAccountActivationPolicy>>(APICallHelper.CreateMemberAccountActivationPolicy, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"MemberAccountActivationPolicy", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "MemberAccountActivationPolicy", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(MemberAccountActivationPolicy model)
        {
            try
            {

                var MemberAccountActivationPolicy = await GetMemberAccountActivationPolicy(model.Id);
                if (MemberAccountActivationPolicy != null)
                {
                    MemberAccountActivationPolicy.PolicyName = model.PolicyName;
                    MemberAccountActivationPolicy.MaximumAccountClossingFee = model.MaximumAccountClossingFee;
                    MemberAccountActivationPolicy.MinimumAccountClossingFee = model.MinimumAccountClossingFee;
                    MemberAccountActivationPolicy.MaximumReopeningFee = model.MaximumReopeningFee;
                    MemberAccountActivationPolicy.MinimumReopeningFee = model.MinimumReopeningFee;
                    MemberAccountActivationPolicy.MaximumRegistrationFee = model.MaximumRegistrationFee;
                    MemberAccountActivationPolicy.IsActive = model.IsActive;
                    MemberAccountActivationPolicy.MinimumRegistrationFee = model.MinimumRegistrationFee;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<MemberAccountActivationPolicy>>(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivationPolicy, model.Id), MemberAccountActivationPolicy);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"MemberAccountActivationPolicy", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "MemberAccountActivationPolicy", MessagesResults.Failed,
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
