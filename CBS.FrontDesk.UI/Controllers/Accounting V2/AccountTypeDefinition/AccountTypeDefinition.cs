using CBS.BusinessService.Accounting_V2.AccountTypeDefinition;
using CBS.BusinessService.Accounting_V2.AccountTypeGroup;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountTypeDefinition;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountTypeGroup;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccountTypeDefinition
{
   // [CheckSessionTimeOut]
    public class AccountTypeDefinitionController : Controller
    {
        private readonly AccountTypeDefinitionService _accountTypeDefinitionService;
        private readonly AccountTypeGroupService _AccountTypeGroupService;

        public AccountTypeDefinitionController(AccountTypeDefinitionService accountTypeDefinitionService, AccountTypeGroupService accountTypeGroupService)
        {
            _accountTypeDefinitionService = accountTypeDefinitionService;
            _AccountTypeGroupService = accountTypeGroupService;
        }

        public async Task<bool> loader()
        {
            var AccountTypeGroups = await _AccountTypeGroupService.GetAllAccountTypeGroupsAsync();
            ViewBag.AccountTypeGroups = AccountTypeGroups;

            var ParentAccountTypes = await _accountTypeDefinitionService.GetAccountTypeDefinitionAsync();
            ViewBag.ParentAccountTypes = ParentAccountTypes;
            
            return true;

        }

        public async Task< ActionResult> Index()
        {
            await loader();
            return View();
        }

        public async Task<ActionResult> List()
        {
            return View();
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await loader();
            if (path == "list")
            {
                partialView = "List";
                var data = await _accountTypeDefinitionService.GetAllAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                var model = new AccountTypeDefinitionDto
                {
                    DisplayOrder = 1,
                    IsActive = true,
                    IsUserStandard = false
                };
                return PartialView(partialView, model);
            }
            else
            {
                var data = await _accountTypeDefinitionService.GetByIdAsync(KEY);
               
                return PartialView(partialView, data);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetAccountTypeDEF()
        {
            try
            {
                var groups = await _accountTypeDefinitionService.GetAllAsync();

                return Json(new { data = groups }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error loading data" }, JsonRequestBehavior.AllowGet);
            }
        }
            [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(AccountTypeDefinitionCommand model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                var result = await _accountTypeDefinitionService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                var result = await _accountTypeDefinitionService.UpdateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "Invalid ID provided." });

            var result = await _accountTypeDefinitionService.DeleteAsync(id);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        //[HttpGet]
        //public async Task<JsonResult> GetAccountTypesForDropdown(bool activeOnly = true)
        //{
        //    try
        //    {
        //        var accountTypes = await _accountTypeDefinitionService.GetDropdownDataAsync(activeOnly);
        //        var dropdownData = accountTypes.Select(at => new
        //        {
        //            id = at.Id,
        //            text = $"[{at.Code}] - {at.Name}",
        //            code = at.Code,
        //            name = at.Name,
        //            isActive = at.IsActive
        //        }).ToList();

        //        return Json(new { success = true, data = dropdownData }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpGet]
        public async Task<JsonResult> GetByGroup(string groupId)
        {
            try
            {
                var accountTypes = await _accountTypeDefinitionService.GetByGroupIdAsync(groupId);
                return Json(new { success = true, data = accountTypes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}