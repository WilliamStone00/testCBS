
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
    public class GuarantyController : BaseController
    {
        // GET: Guaranty
        private readonly GuaranteeServices _services;
        public GuarantyController(GuaranteeServices services)
        {
            _services = services;
        }

        public async Task<ActionResult> Index()
        {
            
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Guaranty model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(Guaranty model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
       
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetGuarantees();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new Guaranty());
            }
            else
            {
                var Guaranty = await _services.GetGuarantee(KEY);
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