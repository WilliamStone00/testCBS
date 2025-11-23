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

                var now = DateTime.Now;

                // 1) Map each line and compute per-line balances/differences
                var data = response.Lines.Select(x =>
                {
                    var openingDr = x.OpeningDR ?? 0m;
                    var openingCr = x.OpeningCR ?? 0m;
                    var periodDr = x.PeriodDR ?? 0m;
                    var periodCr = x.PeriodCR ?? 0m;
                    var closingDr = x.ClosingDR ?? 0m;
                    var closingCr = x.ClosingCR ?? 0m;

                    // Net balances
                    var openingBalance = openingDr - openingCr;
                    var closingBalance = closingDr - closingCr;

                    // Totals for TB control (Opening + Movement)
                    var totalDebit = openingDr + periodDr;
                    var totalCredit = openingCr + periodCr;

                    // Per-line differences
                    var openingDiff = openingDr - openingCr;
                    var movementDiff = periodDr - periodCr;
                    var closingDiff = closingDr - closingCr;
                    var totalDiff = totalDebit - totalCredit;

                    return new TrialBalanceReportItem
                    {
                        AccountNumber = x.AccountNumber,
                        AccountName = x.AccountName,

                        OpeningDebit = openingDr,
                        OpeningCredit = openingCr,
                        MovementDebit = periodDr,
                        MovementCredit = periodCr,
                        ClosingDebit = closingDr,
                        ClosingCredit = closingCr,

                        OpeningBalance = openingBalance,
                        ClosingBalance = closingBalance,
                        Debit = totalDebit,
                        Credit = totalCredit,

                        OpeningDifference = openingDiff,
                        MovementDifference = movementDiff,
                        ClosingDifference = closingDiff,
                        TotalDifference = totalDiff,

                        BranchCode = branchInfo.BranchCode,
                        Phone = branchInfo.Telephone,
                        Address = branchInfo.Address,
                        BranchName = branchInfo.Name,
                        Username = _trialBalanceService.GetUserFullName(),

                        From = model.From,
                        To = model.To,
                        Mode = model.SourceMode == "Temp" ? "( TEMPORAL REPORT) " :  $"( {model.SourceMode.ToUpper()} REPORT )",

                        Date = now.Date,
                        DayTime = now,
                        Time = now.TimeOfDay,
                        Year = now.Year.ToString()
                    };
                }).ToList();

                // 2) Compute GLOBAL SUMS for footer
                var sumOpeningDr = data.Sum(r => r.OpeningDebit);
                var sumOpeningCr = data.Sum(r => r.OpeningCredit);
                var sumMovementDr = data.Sum(r => r.MovementDebit);
                var sumMovementCr = data.Sum(r => r.MovementCredit);
                var sumClosingDr = data.Sum(r => r.ClosingDebit);
                var sumClosingCr = data.Sum(r => r.ClosingCredit);

                var sumOpeningDiff = sumOpeningDr - sumOpeningCr;
                var sumMovementDiff = sumMovementDr - sumMovementCr;
                var sumClosingDiff = sumClosingDr - sumClosingCr;

                var sumTotalDebit = data.Sum(r => r.Debit);
                var sumTotalCredit = data.Sum(r => r.Credit);
                var sumTotalDiff = sumTotalDebit - sumTotalCredit;

                // 3) Push global sums into each row (Crystal footer can just read any row / group)
                foreach (var row in data)
                {
                    row.TotalOpeningDebit = sumOpeningDr;
                    row.TotalOpeningCredit = sumOpeningCr;
                    row.TotalMovementDebit = sumMovementDr;
                    row.TotalMovementCredit = sumMovementCr;
                    row.TotalClosingDebit = sumClosingDr;
                    row.TotalClosingCredit = sumClosingCr;

                    row.TotalOpeningDifference = sumOpeningDiff;
                    row.TotalMovementDifference = sumMovementDiff;
                    row.TotalClosingDifference = sumClosingDiff;
                    row.TotalGlobalDifference = sumTotalDiff;
                }

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
