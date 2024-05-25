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

        public async Task<IEnumerable<MemberRegistrationFeePolicy>> GetMemberAccountActivationPolicys()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<MemberRegistrationFeePolicy>>>(APICallHelper.GetAllMemberAccountActivationPolicy);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<MemberRegistrationFeePolicy>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<MemberRegistrationFeePolicy> GetMemberAccountActivationPolicy(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<MemberRegistrationFeePolicy>>(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivationPolicy, id));
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
        public async Task<ExecutionMessages> Create(MemberRegistrationFeePolicy model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.BankId = GetBankID();
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<MemberRegistrationFeePolicy>>(APICallHelper.CreateMemberAccountActivationPolicy, model);
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
        public async Task<ExecutionMessages> Update(MemberRegistrationFeePolicy model)
        {
            try
            {

                var MemberAccountActivationPolicy = await GetMemberAccountActivationPolicy(model.Id);
                if (MemberAccountActivationPolicy != null)
                {
                    MemberAccountActivationPolicy.PolicyName = model.PolicyName;
                    MemberAccountActivationPolicy.MaximumBuildingContribution = model.MaximumBuildingContribution;
                    MemberAccountActivationPolicy.MinimumBuildingContributionFee = model.MinimumBuildingContributionFee;
                    MemberAccountActivationPolicy.MaximumEntrancenFee = model.MaximumEntrancenFee;
                    MemberAccountActivationPolicy.MinimumEntranceFee = model.MinimumEntranceFee;
                    MemberAccountActivationPolicy.MaximumByeLawsFee = model.MaximumByeLawsFee;
                    MemberAccountActivationPolicy.MaximumLoanPolicyFee = model.MaximumLoanPolicyFee;
                    MemberAccountActivationPolicy.MinimumLoanPolicyFee = model.MinimumLoanPolicyFee;
                    MemberAccountActivationPolicy.EventCodeBuildingContributionFee = model.EventCodeBuildingContributionFee;
                    MemberAccountActivationPolicy.EventCodeByeLawsFee = model.EventCodeByeLawsFee;
                    MemberAccountActivationPolicy.EventCodeEntranceFee = model.EventCodeEntranceFee;
                    MemberAccountActivationPolicy.EventCodeLoanPolicyFee = model.EventCodeLoanPolicyFee;
                    MemberAccountActivationPolicy.IsActive = model.IsActive;
                    MemberAccountActivationPolicy.YearBuildingContributionFee = model.YearBuildingContributionFee;
                    MemberAccountActivationPolicy.LegalForm = model.LegalForm;

                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<MemberRegistrationFeePolicy>>(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivationPolicy, model.Id), MemberAccountActivationPolicy);
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
