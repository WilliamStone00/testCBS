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

namespace CBS.BusinessService.MembersAccountSettings
{
    public class MemberAccountActivationServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public MemberAccountActivationServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objMemberAccountActivation = await GetMemberAccountActivation(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivation, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"MemberAccountActivation", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objMemberAccountActivation, false, $"MemberAccountActivation", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<MemberAccountActivation>> GetMemberAccountActivations()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<MemberAccountActivation>>>(APICallHelper.GetAllMemberAccountActivation);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<MemberAccountActivation>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<MemberAccountActivation> GetMemberAccountActivation(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<MemberAccountActivation>>(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivation, id));
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
        public async Task<MemberAccountActivation> GetMemberAccountActivationByMemberId(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<MemberAccountActivation>>(string.Format(APICallHelper.GetMemberAccountActivationByCustomerID, id));
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
        public async Task<ExecutionMessages> Create(MemberAccountActivation model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.BankId = GetBankID();
                model.BranchId = GetBankID();
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<MemberAccountActivation>>(APICallHelper.CreateMemberAccountActivation, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"MemberAccountActivation", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "MemberAccountActivation", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(MemberAccountActivation model)
        {
            try
            {

                var MemberAccountActivation = await GetMemberAccountActivation(model.Id);
                if (MemberAccountActivation != null)
                {
                    MemberAccountActivation.MemberAccountActivationPolicyId = model.MemberAccountActivationPolicyId;
                    MemberAccountActivation.ClossingFee = model.ClossingFee;
                    MemberAccountActivation.ReopeningFee = model.ReopeningFee;
                    MemberAccountActivation.RegistrationFee = model.RegistrationFee;
                 
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<MemberAccountActivation>>(string.Format(APICallHelper.Get_Update_Delete_MemberAccountActivation, model.Id), MemberAccountActivation);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"MemberAccountActivation", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "MemberAccountActivation", MessagesResults.Failed,
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
