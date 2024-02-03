using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper.Helper;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data;

namespace CBS.BusinessService.Accounting
{
 

    public class AccountingEntryRuleService : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }

        public AccountingEntryRuleService()
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
                var objOperationEvent = await GetAccountingRuleEntryById(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_AccountingRuleEntry, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objOperationEvent.AccountingRuleEntryName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objOperationEvent, false, $"{objOperationEvent.AccountingRuleEntryName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<AccountingRuleEntry>> GetAccountingEntryRules()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<AccountingRuleEntry>>>(APICallHelper.GetAllAccountingRuleEntry);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<    AccountingRuleEntry>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<AccountingRuleEntry> GetAccountingRuleEntryById(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<AccountingRuleEntry>>(string.Format(APICallHelper.Get_Update_Delete_AccountingRuleEntry, id));
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
        public async Task<ExecutionMessages> Create(AccountingRuleEntry model)
        {
            try
            {

                // Make an API call to create an individual profile

                model.BankId = this.BankId;
              
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<AccountingRuleEntry>>(APICallHelper.CreateAccountingRuleEntry, model);

                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"AccountingEntry Rule  {model.AccountingRuleEntryName} successfully ", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, (string)model.AccountingRuleEntryName, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Update(AccountingRuleEntry model)
        {
            try
            {

                var OperationEvent = await GetAccountingRuleEntryById(model.Id);
                if (OperationEvent != null)
                {
                    model.BankId = this.BankId;
                    //model.BranchId = this.BranchId;
                    //model.OrganizationId = this.OrganizationId;
                    OperationEvent.DeterminationAccountId =model.DeterminationAccountId;
                    OperationEvent.BalancingAccountId= model.BalancingAccountId;
                    OperationEvent.AccountingRuleEntryName = model.AccountingRuleEntryName;
                    OperationEvent.BookingDirection= model.BookingDirection;
                    OperationEvent.BankId= model.BankId;
                    OperationEvent.OperationEventAttributeId= model.OperationEventAttributeId;
                
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<AccountingRuleEntry>>(string.Format(APICallHelper.Get_Update_Delete_AccountingRuleEntry, model.Id), OperationEvent);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.AccountingRuleEntryName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.AccountingRuleEntryName, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                        return ExecutionMessage;
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

        public Task<List<FrontDesk.Data.Entity.Accounting.AccountingRuleEntryDto>> GetAccountingEntryRulesDto(IEnumerable<FrontDesk.Data.Entity.Accounting.AccountingRuleEntry> accountingRuleEntries, IEnumerable<FrontDesk.Data.Entity.Accounting.OperationEvent> operationEvents, IEnumerable<OperationEventAttribute> operationEventAttributes, IEnumerable<Account> chartOfAccounts)
        {
            var query = from accountingRuleEntry in accountingRuleEntries
                        join debitAccount in chartOfAccounts on accountingRuleEntry.DeterminationAccountId equals debitAccount.Id
                        join creditAccount in chartOfAccounts on accountingRuleEntry.BalancingAccountId equals creditAccount.Id
                        join operationEventAttribute in operationEventAttributes on accountingRuleEntry.OperationEventAttributeId equals operationEventAttribute.Id
            //            join operationEvent in operationEvents on accountingRuleEntry.OperationEventId equals operationEvent.Id
                        select new AccountingRuleEntryDto
                        {
                            Id = accountingRuleEntry.Id,
                            AccountingRuleEntryName = accountingRuleEntry.AccountingRuleEntryName,
                            BookingDirection = accountingRuleEntry.BookingDirection, // Add your logic for BookingDirection
                            OperationEventAttributeName = operationEventAttribute.Name,
                            OperationEventName = operationEventAttribute.Name,
                            DebitAccountLabel = debitAccount.AccountHolder,
                            CreditAccountLabel = creditAccount.AccountHolder
                        };

            List<AccountingRuleEntryDto> result = query.ToList();

            return Task.FromResult(result);
        }

        public async Task<List<AccountingRuleEntry>> GetaccountingEntryRuleService()
        {
            throw new NotImplementedException();
        }
    }

}
