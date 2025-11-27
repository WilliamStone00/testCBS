using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.BalanceSheet
{
    public class BalanceSheetService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
     
        public BalanceSheetService()
        {
            var baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);

            
        }

        public async Task<List<Group>> GetBalanceSheetAsync(AccountingV2ReportsFilter filter)
        {


            var response  = new BalanceSheetMock();
            return response.Groups;






            
        }

    }
}
