using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationHistory;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Configurations.NotificationConfiguration.NotificationHistory
{
    public class NotificationHistoryService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;
        private readonly BranchServices _branchServices; // To get BranchName
                                                         // Inject a customer service if you have one, to get CustomerName

        public NotificationHistoryService(BranchServices branchServices)
        {
            var baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"]; // Or appropriate base URL
            _apiHelper = new ApiCallerHelper(baseUrl);
            _branchServices = branchServices;
        }

        public async Task<List<NotificationHistoryDto>> GetHistoryByBranchAsync(string branchId)
        {
            string url = string.Format(APICallHelper.GetNotificationHistoryByBranch, branchId);

            // As you requested, check if the user is Head Office
            if (!IsHeadOffice())
            {
                // If not Head Office, force the search to be for the user's current branch
                url = string.Format(APICallHelper.GetNotificationHistoryByBranch, GetBranchID());
            }

            var response = await _apiHelper.GetAsync<ResponseObject<List<NotificationHistoryDto>>>(url);
            var historyList = response?.ApiResponseData?.Data ?? new List<NotificationHistoryDto>();

            // Optional but recommended: Enrich the data with Branch Names for a better UI
            var branches = await _branchServices.GetBranches();
            foreach (var item in historyList)
            {
                item.BranchName = branches.FirstOrDefault(b => b.Id == item.BranchId)?.Name ?? item.BranchId;
            }

            return historyList;
        }

        public async Task<List<NotificationHistoryDto>> GetHistoryByCustomerAsync(string customerId)
        {
            string url = string.Format(APICallHelper.GetNotificationHistoryByCustomer, customerId);
            var response = await _apiHelper.GetAsync<ResponseObject<List<NotificationHistoryDto>>>(url);
            return response?.ApiResponseData?.Data ?? new List<NotificationHistoryDto>();
        }
    }
}
