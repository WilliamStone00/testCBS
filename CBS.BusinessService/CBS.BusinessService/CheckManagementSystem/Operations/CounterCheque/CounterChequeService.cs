using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque
{
    public class CounterChequeService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;

        public CounterChequeService()
        {
            // Ensure this key exists in your Web.config and points to the correct service
            var baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);
        }





        public async Task<List<CounterCheques>> GetChequeDetails(string CustomerId, string BranchId)
        {
            try
            {
                // Build the URL with query parameters
                var url = $"{APICallHelper.GetCustormerchequebook}?customerId={CustomerId}&branchId={BranchId}";

                // Call the API with only 1 argument
                var response = await _apiHelper.GetAsync<ResponseObject<List<CounterCheques>>>(url);



                return response.ApiResponseData?.Data ?? new List<CounterCheques>();
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<ExecutionMessages> IssueCounterChequeAsync(CounterCheques model)
        {
            try
            {
                // Add any necessary data from the user's session before sending
                model.BranchId = GetBranchID();
                model.IssuedBy = GetUserFullName();

                var response = await _apiHelper.PostAsync<ServiceResponse<CounterCheques>>(APICallHelper.IssueCounterCheque, model);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Counter Cheque", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Counter cheque issued successfully.");
                }
                else
                {
                    GetExecutionMessages(model, false, "Counter Cheque", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Counter Cheque", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<CounterCheques> GetCounterChequeDetailsAsync(string id)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ServiceResponse<CounterCheques>>($"{APICallHelper.GetCounterChequeDetails}/{Uri.EscapeDataString(id)}");

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                else
                {
                    throw new Exception(response.ApiResponseData?.Message ?? response.Message ?? "Failed to fetch counter cheque details");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching counter cheque details: {ex.Message}");
                throw new Exception($"Unable to retrieve counter cheque details: {ex.Message}", ex);
            }
        }



        public async Task<CustomDataTable> GetCounterChequesForDataTableAsync(CounterChequeQuery query)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.datatableforcounterrequest, query);

                // ⚠ CRITICAL: If API call fails or returns unsuccessful, THROW exception
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

        public async Task<ExecutionMessages> TakeActionAsync(CounterChequeActionDto model)
        {
            try
            {
                string url;
                switch (model.Action?.ToLower())
                {
                    case "review": url = string.Format(APICallHelper.ReviewCounterCheque, model.CounterChequeId); break;
                    case "validate": url = string.Format(APICallHelper.ValidateCounterCheque, model.CounterChequeId); break;
                    case "reject": url = string.Format(APICallHelper.RejectCounterCheque, model.CounterChequeId); break;
                    default:
                        throw new ArgumentException("Invalid action specified for counter cheque.");
                }

                var payload = new { Motive = model.Motive, ActionBy = GetUserFullName() };

                var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(url, payload);

                if (response.IsSuccess && response.ApiResponseData.Data)
                {
                    GetExecutionMessages(model, true, $"Action '{model.Action}'", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Action completed successfully.");
                }
                else
                {
                    GetExecutionMessages(model, false, $"Action '{model.Action}'", MessagesResults.Failed,
                       ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                       response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Action '{model.Action}'", MessagesResults.Error,
                   ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }



        public async Task<CustomerCheckBookDisplayDto> GetfullChequeDetails(string chequeBookId)
        {
            if (string.IsNullOrEmpty(chequeBookId))
                throw new ArgumentException("ChequeBookId is required.");

            try
            {
                var url = string.Format(APICallHelper.GetCustomerChequeBook, chequeBookId);
                var response = await _apiHelper.GetAsync<ResponseObject<CustomerCheckBookDisplayDto>>(url);

                if (response?.ApiResponseData?.Data == null)
                    return null;

                // Since your DTO already represents a single checkbook, just return it
                var checkBook = response.ApiResponseData.Data;

                return checkBook;
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}
