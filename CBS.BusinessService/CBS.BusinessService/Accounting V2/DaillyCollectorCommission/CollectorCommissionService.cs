using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using ZXing.QrCode.Internal;

namespace CBS.BusinessService.Accounting_V2.Affiliate
{
    public class CollectorCommissionService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly ApiCallerHelper _apiCallerHelper1;
        private readonly ApiCallerHelper _apiCallerHelper2;

        public CollectorCommissionService()
        {
            //change the base url to the actual base url
            string baseUrl = ConfigurationManager.AppSettings["TransactionBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The baseUrl is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);

            string baseUrl1 = ConfigurationManager.AppSettings["CustomerBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl1))
            {
                throw new ConfigurationErrorsException("The baseUrl is missing or empty in Web.config.");
            }
            _apiCallerHelper1 = new ApiCallerHelper(baseUrl1);

            string baseUrl2 = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl2))
            {
                throw new ConfigurationErrorsException("The baseUrl is missing or empty in Web.config.");
            }
            _apiCallerHelper2 = new ApiCallerHelper(baseUrl2);
        }

        public async Task<CustomDataTable2> CustomerDataTableAsync(customerQuery query)
        {
            try
            {

                var response = await _apiCallerHelper1.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.Customerdatatable, query);

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
                throw new Exception($"service unavailable: {ex.Message}", ex);
            }
        }


        public async Task<CollectorComissionResponse> GetCommissionAsync(CollectorComissionResponse model)
        {
            model.OperationType = "CashIn";

            var response = await _apiCallerHelper.PostAsync<ServiceResponse<CollectorComissionResponse>>(APICallHelper.DaillyCollectorCommission, model);

            // check response for success / nulls
            if (response == null || response.ApiResponseData == null || response.ApiResponseData.Data == null)
            {
                // optionally throw or return null and let caller handle
                return null;
            }

            return response.ApiResponseData.Data;
        }

        public async Task<Customer> GetCustomerByIdAsync(string id)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);

                string formattedUrl = string.Format(APICallHelper.getcustomerbyid, encodedId);
                // formattedUrl => "/api/v1/get-checkbook-category/123" (no colon)

                var response = await _apiCallerHelper1.GetAsync<ServiceResponse<Customer>>(formattedUrl);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

        public async Task<CustomerDataDto> GetCustomerAccountDropdownAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException(nameof(customerId));

            var url = string.Format(APICallHelper.getcustomerdata, Uri.EscapeDataString(customerId));
            var response = await _apiCallerHelper2.GetAsync<ServiceResponse<CustomerDataDto>>(url);

            if (response == null || !response.IsSuccess || response.ApiResponseData?.Data == null)
                throw new Exception(response?.ApiResponseData?.Message ?? response?.Message ?? "Failed to fetch customer data.");

            var data = response.ApiResponseData.Data;

            // Defensive: ensure accounts list exists
            var accounts = data.AccountDtos ?? new List<AccountDto>();

            var vm = new CustomerDataDto
            {
                CustomerDto = data.CustomerDto,
                AccountSelectList = accounts
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccountNumber ?? string.Empty, // sent back to backend when selected
                        Text = string.IsNullOrWhiteSpace(a.AccountName)
                            ? $"{a.AccountNumber} ({a.Balance:N2})"
                            : $"{a.AccountNumber} - {a.AccountName} ({a.Balance:N2})"
                    })
                    .OrderBy(x => x.Value)
                    .ToList()
            };

            return vm;
        }

        public async Task<ExecutionMessages> CreateAsync(Payment model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.PostPayment, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            // Ensure ExecutionMessage has a boolean Success property set inside GetExecutionMessages
            return ExecutionMessage;
        }


        //public async Task<CollectorComissionResponse> CreateAsync(CollectorComissionResponse model)
        //{

        //    var response = await _apiCallerHelper2.PostAsync<ServiceResponse<CollectorComissionResponse>>(APICallHelper.Reconciliation, model);

        //    if (response == null || response.ApiResponseData == null || response.ApiResponseData.Data == null)
        //    {
        //        return null;
        //    }

        //    return response.ApiResponseData.Data;
        //}

        public async Task<CustomDataTable> CommisionDataTableAsync(commisionQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.DailyCollectorcommissiondata, query);

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
                throw new Exception($"service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<Affiliateresponse> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.DaillyCollectorGetbyId, encodedId);

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<Affiliateresponse>>(formattedUrl);

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
    }
}

