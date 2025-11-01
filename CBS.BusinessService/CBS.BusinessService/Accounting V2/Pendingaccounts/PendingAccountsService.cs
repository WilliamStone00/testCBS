using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FileUpload;
using CBS.FrontDesk.Data.Entity.Accounting_V2.PendingAccount;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.Pendingaccounts
{
    public class PendingAccountsService : BaseService
    {
        private readonly List<AffiliateAccountDto> _mockAffiliates;
        private readonly ApiCallerHelper _apiCallerHelper;

        public PendingAccountsService()
        {
            //change the base url to the actual base url
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }


        public async Task<CustomDataTable2> DataTableAsync(PendingAccountQuery query)
        {
            try
            {
                var str = JsonConvert.SerializeObject(query);
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.Datatable, query);

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
                throw new Exception($"Error Getting Pending Account Creation Request : {ex.Message}", ex);
            }
        }

        public async Task<ExecutionMessages> CreateAsync(PendingAccountRequest model)
        {
            try
            {

               // string formattedUrl = string.Format(APICallHelper.pendingupdatebyid, Id);
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<PendingAccountDto>>(APICallHelper.AddAccountCreationRequest, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.NameEn, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.NameEn, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.NameEn, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(BranchAccountResponse model)
        {
            try
            {
                string Id = model.Id;
                string formattedUrl = string.Format(APICallHelper.pendingupdatebyid, Id);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<CategoryConfig>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.Name, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        

        public async Task<PendingAccountDto> GetByIdAsync(string Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id)) return null;
                var endpoint = string.Format(APICallHelper.AccountCreationRequestById, Uri.EscapeDataString(Id),GetUserLanguage());
                var response = await _apiCallerHelper.GetAsync<ResponseObject<PendingAccountDto>>(endpoint);
                return response?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                // Log ex
                return null;
            }
        }

        public async Task<ExecutionMessages> ValidateRequestAsync(RequestAction requestAction)
        {
            try
            {
                requestAction.Language = GetUserLanguage();
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.AccountCreationRequestApproval, requestAction);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Validate Pending Account request", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, "Success", null, "Account Creation Request Validated successfully.");
                }
                else
                {
                    var message = response?.ApiResponseData?.Message ?? response?.Message ?? "Validation operation failed";
                    GetExecutionMessages(requestAction, false, "Validate Pending Account ", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, "Failed", null, message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(requestAction, false, "Validate Pending Account request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> RejectRequestAsync(RequestAction requestAction)
        {
            try
            {
                requestAction.Language = GetUserLanguage();
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.AccountCreationRequestRejection, requestAction);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Reject Pending Accounts", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, "Success", null, "Account Creation Request rejected successfully.");
                }
                else
                {
                    var message = response?.ApiResponseData?.Message ?? response?.Message ?? "Reject operation failed";
                    GetExecutionMessages(requestAction, false, "Reject Correspondence", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, "Failed", null, message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(requestAction, false, "Reject Correspondence", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, "Error", ex, ex.Message);
            }

            return ExecutionMessage;
        }
              
    }

}

