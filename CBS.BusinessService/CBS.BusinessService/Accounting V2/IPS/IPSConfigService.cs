using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.IPS;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.IPS
{
    public class IPSConfigService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly BranchServices _branchServices;

        public IPSConfigService()
        {
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<IPSConfig>> GetAllIPSConfigsAsync()
        {
            try
            {
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<IPSConfig>>>(APICallHelper.GetAllIPSConfigs);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<IPSConfig>();
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                throw;
            }
        }

        public async Task<IPSConfig> GetIPSConfigByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetIPSConfigById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<IPSConfig>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                throw;
            }
        }

        public async Task<IPSConfig> GetActiveIPSConfigByYearAsync(int year,string branchid)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.GetIPSConfigByActiveYear, year,branchid);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<IPSConfig>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<IPSConfig>> GetIPSConfigsByYearAsync(int year)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.GetIPSConfigsByYear, year);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<IPSConfig>>>(formattedUrl);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<IPSConfig>();
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                   ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                throw;
            }
        }

        public async Task<CustomDataTable> GetIPSConfigDataTableAsync(IPSConfigQuery query)
        {
            try
            {
                if (!IsHeadOffice())
                {
                    query.BranchId = GetBranchID();
                }

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.IPSConfigDataTable, query);              

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }


                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {

                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                throw;
            }

        }

        public async Task<ExecutionMessages> CreateIPSConfigAsync(IPSConfig model)
        {
            try
            {
                model.CreatedBy = GetUserId();
                model.BranchCode = GetBankCode();
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<IPSConfig>>(APICallHelper.CreateIPSConfig, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false,null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, null, MessagesResults.Error,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateIPSConfigAsync(IPSConfig model)
        {
            try
            {
                var configId = model.Id;
                model.ModifiedBy = GetUserId();
                string formattedUrl = string.Format(APICallHelper.UpdateIPSConfig, configId);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<IPSConfig>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, $"IPS Config for {model.Year}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, $"IPS Config for {model.Year}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"IPS Config for {model.Year}", MessagesResults.Error,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteIPSConfigAsync(string configId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeleteIPSConfig, configId);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"IPS Config ID: {configId}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "IPS Config deleted successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"IPS Config ID: {configId}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to delete IPS Config.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"IPS Config ID: {configId}", MessagesResults.Error,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }       
    }
}