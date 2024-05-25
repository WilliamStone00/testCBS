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
using CBS.FrontDesk.Data;

namespace CBS.BusinessService.Accounting
{
    public class AccountingServices : BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;

        public AccountingServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }


        public async Task<ExecutionMessages> Create(Account model)
        {
            try
            {

                // Make an API call to create an individual profile
          
                model.AccountOwnerId=GetBranchID();
                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<bool>>(APICallHelper.CreateAccount, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Account {model.AccountNumber + " " + model.AccountName} has been created successfully", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "");
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Account {model.AccountNumber + " " + model.AccountName} failed to be created ", MessagesResults.Failed,
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
       
        //public async Task<Account> GetAccount(string id)
        //{
        //    try
        //    {
        //        var cusResponseObject = await _accountingApiCallerHelper.GetAsync<ResponseObject<Account>>(string.Format(APICallHelper.GetAccount, id));
        //        if (cusResponseObject.IsSuccess)
        //        {
        //            if (cusResponseObject.ApiResponseData == null)
        //            {

        //            }
        //            else
        //            {
        //                return cusResponseObject.ApiResponseData.Data;
        //            }

        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw ex;
        //    }
        //}

        public async Task<Account> GetAccount(string id)
        {
            try
            {
                var cusResponseObject =(await GetAllAccounting()).Where(i=>i.Id.Equals(id)).FirstOrDefault();
                return cusResponseObject;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Account>> GetAllLiasionAccount(string BranchId)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetSystemLiaisonAccountQueryUrl, BranchId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<Account>> GetAllBranchAccountUsedToCreditCashFlow(string BranchId)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetAllBranchAccountUsedToCreditCashFlow, BranchId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<Account>> GetAllAccounting()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(APICallHelper.GetAlAccounts);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw(ex);
            }
        }

        public async Task<List<Account>> GetAllAccountForABranch(string branchId)
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(string.Format(APICallHelper.GetAllAccountByBranch,branchId));
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<IEnumerable<AccountingRole>> GetAccountingRoles()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingRole>>>(APICallHelper.GetAllAccountingRules);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<AccountingRole>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ExecutionMessages> Update(Account account)
        {
            try
            {
                account.AccountOwnerId = GetBranchID();
                var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<Account>>(string.Format(APICallHelper.PutAccount, account.Id), account);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Account {account.AccountNumber + " " + account.AccountName} has been updated successfully", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(response.ApiResponseData, false, $"{account.AccountNumber + " " + account.AccountName} has been updated", MessagesResults.Failed,
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


        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var account = await GetAccount(id);
                var inResponse = await _accountingApiCallerHelper.DeleteAsync<ResponseObject<bool>>(string.Format(APICallHelper.Delete_Account, id));
                if (inResponse.IsSuccess)
                {


                    return GetExecutionMessages(inResponse, true, $"{account.AccountNumber + " " + account.AccountName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                   

                }
                else
                {
                    // Handle failure scenario
                    return GetExecutionMessages(account, false, $"{account.AccountNumber + " " + account.AccountName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
                
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return null;
        }
        public async Task<ExecutionMessages> Create(UploadAccountCommand list)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<List<AccountModel>>>(APICallHelper.CreateAccounOnUploadie, list);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{list.AccountModelList[0].AccountNumber}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(response, true, $"{list.AccountModelList[0].AccountNumber}", MessagesResults.Failed,
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
     
    }
}
