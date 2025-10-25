using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;

using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2
{
    public class ManualJournalEntryService : BaseService
    {
        private readonly ApiCallerHelper _manualJournalEntryapiCallerHelper;
        

       // private readonly List<JournalEntry> _mockClearances;
        public ManualJournalEntryService()
        {

            _manualJournalEntryapiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }
        public async Task<List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>> GetAccountsByBranchAsync()
        {
            try
            {
                // ✅ Get required values
                var branchId = "BR001"; // or GetBranchID();
                var language = GetUserLanguage(); // e.g., "en"

                // ✅ Validate before sending
                if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(language))
                    return new List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>();

                // ✅ Properly replace placeholders in endpoint
                var endpoint = APICallHelper.GetAccountsByBranch
                    .Replace("{branchId}", branchId)
                    .Replace("{lang}", language);

                // ✅ Make API call
                var apiResponse = await _manualJournalEntryapiCallerHelper
                    .GetAsync<ResponseObject<List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>>>(endpoint);

                // ✅ Return sorted results if successful
                if (apiResponse.IsSuccess && apiResponse.ApiResponseData != null)
                {
                    return apiResponse.ApiResponseData.Data
                        .OrderBy(x => x.Code)
                        .ToList();
                }

                // ✅ Return empty list if no data
                return new List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>();
            }
            catch (Exception ex)
            {
                //  Proper logging
                System.Diagnostics.Debug.WriteLine($"Error fetching accounts by branch: {ex.Message}");
                throw;
            }
        }

        
        public async Task<CBS.API.Helper.ApiResponse<object>> PostJournalEntryAsync(JournalEntry journalEntry)
        {
            try
            {
                var endpoint = APICallHelper.PostJournalEntry;

                // Directly pass the JournalEntry object
                var apiResponse = await _manualJournalEntryapiCallerHelper
                    .PostAsync<object>(endpoint, journalEntry);

                return apiResponse;
            }
            catch (Exception)
            {
                throw; // Let the controller handle exceptions
            }
        }

        


       

        



        


    }


}


    






