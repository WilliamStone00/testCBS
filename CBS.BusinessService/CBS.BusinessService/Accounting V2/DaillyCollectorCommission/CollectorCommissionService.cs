using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
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
        private readonly ApiCallerHelper _CustomerApiCallerHelper;
        private readonly ApiCallerHelper _CheckbookApiCallerHelper;
        private readonly IndividualProfileServices _individualProfileServices;
        
        public CollectorCommissionService(IndividualProfileServices individualProfileServices)
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
            _CustomerApiCallerHelper = new ApiCallerHelper(baseUrl1);

            string baseUrl2 = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl2))
            {
                throw new ConfigurationErrorsException("The baseUrl is missing or empty in Web.config.");
            }
            _CheckbookApiCallerHelper = new ApiCallerHelper(baseUrl2);
            _individualProfileServices = individualProfileServices;
        }

        public async Task<CustomDataTable2> CustomerDataTableAsync(customerQuery query)
        {
            try
            {

                var response = await _CustomerApiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
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

                var response = await _CustomerApiCallerHelper.GetAsync<ServiceResponse<Customer>>(formattedUrl);

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

        /// <summary>
        /// Gets job snapshot from the API
        /// </summary>
        public async Task<ExecutionMessages> GetJobSnapshotAsync(string jobId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    GetExecutionMessages(
                        null,
                        false,
                        null,
                        MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Failed.ToString(),
                        null,
                        "Job ID is required"
                    );

                    return ExecutionMessage;
                }

                var encodedId = Uri.EscapeDataString(jobId);
                string formattedUrl = string.Format(APICallHelper.GetJobSnapshot, encodedId);

                var response = await _apiCallerHelper
                    .GetAsync<ServiceResponse<JobSnapshotDto>>(formattedUrl);

                if (response != null &&
                    response.ApiResponseData?.Success == true &&
                    response.ApiResponseData.Data != null)
                {
                    GetExecutionMessages(
                        response.ApiResponseData.Data,
                        true,
                        null,
                        MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(),
                        null,
                        response.ApiResponseData.Message
                    );
                }
                else
                {
                    GetExecutionMessages(
                        null,
                        false,
                        null,
                        MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject,
                        SystemMessageStatus.Failed.ToString(),
                        null,
                        response?.ApiResponseData?.Message ?? "Failed to retrieve job snapshot"
                    );
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(
                    null,
                    false,
                    null,
                    MessagesResults.Error,
                    ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Error.ToString(),
                    ex,
                    ex.Message
                );
            }

            return ExecutionMessage;
        }


        public async Task<CustomerDataDto> GetCustomerAccountDropdownAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException(nameof(customerId));

            var agrAggregates = await _individualProfileServices.GetAggregates();
            var customerProfile = await _individualProfileServices.GetCustomer(customerId, agrAggregates);
          
            // Defensive: ensure accounts list exists
            var accounts = customerProfile.CustomerAccounts ?? new List<CustomerAccount>();

            var vm = new CustomerDataDto
            {
                CustomerDto = customerProfile.CustomerList,
                AccountSelectList = accounts
                    .Select(a => new SelectListItem
                    {
                        Value = a.accountNumber ?? string.Empty, // sent back to backend when selected
                        Text = string.IsNullOrWhiteSpace(a.accountName)
                            ? $"{a.accountNumber} ({a.balance:N2})"
                            : $"{a.accountNumber} - {a.accountName} ({a.balance:N2})"
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

        //    var response = await _CheckbookApiCallerHelper.PostAsync<ServiceResponse<CollectorComissionResponse>>(APICallHelper.Reconciliation, model);

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

