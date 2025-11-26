// DailyCollectorLicenseController.cs
using BusinessServices;
using CBS.BusinessService.Accounting_V2.CollectorDevice;
using CBS.BusinessService.Accounting_V2.DaillyCollectorLicenseService;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorLicense;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.DaillyCollectorLicense
{
    public class DailyCollectorLicenseController : Controller
    {
        private readonly ManualDailyCollectionService _manualService;
        private readonly DailyCollectorLicenseService _licenseService;
        private readonly UserManagementServices _userManagementServices;
        private readonly CollectorDeviceService _CollectorDeviceService;
        private readonly BranchServices _branchServices;

        public DailyCollectorLicenseController(
            DailyCollectorLicenseService licenseService,
            UserManagementServices userManagementServices,
            BranchServices baseService, CollectorDeviceService collectorDeviceService, ManualDailyCollectionService manualDailyCollectionService)
        {
            _licenseService = licenseService;
            _userManagementServices = userManagementServices;
            _branchServices = baseService;
            _CollectorDeviceService = collectorDeviceService;
            _manualService = manualDailyCollectionService;
        }

        public async Task<ActionResult> Index()
        {
            await LoadDropdownData();
            return View(new GenerateLicenseRequest());
        }

        public async Task<ActionResult> List()
        {
            await LoadDropdownData();
            return View();
        }

        private async Task LoadDropdownData()
        {
            var branches = await _branchServices.GetBranches();
            var devices = await _CollectorDeviceService.GetDevicedropAsync();

            ViewBag.Branches = branches;
            ViewBag.Devices = devices;
        }


        [HttpPost]
        public async Task<JsonResult> LoadData(LicenseQuery query)
        {
         
            try
            {
                var data = await _licenseService.GetDataTableAsync(query);
                var dataTable = JsonConvert.DeserializeObject<CustomDataTable3>(JsonConvert.SerializeObject(data));
                var licenses = JsonConvert.DeserializeObject<List<DailyCollectorLicense>>(JsonConvert.SerializeObject(dataTable.data));

                return Json(new
                {
                    draw = dataTable.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = licenses
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.dataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        // Add this method to DailyCollectorLicenseController.cs
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,
            string path = null, string serviceOption = null)
        {
            if (path == "list")
            {
                var data = await _licenseService.GetCollectorLicensesAsync(KEY);
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                await LoadDropdownData();
                return PartialView(partialView, new GenerateLicenseRequest());
            }
            else if (path == "details")
            {
                var data = await _licenseService.GetLicenseByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else if (path == "action")
            {
                var data = await _licenseService.GetLicenseByIdAsync(KEY);
                var actionModel = new LicenseActionRequest
                {
                    CollectorUserName = data.CollectorUserName,
                    LicenseId = data?.Id ?? KEY,
                    ActionType = serviceOption // "Revoke", "Deactivate", "Reactivate", "Extend"
                };
                return PartialView(partialView, actionModel);
            }
            else if (path == "activate")
            {
                var data = await _licenseService.GetLicenseByIdAsync(KEY);
                var actionModel = new ActivateLicenseRequest
                {
                   CollectorUserName = data.CollectorUserName,
                  LicenseCode = data.LicenseCode,
                    CollectorUserId = data.CollectorUserId
                };
                return PartialView(partialView, actionModel);
            }
            else if (path == "status")
            {
                return PartialView(partialView, new CheckLicenseStatusRequest());
            }
            else
            {
                var data = await _licenseService.GetLicenseByIdAsync(KEY);
                return PartialView(partialView, data);
            }
        }

        // Add this action method
        [HttpPost]
        public async Task<ActionResult> PerformAction(LicenseActionRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _licenseService.PerformLicenseActionAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Generate(GenerateLicenseRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _licenseService.GenerateLicenseAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Revoke(RevokeLicenseRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _licenseService.RevokeLicenseAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(DeactivateLicenseRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _licenseService.DeactivateLicenseAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reactivate(ReactivateLicenseRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _licenseService.ReactivateLicenseAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(ActivateLicenseRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _licenseService.ActivateLicenseAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<JsonResult> CheckStatus(CheckLicenseStatusRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var data = await _licenseService.CheckLicenseStatusAsync(model);
            return Json(new { success = data != null, data = data });
        }

        [HttpGet]
        public async Task<JsonResult> GetCollectorLicenses(string collectorUserId)
        {
            if (string.IsNullOrWhiteSpace(collectorUserId))
            {
                return Json(new { success = false, message = "Collector User ID is required" },
                    JsonRequestBehavior.AllowGet);
            }

            var data = await _licenseService.GetCollectorLicensesAsync(collectorUserId);
            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> GetCollectorsByBranch(string branchId)
        {
            try
            {
                int order = 2;
                // This would call your service to get collectors by branch
                var collectors = await _manualService.GetCollectorsAsSelectListAsync(branchId, order);

                return Json(collectors, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}