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
        public async Task<List<CBS.FrontDesk.Data.Entity.AccountingV2.AccountDto>> GetAccountsByBranchAsync()
        {
            try
            {
                // ✅ Get required values
                var branchId = "264139464310378"; // or GetBranchID();
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



        public async Task<ExecutionMessages> PostJournalAsync(JournalEntryPayload model)
        {
            try
            {
               
                if (model == null)
                {
                    GetExecutionMessages(model, false, "Journal Entry", MessagesResults.Failed,
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
                    .PostAsync<ServiceResponse<JournalEntryPayload>>(APICallHelper.PostManualJournalEntry, model);

                // ✅ Handle success
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Journal Entry",
                        MessagesResults.Success, ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, "Journal Entry", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Journal Entry", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(),
                    ex, ex.Message);
            }

            return ExecutionMessage;
        }

    }



}





    






