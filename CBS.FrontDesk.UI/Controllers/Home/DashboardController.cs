using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Home
{
    //[CheckSessionTimeOutAttribute]

    public class DashboardController : BaseController
    {
        private readonly GeneralDailyDashboardServices _generalDailyDashboardServices;
        public DashboardController(GeneralDailyDashboardServices generalDailyDashboardServices = null)
        {
            _generalDailyDashboardServices = generalDailyDashboardServices;
        }
        // GET: Dashboard
        public ActionResult HeadOffice()
        {
            return View();
        }
        public ActionResult BranchOffice()
        {
            return View();
        }
        public async Task<ActionResult> DailyOperation()
        {
            return View();
        }
        // New Action to get daily dashboard data
        [HttpGet]
        public async Task<JsonResult> GetLiveDashboard()
        {
            var dashboard = await _generalDailyDashboardServices.GetDailyDashboard();
            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }
    }
}