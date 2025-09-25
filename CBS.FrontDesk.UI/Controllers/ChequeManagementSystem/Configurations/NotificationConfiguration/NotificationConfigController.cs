// Location: ~/Controllers/ChequeManagementSystem/Configurations/NotificationConfiguration/NotificationConfigController.cs

using CBS.BusinessService.CheckManagementSystem.Configurations.NotificationConfiguration;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationConfig;
using CBS.FrontDesk.Data.Message;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Configurations.NotificationConfiguration
{
    [CheckSessionTimeOut]
    public class NotificationConfigController : BaseController
    {
        private readonly NotificationConfigService _notificationConfigService;
        private readonly BranchServices _branchServices;

        // Constructor for Dependency Injection
        public NotificationConfigController(NotificationConfigService notificationConfigService, BranchServices branchServices)
        {
            _notificationConfigService = notificationConfigService;
            _branchServices = branchServices;
        }

        /// <summary>
        /// Action to load the main container view (Index.cshtml).
        /// </summary>
        public async Task<ActionResult> Index()
        {
            // Pre-load data needed for the selection dropdowns on the main page.
            await Loader();
            return View(new NotificationConfig());
        }

        /// <summary>
        /// Helper method to load common data (Branches, Notification Types) into the ViewBag.
        /// </summary>
        //private async Task Loader()
        //{
        //    ViewBag.Branches = await _branchServices.GetBranches();
        //    ViewBag.NotificationTypes = await _notificationConfigService.GetNotificationTypesAsync();
        //}

        private async Task Loader()
        {
            ViewBag.Branches = await _branchServices.GetBranches();

            // Get both notification types and placeholders
            var notificationData = await _notificationConfigService.GetNotificationTypesAsync();
            ViewBag.NotificationTypes = notificationData?.NotificationTypes ?? new List<NotificationTypeDto>();
            ViewBag.PlaceHolders = notificationData?.PlaceHolders ?? new List<PlaceholderDto>();
        }

        /// <summary>
        /// This is the core action for the dynamic UI. It's called via AJAX.
        /// It fetches an existing configuration or creates a new one, then returns it
        /// to the client inside a Partial View.
        /// </summary>
        public async Task<ActionResult> InitializeData(string partialView, bool? isCentralized, string notificationType, string branchId = null)
        {
            bool isCentralizedValue = isCentralized ?? false;
            await Loader();
            NotificationConfig model = null;

            if (!string.IsNullOrWhiteSpace(notificationType))
            {
                // Use our mock service to find a matching configuration.
                var configs = await _notificationConfigService.GetConfigsAsync(isCentralizedValue, isCentralizedValue ? null : branchId, notificationType);
                model = configs.FirstOrDefault();
            }

            // If no existing configuration was found, create a new, pre-populated model.
            if (model == null)
            {
                model = new NotificationConfig
                {
                    IsCentralized = isCentralizedValue,
                    BranchId = isCentralizedValue ? null : branchId,
                    NotificationType = notificationType,
                    IsActive = true, // Default to active
                };
            }

            // Also, get the full definition for the selected type to show placeholders.
            //var allTypes = await _notificationConfigService.GetNotificationTypesAsync();
            //ViewBag.CurrentTypeDefinition = allTypes.FirstOrDefault(t => t.Value == notificationType);

            return PartialView(partialView, model);
        }

        /// <summary>
        /// Handles the submission of the configuration form for both creating new
        //  and updating existing configurations.
        /// </summary>
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(NotificationConfig model)
        {
            
            if (model.Name == null)
            {
                return Json(new { success = false, status = "Failed", message = "Please fill in value for Name." });

            }
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, status = "Failed", message = "Please fill all required fields." });
            }

            ExecutionMessages data;
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                data = await _notificationConfigService.CreateAsync(model);
            }
            else
            {
                data = await _notificationConfigService.UpdateAsync(model);
            }
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        /// <summary>
        /// Handles the deletion of a notification configuration.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return Json(new { success = false, status = "Failed", message = "Invalid ID provided for deletion." });
            }

            var result = await _notificationConfigService.DeleteAsync(KEY);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }
    }
}