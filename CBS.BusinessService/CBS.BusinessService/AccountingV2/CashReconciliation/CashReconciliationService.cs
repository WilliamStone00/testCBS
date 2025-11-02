using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
using CBS.FrontDesk.Data.Entity.AndriodApp;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Microsoft.AspNet.SignalR.Hosting;
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
                throw new Exception("Error fetching cash reconciliation accounts.", ex);
            }
        }


        //public  Task<AccountGroupDto> GetCashReconciliationAccountsAsync(string branchId)
        //{
        //    var result = new AccountGroupDto
        //    {
        //        Treasury = new List<AccountDetailDto>
        //    {
        //        new AccountDetailDto { AccountNumber = "571020001000", AccountName = "CASH IN HAND TILL 1", AccountId = "185017120640895" },
        //        new AccountDetailDto { AccountNumber = "560810000000", AccountName = "CCC SAVINGS ACCOUNTS", AccountId = "378663546416125" },
        //        new AccountDetailDto { AccountNumber = "560320009000", AccountName = "SCB CURRENT ACCOUNTS", AccountId = "781320160686593" },
        //        new AccountDetailDto { AccountNumber = "560810002000", AccountName = "EXPRESS UNION SAVINGS", AccountId = "931321352267142" },
        //        new AccountDetailDto { AccountNumber = "571020000000", AccountName = "CASH IN HAND", AccountId = "949938076276909" },
        //        new AccountDetailDto { AccountNumber = "560320000000", AccountName = "NFC WESTERN UNION OPS A/C", AccountId = "971854780649377" }
        //    },
        //        DeficitAccounts = new List<AccountDetailDto>
        //    {
        //        new AccountDetailDto { AccountNumber = "571020001000", AccountName = "CASH IN HAND TILL 1", AccountId = "185017120640895" },
        //        new AccountDetailDto { AccountNumber = "560810000000", AccountName = "CCC SAVINGS ACCOUNTS", AccountId = "378663546416125" },
        //        new AccountDetailDto { AccountNumber = "560320009000", AccountName = "SCB CURRENT ACCOUNTS", AccountId = "781320160686593" },
        //        new AccountDetailDto { AccountNumber = "560810002000", AccountName = "EXPRESS UNION SAVINGS", AccountId = "931321352267142" },
        //        new AccountDetailDto { AccountNumber = "571020000000", AccountName = "CASH IN HAND", AccountId = "949938076276909" },
        //        new AccountDetailDto { AccountNumber = "560320000000", AccountName = "NFC WESTERN UNION OPS A/C", AccountId = "971854780649377" }
        //    }
        //    };

        //    // ✅ Return as Task to simulate async
        //    return Task.FromResult(result);
        //}

        //public Task<AccountGroupDto> GetCashReconciliationAccountsAsync(string branchId)
        //{
        //    var result = new AccountGroupDto
        //    {
        //        Treasury = new List<AccountDetailDto>
        //        {
        //            new AccountDetailDto { AccountNumber = "571020001000", AccountName = "CASH IN HAND TILL 1", AccountId = "185017120640895" },
        //            new AccountDetailDto { AccountNumber = "560810000000", AccountName = "CCC SAVINGS ACCOUNTS", AccountId = "378663546416125" },
        //            new AccountDetailDto { AccountNumber = "560320009000", AccountName = "SCB CURRENT ACCOUNTS", AccountId = "781320160686593" },
        //            new AccountDetailDto { AccountNumber = "560810002000", AccountName = "EXPRESS UNION SAVINGS", AccountId = "931321352267142" },
        //            new AccountDetailDto { AccountNumber = "571020000000", AccountName = "CASH IN HAND", AccountId = "949938076276909" },
        //            new AccountDetailDto { AccountNumber = "560320000000", AccountName = "NFC WESTERN UNION OPS A/C", AccountId = "971854780649377" }
        //        },
        //        DeficitAccounts = new List<AccountDetailDto> 
        //        {
        //            new AccountDetailDto { AccountNumber = "571020001000", AccountName = "CASH IN HAND TILL 1", AccountId = "185017120640895" },
        //            new AccountDetailDto { AccountNumber = "560810000000", AccountName = "CCC SAVINGS ACCOUNTS", AccountId = "378663546416125" },
        //            new AccountDetailDto { AccountNumber = "560320009000", AccountName = "SCB CURRENT ACCOUNTS", AccountId = "781320160686593" },
        //            new AccountDetailDto { AccountNumber = "560810002000", AccountName = "EXPRESS UNION SAVINGS", AccountId = "931321352267142" },
        //            new AccountDetailDto { AccountNumber = "571020000000", AccountName = "CASH IN HAND", AccountId = "949938076276909" },
        //            new AccountDetailDto { AccountNumber = "560320000000", AccountName = "NFC WESTERN UNION OPS A/C", AccountId = "971854780649377" }
        //        },
        //    };

        //    // Simulate async response
        //    return Task.FromResult(result);
        //}

        //public List<SelectListItem> GetCashReconciliationAccountsAsync(string branchId, string accountType)
        //{
        //    if (string.IsNullOrWhiteSpace(branchId))
        //        throw new ArgumentException("BranchId is required", nameof(branchId));

        //    var treasury = new List<AccountDetailDto>
        //{
        //    new AccountDetailDto { AccountNumber = "571020001000", AccountName = "CASH IN HAND TILL 1", AccountId = "185017120640895" },
        //    new AccountDetailDto { AccountNumber = "560810000000", AccountName = "CCC SAVINGS ACCOUNTS", AccountId = "378663546416125" },
        //    new AccountDetailDto { AccountNumber = "560320009000", AccountName = "SCB CURRENT ACCOUNTS", AccountId = "781320160686593" },
        //    new AccountDetailDto { AccountNumber = "560810002000", AccountName = "EXPRESS UNION SAVINGS", AccountId = "931321352267142" },
        //    new AccountDetailDto { AccountNumber = "571020000000", AccountName = "CASH IN HAND", AccountId = "949938076276909" },
        //    new AccountDetailDto { AccountNumber = "560320000000", AccountName = "NFC WESTERN UNION OPS A/C", AccountId = "971854780649377" }
        //};

        //    var deficitAccounts = new List<AccountDetailDto>
        //    {
        //     new AccountDetailDto { AccountNumber = "571020001000", AccountName = "CASH IN HAND TILL 1", AccountId = "185017120640895" },
        //    new AccountDetailDto { AccountNumber = "560810000000", AccountName = "CCC SAVINGS ACCOUNTS", AccountId = "378663546416125" },
        //    new AccountDetailDto { AccountNumber = "560320009000", AccountName = "SCB CURRENT ACCOUNTS", AccountId = "781320160686593" },
        //    new AccountDetailDto { AccountNumber = "560810002000", AccountName = "EXPRESS UNION SAVINGS", AccountId = "931321352267142" },
        //    new AccountDetailDto { AccountNumber = "571020000000", AccountName = "CASH IN HAND", AccountId = "949938076276909" },
        //    new AccountDetailDto { AccountNumber = "560320000000", AccountName = "NFC WESTERN UNION OPS A/C", AccountId = "971854780649377" }
        //    };

        //    // classic if/else for C# 7.3
        //    List<AccountDetailDto> accounts;
        //    if (string.Equals(accountType, "treasury", StringComparison.OrdinalIgnoreCase))
        //    {
        //        accounts = treasury;
        //    }
        //    else if (string.Equals(accountType, "deficit", StringComparison.OrdinalIgnoreCase))
        //    {
        //        accounts = deficitAccounts;
        //    }
        //    else
        //    {
        //        accounts = new List<AccountDetailDto>();
        //    }

        //    var dropdownItems = accounts
        //        .OrderBy(a => a.AccountName)
        //        .Select(a => new SelectListItem
        //        {
        //            Text = string.Format("{0} ({1})", a.AccountName, a.AccountNumber),
        //            Value = a.AccountId
        //        })
        //        .ToList();

        //    dropdownItems.Insert(0, new SelectListItem { Text = "--- Select ---", Value = "" });

        //    return dropdownItems;
        //}


        //public async Task<Dictionary<string, List<SelectListItem>>> GetCashReconciliationAccountsAsync(string branchId, string accountType)
        //{
        //    if (string.IsNullOrWhiteSpace(branchId))
        //        throw new ArgumentException("BranchId is required", nameof(branchId));

        //    try
        //    {
        //        // Call the API for treasury accounts
        //        var treasuryResponse = await _CashapiCallerHelper.GetAsync<ResponseObject<List<AccountGroupDto>>>(
        //            string.Format(APICallHelper.GetCashReconciliationAccounts, branchId)
        //        );

        //        var treasuryAccounts = treasuryResponse?.ApiResponseData?.Data?
        //            .Where(a => a.AccountType.Equals("treasury", StringComparison.OrdinalIgnoreCase))
        //            .SelectMany(g => g.Treasury)
        //            .OrderBy(a => a.AccountName)
        //            .Select(a => new SelectListItem
        //            {
        //                Text = $"{a.AccountName} ({a.AccountNumber})",
        //                Value = a.AccountId
        //            })
        //            .ToList() ?? new List<SelectListItem>();

        //        treasuryAccounts.Insert(0, new SelectListItem { Text = "--- Select ---", Value = "" });

        //        // Call the API for deficit accounts
        //        var deficitAccounts = treasuryResponse?.ApiResponseData?.Data?
        //            .Where(a => a.AccountType.Equals("deficit", StringComparison.OrdinalIgnoreCase))
        //            .SelectMany(g => g.DeficitAccounts)
        //            .OrderBy(a => a.AccountName)
        //            .Select(a => new SelectListItem
        //            {
        //                Text = $"{a.AccountName} ({a.AccountNumber})",
        //                Value = a.AccountId
        //            })
        //            .ToList() ?? new List<SelectListItem>();

        //        deficitAccounts.Insert(0, new SelectListItem { Text = "--- Select ---", Value = "" });

        //        return new Dictionary<string, List<SelectListItem>>
        //{
        //    { "treasury", treasuryAccounts },
        //    { "deficit", deficitAccounts }
        //};
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error fetching cash reconciliation accounts.", ex);
        //    }
        //}


        public decimal GetAccountBalance(string accountId)
        {
            var balances = new Dictionary<string, decimal>
        {
            { "185017120640895", 120000.50m },
            { "378663546416125", 54000.75m },
            { "781320160686593", 7800.00m },
            { "931321352267142", 45000.25m },
            { "949938076276909", 100000.00m },
            { "971854780649377", 2500.10m }
        };

            return balances.ContainsKey(accountId) ? balances[accountId] : 0m;
        }

        public async Task<ExecutionMessages> Create(CashAndVaultInit model)
        {
            try
            {
                var response = await _CashapiCallerHelper.PostAsync<ServiceResponse<CashAndVaultInit>>(APICallHelper.AddOrUpdateAndriodVersion, model);
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
