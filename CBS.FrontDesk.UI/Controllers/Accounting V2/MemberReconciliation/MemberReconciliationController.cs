using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.MemberReconciliation;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.MemberReconciliation
{
    public class ReconciliationController : Controller
    {
        private readonly MemberReconciliationService _MemberReferenceService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchService;
        private readonly LoanReconciliationService _LoanReconciliationService;



        public ReconciliationController(LoanReconciliationService loanReconciliationService, BranchServices branchServices, MemberReconciliationService reconService, BranchAccountService branchAccountService)
        {
            _MemberReferenceService = reconService;
            _branchAccountService = branchAccountService;
            _branchService = branchServices;
            _LoanReconciliationService = loanReconciliationService;
        }

        // GET: Reconciliation
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }

        public async Task<bool> loader()
        {
            var branches = await _branchService.GetBranches();
            ViewBag.Branches = branches;

            var LoanAccounts = await _LoanReconciliationService.LoanAccountTypeAsync();
            ViewBag.LoanAccounts = LoanAccounts;

            var MemberReference = await _MemberReferenceService.MemberAccountTypesAsync();
            ViewBag.MemberReference = MemberReference;          

            return true;

        }

        //public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        //{
        //    //await loader();
        //    if (path == "list")
        //    {
        //        var data = await _LoanReconciliationService.GetAsync();
        //        return PartialView(partialView, data);

        //    }
        //    //GetRolePermissions
        //    else if (path == "new")
        //    {
        //        return PartialView(partialView, new AffiliateCommand());
        //    }

        //    else
        //    {
        //        var data = await _AffiliateController.GetByIdAsync(KEY);
        //        return PartialView(partialView, data);

        //    }
        //}

        //// returns account types for "member" or "loan"
        //[HttpGet]
        //public async Task<ActionResult> GetAccountTypes(string mode)
        //{
        //    var list = await _MemberReferenceService.GetAccountTypesAsync(mode);
        //    // return simple array of { Id, Name }
        //    return Json(list, JsonRequestBehavior.AllowGet);
        //}

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

        // returns account id & balance for selected account type in the selected mode.
        [HttpGet]
        public async Task<ActionResult> GetAccountBalance(GetBalance balance)
        {
            if (balance == null)
            {
                return Json(new { success = false, message = "Invalid request payload." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                object bal = null;

                if (balance.loan == true)
                {
                    // call loan service (you used GetAccountBalance() originally)
                    bal = await _LoanReconciliationService.GetAccountBalance();
                }
                else if (balance.member == true)
                {
                    // call member service (you used GetAccountBalanceAsync() originally)
                    bal = await _MemberReferenceService.GetAccountBalanceAsync();
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please specify whether you want Loan or Member balance (set Loan or Member to true)."
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = bal }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // TODO: log the exception with your logger, e.g. _logger.LogError(ex, "GetAccountBalance failed");
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Failed to get balance", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}


