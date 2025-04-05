using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Session
{
    public class LocalSession: BaseService
    {
        private readonly ApiCallerHelper _identityServerBaseUrl;
        public LocalSession()
        {
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());

        }
        //public UserSessionDto GetUserCurrentsession()
        //{
        //    try
        //    {
        //        string sessionCode = GetSessionCode();
        //        string username = GetUserNAme();

        //        // Construct query parameters
        //        var queryParams = $"?SessionCode={HttpUtility.UrlEncode(sessionCode)}&Username={HttpUtility.UrlEncode(username)}";

        //        // Final URL to call (assumes APICallHelper returns the relative path)
        //        var fullUrl = $"{APICallHelper.GetUserSessionByUserNameAndCode}{queryParams}";

        //        // Use the configured GET method on _identityServerBaseUrl
        //        var response =  _identityServerBaseUrl.GetAllowAnonymous<ResponseObject<UserSessionDto>>(fullUrl);

        //        return response?.ApiResponseData?.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        public UserSessionDto GetUserCurrentsession(string sessionCode, string username)
        {
            try
            {
                // Construct query parameters
                var queryParams = $"?SessionCode={HttpUtility.UrlEncode(sessionCode)}&Username={HttpUtility.UrlEncode(username)}";

                // Final URL to call (assumes APICallHelper returns the relative path)
                var fullUrl = $"{APICallHelper.GetUserSessionByUserNameAndCode}{queryParams}";

                // Use the configured GET method on _identityServerBaseUrl
                var response = _identityServerBaseUrl.GetAllowAnonymous<ResponseObject<UserSessionDto>>(fullUrl);

                return response?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ExecutionMessages> InvalidateAllActivetUsers(SessionAuth sessionAuth)
        {
            try
            {
                var response = await _identityServerBaseUrl.PostAsync<ServiceResponse<bool>>(
                    APICallHelper.InvalidateAllActivetUsers, sessionAuth);

                if (response.IsSuccess)
                {
                    return GetExecutionMessages(
                        response, true, "Multiple seccion closeup", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(), null, response.Message
                    );
                }

                return GetExecutionMessages(
                    sessionAuth, false, "Multiple seccion closeup", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(), null, response.Message
                );
            }
            catch (Exception ex)
            {
                return GetExecutionMessages(
                    null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex
                );
            }
        }
        public async Task<ExecutionMessages> LogoutuserSessions(AddLogoutSessionCommand logoutUsessions)
        {
            try
            {
                var response = await _identityServerBaseUrl.PostAsync<ServiceResponse<bool>>(
                    APICallHelper.LogoutuserSessions, logoutUsessions);

                if (response.IsSuccess)
                {
                    return GetExecutionMessages(
                        response, true, "Multiple seccion closeup", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(), null, response.Message
                    );
                }

                return GetExecutionMessages(
                    logoutUsessions, false, "Multiple seccion closeup", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(), null, response.Message
                );
            }
            catch (Exception ex)
            {
                return GetExecutionMessages(
                    null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex
                );
            }
        }
        public async Task<ExecutionMessages> GenerateANewSessionRecoveryCode(string userId)
        {
            try
            {
                var response = await _identityServerBaseUrl.PostAsync<ServiceResponse<string>>(
                    APICallHelper.GenerateRecoveryCode, new GenerateNewRecoveryCodeCommand { UserId=userId }); // Ensure the URL constant is correctly named

                if (response.IsSuccess)
                {
                    return GetExecutionMessages(
                        response, true, "Session recovery code generated successfully", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(), null, response.Message
                    );
                }

                return GetExecutionMessages(
                    userId, false, "Failed to generate session recovery code", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(), null, response.Message
                );
            }
            catch (Exception ex)
            {
                return GetExecutionMessages(
                    null, false, "An error occurred while generating the session recovery code.",
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex
                );
            }
        }

    }

}
