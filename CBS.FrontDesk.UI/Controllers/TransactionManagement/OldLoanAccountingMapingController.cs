using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.MemberReconciliation;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    //[CheckSessionTimeOutAttribute]
    public class OldLoanAccountingMapingController : BaseController
    {
        // GET: OldLoanAccountingMaping
        private readonly OldLoanAccountingMapingServices _FeeServices;
        private readonly AffiliateAccountService _affiliateAccountService;
        private readonly LoanProductServices _loanProductServices;
        private readonly BranchServices _branchServices;
        private readonly LoanReconciliationService _LoanReconciliationService;

        public OldLoanAccountingMapingController(OldLoanAccountingMapingServices FeeServices, AffiliateAccountService accountingServices, LoanProductServices loanProductServices = null, BranchServices branchServices = null, LoanReconciliationService loanReconciliationService = null)
        {
            _FeeServices = FeeServices;
            _affiliateAccountService = accountingServices;
            _loanProductServices = loanProductServices;
            _branchServices = branchServices;
            _LoanReconciliationService = loanReconciliationService;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                ViewBag.Key = null;
                await GetChartOfAccounts();
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("InternalServer", "Error");
            }
         
        }
        [HttpPost]
        public async Task<ActionResult> Create(OldLoanAccountingMaping model)
        {
            if (!ModelState.IsValid)
            {
                // If the model is invalid, return the validation errors
                return Json(new { success = false, status = "error", message = "Model validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            if (model.Id == null)
            {
                var data = await _FeeServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(OldLoanAccountingMaping model)
        {
            var data = await _FeeServices.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }
        public async Task<bool> GetChartOfAccounts()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            // Do NOT preload loan types or account ledgers
            ViewBag.LoanTypes = Enumerable.Empty<SelectListItem>();
            ViewBag.AccountLedgers = await _affiliateAccountService.GetAllAffiliateAccounts();

            return true;
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
   
            if (path == "list")
            {
                var data = await _FeeServices.GetOldLoanAccountingMapings();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                ViewBag.Key = null;
                await GetChartOfAccounts();
                return PartialView(partialView, new OldLoanAccountingMaping());
            }
            else
            {
                ViewBag.Key = KEY;
                await GetChartOfAccounts();
                var Fee = await _FeeServices.GetOldLoanAccountingMaping(KEY);
                return PartialView(partialView, Fee);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _FeeServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
       

        [HttpGet]
        public async Task<JsonResult> GetLoanAccountTypesByBranch(string branchId)
        {
            try
            {
                // Get member account types filtered by branch
                var data = await _LoanReconciliationService.LoanAccountTypeAsync(branchId);


                return Json(new { success = true, message = "Success", data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading account types" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetBranchAccountsByBranch(string branchId)
        {
            try
            {
                var branchAccounts = await _affiliateAccountService.GetAllAffiliateAccounts();
                var resultList = branchAccounts.Select(a => new
                {
                    Id = a.Value,
                    Name = string.IsNullOrWhiteSpace(a.Text)
                });

                return Json(resultList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Failed to load branch accounts" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}