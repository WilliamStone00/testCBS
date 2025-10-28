using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.AccountingV2.JournalHead
{
    public class JournalHeadService : BaseService
    {

        private readonly ApiCallerHelper _JournalheadapiCallerHelper;
        

        public JournalHeadService()
        {

            _JournalheadapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

        public async Task<CustomDataTable2> GetJournalHeaderDataTableAsync(JournalEntryQuery query)
        {
            try
            {
               
                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var journalHeaders = (
                   JsonConvert.SerializeObject(query));

                var response = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetJournalHeaderDataTable, query);

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
                System.Diagnostics.Debug.WriteLine($"API Error (Journal Header): {ex.Message}");
                throw new Exception($"Journal header service unavailable: {ex.Message}", ex);
            }
        }


        //public async Task<JournalEntry> GetJournalEntryByIdAsync(string id)
        //{
        //    try
        //    {
        //        var branchId = "BR001";

        //        var response = await _JournalheadapiCallerHelper.GetAsync<ResponseObject<JournalEntry>>(
        //            $"{APICallHelper.GetJournalEntryTempById}?id={id}"
        //        );



        //        if (!response.IsSuccess)
        //            throw new Exception($"API call failed: {response.Message}");

        //        var entry = response.ApiResponseData?.Data;

        //        if (entry == null)
        //            throw new Exception("Journal Entry not found.");

        //        return entry;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"[GetJournalEntryByIdAsync] Error: {ex.Message}");
        //        throw; // Rethrow so the controller can handle/log it
        //    }
        //}

        // Fetch a single journal entry by ID using internal branchId
        public async Task<FrontDesk.Data.Entity.AccountingV2.JournalHead> GetJournalEntryByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Journal entry ID cannot be null or empty.", nameof(id));

                // ✅ Get branchId internally like in GetAccountsByBranchAsync
                var branchId = "BR001"; // Or use GetBranchID();

                // Make API call including branchId
                var response = await _JournalheadapiCallerHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.AccountingV2.JournalHead>>(
                    $"{APICallHelper.GetJournalEntryTempById}?id={id}&branchId={branchId}"
                );

                // Validate response
                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                var entry = response.ApiResponseData?.Data;
                if (entry == null)
                    throw new Exception("Journal Entry not found.");

                return entry;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetJournalEntryByIdAsync] Error: {ex.Message}");
                throw;
            }
        }



        

        public async Task<CustomDataTable2> GetJournalSourceDataTableAsync(JournalEntryQuery query)
        {
            try
            {

                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var journalHeaders = (
                   JsonConvert.SerializeObject(query));

                var response = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetJournalworkticketDataTable, query);

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
                System.Diagnostics.Debug.WriteLine($"API Error (Journal Header): {ex.Message}");
                throw new Exception($"Journal header service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<FrontDesk.Data.Entity.AccountingV2.JournalHead> GetJournalSourceByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Journal entry ID cannot be null or empty.", nameof(id));

                // Define operationCode for API call
                string operationCode = "MANUAL.ENTRY"; // Or call GetBranchID() if you want to use branch info

                // Make API call including operationCode
                var response = await _JournalheadapiCallerHelper.GetAsync<
                    ResponseObject<FrontDesk.Data.Entity.AccountingV2.JournalHead>>(
                    $"{APICallHelper.GetJournalSourceById}?id={id}&OperationCode={operationCode}"
                );

                // Validate response
                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                var entry = response.ApiResponseData?.Data;
                if (entry == null)
                    throw new Exception("Journal Entry not found.");

                // Optionally, set OperationCode or other properties on the returned entry
                entry.OperationCode = operationCode;

                return entry;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetJournalSourceByIdAsync] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ApproveAsync(string id)
        {
            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<bool>>(
                    APICallHelper.ApproveJournalEntry, new { Id = id });
                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to approve journal entry: {ex.Message}", ex);
            }
        }

        // REJECT Journal Entry
        public async Task<bool> RejectAsync(string id)
        {
            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<bool>>(
                    APICallHelper.RejectJournalEntry, new { Id = id });
                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reject journal entry: {ex.Message}", ex);
            }
        }

        
    }
}
