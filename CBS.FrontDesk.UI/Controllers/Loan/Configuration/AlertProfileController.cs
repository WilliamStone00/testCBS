
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
    public class AlertProfileController : BaseController
    {
        // GET: AlertProfile
        private readonly AlertProfileServices _AlertProfileServices;
        public AlertProfileController(AlertProfileServices AlertProfileServices)
        {
            _AlertProfileServices = AlertProfileServices;
        }

        public async Task<ActionResult> Index()
        {
            GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(AlertProfile model)
        {
            if (ModelState.IsValid)
            {
                var data = await _AlertProfileServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(AlertProfile model)
        {
            if (ModelState.IsValid)
            {
                var data = await _AlertProfileServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            GetList();
            if (path == "list")
            {
                var data = await _AlertProfileServices.GetAlertProfiles();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new AlertProfile());
            }
            else
            {
                var AlertProfile = await _AlertProfileServices.GetAlertProfile(KEY);
                return PartialView(partialView, AlertProfile);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _AlertProfileServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public void GetList()
        {
            ViewBag.Languages = _AlertProfileServices.GetLanguages();
            ViewBag.Services = _AlertProfileServices.GetServices();
        }
    }
}