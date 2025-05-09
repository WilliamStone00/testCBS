using CBS.BusinessService;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.SMSP;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SMSP;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.SMSP
{
    //[CheckSessionTimeOutAttribute]
    public class SMSActivityLogController : BaseController
    {
        // GET: SMSActivityLog
        private readonly SMSServices _smsServices;
        private readonly BranchServices _branchServices;

        public SMSActivityLogController(SMSServices smsServices, BranchServices branchServices)
        {
            _smsServices=smsServices;
            _branchServices=branchServices;
        }

       
        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            ViewBag.OperationTypes = _smsServices.GetOperationTypes();
            ViewBag.Statuses = _smsServices.GetStatuses();
            return View();
        }
      

        [HttpPost]
        public async Task<ActionResult> LoadSMSActivityLogData(GetAllSmsLogsDataTableQuery query)
        {
            try
            {
                var dataTable = await _smsServices.GetDataTableAsync(query);
                var sms = JsonConvert.DeserializeObject<List<Sms>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                return Json(new
                {
                    draw = query.Options.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = sms
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading sms log data.");
            }
        }

        [HttpGet]
        public async Task<ActionResult> SMSActivityLogDownload(GetAllSmsLogsDataTableQuery query)
        {
            try
            {
                query.Options = new DataTableOptions
                {
                    pageSize = 10000,
                    start = 0,
                };

                var dataTable = await _smsServices.GetDataTableAsync(query);

                var activationList = JsonConvert.DeserializeObject<List<CMoneyMembersActivationAccount>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                string exportedBy = Session["FullName"]?.ToString() ?? "System Export";

                var exportFile = ExportUtilityCMoney.GenerateCMoneyMemberActivationsExcel(
                    activationList,
                    exportedBy,
                    query.From.ToString("dd/MM/yyyy"),
                    query.To.ToString("dd/MM/yyyy")
                );

                return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting C-Money activation data.");
            }
        }

        public async Task<ActionResult> GetSMSActivityLogDetail(string Key)
        {
            var data = await _smsServices.GetSmsLOgDetailAsync(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> LoadSmsDetails(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return Content("<p class='text-danger'>Invalid SMS ID.</p>");
                }

                var sms = await _smsServices.GetSmsLOgDetailAsync(key);
                if (sms == null)
                {
                    return Content("<p class='text-danger'>SMS not found.</p>");
                }

                return PartialView("_SmsDetailsPartial", sms);
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                Console.WriteLine($"Error loading SMS details for ID {key}: {ex.Message}");
                return Content("<p class='text-danger'>An error occurred while loading SMS details. Please try again later.</p>");
            }
        }


    }

}