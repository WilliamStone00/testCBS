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
                // Always reset at the beginning
                this.HttpContext.Session["rptSource"] = null;

                // ─────────────────────────────────────────────
                // 1) Load 4-column trial balance
                //    (service already handles IncludeZero)
                // ─────────────────────────────────────────────
                var response = await _trialBalanceService.GetTrialBalancesAsync4columns(model);

                if (response == null || !response.Any())
                {
                    return Json(
                        new { success = false, message = "No data found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // ─────────────────────────────────────────────
                // 2) Resolve branch context (same logic as 6-column)
                //    - Consolidated → current user's branch
                //    - BranchId null → current user's branch
                //    - Else → selected branch
                // ─────────────────────────────────────────────
                var currentBranchId = _branchServices.GetBranchID();
                var branchIdToLoad = model.Consolidated || string.IsNullOrWhiteSpace(model.BranchId)
                    ? currentBranchId
                    : model.BranchId;

                var branch = await _branchServices.GetBranch(branchIdToLoad);
                if (branch == null)
                {
                    return Json(
                        new { success = false, message = "Selected branch not found." },
                        JsonRequestBehavior.AllowGet);
                }

                // ─────────────────────────────────────────────
                // 3) Build static header metadata (bank + branch)
                // ─────────────────────────────────────────────
                var header = new BankHeaderInformation
                {
                    BankId = branch.Bank.Id,
                    BankBankCode = branch.Bank.BankCode,
                    BankName = branch.Bank.Name,
                    BankTelephone = branch.Bank.Telephone,
                    BankEmail = branch.Bank.Email,
                    BankAddress = branch.Bank.Address,
                    BankLogoUrl = branch.Bank.LogoUrl,
                    BankMotto = branch.Bank.Motto,
                    BankRegistrationNumber = branch.Bank.RegistrationNumber,
                    BankImmatriculationNumber = branch.Bank.ImmatriculationNumber,
                    BankPBox = branch.Bank.PBox,

                    BranchId = branch.Id,
                    BranchCode = branch.BranchCode,
                    BranchName = branch.Name,
                    BranchTelephone = branch.Telephone,
                    BranchEmail = branch.Email,
                    BranchAddress = branch.Address,
                    BranchLogoUrl = branch.LogoUrl,
                    BranchCapital = branch.Capital,
                    BranchRegistrationNumber = branch.RegistrationNumber,
                    BranchImmatriculationNumber = branch.ImmatriculationNumber,
                    BranchPBox = branch.PBox
                };

                // Common metadata (same pattern as 6-column)
                var now = DateTime.Now;
                var username = _trialBalanceService.GetUserFullName();

                var modeLabel = model.SourceMode == "Temp"
                    ? "( TEMPORAL REPORT) "
                    : $"( {model.SourceMode?.ToUpperInvariant()} REPORT )";

                var consolidationLabel = model.Consolidated
                    ? "CONSOLIDATED"
                    : "BRANCH LEVEL";

                // ─────────────────────────────────────────────
                // 4) Map to 4-column DTO + header & metadata
                //    (DTO shape remains the same)
                // ─────────────────────────────────────────────
                var data = response
                    .Select(x =>
                    {
                        var item = new TrialBalanceFourColumnsFlatItems
                        {
                            // Core 4-column values
                            AccountNumber = x.AccountNumber,
                            AccountName = x.AccountName,
                            Debit = x.Debit,
                            Credit = x.Credit,

                            BeginningBalance = x.BeginningBalance,
                            EndingBalance = x.EndingBalance,
                            OpeningDebit = x.BeginningBalance,     // same as before
                            TotalBeginningNet = x.TotalBeginningNet,
                            TotalEndingNet = x.TotalEndingNet,
                            TotalCredit = x.TotalCredit,
                            TotalDebit = x.TotalDebit,
                            TotalMovementNet = x.TotalMovementNet,
                            TotalBeginningSide = x.TotalBeginningSide,
                            TotalEndingSide = x.TotalEndingSide,
                            BeginningBookingDirection = x.BeginningSide,
                            EndingBookingDirection = x.EndingSide,

                            // Branch / bank display info
                            Phone = branch.Telephone,
                            Address = branch.Address,
                            BranchName = branch.Name,
                            BranchTel = branch.Telephone,
                            BranchEmail = branch.Email,
                            HeadOfficePhone = branch.Bank.Telephone,
                            BranchImmatriculationNumber = branch.Bank.ImmatriculationNumber,
                            LogoUrl = branch.Bank.LogoUrl,
                            PBox = branch.PBox,
                            RegistrationNumber = branch.Bank.RegistrationNumber,
                            DisplayName = branch.DisplayName,
                            Motto = branch.Bank.Motto,
                            BranchLogoUrl = branch.LogoUrl,
                            BranchCode = branch.BranchCode,

                            // Report metadata
                            Username = username,
                            From = model.From,
                            To = model.To,
                            Mode = modeLabel,

                            // New common metadata aligned with 6-column
                            // (ensure these properties exist on the DTO)
                            ConsolidationStatus = consolidationLabel,
                            Date = now.Date,
                            DayTime = now,
                            Time = now.TimeOfDay,
                            Year = now.Year.ToString(),
                            BankAddress = header.BankAddress
                        };

                        // Crystal: inject header into each row
                        ApplyHeader(item, header);
                        return item;
                    })
                    .ToList();

                // ─────────────────────────────────────────────
                // 5) Store for Crystal & return success
                // ─────────────────────────────────────────────
                this.HttpContext.Session["rptSource"] = data;

                return Json(
                    new { success = true, message = "Trial balance 4-column report ready." },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet);
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
