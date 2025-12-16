// DailyCollectorLicenseService.cs
using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.DaillyCollectorLicenseService
{
    public class DailyCollectorLicenseService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        

        public DailyCollectorLicenseService(UserManagementServices userManagementServices)
        {
            string baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'IdentityServerBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<ExecutionMessages> GenerateLicenseAsync(GenerateLicenseRequest model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<DailyCollectorLicense>>(
                    APICallHelper.GenerateLicense, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.CollectorUserId,
                        MessagesResults.Success, ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "License generated successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, model.CollectorUserId, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.CollectorUserId, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> RevokeLicenseAsync(RevokeLicenseRequest model)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.RevokeLicense, model.LicenseId);
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<DailyCollectorLicense>>(
                    formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, model.LicenseId, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "License revoked successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateLicenseAsync(DeactivateLicenseRequest model)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeactivateLicense, model.LicenseId);
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<DailyCollectorLicense>>(
                    formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, model.LicenseId, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "License deactivated successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ReactivateLicenseAsync(ReactivateLicenseRequest model)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.ReactivateLicense, model.LicenseId);
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<DailyCollectorLicense>>(
                    formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, model.LicenseId, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "License reactivated successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ActivateLicenseAsync(ActivateLicenseRequest model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<DailyCollectorLicense>>(
                    APICallHelper.ActivateLicense, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.CollectorUserName,
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "License activated successfully");
                }
                else
                {
                    GetExecutionMessages(model, false, model.CollectorUserName, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.CollectorUserName, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<DailyCollectorLicense> CheckLicenseStatusAsync(CheckLicenseStatusRequest model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<DailyCollectorLicense>>(
                    APICallHelper.CheckLicenseStatus, model);

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

        public async Task<DailyCollectorLicense> GetLicenseByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetLicenseById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<DailyCollectorLicense>>(formattedUrl);

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

        public async Task<IEnumerable<DailyCollectorLicense>> GetCollectorLicensesAsync(string collectorUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(collectorUserId))
                    throw new ArgumentException("collectorUserId is required", nameof(collectorUserId));

                var encodedUserId = Uri.EscapeDataString(collectorUserId);
                string formattedUrl = string.Format(APICallHelper.GetCollectorLicenses, encodedUserId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<DailyCollectorLicense>>>(formattedUrl);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<DailyCollectorLicense>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CustomDataTable> GetDataTableAsync(LicenseQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<CustomDataTable>(
                    APICallHelper.LicenseDataTable, query);

                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                throw new Exception($"Daily Collector License Service unavailable: {ex.Message}", ex);
            }
        }
        // Add this method to DailyCollectorLicenseService.cs
        public async Task<ExecutionMessages> PerformLicenseActionAsync(LicenseActionRequest model)
        {
            try
            {
                // NOTE: Pass the inner type ServiceResponse<DailyCollectorLicense> to PostAsync<T>
                // _apiCallerHelper.PostAsync<T> returns ApiResponse<T>, so T should be ServiceResponse<...>
                CBS.API.Helper.ApiResponse<CBS.FrontDesk.Helper.ServiceResponse<CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense.DailyCollectorLicense>> apiResponse = null;

                // convenience alias for the inner ServiceResponse<> type
                var innerTypeName = typeof(CBS.FrontDesk.Helper.ServiceResponse<CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense.DailyCollectorLicense>).FullName;

                switch ((model.ActionType ?? string.Empty).ToLowerInvariant())
                {
                    case "revoke":
                        {
                            var revokeUrl = string.Format(APICallHelper.RevokeLicense, model.LicenseId);
                            apiResponse = await _apiCallerHelper.PostAsync<CBS.FrontDesk.Helper.ServiceResponse<CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense.DailyCollectorLicense>>(
                                revokeUrl,
                                new RevokeLicenseRequest
                                {
                                    LicenseId = model.LicenseId,
                                    Reason = model.Reason
                                });
                            break;
                        }

                    case "deactivate":
                        {
                            var deactivateUrl = string.Format(APICallHelper.DeactivateLicense, model.LicenseId);
                            apiResponse = await _apiCallerHelper.PostAsync<CBS.FrontDesk.Helper.ServiceResponse<CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense.DailyCollectorLicense>>(
                                deactivateUrl,
                                new DeactivateLicenseRequest
                                {
                                    LicenseId = model.LicenseId,
                                    Reason = model.Reason
                                });
                            break;
                        }

                    case "reactivate":
                        {
                            var reactivateUrl = string.Format(APICallHelper.ReactivateLicense, model.LicenseId);
                            apiResponse = await _apiCallerHelper.PostAsync<CBS.FrontDesk.Helper.ServiceResponse<CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense.DailyCollectorLicense>>(
                                reactivateUrl,
                                new ReactivateLicenseRequest
                                {
                                    LicenseId = model.LicenseId,
                                    Reason = model.Reason
                                });
                            break;
                        }

                    case "extend":
                        {
                            var extendUrl = string.Format("/api/v1/DailyCollectorLicense/{0}/extend", model.LicenseId);
                            apiResponse = await _apiCallerHelper.PostAsync<CBS.FrontDesk.Helper.ServiceResponse<CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense.DailyCollectorLicense>>(
                                extendUrl,
                                new ExtendLicenseRequest
                                {
                                    LicenseId = model.LicenseId,
                                    NewExpiryDate = model.NewExpiryDate ?? DateTime.UtcNow,
                                    Reason = model.Reason
                                });
                            break;
                        }

                    default:
                        throw new ArgumentException($"Invalid action type: {model.ActionType}");
                }

                // At this point apiResponse should be ApiResponse<ServiceResponse<DailyCollectorLicense>>
                // Defensive check: if apiResponse is null -> failure
                if (apiResponse == null)
                {
                    GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, "No response from API");
                    return ExecutionMessage;
                }

                // Check outer wrapper success first (transport/API level)
                if (!apiResponse.IsSuccess)
                {
                    // Outer API call failed
                    GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, apiResponse.Message ?? "API call failed");
                    return ExecutionMessage;
                }

                // Inner service response
                var serviceResp = apiResponse.ApiResponseData; // ServiceResponse<DailyCollectorLicense>

                // If your ServiceResponse<T> uses a boolean Success or IsSuccess property, check it.
                // Many projects use serviceResp.Data != null as success indicator; adapt as needed.
                bool innerSuccess = false;
                string innerMessage = null;
                object innerData = null;

                if (serviceResp != null)
                {
                    // try to get Data property (common)
                    var dataProp = serviceResp.GetType().GetProperty("Data");
                    if (dataProp != null)
                    {
                        innerData = dataProp.GetValue(serviceResp);
                        innerSuccess = innerData != null;
                    }

                    // also try message property
                    var msgProp = serviceResp.GetType().GetProperty("Message");
                    if (msgProp != null)
                    {
                        innerMessage = msgProp.GetValue(serviceResp)?.ToString();
                    }

                    // check explicit Success property if present
                    var succProp = serviceResp.GetType().GetProperty("Success") ?? serviceResp.GetType().GetProperty("IsSuccess");
                    if (succProp != null && succProp.PropertyType == typeof(bool))
                    {
                        var val = succProp.GetValue(serviceResp);
                        if (val is bool b) innerSuccess = innerSuccess || b;
                    }
                }

                if (innerSuccess)
                {
                    var successMsg = innerMessage ?? apiResponse.Message ?? $"{model.ActionType} action completed successfully";
                    GetExecutionMessages(null, true, model.LicenseId, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, successMsg);
                }
                else
                {
                    var failMsg = innerMessage ?? apiResponse.Message ?? "Action failed";
                    GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, failMsg);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.LicenseId, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                return ExecutionMessage;
            }
        }


    }
}