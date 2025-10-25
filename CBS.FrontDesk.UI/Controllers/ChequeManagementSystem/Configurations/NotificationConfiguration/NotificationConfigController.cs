using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Configurations.NotificationConfiguration;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationConfig;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Configurations.NotificationConfiguration
{
    [CheckSessionTimeOut]
    public class NotificationConfigController : BaseController
    {
        private readonly NotificationConfigService _notificationConfigService;
        private readonly BranchServices _branchServices;

        public NotificationConfigController(NotificationConfigService notificationConfigService, BranchServices branchServices)
        {
            _notificationConfigService = notificationConfigService;
            _branchServices = branchServices;
        }

        private async Task Loader()
        {
            ViewBag.Branches = await _branchServices.GetBranches();

            var notificationData = await _notificationConfigService.GetNotificationTypesAsync();
            ViewBag.NotificationTypes = notificationData?.NotificationTypes ?? new List<NotificationTypeDto>();
            ViewBag.PlaceHolders = notificationData?.PlaceHolders ?? new List<PlaceholderDto>();
        }

        public async Task<ActionResult> Index()
        {
            await Loader();
            return View(new NotificationConfig());
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await Loader();
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LoadNotificationData(NotificationconfigQuery query)
        {
            try
            {
                var data = await _notificationConfigService.GetNotificationDataTableAsync(query);
                var dtoList = JsonConvert.DeserializeObject<List<Data.Entity.CheckManagementSystem.Configurations.NotificationConfig.NotificationConfigDto>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = dtoList
                });
            }
            catch
            {
                // Simple, user-friendly error response (no internal details)
                return Json(new
                {
                    draw = query?.Options?.draw ?? "",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    success = false,
                    message = "Failed to load notification configurations. Please try again later."
                });
            }
        }

        /// <summary>
        /// Minimal initializer:
        ///  - path == "list" => return partial list
        ///  - path == "new"  => return empty/new model partial
        ///  - otherwise     => return model by KEY (details/edit)
        /// </summary>
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await Loader();

            if (string.Equals(path, "list", StringComparison.OrdinalIgnoreCase))
            {
                var data = await _notificationConfigService.GetConfigsAsync(false, null, null); // default call - service returns enumerable
                return PartialView(partialView ?? "_NotificationListPartial", data);
            }

            if (string.Equals(path, "new", StringComparison.OrdinalIgnoreCase))
            {
                return PartialView(partialView ?? "_Create", new NotificationConfig());
            }

            // default: get by id
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return new HttpStatusCodeResult(400, "KEY is required to fetch a notification configuration.");
            }

            try
            {
                var model = await _notificationConfigService.GetByIdAsync(KEY);
                if (model == null)
                {
                    return HttpNotFound($"Notification configuration '{KEY}' not found.");
                }

                return PartialView(partialView ?? "_NotificationConfigDetails", model);
            }
            catch
            {
                // simple, non-technical error message
                return new HttpStatusCodeResult(500, "Failed to load configuration. Please try again later.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(NotificationConfig model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, status = "Failed", message = "Please fill all required fields." });
            }

            try
            {
                var tpl = model.TemplateBody ?? string.Empty;
                var matches = Regex.Matches(tpl, @"\$[A-Za-z_]\w*");
                if (matches.Count > 0)
                {
                    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var unique = new List<string>();
                    foreach (Match m in matches)
                    {
                        var token = m.Value.Trim();
                        if (!seen.Contains(token))
                        {
                            seen.Add(token);
                            unique.Add(token);
                        }
                    }
                    model.AvailablePlaceholders = string.Join(",", unique);
                }
                else
                {
                    model.AvailablePlaceholders = string.Empty;
                }

                ExecutionMessages data;
                if (string.IsNullOrWhiteSpace(model.Id))
                    data = await _notificationConfigService.CreateAsync(model);
                else
                    data = await _notificationConfigService.UpdateAsync(model);

                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            catch
            {
                return Json(new { success = false, status = "Error", message = "Failed to save configuration. Please try again later." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return Json(new { success = false, status = "Failed", message = "Invalid ID provided for deletion." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var result = await _notificationConfigService.DeleteAsync(KEY);
                return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, status = "Error", message = "Failed to delete configuration. Please try again later." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
