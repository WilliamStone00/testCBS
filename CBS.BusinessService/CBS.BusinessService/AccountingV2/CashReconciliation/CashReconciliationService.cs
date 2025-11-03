using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
using CBS.FrontDesk.Data.Entity.AndriodApp;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Microsoft.AspNet.SignalR.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.AccountingV2.CashReconciliation
{
    public class CashReconciliationService : BaseService
    {

        private readonly ApiCallerHelper _CashapiCallerHelper;


        // private readonly List<JournalEntry> _mockClearances;
        public CashReconciliationService()
        {

            _CashapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

        public async Task<AccountGroupDto> GetCashReconciliationAccountsAsync(string branchId)  
        {
            try
            {
                var api = string.Format(APICallHelper.GetCashReconciliationAccounts, branchId);
                var apiResponse = await _CashapiCallerHelper.GetAsync<ResponseObject<AccountGroupDto>>(api);
                var response = apiResponse?.ApiResponseData?.Data ?? new AccountGroupDto();

                return response;
                        
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TrialBalanceEntry> GetAccountBalance(string accountId)
        {
            try
            {
                var api = string.Format(APICallHelper.GetAccountBalance, accountId);
                var apiResponse = await _CashapiCallerHelper.GetAsync<ResponseObject<TrialBalanceEntry>>(api);
                var accountBalance = apiResponse?.ApiResponseData?.Data ?? new TrialBalanceEntry();

                return accountBalance;

            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching cash reconciliation accounts.", ex);
            }
        }
       



        //public decimal GetAccountBalance(string accountId)
        //{
        //    var balances = new Dictionary<string, decimal>
        //{
        //    { "293595619306904", 120000.50m },
        //    { "378663546416125", 54000.75m },
        //    { "781320160686593", 7800.00m },
        //    { "571000907171642", 45000.25m },
        //    { "949938076276909", 100000.00m },

        //    { "971854780649377", 2500.10m }
        //};

        //    return balances.ContainsKey(accountId) ? balances[accountId] : 0m;
        //}

        public async Task<ExecutionMessages> Create(CashAndVaultInit model)
        {
            try
            {

                model.GlBalance = model.OGlBalance;
                model.DifferenceInAmount= model.CashInHand - model.GlBalance;
                string jsonData = JsonConvert.SerializeObject(model);
                var response = await _CashapiCallerHelper.PostAsync<ServiceResponse<CashAndVaultInit>>(APICallHelper.SaveReconciliation, model);
                 
                if (response.ApiResponseData != null)

                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
