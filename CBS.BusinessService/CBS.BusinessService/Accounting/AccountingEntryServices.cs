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
    public class AccountingEntryServices:BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;
        private List<Currency> _currencies;

        public AccountingEntryServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
        public List<Currency> Currencies()
        {
            Currency currency = new Currency();
           return  currency.CreateCurrencies();
        }
        public async Task<ExecutionMessages> Create( ManualAccountingEntry model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.ManualEntriePosting, model.ConvertToManualAccountingEntryDto());
                if (response.ApiResponseData.Data)
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


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<CashInfusion>>(APICallHelper.CashInfusion, model);
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

        public async Task<List<AccountingEntry>> GetAllAccountingEntries()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEntry>>>(APICallHelper.AccountingEntry_Entries);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<AccountingEntry>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw(ex);
            }
        }
    }
}
