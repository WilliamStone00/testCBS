using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Helper;
using Microsoft.AspNet.SignalR.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.GLSystemReconciliation
{
    public class GLSystemReconciliationService
    {
        private readonly ApiCallerHelper _systemReconciliationapiCallerHelper;


        public GLSystemReconciliationService()
        {

            _systemReconciliationapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }


        public async Task<CustomDataTable2> GetReconciliationDataTableAsync(ReconciliationQuery query)
        {
            try
            {
                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var response = await _systemReconciliationapiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetReconciliationDataTable, query);

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error (Reconciliation): {ex.Message}");
                throw new Exception($"Reconciliation service unavailable: {ex.Message}", ex);
            }
        }

        //public async Task<CustomDataTable2> GetReconciliationDataTableAsync(object tableRequest)
        //{
        //    try
        //    {
        //        var response = await _systemReconciliationapiCallerHelper.PostAsync<
        //            ResponseObject<CustomDataTable2>
        //        >(APICallHelper.GetReconciliationDataTable, tableRequest);

        //        if (!response.IsSuccess)
        //            throw new Exception($"API call failed: {response.Message}");

        //        if (response.ApiResponseData == null)
        //            throw new Exception("API returned null data");

        //        return response.ApiResponseData.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"DataTable Error: {ex.Message}");
        //        throw new Exception($"DataTable Error: {ex.Message}", ex);
        //    }
        //}



        public async Task<ReconciliationData> GetReconciliationSummaryAsyncs(ReconciliationQuery query)
        {
            try
            {
               

                var response = await _systemReconciliationapiCallerHelper
                    .PostAsync<ResponseObject<ReconciliationData>>(
                        APICallHelper.GetReconciliationSummary, query
                    );

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData?.Data == null)
                    throw new Exception("API returned null data");

                // Return only the summary data
                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Summary Error: {ex.Message}");
                throw;
            }
        }

        public async Task<ReconciliationData> GetReconciliationSummaryAsync(ReconciliationQuerys query)
        {
            try
            {

                

                // Call API and get raw JSON
                var jsonResponse = await _systemReconciliationapiCallerHelper
                    .PostAsync<ServiceResponse<ReconciliationData>>(APICallHelper.GetReconciliationSummary, query);

                // Deserialize the wrapper
              ///*  var apiResponse = JsonConvert.DeserializeObject<Api*/Response<ReconciliationData>>(jsonResponse);

                // check response for success / nulls
                if (jsonResponse == null || jsonResponse.ApiResponseData == null || jsonResponse.ApiResponseData.Data == null)
                {
                    // optionally throw or return null and let caller handle
                    return null;
                }

                return jsonResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Summary Error: {ex.Message}");
                throw;
            }
        }



    }
}
