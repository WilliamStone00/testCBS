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

        public CollectorDeviceController(BranchServices branchServices, CollectorDeviceService collectorDeviceService)
        {
            _branchServices = branchServices;
            _CollectorDeviceService = collectorDeviceService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }
        public async Task<ActionResult> List()
        {
            return View();
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "list")
            {
                var data = await _CollectorDeviceService.GetAsync();
                return PartialView(partialView, data);
            }
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
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetDeviceDetails(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            try
            {
                var device = await _CollectorDeviceService.GetByIdAsync(KEY);
                if (device == null)
                    return Json(new { success = false, message = "Device not found." }, JsonRequestBehavior.AllowGet);

                var result = new
                {
                    success = true,
                    data = new
                    {
                        deviceId = device.DeviceId,
                        deviceName = device.DeviceName,
                        deviceSerialNumber = device.DeviceSerialNumber,
                        deviceVersion = device.DeviceVersion,
                        status = device.Status,
                        assignedCollectorUserId = device.AssignedCollectorUserId ?? string.Empty,
                        assignedOn = device.AssignedOn.ToString(),
                        createdOn = device.CreatedOn.ToString(),
                        modifiedOn = device.ModifiedOn.ToString()
                    }
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading device details." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetDeviceDetailsPartial(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Content("<div class='alert alert-danger'>Invalid ID provided.</div>");

            try
            {
                var device = await _CollectorDeviceService.GetByIdAsync(KEY);
                if (device == null)
                    return Content("<div class='alert alert-danger'>Device not found.</div>");

                return PartialView("_CollectorDeviceDetails", device);
            }
            catch (Exception ex)
            {
                return Content("<div class='alert alert-danger'>Error loading device details.</div>");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(CollectorDeviceresponse model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _CollectorDeviceService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                return await Update(model);
            }
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
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _CollectorDeviceService.DeleteAsync(KEY);

            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }
        
    }
}