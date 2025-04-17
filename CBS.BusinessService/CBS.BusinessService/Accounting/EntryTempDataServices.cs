using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Microsoft.AspNet.SignalR.Hosting;
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
        //private List<Currency> _currencies;
        private BranchServices _branchService;
        public ChartOfAccountManagementPositionService _chartofaccountService { get; }
        private AccountingServices _accountServices { get; set; }
        public EntryTempDataServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            _accountServices = new AccountingServices();
            _branchService = new BranchServices();
            _chartofaccountService = new ChartOfAccountManagementPositionService();
        }
        //public List<Currency> Currencies()
        //{
        //    Currency currency = new Currency();
        //    return currency.CreateCurrencies();
        //}
        public async Task<ExecutionMessages> Create(List<EntryTempData> model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<bool>>(APICallHelper.Post_AccountingEntry_Entries, EntryTempData.ConvertToAccountingEntryPayloadCommand( model,GetBranchID()));
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

 

        public async Task<ExecutionMessages> PostAccountingEntry(AutomatedEventEntriesCommand model)
        {
            try
            {

                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<EntryTempData>>(APICallHelper.Post_AccountingEntry_Entries, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, "Your manual entry was successfully registered", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "Your manual entry was successfully registered");
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Your manual entry was failed to be registered", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Your manual entry was failed to be registered");
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
                 
                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<bool>>(APICallHelper.Post_ManaulEntryApproval_Entries, model);
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
        public async Task<ExecutionMessages>  PostAutomatedEntries(ManualJournalEntryRequest model, AccountingEventRule accountingEventRule)
        {
            bool IsAccountBalance = true;
            string message = "";
            List<EntryTempData> EntryTempDatas = new List<EntryTempData>() { };
            var branchAccounts = await _accountServices.GetAllAccountForABranch(this.GetBranchID());
            var chartOfAccountMps= await _chartofaccountService.GetChartOfAccountManagementPositions();
            var collection = ManualJournalEntryRequest.ConvertToAccountModelData(model);
            try
            {//PostAccountingEntry(List<EntryTempData> model)
                foreach (var item in collection)
                {
                    var accountItem = await _accountServices.GetAccountItemForBranch(branchAccounts, chartOfAccountMps.ToList(), item);
                    if (await _accountServices.CheckAccountBalance(accountItem, Convert.ToDecimal(item.Amount), _accountServices.GetOperationType(item)))
                    {
                        EntryTempDatas.Add(new EntryTempData
                        {
                            AccountingEventId = item.AccountingEventId,
                            AccountBalance = accountItem.CurrentBalance.ToString(),
                            AccountId = accountItem.Id,
                            AccountName = item.AccountName,
                            AccountNumber = item.AccountNumber,
                            BookingDirection = item.BookingDirection,
                            Debit = item.BookingDirection.ToUpper() == "DEBIT" ? (item.Amount.ToString()) : "0",
                            Credit = item.BookingDirection.ToUpper() == "CREDIT" ? (item.Amount.ToString()) : "0",
                            Amount = item.Amount.ToString(),
                            Description = item.Description,
                            Id = item.Id,
                            Reference = item.Reference,
                            BranchId= accountItem.AccountOwnerId,
                            ExternalBranchId = accountItem.LiaisonId
                        });
                    }
                    else
                    {
                        if (accountItem!=null)
                        {
                            IsAccountBalance = false;
                            message = message + $"{accountItem.AccountName}-{accountItem.AccountNumberCU}: Account will be left with a negative balance";
                            continue;
                        }
                        else
                        {
                            IsAccountBalance = false;
                            message = $"There is no account  {item.AccountName}-{item.AccountNumber} present in your branch please contact system admin"; 
                            break;
                        }
                    }

                }
                if (IsAccountBalance)
                {
                    //accountingEventRule.IsDoubleValidationNeeded
                 var eventEntreis=   new AutomatedEventEntriesCommand { EntryTempDatas = EntryTempDatas, IsSystem = accountingEventRule. IsDoubleValidationNeeded = accountingEventRule.IsDoubleValidationNeeded, BranchId = await GetBranchByIDAsync(GetBranchID()),
                     ListOfBranchIds= accountingEventRule.ListOfEligibleBranchId, 
                     AccountingEventRuleId= accountingEventRule.IsChainEntry==false?null: accountingEventRule.AccountingEventRuleId,IsInterBranchTransaction= accountingEventRule .IsInterBranchTransaction,ExternalBranchId=null};
                    return await PostAccountingEntry(eventEntreis);
                    
                }
                else
                {
                    GetExecutionMessages(EntryTempDatas, false, message, MessagesResults.Failed,
                           ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, message);
                    return ExecutionMessage;

                }
            }
            catch (Exception ex)
            {

                GetExecutionMessages(ex, false, ex.Message, MessagesResults.Failed,
                          ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, ex.Message);
                return ExecutionMessage;
            }
        }

        private async Task<string> GetBranchByIDAsync(string branchId)
        {
            branchId = branchId.Equals("DEFAULTID") ? (await _branchService.GetBranches()).Where(x => x.BranchCode == "001").FirstOrDefault().Id : branchId;
            return branchId;
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
