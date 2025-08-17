using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace CBS.BusinessService.Session
{
    public class LocalSession: BaseService
    {
        private readonly ApiCallerHelper _identityServerBaseUrl;
        public LocalSession()
        {
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());

        }
        public UserSessionDto GetCurrentUserSession(string sessionCode, string username)
        {
            string cacheKey = "UserSessionCache_" + sessionCode;

            // 🧠 Check cache first
            if (HttpRuntime.Cache[cacheKey] is UserSessionDto cachedSession)
            {
                return cachedSession;
            }

            // ❗ Session is missing or invalid — fallback to database
            var userSessionFromDb = GetUserCurrentsession(sessionCode, username);
            if (userSessionFromDb != null)
            {
                // Store back into cache for future use
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    userSessionFromDb,
                    null,
                    DateTime.Now.AddMinutes(10),
                    Cache.NoSlidingExpiration
                );
            }

            return userSessionFromDb;
        }

        public UserSessionDto GetUserCurrentsession(string sessionCode, string username)
        
        {
            try
            {
                // Construct query parameters
                var queryParams = $"?SessionCode={HttpUtility.UrlEncode(sessionCode)}&Username={HttpUtility.UrlEncode(username)}";

                // Final URL to call (assumes APICallHelper returns the relative path)
                var fullUrl = $"{APICallHelper.GetUserSessionByUserNameAndCode}{queryParams}";

                // Perform GET request to identity server
                var response = _identityServerBaseUrl.GetAllowAnonymous<ResponseObject<UserSessionDto>>(fullUrl);

                // Handle null response or missing data gracefully
                if (response == null)
                {
                    // Optionally log the error
                    // _logger?.LogWarning("❗Null response from identity server while fetching session.");
                    return null;
                }

                if (!response.IsSuccess || response.ApiResponseData == null || response.ApiResponseData.Data == null)
                {
                    // Optionally log the reason for failure
                    // _logger?.LogWarning($"❗Failed to get session: Success={response.IsSuccess}, Error={response.ApiResponseData?.Message}");
                    return null;
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Optional: Log the exception
                // _logger?.LogError(ex, "❌ Exception occurred while retrieving user session.");
                return null;
            }
        }
        //GetCurrentIdletimeByBranch
        public async Task<TimeSpan> GetCurrentIdletimeByBranch()
        {
            try
            {
                var branchId = GetBranchID(); // You may want to log this or validate
                var response = await _identityServerBaseUrl.GetAsync<ServiceResponse<TimeSpan>>(string.Format(APICallHelper.GetCurrentIdletimeByBranch, branchId));
                if (response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new TimeSpan();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
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

        public async Task<User> GetUser(string userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var user = await ApiCallerHelper.GetAsync<ResponseObject<User>>(string.Format(APICallHelper.GetUserByID, ConvertStringToGuid(userid)));
                if (user.ApiResponseData != null)
                {
                    return user.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateUserProfile(User model)
        {
            try
            {
                var apiUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString();
                var apiCallerHelper = new ApiCallerHelper(apiUrl);

                var response = await apiCallerHelper.PutAsync<ResponseObject<User>>(
                    string.Format(APICallHelper.UpdateUser, model.id), model
                );

                if (response != null && response.IsSuccess)
                {
                    GetExecutionMessages(response, true, response.ApiResponseData?.Data?.firstName,
                        MessagesResults.Success, ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(), null, response.Message);

                    return true;
                }

                // Handle failure case
                GetExecutionMessages(model, false, model?.firstName,
                    MessagesResults.Failed, ExecutionProcessOption.DefaultFailedMessages,
                    SystemMessageStatus.Failed.ToString(), null, response?.Message ?? "Update failed");

                return false;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null,
                    MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);

                return false;
            }
        }

    }

}
