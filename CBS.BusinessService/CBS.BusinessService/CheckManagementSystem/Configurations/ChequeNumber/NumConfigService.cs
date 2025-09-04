using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeNumber;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Configurations.ChequeNumber
{
    public class NumConfigService : BaseService
    {     
            private readonly ApiCallerHelper _apiCallerHelper;

            public NumConfigService()
            {
                string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");
                }
                _apiCallerHelper = new ApiCallerHelper(baseUrl);
            }

            public async Task<IEnumerable<NumConfig>> GetAsync()
            {
                try
                {
                    // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<NumConfig>>>(APICallHelper.GetAll);

                    // CORRECTED: Access the final payload via .ApiResponseData.Data
                    if (response.IsSuccess && response.ApiResponseData?.Data != null)
                    {
                        return response.ApiResponseData.Data;
                    }
                    return new List<NumConfig>();
                }
                catch (Exception ex)
                {
                    // In a real scenario, log 'ex'
                    throw;
                }
            }

            public async Task<NumConfig> GetByIdAsync(string categoryId)
            {
                try
                {
                    string formattedUrl = string.Format(APICallHelper.GetById, categoryId);
                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<NumConfig>>(formattedUrl);

                    // CORRECTED: Access the final payload via .ApiResponseData.Data
                    if (response.IsSuccess)
                    {
                        return response.ApiResponseData?.Data;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    // In a real scenario, log 'ex'
                    throw;
                }
            }

            public async Task<ExecutionMessages> CreateAsync(NumConfig model)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<NumConfig>>(APICallHelper.Create, model);

                    // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> UpdateAsync(NumConfig model)
            {
                try
                {
                    string formattedUrl = string.Format(APICallHelper.Update);
                    var response = await _apiCallerHelper.PutAsync<ServiceResponse<NumConfig>>(formattedUrl, model);

                    // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                    }
                    else
                    {
                        GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }
                return ExecutionMessage;
            }

            public async Task<ExecutionMessages> DelateAsync(string categoryId)
            {
                try
                {
                    if (string.IsNullOrEmpty(APICallHelper.Delete))
                        throw new InvalidOperationException("DeactivateChequeBookCategory URL is not configured.");

                    string formattedUrl = string.Format(APICallHelper.DeactivateChequeBookCategory, categoryId);

                    if (_apiCallerHelper == null)
                        throw new InvalidOperationException("_apiCallerHelper is not initialized.");

                    var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                    if (response == null)
                        throw new InvalidOperationException("API returned null response.");

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(null, true, $"Category ID: {categoryId}", MessagesResults.Success,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                            response.ApiResponseData?.Message ?? "Category deactivated successfully.");
                    }
                    else
                    {
                        GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Failed,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                            response.ApiResponseData?.Message ?? response.Message ?? "Unknown error.");
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Error,
                        ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                }

                return ExecutionMessage ?? new ExecutionMessages
                {
                    MessageString = "No execution message was created.",
                    MessageStatus = MessagesResults.Error.ToString()
                };
            }                  
        }
    }


