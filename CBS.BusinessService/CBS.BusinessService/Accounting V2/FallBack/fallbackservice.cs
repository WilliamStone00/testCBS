using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FallBack;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.FallBack
{
        public class AffiliateToBranchFallbackService : BaseService
        {
            private readonly ApiCallerHelper _apiCallerHelper;

            public AffiliateToBranchFallbackService()
            {
                string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
                }
                _apiCallerHelper = new ApiCallerHelper(baseUrl);
            }

            public async Task<CustomDataTable> GetFallbackLogDataTableAsync(FallbackLogQuery query)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                        "/api/v1/AffiliateToBranchFallbackLog/datatable", query);

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
                    throw new Exception($"Fallback Log service unavailable: {ex.Message}", ex);
                }
            }

            public async Task<FallbackLogResponse> GetByIdAsync(string id)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(id))
                        throw new ArgumentException("id is required", nameof(id));

                    var encodedId = Uri.EscapeDataString(id);
                    string formattedUrl = $"/api/v1/AffiliateToBranchFallbackLog/{encodedId}";

                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<FallbackLogResponse>>(formattedUrl);

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

            public async Task<ExecutionMessages> ResolveAsync(ResolveFallbackRequest request)
            {
                try
                {
                    string formattedUrl = $"/api/v1/AffiliateToBranchFallbackLog/{request.Id}/resolve";
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(formattedUrl, request);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, $"Fallback Log {request.Id}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message ?? "Reconciliation completed successfully.");
                    }
                    else
                    {
                        GetExecutionMessages(request, false, $"Fallback Log {request.Id}", MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(request, false, $"Fallback Log {request.Id}", MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }
        }
    }

