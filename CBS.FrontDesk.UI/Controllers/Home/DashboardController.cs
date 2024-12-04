using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
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
        private readonly BranchServices _branchServices;

        public DashboardController(GeneralDailyDashboardServices generalDailyDashboardServices = null, BranchServices branchServices = null)
        {
            _generalDailyDashboardServices = generalDailyDashboardServices;
            _branchServices = branchServices;
        }
        // GET: Dashboard
        public ActionResult HeadOffice()
        {
            return View();
        }
        public async Task<ActionResult> BranchOffice(string branchid = "n/a")
        {
            ViewBag.BranchID = branchid;
            if (branchid!="n/a")
            {
                var branch = await _branchServices.GetBranch(branchid);
                ViewBag.BranchName = branch?.Name;
            }
            else
            {
                ViewBag.BranchName = Session["BranchName"].ToString();
            }
            return View();
        }
        public ActionResult OpenedBranchDashboard()
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
        [HttpGet]
        public ActionResult OpenedBranches()
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
        public async Task<JsonResult> GetLiveOpenedBranchDashboard(string branchid)
        {
            if (branchid == "n/a")
            {
                var dashboard = await _generalDailyDashboardServices.GetDailyDashboard();
                return Json(dashboard, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var dashboard = await _generalDailyDashboardServices.GetDailyDashboard(branchid);
                return Json(dashboard, JsonRequestBehavior.AllowGet);

            }
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
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null, string branchid = null)

        {
            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY, serviceOption, branchid);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }

            return HttpNotFound(); // Or return a default view for handling unknown paths
        }


        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key, string serviceOption, string branchid)

        {
            if (serviceOption == "open_branches")
            {
                return async () =>
                {
                    var dashboardDtos = await _generalDailyDashboardServices.GetAllDailyDashboards();
                    return PartialView(partialView, dashboardDtos);
                };
            }


            return null;
        }
    }
}