using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.MemberAccountManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CBS.BusinessService.Accounts.MemberReceiptsP
{

    public class AccountManagementService : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        public AccountManagementService(IndividualProfileServices individualProfileServices)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }
               


        public async Task<IEnumerable<ManageAccountStatusCommand>> GetAsync()
        {
            try
            {
                // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                var response = await _transactionApiHelper.GetAsync<ServiceResponse<List<ManageAccountStatusCommand>>>(APICallHelper.GetAllMAM);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<ManageAccountStatusCommand>();
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

        public async Task<CustomDataTable> GetDataTableAsync(ManageAccountmanaQuery query)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.MAMdatatable, query);

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
                throw new Exception($"MAM Service book service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ManageAccountStatusCommand> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetMAMById, encodedId);

                var response = await _transactionApiHelper.GetAsync<ServiceResponse<ManageAccountStatusCommand>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> CreateAsync(ManageAccountStatusCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<ManageAccountStatusCommand>>(APICallHelper.CreateMAM, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(ManageAccountStatusCommand model)
        {
            try
            {
                var catid = model.Id;
                string formattedUrl = string.Format(APICallHelper.UpdateMAM, catid);
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<ManageAccountStatusCommand>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, null, MessagesResults.Success,
                    ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false,null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteAsync(string categoryId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeactivateMAM, categoryId);
                var response = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true,null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message );
                }
                else
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate category.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }


    }
}
