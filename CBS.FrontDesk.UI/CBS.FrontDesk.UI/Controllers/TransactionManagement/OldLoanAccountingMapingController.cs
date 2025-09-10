using CBS.FrontDesk.Data.Message;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.BusinessService.Config;
using System.Linq;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    //[CheckSessionTimeOutAttribute]
    public class OldLoanAccountingMapingController : BaseController
    {
        // GET: OldLoanAccountingMaping
        private readonly OldLoanAccountingMapingServices _FeeServices;
        private readonly ChartOfAccountServicesAnnex _accountingServices;
        private readonly LoanProductServices _loanProductServices;
        private readonly BranchServices _branchServices;

        public OldLoanAccountingMapingController(OldLoanAccountingMapingServices FeeServices, ChartOfAccountServicesAnnex accountingServices, LoanProductServices loanProductServices = null, BranchServices branchServices = null)
        {
            _FeeServices = FeeServices;
            _accountingServices = accountingServices;
            _loanProductServices = loanProductServices;
            _branchServices = branchServices;
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
        public async Task<bool> GetChartOfAccounts()
        {
            var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            ViewBag.LoanTypes = productEnumAgregates.LoanTypes;
            ViewBag.AccountLedgers = chartOfAccounts;
            return true;
        }
    }
}