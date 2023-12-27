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
    public class LoanPurposeController : BaseController
    {
        // GET: LoanPurpose
        private readonly LoanPurposeServices _LoanPurposeServices;
        public LoanPurposeController(LoanPurposeServices LoanPurposeServices)
        {
            _LoanPurposeServices = LoanPurposeServices;
           
        }
        public async Task<ActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanPurpose model)
        {
            if (ModelState.IsValid)
            {
                var data = await _LoanPurposeServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(LoanPurpose model)
        {
            if (ModelState.IsValid)
            {
                var data = await _LoanPurposeServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _LoanPurposeServices.GetLoanPurposes();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new LoanPurpose());
            }
            else
            {
                var LoanPurpose = await _LoanPurposeServices.GetLoanPurpose(KEY);
                return PartialView(partialView, LoanPurpose);
            }
        }


        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _LoanPurposeServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}