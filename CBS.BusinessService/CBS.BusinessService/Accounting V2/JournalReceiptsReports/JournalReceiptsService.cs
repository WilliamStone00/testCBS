using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountingReportsD;
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

namespace CBS.BusinessService.Accounting_V2.JournalReceiptsReports
{
    public class JournalReceiptsService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly TrialBalance6ColumnsMock _trialBalance6ColumnsMock;

        public JournalReceiptsService()
        {
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _trialBalance6ColumnsMock = new TrialBalance6ColumnsMock();
        }

        /// <summary>
        /// Fetch account statement (list of ledger movements).
        /// </summary>


        public async Task<List<ReceiptResponseDto>> GetReceiptAsync(ReceiptV2Filter filter)
        {
            string jsonFilter = JsonConvert.SerializeObject(filter, Formatting.Indented);

            var response = await _apiCallerHelper.PostAsync<
                ServiceResponse<List<ReceiptResponseDto>>
            >(APICallHelper.JournalReceipts, filter);

            return response?.ApiResponseData?.Data ?? new List<ReceiptResponseDto>();
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
