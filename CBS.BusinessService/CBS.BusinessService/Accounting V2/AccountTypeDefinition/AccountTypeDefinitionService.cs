using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountTypeDefinition;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.AccountTypeDefinition
{
    public class AccountTypeDefinitionService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public AccountTypeDefinitionService()
        {
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<AccountTypeDefinitionDto>> GetAllAsync(bool includeInactive = false)
        {
            try
            {
                string url = $"{APICallHelper.GetAllAccountTypeDefinition}?includeInactive={includeInactive}";
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<AccountTypeDefinitionDto>>>(url);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountTypeDefinitionDto>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public async Task<CustomDataTable> GetDataTableAsync(AccountTypeDefinitionQuery query)
        //{
        //    try
        //    {
        //        var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
        //            APICallHelper.GetAllAccountTypeDefinition, query);

        //        if (!response.IsSuccess)
        //        {
        //            throw new Exception($"API call failed: {response.Message}");
        //        }

        //        if (response.ApiResponseData == null)
        //        {
        //            throw new Exception("API returned null data");
        //        }

        //        return response.ApiResponseData.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
        //        throw new Exception($"Account Type Definition service unavailable: {ex.Message}", ex);
        //    }
        //}

        public async Task<AccountTypeDefinitionDto> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                string formattedUrl = string.Format(APICallHelper.GetAccountTypeDefinitionById, id);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<AccountTypeDefinitionDto>>(formattedUrl);

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

        public async Task<IEnumerable<AccountTypeDefinitionDto>> GetByGroupIdAsync(string groupId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new ArgumentException("groupId is required", nameof(groupId));

                string formattedUrl = string.Format(APICallHelper.GetByGroupId, groupId);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<AccountTypeDefinitionDto>>>(formattedUrl);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountTypeDefinitionDto>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<AccountTypeDefinitionDto>> GetTreeByGroupIdAsync(string groupId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new ArgumentException("groupId is required", nameof(groupId));

                string formattedUrl = string.Format(APICallHelper.GetTreeByGroupId, groupId);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<AccountTypeDefinitionDto>>>(formattedUrl);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountTypeDefinitionDto>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<AccountTypeDefinitionDto>> GetChildrenByParentIdAsync(string parentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentId))
                    throw new ArgumentException("parentId is required", nameof(parentId));

                string formattedUrl = string.Format(APICallHelper.GetChildrenByParentId, parentId);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<AccountTypeDefinitionDto>>>(formattedUrl);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<AccountTypeDefinitionDto>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> CreateAsync(AccountTypeDefinitionCommand model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<AccountTypeDefinitionDto>>(
                    APICallHelper.CreateAccountTypeDefinition, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
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

        public async Task<ExecutionMessages> UpdateAsync(AccountTypeDefinitionCommand model)
        {
            try
            {
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<AccountTypeDefinitionDto>>(
                    APICallHelper.UpdateAccountTypeDefinition, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message);
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

        public async Task<ExecutionMessages> DeleteAsync(string id)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeleteAccountTypeDefinition, id);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Account Type Definition ID: {id}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Account Type Definition deleted successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Account Type Definition ID: {id}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to delete Account Type Definition.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Account Type Definition ID: {id}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<IEnumerable<AccountTypeDefinitionDto>> GetDropdownDataAsync(bool activeOnly = true)
        {
            try
            {
                var accountTypes = await GetAllAsync(!activeOnly);

                return accountTypes
                    .Where(at => !activeOnly || at.IsActive)
                    .OrderBy(at => at.DisplayOrder)
                    .ThenBy(at => at.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<AccountTypeDefinitionDto>> GetAccountTypeDefinitionAsync()
        {
            try
            {
               
                var includeInactive = true;
                string formattedUrl = string.Format(APICallHelper.GetAllAccountTypeDefinition, includeInactive);

                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<AccountTypeDefinitionDto>>>(formattedUrl);
                var AccountTypeDefinition = response?.ApiResponseData?.Data ?? new List<AccountTypeDefinitionDto>();

                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    AccountTypeDefinition = AccountTypeDefinition.Where(a => a.Id == currentBranchId).ToList();
                }
                else
                {
                    var defaultAffiliate = new AccountTypeDefinitionDto
                    {
                        Id = "All",
                        Name = "All AccountTypeDefinition",
                        Code = "ALL",
                        IsActive = true
                    };
                    AccountTypeDefinition.Insert(0, defaultAffiliate);
                }

                // Optional: format name for display and order by Code
                return AccountTypeDefinition
                    .Select(a =>
                    {
                        a.Name = $"[{a.Code}] - {a.Name} ";
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