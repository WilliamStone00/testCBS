using CBS.BusinessService;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DDOSControlP
{
    [CheckSessionTimeOutAttribute]
    public class RequestMonitotingController : BaseController
    {
        private readonly RateLimiteTrackerLoggerServices _rateLimiteTrackerLoggerServices;
        private readonly BranchServices _branchServices;

        public RequestMonitotingController(RateLimiteTrackerLoggerServices rateLimiteTrackerLoggerServices, BranchServices branchServices)
        {
            _rateLimiteTrackerLoggerServices = rateLimiteTrackerLoggerServices;
            _branchServices = branchServices;
        }

        // Redirect Index to RateLimitDashboard with defaults
        public ActionResult Index()
        {
            var today = DateTime.Today;

            return RedirectToAction("RateLimitDashboard", new GetRateLimitDashboardQuery
            {
                BranchId = "n/a",
                QueryTopValue = "10",
                StartDate = today,
                EndDate = today
            });
        }
        [HttpGet]
        public async Task<ActionResult> ExportDashboardDataToExcel(GetRateLimitDashboardQuery query)
        {
            // 1. Fetch filtered data from service
            var rateLimiteTrackerLoggers = await _rateLimiteTrackerLoggerServices.ExportDashboard(query);

            string exportedBy = Session["FullName"]?.ToString() ?? "System Export";

            var exportFile = ExportUtilityRateLimitDashboardLogs.ExportRateLimitLogsToExcel(
                rateLimiteTrackerLoggers,
                query.StartDate,
                query.EndDate
            );

            return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
        }


        // Load full dashboard with filters
        public async Task<ActionResult> RateLimitDashboard(GetRateLimitDashboardQuery rateLimitDashboardQuery)
        {
            var dashboardDto = await _rateLimiteTrackerLoggerServices.GetDasgboardAsync(rateLimitDashboardQuery);
            ViewBag.Branches = await _branchServices.GetBranches();
            return View("Index", dashboardDto); // ✅ explicitly load Index.cshtml
        }

        [HttpPost]
        public async Task<ActionResult> GetRateLimitDashboardPartial(GetRateLimitDashboardQuery query)
       {
            var data = await _rateLimiteTrackerLoggerServices.GetDasgboardAsync(query);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> ToggleBlockStatus(string ip, bool block)
        {
            try
            {
                //var result = await _rateLimiteTrackerLoggerServices.ToggleBlockStatusByIp(ip, block);
                var rateLimiteTrackerLogger = await _rateLimiteTrackerLoggerServices.GetRateLimiteTrackerLogger(ip);
                return Json(new { success = true, message = block ? "Blocked successfully." : "Unblocked successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public async Task<ActionResult> Details(string KEY)
        {
            ViewBag.KEY = KEY;
            var rateLimiteTrackerLogger = await _rateLimiteTrackerLoggerServices.GetRateLimiteTrackerLogger(KEY);
            return View(rateLimiteTrackerLogger);
        }

        [HttpPost]
        public async Task<ActionResult> GetLogsByKey(GetRateLimitLogsByKeyQuery query)
        {
            var result = await _rateLimiteTrackerLoggerServices.GetRateLimiteTrackerLoggerBylogs_by_key(query);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> LoadData(GetRateLimitTrackerDataTableQuery query)
        {
            try
            {
                var dataTable = await _rateLimiteTrackerLoggerServices.GetDataTableAsync(query);
                var rateLimiteTrackerLoggers = JsonConvert.DeserializeObject<List<RateLimiteTrackerLogger>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                return Json(new
                {
                    draw = query.Options.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = rateLimiteTrackerLoggers
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading rate limit tracker data.");
            }
        }

    }

}