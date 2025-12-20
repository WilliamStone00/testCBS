using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.SharedMonth
{
    public class SharedMonthSimulationService
    {

        private readonly ApiCallerHelper _apiCallerHelper;


        public SharedMonthSimulationService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

        public async Task<List<ShareMonthPsiReportLineDto>> GetSimulationData(SharedMonthSimulation model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var apiResponse =
                await _apiCallerHelper.PostAsync<ResponseObject<List<ShareMonthPsiReportLineDto>>>(
                    APICallHelper.InterestDistributionSimulation,
                    model
                );

            return apiResponse?.ApiResponseData?.Data ?? new List<ShareMonthPsiReportLineDto>();
        }


    }
}
