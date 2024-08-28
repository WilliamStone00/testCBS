using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingDayObject
{
    public class AccountingDayServices : BaseService
    {
        private readonly ApiCallerHelper _transactionBaseConfigApiHelper;
        private readonly BranchServices _branchServices;

        public AccountingDayServices(BranchServices branchServices = null)
        {
            _transactionBaseConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = branchServices;
        }
        public async Task<AccountingDay> GetAccountingDay(string id)
        {
            try
            {


                var cusResponseObject = await _transactionBaseConfigApiHelper.GetAsync<ResponseObject<AccountingDay>>(string.Format(APICallHelper.GetAccountingDayById, id));
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
        public async Task<ExecutionMessages> RemoveAccountingDay(string id)
        {
            try
            {
                var inResponse = await _transactionBaseConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_DepositLimit, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<AccountingDay>> GetAccountingDayQueries(GetAccountingDayQuery getAccountingDay)
        {
            try
            {

                if (getAccountingDay.BranchId==null)
                {
                    getAccountingDay.BranchId = GetBranchID();
                }
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<AccountingDay>>>(APICallHelper.GetAllOpenedAccountingDays, getAccountingDay);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var accountingDays = couApiResponse.ApiResponseData.Data;

                    if (IsHeadOffice())
                    {
                        var branches = await _branchServices.GetBranches();

                        // Join accounting days with branches, allowing for null BranchId
                        var accountings = from accountingDay in accountingDays
                                          join branch in branches on accountingDay.BranchId equals branch.Id into accountingWithBranch
                                          from branch in accountingWithBranch.DefaultIfEmpty() // This allows for null BranchId
                                          select MapDailyTellerWithBranch(accountingDay, branch);

                        return accountings;
                    }
                    else
                    {
                        var branch = await _branchServices.GetBranch(GetBranchID());

                        // Filter accountingDays where BranchId matches or is null
                        var accountings = accountingDays
                                          .Where(x => x.BranchId == branch?.Id || x.BranchId == null)
                                          .Select(accountingDay => MapDailyTellerWithBranch(accountingDay, branch));

                        return accountings;
                    }
                }


                // Return an empty collection if no results are found or the API call fails
                return Enumerable.Empty<AccountingDay>();
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here) and rethrow it
                throw;
            }
        }
        private AccountingDay MapDailyTellerWithBranch(AccountingDay accountingDay, Branch branch)
        {
            // Initialize the mapping with default values in case the branch is null
            var data = new AccountingDay
            {
                Id = accountingDay.Id,
                BranchId = branch?.Id ?? accountingDay.BranchId, // Use the branch's Id if available; otherwise, use the existing BranchId
                BranchCode = branch?.BranchCode ?? "N/A", // Default to "N/A" if branch code is not available
                BranchName = branch?.Name ?? "Centralised System", // Default to "Centralised System" if branch name is not available
                Date = accountingDay.Date,
                IsClosed = accountingDay.IsClosed,
                ClosedBy = accountingDay.ClosedBy,
                OpenedBy = accountingDay.OpenedBy,
                Note = accountingDay.Note,
                ReOpenedDate = accountingDay.ReOpenedDate,
                ClosedAt = accountingDay.ClosedAt,
                OpenedAt = accountingDay.OpenedAt,
                StrClosedAt = accountingDay.ClosedAt.HasValue ? accountingDay.ClosedAt.Value.ToString("dd-MM-yyyy, hh:mm:ss") : "N/A", // Format if value exists; otherwise, default to "N/A"
                StrDate = accountingDay.Date.ToString("dd-MM-yyyy"),
                StrOpenedAt = accountingDay.OpenedAt.HasValue ? accountingDay.OpenedAt.Value.ToString("dd-MM-yyyy, hh:mm:ss") : "N/A", // Format if value exists; otherwise, default to "N/A"
                IsCentralized = accountingDay.IsCentralized
            };

            return data;
        }

        public async Task<List<AccountingDayDS>> GetAccountingDayDsAsync(List<AccountingDay> accountingDays)
        {
            // Initialize an empty list to hold the mapped AccountingDayDS objects
            var accountingDayDSList = new List<AccountingDayDS>();
            Branch branch = await _branchServices.GetBranch(GetBranchID()); 
            // Iterate over each AccountingDay in the list
            foreach (var accountingDay in accountingDays)
            {

                // Map the AccountingDay to AccountingDayDS
                var data = new AccountingDayDS
                {
                    Id = accountingDay.Id,
                    BranchId = accountingDay.BranchId, // Use the branch's Id if available; otherwise, use the existing BranchId
                    BranchCode = accountingDay.BranchCode ?? "N/A", // Default to "N/A" if branch code is not available
                    BranchName = accountingDay.BranchName ?? "Centralised System", // Default to "Centralised System" if branch name is not available
                    Date = accountingDay.Date,
                    IsClosed = accountingDay.IsClosed,
                    ClosedBy = accountingDay.ClosedBy,
                    OpenedBy = accountingDay.OpenedBy,
                    Note = accountingDay.Note,
                    ReOpenedDate = accountingDay.ReOpenedDate,
                    ClosedAt = accountingDay.ClosedAt.Value,
                    OpenedAt = accountingDay.OpenedAt.Value,
                    IsCentralized = accountingDay.IsCentralized,

                    HeadOfficeName = branch.Bank.Name,
                    HeadOfficeAddress = branch.Bank.Address,
                    HeadOfficeTelephone = branch.Bank.Telephone,
                    HeadOfficeEmail = branch.Bank.Email,
                    HeadOfficeWebSite = branch.Bank.WebSite,
                    HeadOfficeInitial = branch.Bank.BankInitial,
                    HeadOfficeCode = branch.Bank.BankCode,
                    Logo = branch.Bank.LogoUrl,
                    BranchAddress = branch.Address,
                    BranchTelephone = branch.Telephone 
                };

                accountingDayDSList.Add(data);
            }
           
            return accountingDayDSList;
        }

        public async Task<ExecutionMessages> HandleAccountingDayAsync(OpenOrCloseOfAccountingDayCommand model, string apiEndpoint)
        {
            try
            {
                if (!IsHeadOffice())
                {
                    model.Branches = new List<BranchListing>
            {
                new BranchListing
                {
                    BranchCode = GetBankCode(),
                    BranchId = GetBranchID(),
                    BranchName = GetBranchName()
                }
            };
                }

                if (model.IsCentraliseOpening)
                {
                    model.Branches = new List<BranchListing>();
                }

                // Filter out branches with null BranchId
                if (model.Branches != null)
                {
                    model.Branches = model.Branches.Where(b => b.BranchId != null).ToList();
                }

                var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<List<OpenOrCloseOfAccountingDayResultDto>>>(apiEndpoint, model);

                if (response.ApiResponseData != null)
                {
                    // Successful operation
                    GetExecutionMessages(response, true, null, MessagesResults.Success, ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    // Failed operation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
        public Task<ExecutionMessages> OpenOfAccountingDay(OpenOrCloseOfAccountingDayCommand model)
        {
            return HandleAccountingDayAsync(model, APICallHelper.OpenOfAccountingDay);
        }
        public Task<ExecutionMessages> CloseOfAccountingDay(OpenOrCloseOfAccountingDayCommand model)
        {
            return HandleAccountingDayAsync(model, APICallHelper.CloseOfAccountingDay);
        }

        public async Task<ExecutionMessages> AccountingDayActions(string id, string Option)
        {
            try
            {
                var model = new AccountingDayActionsCommand { Id = id, Option = Option };

                var response = await _transactionBaseConfigApiHelper.PostAsync<ServiceResponse<OpenOrCloseOfAccountingDayResultDto>>(APICallHelper.AccountingDayActionsCommand, model);

                if (response.ApiResponseData != null)
                {
                    // Successful operation
                    GetExecutionMessages(response, true, null, MessagesResults.Success, ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    // Failed operation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
