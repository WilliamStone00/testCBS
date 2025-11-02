
using CBS.BusinessService.AccountingV2;
using CBS.BusinessService.AccountingV2.VaultInitialisation;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.VaultInitialisation
{
    public class CashReconciliationController : Controller
    {

        private readonly BranchServices _branchServices;
        private readonly CashReconciliationService _cashReconciliationService;


        public CashReconciliationController(BranchServices branchServices, CashReconciliationService cashReconciliationService, ManualJournalEntryService manualJournalEntryService)
        {

            _branchServices = branchServices;
            _cashReconciliationService = cashReconciliationService;


        }
        // GET: CashReconciliation
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await Loader();
            return View();
        }


        public async Task<bool> Loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            return true;
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReconciliationAccounts(string branchId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(branchId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Branch ID is required."
                    }, JsonRequestBehavior.AllowGet);
                }

                var data = await _cashReconciliationService.GetCashReconciliationAccountsAsync(branchId);

                if (data == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No account data found for this branch."
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    message = "Accounts retrieved successfully.",
                    data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
               
                return Json(new
                {
                    success = false,
                    message = "An unexpected error occurred. Please try again later."
                }, JsonRequestBehavior.AllowGet);
            }
        }



      

        [HttpGet]
        public JsonResult GetAccountBalance(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return Json(new { success = false, message = "Account ID is required" }, JsonRequestBehavior.AllowGet);

            var balance = _cashReconciliationService.GetAccountBalance(accountId);
            return Json(new { success = true, balance = balance }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SaveCashReconciliation(CashAndVaultInit model)
        {
            if (model == null)
                return Json(new { success = false, message = " Invalid or empty model." });

            try
            {
                var execMessage = await _cashReconciliationService.Create(model);

                if (execMessage == null)
                    return Json(new { success = false, message = "No response from service." });

                if (!execMessage.Result)
                    return Json(new
                    {
                        success = false,
                        message = execMessage.MessageString ?? " Failed to save cash reconciliation.",
                        data = execMessage.Data
                    });

                return Json(new
                {
                    success = true,
                    message = execMessage.MessageString ?? "Cash reconciliation saved successfully.",
                    data = execMessage.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }
        }
    }
}
