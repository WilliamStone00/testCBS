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
    public class DocumentController : BaseController
    {
        // GET: Document
        private readonly DocumentServices _DocumentServices;

        public DocumentController(DocumentServices DocumentServices)
        {
            _DocumentServices = DocumentServices;
        }
        
        public async Task<ActionResult> Index()
        {
           
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Document model)
        {
            if (model.Id==null)
            {
                if (ModelState.IsValid)
                {
                    var data = await _DocumentServices.Create(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
            }
            else
            {
                return await Update(model);
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(Document model)
        {
            if (ModelState.IsValid)
            {
                var data = await _DocumentServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _DocumentServices.GetDocuments();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                ViewBag.Key = null;
                return PartialView(partialView, new Document());
            }
            else
            {
                ViewBag.Key = KEY;
                var Period = await _DocumentServices.GetDocument(KEY);
                return PartialView(partialView, Period);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _DocumentServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}