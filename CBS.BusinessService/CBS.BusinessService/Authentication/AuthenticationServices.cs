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
                    if (response.ApiResponseData.Data.BranchID!=null)
                    {
                        HttpContext.Current.Session["Token"] = response.ApiResponseData.Data.bearerToken;
                        var branchApiResponse = await _BankServer.GetAsync<ResponseObject<Branch>>(string.Format(APICallHelper.Get_Update_Delete_Branch,response.ApiResponseData.Data.BranchID));
                        
                        if (branchApiResponse.ApiResponseData!=null)
                        {
                            if (branchApiResponse.ApiResponseData.Data != null)
                            {
                                var bank = branchApiResponse.ApiResponseData.Data.Bank;
                                var branch = branchApiResponse.ApiResponseData.Data;
                                response.ApiResponseData.Data.Bank = bank;
                                response.ApiResponseData.Data.BankID = bank.Id;
                                response.ApiResponseData.Data.Branch = branch;
                            }
                        }
                    }
                    response.ApiResponseData.Data.password = request.Password;
                    GetExecutionMessages(response.ApiResponseData.Data, true, request.UserName, MessagesResults.Success,
                        ExecutionProcessOption.LoginSuccessful, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Data.refreshToken);
                    return ExecutionMessage;

                }

                errormessage = response.Message;
            }
            catch (Exception ex)
            {

                GetExecutionMessages(null,false, request.UserName, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);

            }
            GetExecutionMessages(null, false, request.UserName, MessagesResults.Failed,
                ExecutionProcessOption.InvalidUserNameOrPassword, SystemMessageStatus.Failed.ToString(), null,
                errormessage);
            return ExecutionMessage;
        }


    }
}
