using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.EndOfYearClosure
{
    public class EndOfYearClosureService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;

        public EndOfYearClosureService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());

        }

        public async Task<List<accountingyear>> GetAccountingYearByBranchIdAsync(string branchId , string init)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(branchId))
                    throw new ArgumentException("Branch ID cannot be null or empty.", nameof(branchId));


                // ✅ Make API call
                var url = string.Format(APICallHelper.GetAccoutingYearByBranchId, branchId, init);
                var response = await _apiCallerHelper.GetAsync<ResponseObject<accountingyear>>(url);
                // ✅ Validate response
                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                var data = response.ApiResponseData?.Data;

                if (data == null)
                    throw new Exception("Accounting Year not found.");

                // ✅ Wrap single object into a list
                return new List<accountingyear> { data };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAccountingYearByBranchIdAsync] Error: {ex.Message}");
                throw;
            }
        }


        public async Task<ApiResponse<YearClosureStatus>> GetYearClosureStatusAsync(
    string branchId,
    string accountingYearId)
        {
            if (string.IsNullOrWhiteSpace(branchId) ||
                string.IsNullOrWhiteSpace(accountingYearId))
            {
                return new ApiResponse<YearClosureStatus>
                {
                    Message = "BranchId and AccountingYearId are required."
                };
            }

            try
            {
                var url = string.Format(
                    APICallHelper.GetYearClosureStatus,
                    branchId,
                    accountingYearId
                );

                // 🔁 Same pattern as Shared Month
                var response =
                    await _apiCallerHelper.GetAsync<ResponseObject<YearClosureStatus>>(url);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    return new ApiResponse<YearClosureStatus>
                    {
                        IsSuccess = true,
                        ApiResponseData = response.ApiResponseData.Data,
                        Message = response.Message
                    };
                }

                return new ApiResponse<YearClosureStatus>
                {
                    IsSuccess = false,
                    Message = response.Message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<YearClosureStatus>
                {
                    IsSuccess = false,
                    Message = $"Failed to fetch year closure status: {ex.Message}"
                };
            }
        }





        public async Task<ApiResponse<ResponseObject<bool>>> SaveInitiateClosure(CloseYearInitiate model)
        {
            if (model == null)
                return new ApiResponse<ResponseObject<bool>>
                {
                    IsSuccess = false,
                    Message = "Request model is null"
                };

            try
            {
                var apiResponse = await _apiCallerHelper.PostAsync<ResponseObject<bool>>(
                    APICallHelper.InitiateClosure,
                    model
                );

                // ✅ Return full backend response
                return apiResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ResponseObject<bool>>
                {
                    IsSuccess = false,
                    Message = $"Close of Year API call failed: {ex.Message}"
                };
            }
        }


        public async Task<bool> SaveReviewClosure(ReviewClosureRequest model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _apiCallerHelper.PostAsync<ResponseObject<bool>>(
                    APICallHelper.ReviewClosure,   // your API endpoint
                    model
                );

                return apiResponse?.ApiResponseData?.Data?? false;
            }
            catch
            {
                throw; // preserves stack trace
            }
        }





        public async Task<List<EndOfYearConfig>> GetAllEndTaskAsync()
        {
            try
            {
                var apiResponse = await _apiCallerHelper.GetAsync<ResponseObject<List<EndOfYearConfig>>>(APICallHelper.GetAllEndOfYearTask);

                if (apiResponse == null || apiResponse.ApiResponseData == null)
                    return new List<EndOfYearConfig>();

                return apiResponse.ApiResponseData.Data ?? new List<EndOfYearConfig>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllAsync] Error fetching all Accounting Year: {ex.Message}");
                return new List<EndOfYearConfig>();
            }
        }



        public async Task<CustomDataTable2> AdjustmentEntriesDataTableAsync(YearEndChecklistStatusQuery query)
        {
            try
            {
                //// If not head office and branchId is null, set it
                //if (!IsHeadOffice() && string.IsNullOrEmpty(query.BranchId))
                //{
                //    query.BranchId = GetBranchID();
                //}

                query.Options.sortColumnName = "";
                query.Options.sortColumnDirection = "";

                var journalHeaders = (
                   JsonConvert.SerializeObject(query));

                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetCheckListStatusDataTable, query);

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




        public async Task<ExecutionMessages> PostAdjustmentAsync(EndOfYear model)
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
                if (model.BranchId == null || model.BranchId == string.Empty)
                {
                    model.BranchId = GetBranchID(); // <-- Add this line
                }
               
                model.CorrelationId = "CORR-GL2GL-20251101-02";

                model.AuxiliaryReference = GetUserFullName();
                model.OperationCode = "EOY.ADJ";
                model.ExternalApplicationName = "TSC.BackOffice";
                model.PostMode = "HOLD_FOR_APPROVAL";
                model.Narration = model.Payload.Memo;
                model.CreatedBy = GetUserFullName();
                //model.CorrelationId = "CORR-IB-20251013-01";
                //model.AuxiliaryReference = "AUX-IB-RECLASS-10";


                // ✅ Call API
                var response = await _apiCallerHelper
                    .PostAsync<ServiceResponse<ManualEntryresponnse>>(APICallHelper.PostJournalAdjustment, model);

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
        public async Task<ApiResponse<ResponseObject<bool>>> SaveClosure(CloseOfYear model)
        {
            if (model == null)
                return new ApiResponse<ResponseObject<bool>>
                {
                    IsSuccess = false,
                    Message = "Request model is null"
                };

            model.HOBranchId = GetBranchID();

            try
            {
                var apiResponse = await _apiCallerHelper.PostAsync<ResponseObject<bool>>(
                    APICallHelper.CloseYear,
                    model
                );

                // ✅ Return full backend response
                return apiResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ResponseObject<bool>>
                {
                    IsSuccess = false,
                    Message = $"Close of Year API call failed: {ex.Message}"
                };
            }
        }

    }

}

