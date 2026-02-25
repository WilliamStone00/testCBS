using CBS.BusinessService.Accounting_V2.ExportReports;
using CBS.BusinessService.Accounting_V2.TrialBalance;
using CBS.BusinessService.Config;
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
        private readonly BranchServices _branchServices;

        /// <summary>
        /// Constructor – dependency injection
        /// </summary>
        public TrialBalance4ColumnsController(
            TrialBalances4ColumnService trialBalanceService,
            TrialBalance4ColumnsExport repoExcel, BranchServices branchServices)
        {
            _trialBalanceService = trialBalanceService 
                ?? throw new ArgumentNullException(nameof(trialBalanceService));

            _repoExcel = repoExcel 
                ?? throw new ArgumentNullException(nameof(repoExcel));
            _branchServices = branchServices;
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
        

        public async Task<ActionResult> DeleteDownloadedFiles()
        {
            try
            {
                string userName = _trialBalanceService.GetUserFullName();
                string directoryPath = Server.MapPath(
                    $"~/TempReportFiles/TrialBalance/{userName}");
                ClearTempReportFiles(directoryPath);
                return Json(new
                {
                    success = true,
                    message = "Temporary report files deleted successfully."
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


        public async Task<ActionResult> GenerateTrialBalance(AccountingV2ReportsFilter model)
        {
            try
            {
                HttpContext.Session["rptSource"] = null;

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

                string userName = _trialBalanceService.GetUserFullName();


                var branch = await _branchServices.GetBranch(model.BranchId);
                string branchName = branch?.Name ?? "Unknown";

                var branchcode = await _branchServices.GetBranch(model.BranchId);
                string branchcod = branch?.BranchCode ?? "Unknown";

                string directoryPath = Server.MapPath(
                    $"~/TempReportFiles/TrialBalance/{userName}");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string fileName =
    $"TB_4_{branchcod}_{DateTime.Now:yyyy_MM_dd}.xlsx";


                string fullPath = Path.Combine(directoryPath, fileName);

                if (!System.IO.File.Exists(fullPath))
                {
                    _repoExcel.ExportTb4(response, fullPath, userName);
                }

                // ✅ RETURN ALL FILES FOR USER
                var files = Directory.GetFiles(directoryPath)
                 .OrderByDescending(System.IO.File.GetCreationTime)
                 .Select(f => new
                 {
                     FileName = fileName,
                     fileSize = $"{new FileInfo(f).Length / 1024} KB",
                     Name = branchName
                 })
                 .ToList();

                HttpContext.Session["rptSource"] = response;

                return Json(new
                {
                    success = true,
                    message = "Trial balance (4 columns) report ready.",
                    files = files
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
            if (string.IsNullOrWhiteSpace(file))
                return HttpNotFound();

            // ✅ Current user
            string userName = _trialBalanceService.GetUserFullName();

            // ✅ User-scoped path
            string fullPath = Path.Combine(
                Server.MapPath($"~/TempReportFiles/TrialBalance/{userName}"),
                file
            );

            if (!System.IO.File.Exists(fullPath))
                return HttpNotFound();

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                file
            );
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

