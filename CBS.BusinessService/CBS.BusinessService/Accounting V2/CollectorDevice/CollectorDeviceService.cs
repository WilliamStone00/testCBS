using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.CollectorDevice;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.CollectorDevice
{
    public class CollectorDeviceService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public CollectorDeviceService()
        {
            //change the base url to the actual base url
            string baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'IdentityServerBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<CollectorDeviceresponse>> GetAsync()
        {
            try
            {
                // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<CollectorDeviceresponse>>>(APICallHelper.GetAllCollectorDevice);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<CollectorDeviceresponse>();
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

       public async Task<CollectorDeviceresponse> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetCollectorDeviceById, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<CollectorDeviceresponse>>(formattedUrl);

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

        public async Task<ExecutionMessages> CreateAsync(CollectorDeviceresponse model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<CollectorDeviceresponse>>(APICallHelper.CreateCollectorDevice, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.DeviceName, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.DeviceName, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.DeviceName, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(CollectorDeviceresponse model)
        {
            try
            {
                var catid = model.Id;
                string formattedUrl = string.Format(APICallHelper.UpdateCollectorDevice, catid);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<CollectorDeviceresponse>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.DeviceName, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.DeviceName, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.DeviceName, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteAsync(string categoryId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeactivateCollectorDevice, categoryId);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

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
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate category.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }      

    }
}

