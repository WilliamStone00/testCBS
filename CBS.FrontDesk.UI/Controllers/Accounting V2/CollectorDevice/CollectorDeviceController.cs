using CBS.BusinessService.Accounting_V2.API;
using CBS.BusinessService.Accounting_V2.CollectorDevice;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API;
using CBS.FrontDesk.Data.Entity.Accounting_V2.CollectorDevice;
using CBS.FrontDesk.Data.Message;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.CollectorDevice
{
   // [CheckSessionTimeOut]
    public class CollectorDeviceController : Controller
    {
        private readonly BranchServices _branchServices;
        private readonly CollectorDeviceService _CollectorDeviceService;

        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public CollectorDeviceController(BranchServices branchServices, CollectorDeviceService collectorDeviceService)
        {
            _branchServices = branchServices;
            _CollectorDeviceService = collectorDeviceService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            //await loader();
            if (path == "list")
            {
                var data = await _CollectorDeviceService.GetAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {
                return PartialView(partialView, new CollectorDeviceresponse());
            }

            else
            {
                var data = await _CollectorDeviceService.GetByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCollectorDevices()
        {
            try
            {
                // Simple GET all - no complex form parsing
                var devices = await _CollectorDeviceService.GetAsync();

                var result = devices.Select(d => new
                {
                    id = d.Id,
                    createdOn = d.CreatedOn.ToString(),
                    deviceName = d.DeviceName ?? string.Empty,
                    deviceSerialNumber = d.DeviceSerialNumber ?? string.Empty,
                    deviceVersion = d.DeviceVersion ?? string.Empty,
                    status = d.Status,
                    assignedCollectorUserId = d.AssignedCollectorUserId ?? string.Empty
                }).ToList();

                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error loading collector devices");
                return Json(new { error = "Error loading data" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(CollectorDeviceresponse model)
        {
            // Use IsNullOrWhiteSpace so empty string Ids don't behave like null
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _CollectorDeviceService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                // IMPORTANT: return the ActionResult from Update
                return await Update(model);
            }

            // unreachable now but keep for safety (or remove)
            // return Json(new { success = false, status = false, message = "Fillsss the required fields." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(CollectorDeviceresponse model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _CollectorDeviceService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _CollectorDeviceService.DeleteAsync(KEY);

            // Map to simple JSON shape the client expects. Adjust if result has different property names.
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

     
    }
}