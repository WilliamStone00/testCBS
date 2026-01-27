using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Translations;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.Translations
{
 public class TranslationPlaceholderService : BaseService
        {
            private readonly ApiCallerHelper _apiCallerHelper;

            public TranslationPlaceholderService()
            {
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
             
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("The API base URL is missing in Web.config.");
                }
                _apiCallerHelper = new ApiCallerHelper(baseUrl);
            }

        public async Task<IEnumerable<TranslationPlaceholder>> GetTranslationPlaceholdersAsync()
            {
                try
                {
                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<TranslationPlaceholder>>>(
                        APICallHelper.GetAllTranslationPlaceholders);

                    if (response.IsSuccess && response.ApiResponseData?.Data != null)
                    {
                        return response.ApiResponseData.Data;
                    }
                    return new List<TranslationPlaceholder>();
                }
                catch (Exception ex)
                {
                    // Log error
                    throw new Exception($"Failed to fetch translation placeholders: {ex.Message}", ex);
                }
            }

            public async Task<CustomDataTable> GetTranslationPlaceholdersDataTableAsync(TranslationPlaceholderQuery query)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                        APICallHelper.LoadTranslationPlaceholdersDataTable, query);

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
                    throw new Exception($"Translation placeholder service unavailable: {ex.Message}", ex);
                }
            }

            public async Task<TranslationPlaceholder> GetTranslationPlaceholderByIdAsync(string id)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(id))
                        throw new ArgumentException("id is required", nameof(id));

                    var encodedId = Uri.EscapeDataString(id);
                    string formattedUrl = string.Format(APICallHelper.GetTranslationPlaceholderById, encodedId);

                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<TranslationPlaceholder>>(formattedUrl);

                    if (response.IsSuccess)
                    {
                        return response.ApiResponseData?.Data;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to fetch translation placeholder: {ex.Message}", ex);
                }
            }

            public async Task<ExecutionMessages> CreateTranslationPlaceholderAsync(TranslationPlaceholder model)
            {
                try
                {
                    model.CreatedBy = GetUserFullName();
                    model.CreatedDate = DateTime.Now;
                   
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<TranslationPlaceholder>>(
                        APICallHelper.CreateTranslationPlaceholder, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(null, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                            response.Message ?? "Translation placeholder created successfully.");
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.Placeholder, MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                            response.Message ?? response.Message ?? "Failed to create translation placeholder.");
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.Placeholder, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> UpdateTranslationPlaceholderAsync(TranslationPlaceholder model)
            {
                try
                {
                    model.ModifiedBy = GetUserFullName();
                    model.ModifiedDate = DateTime.Now;

                    string formattedUrl = string.Format(APICallHelper.UpdateTranslationPlaceholder, model.Id);
                    var response = await _apiCallerHelper.PutAsync<ServiceResponse<TranslationPlaceholder>>(formattedUrl, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(null, true, null, MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                            response.Message ?? "Translation placeholder updated successfully.");
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.Placeholder, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                            response.ApiResponseData?.Message ?? response.Message ?? "Failed to update translation placeholder.");
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.Placeholder, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> DeactivateTranslationPlaceholderAsync(string placeholderId)
            {
                try
                {
                    string formattedUrl = string.Format(APICallHelper.DeactivateTranslationPlaceholder, placeholderId);
                    var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(null, true, $"Placeholder ID: {placeholderId}", MessagesResults.Success,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                            response.ApiResponseData?.Message ?? "Translation placeholder deactivated successfully.");
                    }
                    else
                    {
                        GetExecutionMessages(null, false, $"Placeholder ID: {placeholderId}", MessagesResults.Failed,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                            response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate translation placeholder.");
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(null, false, $"Placeholder ID: {placeholderId}", MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }

                return ExecutionMessage;
            }

            //public async Task<byte[]> ExportTranslationPlaceholdersAsync(TranslationPlaceholderQuery query)
            //{
            //    try
            //    {
            //        var response = await _apiCallerHelper.PostAsync<ResponseObject<byte[]>>(
            //            APICallHelper.ExportTranslationPlaceholders, query);

            //        if (!response.IsSuccess)
            //        {
            //            throw new Exception($"Export API call failed: {response.Message}");
            //        }

            //        return response.ApiResponseData?.Data;
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception($"Failed to export translation placeholders: {ex.Message}", ex);
            //    }
            //}

           
        }
    }

