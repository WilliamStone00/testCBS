using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
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
            var baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"]; // Add this to your config
            if (string.IsNullOrEmpty(baseUrl))
                throw new ConfigurationErrorsException("The 'CustomerDataMartBaseUrl' appSetting is missing or empty in Web.config.");

            _apiHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<dynamic> GetCustomerByIdAsync(string customerId)
        {
            try
            {
                //string url = $"/api/v1/GetCustomerDataMart/{customerId}";

                // Use the existing GetAsync pattern with dynamic type
               // var response = await _apiHelper.GetAsync<ServiceResponse<dynamic>>(url);
                var response = await _apiHelper.GetAsync<ServiceResponse<List<CategoryConfig>>>(APICallHelper.getcustomerdata);

                if (response != null && response.IsSuccess)
                {
                    // Return the data part of the response
                    return response.ApiResponseData?.Data;
                }
                else
                {
                    throw new Exception(response?.ApiResponseData?.Message ?? "Failed to retrieve customer data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching customer data: {ex.Message}");
            }
        }
    }
}
