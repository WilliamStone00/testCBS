using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.LoanCommitee
{
    public class LoanCommiteeMemberServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly UserManagementServices _UserManagementServices;
        public LoanCommiteeMemberServices(UserManagementServices userManagementServices = null)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _UserManagementServices = userManagementServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommeteeMember, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Loan commitee member", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, $"Loan commitee member", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<LoanCommiteeMember>> GetLoanCommiteeMembers()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanCommiteeMember>>>(APICallHelper.GetAllLoanCommeteeMember);
                if (couApiResponse.ApiResponseData!=null)
                {
                    var users=await _UserManagementServices.GetUsers();

                    var data = (from a in couApiResponse.ApiResponseData.Data
                               join b in users on
                    a.UserId equals b.id.ToString()
                               select new LoanCommiteeMember
                               {
                                   Id = a.Id,
                                   User = b,
                                   LoanCommiteeGroupId = a.LoanCommiteeGroupId,
                                   LoanCommiteeGroup = a.LoanCommiteeGroup,
                                   UserId = a.UserId,
                                   LoanCommiteeValidationHistories = a.LoanCommiteeValidationHistories
                               }).ToList();

                    return data;
                }
                return new List<LoanCommiteeMember>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanCommiteeMember> GetLoanCommiteeMember(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanCommiteeMember>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommeteeMember, id));
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
        public async Task<ExecutionMessages> Create(LoanCommiteeMember model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanCommiteeMember>>(APICallHelper.CreateLoanCommeteeMember, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Member added and", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Id, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanCommiteeMember model)
        {
            try
            {

                var LoanCommiteeMember = await GetLoanCommiteeMember(model.Id);
                if (LoanCommiteeMember != null)
                {
                    LoanCommiteeMember.LoanCommiteeGroupId = model.LoanCommiteeGroupId;
                    LoanCommiteeMember.UserId = model.UserId;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanCommiteeMember>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommeteeMember, model.Id), LoanCommiteeMember);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Member added and", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Id, MessagesResults.Failed,
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
