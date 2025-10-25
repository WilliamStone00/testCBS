// Location: ~/BusinessService/CheckManagementSystem/Configurations/NotificationConfiguration/NotificationConfigService.cs

using BusinessServices;
using CBS.API.Helper; // For ApiCallerHelper
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationConfig;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Configurations.NotificationConfiguration
{
    public class NotificationConfigService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public NotificationConfigService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        #region Live Service Methods

        /// <summary>
        /// Gets the list of all available notification type definitions from the backend API.
        /// </summary>
        //public async Task<IEnumerable<StringValues>> GetNotificationTypesAsync()
        //{
        //    try
        //    {
        //        var url = APICallHelper.GetNotificationTypes;

        //        // Deserialize into the aggregate DTO so we match the JSON shape
        //        var response = await _apiCallerHelper.GetAsync<ServiceResponse<NotificationAggregatesDto>>(url);

        //        if (response != null && response.IsSuccess && response.ApiResponseData?.Data?.NotificationTypes != null)
        //        {
        //            return response.ApiResponseData.Data.NotificationTypes;
        //        }

        //        return Enumerable.Empty<StringValues>();
        //    }
        //    catch (Exception)
        //    {
        //        // log if needed
        //        throw;
        //    }
        //}

        public async Task<NotificationAggregatesDto> GetNotificationTypesAsync()
        {
            try
            {
                var url = APICallHelper.GetNotificationTypes;
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<NotificationAggregatesDto>>(url);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }

                return new NotificationAggregatesDto(); // or null, depending on your preference
            }
            catch (Exception)
            {
                // log if needed
                throw;
            }
        }

        public async Task<CustomDataTable> GetNotificationDataTableAsync(NotificationconfigQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.notdata, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
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
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }

        //public async Task<IEnumerable<StringValues>> GetFeeTypesAsync()
        //{
        //    try
        //    {
        //        var url = APICallHelper.GetFeeTypes;
        //        var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<StringValues>>>(url);
        //        if (response != null && (response.IsSuccess) && response.ApiResponseData?.Data != null)
        //            return response.ApiResponseData.Data;
        //        return Enumerable.Empty<StringValues>();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Gets saved configurations from the API, supporting all filters.
        /// </summary>
        public async Task<IEnumerable<NotificationConfig>> GetConfigsAsync(
           bool isCentralized,
           string branchId = null,
           string notificationType = null)
        {
            try
            {
                var url = APICallHelper.VerifyNotificationConfig;

                var request = new VerifyNotificationConfigRequest
                {
                    IsCentralised = isCentralized,
                    BranchId = branchId,
                    NotificationType = notificationType
                };

                var response = await _apiCallerHelper.PostAsync<ServiceResponse<NotificationConfig>>(url, request);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    // Wrap the single config in a collection
                    return new[] { response.ApiResponseData.Data };
                }

                return Enumerable.Empty<NotificationConfig>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<NotificationConfig> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("id is required", nameof(id));

            try
            {
                // Replace the APICallHelper constant below with the actual one if different.
                // Expected form: "api/notifications/configurations/{0}"
                string url = string.Format(APICallHelper.GetNotificationConfigById, id);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<NotificationConfig>>(url);

                // If the API call failed, surface a clear exception (controller will handle it).
                if (response == null || !response.IsSuccess)
                    throw new Exception(response?.Message ?? "Failed to call notification service.");

                // If API succeeded but no data was returned, return null so caller can treat as NotFound.
                return response.ApiResponseData?.Data;
            }
            catch
            {
                // rethrow to allow controller to send a generic failure message
                throw;
            }
        }



        /// <summary>
        /// Creates a new NotificationConfig by sending it to the backend API.
        /// </summary>
        public async Task<ExecutionMessages> CreateAsync(NotificationConfig model)
        {
            try
            {
                // Assumes APICallHelper.CreateNotificationConfig is "api/notifications/configurations"
                var url = APICallHelper.Createnot;
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<NotificationConfig>>(url, model);

                if (response != null && response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, "Notification Config", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, "Notification Config", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Notification Config", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        /// <summary>
        /// Updates an existing NotificationConfig via the backend API.
        /// </summary>
        public async Task<ExecutionMessages> UpdateAsync(NotificationConfig model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Id))
                {
                    GetExecutionMessages(model, false, "Notification Config", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or ID provided for update.");
                    return ExecutionMessage;
                }

                // Assumes APICallHelper.UpdateNotificationConfig is "api/notifications/configurations/{0}"
                string url = string.Format(APICallHelper.Updatenot, model.Id);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<NotificationConfig>>(url, model);

                if (response != null && response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData?.Data, true, model.NotificationType, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.NotificationType, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Notification Config", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        /// <summary>
        /// Deletes a NotificationConfig via the backend API.
        /// </summary>
        public async Task<ExecutionMessages> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    GetExecutionMessages(null, false, $"Notification Config:{id}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Invalid ID provided.");
                    return ExecutionMessage;
                }

                // Assumes APICallHelper.DeleteNotificationConfig is "api/notifications/configurations/{0}"
                string url = string.Format(APICallHelper.Deletenot, id);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(url);

                if (response != null && response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Notification Config:{id}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message ?? "Deleted successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Notification Config:{id}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, response?.ApiResponseData?.Message ?? response?.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Notification Config:{id}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        #endregion
    }
}