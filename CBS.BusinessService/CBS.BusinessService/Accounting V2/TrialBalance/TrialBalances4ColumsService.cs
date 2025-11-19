using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.TrialBalance
{
    public class TrialBalances4ColumnService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly TrialBalance6ColumnsMock _trialBalance6ColumnsMock;

        public TrialBalances4ColumnService()
        {
            // Hardcoded base URL (intentionally allowed)
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _trialBalance6ColumnsMock = new TrialBalance6ColumnsMock();
        }

        /// <summary>
        /// Fetch trial balances using provided filter (6-column format).
        /// </summary>
        public async Task<GenericReportResponseV2Dto> GetTrialBalancesAsync4columns(AccountingV2ReportsFilter filter)
        {
            try
            {
               
                // POST request to the API
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<List<TrialBalanceV2Dto>>>(
                    APICallHelper.TrialBalance4,
                    filter
                );


                var result = new GenericReportResponseV2Dto();

                if (response?.IsSuccess == true)
                {
                    result.Lines = response.ApiResponseData?.Data ?? new List<TrialBalanceV2Dto>();
                }
                else
                {
                    result.Lines = new List<TrialBalanceV2Dto>();
                    System.Diagnostics.Debug.WriteLine($"TrialBalanceService.GetTrialBalancesAsync6columns: API returned failure ({response?.Message})");
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TrialBalanceService.GetTrialBalancesAsync6columns Error: {ex}");
                // optional: throw new Exception("Failed to fetch trial balances", ex);
                return new GenericReportResponseV2Dto
                {
                    Lines = new List<TrialBalanceV2Dto>()
                };
            }
        }

        public async Task<GenericReportResponseV2Dto> GetTrialMockInformation()
        {
            
            var results  =    TrialBalance6ColumnsMock.GetMockData();
            return results;
        }
    }
}
