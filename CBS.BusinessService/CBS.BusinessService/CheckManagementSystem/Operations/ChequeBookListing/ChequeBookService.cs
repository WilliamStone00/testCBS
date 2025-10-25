using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing
{
    public class ChequeBookService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public ChequeBookService()
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'CheckbookServiceBaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        //public async Task<CustomDataTable> GetChequeBooksDataTableAsync(ChequeBookQuery query)
        //{
        //    try
        //    {
        //        var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
        //            APICallHelper.GetChequeBooksDataTable, query);

        //        if (response.IsSuccess && response.ApiResponseData != null)
        //        {
        //            return response.ApiResponseData.Data;
        //        }

        //        return new CustomDataTable(
        //            draw: Convert.ToInt32(query.DataTableOptions.draw),
        //            recordsTotal: 0,
        //            recordsFiltered: 0,
        //            data: new List<object>(),
        //            dataTableOptions: query.DataTableOptions
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error fetching cheque books data: {ex.Message}");
        //    }
        //}
      
        public async Task<CustomDataTable> GetChequeBooksDataTableAsync(ChequeBookQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetChequeBooksDataTable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
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
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ChequeBook> GetChequeBookByIdAsync(string chequeBookId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.GetChequeBookById, chequeBookId);
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<ChequeBook>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching cheque book details: {ex.Message}");
            }
        }

        public async Task<ExecutionMessages> CancelChequeBookAsync(string chequeBookId, string cancellationReason)
        {
            try
            {
                var payload = new { ChequeBookId = chequeBookId, Reason = cancellationReason };
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CancelChequeBook, payload);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, "Cheque Book", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "Cheque book cancelled successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, "Cheque Book", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Cheque Book", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> MarkLeafAsUsedAsync(string leafId, string statement)
        {
            try
            {
                var payload = new { LeafId = leafId, Statement = statement };
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.MarkLeafAsUsed, payload);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "Leaf marked as used successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> BlockLeafAsync(string leafId, string blockReason)
        {
            try
            {
                var payload = new { LeafId = leafId, Reason = blockReason };
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.BlockChequeLeaf, payload);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                        null, response.ApiResponseData?.Message ?? "Leaf blocked successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }
    }
}
