using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem
{
    // Add this to your ChequeRequestService.cs or create a new CustomerService.cs

    public class CustomerService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;

        public CustomerService()
        {
            var baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"]; 
            if (string.IsNullOrEmpty(baseUrl))
                throw new ConfigurationErrorsException("The 'CustomerDataMartBaseUrl' appSetting is missing or empty in Web.config.");

            _apiHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<dynamic> GetCustomerByIdAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException(nameof(customerId));

            try
            {
                // Build URL: substitute placeholder correctly
                var url = string.Format(APICallHelper.getcustomerdata, Uri.EscapeDataString(customerId));
               
                // Use the correct response DTO type
                var response = await _apiHelper.GetAsync<ServiceResponse<CustomerDataDto>>(url);

                if (response != null && response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data; // object with customerDto + accountDtos
                }

                // If response shows a failure, include API messages in exception
                var message = response?.ApiResponseData?.Message ?? response?.Message ?? "Failed to retrieve customer data";
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                // Prefer logging full exception (with inner) to diagnose; rethrow or return null as needed
                // _logger.LogError(ex, "Error fetching customer data for {CustomerId}", customerId);
                throw new Exception($"Error fetching customer data: {ex.Message}", ex);
            }
        }

    }
}
