using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Configurations.ChequeNumber;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeNumber;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.ChequeNumber
{
    //[CheckSessionTimeOut]
    public class NumConfigController : Controller
    {
        private readonly NumConfigService _NumConfigService;
        private readonly BranchServices _branchServices;

        /// <summary>
        /// Injects the required NumConfigService via dependency injection.
        /// </summary>
        /// <param name="NumConfigService">The service for cheque admin operations.</param>
        public NumConfigController(NumConfigService NumConfigService, BranchServices branchServices)
        {
            _NumConfigService = NumConfigService;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new NumConfig()); // pass categories as model
        }
        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            return true;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();

        }

        [HttpPost]
        public async Task<JsonResult> LoadNumConfigData(NumconfogQuery query)
        {
            try
            {
                // Call the main service to get the DataTable
                var data = await _NumConfigService.GetNumConfigDataTableAsync(query);

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = data.data
                });
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"LoadNumConfigData Error: {ex.Message}");

                // Return a clean JSON response for DataTable even on error
                return Json(new
                {
                    draw = query.DataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = $"Failed to load NumConfig data: {ex.Message}"
                });
            }
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "list")
            {
                var data = await _NumConfigService.GetAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {
                await loader();
                return PartialView(partialView, new NumConfig());
            }

            else
            {
                await loader();
                var data = await _NumConfigService.GetByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(NumConfig model)
        {
            if (model.Id == null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _NumConfigService.CreateAsync(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
            }
            else
            {
                await Update(model);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(NumConfig model)
        {
            if (ModelState.IsValid)
            {
                var data = await _NumConfigService.UpdateAsync(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            var result = await _NumConfigService.DelateAsync(KEY);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }
    }
}
