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
    public class PenaltyController : BaseController
    {
        // GET: Penalty
        private readonly PenaltyServices _PenaltyServices;
        private readonly AccountingServices _accountingServices;
        public PenaltyController(PenaltyServices PenaltyServices, AccountingServices accountingServices)
        {
            _PenaltyServices = PenaltyServices;
            _accountingServices = accountingServices;
        }
        public async Task<ActionResult> Index()
        {
            await GetValues();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Penalty model)
        {
            if (ModelState.IsValid)
            {
                var data = await _PenaltyServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(Penalty model)
        {
            if (ModelState.IsValid)
            {
                var data = await _PenaltyServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            await GetValues();
            if (path == "list")
            {
                var data = await _PenaltyServices.GetPenaltys();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new Penalty());
            }
            else
            {
                var Penalty = await _PenaltyServices.GetPenalty(KEY);
                return PartialView(partialView, Penalty);
            }
        }

        public async Task<bool> GetValues()
        {
            ViewBag.Roles = await _accountingServices.GetAccountingRoles();
            return true;
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _PenaltyServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}