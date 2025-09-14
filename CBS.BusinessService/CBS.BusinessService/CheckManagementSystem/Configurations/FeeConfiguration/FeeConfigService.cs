//using BusinessServices;
//using CBS.API.Helper;
//using CBS.FrontDesk.Data.Entity;
//using CBS.FrontDesk.Data.Entity.Accounting;
//using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
//using CBS.FrontDesk.Data.Message;
//using CBS.FrontDesk.Data.UserManagement;
//using CBS.FrontDesk.Helper;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Threading.Tasks;

//namespace CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration
//{
//    public class FeeConfigService : BaseService
//    {
//        private readonly ApiCallerHelper _apiCallerHelper;

//        public FeeConfigService()
//        {
//            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
//            if (string.IsNullOrEmpty(baseUrl))
//                throw new ConfigurationErrorsException("The 'FeeServiceBaseUrl' appSetting is missing or empty in Web.config.");

//            _apiCallerHelper = new ApiCallerHelper(baseUrl);
//        }

//        /// <summary>
//        /// Get list of FeeConfigs. Supports filters by feeType, branchId, centralized flag.
//        /// </summary>
//        public async Task<IEnumerable<FeeConfig>> GetConfigsAsync(string feeType = null, string branchId = null, bool? centralized = null)
//        {
//            try
//            {
//                var queryParts = new List<string>();
//                if (!string.IsNullOrEmpty(feeType)) queryParts.Add($"feeType={Uri.EscapeDataString(feeType)}");
//                if (!string.IsNullOrEmpty(branchId)) queryParts.Add($"branchId={Uri.EscapeDataString(branchId)}");
//                if (centralized.HasValue) queryParts.Add($"centralized={centralized.Value.ToString().ToLower()}");

//                var url = APICallHelper.GetAllFeeConfigs + (queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty);

//                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<FeeConfig>>>(url);

//                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
//                    return response.ApiResponseData.Data;

//                return Enumerable.Empty<FeeConfig>();
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }

//        /// <summary>
//        /// Get FeeConfigs by FeeType only.
//        /// </summary>
//        public async Task<IEnumerable<FeeConfig>> GetByFeeTypeAsync(string feeType)
//        {
//            return await GetConfigsAsync(feeType: feeType);
//        }

//        /// <summary>
//        /// Get FeeConfigs by BranchId + FeeType.
//        /// </summary>
//        public async Task<IEnumerable<FeeConfig>> GetByBranchAndFeeTypeAsync(string branchId, string feeType)
//        {
//            return await GetConfigsAsync(feeType: feeType, branchId: branchId);
//        }

//        /// <summary>
//        /// Get list of available FeeTypes (enum values)
//        /// </summary>
//        public async Task<IEnumerable<object>> GetFeeTypesAsync()
//        {
//            try
//            {
//                var url = APICallHelper.GetFeeTypes; // make sure this constant points to backend endpoint
//                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<StringValues>>>(url);

//                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
//                    return response.ApiResponseData.Data;

//                return Enumerable.Empty<object>();
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }

//        ////***************************************** end of real methods *****************************************
//        ///***************************************** MOCK methods *****************************************

//        public async Task<IEnumerable<FeeConfig>> GetAllConfigsAsSummaryAsync()
//        {
//            // We will work with the existing mock data for now.
//            // This simulates fetching a summarized list from the backend.
//            var summaryList = _mockFeeConfigs.Select(c => new FeeConfig
//            {
//                Id = c.Id,
//                // Use the BranchName if it's a branch-specific config
//               // Scope = c.IsCentralized ? "Centralized" : (c.BranchName ?? "Branch Specific"),
//                FeeType = c.FeeType,
//                Description = c.Description,
//                IsActive = c.IsActive
//            }).ToList();

//            // Simulate an async operation and return the result.
//            return await Task.FromResult(summaryList);
//        }

//        /// <summary>
//        /// // MOCK: Get list of available FeeTypes (enum values)
//        /// </summary>
//        /// <returns></returns>
//        public async Task<List<StringValues>> GetFeeTypesMockAsync()
//        {
//            try
//            {
//                //var url = APICallHelper.GetFeeTypes; // make sure this constant points to backend endpoint
//                //var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<StringValues>>>(url);

//                var Data = new List<StringValues>
//                        {
//                            new StringValues { Text = "CashIn", Value = "CashIn" },
//                            new StringValues { Text = "CashOut", Value = "CashOut" },
//                            new StringValues { Text = "CheckFee", Value = "CheckFee" },
//                            new StringValues { Text = "InterBranch", Value = "InterBranch" }
//                        };


