
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
    [CheckSessionTimeOutAttribute]

    public class DocumentPackController : BaseController
    {
        // GET: DocumentPack
        private readonly DocumentPackServices _services;
        public DocumentPackController(DocumentPackServices services)
        {
            _services = services;
        }

        public async Task<ActionResult> Index()
        {
            var guaGuranties = await _services.GetDocuments();
            ViewBag.Guaranties = guaGuranties;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(DocumentPack model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(DocumentPack model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            var guaGuranties = await _services.GetDocuments();
            ViewBag.Guaranties = guaGuranties;
            if (path == "list")
            {
                var data = await _services.GetDocumentPacks();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {

                return PartialView(partialView, new DocumentPack());
            }
            else
            {
                var Guaranty = await _services.GetDocumentPack(KEY);
                return PartialView(partialView, Guaranty);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}