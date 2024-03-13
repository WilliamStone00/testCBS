using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Config
{
    public class AlertProfileServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public AlertProfileServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objAlertProfile = await GetAlertProfile(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_AlertProfile, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objAlertProfile.name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objAlertProfile, false, $"{objAlertProfile.name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<AlertProfile>> GetAlertProfiles()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<AlertProfile>>>(APICallHelper.GetAllAlertProfile);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<AlertProfile>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<AlertProfile> GetAlertProfile(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<AlertProfile>>(string.Format(APICallHelper.Get_Update_Delete_AlertProfile, id));
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
        public async Task<ExecutionMessages> Create(AlertProfile model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.branchId = GetBranchID();
                model.bankId = GetBankID();
                model.organizationId = GetOrganizationID();
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<AlertProfile>>(APICallHelper.CreateAlertProfile, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(AlertProfile model)
        {
            try
            {

                var AlertProfile = await GetAlertProfile(model.id);
                if (AlertProfile != null)
                {
                    AlertProfile.name = model.name;
                    AlertProfile.msisdn = model.msisdn;
                    AlertProfile.sendSMS = model.sendSMS;
                    AlertProfile.isSupperAdmin = model.isSupperAdmin;
                    AlertProfile.language = model.language;
                    AlertProfile.activeStatus = model.activeStatus;
                    AlertProfile.serviceId = model.serviceId;
                    AlertProfile.bankId = model.bankId;
                    AlertProfile.branchId = model.branchId;
                    AlertProfile.organizationId = model.organizationId;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<AlertProfile>>(string.Format(APICallHelper.Get_Update_Delete_AlertProfile, model.id), AlertProfile);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.name, MessagesResults.Failed,
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
