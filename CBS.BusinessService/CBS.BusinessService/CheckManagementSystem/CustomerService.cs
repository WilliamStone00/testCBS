using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;

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

       

            public async Task<CustomerDataDto> GetCustomerAccountDropdownAsync(string customerId)
            {
                if (string.IsNullOrWhiteSpace(customerId))
                    throw new ArgumentException(nameof(customerId));

                var url = string.Format(APICallHelper.getcustomerdata, Uri.EscapeDataString(customerId));
                var response = await _apiHelper.GetAsync<ServiceResponse<CustomerDataDto>>(url);

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
                            Value = a.Id ?? string.Empty, // sent back to backend when selected
                            Text = string.IsNullOrWhiteSpace(a.AccountName)
                                ? $"{a.AccountNumber} ({a.Balance:N2})"
                                : $"{a.AccountNumber} - {a.AccountName} ({a.Balance:N2})"
                        })
                        .OrderBy(x => x.Text)
                        .ToList()
                };

                return vm;
            }


}
}