//                var response = ServiceResponse<List<StringValues>>.ReturnResultWith200(Data, "Success");

//                if (response != null && response.Success && response.Data != null)
//                    return response.Data;

//                return new List<StringValues>();
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }

//        private static readonly List<FeeConfig> _mockFeeConfigs = new List<FeeConfig>
//        {
//            // Mock object 1: Centralized CashIn
//            new FeeConfig
//            {
//                Id = "FeeConfig961205778227",
//                IsCentralized = true,
//                BranchId = null,
//                BranchName = "Central Branch",
//                Description = "Configuration centralisée",
//                FeeType = "CashIn",
//                AcceptPercentage = true,
//                PercentageApplied = 2.0,
//                AcceptRange = false,
//                IsActive = true,
//                FeeTypeRanges = new List<Range>
//                {
//                    new Range { FromAmount = 0, ToAmount = 1000, Fee = 10 }
//                }
//            },
//            // Mock object 2: Branch-specific CashOut
//            new FeeConfig
//            {
//                Id = "FeeConfig731204065348",
//                IsCentralized = false,
//                BranchId = "BR123",
//                BranchName = "Main Branch",
//                Description = "Configuration par branche",
//                FeeType = "CashOut",
//                AcceptPercentage = false,
//                PercentageApplied = null,
//                AcceptRange = true,
//                IsActive = true,
//                FeeTypeRanges = new List<Range>
//                {
//                    new Range { FromAmount = 0, ToAmount = 500, Fee = 5 },
//                    new Range { FromAmount = 501, ToAmount = 2000, Fee = 15 }
//                }
//            },
//            // Mock object 3: Centralized CheckFee
//            new FeeConfig
//            {
//                Id = "FeeConfigABCDEFGHIJKL",
//                IsCentralized = true,
//                BranchId = null,
//                BranchName = "Central Branch",
//                Description = "Frais de carnet de chèques",
//                FeeType = "CheckFee",
//                AcceptPercentage = false,
//                PercentageApplied = null,
//                AcceptRange = true,
//                IsActive = true,
//                FeeTypeRanges = new List<Range>
//                {
//                    new Range { FromAmount = 0, ToAmount = 10000, Fee = 2500 },
//                    new Range { FromAmount = 10001, ToAmount = 50000, Fee = 5000 }
//                }
//            }
//        };

//        public async Task<IEnumerable<FeeConfig>> GetConfigsMockAsync(string feeType = null, string branchId = null, bool? centralized = null)
//        {
//            // Start with the full list of mock data.
//            IEnumerable<FeeConfig> query = _mockFeeConfigs;

//            // Apply filters one by one, just like the backend would.
//            if (centralized.HasValue)
//            {
//                query = query.Where(c => c.IsCentralized == centralized.Value);
//            }

//            if (!string.IsNullOrEmpty(feeType))
//            {
//                query = query.Where(c => c.FeeType.Equals(feeType, StringComparison.OrdinalIgnoreCase));
//            }

//            if (!string.IsNullOrEmpty(branchId))
//            {
//                query = query.Where(c => c.BranchId == branchId);
//            }

//            // Return the filtered list.
//            return await Task.FromResult(query.ToList());
//        }

//        /// <summary>
//        /// MOCK: Gets a single FeeConfig by its ID.
//        /// </summary>
//        public async Task<FeeConfig> GetByIdMockAsync(string id)
//        {
//            if (string.IsNullOrWhiteSpace(id)) return null;

//            var config = _mockFeeConfigs.FirstOrDefault(c => c.Id == id);

//            return await Task.FromResult(config);
//        }

//        /// <summary>
//        /// MOCK: Simulates creating a new FeeConfig.
//        /// </summary>
//        public async Task<ExecutionMessages> CreateMockAsync(FeeConfig model)
//        {
//            // Simulate creating a new ID and adding to our "database".
//            model.Id = "FeeConfig" + new Random().Next(1000, 9999);
//            _mockFeeConfigs.Add(model);

//            GetExecutionMessages(model, true, "FeeConfig", MessagesResults.Success, ExecutionProcessOption.InsertObject, "Success");
//            return await Task.FromResult(ExecutionMessage);
//        }

//        /// <summary>
//        /// MOCK: Simulates updating an existing FeeConfig.
//        /// </summary>
//        // In FeeConfigService.cs

