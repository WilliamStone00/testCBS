using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Loan.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.BusinessService.Accounting;

namespace CBS.FrontDesk.UI.Controllers.Loan.Configuration
{
    public class LoanProductController : BaseController
    {
        // GET: LoanProduct
        private readonly LoanProductServices _LoanProductServices;
        private readonly AccountingServices _accountingServices;
        public LoanProductController(LoanProductServices LoanProductServices, AccountingServices accountingServices)
        {
            _LoanProductServices = LoanProductServices;
            _accountingServices = accountingServices;
        }
        public async Task<ActionResult> Index()
        {
            await GetValues();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanProduct model)
        {
            if (ModelState.IsValid)
            {
                var data = await _LoanProductServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(LoanProduct model)
        {
            if (ModelState.IsValid)
            {
                var data = await _LoanProductServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path=null)
        {
        
            if (path=="list")
            {
                var data = await _LoanProductServices.GetLoanProducts();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                await GetValues();
                return PartialView(partialView, new LoanProduct());
            }
            else
            {
                await GetValues();
                var LoanProduct = await _LoanProductServices.GetLoanProduct(KEY);
                return PartialView(partialView, LoanProduct);
            }
        }

        public async Task<bool> GetValues()
        {
            var agreggates= await _LoanProductServices.GetAgreggates();

            ViewBag.SheduleTypes = _LoanProductServices.GetScheduleTypes();
            ViewBag.Penalties = agreggates.Penalties;
            ViewBag.Fees = agreggates.Fees;
            ViewBag.Currencies = await _LoanProductServices.GetCurrencies();
            ViewBag.CustomerProfiles = _LoanProductServices.GetCustomerProfile();
            ViewBag.DocumentPackes = agreggates.DocumentPackes;
            ViewBag.GuranteePackes = agreggates.GuranteePackes;
            ViewBag.Taxes = agreggates.Taxes;
            ViewBag.FundingLines = agreggates.FundingLines;
            ViewBag.InstallmentTypes = agreggates.InstallmentTypes;
            ViewBag.AccountingRules = await _accountingServices.GetAccountingRoles();
            return true;
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _LoanProductServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}