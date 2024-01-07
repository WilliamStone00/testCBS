using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CBS.FrontDesk.Helper.Helper;
using CBS.FrontDesk.Service;

namespace CBS.BusinessService.Accounting
{
    public class AccountingRuleService : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }

        public AccountingRuleService()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            this.BranchId = GetBranchID();
            this.BankId = GetBankID();
            this.OrganizationId = GetOrganizationID();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objOperationEvent = await GetAccountingRuleById(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_AccountingRule, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objOperationEvent.RuleName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objOperationEvent, false, $"{objOperationEvent.RuleName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<FrontDesk.Data.Entity.Accounting.AccountingRule>> GetAccountingRules()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.AccountingRule>>>(APICallHelper.GetAllAccountingRule);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FrontDesk.Data.Entity.Accounting.AccountingRule>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<FrontDesk.Data.Entity.Accounting.AccountingRule> GetAccountingRuleById(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.AccountingRule>>(string.Format(APICallHelper.Get_Update_Delete_AccountingRule, id));
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
        public async Task<ExecutionMessages> Create(FrontDesk.Data.Entity.Accounting.AccountingRule model)
        {
            try
            {

                // Make an API call to create an individual profile

              
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.AccountingRule>>(APICallHelper.CreateAccountingRule, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.RuleName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, (string)model.RuleName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(FrontDesk.Data.Entity.Accounting.AccountingRule model)
        {
            try
            {

                var OperationEvent = await GetAccountingRuleById(model.Id);
                if (OperationEvent != null)
                {
                    OperationEvent.RuleName = model.RuleName;
                    OperationEvent.Id = model.Id;
                
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.AccountingRule>>(string.Format(APICallHelper.Get_Update_Delete_AccountingRule, model.Id), OperationEvent);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.RuleName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.RuleName, MessagesResults.Failed,
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