//        public async Task<ExecutionMessages> UpdateMockAsync(FeeConfig model)
//        {
//            var existing = _mockFeeConfigs.FirstOrDefault(c => c.Id == model.Id);
//            if (existing != null)
//            {
//                // ...
//                GetExecutionMessages(model, true, "FeeConfig", MessagesResults.Success, ExecutionProcessOption.UpdateUpject, "Success");
//            }
//            else
//            {
//                // FIX APPLIED HERE: The last parameter is likely 'responseMessage', not 'message'.
//                GetExecutionMessages(model, false, "FeeConfig", MessagesResults.Failed,
//                    ExecutionProcessOption.UpdateUpject, "Failed", null, "Item not found.");
//            }
//            return await Task.FromResult(ExecutionMessage);
//        }


//        /// <summary>
//        /// MOCK: Simulates deleting a FeeConfig.
//        /// </summary>
//        // In FeeConfigService.cs

//        // In FeeConfigService.cs

//        // In FeeConfigService.cs

//        // In FeeConfigService.cs

//        public async Task<ExecutionMessages> DeleteMockAsync(string id)
//        {
//            var itemToRemove = _mockFeeConfigs.FirstOrDefault(c => c.Id == id);
//            if (itemToRemove != null)
//            {
//                _mockFeeConfigs.Remove(itemToRemove);

//                // This call was likely correct already, but let's ensure it has all 8 parameters for consistency.
//                GetExecutionMessages(null, true, $"FeeConfig:{id}", MessagesResults.Success,
//                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, "Deleted successfully.");
//            }
//            else
//            {
//                // --- THIS IS THE FIX ---
//                // We now provide all 8 parameters in the correct order, matching the working example.
//                GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Failed,
//                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
//            }
//            return await Task.FromResult(ExecutionMessage);
//        }
/////***************************************** end of MOCK methods *****************************************

///// <summary>
///// Get a single FeeConfig by id
///// </summary>
//public async Task<FeeConfig> GetByIdAsync(string id)
//                {
//                    try
//                    {
//                        if (string.IsNullOrWhiteSpace(id)) return null;

//                        string url = string.Format(APICallHelper.GetFeeConfigById, id);
//                        var response = await _apiCallerHelper.GetAsync<ServiceResponse<FeeConfig>>(url);

//                        if (response != null && response.IsSuccess)
//                            return response.ApiResponseData?.Data;

//                        return null;
//                    }
//                    catch (Exception)
//                    {
//                        throw;
//                    }
//                 }

//        /// <summary>
//        /// Create a new FeeConfig
//        /// </summary>
//        public async Task<ExecutionMessages> CreateAsync(FeeConfig model)
//        {
//            try
//            {
//                var response = await _apiCallerHelper.PostAsync<ServiceResponse<FeeConfig>>(APICallHelper.CreateFeeConfig, model);

//                if (response != null && response.IsSuccess)
//                {
//                    GetExecutionMessages(response.ApiResponseData?.Data, true, model?.FeeType ?? "FeeConfig", MessagesResults.Success,
//                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
//                }
//                else
//                {
//                    GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Failed,
//                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
//                }
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
//            }

//            return ExecutionMessage;
//        }

//        /// <summary>
//        /// Update existing FeeConfig (model.Id must be set)
//        /// </summary>
//        public async Task<ExecutionMessages> UpdateAsync(FeeConfig model)
//        {
//            try
//            {
//                if (model == null || string.IsNullOrWhiteSpace(model.Id))
//                {
//                    GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Failed,
//                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or Id.");
//                    return ExecutionMessage;
//                }

//                string url = string.Format(APICallHelper.UpdateFeeConfig, model.Id);
//                var response = await _apiCallerHelper.PutAsync<ServiceResponse<FeeConfig>>(url, model);

//                if (response != null && response.IsSuccess)
//                {
//                    GetExecutionMessages(response.ApiResponseData?.Data, true, model.FeeType, MessagesResults.Success,
//                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
//                }
//                else
//                {
//                    GetExecutionMessages(model, false, model.FeeType, MessagesResults.Failed,
//                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
//                }
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
//            }
//            return ExecutionMessage;
//        }

//        /// <summary>
//        /// Deactivate (soft-delete) a FeeConfig by id
//        /// </summary>
//        public async Task<ExecutionMessages> DeleteAsync(string id)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(id))
//                {
//                    GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Failed,
//                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Invalid id.");
//                    return ExecutionMessage;
//                }

