

using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.MockData;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Office.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
{

    //[CheckSessionTimeOut]
    public class TrialBalanceController : BaseController
    {
        private readonly TrialBalanceService _trialBalanceService;

        public TrialBalanceController(TrialBalanceService trialBalanceService)
        {
            _trialBalanceService = trialBalanceService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GenerateTrialBalance(TrialBalanceFilterQuery model)
        {
            try
            {
                var response = await _trialBalanceService.GetTrialBalancesAsync6columns(model);
                var response2 = await _trialBalanceService.GetTrialMockInformation();

                if (response == null || response.Lines == null || !response.Lines.Any())
                    return Json(new { success = false, message = "No data found for the selected filters." }, JsonRequestBehavior.AllowGet);

                var data = response.Lines.Select(x => new TrialBalanceReportItem
                {
                    AccountNumber = x.AccountNumber,
                    AccountName = x.AccountName,
                    OpeningDebit = x.OpeningDR ?? 0,
                    OpeningCredit = x.OpeningCR ?? 0,
                    MovementDebit = x.PeriodDR ?? 0,
                    MovementCredit = x.PeriodCR ?? 0,
                    ClosingDebit = x.ClosingDR ?? 0,
                    ClosingCredit = x.ClosingCR ?? 0
                }).ToList();
                Session["rptSource"] = data;
                return Json(new { success = true, message = $"Report file not found." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetReport(string path)
        {

            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"TrialBalance6Columns.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            this.HttpContext.Session["rpttitle"] = $"TB6";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);




            //this.HttpContext.Session["rptType"] = "ReportParameterLess";
            //this.HttpContext.Session["ReportName"] = $"TrialBalance6Columns.rpt";
            //this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            //this.HttpContext.Session["rpttitle"] = $"TB6";


            //if (path == "loan")
            //{
            //    this.HttpContext.Session["rptType"] = "ReportParameterLess";
            //    this.HttpContext.Session["ReportName"] = $"MainReportLoan.rpt";
            //    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            //    this.HttpContext.Session["rpttitle"] = $"MemberLoanReceipts";
            //    return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            //}
            //else
            //{
            //    this.HttpContext.Session["rptType"] = "ReportParameterLess";
            //    this.HttpContext.Session["ReportName"] = $"MainReport.rpt";
            //    this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/MainReport.rpt";
            //    this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
            //    return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            //}
        }


    }
}