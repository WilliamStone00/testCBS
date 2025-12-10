//using CBS.BusinessService.Accounting_V2.BranchAccountService;
//using CBS.BusinessService.Accounting_V2.TrialBalance;
//using CBS.BusinessService.Config;

//using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
//using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
//using CBS.FrontDesk.Data.ReportDataSetDto;
//using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;

//using CrystalDecisions.CrystalReports.Engine;
//using CrystalDecisions.Shared;
//using CrystalDecisions.Web;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Mvc;

//namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
//{
//    public class TrialBalance4ColmunsController : BaseController
//    {
//        private readonly TrialBalances4ColumnService _trialBalanceService;
//        private readonly BranchAccountService _branchAccountService;
//        private readonly BranchServices _branchServices;

//        public TrialBalance4ColmunsController(
//            TrialBalances4ColumnService trialBalanceService,
//            BranchServices branchServices,
//            BranchAccountService branchAccountService)
//        {
//            _branchServices = branchServices;
//            _trialBalanceService = trialBalanceService;
//            _branchAccountService = branchAccountService;
//        }

//        public async Task<ActionResult> Index()
//        {
//            return View();
//        }

//        [HttpPost]

//        public async Task<ActionResult> GenerateTrialBalance(AccountingV2ReportsFilter model)
//        {
//            try
//            {
//                // Always reset at the beginning
//                this.HttpContext.Session["rptSource"] = null;

//                // ─────────────────────────────────────────────
//                // 1) Load 4-column trial balance
//                //    (service already handles IncludeZero)
//                // ─────────────────────────────────────────────
//                var response = await _trialBalanceService.GetTrialBalancesAsync4columns(model);

//                if (response == null || !response.Any())
//                {
//                    return Json(
//                        new { success = false, message = "No data found for the selected filters." },
//                        JsonRequestBehavior.AllowGet);
//                }

//                // ─────────────────────────────────────────────
//                // 2) Resolve branch context (same logic as 6-column)
//                //    - Consolidated → current user's branch
//                //    - BranchId null → current user's branch
//                //    - Else → selected branch
//                // ─────────────────────────────────────────────
//                var currentBranchId = _branchServices.GetBranchID();
//                var branchIdToLoad = model.Consolidated || string.IsNullOrWhiteSpace(model.BranchId)
//                    ? currentBranchId
//                    : model.BranchId;

//                var branch = await _branchServices.GetBranch(branchIdToLoad);
//                if (branch == null)
//                {
//                    return Json(
//                        new { success = false, message = "Selected branch not found." },
//                        JsonRequestBehavior.AllowGet);
//                }

//                // ─────────────────────────────────────────────
//                // 3) Build static header metadata (bank + branch)
//                // ─────────────────────────────────────────────
//                var header = new BankHeaderInformation
//                {
//                    BankId = branch.Bank.Id,
//                    BankBankCode = branch.Bank.BankCode,
//                    BankName = branch.Bank.Name,
//                    BankTelephone = branch.Bank.Telephone,
//                    BankEmail = branch.Bank.Email,
//                    BankAddress = branch.Bank.Address,
//                    BankLogoUrl = branch.Bank.LogoUrl,
//                    BankMotto = branch.Bank.Motto,
//                    BankRegistrationNumber = branch.Bank.RegistrationNumber,
//                    BankImmatriculationNumber = branch.Bank.ImmatriculationNumber,
//                    BankPBox = branch.Bank.PBox,

//                    BranchId = branch.Id,
//                    BranchCode = branch.BranchCode,
//                    BranchName = branch.Name,
//                    BranchTelephone = branch.Telephone,
//                    BranchEmail = branch.Email,
//                    BranchAddress = branch.Address,
//                    BranchLogoUrl = branch.LogoUrl,
//                    BranchCapital = branch.Capital,
//                    BranchRegistrationNumber = branch.RegistrationNumber,
//                    BranchImmatriculationNumber = branch.ImmatriculationNumber,
//                    BranchPBox = branch.PBox
//                };

//                // Common metadata (same pattern as 6-column)
//                var now = DateTime.Now;
//                var username = _trialBalanceService.GetUserFullName();

//                var modeLabel = model.SourceMode == "Temp"
//                    ? "( TEMPORAL REPORT) "
//                    : $"( {model.SourceMode?.ToUpperInvariant()} REPORT )";

