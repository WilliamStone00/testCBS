using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReconciledLine;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.ReconciledLine
{
    public class ReconciledLineService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;


        public ReconciledLineService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }


        public async Task<CustomDataTable2> GetReconciledLineDataTableAsync(ReconciledQuery query)
        {
            try
            {
                if (!IsHeadOffice() && string.IsNullOrEmpty(query.BranchId))
                {
                    query.BranchId = GetBranchID();
                }
                query.Options.pageSize = 10;
                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var ReconciledLine = (
                   JsonConvert.SerializeObject(query));

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetReconciledLineDataTable, query);

                // If API call fails or response unsuccessful
                if (!response.IsSuccess)
                {
                    throw new Exception($" {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" {ex.Message}");
                throw new Exception($" {ex.Message}", ex);
            }
        }


        public async Task<List<Reconciled>> GetReconciledLinesByJournalHeaderId(string referenceNumber)
        {
            try
            {
                var url = string.Format(APICallHelper.GetReconciledLines, referenceNumber);

                var response = await _apiCallerHelper
                    .GetAsync<ResponseObject<List<Reconciled>>>(url);

                if (response?.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }

                return new List<Reconciled>();
            }
            catch (Exception ex)
            {
                // TODO: log error
                throw;
            }
        }
    }
}
