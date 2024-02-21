using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class TellerCashReplenishmentServices : BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;
        

        public TellerCashReplenishmentServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }
        public async Task<ExecutionMessages> CreateCashReplenishmentRequest(CashInfusionRequest model)
        {
            var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<DetailsDto>>(APICallHelper.TellerCashReplenishmentRequest, model.GetRequest());
            if (response.IsSuccess)
            {
                // Successful creation
                GetExecutionMessages(response, true, $"Transaction was successfull", MessagesResults.Success,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                return ExecutionMessage;
            }
            else
            {
                // Failed creation
                GetExecutionMessages(model, false, $"Transaction was not successfull", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                return ExecutionMessage;
            }
        }
        public async Task<ExecutionMessages> CreateApprovalRequest(Approval model)
        {
            try
            {

                string url = string.Format(APICallHelper.TellerCashReplenishmentRequest,model.id);
                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<DetailsDto>>(url, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Transaction was successfull", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Transaction was not successfull", MessagesResults.Failed,
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
        public async Task<List<DetailsDto>> GetAllCashReplenimentRequest()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<DetailsDto>>>(APICallHelper.TellerCashReplenishmentRequest);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DetailsDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<DetailsDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
    

        public async Task<DetailsDto> GetCashReplenimentRequest(string Id)
        {
            try
            {
                var url = string.Format(APICallHelper.TellerCashReplenishmentRequestApproval, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<DetailsDto>>(url);
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.requesterUserId);
                    couApiResponse.ApiResponseData.Data.requesterUserId = user.name + "," + user.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new DetailsDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<DisplayData>> GetCashReplenimentRequestId()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<DisplayData>>>(APICallHelper.CurrentOpenOfDayHistory);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DisplayData>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<DisplayData>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<UserList> GetUser(string userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var user = await ApiCallerHelper.GetAsync<ResponseObject<UserList>>(string.Format(APICallHelper.GetUserByID, userid));
                if (user.IsSuccess)
                {
                    user.ApiResponseData.Data.name = $"{user.ApiResponseData.Data.firstName} {user.ApiResponseData.Data.lastName}";
                    user.ApiResponseData.Data.strlastLoginDate = user.ApiResponseData.Data.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                    user.ApiResponseData.Data.status = user.ApiResponseData.Data.isActive ? "Active" : "In-active";
                    user.ApiResponseData.Data.ChangePassword.userName = user.ApiResponseData.Data.userName;
                    user.ApiResponseData.Data.roleID = user.ApiResponseData.Data.userRoles.Select(role => role.roleId).First();
                }
                return user.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
