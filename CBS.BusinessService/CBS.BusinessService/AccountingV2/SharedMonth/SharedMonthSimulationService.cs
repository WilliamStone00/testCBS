using BusinessServices;
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
    public class SharedMonthSimulationService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly ApiCallerHelper _apiCallerHelperT;



        public SharedMonthSimulationService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
            _apiCallerHelperT = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<List<ShareMonthPsiReportLineDto>> GetSimulationData(SharedMonthSimulation model)
        {

            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.ProductId = "0000";
            model.StartPeriodKey = "jan";
            model.EndPeriodKey = "feb";

            var apiResponse =
                await _apiCallerHelper.PostAsync<ResponseObject<List<ShareMonthPsiReportLineDto>>>(
                    APICallHelper.InterestDistributionSimulation1,
                    model
                );

            return apiResponse?.ApiResponseData?.Data ?? new List<ShareMonthPsiReportLineDto>();
        }

        public async Task<ApiResponse<ResponseObject<CreateSimulations>>> CreateShareMonthSimulationAsync(CreateSimulations model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Date = DateTime.Now;
            model.SimulatedByUserName = GetUserFullName();
            model.SimulatedByUserId = GetUserID();

            try
            {
                var apiResponse =
                    await _apiCallerHelperT.PostAsync<ResponseObject<CreateSimulations>>(
                        APICallHelper.CreateShareMonthSimulation, // API endpoint
                        model                                      // FULL model posted
                    );

                return apiResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }
}
