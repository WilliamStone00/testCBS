using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.API.Helper;
using System.Configuration;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;

namespace CBS.BusinessService
{
    public class AccountTypeServices:BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;
        private List<Currency> _currencies;

        public AccountTypeServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
        public List<Currency> Currencies()
        {
            Currency currency = new Currency();
           return  currency.CreateCurrencies();
        }
        public async Task<ExecutionMessages> Create( AccountType model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<AccountType>>(APICallHelper.Create_AccountTypeForSystem, model.ConvertToDto());
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

        public async Task<ExecutionMessages> CreateCashInfusion(CashInfusion model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<CashInfusion>>(APICallHelper.CashReplenishmentRequest, model);
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

        
        public async Task<ExecutionMessages> Update(AccountType modelx)
        {
            try
            {

                var model = await GetAccountType(modelx.Id);
                if (model != null)
                {
                    
                    var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<AccountType>>(string.Format(APICallHelper.Update_AccountType, model.Id), modelx);
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
                        GetExecutionMessages(model, false, (string)model.name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var model = await this.GetAccountType(id);
                var inResponse = await _accountingApiCallerHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.GetAccountType, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{model.name} {model.operationAccountTypeId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.ApiResponseData.Status);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(inResponse, false, $"{model.name} {model.operationAccountTypeId}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<List<AccountType>> GetAllAccountTypes()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountType>>>(APICallHelper.GetAllAccountType);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<AccountType>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<AccountType> GetAccountType(string id)
        {
            try
            {
                var cusResponseObject = await _accountingApiCallerHelper.GetAsync<ResponseObject<AccountType>>(string.Format(APICallHelper.GetAccountType, id));
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
    }
}
