using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using BusinessServices;

namespace CBS.BusinessService.LoanP
{
    public class PcmfLoanPurposeService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly string _baseUrl;

        public PcmfLoanPurposeService()
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
        public async Task<ExecutionMessages> CreateAsync(PcmfLoanPurpose model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<PcmfLoanPurpose>>(
                    APICallHelper.PcmfLoanPurposeCreate, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(
                        response.ApiResponseData.Data,
                        true,
                        model.NameEn,
                        MessagesResults.Success,
                        ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData?.Message ?? "PCMF Loan Purpose created successfully");
                }
                else
                {
                    GetExecutionMessages(
                        model,
                        false,
                        model.NameEn,
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
                    model.NameEn,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message);
            }

            return ExecutionMessage;
        }

        /* ========================= GET BY ID ========================= */
        public async Task<PcmfLoanPurpose> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id is required", nameof(id));

            var encodedId = Uri.EscapeDataString(id);
            var url = string.Format(APICallHelper.PcmfLoanPurposeGetById, encodedId);

            var response = await _apiCallerHelper.GetAsync<ServiceResponse<PcmfLoanPurpose>>(url);
            return response.IsSuccess ? response.ApiResponseData?.Data : null;
        }

        /* ========================= GET ALL ========================= */
        public async Task<IEnumerable<PcmfLoanPurpose>> GetAllAsync()
        {
            var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<PcmfLoanPurpose>>>(
                APICallHelper.PcmfLoanPurposeGetAll);

            return response.IsSuccess && response.ApiResponseData?.Data != null
                ? response.ApiResponseData.Data
                : new List<PcmfLoanPurpose>();
        }

        /* ========================= UPDATE ========================= */
        public async Task<ExecutionMessages> UpdateAsync(PcmfLoanPurpose model)
        {
            try
            {
                var encodedId = Uri.EscapeDataString(model.Id);
                var url = string.Format(APICallHelper.PcmfLoanPurposeUpdate, encodedId);

                var response = await _apiCallerHelper.PutAsync<ServiceResponse<PcmfLoanPurpose>>(url, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(
                        response.ApiResponseData.Data,
                        true,
                        model.NameEn,
                        MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData?.Message ?? "PCMF Loan Purpose updated successfully");
                }
                else
                {
                    GetExecutionMessages(
                        model,
                        false,
                        model.NameEn,
                        MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject,
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
                    model.NameEn,
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
                var url = string.Format(APICallHelper.PcmfLoanPurposeDelete, encodedId);

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
                        response.ApiResponseData?.Message ?? "PCMF Loan Purpose deleted successfully");
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
