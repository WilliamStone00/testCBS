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
            // Use IsNullOrWhiteSpace so empty string Ids don't behave like null
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _CategoryConfigService.CreateCategoryAsync(model);
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

        //[HttpPost]
        //public async Task<ActionResult> CreateOrUpdate(CategoryConfig model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        // For a validation failure, success should definitely be false.
        //        return Json(new
        //        {
        //            success = false,
        //            status = "ValidationError", // Provide a more specific status
        //            message = "Validation failed. Please check the form for errors."
        //        });
        //    }

        //    ExecutionMessages result;
        //    string operationType;

        //    if (string.IsNullOrWhiteSpace(model.Id))
        //    {
        //        result = await _CategoryConfigService.CreateCategoryAsync(model);
        //        operationType = "Insert";
        //    }
        //    else
        //    {
        //        result = await _CategoryConfigService.UpdateCategoryAsync(model);
        //        operationType = "Update";
        //    }

        //    bool isAjaxSuccess = result.Result ||
        //                         (result.MessageStatus == SystemMessageStatus.Exist.ToString());

        //     return Json(new
        //    {
        //        success = isAjaxSuccess,
        //        status = result.MessageStatus,
        //        message = Messaging.MessageResult(result),
        //        optype = operationType,
        //        reloadDataView = "Yes",
        //        controllerName = "Categoryconfic",
        //        divLoaderList = "datalistingview",
        //        tableName = "myDataTable",
        //        dataLoaderActionName = "_CategoryDataTable",
        //        divLoaderCreator = "datalistingview",
        //        reinitializedActionName = "_Categories"
        //    });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(CategoryConfig model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _CategoryConfigService.UpdateCategoryAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." });

            var result = await _CategoryConfigService.DeactivateCategoryAsync(KEY);
            // We will make the script that calls this expect the simple response
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }


        //[HttpGet]
        //public async Task<ActionResult> Delete(string KEY)
        //{
        //    var result = await _CategoryConfigService.DeactivateCategoryAsync(KEY);

        //    // Your DeleteRecordDataTable script expects a full response object
        //    // so it knows what/how to reload.
        //    if (result.Result)
        //    {
        //        // On success, return a message AND instructions to reload the data.
        //        return Json(new
        //        {
        //            success = true,
        //            status = result.MessageStatus,
        //            message = Messaging.MessageResult(result),
        //            // The reload instructions:
        //            controller = "Categoryconfic",
        //            datatable = "myDataTable",
        //            PartialView = "_CategoryDataTable",
        //            pr = 0,
        //            div = "datalistingview",
        //            id = (string)null,
        //            path = "list"
        //        }, JsonRequestBehavior.AllowGet);
        //    }

        //    // On failure, just return the error message.
        //    return Json(new
        //    {
        //        success = false,
        //        status = result.MessageStatus,
        //        message = Messaging.MessageResult(result)
        //    }, JsonRequestBehavior.AllowGet);
        //}


    }
}
