using CBS.BusinessService.CheckManagementSystem.Configurations.NotificationConfiguration.NotificationHistory;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Configurations.NotificationConfiguration.NotificationHistory
{
    [CheckSessionTimeOut]
    public class NotificationHistoryController : BaseController
    {
        private readonly NotificationHistoryService _historyService;
        private readonly BranchServices _branchServices;
        // Inject customer service if needed for customer search dropdown

        public NotificationHistoryController(NotificationHistoryService historyService, BranchServices branchServices)
        {
            _historyService = historyService;
            _branchServices = branchServices;
        }

        // This action loads the main page with the filter controls.
        public async Task<ActionResult> Index()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            // ViewBag.Customers = ... load customers for a dropdown
            return View();
        }

        // This is the dedicated AJAX endpoint for the DataTable.
        [HttpPost]
        public async Task<ActionResult> LoadHistoryData(string searchBy, string value)
        {
            if (string.IsNullOrWhiteSpace(searchBy) || string.IsNullOrWhiteSpace(value))
            {
                // Return an empty data set if no search criteria is provided
                return Json(new { data = new List<NotificationHistoryDto>() });
            }

            List<NotificationHistoryDto> data;

            // Route the request to the correct service method based on the user's choice.
            if (searchBy.Equals("branch", StringComparison.OrdinalIgnoreCase))
            {
                data = await _historyService.GetHistoryByBranchAsync(value);
            }
            else if (searchBy.Equals("customer", StringComparison.OrdinalIgnoreCase))
            {
                data = await _historyService.GetHistoryByCustomerAsync(value);
            }
            else
            {
                data = new List<NotificationHistoryDto>();
            }

            // Return the data in the format the client-side DataTable expects
            return Json(new { data = data });
        }
    }
}