//                var consolidationLabel = model.Consolidated
//                    ? "CONSOLIDATED"
//                    : "BRANCH LEVEL";

//                // ─────────────────────────────────────────────
//                // 4) Map to 4-column DTO + header & metadata
//                //    (DTO shape remains the same)
//                // ─────────────────────────────────────────────
//                var data = response
//                    .Select(x =>
//                    {
//                        var item = new TrialBalanceFourColumnsFlatItems
//                        {
//                            // Core 4-column values
//                            AccountNumber = x.AccountNumber,
//                            AccountName = x.AccountName,
//                            Debit = x.Debit,
//                            Credit = x.Credit,

//                            BeginningBalance = x.BeginningBalance,
//                            EndingBalance = x.EndingBalance,
//                            OpeningDebit = x.BeginningBalance,     // same as before
//                            TotalBeginningNet = x.TotalBeginningNet,
//                            TotalEndingNet = x.TotalEndingNet,
//                            TotalCredit = x.TotalCredit,
//                            TotalDebit = x.TotalDebit,
//                            TotalMovementNet = x.TotalMovementNet,
//                            TotalBeginningSide = x.TotalBeginningSide,
//                            TotalEndingSide = x.TotalEndingSide,
//                            BeginningBookingDirection = x.BeginningSide,
//                            EndingBookingDirection = x.EndingSide,

//                            // Branch / bank display info
//                            Phone = branch.Telephone,
//                            Address = branch.Address,
//                            BranchName = branch.Name,
//                            BranchTel = branch.Telephone,
//                            BranchEmail = branch.Email,
//                            HeadOfficePhone = branch.Bank.Telephone,
//                            BranchImmatriculationNumber = branch.Bank.ImmatriculationNumber,
//                            LogoUrl = branch.Bank.LogoUrl,
//                            PBox = branch.PBox,
//                            RegistrationNumber = branch.Bank.RegistrationNumber,
//                            DisplayName = branch.DisplayName,
//                            Motto = branch.Bank.Motto,
//                            BranchLogoUrl = branch.LogoUrl,
//                            BranchCode = branch.BranchCode,

//                            // Report metadata
//                            Username = username,
//                            From = model.From,
//                            To = model.To,
//                            Mode = modeLabel,

//                            // New common metadata aligned with 6-column
//                            // (ensure these properties exist on the DTO)
//                            ConsolidationStatus = consolidationLabel,
//                            Date = now.Date,
//                            DayTime = now,
//                            Time = now.TimeOfDay,
//                            Year = now.Year.ToString(),
//                            BankAddress = header.BankAddress
//                        };

//                        // Crystal: inject header into each row
//                        ApplyHeader(item, header);
//                        return item;
//                    })
//                    .ToList();

//                // ─────────────────────────────────────────────
//                // 5) Store for Crystal & return success
//                // ─────────────────────────────────────────────
//                this.HttpContext.Session["rptSource"] = data;

//                return Json(
//                    new { success = true, message = "Trial balance 4-column report ready." },
//                    JsonRequestBehavior.AllowGet);
//            }
//            catch (Exception ex)
//            {
//                return Json(
//                    new { success = false, message = ex.Message },
//                    JsonRequestBehavior.AllowGet);
//            }
//        }




//        private void ApplyHeader(TrialBalanceFourColumnsFlatItems item, BankHeaderInformation header)
//        {
//            item.BankId = header.BankId;
//            item.BankBankCode = header.BankBankCode;
//            item.BankName = header.BankName;
//            item.BankTelephone = header.BankTelephone;
//            item.BankEmail = header.BankEmail;
//            item.BankAddress = header.BankAddress;
//            item.BankLogoUrl = header.BankLogoUrl;
//            item.BankMotto = header.BankMotto;
//            item.BankRegistrationNumber = header.BankRegistrationNumber;
//            item.BankImmatriculationNumber = header.BankImmatriculationNumber;
//            item.BankPBox = header.BankPBox;

//            item.BranchId = header.BranchId;
//            item.BranchCode = header.BranchCode;
//            item.BranchName = header.BranchName;
//            item.BranchTelephone = header.BranchTelephone;
//            item.BranchEmail = header.BranchEmail;
//            item.BranchAddress = header.BranchAddress;
//            item.BranchLogoUrl = header.BranchLogoUrl;
//            item.BranchCapital = header.BranchCapital;
//            item.BranchRegistrationNumber = header.BranchRegistrationNumber;
//            item.BranchImmatriculationNumber = header.BranchImmatriculationNumber;
//            item.BranchPBox = header.BranchPBox;
//        }























