using CBS.BusinessService.LoanP;
using CBS.BusinessService.LoanP.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Loan.Configuration
{
    public class PcmfLoanPurposeController : Controller
    {
        private readonly PcmfLoanPurposeService _pcmfLoanPurposeService;

        public PcmfLoanPurposeController(PcmfLoanPurposeService pcmfLoanPurposeService)
        {
            _pcmfLoanPurposeService = pcmfLoanPurposeService;
        }

        public ActionResult Index()
        {
            return View();
        }
    
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,
            string path = null, string serviceOption = null)
        {
            if (path == "details")
            {
                var data = await _pcmfLoanPurposeService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else if (path == "edit")
            {
                var data = await _pcmfLoanPurposeService.GetByIdAsync(KEY);
               return PartialView(partialView, data);
            }
            else if (path == "delete")
            {
                var data = await _pcmfLoanPurposeService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else // create
            {
                return PartialView(partialView, new UpdatePcmfLoanPurposeRequest());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(UpdatePcmfLoanPurposeRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var isCreate = string.IsNullOrWhiteSpace(model.Id);
            var result = isCreate
                ? await _pcmfLoanPurposeService.CreateAsync(model)
                : await _pcmfLoanPurposeService.UpdateAsync(model);

            return Json(new
            {
                success = result.Result,
                message = Messaging.MessageResult(result)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "ID is required." });

            var result = await _pcmfLoanPurposeService.DeleteAsync(id);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        public async Task<JsonResult> GetAll()
        {
            var data = await _pcmfLoanPurposeService.GetAllAsync();
            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "ID is required" },
                    JsonRequestBehavior.AllowGet);
            }

            var data = await _pcmfLoanPurposeService.GetByIdAsync(id);
            return Json(new { success = data != null, data = data }, JsonRequestBehavior.AllowGet);
        }
    }
}