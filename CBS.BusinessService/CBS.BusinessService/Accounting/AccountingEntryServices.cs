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
using CBS.FrontDesk.Data.UserManagement;
using System.Runtime.InteropServices;


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
        public async Task<List<DisplayData>> GetCashReplenimentRequestId()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<DisplayData>>>(APICallHelper.CurrentOpenOfDayHistory);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<DisplayData>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<DisplayData>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public List<Currency> Currencies()
        {
            Currency currency = new Currency();
           return  currency.CreateCurrencies();
        }
        public async Task<ExecutionMessages> CreateManualAccountingEntry( ManualAccountingEntry model)
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

        public async Task<List<CashReplenimentRequest>> GetAllCashReplenimentRequest()
        {
      
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<CashReplenimentRequest>>>(APICallHelper.GetAllCashReplenishmentRequests);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData==null)
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

        public async Task<CashReplenimentRequest> GetCashReplenimentRequest(string Id)
        {
            try
            {
                var ff = string.Format(APICallHelper.GetCashReplenishmentRequestById, Id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<CashReplenimentRequest>>(string.Format(APICallHelper.GetCashReplenishmentRequestById, Id));
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.IssuedBy = user.name + "," + user.phoneNumber+ " ";
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

        public async Task<CashReplenimentRequest> GetCashReplenimentReferenceRequest(string Id)
        {
            try
            {
               
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<CashReplenimentRequest>>(string.Format(APICallHelper.GetCashReplenishmentRequestIdReference, Id));
                if (couApiResponse.IsSuccess)
                {
                    var user = await GetUser(couApiResponse.ApiResponseData.Data.IssuedBy);
                    couApiResponse.ApiResponseData.Data.IssuedBy = user.name + "," + user.phoneNumber + " ";
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
        public async Task<ExecutionMessages> CreateApprovalRequest(CashApprovalResponse model)
        {
            try
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
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<UserList>> GetUserList()
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var userLists = await ApiCallerHelper.GetAsync<ResponseObject<List<UserList>>>(APICallHelper.GetUsers);
                var newList = new List<UserList>();
                
                if (userLists!=null)
                {
                    var braches = await GetBranches();


                    foreach (var a in userLists.ApiResponseData.Data)
                    {
                        a.name = $"{a.firstName} {a.lastName}";
                        a.strlastLoginDate = a.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                        a.status = a.isActive ? "Active" : "In-active";
                        if (a.BranchID!=null)
                        {
                            a.Branch = braches.Where(x => x.Id == a.BranchID).FirstOrDefault();
                            a.Bank= braches.Where(x => x.Id == a.BranchID).FirstOrDefault().Bank;
                            if (a.Bank==null)
                            {
                                a.BankID = null;
                                a.Bank = new FrontDesk.Data.Entity.Config.Bank();
                            }
                    
                        }
                        else
                        {
                            a.Bank = new FrontDesk.Data.Entity.Config.Bank();
                            a.Branch = new FrontDesk.Data.Entity.Config.Branch();
                        }
                        newList.Add(a);
                    }
                    return newList;
                }

               
                
                return userLists.ApiResponseData.Data;
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
                var branchApiResponse = await ApiCallerHelper.GetAsync<ResponseObject<   FrontDesk.Data.Entity.Config.Bank>>((string.Format(APICallHelper.Get_Update_Delete_Bank, GetBankID())));
                var bracBranches = branchApiResponse.ApiResponseData.Data;
                var branches = bracBranches.Branches;
                return branches;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<UserList> GetUser(string userid)
        {
            try
            {
                var ApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
                var user = await ApiCallerHelper.GetAsync<ResponseObject<UserList>>(string.Format(APICallHelper.GetUserByID, userid));
                if (user.IsSuccess)
                {
                    user.ApiResponseData.Data.name = $"{user.ApiResponseData.Data.firstName} {user.ApiResponseData.Data.lastName}";
                    user.ApiResponseData.Data.strlastLoginDate = user.ApiResponseData.Data.lastLoginDate.ToString("dd-MM-yyyy hh:mm:ss");
                    user.ApiResponseData.Data.status = user.ApiResponseData.Data.isActive ? "Active" : "In-active";
                    user.ApiResponseData.Data.ChangePassword.userName = user.ApiResponseData.Data.userName;
                    user.ApiResponseData.Data.roleID = user.ApiResponseData.Data.userRoles.Select(role => role.roleId).First();
                }
                return user.ApiResponseData.Data;
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

        public  async Task<bool>  CheckIfTransactionReferenceIdExist(string Id)
        {
            try
            {
                var url = string.Format(APICallHelper.CheckIfTransactionReversalRequestByReferenceIdExist, Id);
                var couApiResponse =   await _accountingApiCallerHelper.GetAsync<ResponseObject<bool>>(url);
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
    }
}
