using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class EntryTempDataServices : BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;
        private List<Currency> _currencies;

        public EntryTempDataServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
        public List<Currency> Currencies()
        {
            Currency currency = new Currency();
            return currency.CreateCurrencies();
        }
        public async Task<ExecutionMessages> Create(List<EntryTempData> model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<bool>>(APICallHelper.Post_AccountingEntry_Entries, EntryTempData.ConvertToAccountingEntryPayloadCommand( model));
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
                    GetExecutionMessages(model, false, response.Message, MessagesResults.Failed,
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
        //   
        public async Task<ExecutionMessages> PostAccountingEntry(List<EntryTempData> model)
        {
            try
            {

                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<EntryTempData>>(APICallHelper.Post_AccountingEntry_Entries, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, response.Message, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, response.Message, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> PostAutomatedJournalEntry(AutomatedEventEntryCommand model,bool hasError=false)
        {
            try
            {
                if (hasError)
                {
                    return GetExecutionMessages(model, false, $"Not all the account are present in {this.GetBankName()}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"Not all the account are present in {this.GetBankName()}");
                }
                if (!CheckIfDoubleEntryIsRespected(model.Entries))
                {
                    return GetExecutionMessages(model, false, $"The double entry principle is not respected.", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"The double entry principle is not respected.");
                }
                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<EventEntryResponse>>(APICallHelper.PostAutomatedEventEntryCommand_url, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, response.Message, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, response.Message, MessagesResults.Failed,
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

        private bool CheckIfDoubleEntryIsRespected(List<AutomatedEventEntry> entries)
        {
            return entries.Where(x => x.BookingDirection.ToLower().Equals("debit")).Sum(x => x.Amount) == entries.Where(x => x.BookingDirection.ToLower().Equals("credit")).Sum(x => x.Amount);
        }

        public async Task<ExecutionMessages> ApproveAccountingEntry(EntryApproval model)
        {
            try
            {
                var models = new
                {
                    Id = model.Id,
                    HasApproved = model.HasApproved,
                    TransactionDate = BaseUtilities.UtcToLocal()
                };
                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<bool>>(APICallHelper.Post_ManaulEntryApproval_Entries, models);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, response.Message, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, response.Message, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Update(EntryTempData modelx)
        {
            try
            {

                var model = await GetAccountrJournalEntry(modelx.Id);
                if (model != null)
                {

                    var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<EntryTempData>>(string.Format(APICallHelper.Url_Get_Update_delete_EntryTempData, model.Id), modelx);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.AccountName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.AccountName, MessagesResults.Failed,
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
                var model = await this.GetAccountrJournalEntry(id);
                var inResponse = await _accountingApiCallerHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Url_Get_Update_delete_EntryTempData, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, inResponse.Message, MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(inResponse, false, inResponse.Message, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<List<EntryTempData>> GetAllEntriesForJournalEntryReference(string Id)
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<EntryTempData>>>(string.Format(APICallHelper.Url_Get_RefereceId, Id));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<EntryTempData>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<PostedEntry> GetPostedEntryReference(string Id)
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<PostedEntry>>(string.Format(APICallHelper.Url_Get_RefereceId, Id));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new PostedEntry();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<EntryTempData> GetAccountrJournalEntry(string id)
        {
            try
            {
                var cusResponseObject = await _accountingApiCallerHelper.GetAsync<ResponseObject<EntryTempData>>(string.Format(APICallHelper.Url_Get_Update_delete_EntryTempData, id));
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

        public async Task<List<PostedEntry>> GetManualEntriesAsync()
        {
            try
            {
                var cusResponseObject = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<PostedEntry>>>(APICallHelper.Url_Get_AllPostedEntries);
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
