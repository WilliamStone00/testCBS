using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.ExportReports;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using Newtonsoft.Json;
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
        private readonly TrialBalance6ColumnsExport _repoExcel;

        public TrialBalance6ColumnsController(
            TrialBalances6ColumnService trialBalanceService,
            BranchServices branchServices,
            TrialBalance6ColumnsExport repoexcel,
            BranchAccountService branchAccountService)
        {
            _branchServices = branchServices;
            _trialBalanceService = trialBalanceService;
            _repoExcel = repoexcel;
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
                // Always reset the report source at the beginning
                HttpContext.Session["rptSource"] = null;

                // ─────────────────────────────────────────────
                // 1) Load trial balance (6 columns)
                // ─────────────────────────────────────────────
                var response = await _trialBalanceService.GetTrialBalancesAsync6columns(model);

                string jsonFilter = JsonConvert.SerializeObject(model, Formatting.Indented);

                if (response?.Lines == null || !response.Lines.Any())
                {
                    return Json(
                        new { success = false, message = "No records found for the selected filters." },
                        JsonRequestBehavior.AllowGet);
                }

                // ─────────────────────────────────────────────
                // 2) Resolve branch context (for header)
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

                var now = DateTime.Now;
                var username = _trialBalanceService.GetUserFullName();
                var modeLabel = model.SourceMode == "Temp"
                    ? "( TEMPORAL REPORT) "
                    : $"( {model.SourceMode?.ToUpperInvariant()} REPORT )";

                var consolidationLabel = model.Consolidated
                    ? "CONSOLIDATED"
                    : "BRANCH LEVEL";

                // ─────────────────────────────────────────────
                // 4) Map lines → Crystal dataset rows
                //    - Attach header info to each row
                // ─────────────────────────────────────────────
                var data = response.Lines
                    .Select(x =>
                    {
                        var item = new TrialBalanceReportItem
                        {
                            AccountNumber = x.AccountNumber,
                            AccountName = x.AccountName,
                            OpeningDebit = x.OpeningDR,
                            OpeningCredit = x.OpeningCR,
                            MovementDebit = x.PeriodDR,
                            MovementCredit = x.PeriodCR,
                            ClosingDebit = x.ClosingDR,
                            ClosingCredit = x.ClosingCR,

                            TotalOpeningDebit = x.TotalOpeningDR,
                            TotalOpeningCredit = x.TotalOpeningCR,
                            TotalMovementDebit = x.TotalMovementDR,
                            TotalMovementCredit = x.TotalMovementCR,
                            TotalClosingDebit = x.TotalClosingDR,
                            TotalClosingCredit = x.TotalClosingCR,
                            TotalOpeningDifference = x.TotalOpeningDifference,
                            TotalMovementDifference = x.TotalMovementDifference,
                            TotalClosingDifference = x.TotalClosingDifference,

                            Phone = branch.Telephone,
                            Address = branch.Address,
                            Username = username,
                            From = model.From,
                            To = model.To,
                            Mode = modeLabel,
                            ConsolidationStatus = consolidationLabel,

                            Date = now.Date,
                            DayTime = now,
                            Time = now.TimeOfDay,
                            Year = now.Year.ToString(),
                            BankAddress = header.BankAddress
                        };

                        // Crystal: header per-row
                        ApplyHeader(item, header);
                        return item;
                    })
                    .ToList();


                _repoExcel.ExportTb6(data,@"C:\Exports\TB65454.xlsx", _trialBalanceService.GetUserFullName());








                // ─────────────────────────────────────────────
                // 5) Push prepared dataset to session for Crystal
                // ─────────────────────────────────────────────
                HttpContext.Session["rptSource"] = data;

                return Json(
                    new { success = true, message = "Trial balance report ready." },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // You can plug in your logger here if needed
                return Json(
                    new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet);
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
