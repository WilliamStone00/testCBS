using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
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

                var header = new BankHeaderInformation
                {
                    BankId = BranchInformation.Bank.Id,
                    BankBankCode = BranchInformation.Bank.BankCode,
                    BankName = BranchInformation.Bank.Name,
                    BankTelephone = BranchInformation.Bank.Telephone,
                    BankEmail = BranchInformation.Bank.Email,
                    BankAddress = BranchInformation.Bank.Address,
                    BankLogoUrl = BranchInformation.Bank.LogoUrl,
                    BankMotto = BranchInformation.Bank.Motto,
                    BankRegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                    BankImmatriculationNumber = BranchInformation.Bank.ImmatriculationNumber,
                    BankPBox = BranchInformation.Bank.PBox,

                    BranchId = BranchInformation.Id,
                    BranchCode = BranchInformation.BranchCode,
                    BranchName = BranchInformation.Name,
                    BranchTelephone = BranchInformation.Telephone,
                    BranchEmail = BranchInformation.Email,
                    BranchAddress = BranchInformation.Address,
                    BranchLogoUrl = BranchInformation.LogoUrl,
                    BranchCapital = BranchInformation.Capital,
                    BranchRegistrationNumber = BranchInformation.RegistrationNumber,
                    BranchImmatriculationNumber = BranchInformation.ImmatriculationNumber,
                    BranchPBox = BranchInformation.PBox
                };



                // Convert to report dataset format
                var data = response.Select(x =>
                {
                    var item = new TrialBalanceFourColumnsFlatItems
                    {
                        AccountNumber = x.AccountNumber,
                        AccountName = x.AccountName,
                        Credit = x.Credit,
                        Debit = x.Debit,
                        TotalBeginningNet = x.TotalBeginningNet,
                        OpeningDebit = x.BeginningBalance,
                        TotalEndingNet = x.TotalEndingNet,
                        EndingBalance = x.EndingBalance,
                        BeginningBalance = x.BeginningBalance,
                        TotalCredit = x.TotalCredit,
                        TotalDebit = x.TotalDebit,           
                        BeginningBookingDirection = x.BeginningSide,
                        EndingBookingDirection = x.EndingSide,
                        Phone = BranchInformation.Telephone,
                        Address = BranchInformation.Address,
                        BranchName = BranchInformation.Name,
                        BranchTel = BranchInformation.Telephone,
                        HeadOfficePhone = BranchInformation.Bank.Telephone,
                        BranchEmail = BranchInformation.Email,
                        TotalEndingSide = x.TotalEndingSide,
                        BranchImmatriculationNumber = BranchInformation.Bank.ImmatriculationNumber,
                        LogoUrl = BranchInformation.Bank.LogoUrl,
                        PBox = BranchInformation.PBox,
                        RegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                        DisplayName = BranchInformation.DisplayName,                   
                        TotalMovementNet = x.TotalMovementNet,
                        TotalBeginningSide = x.TotalBeginningSide,
                        Username = _trialBalanceService.GetUserFullName(),
                        From = model.From,
                        Mode = model.SourceMode == "Temp" ? "( TEMPORAL REPORT) " : $"( {model.SourceMode.ToUpper()} REPORT )",
                        To = model.To
                    };

                    // 🔥 This is where ApplyHeader is used
                    ApplyHeader(item, header);

                    return item;
                }).ToList();


                //var data = response.Select(x => new TrialBalanceFourColumnsFlatItems
                //{
                //    AccountNumber = x.AccountNumber,
                //    AccountName = x.AccountName,
                //    Credit = x.Credit,
                //    Debit = x.Debit,
                //    TotalBeginningNet = x.TotalBeginningNet,
                //    OpeningDebit = x.BeginningBalance,
                //    TotalEndingNet = x.TotalEndingNet,
                //    EndingBalance = x.EndingBalance,
                //    BeginningBalance = x.BeginningBalance,
                //    TotalCredit = x.TotalCredit,
                //    TotalDebit = x.TotalDebit,


                //    BranchLogoUrl = BranchInformation.LogoUrl,
                //    BranchCode = BranchInformation.BranchCode,
                //    BeginningBookingDirection = x.BeginningSide,
                //    EndingBookingDirection = x.EndingSide,
                //    Phone = BranchInformation.Telephone,
                //    Address = BranchInformation.Address,
                //    BranchName = BranchInformation.Name,
                //    BranchTel = BranchInformation.Telephone,
                //    HeadOfficePhone = BranchInformation.Bank.Telephone,
                //    BranchEmail = BranchInformation.Email,
                //    TotalEndingSide = x.TotalEndingSide,
                //    BranchImmatriculationNumber = BranchInformation.Bank.ImmatriculationNumber,
                //    LogoUrl = BranchInformation.Bank.LogoUrl,
                //    PBox = BranchInformation.PBox,
                //    RegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                //    DisplayName = BranchInformation.DisplayName,
                //    Motto = BranchInformation.Bank.Motto,




                //    TotalMovementNet = x.TotalMovementNet,
                //    TotalBeginningSide = x.TotalBeginningSide,
                //    Username = _trialBalanceService.GetUserFullName(),
                //    From = model.From,
                //    Mode = model.SourceMode == "Temp" ? "( TEMPORAL REPORT) " : $"( {model.SourceMode.ToUpper()} REPORT )",
                //    To = model.To
                //}).ToList();

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




        private void ApplyHeader(TrialBalanceFourColumnsFlatItems item, BankHeaderInformation header)
        {
            item.BankId = header.BankId;
            item.BankBankCode = header.BankBankCode;
            item.BankName = header.BankName;
            item.BankTelephone = header.BankTelephone;
            item.BankEmail = header.BankEmail;
            item.BankAddress = header.BankAddress;
            item.BankLogoUrl = header.BankLogoUrl;
            item.BankMotto = header.BankMotto;
            item.BankRegistrationNumber = header.BankRegistrationNumber;
            item.BankImmatriculationNumber = header.BankImmatriculationNumber;
            item.BankPBox = header.BankPBox;

            item.BranchId = header.BranchId;
            item.BranchCode = header.BranchCode;
            item.BranchName = header.BranchName;
            item.BranchTelephone = header.BranchTelephone;
            item.BranchEmail = header.BranchEmail;
            item.BranchAddress = header.BranchAddress;
            item.BranchLogoUrl = header.BranchLogoUrl;
            item.BranchCapital = header.BranchCapital;
            item.BranchRegistrationNumber = header.BranchRegistrationNumber;
            item.BranchImmatriculationNumber = header.BranchImmatriculationNumber;
            item.BranchPBox = header.BranchPBox;
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
