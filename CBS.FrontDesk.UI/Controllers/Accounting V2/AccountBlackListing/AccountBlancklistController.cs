using CBS.BusinessService.Accounting_V2.AccountBlacklisting;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountBlackList;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccountBlackListing
{
    [CheckSessionTimeOut]
    public class AccountBlancklistController : Controller
    {
        private readonly AffiliateService _AffiliateServices;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        private readonly AffiliateAccountService _AffiliateAccountsServices;
        private readonly AccountBlacklistService _AccountBlacklistService;

        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public AccountBlancklistController(AccountBlacklistService accountBlacklistService , AffiliateAccountService affiliateAccountService, BranchAccountService branchAccountService, AffiliateService affiliateService, BranchServices branchServices)
        {
            _AffiliateServices = affiliateService;
            _branchServices = branchServices;
            _branchAccountService = branchAccountService;
            _AffiliateAccountsServices = affiliateAccountService;
            _AccountBlacklistService = accountBlacklistService;
        }
        // GET: AccountBlancklist
        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new AccountBlacklist());
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var Menu = await Menudropdown();
            ViewBag.Menu = Menu;

            var affiliateAccounts = await _AffiliateAccountsServices.GetAllAffiliateAccounts();
            ViewBag.AffiliateAccounts = affiliateAccounts;
            return true;
        }

        public async Task<List<SelectListItem>> Menudropdown()
        {
            await Task.CompletedTask; // for async signature consistency

            var menuItems = new List<SelectListItem>
                {                    
                    new SelectListItem { Text = "Manual entries", Value = "ManualEntries" },
                    new SelectListItem { Text = "Other Cash in", Value = "OtherCashIn" },
                    new SelectListItem { Text = "Other cash out", Value = "OtherCashOut" },
                    new SelectListItem { Text = "Member to GL", Value = "MemberToGL" },
                    new SelectListItem { Text = "GL to Member", Value = "GLToMember" },
                    new SelectListItem { Text = "GL to GL", Value = "GLToGL" }
                };

            return menuItems;
        }

        [HttpGet]
        public async Task<ActionResult> GetBranchAccountsByBranch(string branchId)
        {
            try
            {
                var branchAccounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);

                var resultList = branchAccounts.Select(a => new
                {
                    Id = a.Id,
                    Name = string.IsNullOrWhiteSpace(a.Name) ? a.Id : $"[{a.Code}] - {a.Name}"
                });

                return Json(resultList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Failed to load branch accounts" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }

        // Note: use [FromBody] so model binder reads the JSON DataTables sends.
        [HttpPost]
        public async Task<JsonResult> LoadData(AccountBlacklistQuery query)
        {
            try
            {
                var data = await _AccountBlacklistService.GetDataTableAsync(query);

                var Response = JsonConvert.DeserializeObject<List<AccountBlacklistresponse>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Response
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
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
            //await loader();
            if (path == "list")
            {
                var data = await _AccountBlacklistService.GetAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {
                return PartialView(partialView, new AccountBlacklist());
            }

            else
            {
                var data = await _AccountBlacklistService.GetByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(AccountBlacklist model)
        {
            // Use IsNullOrWhiteSpace so empty string Ids don't behave like null
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _AccountBlacklistService.CreateAsync(model);
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
        public async Task<ActionResult> Update(AccountBlacklist model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _AccountBlacklistService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _AccountBlacklistService.DeleteAsync(KEY);

            // Map to simple JSON shape the client expects. Adjust if result has different property names.
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

    }
}