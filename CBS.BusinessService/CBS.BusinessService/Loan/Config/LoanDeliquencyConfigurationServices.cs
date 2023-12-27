using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.Loan.Config
{
    public class LoanDeliquencyConfigurationServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanDeliquencyConfigurationServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanDeliquencyConfiguration = await GetLoanDeliquencyConfiguration(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanDeliquencyConfiguration, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objLoanDeliquencyConfiguration.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanDeliquencyConfiguration, false, $"{objLoanDeliquencyConfiguration.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<LoanDeliquencyConfiguration>> GetLoanDeliquencyConfigurations()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanDeliquencyConfiguration>>>(APICallHelper.GetAllLoanDeliquencyConfiguration);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanDeliquencyConfiguration>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanDeliquencyConfiguration> GetLoanDeliquencyConfiguration(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanDeliquencyConfiguration>>(string.Format(APICallHelper.Get_Update_Delete_LoanDeliquencyConfiguration, id));
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
        public async Task<ExecutionMessages> Create(LoanDeliquencyConfiguration model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanDeliquencyConfiguration>>(APICallHelper.CreateLoanDeliquencyConfiguration, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanDeliquencyConfiguration model)
        {
            try
            {

                var LoanDeliquencyConfiguration = await GetLoanDeliquencyConfiguration(model.Id);
                if (LoanDeliquencyConfiguration != null)
                {
                    LoanDeliquencyConfiguration.Name = model.Name;
                    LoanDeliquencyConfiguration.DaysFrom = model.DaysFrom;
                    LoanDeliquencyConfiguration.DaysTo = model.DaysTo;
                    LoanDeliquencyConfiguration.Status = model.Status;
                    LoanDeliquencyConfiguration.ActionToPerform = model.ActionToPerform;
                    LoanDeliquencyConfiguration.SendSMStoClient = model.SendSMStoClient;
                    LoanDeliquencyConfiguration.BankId = model.BankId;
                    LoanDeliquencyConfiguration.BranchId = model.BranchId;
                    LoanDeliquencyConfiguration.BranchId = model.BranchId;
                    LoanDeliquencyConfiguration.OrganizationId = model.OrganizationId;
                    LoanDeliquencyConfiguration.AccountingRuleId = model.AccountingRuleId;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanDeliquencyConfiguration>>(string.Format(APICallHelper.Get_Update_Delete_LoanDeliquencyConfiguration, model.Id), LoanDeliquencyConfiguration);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
