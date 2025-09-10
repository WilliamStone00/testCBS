
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
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
    [CheckSessionTimeOutAttribute]

    public class AttachedLoanDocumentsController : BaseController
    {
        // GET: AttachedLoanDocuments
        private readonly AttachedDocumentServices _services;
        public AttachedLoanDocumentsController(AttachedDocumentServices services)
        {
            _services = services;
        }

        public async Task<ActionResult> Index()
        {

            return View(new DocumentAttachedToLoan { LoanApplicationId = "1234"});
        }
        public async Task<ActionResult> CustomerDocument()
        {

            return View(new CustomerDocumentRequest() { CustomerID = "262352744171912" });
        }
        [HttpPost]
        public async Task<ActionResult> UploadCustomerFiles(CustomerDocumentRequest model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.UploadFiles(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> UploadFiles(DocumentAttachedToLoan model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.UploadFiles(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            return PartialView(partialView, new DocumentAttachedToLoan { LoanApplicationId = KEY });
            if (path == "list")
            {
                var data = await _services.GetDocumentAttachedToLoanByLoanID(KEY);
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new DocumentAttachedToLoan { LoanApplicationId = KEY });
            }
            else
            {
                var data = await _services.GetDocumentAttachedToLoan(KEY);
                return PartialView(partialView, data);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}