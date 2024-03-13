using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Configuration
{
    public class FundingLineController : BaseController
    {
        // GET: FundingLine
        private readonly FundingLineServices _FundingLineServices;
        private readonly AccountingServices _accountingServices;
        public FundingLineController(FundingLineServices FundingLineServices, AccountingServices accountingServices)
        {
            _FundingLineServices = FundingLineServices;
            _accountingServices = accountingServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(FundingLine model)
        {
            if (ModelState.IsValid)
            {
                var data = await _FundingLineServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(FundingLine model)
        {
            if (ModelState.IsValid)
            {
                var data = await _FundingLineServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            await GetList();
            if (path == "list")
            {
                var data = await _FundingLineServices.GetFundingLines();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new FundingLine());
            }
            else
            {
                var FundingLine = await _FundingLineServices.GetFundingLine(KEY);
                return PartialView(partialView, FundingLine);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _FundingLineServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> GetList()
        {
            ViewBag.Currencies = await _FundingLineServices.GetCurrencies();
            ViewBag.Roles = await _accountingServices.GetAccountingRoles();
            return true;
        }
    }
}