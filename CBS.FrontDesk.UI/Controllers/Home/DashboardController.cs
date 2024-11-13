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
        [HttpGet]
        public ActionResult HeadOfficeStatistics()
        {
            return View();
        }
        [HttpGet]
        public ActionResult BranchStatistics()
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
        // New Action to get daily dashboard data
        [HttpGet]
        public async Task<JsonResult> GetMemberDashboard()
        {
            var dashboardStatistics = await _generalDailyDashboardServices.GetCustomerDashboardAdmin();
            var dashboard = _generalDailyDashboardServices.ConvertToMainDashboardMembers(dashboardStatistics);
            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> GetOrdinaryAccountsDashboard()
        {
            var dashboardStatistics = await _generalDailyDashboardServices.GetAccountsDashboardAdmin();
            var dashboard = _generalDailyDashboardServices.ConvertToMainDashboardOrdinaryAccounts(dashboardStatistics);
            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetAccountingsDashboard()
        {
            var dashboardStatistics = await _generalDailyDashboardServices.GetAccountingDashboardAdmin();
            var dashboard = _generalDailyDashboardServices.GenerateDashboardAccountingStatistics(dashboardStatistics);
            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> GetLoanDashboard()
        {
            var dashboard = await _generalDailyDashboardServices.GetLoanDashboardAdmin();
            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }

        // New Action to get daily dashboard data
        [HttpGet]
        public async Task<JsonResult> GetLiveDashboardHeadOffice()
        {
            var dashboard = await _generalDailyDashboardServices.GetDailyDashboardHeadOffice();
            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }
    }
}