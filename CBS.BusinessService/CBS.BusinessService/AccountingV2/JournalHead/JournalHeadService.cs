using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AndriodApp;
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


        // Fetch a single journal entry by ID using internal branchId
        public async Task<FrontDesk.Data.Entity.AccountingV2.JournalHead> GetJournalEntryByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Journal entry ID cannot be null or empty.", nameof(id));
                // ✅ Make API call
                var response = await _JournalheadapiCallerHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.AccountingV2.JournalHead>>(string.Format(APICallHelper.GetJournalEntryTempById,id));

                // ✅ Validate response
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

        public async Task<WorkflowTicket> GetJournalSourceByIdAsync(string id)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(id))
                //    throw new ArgumentException("Journal entry ID cannot be null or empty.", nameof(id));

                // Define operationCode for API call
                /* string operationCode = "MANUAL.ENTRY";*/ // Or call GetBranchID() if you want to use branch info

                // Make API call including operationCode
                var response = await _JournalheadapiCallerHelper.GetAsync<
                    ResponseObject<WorkflowTicket>>(string.Format(APICallHelper.GetJournalSourceById, id));
                if (response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }
                return null;


                //// Validate response
                //if (!response.IsSuccess)
                //    throw new Exception($"API call failed: {response.Message}");

                //var entry = response.ApiResponseData?.Data;
                //if (entry == null)
                //    throw new Exception("Journal Entry not found.");

                // Optionally, set OperationCode or other properties on the returned entry
                //entry.OperationCode = operationCode;

                //return entry;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"[GetJournalSourceByIdAsync] Error: {ex.Message}");
                throw;
            }
        }
       
        public async Task<JournalApprovalResponse> ApproveSourceAsync(JournalApproval model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveSourceJournalEntry, model); // now sending full model
                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<JournalApprovalResponse> ApproveDestinationAsync(JournalApproval model)
        {
          
            model.DestinationBranchId = model.BranchId;
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
            
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveDestinationJournalEntry, // your destination endpoint
                    model
                );

                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        // REJECT Journal Entry
        public async Task<JournalApprovalResponse> ApproveMemberReconciliationAsync(JournalApproval model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveMemberReconciliation, model); // now sending full model
                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<JournalApprovalResponse> ApproveCashReconciliationAsync(JournalApproval model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveCashReconciliation, model); // now sending full model
                return apiResponse.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
