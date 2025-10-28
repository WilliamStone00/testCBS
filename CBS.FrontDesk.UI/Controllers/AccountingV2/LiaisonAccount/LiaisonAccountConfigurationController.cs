using CBS.BusinessService.Accounting;
using CBS.BusinessService.AccountingV2.BranchCashConfigV;
using CBS.BusinessService.AccountingV2.LiaisonAccountConfiguration; // adjust namespace as needed
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonAccountConfiguration;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.AccountingV2.LiaisonAccountConfiguration;
using Microsoft.AspNetCore.Mvc;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.LiaisonAccount
{
    public class LiaisonAccountConfigurationController : BaseController
    {
        private readonly LiaisonAccountConfigurationService _liaisonService;
        private readonly MockLiaisonAccountConfiguration _mockData;
        private readonly BranchServices _branchServices;
        private readonly ChartOfAccountServicesAnnex _chartOfAccountService;
        private readonly BranchCashConfigService _branchCashConfig;

        public LiaisonAccountConfigurationController(
            LiaisonAccountConfigurationService liaisonService,
            MockLiaisonAccountConfiguration mockLiaisonAccountConfiguration,
            BranchServices branchServices,
            ChartOfAccountServicesAnnex chartOfAccountService,
            BranchCashConfigService branchCashConfig
            )

        {
            _branchCashConfig = branchCashConfig;
            _liaisonService = liaisonService;
            _branchServices = branchServices;
            _chartOfAccountService = chartOfAccountService;
            _mockData = mockLiaisonAccountConfiguration;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                await InitializeDropdowns();
                return View();

            }
            catch (Exception)
            {
                return RedirectToAction("InternalServer", "Error");
            }
        }

        public async Task InitializeDropdowns()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            ViewBag.Liasons = await _chartOfAccountService.GetGLAccountsQueryByBranch();
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string branchId = null, string liaisonAccountId = null)
        {
            if (path == "list")
            {
                // Get all data first (you might want to implement server-side filtering)
                var data = await _liaisonService.GetLiaisonAccountMappings();

                // Client-side filtering (consider implementing server-side filtering in service)
                if (!string.IsNullOrEmpty(branchId))
                {
                    data = data.Where(x => x.BranchId == branchId);
                }
                if (!string.IsNullOrEmpty(liaisonAccountId))
                {
                    data = data.Where(x => x.HeadOfficeLiaisonAccountId == liaisonAccountId);
                }

                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                await InitializeDropdowns();
                return PartialView(partialView, new LiaisonAccountConfigurationDto());
            }
            else
            {
                await InitializeDropdowns();
                var data = await _liaisonService.GetLiaisonAccountMapping(KEY);
                return PartialView(partialView, data);
            }
        }


        [HttpPost]
        public async Task<ActionResult> Create(LiaisonAccountConfigurationDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    status = "error",
                    message = "Validation failed",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (string.IsNullOrEmpty(model.BranchId))
            {
                return Json(new { success = false, message = "Branch is required" });
            }

            if (string.IsNullOrEmpty(model.HeadOfficeLiaisonAccountId))
            {
                return Json(new { success = false, message = "Liaison account is required" });
            }

            // ✅ Fixed logic: Check if this is a new record (Id is empty) or existing record
            if (string.IsNullOrEmpty(model.Id))
            {
                var data = await _liaisonService.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                return await Update(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Update(LiaisonAccountConfigurationDto model)
        {
            var data = await _liaisonService.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _liaisonService.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetAccountsByBranch(string branchId)
        {
            try
            {
                // Fetch accounts for the selected branch
                var accounts = await _chartOfAccountService.GetGLAccountsQueryByBranch(branchId);

                if (accounts != null)
                {
                    var result = accounts.Select(a => new
                    {
                        id = a.Value,
                        text = a.Text
                    });

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //public async Task<ActionResult> Ajaxloader(string Key)
        //{
        //    var listing = await _chartOfAccountService.GetGLAccountsQueryByBranch(Key);
        //    return Json(listing, JsonRequestBehavior.AllowGet);
        //}

        //public async Task<IActionResult> GetBranches(string branchId)
        //{
        //    var result = await _branchCashConfig.GetBranchesAsync(branchId);

        //    return Json(result);
        //}
    }
}