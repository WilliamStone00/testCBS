using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper.Helper;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Service;
using System.Web.Mvc;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.API.Helper.APICallHelper;

namespace CBS.BusinessService.Accounting
{
    public class ChartOfAccountServices:BaseApiServices
    {
        private readonly ApiCallerHelper _ConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }

        public ChartOfAccountServices()
        {
            _ConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            this.BranchId = GetBranchID();
            this.BankId = GetBankID();
            this.OrganizationId = GetOrganizationID();
        }
        public Task<List<SelectListItem>> GetBookingDirections()
        {
            var bookingDirections = new SelectListItem[] { new SelectListItem { Text = "Debit", Value = "Debit" }, new SelectListItem { Text = "Credit", Value = "Credit" } }.ToList();
            return Task.FromResult(bookingDirections);
        }
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objOperationEvent = await GetChartOfAccountByAccountNumber(id);
                var inResponse = await _ConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Delete_ChartOfAccount, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objOperationEvent.AccountNumber + " " + objOperationEvent.LabelEn}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objOperationEvent, false, $"{objOperationEvent.AccountNumber + " " + objOperationEvent.LabelEn}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<AccountTreeNode>> GetAllChartOfAccountTreeNodes()
        {
            try
            {
                var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.AccountTreeNode>>>(APICallHelper.GetAllJsTreeNode);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<FrontDesk.Data.Entity.Accounting.AccountTreeNode>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<ChartOfAccount>> GetAllChartOfAccounts()
        {
            try
            {

                try
                {
                    var couApiResponse = await _ConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>>(APICallHelper.GetAllChartOfAccount);
                    if (couApiResponse.IsSuccess)
                    {

                        //    couApiResponse.ApiResponseData.Data.RemoveAll(x => x.LabelEn.Contains(x.AccountNumber));
                        return couApiResponse.ApiResponseData.Data.OrderBy(x => x.AccountNumber).ToList();
                    }
                    return new List<ChartOfAccount>();
                }
                catch (Exception ex)
                {

                    throw(ex);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ChartOfAccount> GetChartOfAccountById(string id)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccount, id));
                if (cusResponseObject.IsSuccess)
                {
                    if (cusResponseObject.ApiResponseData==null)
                    {
                        
                    }
                    else
                    {
                        return cusResponseObject.ApiResponseData.Data;
                    }
                    
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(ChartOfAccountDto model)
        {
            try
            {

                // Make an API call to create an individual profile

             
                var response = await _ConfigApiHelper.PostAsync<ServiceResponse<ChartOfAccount>>(APICallHelper.CreateChartOfAccount, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.AccountNumber+" "+model.LabelEn}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.AccountNumber+ " " + model.LabelEn}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(ChartOfAccount model)
        {
            ChartOfAccount chartOfAccount = null;
            try
            {

                var Account = await GetChartOfAccountByAccountNumber (model.AccountNumber);
                if (model != null)
                {

                    if (Account!=null)
                    {
                        if (Account.ParentAccountId==null&& model.AccountNumber.Length>=3)
                        {
                            var number = model.AccountNumber.Substring(0,model.AccountNumber.Length - 1);
                            chartOfAccount = await GetChartOfAccountByAccountNumber(number);
                            if (chartOfAccount == null) 
                            {
                                GetExecutionMessages(model, false, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Failed,
                             ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Parent account reference not found");
                                return ExecutionMessage;

                            }
                            Account.ParentAccountId = chartOfAccount.Id;
                            Account.ParentAccountNumber = chartOfAccount.AccountNumber;
                        }
                        Account.AccountNumber = model.AccountNumber;
                        Account.LabelEn = model.LabelEn;
                        Account.LabelFr = model.LabelFr;
                        Account.IsBalanceSheetAccount = model.IsBalanceSheetAccount;
                        Account.AccountCartegoryId = model.AccountCartegoryId;
                        Account.CanBeNegative= model.CanBeNegative;
                        Account.IsDebit= model.IsDebit;
                        if (chartOfAccount != null)
                        {
                            Account.ParentAccountId = chartOfAccount.ParentAccountId;
                            Account.ParentAccountNumber = chartOfAccount.ParentAccountNumber;
                        }
            
                      
                        var response = await _ConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccount, Account.Id), Account);
                        if (response.IsSuccess)
                        {
                            // Successful creation
                            GetExecutionMessages(response, true, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Success,
                                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                            return ExecutionMessage;
                        }
                        else
                        {
                            // Failed creation
                            GetExecutionMessages(model, false, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Failed,
                                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                            return ExecutionMessage;
                        }
                    }
                    else
                    {
                        var response = new ApiResponse<ServiceResponse<ChartOfAccount>>();
                        response.Message= $"{model.AccountNumber} cannot be updated because it doesn't exist in the system kindly create";
                        GetExecutionMessages(model, false, $"{model.AccountNumber} cannot be updated because it doesn't exist in the system kindly create", MessagesResults.Failed,
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

        public async Task<ExecutionMessages> UpdateAccountCategory(ChartOfAccount model)
        {
            ChartOfAccount chartOfAccount = null;
            try
            {

 
                if (model != null)
                {

                    
                        


                        var response = await _ConfigApiHelper.PutAsync<ServiceResponse<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccount, model.Id), model);
                        if (response.IsSuccess)
                        {
                            // Successful creation
                            GetExecutionMessages(response, true, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Success,
                                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                            return ExecutionMessage;
                        }
                        else
                        {
                            // Failed creation
                            GetExecutionMessages(model, false, $"{model.AccountNumber + " " + model.LabelEn}", MessagesResults.Failed,
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

        public async Task<ChartOfAccount> GetChartOfAccountByAccountNumber(string accountNumber)
        {
            try
            {
                var cusResponseObject = await _ConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.ChartOfAccount>>(string.Format(APICallHelper.Get_ChartOfAccount_By_AccountNumber, accountNumber));
                if (cusResponseObject.IsSuccess)
                {
                    if (cusResponseObject.ApiResponseData == null)
                    {

                    }
                    else
                    {
                        return cusResponseObject.ApiResponseData.Data;
                    }

                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

            ////////*****************************************************************************//////
            // Simple temporary methods for liaison mapping
        public async Task<IEnumerable<ChartOfAccount>> GetAssetAccountsByBranch(string branchId)
        {
            try
            {
                var allAccounts = await GetAllChartOfAccounts();
                return allAccounts.Where(a => a.AccountNumber.StartsWith("1")).ToList();
            }
            catch (Exception ex)
            {
                return new List<ChartOfAccount>();
            }
        }

        public async Task<IEnumerable<ChartOfAccount>> GetLiabilityAccountsByBranch(string branchId)
        {
            try
            {
                var allAccounts = await GetAllChartOfAccounts();
                return allAccounts.Where(a => a.AccountNumber.StartsWith("2")).ToList();
            }
            catch (Exception ex)
            {
                return new List<ChartOfAccount>();
            }
        }

    }
}

