// File: CBS.FrontDesk.UI.Controllers/Accounting_V2/AccountBlackListing/AccountBlancklistController.cs
using CBS.BusinessService.Accounting_V2.AccountBlacklisting;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountBlackList;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccountBlackListing
{
    /// <summary>
    /// MVC5 controller for Account Menu Blacklisting UI:
    /// - Index: create/update view
    /// - List: filters + server-side DataTable
    /// - LoadData: DataTable POST endpoint
    /// - InitializeData: load partials (new/edit/details)
    /// - Branch account AJAX helper
    /// - CreateOrUpdate / Update / Delete
    /// </summary>
    [CheckSessionTimeOut]
    public class AccountBlancklistController : Controller
    {
        private readonly AffiliateService _affiliateServices;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        private readonly AffiliateAccountService _affiliateAccountsServices;
        private readonly AccountBlacklistService _accountBlacklistService;

        /// <summary>Inject dependencies used by the UI.</summary>
        public AccountBlancklistController(
            AccountBlacklistService accountBlacklistService,
            AffiliateAccountService affiliateAccountService,
            BranchAccountService branchAccountService,
            AffiliateService affiliateService,
            BranchServices branchServices)
        {
            _affiliateServices = affiliateService;
            _branchServices = branchServices;
            _branchAccountService = branchAccountService;
            _affiliateAccountsServices = affiliateAccountService;
            _accountBlacklistService = accountBlacklistService;
        }

        /// <summary>Landing page for the configuration form.</summary>
        public async Task<ActionResult> Index()
        {
            await Loader();
            return View(new AccountBlacklist());
        }

        /// <summary>Common dropdown loads (Branches, Menu, AffiliateAccounts).</summary>
        private async Task<bool> Loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            ViewBag.Menu = await Menudropdown();
            ViewBag.AffiliateAccounts = await _affiliateAccountsServices.GetAllAffiliateAccounts();

            return true;
        }

        /// <summary>Static list for Menu dropdown (keep values in sync with back-end “Menu”).</summary>
        public async Task<List<SelectListItem>> Menudropdown()
        {
            await Task.CompletedTask;
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Manual entries", Value = "ManualEntries" },
                new SelectListItem { Text = "Other Cash in", Value = "OtherCashIn" },
                new SelectListItem { Text = "Other cash out", Value = "OtherCashOut" },
                new SelectListItem { Text = "Member to GL", Value = "MemberToGL" },
                new SelectListItem { Text = "GL to Member", Value = "GLToMember" },
                new SelectListItem { Text = "GL to GL", Value = "GLToGL" }
            };
        }

        /// <summary>AJAX: Return branch accounts for a given branch.</summary>
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
            catch
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Failed to load branch accounts" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>Filters view with server-side DataTable.</summary>
     
        public async Task<ActionResult> List()
        {
            await Loader();
            return View();
        }

        /// <summary>
        /// DataTables server-side endpoint.
        /// Sample JSON POST (payload is AccountBlacklistQuery):
        /// 
        /// {
        ///   "BranchID": "BR-001",
        ///   "Menu": "ManualEntries",
        ///   "AffiliateAccountId": "AFFACC-123",
        ///   "CreatedFromUtc": "2025-10-01T00:00:00Z",
        ///   "CreatedToUtc": "2025-10-31T23:59:59Z",
        ///   "Options": {
        ///     "draw": "1", "start": 0, "length": 10, "skip": 0, "pageSize": 10,
        ///     "searchValue": "", "sortColumnName": "CreatedDate", "sortColumnDirection": "desc"
        ///   }
        /// }
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> LoadData(AccountBlacklistQuery query)
        {
            try
            {
                // Map front-end query → your service call (which wraps the back-end DataTable handler)
                var data = await _accountBlacklistService.GetDataTableAsync(query);

                // deserialise to front-end row model (so table uses consistent casing)
                var rows = JsonConvert.DeserializeObject<List<AccountBlacklistresponse>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = rows
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

        /// <summary>Load partial views for details/edit/new.</summary>
        [HttpGet]
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await Loader();
            // You can extend path switching here
            if (string.Equals(path, "list", StringComparison.OrdinalIgnoreCase))
            {
                var data = await _accountBlacklistService.GetAsync();
                return PartialView(partialView, data);
            }
            else if (string.Equals(path, "new", StringComparison.OrdinalIgnoreCase))
            {
                return PartialView(partialView, new AccountBlacklist());
            }
            else if (string.Equals(path, "Edit", StringComparison.OrdinalIgnoreCase))
            {
                var data = await _accountBlacklistService.Edit(KEY);
                return PartialView(partialView, data);
            }
            else
            {
                var data = await _accountBlacklistService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
        }

        /// <summary>Create or update a blacklist configuration.</summary>
        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(AccountBlacklist model)
        {
          
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });
                model.AffiliateId = (await _affiliateServices.GetAffiliateAsync())?.Id;
                var result = await _accountBlacklistService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                return await Update(model);
            }
        }

        /// <summary>Update an existing blacklist configuration.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(AccountBlacklist model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _accountBlacklistService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        /// <summary>Delete a blacklist row (consider switching to POST later).</summary>
        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _accountBlacklistService.DeleteAsync(KEY);
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success, message }, JsonRequestBehavior.AllowGet);
        }
    }
}
