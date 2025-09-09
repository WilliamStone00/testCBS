using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.UI.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem
{
    //[CheckSessionTimeOut]
    public class CategoryconficController : BaseController
    {
        private readonly CategoryConfigService _CategoryConfigService;
        private readonly BranchServices _branchServices;

        /// <summary>
        /// Injects the required CategoryConfigService via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public CategoryconficController(CategoryConfigService CategoryConfigService,BranchServices branchServices)
        {
            _CategoryConfigService = CategoryConfigService;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new CategoryConfig()); // pass categories as model
        }
        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
             ViewBag.Branches = branches;

            return true;
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "list")
            {
                var data = await _CategoryConfigService.GetCategoriesAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {
                await loader();
                return PartialView(partialView, new CategoryConfig());
            }

            else
            {
                await loader();
                var data = await _CategoryConfigService.GetCategoryByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(CategoryConfig model)
        {
            if (model.Id == null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _CategoryConfigService.CreateCategoryAsync(model);
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
        public async Task<ActionResult> Update(CategoryConfig model)
        {
            if (ModelState.IsValid)
            {
                var data = await _CategoryConfigService.UpdateCategoryAsync(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            var result = await _CategoryConfigService.DeactivateCategoryAsync(KEY);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }
    }
}
