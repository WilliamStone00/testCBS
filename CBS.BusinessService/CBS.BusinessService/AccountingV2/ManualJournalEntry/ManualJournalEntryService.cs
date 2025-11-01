using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.Config;
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
        //public async Task<List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>> GetAccountsByBranchAsync()
        //{
        //    try
        //    {
        //        // ✅ Get required values
        //        var branchId = GetBranchID();
        //        var language = GetUserLanguage(); // e.g., "en"

        //        // ✅ Validate before sending
        //        if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(language))
        //            return new List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>();

        //        // ✅ Properly replace placeholders in endpoint
        //        var endpoint = APICallHelper.GetAccountsByBranch
        //            .Replace("{branchId}", branchId)
        //            .Replace("{lang}", language);

        //        // ✅ Make API call
        //        var apiResponse = await _manualJournalEntryapiCallerHelper
        //            .GetAsync<ResponseObject<List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>>>(endpoint);

        //        // ✅ Return sorted results if successful
        //        if (apiResponse.IsSuccess && apiResponse.ApiResponseData != null)
        //        {
        //            return apiResponse.ApiResponseData.Data
        //                .OrderBy(x => x.Code)
        //                .ToList();
        //        }

        //        // ✅ Return empty list if no data
        //        return new List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>();
        //    }
        //    catch (Exception ex)
        //    {
        //        //  Proper logging
        //        System.Diagnostics.Debug.WriteLine($"Error fetching accounts by branch: {ex.Message}");
        //        throw;
        //    }
        //}
        // Using your project's full type name for clarity:
        public async Task<List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>> GetAccountsByBranchAsync()
        {
            // Simulate network latency
            await Task.Delay(50);

            // If you already have AccountDto in your project, use that.
            // If not, uncomment the fallback class below and remove the namespace prefix used later.
            /*
            public class AccountDto
            {
                public string AccountNumber { get; set; }
                public string AccountName { get; set; }
                public string Code { get; set; }
                public string Currency { get; set; }
                public bool IsActive { get; set; }
                public string BranchId { get; set; }
            }
            */

            // Build sample data (adjust fields to match your real AccountDto)
            var sample = new List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>
    {
        new CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto
        {
            Code = "001",
            AccountNumber = "1000000001",
            AccountName = "Cash in Hand",
            Currency = "XAF",
            IsActive = true,
            // BranchId = "BR001" // set if property exists
        },
        new CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto
        {
            Code = "010",
            AccountNumber = "1000000010",
            AccountName = "Bank - Main",
            Currency = "XAF",
            IsActive = true,
        },
        new CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto
        {
            Code = "100",
            AccountNumber = "1000000100",
            AccountName = "Suspense Account",
            Currency = "XAF",
            IsActive = false,
        }
    };

            // Return sorted by Code to match your original method behavior
            return sample.OrderBy(x => x.Code).ToList();
        }



        public async Task<ExecutionMessages> PostJournalAsync(JournalEntryPayload model)
        {
            try
            {
               
                if (model == null)
                {
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                        null, "Payload cannot be null.");
                    return ExecutionMessage;

                    
                }
                if (model.BranchId==null || model.BranchId==string.Empty)
                {
                    model.BranchId = GetBranchID(); // <-- Add this line
                }
                
                model.Reference = "ME-20251013-2003435135";
                model.OperationCode = "MANUAL.ENTRY";
                model.ExternalApplicationName = "TSC.BackOffice";
                model.PostMode = "HOLD_FOR_APPROVAL";
                model.Narration = model.Payload.Memo;
                //model.CorrelationId = "CORR-IB-20251013-01";
                //model.AuxiliaryReference = "AUX-IB-RECLASS-10";


                // ✅ Call API
                var response = await _manualJournalEntryapiCallerHelper
                    .PostAsync<ServiceResponse<ManualEntryresponnse>>(APICallHelper.PostManualJournalEntry, model);

                // ✅ Handle success
                // ✅ Handle success or failure
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    var journalData = response.ApiResponseData.Data; // ManualEntryData object

                    GetExecutionMessages(journalData, true, null,
                        MessagesResults.Success, ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);


                   
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(),
                    ex, ex.Message);
            }

            return ExecutionMessage;
        }
            
        }

    }









    






