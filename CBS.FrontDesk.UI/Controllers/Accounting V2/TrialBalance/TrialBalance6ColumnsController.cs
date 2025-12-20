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
    public class TrialBalance6ColumnsController : BaseController
    {
        /// <summary>
        /// Service responsible for building the Trial Balance (6 columns) dataset.
        /// Responsible for exporting the Trial Balance 6-Column Excel report.
        /// </summary>
        private readonly TrialBalances6ColumnService _trialBalanceService;
        private readonly TrialBalance6ColumnsExport _repoExcel;

        /// <summary>
        /// Controller constructor – dependency injection ensures service availability.
        /// </summary>
        public TrialBalance6ColumnsController(
            TrialBalances6ColumnService trialBalanceService,
            TrialBalance6ColumnsExport repoExcel)
        {
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
            _repoExcel = repoExcel ?? throw new ArgumentNullException(nameof(repoExcel));
        }


        /// <summary>
        /// Displays the Trial Balance 6 Columns main page.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }



        /// <summary>
        /// Generates the Trial Balance (6 columns) using the provided filter and exports it to Excel.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateTrialBalance(AccountingV2ReportsFilter model)
        {
            try
            {
                // Reset report dataset in session
                HttpContext.Session["rptSource"] = null;

                // Build Trial Balance dataset
                var response = await _trialBalanceService.BuildTrialBalanceDataset(model);

                if (response == null || !response.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No records found for the selected filters."
                    }, JsonRequestBehavior.AllowGet);
                }

                string userName = _trialBalanceService.GetUserFullName();

                // ✅ User-based folder
                string directoryPath = Server.MapPath(
                    $"~/TempReportFiles/TrialBalance6/{userName}");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // ✅ User-based filename
                string fileName =
                    $"TrialBalance6_{userName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.xlsx";

                string fullPath = Path.Combine(directoryPath, fileName);

                // ✅ Create only if not exists
                if (!System.IO.File.Exists(fullPath))
                {
                    _repoExcel.ExportTb6(
                        response,
                        fullPath,
                        userName);
                }

                HttpContext.Session["rptSource"] = response;

                // ✅ Return all files for user (latest first)
                var files = Directory.GetFiles(directoryPath)
                    .OrderByDescending(System.IO.File.GetCreationTime)
                    .Select(Path.GetFileName)
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "Trial balance report ready.",
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
        /// Handles the physical download of the generated Excel file.
        /// The file is deleted immediately after downloading.
        /// </summary>
        public ActionResult Download(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return HttpNotFound();

            string userName = _trialBalanceService.GetUserFullName();

            string fullPath = Path.Combine(
                Server.MapPath($"~/TempReportFiles/TrialBalance6/{userName}"),
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


        public string ClearTempReportFiles(string directoryPath)
        {
           

            // Log or return the folder path
            System.Diagnostics.Debug.WriteLine("Cleaning TempReportFiles directory: " + directoryPath);

            if (Directory.Exists(directoryPath))
            {
                // Delete all files
                foreach (string file in Directory.GetFiles(directoryPath))
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to delete file: " + file + " | " + ex.Message);
                    }
                }

                // Delete all subfolders
                foreach (string subDir in Directory.GetDirectories(directoryPath))
                {
                    try
                    {
                        Directory.Delete(subDir, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to delete folder: " + subDir + " | " + ex.Message);
                    }
                }

                return $"Temp files cleared successfully. Path: {directoryPath}";
            }
            else
            {
                return $"Folder does not exist. Path: {directoryPath}";
            }
        }



        /// <summary>
        /// Prepares Crystal Report parameters for Trial Balance (6 columns) preview.
        /// </summary>
        [HttpPost]
        public ActionResult GetReport(string path)
        {
            string reportPath = Server.MapPath("~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt");

            // Template file must exist
            if (!System.IO.File.Exists(reportPath))
                return Json(new { success = false, message = "Report template file missing." },
                    JsonRequestBehavior.AllowGet);

            // Store Crystal parameters in session
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "TrialBalance6Columns.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/TrialBalance6Columns.rpt";
            this.HttpContext.Session["rpttitle"] = "TB6";

            return Json(new
            {
                success = true,
                message = "Report parameters set successfully."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