//        [HttpPost]
//        public ActionResult GetReport(string path)
//        {
//            // Crystal report metadata
//            this.HttpContext.Session["rptType"] = "ReportParameterLess";
//            this.HttpContext.Session["ReportName"] = "TrialBalance4Columns.rpt";
//            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance4Columns.rpt";
//            this.HttpContext.Session["rpttitle"] = "TB4";

//            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);
//        }
//    }
//}



using CBS.BusinessService.Accounting_V2.ExportReports;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance
{
    public class TrialBalance4ColumnsController : BaseController
    {
        /// <summary>
        /// Service responsible for building Trial Balance (4 columns) dataset.
        /// Service responsible for exporting Trial Balance 4-Column Excel report.
        /// </summary>
        private readonly TrialBalances4ColumnService _trialBalanceService;
        private readonly TrialBalance4ColumnsExport _repoExcel;

        /// <summary>
        /// Constructor – dependency injection
        /// </summary>
        public TrialBalance4ColumnsController(
            TrialBalances4ColumnService trialBalanceService,
            TrialBalance4ColumnsExport repoExcel)
        {
            _trialBalanceService = trialBalanceService 
                ?? throw new ArgumentNullException(nameof(trialBalanceService));

            _repoExcel = repoExcel 
                ?? throw new ArgumentNullException(nameof(repoExcel));
        }

        /// <summary>
        /// Displays Trial Balance 4 Columns main page
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Generates Trial Balance (4 Columns) and exports to Excel
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateTrialBalance(AccountingV2ReportsFilter model)
        {
            try
            {
                // Reset report session
                HttpContext.Session["rptSource"] = null;

                // Build dataset (same pattern as TB6)
                var response = await _trialBalanceService
                    .BuildTrialBalanceDataset(model);

                if (response == null || !response.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No records found for the selected filters."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Prepare file info
                string fileName = $"TrialBalance4_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempReportFiles");

                // Ensure directory exists & clean old files
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                else
                {
                    ClearTempReportFiles(directoryPath);
                }

                string fullPath = Path.Combine(directoryPath, fileName);

                // Export to Excel
                _repoExcel.ExportTb4(
                    response,
                    fullPath,
                    _trialBalanceService.GetUserFullName());

                // Store dataset for Crystal Reports preview
                HttpContext.Session["rptSource"] = response;

                return Json(new
                {
                    success = true,
                    message = "Trial balance (4 columns) report ready.",
                    downloadFile = fileName
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Downloads the generated Excel file
        /// </summary>
        public ActionResult Download(string file)
        {
            string fullPath = Path.Combine(Server.MapPath("~/TempReportFiles"), file);

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);


            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                file);
        }

        /// <summary>
        /// Clears temporary report files
        /// </summary>
        private void ClearTempReportFiles(string directoryPath)
        {
            System.Diagnostics.Debug.WriteLine(
                "Cleaning TempReportFiles directory: " + directoryPath);

            if (!Directory.Exists(directoryPath))
                return;

            foreach (string file in Directory.GetFiles(directoryPath))
            {
                try { System.IO.File.Delete(file); }
                catch { /* log if needed */ }
            }

            foreach (string dir in Directory.GetDirectories(directoryPath))
            {
                try { Directory.Delete(dir, true); }
                catch { /* log if needed */ }
            }
        }

        /// <summary>
        /// Prepares Crystal parameters for Trial Balance 4 Columns
        /// </summary>
        [HttpPost]
        public ActionResult GetReport(string path)
        {
            string reportPath = Server.MapPath(
                "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance4Columns.rpt");

            if (!System.IO.File.Exists(reportPath))
            {
                return Json(new
                {
                    success = false,
                    message = "Report template file missing."
                }, JsonRequestBehavior.AllowGet);
            }

            HttpContext.Session["rptType"] = "ReportParameterLess";
            HttpContext.Session["ReportName"] = "TrialBalance4Columns.rpt";
            HttpContext.Session["rptpath"] =
                "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance4Columns.rpt";
            HttpContext.Session["rpttitle"] = "TB4";

            return Json(new
            {
                success = true,
                message = "Report parameters set successfully."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}

