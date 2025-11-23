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
    public class TrialBalance4ColmunsController : BaseController
    {
        private readonly TrialBalances4ColumnService _trialBalanceService;
        private readonly BranchAccountService _branchAccountService;
        private readonly BranchServices _branchServices;

        public TrialBalance4ColmunsController(
            TrialBalances4ColumnService trialBalanceService,
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
                // Service already handles IncludeZero filter
                var response = await _trialBalanceService.GetTrialBalancesAsync4columns(model);
                var BranchInformation = await _branchServices.GetBranch(model.BranchId);

                if (BranchInformation == null)
                {
                    return Json(new { success = false, message = "Branch not found." }, JsonRequestBehavior.AllowGet);
                }
                
                if (response == null)
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "No data found for the selected filters." }, JsonRequestBehavior.AllowGet);
                }

                // Convert to report dataset format
                var data = response.Select(x => new TrialBalanceFourColumnsFlatItens
                {
                    AccountNumber = x.AccountNumber,
                    AccountName = x.AccountName,
                    Credit = x.Credit,
                    Debit = x.Debit,
                    EndingBalance = x.TotalEndingNet,
                    BeginningBalance = x.TotalEndingNet,
                    
                    TotalCredit = x.TotalCredit,
                    TotalDebit = x.TotalDebit,
                    BranchCode = BranchInformation.BranchCode,
                    BeginningBookingDirection = x.BeginningSide,
                    EndingBookingDirection = x.EndingSide,
                    Phone = BranchInformation.Telephone,
                    Address = BranchInformation.Address,
                    BranchName = BranchInformation.Name,
                    BranchTel = BranchInformation.Telephone,
                    HeadOfficePhone = BranchInformation.Bank.Telephone,
                    BranchEmail = BranchInformation.Email,
                    TotalBeginningSide = x.TotalBeginningSide,
                    TotalEndingSide = x.TotalEndingSide,
                    ImmatriculationNumber = BranchInformation.Bank.ImmatriculationNumber,
                    LogoUrl = BranchInformation.Bank.LogoUrl,
                    Motto = BranchInformation.Bank.Motto,
                    RegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                    TotalMovementNet = x.TotalMovementNet,


                    PBox = BranchInformation.PBox,
                    DisplayName = BranchInformation.DisplayName,


                    Username = _trialBalanceService.GetUserFullName(),
                    From = model.From,
                    Mode = model.SourceMode == "Temp" ? "( TEMPORAL REPORT) " : $"( {model.SourceMode.ToUpper()} REPORT )",
                    To = model.To
                }).ToList();

                // Store records for Crystal
                this.HttpContext.Session["rptSource"] = data;
                //this.HttpContext.Session["ReportFilters"] = model;

                return Json(new { success = true, message = "Report data ready." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetReport(string path)
        {
            // Crystal report metadata
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "TrialBalance4Columns.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance4Columns.rpt";
            this.HttpContext.Session["rpttitle"] = "TB4";

            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);
        }
    }
}
