using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeCertification;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.ChequeCertification
{
    public class ChequeCertificationService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public ChequeCertificationService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<ChequeCertificationDto>> GetAllAsync()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<ChequeCertificationDto>>>(APICallHelper.GetAllChequeCertifications);
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                    return response.ApiResponseData.Data;

                return new List<ChequeCertificationDto>();
            }
            catch (Exception)
            {
                // optionally log
                throw;
            }
        }

        public async Task<ChequeCertificationDto> GetByIdAsync(string chequeCertificationID)
        {
            try
            {
                string url = string.Format(APICallHelper.GetChequeCertificationById, chequeCertificationID);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<ChequeCertificationDto>>(url);
                if (response.IsSuccess)
                    return response.ApiResponseData?.Data;

                return null;
            }
            catch (Exception)
            {
                // optionally log
                throw;
            }
        }

        public async Task<ExecutionMessages> CreateAsync(ChequeCertificationDto model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<ChequeCertificationDto>>(APICallHelper.CreateChequeCertification, model);

                if (response.IsSuccess)
                {
                    // Use ChequeCertificationID in messages (fallback to MemberReference if needed)
                    var idForMsg = response.ApiResponseData.Data?.ChequeCertificationID ?? model.ChequeCertificationID ?? model.MemberReference;
                    GetExecutionMessages(response.ApiResponseData.Data, true, idForMsg,
                        MessagesResults.Success, ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message);
                }
                else
                {
                    var idForMsg = model?.ChequeCertificationID ?? model?.MemberReference ?? "Unknown";
                    GetExecutionMessages(model, false, idForMsg,
                        MessagesResults.Failed, ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                var idForMsg = model?.ChequeCertificationID ?? model?.MemberReference ?? "Unknown";
                GetExecutionMessages(model, false, idForMsg,
                    MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(ChequeCertificationDto model)
        {
            try
            {
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<ChequeCertificationDto>>(APICallHelper.UpdateChequeCertification, model);

                if (response.IsSuccess)
                {
                    var idForMsg = model?.ChequeCertificationID ?? model?.MemberReference ?? "Unknown";
                    GetExecutionMessages(response.ApiResponseData.Data, true, idForMsg,
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message);
                }
                else
                {
                    var idForMsg = model?.ChequeCertificationID ?? model?.MemberReference ?? "Unknown";
                    GetExecutionMessages(model, false, idForMsg,
                        MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                var idForMsg = model?.ChequeCertificationID ?? model?.MemberReference ?? "Unknown";
                GetExecutionMessages(model, false, idForMsg,
                    MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ReviewAsync(string chequeCertificationID)
        {
            try
            {
                string url = string.Format(APICallHelper.ReviewChequeCertification, chequeCertificationID);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<ChequeCertificationDto>>(url, null);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, chequeCertificationID,
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, chequeCertificationID,
                        MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, chequeCertificationID,
                    MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ValidateAsync(string chequeCertificationID)
        {
            try
            {
                string url = string.Format(APICallHelper.ValidateChequeCertification, chequeCertificationID);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<ChequeCertificationDto>>(url, null);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, chequeCertificationID,
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, chequeCertificationID,
                        MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, chequeCertificationID,
                    MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> RejectAsync(string chequeCertificationID)
        {
            try
            {
                string url = string.Format(APICallHelper.RejectChequeCertification, chequeCertificationID);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<ChequeCertificationDto>>(url, null);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, chequeCertificationID,
                        MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, chequeCertificationID,
                        MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, chequeCertificationID,
                    MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteAsync(string chequeCertificationID)
        {
            try
            {
                string url = string.Format(APICallHelper.DeleteChequeCertification, chequeCertificationID);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(url);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"ID: {chequeCertificationID}",
                        MessagesResults.Success, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Deleted successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"ID: {chequeCertificationID}",
                        MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Unknown error.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"ID: {chequeCertificationID}",
                    MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage ?? new ExecutionMessages
            {
                MessageString = "No execution message was created.",
                MessageStatus = MessagesResults.Error.ToString()
            };
        }
    }
}
