using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.MemberReconciliation;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.MemberReconciliation
{
    // [CheckSessionTimeOut]
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

            //var MemberReference = await _MemberReferenceService.MemberAccountTypesAsync();
            //ViewBag.MemberReference = MemberReference;

            return true;

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


        [HttpPost]
        public async Task<JsonResult> GetAccountBalance(GetBalance balance)
        {
            if (balance == null)
            {
                return Json(new { success = false, message = "Invalid request payload." });
            }

            try
            {
                object bal = null;
                if (balance.loan)
                {

                    // Ensure LoanType is set
                    balance.LoanType = balance.LoanType ?? balance.accountTypeId;
                    bal = await _LoanReconciliationService.GetAccountBalance(balance);

                }
                else if (balance.member)
                {
                    // Ensure AccountType is set
                    balance.AccountType = balance.AccountType ?? balance.accountTypeId;
                    bal = await _MemberReferenceService.GetAccountBalanceAsync(balance);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please specify whether you want Loan or Member balance (set Loan or Member to true)."
                    });
                }

                return Json(new { success = true, data = bal });
            }
            catch (Exception ex)
            {
                // Log the exception
                Response.StatusCode = 500;
                return Json(new
                {
                    success = false,
                    message = "Failed to get balance",
                    error = ex.Message
                });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> ReconcileTrialBalance(string BranchId, string BranchAccountId, decimal TotalBalance, int AccountCount, int LoanCount, string mode, string accountTypeId)
        //{
        //    try
        //    {
        //        // Create the balance object with all required data
        //        var balance = new GetBalance
        //        {
        //            BranchId = BranchId,
        //            BranchAccount = BranchAccountId,
        //            TotalBalance = TotalBalance,
        //        };

        //        var response = await _MemberReferenceService.ReconcileTrialBalance(balance);

               

        //        return Json(new { success = false, message = response.ApiResponseData?.Message ?? response.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Failed to reconcile trial balance",
        //            error = ex.Message
        //        });
        //    }
        //}

        [HttpPost]
        public async Task<JsonResult> FinalizeReconciliation(FinalReconciliationRequest request)
        {
            try
            {
                // Call your final reconciliation service
                var result = await _MemberReferenceService.FinalizeReconciliation(request);

                return Json(new
                {
                    success = true,
                    message = Messaging.MessageResult(result),
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to finalize reconciliation",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetMemberAccountTypesByBranch(string branchId)
        {
            try
            {
                // Get member account types filtered by branch
                var memberAccounts = await _MemberReferenceService.MemberAccountTypesAsync(branchId);

                // Convert to SelectListItem format
                //var accountTypes = memberAccounts.Select(x => new SelectListItem
                //{
                //    Value = x.Value?.ToString() ?? x.Id?.ToString() ?? "",
                //    Text = x.Text ?? x.Name ?? "Unknown"
                //}).ToList();

                return Json(new { success = true, data = memberAccounts });
            }
            catch (Exception ex)
            {               
                return Json(new { success = false, message = "Error loading account types" });
            }
        }
    }
}


