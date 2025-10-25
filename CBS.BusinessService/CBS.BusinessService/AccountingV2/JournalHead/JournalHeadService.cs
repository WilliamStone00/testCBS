using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountongV2;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.JournalHead
{
    public class JournalHeadService : BaseService
    {

        private readonly ApiCallerHelper _JournalheadapiCallerHelper;
        

        public JournalHeadService()
        {

            _JournalheadapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

        public async Task<CustomDataTable> GetJournalHeaderDataTableAsync(JournalEntryQuery query)
        {
            try
            {
                

                var response = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
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

        public async Task<JournalEntry> GetJournalEntryByIdAsync(string id)
        {
            try
            {
                var response = await _JournalheadapiCallerHelper.GetAsync<ResponseObject<JournalEntry>>(
                    $"{APICallHelper.GetJournalEntryById}?id={id}"
                );



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
                throw; // Rethrow so the controller can handle/log it
            }
        }




        public async Task<CustomDataTable> GetJournalSourceDataTableAsync(JournalEntryQuery query)
        {
            try
            {
                var response = await _JournalheadapiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
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

        //public async Task<JournalEntry> GetJournalEntryByIdAsync(string id)
        //{
        //    await Task.Delay(50); // simulate async delay
        //    var entry = _mockEntries.FirstOrDefault(e => e.Id == id); // <-- connected here
        //    if (entry == null)
        //        throw new Exception("Journal Entry not found.");
        //    return entry;
        //}



        //public async Task<CustomDataTable> GetJournalHeaderDataTableAsync(JournalEntryQuery query)
        //{
        //    await Task.Delay(50); // simulate async delay

        //    // Use the same _mockEntries here
        //    var enrichedEntries = _mockEntries.Select((x, i) => new
        //    {
        //        Id = x.Id,
        //        CreatedAt = x.AccountingDate.AddDays(-1),
        //        Reference = $"ME-20251013-20{i + 1:D2}",
        //        Narrative = x.Narration,
        //        BranchName = x.BranchId,
        //        AccountingDate = x.AccountingDate,
        //        PostMode = x.PostMode
        //    }).ToList();

        //    // Filters...
        //    var filtered = enrichedEntries.AsEnumerable();
        //    if (!string.IsNullOrWhiteSpace(query.BranchId))
        //        filtered = filtered.Where(x => x.BranchName.Equals(query.BranchId, StringComparison.OrdinalIgnoreCase));
        //    if (!string.IsNullOrWhiteSpace(query.Reference))
        //        filtered = filtered.Where(x => x.Reference.IndexOf(query.Reference, StringComparison.OrdinalIgnoreCase) >= 0);
        //    if (query.AccountingDate.HasValue)
        //        filtered = filtered.Where(x => x.AccountingDate.Date == query.AccountingDate.Value.Date);
        //    if (!string.IsNullOrWhiteSpace(query.PostMode))
        //        filtered = filtered.Where(x => x.PostMode.Equals(query.PostMode, StringComparison.OrdinalIgnoreCase));

        //    int drawValue = 1;
        //    if (!string.IsNullOrEmpty(query?.Options?.draw))
        //        int.TryParse(query.Options.draw, out drawValue);

        //    return await Task.FromResult(new CustomDataTable
        //    {
        //        draw = drawValue,
        //        recordsTotal = enrichedEntries.Count,
        //        recordsFiltered = filtered.Count(),
        //        data = filtered.ToList()
        //    });
        //}

        //private readonly List<JournalEntry> _mockEntries = new List<JournalEntry>
        //{
        //    new JournalEntry
        //    {
        //        Id = "00000001",
        //        OperationCode = "MANUAL.ENTRY",
        //        BranchId = "1",
        //        BranchName = "H O D",
        //        Reference = " ME-20251013-20",
        //        CounterpartyBranchId = "BR-007",
        //        AccountingDate = DateTime.Parse("2025-10-13"),
        //        PostMode = "Manual",
        //        CorrelationId = "CORR-IB-20251013-01",
        //        Narration = "Interbranch reclass via back-office",
        //        ExternalApplicationName = "TSC.BackOffice",
        //        Payload = new JournalPayload
        //        {
        //            Memo = "Manual interbranch treatment (notify destination for completion)",
        //            AllowUnbalanced = false,
        //            Entries = new List<JournalEntryLine>
        //            {
        //                new JournalEntryLine { AffiliateAccountId = "AFF-210900", Naration = "Due to BR-007", Dr = false, Cr = true, Amount = 120000m },
        //                new JournalEntryLine { AffiliateAccountId = "AFF-130900", Naration = "Due from BR-007", Dr = true, Cr = false, Amount = 120000m }
        //            }
        //        }
        //    },
        //    new JournalEntry
        //    {
        //        Id = "000000002",
        //        OperationCode = "MANUAL.ENTRY",
        //        BranchId = "BR-002",
        //        BranchName = "Bam",
        //        Reference = " ME-20251013-21",
        //        CounterpartyBranchId = "BR-008",
        //        AccountingDate = DateTime.Parse("2025-10-14"),
        //        PostMode = "Automatic",
        //        CorrelationId = "CORR-IB-20251014-02",
        //        Narration = "Manual transfer adjustment",
        //        ExternalApplicationName = "TSC.BackOffice",
        //        Payload = new JournalPayload
        //        {
        //            Memo = "Manual interbranch transfer adjustment",
        //            AllowUnbalanced = false,
        //            Entries = new List<JournalEntryLine>
        //            {
        //                new JournalEntryLine { AffiliateAccountId = "AFF-310100", Naration = "Transfer to BR-008", Dr = false, Cr = true, Amount = 50000m },
        //                new JournalEntryLine { AffiliateAccountId = "AFF-320200", Naration = "Transfer from BR-008", Dr = true, Cr = false, Amount = 50000m }
        //            }
        //        }
        //    }
        //};

    }
}
