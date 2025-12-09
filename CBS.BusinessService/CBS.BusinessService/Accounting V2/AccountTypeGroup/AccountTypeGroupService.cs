// AccountTypeGroupService.cs
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountTypeGroup;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.AccountTypeGroup
{
    public class AccountTypeGroupService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public AccountTypeGroupService()
        {
            string baseUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'TransactionBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<AccountTypeGroupResponse>> GetAsync()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<AccountTypeGroupResponse>>>(APICallHelper.GetAllAccountTypeGroups2);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountTypeGroupResponse>();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw;
            }
        }

        public async Task<AccountTypeGroupResponse> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetAccountTypeGroupById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<AccountTypeGroupResponse>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> CreateAsync(AccountTypeGroupResponse model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<object>>(APICallHelper.CreateAccountTypeGroup, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model.Name, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "Account Type Group created successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(AccountTypeGroupResponse model)
        {
            try
            {
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<AccountTypeGroupResponse>>(APICallHelper.UpdateAccountTypeGroup, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model.Name, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "Account Type Group updated successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteAsync(string id)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeleteAccountTypeGroup, id);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"ID: {id}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Account Type Group deleted successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"ID: {id}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to delete Account Type Group.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"ID: {id}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<IEnumerable<AccountTypeGroupResponse>> GetAllAccountTypeGroupsAsync()
        {
            try
            {
               string formattedUrl = string.Format(APICallHelper.GetAllAccountTypeGroups2);

                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<AccountTypeGroupResponse>>>(formattedUrl);
                var affiliates = response?.ApiResponseData?.Data ?? new List<AccountTypeGroupResponse>();

                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    affiliates = affiliates.Where(a => a.Id == currentBranchId).ToList();
                }
                else
                {
                    var defaultAffiliate = new AccountTypeGroupResponse
                    {
                        Id = "All",
                        Name = "All Affiliates",
                        Code = "ALL",
                        IsActive = true
                    };
                    affiliates.Insert(0, defaultAffiliate);
                }

                // Optional: format name for display and order by Code
                return affiliates
                    .Select(a =>
                    {
                        a.Name = $"[{a.Code}] - {a.Name}";
                        return a;
                    })
                    .OrderBy(a => a.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}