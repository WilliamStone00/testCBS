using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.InterestProductConfig;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.InterestProductConfig
{
    public class InterestProductConfigService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;
        


        public InterestProductConfigService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }


        public async Task<CustomDataTable2> GetJournalHeaderDataTableAsync(InterestProductConfigQuery query)
        {
            try
            {
                // If not head office and branchId is null, set it
                if (!IsHeadOffice() && string.IsNullOrEmpty(query.BranchId))
                {
                    query.BranchId = GetBranchID();
                }

                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var journalHeaders = (
                   JsonConvert.SerializeObject(query));

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetInterestProductConfigDataTable, query);

                // If API call fails or response unsuccessful
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error (Interest Product Config): {ex.Message}");
                throw new Exception($"Interest Product Config Service unavailable: {ex.Message}", ex);
            }
        }


    }
}
