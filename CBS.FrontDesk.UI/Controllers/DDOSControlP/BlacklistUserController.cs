using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.RequestLoggerServicesP;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using ClosedXML.Excel;
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
    //[CheckSessionTimeOutAttribute]

    public class BlacklistUserController : BaseController
    {
        // GET: BlacklistUser
        private readonly RateLimitedUserService _membersServices;
        private readonly UserManagementServices _userManagementServices;
        public BlacklistUserController(RateLimitedUserService membersServices, UserManagementServices userManagementServices)
        {
            _membersServices = membersServices;
            _userManagementServices=userManagementServices;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.Users=await _userManagementServices.GetUserDropDownList();
            return View(new RateLimitedUser());
        }
        // Filters and dashboard
        [HttpPost]
        public async Task<ActionResult> WAFDashboard(DateTime? fromDate, DateTime? toDate)
        {
            var result = await _membersServices.WafDashBoardForBlockedUSers(new GetWafDashboardQuery { FromDate=fromDate, ToDate=toDate.Value });
            return View(result);
        }
        [HttpPost]
        public async Task<ActionResult> ReleaseSelected(List<string> ids)
        {
            var data=await _membersServices.DeleteAsync(ids); // implement this method
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        // Export Excel
        [HttpPost]
        public async Task<ActionResult> ExportWafDashboardToExcel(DateTime? fromDate, DateTime? toDate)
        {
            var dashboard = await _membersServices.WafDashBoardForBlockedUSers(new GetWafDashboardQuery { FromDate=fromDate, ToDate=toDate.Value });

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Blocked Users");

            // Headers
            ws.Cell(1, 1).Value = "IP Address";
            ws.Cell(1, 2).Value = "User";
            ws.Cell(1, 3).Value = "Reason";
            ws.Cell(1, 4).Value = "Country";
            ws.Cell(1, 5).Value = "Branch";
            ws.Cell(1, 6).Value = "Timestamp";

            // Rows
            for (int i = 0; i < dashboard.BlockedUsers.Count; i++)
            {
                var user = dashboard.BlockedUsers[i];
                ws.Cell(i + 2, 1).Value = user.IpAddress;
                ws.Cell(i + 2, 2).Value = user.UserName;
                ws.Cell(i + 2, 3).Value = user.Reason;
                ws.Cell(i + 2, 4).Value = user.Country;
                ws.Cell(i + 2, 5).Value = user.Branch;
                ws.Cell(i + 2, 6).Value = user.Timestamp.ToString("g");
            }

            // Auto size columns
            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"WAF_Dashboard_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<ActionResult> WAFDashboard()
        {
            var wafDashboardData = await _membersServices.WafDashBoardForBlockedUSers(new GetWafDashboardQuery());
            return View(wafDashboardData);
        }
        [HttpPost]
        public async Task<ActionResult> LoadData(GetBlockedUsersDataTableQuery query)
        {
            try
            {
                var dataTable = await _membersServices.GetDataTableAsync(query);
                var rateLimitedUsers = JsonConvert.DeserializeObject<List<RateLimitedUser>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                return Json(new
                {
                    draw = query.Options.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = rateLimitedUsers
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading C-Money activation data.");
            }
        }
        [HttpPost]
        public async Task<ActionResult> ExportWafDashboardToExcel(GetWafDashboardQuery query)
        {
            var dashboard = await _membersServices.WafDashBoardForBlockedUSers(query);

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Blocked Users");

            // Headers
            ws.Cell(1, 1).Value = "IP Address";
            ws.Cell(1, 2).Value = "User";
            ws.Cell(1, 3).Value = "Reason";
            ws.Cell(1, 4).Value = "Country";
            ws.Cell(1, 5).Value = "Branch";
            ws.Cell(1, 6).Value = "Timestamp";

            // Rows
            for (int i = 0; i < dashboard.BlockedUsers.Count; i++)
            {
                var user = dashboard.BlockedUsers[i];
                ws.Cell(i + 2, 1).Value = user.IpAddress;
                ws.Cell(i + 2, 2).Value = user.UserName;
                ws.Cell(i + 2, 3).Value = user.Reason;
                ws.Cell(i + 2, 4).Value = user.Country;
                ws.Cell(i + 2, 5).Value = user.Branch;
                ws.Cell(i + 2, 6).Value = user.Timestamp.ToString("g");
            }

            // Auto size columns
            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"WAF_Dashboard_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(RateLimitedUser model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            serviceAction = GetInsertServiceAction(model);

            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(RateLimitedUser model)
        {
            return () => _membersServices.Add(model.BlockRequest);

        }
 


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY);

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
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return HttpNotFound("ID is required.");

            var entry = await _membersServices.GetByIdAsync(id);
            if (entry == null)
                return HttpNotFound("Blacklist entry not found.");

            return PartialView("_Details", entry);
        }
        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key)
        {
            if (path == "list")
            {
                return async () =>
                {
                    var data = await _membersServices.GetAllAsync();
                    return PartialView(partialView, data.ToList());

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new RateLimitedUser());
            }
            else
            {
                return async () => PartialView(partialView, await _membersServices.GetByIdAsync(key));
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _membersServices.DeleteAsync(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}