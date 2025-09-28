using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

        public async Task<CustomDataTable> GetCounterChequesForDataTableAsync(CounterChequeQuery query)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(APICallHelper.GetCounterChequesForDataTable, query);

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }

                // Return an empty, but valid, CustomDataTable on failure
                return new CustomDataTable(
                    draw: Convert.ToInt32(query.Options?.draw),
                    recordsTotal: 0, recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.Options);
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                return new CustomDataTable(
                    draw: Convert.ToInt32(query.Options?.draw),
                    recordsTotal: 0, recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: query.Options);
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
    }
}