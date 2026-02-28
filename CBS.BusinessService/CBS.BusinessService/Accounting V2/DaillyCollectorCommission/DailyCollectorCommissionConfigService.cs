using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.DailyCollectorCommission
{
    public class DailyCollectorCommissionConfigService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public DailyCollectorCommissionConfigService()
        {
            string baseUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'TransactionBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<List<DailyCollectorCommissionShareConfig>> GetAllConfigsAsync()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<DailyCollectorCommissionShareConfig>>>(APICallHelper.DailyCollectorCommissionDatatable);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<DailyCollectorCommissionShareConfig>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CustomDataTable> GetConfigDataTableAsync(DaillycollectorCommissionConfigQuery query)
        {
            try
            {
                if (!IsHeadOffice())
                {
                    query.BranchId = GetBranchID();
                }

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.DailyCollectorCommissionDatatable, query);

                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                throw new Exception($"Daily Collector Commission service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<DailyCollectorCommissionShareConfig> GetConfigByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.DailyCollectorCommissionGetById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<DailyCollectorCommissionShareConfig>>(formattedUrl);

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

        public async Task<ExecutionMessages> CreateConfigAsync(DailyCollectorCommissionShareConfig model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<DailyCollectorCommissionShareConfig>>(
                    APICallHelper.DailyCollectorCommissionCreate, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Commission Config", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, "Commission Config", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Commission Config", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateConfigAsync(DailyCollectorCommissionShareConfig model)
        {
            try
            {
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<DailyCollectorCommissionShareConfig>>(
                    APICallHelper.DailyCollectorCommissionUpdate, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Commission Config", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, "Commission Config", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Commission Config", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteConfigAsync(string configId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DailyCollectorCommissionDelete, configId);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Config ID: {configId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Commission config deleted successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Config ID: {configId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to delete commission config.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Config ID: {configId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }
    }
}