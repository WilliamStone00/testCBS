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

                // Retrieve 6-column trial balance dataset
                var response = await _trialBalanceService.GetTrialBalancesAsync6columns(model);

                // Retrieve branch and bank metadata (for report header)
                var BranchInformation = await _branchServices.GetBranch(model.BranchId);

                // Stop processing if no financial records available
                if (response?.Lines == null || !response.Lines.Any())
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "No records found for the selected filters." }, JsonRequestBehavior.AllowGet);
                }

                // Stop processing if branch cannot be found
                if (BranchInformation == null)
                {
                    this.HttpContext.Session["rptSource"] = null;
                    return Json(new { success = false, message = "Selected branch not found." }, JsonRequestBehavior.AllowGet);
                }

                // Build report header (bank + branch static details displayed in the report)
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

                var now = DateTime.Now;

                // ========================================================================
                // STEP 1: Build each report line (per account)
                // ------------------------------------------------------------------------
                // • Extract opening / movement / closing values from the dataset
                // • Compute balances and differences for the current account only
                // • Attach bank & branch header metadata to the report row
                //   Crystal Reports requires the header to be present in each row
                // ========================================================================
                var data = response.Lines.Select(x =>
                {
                    var openingDr = x.OpeningDR ?? 0m;
                    var openingCr = x.OpeningCR ?? 0m;
                    var periodDr = x.PeriodDR ?? 0m;
                    var periodCr = x.PeriodCR ?? 0m;
                    var closingDr = x.ClosingDR ?? 0m;
                    var closingCr = x.ClosingCR ?? 0m;

                    var openingBalance = openingDr - openingCr;
                    var closingBalance = closingDr - closingCr;

                    var totalDebit = openingDr + periodDr;
                    var totalCredit = openingCr + periodCr;

                    var openingDiff = openingDr - openingCr;
                    var movementDiff = periodDr - periodCr;
                    var closingDiff = closingDr - closingCr;
                    var totalDiff = totalDebit - totalCredit;

                    var item = new TrialBalanceReportItem
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

                        Phone = BranchInformation.Telephone,
                        Address = BranchInformation.Address,
                        Username = _trialBalanceService.GetUserFullName(),
                        From = model.From,
                        To = model.To,
                        Mode = model.SourceMode == "Temp" ? "( TEMPORAL REPORT) " : $"( {model.SourceMode.ToUpper()} REPORT )",
                        Date = now.Date,
                        DayTime = now,
                        Time = now.TimeOfDay,
                        Year = now.Year.ToString()
                    };

                    ApplyHeader(item, header);
                    return item;
                }).ToList();

                // ========================================================================
                // STEP 2: Calculate global totals for footer
                // ------------------------------------------------------------------------
                // These totals are used by Crystal Reports to display financial control
                // totals covering the entire trial balance — not just a single account.
                // (Opening totals, movement totals, closing totals and net differences)
                // ========================================================================
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

                // ========================================================================
                // STEP 3: Embed global totals inside every row
                // ------------------------------------------------------------------------
                // Crystal Reports reads footer totals from rows, not from separate summary
                // objects. To ensure the totals appear accurately in all export formats
                // (PDF, Excel, Print), the totals are assigned to each row of the dataset.
                // ========================================================================
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

                // Send fully prepared dataset to Crystal
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
            // Path pointing to the .rpt file for Crystal Reports
            var reportPath = Server.MapPath("~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt");

            if (!System.IO.File.Exists(reportPath))
                return Json(new { success = false, message = "Report template file missing." }, JsonRequestBehavior.AllowGet);

            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "TrialBalance6Columns.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            this.HttpContext.Session["rpttitle"] = "TB6";

            return Json(new { success = true, message = "Report parameters set successfully." }, JsonRequestBehavior.AllowGet);
        }

        // Map bank and branch header details into each report row
        private void ApplyHeader(TrialBalanceReportItem item, BankHeaderInformation header)
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
    }
}
