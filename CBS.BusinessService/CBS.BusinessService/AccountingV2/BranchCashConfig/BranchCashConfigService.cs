using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.AccountingV2.BranchCashConfigV
{
    public class BranchCashConfigService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly BranchAccountService _branchAccountService;

        public BranchCashConfigService()
        {
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _branchAccountService = new BranchAccountService();
        }

        public async Task<IEnumerable<BranchCashConfig>> GetBranchCashConfigsAsync()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<BranchCashConfig>>>(APICallHelper.GetAllBranchCashConfigs);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<BranchCashConfig>();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw;
            }
        }

        public async Task<BranchCashConfig> GetBranchCashConfigByIdAsync(string id)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.GetBranchCashConfigById, id);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<BranchCashConfig>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw;
            }
        }

        // Removed the commented-out GetBranchCashConfigByBranchIdAsync

        // This is the functional GetBranchCashConfigByBranchIdAsync (uncommented in your original submission)
        public async Task<BranchCashConfig> GetBranchCashConfigByBranchIdAsync(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
                return null;

            try
            {
                string formattedUrl = string.Format(APICallHelper.GetBranchCashConfigByBranchId, branchId);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<BranchCashConfig>>(formattedUrl);

                return response.IsSuccess ? response.ApiResponseData?.Data : null;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw;
            }
        }

        public async Task<ExecutionMessages> CreateOrUpdateBranchCashConfigAsync(BranchCashConfig model)
        {
            try
            {

                if (string.IsNullOrEmpty(model.Id))
                {
                    // Create new
                    return await CreateBranchCashConfigAsync(model);
                }
                else
                {
                    // Update existing
                    return await UpdateBranchCashConfigAsync(model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateOrUpdateBranchCashConfigAsync: {ex.Message}");

                GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                return ExecutionMessage;
            }
        }

        // Renamed 'GetBranchCashConfigs' to 'GetAllFilteredConfigs' for clarity (since you had two methods named 'GetBranchCashConfigs')
        public async Task<IEnumerable<BranchCashConfig>> GetAllFilteredConfigs()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<BranchCashConfig>>>(APICallHelper.GetAllBranchCashConfigs);
                var configs = response?.ApiResponseData?.Data ?? new List<BranchCashConfig>();

                // Filter by branch if user is not head office
                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    configs = configs.Where(c => c.BranchId == currentBranchId).ToList();
                }

                return configs.OrderBy(c => c.BranchId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> CreateBranchCashConfigAsync(BranchCashConfig model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<BranchCashConfig>>(APICallHelper.CreateBranchCashConfig, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateBranchCashConfigAsync(BranchCashConfig model)
        {
            try
            {
                var configId = model.Id;
                string formattedUrl = string.Format(APICallHelper.UpdateBranchCashConfig, configId);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<BranchCashConfig>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateBranchCashConfigAsync(string configId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeactivateBranchCashConfig, configId);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Branch Cash Config ID: {configId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Branch cash configuration deactivated successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Branch Cash Config ID: {configId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate branch cash configuration.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Branch Cash Config ID: {configId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<CustomDataTable> GetDataTableAsync(BranchCashConfigQueryDto getAllUsersDataTableQuery)
        {
            if (!IsHeadOffice())
            {
                getAllUsersDataTableQuery.BranchId = GetBranchID();
            }

            var apiResponse = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoadDataTablePagginationForBranchCashConfigs,
                getAllUsersDataTableQuery
            );

            if (apiResponse.IsSuccess && apiResponse.ApiResponseData != null)
            {
                return apiResponse.ApiResponseData.Data;
            }

            return new CustomDataTable(
                draw: Convert.ToInt32(getAllUsersDataTableQuery.Options.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(),
                dataTableOptions: getAllUsersDataTableQuery.Options
            );
        }

        public List<BranchCashConfig> MapToBranchCashConfigDownloadDtos(IEnumerable<BranchCashConfig> configs)
        {
            // Simplified: If the DTO is the final required structure, we just select and return. 
            // Removed the redundant MapToBranchCashConfigDownloadDto helper.
            return configs.Select(config => new BranchCashConfig
            {
                BranchId = config.BranchId,
                CashInHandAccountId = config.CashInHandAccountId,
                VaultAccountId = config.VaultAccountId,
                SurplusIncomeAccountId = config.SurplusIncomeAccountId,
                RevenueAccountId = config.RevenueAccountId,
                ShortageExpenseAccountId = config.ShortageExpenseAccountId,
                RealTimeCashPosting = config.RealTimeCashPosting,
                PartnerAccountId = config.PartnerAccountId,
                CamcculAccountId = config.CamcculAccountId,
                HeadOfficeLiaisonAccountId = config.HeadOfficeLiaisonAccountId,
                FormFeeIncomeAccountId = config.FormFeeIncomeAccountId
            }).ToList();
        }

        // Removed the redundant MapToBranchCashConfigDownloadDto method

        public async Task<IEnumerable<BranchAccount>> GetBranchesAsync(string branchId)
        {
            try
            {
                var language = GetLanguage();
                // Call API using parameters (branchId + language)
                var url = $"{APICallHelper.GetAllBranch}?branchId={branchId}&lang={language}";
                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<BranchAccount>>>(url);

                var branches = response?.ApiResponseData?.Data ?? new List<BranchAccount>();

                // If user is not head office, limit list to his branch only
                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    branches = branches
                        .Where(b => b.Id == currentBranchId)
                        .ToList();
                }
                else
                {
                    // Insert default "All" entry at top of list
                    branches.Insert(0, new BranchAccount
                    {
                        Id = "All",
                        Code = "All",
                        Name = language == "fr"
                            ? "Toutes les agences"
                            : "All Branches"
                    });
                }

                // Prepare formatted names for dropdown display
                return branches
                    .Select(b => new BranchAccount
                    {
                        Id = b.Id,
                        Code = b.Code,
                        Name = $"[{b.Code}] {b.Name}"
                    })
                    .OrderBy(b => b.Code)
                    .ToList();
            }
            catch (Exception)
            {
                // You might log the exception here
                throw;
            }
        }
    }
}