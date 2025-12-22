using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.AndriodApp;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
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
        private readonly BranchServices _branchServices;


        public JournalHeadService()
        {

            _JournalheadapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

       
        public async Task<CustomDataTable2> GetJournalHeaderDataTableAsync(JournalEntryQuery query)
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
                var response = await _JournalheadapiCallerHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.AccountingV2.JournalHead>>(string.Format(APICallHelper.GetJournalEntryTempById, id));

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








        public async Task<CustomDataTable2> GetJournalSourceDataTableAsync(JournalEntryQuery journalEntry)
        {
            try
            {
                // Build the base query
                var query = new GetallWorkFlowTicketsQuery
                {
                    BranchId = journalEntry.BranchId, // decide below
                    CounterpartyBranchId = null, // decide below
                    TicketType = journalEntry.TicketSource, // server will filter by TicketType
                    FromDate = journalEntry.StartDate,
                    ToDate = journalEntry.EndDate,
                    State = null,
                    DataTableOptions = journalEntry.Options ?? new DataTableOptions()
                };

                // Force client-side sort params empty; server orders by OpenedAtUtc
                query.DataTableOptions.sortColumnName = string.Empty;
                query.DataTableOptions.sortColumnDirection = string.Empty;

                // Apply branch filters per rules
                if (IsHeadOffice())
                {
                    // Head Office: get ALL — leave BranchId & CounterpartyBranchId as null
                }
                else
                {
                    var myBranchId = GetBranchID();
                    var isDestination = string.Equals(journalEntry.TicketSource, "Destination", StringComparison.OrdinalIgnoreCase);

                    if (isDestination)
                    {
                        // Non-HO + Destination: filter by CounterpartyBranchId only
                        query.CounterpartyBranchId = myBranchId;
                        query.BranchId = null; // ensure BranchId is NOT set
                    }
                    else
                    {
                        // Non-HO + Source (or anything else): filter by BranchId only
                        query.BranchId = myBranchId;
                        query.CounterpartyBranchId = null; // ensure Counterparty is NOT set
                        //query.TicketType = "Source";
                    }
                }

                var response = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetJournalworkticketDataTable, query);

                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

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
                
                // Make API call including operationCode
                var response = await _JournalheadapiCallerHelper.GetAsync<
                    ResponseObject<WorkflowTicket>>(string.Format(APICallHelper.GetJournalSourceById, id));
                if (response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }
                return null;


                
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"[GetJournalSourceByIdAsync] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<ApiResponse< ResponseObject<JournalApprovalResponse>>> ApproveSourceAsync(JournalApproval model)
        {
            model.BranchId = null;
            model.DestinationBranchId = null;
            model.TicketType = null;
            model.Approve = true;
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveSourceJournalEntry, model); // now sending full model
               /* if (apiResponse.IsSuccess)
                {
                    return apiResponse?.ApiResponseData?.Data;
                }*/
                return apiResponse;
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ApiResponse<ResponseObject<JournalApprovalResponse>>> ApproveDestinationAsync(JournalApproval model)
        {

            model.DestinationBranchId = model.BranchId;
            model.BranchId = null;
            model.SourceBranchId = null;
            model.TicketType = null;   
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {

                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveDestinationJournalEntry, // your destination endpoint
                    model
                );

                return apiResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        // REJECT Journal Entry
        public async Task<ApiResponse<ResponseObject<JournalApprovalResponse>>> ApproveMemberReconciliationAsync(JournalApproval model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveMemberReconciliation, model); // now sending full model
                return apiResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ApiResponse<ResponseObject<JournalApprovalResponse>>> ApproveCashReconciliationAsync(JournalApproval model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveCashReconciliation, model); // now sending full model
                return apiResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<ApiResponse<ResponseObject<JournalApprovalResponse>>> RejectAsync(JournalApproval model)
        {
            
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<JournalApprovalResponse>>(
                    APICallHelper.ApproveSourceJournalEntry, model); // now sending full model
                return apiResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseObject<FilterResponse>> GetFilters(GetFirlterData model)
        {
            var response = await _JournalheadapiCallerHelper
                .PostAsync<ResponseObject<FilterResponse>>(APICallHelper.GetFilter, model);

            return response.ApiResponseData;
        }


    }
}
