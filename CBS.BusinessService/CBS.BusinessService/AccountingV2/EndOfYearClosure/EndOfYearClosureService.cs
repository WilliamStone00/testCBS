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
                if (string.IsNullOrWhiteSpace(branchId))
                    throw new ArgumentException("Branch ID cannot be null or empty.", nameof(branchId));


                // ✅ Make API call
                var response = await _apiCallerHelper.GetAsync<ResponseObject<accountingyear>>(string.Format(APICallHelper.GetAccoutingYearByBranchId, branchId));

                // ✅ Validate response
                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                var data = response.ApiResponseData?.Data;

                if (data == null)
                    throw new Exception("Accounting Year not found.");

                // ✅ Wrap single object into a list
                return new List<accountingyear> { data };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAccountingYearByBranchIdAsync] Error: {ex.Message}");
                throw;
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

        public async Task<EndOfYear> SaveReviewClosure(EndOfYear model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _apiCallerHelper.PostAsync<ResponseObject<EndOfYear>>(
                    APICallHelper.ReviewClosure,   // ✔ your correct endpoint
                    model
                );

                return apiResponse?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                throw;  // don't throw ex (removes stack trace)
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
