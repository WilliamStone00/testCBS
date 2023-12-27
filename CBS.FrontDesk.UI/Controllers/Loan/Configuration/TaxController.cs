using CBS.BusinessService.Accounting;
using CBS.BusinessService.Loan.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Loan.Configuration
{
    public class TaxController : BaseController
    {
        // GET: Tax
        private readonly TaxServices _TaxServices;
        private readonly AccountingServices _accountingServices;
        public TaxController(TaxServices TaxServices, AccountingServices accountingServices)
        {
            _TaxServices = TaxServices;
            _accountingServices = accountingServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Tax model)
        {
            if (ModelState.IsValid)
            {
                var data = await _TaxServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(Tax model)
        {
            if (ModelState.IsValid)
            {
                var data = await _TaxServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            await GetList();
            if (path == "list")
            {
                var data = await _TaxServices.GetTaxs();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new Tax());
            }
            else
            {
                var Tax = await _TaxServices.GetTax(KEY);
                return PartialView(partialView, Tax);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _TaxServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> GetList()
        {
            ViewBag.Roles = await _accountingServices.GetAccountingRoles();
            return true;
        }
    }
}