using System;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.User;
using CBS.API.Helper;
using CBS.FrontDesk.Helper;
using System.Configuration;
using BusinessServices;
using CBS.FrontDesk.Data.Message;
using System.Reflection;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.UserManagement;
using System.Web;

namespace CBS.FrontDesk.Service
{
    public interface IAuthenticationServices
    {
        Task<ExecutionMessages> AuthenticateUser(AuthRequest request);
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
                    HttpContext.Current.Session["Token"] = userAuth.bearerToken;
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
                ExecutionProcessOption.InvalidUserNameOrPassword, SystemMessageStatus.Failed.ToString(), null,
                errormessage);
            return ExecutionMessage;
        }


    }
}
