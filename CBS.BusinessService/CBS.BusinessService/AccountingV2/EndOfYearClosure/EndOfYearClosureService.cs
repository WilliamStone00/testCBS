using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.EndOfYearClosure
{
    public class EndOfYearClosureService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;

        public EndOfYearClosureService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

        public async Task<List<accountingyear>> GetAccountingYearByBranchIdAsync(string branchId)
        {
            try
            {
                // Call API
                var apiResponse = await _apiCallerHelper.GetAsync<ResponseObject<accountingyear>>(APICallHelper.GetAccoutingYearByBranchId);

                if (apiResponse == null || apiResponse.ApiResponseData == null || apiResponse.ApiResponseData.Data == null)
                    return new List<accountingyear>();

                var data = apiResponse.ApiResponseData.Data;

                // Wrap single object into a list
                var list = new List<accountingyear> { data };

                // Filter by branch if branchId is provided
                if (!string.IsNullOrEmpty(branchId))
                {
                    list = list.Where(x => x.BranchId == branchId).ToList();
                }

                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync] Error fetching Accounting Year: {ex.Message}");
                return new List<accountingyear>();
            }
        }



        public async Task<EndOfYear> SaveInitiateClosure(EndOfYear model)
        {


            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {

                var apiResponse = await _apiCallerHelper.PostAsync<ResponseObject<EndOfYear>>(
                    APICallHelper.InitiateClosure, // your destination endpoint
                    model
                );

                return apiResponse?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<List<EndOfYearTask>> GetAllEndTaskAsync()
        {
            try
            {
                var apiResponse = await _apiCallerHelper.GetAsync<ResponseObject<List<EndOfYearTask>>>(APICallHelper.GetAllEndOfYearTask);

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new List<EndOfYearTask>();

                return apiResponse.ApiResponseData.Data ?? new List<EndOfYearTask>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync] Error fetching all Accounting Year: {ex.Message}");
                return new List<EndOfYearTask>();
            }
        }
    }
}
