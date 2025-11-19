using CBS.BusinessService.Accounting_V2.BranchAccountService;
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

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
{

    //[CheckSessionTimeOut]
    public class TrialBalance4ColmunsController : BaseController
    {
        private readonly TrialBalances4ColumnService _trialBalanceService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;
        


        public TrialBalance4ColmunsController(TrialBalances4ColumnService trialBalanceService,
            BranchServices branchServices,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _trialBalanceService = trialBalanceService;
            _branchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GenerateTrialBalance(AccountingV2ReportsFilter model)
        {
            try
            {
                
                var response = await _trialBalanceService.GetTrialBalancesAsync4columns(model);
                var response2 = await _trialBalanceService.GetTrialMockInformation();
                
                var BranchInformation = await _branchServices.GetBranch(model.BranchId);



                if (response == null || response.Lines == null || !response.Lines.Any())
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." }, JsonRequestBehavior.AllowGet);
                }
                    

                var data = response.Lines.Select(x => new TrialBalanceReportItem
                {
                    AccountNumber = x.AccountNumber,
                    AccountName = x.AccountName,
                    Credit = x.OpeningCR ?? 0,
                    Debit = x.Debit ?? 0,
                    ClosingBalance = x.ClosingBalanceFour ?? 0,
                    OpeningBalance = x.OpeningBalanceFour ?? 0,
                    BranchCode = BranchInformation.BranchCode,
                    Phone = BranchInformation.Telephone,
                    Address = BranchInformation.Address,
                    BranchName = BranchInformation.Name,
                    Username = _trialBalanceService.GetUserFullName(),
                    From = model.From,
                    To = model.To,


                }).ToList();
                this.HttpContext.Session["rptSource"] = data;
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
            this.HttpContext.Session["ReportName"] = $"TrialBalance4Columns.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance4Columns.rpt";
            this.HttpContext.Session["rpttitle"] = $"TB4";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);




        }


    }
}