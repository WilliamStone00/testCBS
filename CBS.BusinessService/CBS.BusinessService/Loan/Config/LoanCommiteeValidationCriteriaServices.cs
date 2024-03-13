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
    public class LoanCommiteeValidationCriteriaServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanCommiteeValidationCriteriaServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanCommiteeValidationCriteria = await GetLoanCommiteeValidationCriteria(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanCommiteeValidationCriteria, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objLoanCommiteeValidationCriteria.loanProductId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanCommiteeValidationCriteria, false, $"{objLoanCommiteeValidationCriteria.loanProductId}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<LoanCommiteeValidationCriteria>> GetLoanCommiteeValidationCriterias()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanCommiteeValidationCriteria>>>(APICallHelper.GetAllLoanCommiteeValidationCriteria);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanCommiteeValidationCriteria>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanCommiteeValidationCriteria> GetLoanCommiteeValidationCriteria(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanCommiteeValidationCriteria>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommiteeValidationCriteria, id));
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
        public async Task<ExecutionMessages> Create(LoanCommiteeValidationCriteria model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanCommiteeValidationCriteria>>(APICallHelper.CreateLoanCommiteeValidationCriteria, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.loanProductId}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.loanProductId, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanCommiteeValidationCriteria model)
        {
            try
            {

                var LoanCommiteeValidationCriteria = await GetLoanCommiteeValidationCriteria(model.id);
                if (LoanCommiteeValidationCriteria != null)
                {
                    LoanCommiteeValidationCriteria.loanProductId = model.loanProductId;
                    LoanCommiteeValidationCriteria.committeeSize = model.committeeSize;
                    LoanCommiteeValidationCriteria.acceptedValueForApproval = model.acceptedValueForApproval;
                    LoanCommiteeValidationCriteria.canBeOverriddenBySupervisor = model.canBeOverriddenBySupervisor;
                    LoanCommiteeValidationCriteria.supervisorId = model.supervisorId;
                    LoanCommiteeValidationCriteria.daysForAlert = model.daysForAlert;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanCommiteeValidationCriteria>>(string.Format(APICallHelper.Get_Update_Delete_LoanCommiteeValidationCriteria, model.id), LoanCommiteeValidationCriteria);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.loanProductId}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.loanProductId, MessagesResults.Failed,
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
