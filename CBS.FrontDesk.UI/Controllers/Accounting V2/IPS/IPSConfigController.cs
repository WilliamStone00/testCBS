using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.IPS;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.IPS;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.IPS
{
   // [CheckSessionTimeOut]
    public class IPSConfigController : BaseController
    {
        private readonly IPSConfigService _ipsConfigService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _BranchAccountService;

        public IPSConfigController(IPSConfigService ipsConfigService, BranchServices branchServices, BranchAccountService branchAccountService)
        {
            _ipsConfigService = ipsConfigService;
            _branchServices = branchServices;
            _BranchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index()
        {
            await Loader();
            return View(new IPSConfig());
        }

        public async Task<bool> Loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
                      
            return true;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await Loader();
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LoadIPSConfigData(IPSConfigQuery query)
        {
            try
            {
                var data = await _ipsConfigService.GetIPSConfigDataTableAsync(query);
                var ipsConfigs = JsonConvert.DeserializeObject<List<IPSConfig>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = ipsConfigs
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await Loader();

            if (path == "list")
            {
                var data = await _ipsConfigService.GetAllIPSConfigsAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new IPSConfig());
            }
            else if (path == "activeyear")
            {
                if (int.TryParse(KEY, out int year))
                {
                    var data = await _ipsConfigService.GetActiveIPSConfigByYearAsync(year, serviceOption);
                    return PartialView(partialView ?? "_IPSConfigDetailsPartial", data);
                }
                return PartialView(partialView, null);
            }
            else if (path == "year")
            {
                if (int.TryParse(KEY, out int year))
                {
                    var data = await _ipsConfigService.GetIPSConfigsByYearAsync(year);
                    return PartialView(partialView ?? "_IPSConfigYearPartial", data);
                }
                return PartialView(partialView, null);
            }
            else
            {
                var data = await _ipsConfigService.GetIPSConfigByIdAsync(KEY);
                return PartialView(partialView ?? "_IPSConfigDetailsPartial", data);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(IPSConfig model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _ipsConfigService.CreateIPSConfigAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                return await Update(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(IPSConfig model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _ipsConfigService.UpdateIPSConfigAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _ipsConfigService.DeleteIPSConfigAsync(KEY);
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetByActiveYear(int year,string branchid)
        {
            var data = await _ipsConfigService.GetActiveIPSConfigByYearAsync(year,branchid);
            return PartialView("_IPSConfigDetailsPartial", data);
        }

        [HttpGet]
        public async Task<ActionResult> GetByYear(int year)
        {
            var data = await _ipsConfigService.GetIPSConfigsByYearAsync(year);
            return PartialView("_IPSConfigYearPartial", data);
        }

        [HttpGet]
        public async Task<ActionResult> GetBranchAccountsByBranch(string branchId)
        {
            try
            {
                var branchAccounts = await _BranchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);

                var resultList = branchAccounts.Select(a => new
                {
                    Id = a.Id,
                    Name = string.IsNullOrWhiteSpace(a.Name) ? a.Id : $"{a.Name}"
                });

                return Json(resultList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Failed to load branch accounts" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}