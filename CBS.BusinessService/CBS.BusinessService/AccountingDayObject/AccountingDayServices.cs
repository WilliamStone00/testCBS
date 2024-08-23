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

        public async Task<IEnumerable<AccountingDay>> GetAccountingDayQueries(GetAccountingDayQuery getAccountingDay)
        {
            try
            {
                // Determine if the request is from the head office or a branch
                if (IsHeadOffice())
                {
                    getAccountingDay.QueryParameter = "All";
                }
                else
                {
                    getAccountingDay.ByBranch = true;
                    getAccountingDay.QueryParameter = GetBranchID();
                }

                // Fetch all opened accounting days based on the query parameters
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<List<AccountingDay>>>(APICallHelper.GetAllOpenedAccountingDays, getAccountingDay);

                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var accountingDays = couApiResponse.ApiResponseData.Data;

                    if (IsHeadOffice())
                    {
                        var branches = await _branchServices.GetBranches();
                        var accountings = from branch in branches
                                          join accountingDay in accountingDays on branch.Id equals accountingDay.BranchId
                                          select MapDailyTellerWithBranch(accountingDay, branch);

                        return accountings;
                    }
                    else
                    {
                        var branch = await _branchServices.GetBranch(GetBranchID());
                        var accountings = accountingDays
                                          .Where(x => x.BranchId == branch.Id)
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
            return new AccountingDay
            {
                Id = accountingDay.Id,
                BranchId = branch.Id, // Use the branch's Id
                BranchCode = branch.BranchCode, // Map the branch code
                BranchName = branch.Name, // Map the branch name
                Date = accountingDay.Date,
                IsClosed = accountingDay.IsClosed,
                ClosedBy = accountingDay.ClosedBy,
                OpenedBy = accountingDay.OpenedBy,
                ClosedAt = accountingDay.ClosedAt,
                OpenedAt = accountingDay.OpenedAt,
                IsCentralized = accountingDay.IsCentralized,
            };
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

    }

}
