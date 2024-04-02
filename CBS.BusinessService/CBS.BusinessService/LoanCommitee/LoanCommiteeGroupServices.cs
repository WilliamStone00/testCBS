using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.LoanCommitee
{
    public class LoanCommiteeGroupServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanCommiteeGroupServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanCommiteeGroup = await GetLoanCommiteeGroup(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommiteeValidationCriteria, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objLoanCommiteeGroup.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanCommiteeGroup, false, $"{objLoanCommiteeGroup.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<LoanCommiteeGroup>> GetLoanCommiteeGroups()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanCommiteeGroup>>>(APICallHelper.GetAllLoanCommiteeValidationCriteria);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanCommiteeGroup>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanCommiteeGroup> GetLoanCommiteeGroup(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanCommiteeGroup>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommiteeValidationCriteria, id));
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
        public async Task<ExecutionMessages> Create(LoanCommiteeGroup model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanCommiteeGroup>>(APICallHelper.CreateLoanCommiteeValidationCriteria, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanCommiteeGroup model)
        {
            try
            {

                var LoanCommiteeGroup = await GetLoanCommiteeGroup(model.Id);
                if (LoanCommiteeGroup != null)
                {
                    LoanCommiteeGroup.Name = model.Name;
                    LoanCommiteeGroup.MinimumLoanAmount = model.MinimumLoanAmount;
                    LoanCommiteeGroup.MaximumLoanAmount = model.MaximumLoanAmount;
                    LoanCommiteeGroup.CommiteeLeaderUserId = model.CommiteeLeaderUserId;
                    LoanCommiteeGroup.Description = model.Description;
                    LoanCommiteeGroup.NumberOfMembers = model.NumberOfMembers;
                    LoanCommiteeGroup.NumberToApprovalsToValidationALoan = model.NumberToApprovalsToValidationALoan;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanCommiteeGroup>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommiteeValidationCriteria, model.Id), LoanCommiteeGroup);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
