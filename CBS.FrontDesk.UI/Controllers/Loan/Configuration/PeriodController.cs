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

    public class PeriodController : BaseController
    {
        // GET: Period
        private readonly PeriodServices _PeriodServices;
        public PeriodController(PeriodServices PeriodServices)
        {
            _PeriodServices = PeriodServices;
        }
      
        public async Task<ActionResult> Index()
        {
           
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Period model)
        {
            if (ModelState.IsValid)
            {
                var data = await _PeriodServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(Period model)
        {
            if (ModelState.IsValid)
            {
                var data = await _PeriodServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path=null)
        {
            if (path=="list")
            {
                var data = await _PeriodServices.GetPeriods();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new Period());
            }
            else
            {
                var Period = await _PeriodServices.GetPeriod(KEY);
                return PartialView(partialView, Period);
                
            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _PeriodServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}