using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace CBS.BusinessService
{
    public class AccountingEntryServices : BaseService, IAccountingEntryServices
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;
        private readonly ApiCallerHelper _TransactionBaseUrl;
        private readonly ApiCallerHelper _IdentityServerBaseUrl;
        private readonly object _logger;

        private AccountingServices _accountServices { get; set; }
        public BranchServices branchServices { get;   set; }

        //private List<Currency> _currencies;
        public AccountingEntryServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            _TransactionBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _IdentityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            branchServices = new BranchServices();
            _accountServices = new AccountingServices();
        }
        public async Task<List<CashRoot>> GetCashReplenimentCurrentOpenOfDayHistoryRequestId()
        {
            try
            {
                var couApiResponse = await _TransactionBaseUrl.GetAsync<ResponseObject<List<CashRoot>>>(APICallHelper.CurrentOpenOfDayHistory);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CashRoot>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<CashRoot>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<ExecutionMessages> CreateManualAccountingEntry(ManualAccountingEntry model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.ManualEntriePosting, model.ConvertToManualAccountingEntryDto());
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
        public async Task<ExecutionMessages> Update(CashInfusion model)
        {
            try
            {

                var OperationEvent = await GetCashReplenimentReferenceRequest(model.Id);
                if (OperationEvent != null)
                {

              
                    model.CurrentOpenOfDayHistoryId = "model.CurrentOpenOfDayHistoryId";
                
                    model.ReferenceNumber = OperationEvent.ReferenceId;
                    model.RequestMessage = "model.CurrentOpenOfDayHistoryId";
                    model.Amount = OperationEvent.AmountRequested;
                    var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<CashInfusion>>(string.Format(APICallHelper.UpdateCashReplenishmentRequest, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.CurrentOpenOfDayHistoryId} Transaction was successfull", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.CurrentOpenOfDayHistoryId} Transaction failed", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> CashReplenishmentRequest(CashInfusion model)
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

        public async Task<ExecutionMessages> RedirectedForBankCashOutRequest(RedirectedForBankCashOut model)
        {
            try
            {
                //if (string.IsNullOrEmpty(model.Message))
                //{
                //    string message = "The Bank Deposit Request message is required opertion failed";
                //    GetExecutionMessages(model, false, message, MessagesResults.Failed,
                //       ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, message);

                //}
                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<RedirectedForBankCashOut>>(APICallHelper.AccountingEntry_BankingOperationRedirectedForBankCashOut, model  );
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
        public async Task<ExecutionMessages> DepositNotificationRequest(DepositNotification model)
        {
            try
            {
                if (string.IsNullOrEmpty( model.Message))
                {
                    string message = "The Bank Deposit Request message is required opertion failed";
                    GetExecutionMessages(model, false, message, MessagesResults.Failed,
                       ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, message);

                }
                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<DepositNotification>>(APICallHelper.DepositNotificationUrl, model.ConvertToTransferData(this.GetBranchID()));
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
        public async Task<ExecutionMessages> DepositNotificationApprovalRequest(Approval model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<DepositNotificationApproval>>(APICallHelper.ApproveDepositNotificationUrl, model);
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
        public async Task<ExecutionMessages> DepositNotificationApprovalRequest(DepositNotificationApproval model)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<DepositNotificationApproval>>(APICallHelper.ApproveDepositNotificationUrl, model);
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
        public async Task<ExecutionMessages> UpdateDepositNotificationRequest(DepositNotification model)
        {
            try
            {

                var OperationEvent = await GetDepositNotificationRequest(model.Id);
                if (OperationEvent != null)
                {

               

                    var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<bool>>(string.Format(APICallHelper.CancelDepositNotificationCommandUrl, model.Id), new {Id=model.Id});
                    if (response.IsSuccess)
                    {
                        // Successful creation.
                        GetExecutionMessages(response, true, $"{model.Id} Transaction was successfull", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.Id} Transaction failed", MessagesResults.Failed,
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
        public async Task<List<LiaisonLedgerEntry>> RetrieveLiasonAccountingEntries(SystemQuery model)
        {
            try
            {
                return await _accountingApiCallerHelper.PostLiaisonAccountAsync(APICallHelper.AccountingEntry_LiaisonEntries, model);
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }
        }
        //
        public async Task<List<AccountingEntry>> RetrieveAccountingEntries(SystemQuery model)
        {
            try
            {

                // Make an API call to create an individual profile


                return await _accountingApiCallerHelper.PostAccountingAsync(APICallHelper.AccountingEntry_Posting_Entries, model);

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }

        }
        public async Task<List<FSDocument>> GetAllFSDocument()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<FSDocument>>>(APICallHelper.GetAllFinancialDocument);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<FSDocument>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<BalansheetRpt>> GetBalanceSheetDataEntriesRPT(BSQuery model)
        {
            //Task<ServiceResponseXX<BalanceSheetData>> apiCallTask = null;
            List<BalansheetRpt> balanceSheetData = new List<BalansheetRpt>();
       
                try
                {
                    // Create a task for the API call
                    var apiCallTask = await _accountingApiCallerHelper.PostBalansheetAsync(APICallHelper.BalanceSheet_EntriesUrlPDF, model);
                return apiCallTask;

                }
                catch (OperationCanceledException)
                {
                    // This could happen if the timeout occurs and the API call is cancelled
                    
                }
                catch (Exception ex)
                {
                    // Log and handle exception
                    GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                        SystemMessageStatus.Failed.ToString(), ex);

                  
                }
          
            return balanceSheetData;
        }
        //

        public async Task<IncomeAndExpenseDto> GenerateIncomeStatementEntries(BSQuery model)
        {
            //Task<ServiceResponseXX<BalanceSheetData>> apiCallTask = null;


            try
            {
                // Create a task for the API call
                var apiCallTask = await _accountingApiCallerHelper.PostAsync<ResponseObject<IncomeAndExpenseDto>>(APICallHelper.AccountingEntry_IncomeStatement, model, 300);
                if (apiCallTask.IsSuccess)
                {

                    return apiCallTask.ApiResponseData.Data;


                }
                else
                {
                    return null;
                }

            }
            catch (OperationCanceledException)
            {
                // This could happen if the timeout occurs and the API call is cancelled

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);


            }


            return null;
        }
        //public async Task<BalanceSheetData> GetBalanceSheetDataEntries(BSQuery model)
        //{
        //    //Task<ServiceResponseXX<BalanceSheetData>> apiCallTask = null;


        //    try
        //    {
        //        // Create a task for the API call
        //        var apiCallTask = await _accountingApiCallerHelper.PostAsync<ResponseObject<BalanceSheetData>>(APICallHelper.BalanceSheet_EntriesUrl, model, 300);
        //        if (apiCallTask.IsSuccess)
        //        {

        //                return apiCallTask.ApiResponseData.Data;


        //        }
        //        else
        //        {
        //            return null; 
        //        }

        //    }
        //    catch (OperationCanceledException ex)
        //    {
        //        // This could happen if the timeout occurs and the API call is cancelled

        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
        //            SystemMessageStatus.Failed.ToString(), ex);


        //    }


        //    return null;
        //}


        public async Task<BalanceSheetData> GetBalanceSheetDataEntries(BSQuery model)
        {
            try
            {
 

                var apiCallTask = await _accountingApiCallerHelper
                    .PostAsync<ResponseObject<BalanceSheetData>>(APICallHelper.BalanceSheet_EntriesUrl, model, 300);

                if (apiCallTask.IsSuccess)
                { 
                    return apiCallTask.ApiResponseData.Data;
                }
                else
                {
             
                    return null;
                }
            }
            catch (OperationCanceledException ex)
            {
         

                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            catch (Exception ex)
            {
               
            }

            return null;
        }

        public async Task<List<AccountingEntry>> RetrieveAccountingEntries(JEQuery model)
        {
            try
            {

                // Make an API call to create an individual profile


                return await _accountingApiCallerHelper.PostAccountingAsync(APICallHelper.AccountingEntry_Posting_Entries, model);

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }

        }
        public async Task<List<AccountingEntry>> GetAllAccountingEntries()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEntry>>>(APICallHelper.AccountingEntry_Entries);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<AccountingEntry>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<AccountingEntry>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<AccountingEntry>> GetAccountingEntriesByReferceId(string reference)
        {
            try
            {
                string url = string.Format(APICallHelper.AccountingEntry_Get_reference_Id, reference);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEntry>>>(url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<AccountingEntry>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<AccountingEntry>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<AccountingEntry>> GetAllAccountingEntriesForAnAccountPerBranch(string branchId, string accountId)
        {
            try
            {
                string url = string.Format(APICallHelper.AccountingEntry_Entries_branchId_accountId, branchId, accountId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEntry>>>(url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<AccountingEntry>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<AccountingEntry>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }


        public async Task<List<AccountingEntry>> GetTrialBalance4ColumnEntries(TrialBalance4Column trialBalance)
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEntry>>>(APICallHelper.Trialbalance4Column_Entries);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<AccountingEntry>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<AccountingEntry>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<List<CashReplenimentRequestDto>> GetAllCashRequestApprovalQuery()
        {

            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<CashReplenimentRequestDto>>>(APICallHelper.GetAllCashRequestApprovalQuery);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CashReplenimentRequestDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<CashReplenimentRequestDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        //BankingOperation/GetAllDepositNotificationRedirectionQuery

        public async Task<List<DepositNotificationDto>> GetAllDepositNotificationRequest()
        {

            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<DepositNotificationDto>>>(APICallHelper.GetAllDepositNotificationRequestRequests);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DepositNotificationDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<DepositNotificationDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<List<DepositNotificationDto>> GetAllDepositNotificationRedirectionRequest()
        {

            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<DepositNotificationDto>>>(APICallHelper.GetAllDepositNotificationRedirectionQuery);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DepositNotificationDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<DepositNotificationDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<List<CashReplenimentRequestDto>> GetAllCashReplenimentRequest()
        {

            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<CashReplenimentRequestDto>>>(APICallHelper.GetAllCashReplenishmentRequests);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CashReplenimentRequestDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<CashReplenimentRequestDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<CashReplenimentRequest>> GetAllCashReplenimentRequestByBranch(bool VALUE)
        {

            try
            {
                string Url = string.Empty;
                Url = string.Format(APICallHelper.GetAllCashReplenishmentQueryAsBranch, VALUE);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<CashReplenimentRequest>>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CashReplenimentRequest>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<CashReplenimentRequest>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<CashReplenimentRequest>> GetAllCashReplenimentRequestByBranch(string Id)
        {

            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<CashReplenimentRequest>>>(APICallHelper.GetAllCashReplenishmentRequests);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CashReplenimentRequest>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<CashReplenimentRequest>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<List<UsersNotification>> GetUserNotificationRequest()
        {
            try
            {
                //var ff = string.Format(APICallHelper.UserNotificationRequestByIdUrl, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<UsersNotification>>>(APICallHelper.UserNotificationRequestUrl);

                if (couApiResponse.IsSuccess)
                {
                    //var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    //couApiResponse.ApiResponseData.Data.IssuedBy = user.Name + "," + user.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<UsersNotification>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<UsersNotification>> GetUserNotificationRequestByBranchId(string branchID)
        {
            try
            {
                var endpoint = string.Format(APICallHelper.UserNotificationRequestByIdUrl, branchID);
                //var ff = string.Format(APICallHelper.UserNotificationRequestByIdUrl, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<UsersNotification>>>(endpoint);

                if (couApiResponse.IsSuccess)
                {
                    //var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    //couApiResponse.ApiResponseData.Data.IssuedBy = user.Name + "," + user.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<UsersNotification>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }


        public async Task<DepositNotificationDto> GetDepositNotificationRequest(string Id)
        {
            try
            {
                var ff = string.Format(APICallHelper.GetDepositNotificationRequestById, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<DepositNotificationDto>>(string.Format(APICallHelper.GetDepositNotificationRequestRequests, Id));
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.IssuedBy = user.name + "," + user.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new DepositNotificationDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<CashReplenimentRequest> GetCashReplenimentRequest(string Id)
        {
            try
            {
                var ff = string.Format(APICallHelper.GetCashReplenishmentRequestById, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<CashReplenimentRequest>>(string.Format(APICallHelper.GetCashReplenishmentRequestById, Id));
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.TempId1 = user.name + "," + user.phoneNumber + " ";
                    couApiResponse.ApiResponseData.Data.TempId3 = couApiResponse.ApiResponseData.Data.TempData;
                    if (couApiResponse.ApiResponseData.Data.Status != "Pending")
                    {
                        var userx = await GetUser(couApiResponse.ApiResponseData.Data.ApprovedBy);
                        couApiResponse.ApiResponseData.Data.TempId2 = userx.name + "," + userx.phoneNumber + " ";
                        if (couApiResponse.ApiResponseData.Data.CorrespondingBranchId != "xxx" && couApiResponse.ApiResponseData.Data.CorrespondingBranchId != "1")
                        {
                            couApiResponse.ApiResponseData.Data.CorrespondingBranch = (await branchServices.GetBranch(couApiResponse.ApiResponseData.Data.CorrespondingBranchId)).Name;
                        }
                    }
                    return couApiResponse.ApiResponseData.Data;
                }
                return new CashReplenimentRequest();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        //public async Task<BankTransaction> GetBankTransactionById(string Id)
        //{
        //    try
        //    {

        //        var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<BankTransaction>>(string.Format(APICallHelper.GetBankTransactionQueryByIdURL, Id));
        //        if (couApiResponse.IsSuccess)
        //        {
        //            var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
        //            couApiResponse.ApiResponseData.Data.IssuedBy = user.Name + "," + user.phoneNumber + " ";
        //            return couApiResponse.ApiResponseData.Data;
        //        }
        //        return new CashReplenimentRequest();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw (ex);
        //    }
        //} 
        public async Task<CashReplenimentRequest> GetCashReplenimentReferenceRequest(string Id)
        {
            try
            {

                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<CashReplenimentRequest>>(string.Format(APICallHelper.GetCashReplenishmentRequestById, Id));
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.TempId1 = user.name + "," + user.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new CashReplenimentRequest();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<CashReplenimentRequest> GetCashReplenishmentRequestIdReference(string Id)
        {
            try
            {

                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<CashReplenimentRequest>>(string.Format(APICallHelper.GetCashReplenishmentRequestIdReference, Id));
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.TempId1 = user.name + "," + user.phoneNumber + " ";
                    var userx = await GetUser(couApiResponse.ApiResponseData.Data.ApprovedBy);
                    couApiResponse.ApiResponseData.Data.TempId2 = userx.name + "," + userx.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new CashReplenimentRequest();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<ExecutionMessages> UploadFiles(AddDocumentUploadedCommand documentRequest)
        {
            try
            {
                // Check if files are attached
                if (documentRequest.FormFiles == null)
                {
                    // throw new FileLoadException("File not uploaded successfully") ;
                    // Handle case where no files are attached
                    return GetExecutionMessages(documentRequest, false, "File", MessagesResults.Failed,
                  ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
              null);
                }
                var additionalParams = new Dictionary<string, string>
                {

                     { "IsSynchronus", documentRequest.IsSynchronus.ToString()},
                    { "OperationID", documentRequest.OperationID },
                    { "DocumentId", "N/A" },
                    { "DocumentType", documentRequest.DocumentType },
                    { "ServiceType", documentRequest.ServiceType },
                    { "CallBackBaseUrl",ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString()},
                    { "CallBackEndPoint", APICallHelper.AttachedDocuments},
                    { "RemoteFilePath", documentRequest.RemoteFilePath },
                };
                List<HttpPostedFileBase> httpPostedFileBases = new List<HttpPostedFileBase>();
                httpPostedFileBases.Add(documentRequest.FormFiles);
                var response = await _IdentityServerBaseUrl.PostFilesAndParamsAsync(APICallHelper.AttachedDocuments, additionalParams, httpPostedFileBases);
                if (response.statusCode == 200)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        "Uploaded successfully");
                    return ExecutionMessage;
                }
                else
                {
                    GetExecutionMessages(documentRequest, false, null, MessagesResults.Failed,
                  ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "File upload failed");
                }


            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<IExecutionMessages> CreateBankCashTransaction(BankCashOut model)
        {
            try
            {

                if (model.UploadedFile == null)
                {
                    throw new Exception("No receipt was uploaded, Kindly upload the reciept of the transaction!");
                }
                else
                {
                    var DocModel = new AddDocumentUploadedCommand
                    {
                        FormFiles = model.UploadedFile,
                        IsSynchronus = true,
                        OperationID = model.Id,
                        DocumentType = "BranchBankCashOut",
                        ServiceType = "AccountingService".ToUpper(),
                        DocumentId = model.BankTransactionReference,
                        CallBackBaseUrl = "N/A",
                        CallBackEndPoint = "N/A",
                        RemoteFilePath = APICallHelper.AttachedCashOutReceiptRemotely
                    };
                    var modelFile = await UploadFiles(DocModel);
                    if (modelFile.Data == null)
                    {
                        throw new Exception("File upload failed and response could not be interpreted!");
                    }
                    else
                    {
                        APICallBackRespose responsed = (APICallBackRespose)modelFile.Data;
                        model.FileUpload = responsed.data.fullPath;

                        var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.BankCashOutCommandUrl, model.ConvertToTransferData());
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

        public async Task<IExecutionMessages> UploadBankDepositTransactionReciept(UploadBankReciept model)
        {
            try
            {

                if (model.UploadedFile == null)
                {
                    throw new Exception("No receipt was uploaded, Kindly upload the reciept of the transaction!");
                }
                else
                {
                    var DocModel = new AddDocumentUploadedCommand
                    {
                        FormFiles = model.UploadedFile,
                        IsSynchronus = true,
                        OperationID = model.Id,
                        DocumentType = "BranchBankDeposit",
                        ServiceType = "AccountingService".ToUpper(),
                        DocumentId = model.BankTransactionReference,
                        CallBackBaseUrl = "N/A",
                        CallBackEndPoint = "N/A",
                        RemoteFilePath = APICallHelper.AttachedCashInReceiptRemotely
                    };
                    var modelFile = await UploadFiles(DocModel);
                    if (modelFile.Data == null)
                    {
                        throw new Exception("File upload failed and response could not be interpreted!");
                    }
                    else
                    {
                        APICallBackRespose responsed = (APICallBackRespose)modelFile.Data;
                        model.FileUpload = responsed.data.fullPath;
                        var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<bool>>(string.Format(APICallHelper.UpdateDepositNotificationCommandUrl, model.Id), model.ConvertToUploadBankRecieptDto());
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
        public async Task<IExecutionMessages> CreateBranchToBranchTransferTransaction(BranchToBranchTransfer model)
        {
            try
            {
                var modal = model.ConvertToTransferData();

                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.BranchToBranchTransferUrl, modal);
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
        //BankDepositCashClearing
        public async Task<IExecutionMessages> CreateCashClearingTransaction(CashClearing model)
        {
            try
            {

                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CashClearingTransferCashReplenishmentUrl, model.ConvertToTransferData());
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

        public async Task<IExecutionMessages> BankDepositCashClearingTransaction(BankDepositCashClearing model)
        {
            try
            {

                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CashClearingTransferBankDepositUrl, model.ConvertToTransferData());
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
        public async Task<ExecutionMessages> CreateApprovalRequest(CashApprovalResponse model, bool HasError)
        {
            try
            {
                if (HasError)
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                                      SystemMessageStatus.Failed.ToString(), new Exception("Your cannot redirect your cash request to you self,Kindly contact admin for assitance"));
                }
                else
                {
                    var listOfAccounts = await _accountServices.GetAllBranchAccountUsedToCreditCashFlow(this.GetBranchID());
                    if (model.IsApproved==true && listOfAccounts.Count==0)
                    {
                        if (_accountServices.IsHeadOffice())
                        {
                            var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<CashApprovalResponse>>(APICallHelper.CashReplenishmentResponse, model);
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
                        else
                        {
                            GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                                                  SystemMessageStatus.Failed.ToString(), new Exception("There no bank account placed under the management of this branch, Please kindly contact system administrator"));

                        }

                    }
                    else
                    {
                        var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<CashApprovalResponse>>(APICallHelper.CashReplenishmentResponse, model);
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

        //BankDepositCashClearing
        public async Task<IExecutionMessages> AcceptanceRequestURl(string model)
        {
            try
            {

                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.AcceptanceRequestURl, new { Id = model });
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

        public async Task<IEnumerable<User>> GetUserList()
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var userLists = await ApiCallerHelper.GetAsync<ResponseObject<List<User>>>(APICallHelper.GetUsers);
                var newList = new List<User>();

                if (userLists.ApiResponseData != null)
                {
                    var braches = await GetBranches();


                    foreach (var a in userLists.ApiResponseData.Data)
                    {
                        a.name = $"{a.firstName} {a.lastName}";
                        a.strlastLoginDate = a.LastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        a.status = a.isActive ? "Active" : "In-active";
                        if (a.BranchID != null)
                        {
                            //a.Brancch = braches.Where(x => x.Id == a.BranchID).FirstOrDefault();
                            //a.Bank = braches.Where(x => x.Id == a.BranchID).FirstOrDefault().Bank;
                            //if (a.Bank == null)
                            //{
                            //    a.BankID = null;
                            //    a.Bank = new FrontDesk.Data.Entity.Config.Bank();
                            //}

                        }
                        else
                        {
                            a.Bank = new FrontDesk.Data.Entity.Config.Bank();
                            a.Brancch = new FrontDesk.Data.Entity.Config.Branch();
                        }
                        newList.Add(a);
                    }
                    return newList;
                }



                return newList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<FrontDesk.Data.Entity.Config.Branch>> GetBranches()
        {////780400915211061
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
                var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Config.Bank>>((string.Format(APICallHelper.Get_Update_Delete_Bank, GetBankID())));
                var bracBranches = branchApiResponse.ApiResponseData.Data;
                var branches = bracBranches.Branches;
                return branches;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<User> GetUser(string userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var user = await ApiCallerHelper.GetAsync<ResponseObject<User>>(string.Format(APICallHelper.GetUserByID, userid));
                if (user.IsSuccess)
                {
                    user.ApiResponseData.Data.name = $"{user.ApiResponseData.Data.firstName} {user.ApiResponseData.Data.lastName}";
                    user.ApiResponseData.Data.strlastLoginDate = user.ApiResponseData.Data.LastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                    user.ApiResponseData.Data.status = user.ApiResponseData.Data.isActive ? "Active" : "In-active";
                    user.ApiResponseData.Data.ChangePassword.userName = user.ApiResponseData.Data.userName;
                    user.ApiResponseData.Data.roleID = user.ApiResponseData.Data.userRoles.Select(role => role.roleId).First();
                    return user.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TransactionReversalDetailRequestDto> GetTransasctionReversalRequestById(string Id)
        {
            try
            {
                var url = string.Format(APICallHelper.GetTransactionReversalRequest, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<TransactionReversalDetailRequestDto>>(url);
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.IssuedBy = user.name + "," + user.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new TransactionReversalDetailRequestDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<TransactionReversalDetailRequestDto> GetTransactionReversalRequestByReferenceId(string Id)
        {
            try
            {
                var url = string.Format(APICallHelper.GetTransactionReversalRequestByReferenceId, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<TransactionReversalDetailRequestDto>>(url);
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.IssuedBy = user.name + "," + user.phoneNumber + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new TransactionReversalDetailRequestDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<bool> CheckIfTransactionReferenceIdExist(string Id)
        {
            try
            {
                var url = string.Format(APICallHelper.CheckIfTransactionReversalRequestByReferenceIdExist, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<bool>>(url);
                if (couApiResponse.IsSuccess)
                {

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<ExecutionMessages> TransactionReversalRequestApproval(TransactionReversalRequestApproval model)
        {
            try
            {

                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<TransactionReversalDetailRequestDto>>(APICallHelper.TransactionReversalRequestApproval, model);

                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response.ApiResponseData, true, $"Transaction was successfull", MessagesResults.Success,
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

        public async Task<ExecutionMessages> TransactionReversalRequest(TransactionReversalRequest model)
        {
            try
            {

                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<TransactionReversalDetailRequestDto>>(APICallHelper.TransactionReversalRequest, model);

                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response.ApiResponseData, true, $"Transaction was successfull", MessagesResults.Success,
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
        public async Task<List<TransactionReversalDetailRequestDto>> GetAllTransasctionReversalRequest()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<TransactionReversalDetailRequestDto>>>(APICallHelper.GetAllTransactionReversalRequest);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<TransactionReversalDetailRequestDto>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<TransactionReversalDetailRequestDto>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<ExecutionMessages> CreateTransactionReversalRequest(TransactionReversalRequest model)
        {
            var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<TransactionReversalDetailRequestDto>>(APICallHelper.TransactionReversalRequest, model);
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

        public async Task<List<BranchLiaisonLedgerEntry>> RetrieveBranchLiaisonAccountingEntries(SystemQuery model)
        {
            try
            {

                // Make an API call to create an individual profile


                return await _accountingApiCallerHelper.PostBranchLiaisonAccountAsync(APICallHelper.AccountingEntry_BranchLiaisonEntries, model);

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }
        }

        public async Task<List<TrialBalance6ColumnDto>> RetrieveTrialBalance6ColumnEntries(SystemQuery model)
        {
            try
            {

                // Make an API call to create an individual profile


                var modelc = await _accountingApiCallerHelper.PostTrialBalance6ColumnAsyncAsync(APICallHelper.AccountingEntry_Generate6ColumnTrialBalance, model);
                return modelc;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }
        }

        public async Task<List<TrialBalance4ColumnDto>> RetrieveTrialBalance4ColumnEntries(SystemQuery model)
        {
            try
            {

                // Make an API call to create an individual profile


                return await _accountingApiCallerHelper.PostTrialBalance4ColumnAsyncAsync(APICallHelper.AccountingEntry_Generate4ColumnTrialBalance, model);

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }
        }

        public async Task<List<ModelBalanceSheetAssets>> RetrieveBalanceSheetColumnEntries(SystemQuery model)
        {
            try
            {

                // Make an API call to create an individual profile


                return await _accountingApiCallerHelper.PostModelBalanceSheetAssetsAsync(APICallHelper.AccountingEntry_BalanceSheetColumn, model);

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }
        }

        public async Task<List<ModelBalanceSheetAssets>> RetrieveIncomeAndExpenseEntries(BSQuery model)
        {
            try
            {

                // Make an API call to create an individual profile


                return await _accountingApiCallerHelper.PostIncomeAndExpenseEntriesAsync(APICallHelper.AccountingEntry_IncomeStatement, model);

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }
        }

        public async Task<BankTransaction> GetBankTransactionByReferenceId(string referenceId)
        {
            try
            {
                var urlString = string.Format(APICallHelper.GetBankTransactionQueryByReferenceIdURL, referenceId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<BankTransaction>>(string.Format(urlString));
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.CreatedBy);
                    couApiResponse.ApiResponseData.Data.CreatedBy = user.roleName + "," + user.name + " ";
                    return couApiResponse.ApiResponseData.Data;
                }
                return new BankTransaction();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<AccountingGeneralLedgerDetails> GetAccountGLEntries(SystemQuery model)
        {
            try
            {

                // Make an API call to create an individual profile
                var urlString = APICallHelper.AccountingEntry_AccountGl;
                var couApiResponse = await  _accountingApiCallerHelper.PostGLAsync(urlString,model);
                if (couApiResponse!=null)
                {
                     
                    return couApiResponse;
                }
                else
                {
                    throw new Exception("There are no accounting entries made for accountId:"+ GetAccountValues(model.AccountIds));
                }
 
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                throw (ex);
            }
        }

        private string GetAccountValues(List<string> accountIds)
        {
            string message = string.Empty;
            foreach (var item in accountIds)
            {
                message = message + ", ";
            }
            return message;
        }

         
        public async Task<BranchToBranchTransferDto> GetCachedBranchToBranchTransferData(string referenceId)
        {
            try
            {
                var urlString = string.Format(APICallHelper.GetCashBranchToBranchTransferUrl, referenceId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<BranchToBranchTransferDto>>(string.Format(urlString));
                if (couApiResponse.IsSuccess)
                {
                    
                    return couApiResponse.ApiResponseData.Data;
                }
                return new BranchToBranchTransferDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }

        public async Task<List<CashReplenimentRequestDto>> GetCashReplenishmentEntries(QueryFilter model)
        {
            try
            {
                //status={0}&fromDate={1}&toDate={2}&branchId={3}&issuedBy={4}&approvedBy={5}
                string url = APICallHelper.Url_Get_AllCashRelenishmentRequest;//, model.Status, model.FromDate,model.ToDate,model.BranchId,model.IssuedBy,model.ApprovedBy);
                var cusResponseObject = await _accountingApiCallerHelper.PostCashReplenishmentEntriesAsync<ResponseObject<List<CashReplenimentRequestDto>>>(url, model);
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<List<DepositNotificationDto>> GetBankDepositEntries(QueryFilter model)
        {
            try
            {
                //status={0}&fromDate={1}&toDate={2}&branchId={3}&issuedBy={4}&approvedBy={5}
                string url = APICallHelper.Url_Get_AllBankDepositeRequest;//, model.Status, model.FromDate,model.ToDate,model.BranchId,model.IssuedBy,model.ApprovedBy);
                var cusResponseObject = await _accountingApiCallerHelper.PostDepositNotificationAsync<ApiResponse<List<DepositNotificationDto>>>(url, model);
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData;
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

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CashRoot
    {
        public string id { get; set; }
        public string text { get; set; }
    }


}
