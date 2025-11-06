using CBS.BusinessService.Accounting_V2;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.Pendingaccounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.PendingAccount;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.BranchAccounting
{
    public class BranchAccountController : Controller
    {
        private readonly AffiliateAccountService _AffiliateAccountService;
        private readonly AffiliateAccountMockService _affiliateAccountMockService;
        private readonly AffiliateService _AffiliateService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        private readonly ChartOfAccountsV2mockService _accountsService;
        private readonly ChartOfAccountsV2Service _chartOfAccountsV;
        private readonly PendingAccountsService _pendingAccountsService;

        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public BranchAccountController(ChartOfAccountsV2mockService chartOfAccountsV2MockService, AffiliateAccountService affiliateaccountService, BranchServices branchServices, AffiliateService affiliateService, AffiliateAccountMockService affiliateAccountMockService, BranchAccountService branchAccountService, ChartOfAccountsV2Service chartOfAccountsV, PendingAccountsService pendingAccountsService)
        {
            _AffiliateAccountService = affiliateaccountService;
            _branchServices = branchServices;
            _AffiliateService = affiliateService;
            _affiliateAccountMockService = affiliateAccountMockService;
            _branchAccountService = branchAccountService;
            _accountsService = chartOfAccountsV2MockService;
            _chartOfAccountsV = chartOfAccountsV;
            _pendingAccountsService = pendingAccountsService;
        }




        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();

        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

          /*  var affiliate = await _affiliateAccountMockService.GetAffiliatesFromMockAsync();
            ViewBag.Affiliates = affiliate;

            var Chartofaccount = await _affiliateAccountMockService.GetAffiliatesFromMockAsync();
            ViewBag.HoPcmfAccountId = Chartofaccount;*/

            var affiliateAccounts = await _AffiliateAccountService.GetAllAffiliateAccounts1();
            ViewBag.AffiliateAccounts = affiliateAccounts;

            var classes = _chartOfAccountsV.GetAllClass2();
            ViewBag.Classes = classes;

            ViewBag.Languages = new List<SelectListItem>
            {
                new SelectListItem { Text = "English", Value = "en" },
                new SelectListItem { Text = "French",  Value = "fr" }
            };

            return true;
        }

        [HttpPost]
        public async Task<JsonResult> LoadBranchData(BranchAccountQuery query)
        {
            //await loader();
            try
            {

                var data = await _branchAccountService.GetDataTableAsync(query);
              
                var Affiliate = JsonConvert.DeserializeObject<List<Data.Entity.Accounting_V2.BranchAccount.BranchAccountResponse>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Affiliate
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
            await loader();
            if (path == "list")
            {
                var data = await _branchAccountService.GetAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {

                var model = new PendingAccountRequest();
                model.Scope = "Branch";
                model.RequiresMapping = false;
                model.Language = _branchAccountService.GetUserLanguage();
                model.AffiliateId = "1";


                if (serviceOption == "root")
                {
                    model.IsOrigin = true;
                }
                else
                {

                    if (!string.IsNullOrWhiteSpace(KEY))
                    {
                        // set ParentId so the view receives it in the hidden field

                        var parentData = await _branchAccountService.GetByIdAsync(KEY);
                        model.ParentId = parentData?.Id;
                        model.Class = parentData?.Class;
                        model.AffiliateAccountId = parentData?.AffiliateAccountId;
                        model.ParentAccountNumber = parentData?.Code;
                        model.Code = AccountManagementPositionCalculator.ComposeChildCode(parentData?.Code,"");
                        model.BranchId = parentData?.BranchId;


                    }

                  
                }
                 
                return PartialView(partialView, model);
            }
            else
            {
                var data = await _branchAccountService.GetByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }

        public async Task<ActionResult> InitializeData2(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
                 var data = await _branchAccountService.GetByIdfordetailsAsync(KEY);
                return PartialView(partialView, data);
                      
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(PendingAccountRequest model)
        {
            // Use IsNullOrWhiteSpace so empty string Ids don't behave like null
          /*  if (string.IsNullOrWhiteSpace(model.Id))
            {*/
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _pendingAccountsService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
           /* }
            else
            {
                // IMPORTANT: return the ActionResult from Update
                return await Update(model);
            }*/

            // unreachable now but keep for safety (or remove)
            // return Json(new { success = false, status = false, message = "Fillsss the required fields." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(BranchAccountCommand model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _branchAccountService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _branchAccountService.DeleteAsync(KEY);

            // Map to simple JSON shape the client expects. Adjust if result has different property names.
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

    }
}
