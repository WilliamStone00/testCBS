// Location: ~/BusinessService/CheckManagementSystem/Configurations/NotificationConfiguration/NotificationConfigService.cs

using BusinessServices;
using CBS.API.Helper; // For ApiCallerHelper
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationConfig;
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
            // The base URL of your backend API, e.g., "http://localhost:5000/"
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
        public async Task<IEnumerable<NotificationType>> GetNotificationTypesAsync()
        {
            try
            {
                // Assumes APICallHelper.GetNotificationTypes is a constant like "api/notifications/types"
                var url = APICallHelper.GetNotificationTypes;

                // The ApiCallerHelper makes the HTTP GET request and deserializes the JSON response.
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<NotificationType>>>(url);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                // Return an empty list if the call fails or returns no data
                return Enumerable.Empty<NotificationType>();
            }
            catch (Exception ex)
            {
                // In a real scenario, you would log the exception 'ex'
                throw;
            }
        }

        /// <summary>
        /// Gets saved configurations from the API, supporting all filters.
        /// </summary>
        public async Task<IEnumerable<NotificationConfig>> GetConfigsAsync(bool isCentralized, string branchId = null, string notificationType = null)
        {
            try
            {
                // Build the query string for the API call
                var queryParts = new List<string>
                {
                    $"isCentralized={isCentralized.ToString().ToLower()}"
                };

                if (!isCentralized && !string.IsNullOrEmpty(branchId))
                {
                    queryParts.Add($"branchId={Uri.EscapeDataString(branchId)}");
                }

                if (!string.IsNullOrEmpty(notificationType))
                {
                    queryParts.Add($"notificationType={Uri.EscapeDataString(notificationType)}");
                }

                // Assumes APICallHelper.GetNotificationConfigs is "api/notifications/configurations"
                var url = APICallHelper.GetAllnot + "?" + string.Join("&", queryParts);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<NotificationConfig>>>(url);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return Enumerable.Empty<NotificationConfig>();
            }
            catch (Exception ex)
            {
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