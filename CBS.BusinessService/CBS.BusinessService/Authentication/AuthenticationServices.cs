using System;
using System.Threading.Tasks;
using CBS.API.Helper;
using CBS.FrontDesk.Helper;
using System.Configuration;
using BusinessServices;
using CBS.FrontDesk.Data.Message;
using System.Web;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.UserManagement;
using DocumentFormat.OpenXml.EMMA;
using Irony.Parsing;
using System.Web.Security;

namespace CBS.FrontDesk.Service
{
    public interface IAuthenticationServices
    {
        Task<ExecutionMessages> AuthenticateUser(AuthRequest request);
        Task<ExecutionMessages> Logout();
    }

    public class AuthenticationServices : BaseService, IAuthenticationServices
    {

        private readonly ApiCallerHelper _identityServer;
        private readonly ApiCallerHelper _BankServer;
        public AuthenticationServices()
        {
            _identityServer = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            _BankServer = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> AuthenticateUser(AuthRequest request)
        {
            string errormessage = null;
            try
            {
                var response = await _identityServer.PostAsync<ResponseObject<UserDto>>(APICallHelper.Authentication, request);
                if (response.IsSuccess)
                {

                    var userAuth = response.ApiResponseData.Data;




                    response.ApiResponseData.Data.Branch = userAuth.Branch;
                    userAuth.password = request.Password;
                    GetExecutionMessages(userAuth, true, request.UserName, MessagesResults.Success,
                        ExecutionProcessOption.LoginSuccessful, SystemMessageStatus.Success.ToString(), null,
                        userAuth.refreshToken);
                    return ExecutionMessage;

                }

                errormessage = response.Message;
            }
            catch (Exception ex)
            {

                GetExecutionMessages(null, false, request.UserName, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);

            }
            GetExecutionMessages(null, false, request.UserName, MessagesResults.Failed,
                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                errormessage);
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Logout()
        {
            string errormessage = null;
            try
            {
                var logoutSessionCommand = new AddLogoutSessionCommand { UserId = ConvertStringToGuid(GetUserID()) };
                var response = await _identityServer.PostAsync<ResponseObject<bool>>(APICallHelper.SessionLogout, logoutSessionCommand);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, null, MessagesResults.Success,
                        ExecutionProcessOption.LoginSuccessful, SystemMessageStatus.Success.ToString(), null,
                        response.Message);
                    return ExecutionMessage;

                }

                errormessage = response.Message;
            }
            catch (Exception ex)
            {

                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);

            }
            GetExecutionMessages(null, false, null, MessagesResults.Failed,
                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                errormessage);
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> MFAAuthenticateUser(MFAActivation request)
        {
            string errormessage = null;
            try
            {
                var response = await _identityServer.PostAsync<ResponseObject<UserDto>>(APICallHelper.MFAAuthentication, request);
                if (response.IsSuccess)
                {
                    var userAuth = response.ApiResponseData.Data;
                    HttpContext.Current.Session["Token"] = userAuth.bearerToken;
                    HttpContext.Current.Session["BranchObject"] = userAuth.Branch;
                    response.ApiResponseData.Data.Branch = userAuth.Branch;
                    userAuth.password = request.Code;
                    GetExecutionMessages(userAuth, true, userAuth.userName, MessagesResults.Success,
                        ExecutionProcessOption.LoginSuccessful, SystemMessageStatus.Success.ToString(), null,
                        userAuth.refreshToken);
                    return ExecutionMessage;

                }

                errormessage = response.Message;
            }
            catch (Exception ex)
            {

                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);

            }
            GetExecutionMessages(null, false, null, MessagesResults.Failed,
                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                errormessage);
            return ExecutionMessage;
        }

    }
}
