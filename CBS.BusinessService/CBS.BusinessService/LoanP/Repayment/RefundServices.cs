using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.LoanRepayment;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Repayment
{
    public class RefundServices : BaseService
    {
        private readonly ApiCallerHelper _loanApiHelper;
        private readonly ApiCallerHelper _TransactionApiHelper;

        public RefundServices()
        {
            _loanApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _TransactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        /// <summary>
        /// Get refunds by CustomerId with support for IncludeDetails, Take, StartDate, EndDate.
        /// Maps to GET /api/v1/Refund/by-customer/{customerId}?includeDetails={bool}&take={int?}&startDate={yyyy-MM-dd}&endDate={yyyy-MM-dd}
        /// </summary>
        public async Task<List<Refund>> GetRefundsByCustomerId(
            string customerId,
            bool includeDetails = false,
            int? take = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var baseUrl = string.Format(APICallHelper.RefundByCustomerId, Uri.EscapeDataString(customerId));

                var qs = new List<string>();

                if (includeDetails) qs.Add("includeDetails=true"); // only send if true
                if (take.HasValue) qs.Add($"take={take.Value}");
                if (startDate.HasValue) qs.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue) qs.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var url = qs.Count > 0 ? $"{baseUrl}?{string.Join("&", qs)}" : baseUrl;

                var resp = await _loanApiHelper.GetAsync<ResponseObject<List<Refund>>>(url);

                if (resp?.IsSuccess == true && resp.ApiResponseData?.Data != null)
                    return resp.ApiResponseData.Data;

                return new List<Refund>();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get a single refund by its ID.
        /// </summary>
        public async Task<Refund> GetRefundById(string id)
        {
            try
            {
                var url = string.Format(APICallHelper.RefundById, Uri.EscapeDataString(id));
                var resp = await _loanApiHelper.GetAsync<ResponseObject<Refund>>(url);

                if (resp?.IsSuccess == true)
                    return resp.ApiResponseData?.Data;

                return null;
            }
            catch
            {
                throw;
            }
        }

        public async Task<CustomDataTable> GeRefundDataTableAsync(LoanRefundQuery query)
        {
            try
            {
                if (!IsHeadOffice() && string.IsNullOrEmpty(query.BranchId))
                {
                    query.BranchId = GetBranchID();
                }

               

                var response = await _loanApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetLoanDataTable, query);



                if (!response.IsSuccess)
                    throw new Exception($"API call failed: {response.Message}");

                if (response.ApiResponseData == null)
                    throw new Exception("API returned null data");

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error (Reconciliation): {ex.Message}");
                throw new Exception($"Loan service unavailable: {ex.Message}", ex);
            }
        }


        public async Task<ApiResponse<bool>> PushRefundsAsync(List<PartialBulkOperation> operations)
        {
            if (operations == null || !operations.Any())
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = "No refund operations provided.",
                    ApiResponseData = false
                };
            }

            try
            {
                // Prepare the API request payload
                var request = new BUlkRefundReconcilliation
                {
                    RefundCarrierBulkOperations = operations
                };

                // Call your API helper
                var apiResponse = await _TransactionApiHelper.PostAsync<bool>(
                    APICallHelper.PushRefund,
                    request
                );

                return apiResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"API call failed: {ex.Message}",
                    ApiResponseData = false
                };
            }
        }


    }

}
