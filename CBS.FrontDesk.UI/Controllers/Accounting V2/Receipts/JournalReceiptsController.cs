using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.JournalReceiptsReports;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;

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

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.Receipts
{

    //[CheckSessionTimeOut]
    public class JournalReceiptsController : BaseController
    {
        private readonly JournalReceiptsService _journalReceiptsService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;
        


        public JournalReceiptsController(JournalReceiptsService journalReceiptsService,
            BranchServices branchServices,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _journalReceiptsService = journalReceiptsService;
            _branchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GenerateReciept(ReceiptV2Filter filter)
        {
            try
            {
                
                var response = await _journalReceiptsService.GetReceiptAsync(filter);

               

                if (response == null)
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." }, JsonRequestBehavior.AllowGet);
                }
                    

                //var data = response.Lines.Select(x => new TrialBalanceReportItem
                //{
                //    AccountNumber = x.AccountNumber,
                //    AccountName = x.AccountName,
                //    OpeningDebit = x.OpeningDR ?? 0,
                //    OpeningCredit = x.OpeningCR ?? 0,
                //    MovementDebit = x.PeriodDR ?? 0,
                //    MovementCredit = x.PeriodCR ?? 0,
                //    ClosingDebit = x.ClosingDR ?? 0,
                //    ClosingCredit = x.ClosingCR ?? 0,
                //    BranchCode = BranchInformation.BranchCode,
                //    Phone = BranchInformation.Telephone,
                //    Address = BranchInformation.Address,
                //    BranchName = BranchInformation.Name,
                //    Username = _trialBalanceService.GetUserFullName(),
                //    From = model.From,
                //    To = model.To,


                //}).ToList();
                //this.HttpContext.Session["rptSource"] = data;
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
            this.HttpContext.Session["ReportName"] = $"TrialBalance6Columns";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            this.HttpContext.Session["rpttitle"] = $"TB6";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);




        }


    }
}