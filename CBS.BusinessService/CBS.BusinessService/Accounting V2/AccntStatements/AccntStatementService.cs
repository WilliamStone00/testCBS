using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;  // ✔ Correct namespace for AccountStatementResponse
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.AccntStatements
{
    public class AccntStatementService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly TrialBalance6ColumnsMock _trialBalance6ColumnsMock;

        public AccntStatementService()
        {
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _trialBalance6ColumnsMock = new TrialBalance6ColumnsMock();
        }

        /// <summary>
        /// Fetch account statement (list of ledger movements).
        /// </summary>
        //public async Task<List<AccountStatementResponse>> GetAccntStatement(AccountingV2ReportsFilter filter)
        //{
        //    if (filter.AccountNumbers.Count()==1)
        //    {
        //        filter.AccountNumbers = new List<string>();
        //    }

        //    string jsonFilter = JsonConvert.SerializeObject(filter, Formatting.Indented);
        //    var response = await _apiCallerHelper.PostAsync<
        //        ServiceResponse<List<AccountStatementResponse>>
        //    >(APICallHelper.AccntsStatements, filter);

        //    if (response?.IsSuccess == true)
        //    {
        //        return response.ApiResponseData?.Data ?? new List<AccountStatementResponse>();
        //    }

        //    return new List<AccountStatementResponse>();
        //}

        public async Task<List<AccountStatementResponse>> GetAccntStatement(AccountingV2ReportsFilter filter)
        {
            // Avoid null errors
            if (filter.AccountNumbers == null)
                filter.AccountNumbers = new List<string>();

            string jsonFilter = JsonConvert.SerializeObject(filter, Formatting.Indented);

            var response = await _apiCallerHelper.PostAsync<
                ServiceResponse<object>
            >(APICallHelper.AccntsStatements, filter);

            if (response?.IsSuccess != true || response.ApiResponseData?.Data == null)
                return new List<AccountStatementResponse>();

            var data = response.ApiResponseData.Data;

            // Handle BOTH object and list
            if (data is JArray)
            {
                return ((JArray)data).ToObject<List<AccountStatementResponse>>();
            }
            else if (data is JObject)
            {
                return new List<AccountStatementResponse>
        {
            ((JObject)data).ToObject<AccountStatementResponse>()
        };
            }

            return new List<AccountStatementResponse>();
        }

        /// <summary>
        /// Returns mock trial balance information (6-column format).
        /// </summary>
        public async Task<GenericReportResponseV2Dto> GetTrialMockInformation()
        {
            return TrialBalance6ColumnsMock.GetMockData();
        }
    }
}
