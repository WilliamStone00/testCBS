using BusinessServices;
using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.AuditTrailP;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.LoanP.LoanAdjustmentP;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.LoanAdjustmentP;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.FrontDesk.UI.Helper;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.LoanAdjustmentP
{
    //[CheckSessionTimeOutAttribute]

    public class LoanAdjustmentConsoleController : BaseController
    {
        private readonly LoanAdjustmentService _loanAdjustmentService;
        private readonly BranchServices _branchServices;
        private readonly LoanServices _LoanServices;

        public LoanAdjustmentConsoleController(LoanAdjustmentService loanAdjustmentService = null, BranchServices branchServices = null, LoanServices loanServices = null)
        {
            _loanAdjustmentService = loanAdjustmentService;
            _branchServices = branchServices;
            _LoanServices=loanServices;
        }

        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        public async Task<ActionResult> LoanAdjustment()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        
        public async Task<ActionResult> ViewRequest(string id)
        {
            var model = await _loanAdjustmentService.GetLoanAdjustmentRequestAsync(id);
            if (model == null)
            {
                ViewBag.message = $"❌ No loan adjustment request found with ID '{id}'";
                return PartialView("_DataNotFound");
            }

            return PartialView("_LoanAdjustmentDetail", model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAdjustment(SubmitLoanAdjustmentRequestCommand command)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request data." });
            }

            command.RequestedBy = Session["UserId"]?.ToString();
            var result = await _loanAdjustmentService.SubmitLoanAdjustmentRequestAsync(command);
            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            });
        }

        [HttpPost]
        public async Task<ActionResult> ApproveRequest(ApproveLoanAdjustmentRequestCommand command)
        {
            var result = await _loanAdjustmentService.ApproveLoanAdjustmentRequestAsync(command);
            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            });
        }

        [HttpPost]
        public async Task<ActionResult> RejectRequest(RejectLoanAdjustmentRequestCommand command)
        {
            command.RejectedBy = Session["UserId"]?.ToString();
            var result = await _loanAdjustmentService.RejectLoanAdjustmentRequestAsync(command);
            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            });
        }

        [HttpPost]
        public async Task<ActionResult> LoadDataTable(GetLoanAdjustmentRequestsDataTableQuery query, string searchCriteria)
        {
            var dataTable = await _loanAdjustmentService.GetDataTableAsync(query);
            var requestList = JsonConvert.DeserializeObject<List<LoanAdjustmentRequestDto>>(JsonConvert.SerializeObject(dataTable.data));

            return Json(new
            {
                draw = query.Options.draw,
                recordsTotal = dataTable.recordsTotal,
                recordsFiltered = dataTable.recordsFiltered,
                data = requestList
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetRequestDetailPartial(string id)
        {
            var model = await _loanAdjustmentService.GetLoanAdjustmentRequestAsync(id);
            return PartialView("_LoanAdjustmentDetail", model);
        }
        [HttpPost]
        public async Task<ActionResult> LoadLoanDataTable(GetLoansDataTableQuery query)
        {
            query.DataTableOptions.sortColumnName = "LoanDate";
            var dataTable = await _LoanServices.GetDataTableAsync(query);
            var loanList = JsonConvert.DeserializeObject<List<Loan>>(JsonConvert.SerializeObject(dataTable.data));

            return Json(new
            {
                draw = query.DataTableOptions.draw,
                recordsTotal = dataTable.recordsTotal,
                recordsFiltered = dataTable.recordsFiltered,
                data = loanList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetReport()
        {
            HttpContext.Session["rptType"] = "ReportParameterLess";
            HttpContext.Session["ReportName"] = "LoanAdjustmentRequestReport.rpt";
            HttpContext.Session["rptpath"] = "~/AppFiles/Reporting/Loan/LoanAdjustmentRequestReport.rpt";
            HttpContext.Session["rpttitle"] = "Loan Adjustment Request Report";

            return Json(new
            {
                success = true,
                status = true,
                message = "Loan adjustment report initialized."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}