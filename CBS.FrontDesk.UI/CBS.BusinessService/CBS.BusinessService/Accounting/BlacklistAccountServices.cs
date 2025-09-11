using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class BlacklistAccountServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
 
        public BlacklistAccountServices(UserManagementServices userManagementServices = null)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
           
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_BlacklistAccount, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Black listing Account", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, $"Black listing Account", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<BlacklistAccount>> GetBlacklistAccounts()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<BlacklistAccount>>>(APICallHelper.GetAllBlacklistAccount);
                if (couApiResponse.ApiResponseData != null)
                {
                  
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<BlacklistAccount>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<BlacklistAccount> GetBlacklistAccount(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<BlacklistAccount>>(string.Format(APICallHelper.Get_Update_Delete_BlacklistAccount, id));
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
        public async Task<ExecutionMessages> Create(BlacklistedAccount model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<BlacklistAccount>>(APICallHelper.CreateBlacklistAccount, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"BlacklistAccount added and", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "model.Id", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(BlacklistAccount model)
        {
            try
            {

                var BlacklistAccount = await GetBlacklistAccount(model.Id);
                if (BlacklistAccount != null)
                {
                   
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<BlacklistAccount>>(string.Format(APICallHelper.Get_Update_Delete_BlacklistAccount, model.Id), BlacklistAccount);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"BlacklistAccount added and", MessagesResults.Success,
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
