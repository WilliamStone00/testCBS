using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.ConfigurationsManualEntry
{
    public class ConfigurationManualEntryService : BaseService
    {

        private readonly ApiCallerHelper _configurationapiCallerHelper;


        public ConfigurationManualEntryService()
        {

            _configurationapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

        public async Task<List<ConfigurationManualEntries>> GetAllAsync()
        {
            try
            {
                var apiResponse = await _configurationapiCallerHelper
                    .GetAsync<List<ConfigurationManualEntries>>(APICallHelper.GetAllConfigurationManualEntry);

                return apiResponse.ApiResponseData ?? new List<ConfigurationManualEntries>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                throw new Exception($"Configuration service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ConfigurationManualEntries> GetData(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new ConfigurationManualEntries();

                var apiResponse = await _configurationapiCallerHelper
                    .GetAsync<ResponseObject<ConfigurationManualEntries>>(
                        string.Format(APICallHelper.GetConfigurationManualEntryById, id)
                    );

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new ConfigurationManualEntries();

                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Optional: log error for diagnostics
                System.Diagnostics.Debug.WriteLine($"[GetData] Error fetching ConfigurationManualEntries: {ex.Message}");
                return new ConfigurationManualEntries();
            }
        }



        public async Task<ExecutionMessages> UpdateConfigurationManualEntryAsync(ConfigurationManualEntries model)
        {
            try
            {

                // Send model to API via POST (or PUT if your API expects PUT)
                var response = await _configurationapiCallerHelper.PostAsync<ServiceResponse<ConfigurationManualEntries>>(APICallHelper.UpdateConfigurationManualEntry, model);

                if (response.IsSuccess)
                {
                    // Success execution message
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Type, MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message
                    );
                }
                else
                {
                    // Failure execution message
                    GetExecutionMessages(model, false, model.Type, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject,
                        SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message
                    );
                }
            }
            catch (Exception ex)
            {
                // Exception execution message
                GetExecutionMessages(model, false, model.Type, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message
                );
            }

            return ExecutionMessage; // Return accumulated execution result
        }

    }
}

