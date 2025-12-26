using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.LoanP.Config
{
           public class LoanTargetCatalogService : BaseService
        {
            private readonly ApiCallerHelper _apiCallerHelper;

            public LoanTargetCatalogService()
            {
                string baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("The 'IdentityServerBaseUrl' appSetting is missing or empty in Web.config.");
                }
                _apiCallerHelper = new ApiCallerHelper(baseUrl);
            }

            public async Task<ExecutionMessages> CreateAsync(CreateLoanTargetCatalogRequest model)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<LoanTargetCatalog>>(
                        APICallHelper.LoanTargetCatalogCreate, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.NameEn,
                            MessagesResults.Success, ExecutionProcessOption.InsertObject,
                            SystemMessageStatus.Success.ToString(), null,
                            response.ApiResponseData?.Message ?? "Loan target catalog created successfully");
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.NameEn, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                            null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.NameEn, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<LoanTargetCatalog> GetByIdAsync(string id)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(id))
                        throw new ArgumentException("id is required", nameof(id));

                    var encodedId = Uri.EscapeDataString(id);
                    string formattedUrl = string.Format(APICallHelper.LoanTargetCatalogGet + "?id={0}", encodedId);

                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<LoanTargetCatalog>>(formattedUrl);

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

            public async Task<IEnumerable<LoanTargetCatalog>> GetAllAsync()
            {
                try
                {
                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<LoanTargetCatalog>>>(
                        APICallHelper.LoanTargetCatalogGetAll);

                    if (response.IsSuccess && response.ApiResponseData?.Data != null)
                    {
                        return response.ApiResponseData.Data;
                    }
                    return new List<LoanTargetCatalog>();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            public async Task<ExecutionMessages> UpdateAsync(UpdateLoanTargetCatalogRequest model)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<LoanTargetCatalog>>(
                        APICallHelper.LoanTargetCatalogUpdate, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.NameEn,
                            MessagesResults.Success, ExecutionProcessOption.UpdateUpject,
                            SystemMessageStatus.Success.ToString(), null,
                            response.ApiResponseData?.Message ?? "Loan target catalog updated successfully");
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.NameEn, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                            null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.NameEn, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> DeleteAsync(string id)
            {
                try
                {
                    var model = new DeleteLoanTargetCatalogRequest { Id = id };
                    string formattedUrl = APICallHelper.LoanTargetCatalogDelete + "?id=" + Uri.EscapeDataString(id);

                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(formattedUrl, model);

                    if (response.IsSuccess && response.ApiResponseData?.Data == true)
                    {
                        GetExecutionMessages(null, true, id, MessagesResults.Success,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(),
                            null, response.ApiResponseData?.Message ?? "Loan target catalog deleted successfully");
                    }
                    else
                    {
                        GetExecutionMessages(model, false, id, MessagesResults.Failed,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(),
                            null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(id, false, id, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<CustomDataTable> GetDataTableAsync(LoanTargetCatalogQuery query)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<CustomDataTable>(
                        "/api/v1/LoanTargetCatalog/GetDataTable", query);

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
                    throw new Exception($"Loan Target Catalog Service unavailable: {ex.Message}", ex);
                }
            }
        }
    }

