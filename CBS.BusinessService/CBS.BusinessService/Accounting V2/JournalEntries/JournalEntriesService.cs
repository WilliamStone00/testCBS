using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.JournalEntries
{
    public class JournalEntriesService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        public JournalEntriesService()
        {
            var baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<List<JournalDtoEntriesV2Dto>> GetJournalEntriesAsync(AccountingV2ReportsFilter filter)
        {
            string jsonFilter = JsonConvert.SerializeObject(filter, Formatting.Indented);
            try
            {
                var response = await _apiCallerHelper.PostAsync<
                ServiceResponse<List<JournalDtoEntriesV2Dto>>
            >(APICallHelper.JournalEntries, filter);
                // If API returned success
                if (response?.IsSuccess == true)
                {
                    return response.ApiResponseData?.Data
                           ?? new List<JournalDtoEntriesV2Dto>();
                }

                // API returned failure
                System.Diagnostics.Debug.WriteLine(
                    $"JournalEntriesService.GetJournalEntriesAsync: API returned failure ({response?.Message})"
                );

                return new List<JournalDtoEntriesV2Dto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"JournalEntriesService.GetJournalEntriesAsync Error: {ex}"
                );

                return new List<JournalDtoEntriesV2Dto>();
            }
        }

    }
}
