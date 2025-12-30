using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;

namespace CBS.BusinessService.LoanP
{
          public class LoanTypeService : BaseService
        {
            private readonly ApiCallerHelper _apiCallerHelper;
            private readonly string _baseUrl;

            public LoanTypeService()
            {
                _baseUrl = ConfigurationManager.AppSettings["LoanBaseUrl"];
                if (string.IsNullOrWhiteSpace(_baseUrl))
                {
                    throw new ConfigurationErrorsException(
                        "The 'LoanBaseUrl' appSetting is missing or empty in Web.config.");
                }

                _apiCallerHelper = new ApiCallerHelper(_baseUrl);
            }

            /* ========================= CREATE ========================= */
            public async Task<ExecutionMessages> CreateAsync(LoanType model)
            {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<LoanType>>(
                        APICallHelper.LoanTypeCreate, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(
                            response.ApiResponseData.Data,
                            true,
                            model.Name,
                            MessagesResults.Success,
                            ExecutionProcessOption.InsertObject,
                            SystemMessageStatus.Success.ToString(),
                            null,
                            response.ApiResponseData?.Message ?? "Loan type created successfully");
                    }
                    else
                    {
                        GetExecutionMessages(
                            model,
                            false,
                            model.Name,
                            MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject,
                            SystemMessageStatus.Failed.ToString(),
                            null,
                            response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(
                        model,
                        false,
                        model.Name,
                        MessagesResults.Error,
                        ExecutionProcessOption.TryCatch,
                        SystemMessageStatus.Error.ToString(),
                        ex,
                        ex.Message);
                }

                return ExecutionMessage;
            }

            /* ========================= GET BY ID ========================= */
            public async Task<LoanType> GetByIdAsync(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                var url = string.Format(APICallHelper.LoanTypeGetById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<LoanType>>(url);
                return response.IsSuccess ? response.ApiResponseData?.Data : null;
            }

            /* ========================= GET ALL ========================= */
            public async Task<IEnumerable<LoanType>> GetAllAsync()
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<LoanType>>>(
                    APICallHelper.LoanTypeGetAll);

                return response.IsSuccess && response.ApiResponseData?.Data != null
                    ? response.ApiResponseData.Data
                    : new List<LoanType>();
            }

            /* ========================= UPDATE ========================= */
            public async Task<ExecutionMessages> UpdateAsync(LoanType model)
            {
                try
                {
                    var encodedId = Uri.EscapeDataString(model.Id);
                    var url = string.Format(APICallHelper.LoanTypeUpdate, encodedId);

                    var response = await _apiCallerHelper.PutAsync<ServiceResponse<LoanType>>(url, model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(
                            response.ApiResponseData.Data,
                            true,
                            model.Name,
                            MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages,
                            SystemMessageStatus.Success.ToString(),
                            null,
                            response.ApiResponseData?.Message ?? "Loan type updated successfully");
                    }
                    else
                    {
                        GetExecutionMessages(
                            model,
                            false,
                            model.Name,
                            MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages,
                            SystemMessageStatus.Failed.ToString(),
                            null,
                            response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(
                        model,
                        false,
                        model.Name,
                        MessagesResults.Error,
                        ExecutionProcessOption.TryCatch,
                        SystemMessageStatus.Error.ToString(),
                        ex,
                        ex.Message);
                }

                return ExecutionMessage;
            }

            /* ========================= DELETE ========================= */
            public async Task<ExecutionMessages> DeleteAsync(string id)
            {
                try
                {
                    var encodedId = Uri.EscapeDataString(id);
                    var url = string.Format(APICallHelper.LoanTypeDelete, encodedId);

                    var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(url);

                    if (response.IsSuccess && response.ApiResponseData?.Data == true)
                    {
                        GetExecutionMessages(
                            null,
                            true,
                            id,
                            MessagesResults.Success,
                            ExecutionProcessOption.DeleteObject,
                            SystemMessageStatus.Success.ToString(),
                            null,
                            response.ApiResponseData?.Message ?? "Loan type deleted successfully");
                    }
                    else
                    {
                        GetExecutionMessages(
                            id,
                            false,
                            id,
                            MessagesResults.Failed,
                            ExecutionProcessOption.DeleteObject,
                            SystemMessageStatus.Failed.ToString(),
                            null,
                            response.ApiResponseData?.Message ?? response.Message);
                    }
                }
                catch (Exception ex)
                {
                    GetExecutionMessages(
                        id,
                        false,
                        id,
                        MessagesResults.Error,
                        ExecutionProcessOption.TryCatch,
                        SystemMessageStatus.Error.ToString(),
                        ex,
                        ex.Message);
                }

                return ExecutionMessage;
            }
        }
    }