//                string url = string.Format(APICallHelper.DeleteFeeConfig, id);
//                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(url);

//                if (response != null && response.IsSuccess)
//                {
//                    GetExecutionMessages(null, true, $"FeeConfig:{id}", MessagesResults.Success,
//                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message ?? "Deleted");
//                }
//                else
//                {
//                    GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Failed,
//                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
//                }
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
//            }
//            return ExecutionMessage;
//        }
//    }
//}


// -----------------------------------------------------------------------------
// File: FeeConfigService.cs (production - consumes API endpoints)
// -----------------------------------------------------------------------------
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration
{
    public class FeeConfigService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public FeeConfigService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");

            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<FeeConfig>> GetConfigsAsync(string feeType = null, string branchId = null, bool? centralized = null)
        {
            try
            {
                var queryParts = new List<string>();
                if (!string.IsNullOrEmpty(feeType)) queryParts.Add($"feeType={Uri.EscapeDataString(feeType)}");
                if (!string.IsNullOrEmpty(branchId)) queryParts.Add($"branchId={Uri.EscapeDataString(branchId)}");
                if (centralized.HasValue) queryParts.Add($"centralized={centralized.Value.ToString().ToLower()}");

                var url = APICallHelper.GetAllFeeConfigs + (queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<FeeConfig>>>(url);

                if (response != null && (response.IsSuccess) && response.ApiResponseData?.Data != null)
                    return response.ApiResponseData.Data;

                return Enumerable.Empty<FeeConfig>();
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
                throw;
            }
        }

        public async Task<IEnumerable<StringValues>> GetFeeTypesAsync()
        {
            try
            {
                var url = APICallHelper.GetFeeTypes;
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<StringValues>>>(url);
                if (response != null && (response.IsSuccess) && response.ApiResponseData?.Data != null)
                    return response.ApiResponseData.Data;
                return Enumerable.Empty<StringValues>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FeeConfig> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) return null;
                string url = string.Format(APICallHelper.GetFeeConfigById, id);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<FeeConfig>>(url);
                if (response != null && (response.IsSuccess ))
                    return response.ApiResponseData?.Data;
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> CreateAsync(FeeConfig model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<FeeConfig>>(APICallHelper.CreateFeeConfig, model);
                if (response != null && (response.IsSuccess))
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model?.FeeType ?? "FeeConfig", MessagesResults.Success, ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Failed, ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(FeeConfig model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Id))
                {
                    GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or Id.");
                    return ExecutionMessage;
                }

                string url = string.Format(APICallHelper.UpdateFeeConfig, model.Id);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<FeeConfig>>(url, model);
                if (response != null && (response.IsSuccess))
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model.FeeType, MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.FeeType, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model?.FeeType ?? "FeeConfig", MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Invalid id.");
                    return ExecutionMessage;
                }

                string url = string.Format(APICallHelper.DeleteFeeConfig, id);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(url);
                if (response != null && (response.IsSuccess))
                {
                    GetExecutionMessages(null, true, $"FeeConfig:{id}", MessagesResults.Success, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message ?? "Deleted");
                }
                else
                {
                    GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        //#region Mock methods placeholder (delegates to a mock service when needed)
        //// You can implement these by delegating to MockFeeConfigService in development environment.
        //public Task<IEnumerable<FeeConfig>> GetConfigsMockAsync(string feeType = null, string branchId = null, bool? centralized = null)
        //{
        //    var mock = new MockFeeConfigService();
        //    return mock.GetConfigsMockAsync(feeType, branchId, centralized);
        //}

        //public Task<IEnumerable<FeeConfig>> GetAllConfigsAsSummaryAsync() => new MockFeeConfigService().GetAllConfigsAsSummaryAsync();
        //public Task<List<StringValues>> GetFeeTypesMockAsync() => new MockFeeConfigService().GetFeeTypesMockAsync();
        //public Task<FeeConfig> GetByIdMockAsync(string id) => new MockFeeConfigService().GetByIdMockAsync(id);
        //public Task<ExecutionMessages> CreateMockAsync(FeeConfig model) => new MockFeeConfigService().CreateMockAsync(model);
        //public Task<ExecutionMessages> UpdateMockAsync(FeeConfig model) => new MockFeeConfigService().UpdateMockAsync(model);
        //public Task<ExecutionMessages> DeleteMockAsync(string id) => new MockFeeConfigService().DeleteMockAsync(id);
        //#endregion
    }
}




