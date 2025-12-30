using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.BusinessService.Config;

namespace CBS.FrontDesk.UI.Controllers.Configuration
{
    [CheckSessionTimeOutAttribute]

    public class LoanTermController : BaseController
    {
        // GET: LoanTerm
        private readonly LoanTermServices _LoanTermServices;
        public LoanTermController(LoanTermServices PeriodServices)
        {
            _LoanTermServices = PeriodServices;
        }

        public async Task<ActionResult> Index()
        {
            loader();
            ViewBag.Key = null;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanTerm model)
        {
            loader();
            if (model.Id == null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _LoanTermServices.Create(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }

                // Collect validation error messages
                var validationMessages = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .SelectMany(ms => ms.Value.Errors
                        .Select(error => new { Field = ms.Key, Error = error.ErrorMessage }))
                    .ToList();

                return Json(new
                {
                    success = false,
                    status = false,
                    message = "Validation failed.",
                    validationMessages // Include detailed error messages
                });
            }
            else
            {
                return await Update(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Update(LoanTerm model)
        {
            if (ModelState.IsValid)
            {
                var data = await _LoanTermServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
         {
            loader();
            if (path == "list")
            {
                var data = await _LoanTermServices.GetLoanTerms();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new LoanTerm());
            }
            else
            {
                var Period = await _LoanTermServices.GetLoanTerm(KEY);
                return PartialView(partialView, Period);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _LoanTermServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        private bool loader()
        {
            // Load LoanTermKind enum values
            var LtGroupOptions = Enum.GetValues(typeof(LoanTermKind))
                .Cast<LoanTermKind >()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                })
                .ToList();
            ViewBag.TermKind = LtGroupOptions;
            return true;
        }
    }
}