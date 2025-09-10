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

    public class InstallmentTypeController : BaseController
    {
        // GET: InstallmentType
        private readonly InstallmentTypeServices _InstallmentTypeServices;
        public InstallmentTypeController(InstallmentTypeServices InstallmentTypeServices)
        {
            _InstallmentTypeServices = InstallmentTypeServices;
        }
      
        public async Task<ActionResult> Index()
        {
            GetValues();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(InstallmentType model)
        {
            if (ModelState.IsValid)
            {
                var data = await _InstallmentTypeServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(InstallmentType model)
        {
            if (ModelState.IsValid)
            {
                var data = await _InstallmentTypeServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path=null)
        {
            GetValues();
            if (path=="list")
            {
                var data = await _InstallmentTypeServices.GetInstallmentTypes();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new InstallmentType());
            }
            else
            {
                var InstallmentType = await _InstallmentTypeServices.GetInstallmentType(KEY);
                return PartialView(partialView, InstallmentType);
            }
        }

        public void GetValues()
        {
            ViewBag.Values = _InstallmentTypeServices.GetInstallmentTypesEnums();
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _InstallmentTypeServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}