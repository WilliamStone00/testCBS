using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
{
    public class TrialBalance6ColumnsController : BaseController
    {
        private readonly TrialBalances6ColumnService _trialBalanceService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;

        public TrialBalance6ColumnsController(
            TrialBalances6ColumnService trialBalanceService,
            BranchServices branchServices,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _trialBalanceService = trialBalanceService;
            _branchAccountService = branchAccountService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GenerateTrialBalance(AccountingV2ReportsFilter model)
        {

            try
            {
                this.HttpContext.Session["rptSource"] = null;
                var response = await _trialBalanceService.GetTrialBalancesAsync6columns(model);
                var branchInfo = await _branchServices.GetBranch(model.BranchId);

                // Validate response
                if (response?.Lines == null || !response.Lines.Any())
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "No records found for the selected filters." }, JsonRequestBehavior.AllowGet);
                }

                // Validate branch
                if (branchInfo == null)
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "Selected branch not found." }, JsonRequestBehavior.AllowGet);
                }

                // Build report dataset
                var data = response.Lines.Select(x => new TrialBalanceReportItem
                {
                    AccountNumber = x.AccountNumber,
                    AccountName = x.AccountName,
                    OpeningDebit = x.OpeningDR ?? 0,
                    OpeningCredit = x.OpeningCR ?? 0,
                    MovementDebit = x.PeriodDR ?? 0,
                    MovementCredit = x.PeriodCR ?? 0,
                    ClosingDebit = x.ClosingDR ?? 0,
                    ClosingCredit = x.ClosingCR ?? 0,
                    BranchCode = branchInfo.BranchCode,
                    Phone = branchInfo.Telephone,
                    Address = branchInfo.Address,
                    BranchName = branchInfo.Name,
                    Username = _trialBalanceService.GetUserFullName(),
                    From = model.From,
                    Mode = model.SourceMode,
                    To = model.To
                }).ToList();

                this.HttpContext.Session["rptSource"] = data;

                return Json(new { success = true, message = "Trial balance report ready." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult GetReport(string path)
        {
            // Report file physical path
            var reportPath = Server.MapPath("~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt");

            if (!System.IO.File.Exists(reportPath))
                return Json(new { success = false, message = "Report template file missing." }, JsonRequestBehavior.AllowGet);

            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "TrialBalance6Columns.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            this.HttpContext.Session["rpttitle"] = "TB6";

            return Json(new { success = true, message = "Report parameters set successfully." }, JsonRequestBehavior.AllowGet);
        }
    }
}
