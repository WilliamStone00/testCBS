using CBS.BusinessService.Accounting_V2.IPS.IPSReporting;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.API.IPSReporting;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.UI.Helper;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.IPS.IPSReporting
{
    [CheckSessionTimeOutAttribute]
    public class IPSReportingController : BaseController
    {
        private readonly IPSReportBuilderService _reportBuilder;
        private readonly BranchServices _branchServices;

        public IPSReportingController(
            IPSReportBuilderService reportBuilder,
            BranchServices branchServices)
        {
            _reportBuilder = reportBuilder;
            _branchServices = branchServices;
        }

        // GET: IPSReporting/Index
        public async Task<ActionResult> Index()
        {

            await loader();
            return View();
        }

        public async Task loader()
        {
            ViewBag.Branches = await _branchServices.GetBranches();

            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, 1, 1); // 1st January current year

            var model = new InsurancePremiumDto
            {
                StartDate = startDate,
                EndDate = today,
                ReportType = "InsurancePremium", // Default report type
                PrintOption = "PDF" // Default print option
            };
                        
            ViewBag.ReportType = new List<SelectListItem>
            {
               new SelectListItem { Text = "Insurance Premium", Value = "InsurancePremium" },
               new SelectListItem { Text = "Loan Protection", Value = "LoanProtection" },
               new SelectListItem { Text = "Life Savings", Value = "LifeSavings" },
               new SelectListItem { Text = "Loans In Excess Balance Of An Amount", Value = "LoansInExcessBalanceOfAnAmount" },
                new SelectListItem { Text = "Loans For Members Above An Age", Value = "LoansForMembersAboveAnAge" },
                new SelectListItem { Text = "Group Loans", Value = "GroupLoans" },
                new SelectListItem { Text = "Main and Staff Elected Loans", Value = "MainAndStaffElectedLoans" },
                new SelectListItem { Text = "Loans Summary Report", Value = "LoansSummaryReport" },
                new SelectListItem { Text = "Savings In Excess Balance of An Amount", Value = "SavingsInExcessBalanceOfAnAmount" },
                new SelectListItem { Text = "Group Savings", Value = "GroupSavings" },
                new SelectListItem { Text = "Savings For Deceased Members", Value = "SavingsForDeceasedMembers" },
                new SelectListItem { Text = "Savings Summary Report", Value = "SavingsSummaryReport" }

            };
                        
            ViewBag.PrintType = new List<SelectListItem>
            {
                new SelectListItem { Text = "Excel", Value = "Excel" },
               new SelectListItem { Text = "PDF", Value = "PDF" }
            };
          
        }

        // POST: IPSReporting/GenerateReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateReport(InsurancePremiumDto parameters)
        {
            Session["MainData"] = null;
            Session["SubReportsData"] = null;
            Session["ReportParameters"] = null;

            if (parameters.ReportType != "InsurancePremium")
                return Json(new { success = false, message = "Report Type not Yet available."});
            if (parameters == null)
                return Json(new { success = false, message = "No parameters provided." });

            // Validate dates
            if (parameters.StartDate > parameters.EndDate)
            {
                return Json(new { success = false, message = "Start date cannot be after end date." });
            }

            if (parameters.EndDate > DateTime.Today)
            {
                return Json(new { success = false, message = "End date cannot be in the future." });
            }


            try
            {
                // Build report data
                var reportData = await _reportBuilder.BuildInsurancePremiumRows(parameters);

                // Check for empty data
                if (reportData == null || !reportData.Any() || reportData.Count == 0)
                {
                    return Json(new { success = false, message = "No data found for the selected criteria." });
                }

                var startdate = DateHelper.FormatNullableDate(parameters.StartDate);
                var enddate = DateHelper.FormatNullableDate(parameters.EndDate);
                // Prepare report parameters for Crystal Reports
                var rptParameters = new Dictionary<string, object>
                {
                    { "DateFrom", startdate },
                    { "DateTo", enddate },
                    { "ReportTitle", "INSURANCE PREMIUM REPORT" },
                    { "PrintedOn", DateTime.Now.ToString("dd/MM/yyyy, hh:mm:ss") },
                    { "BranchId", parameters.BranchId },
                    { "PrintOption", parameters.PrintOption }
                };

                // Store in session
                Session["MainData"] = reportData;
                Session["ReportParameters"] = rptParameters;



                // Determine report path based on print option
                string relativePath = "IPSReports/CombinedInsurancePremiums.rpt";
                string reportTitle = "INSURANCE PREMIUM REPORT";
                // For PDF only option, we might use a different report layout
                if (parameters.PrintOption == "PDF" && parameters.ReportType == "InsurancePremium")
                {
                    // Use PDF optimized report if needed
                    relativePath = "IPSReports/CombinedInsurancePremiums.rpt";
                }

                var viewerUrl = Url.Content(
                    $"/ReportForm/ReportViewer.aspx" +
                    $"?reportPath={HttpUtility.UrlEncode(relativePath)}" +
                    $"&reportName={HttpUtility.UrlEncode(reportTitle)}"
                );

                return Json(new
                {
                    success = true,
                    redirectUrl = viewerUrl,
                    reportType = parameters.ReportType,
                    printOption = parameters.PrintOption
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating report: {ex.Message}" });
            }
        }

        // GET: IPSReporting/GetReportParameters
        //[HttpGet]
        //public async Task<ActionResult> GetReportParameters(string reportType)
        //{
        //    // This can be used to dynamically load parameters based on report type
        //    var branches = await _branchServices.G;

        //    return Json(new
        //    {
        //        success = true,
        //        branches = branches.Select(b => new { b.Id, b.Name }),
        //        defaultStartDate = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd"),
        //        defaultEndDate = DateTime.Today.ToString("yyyy-MM-dd")
        //    }, JsonRequestBehavior.AllowGet);
        //}

        // POST: IPSReporting/ExportReport
        [HttpPost]
        public ActionResult ExportReport(string format)
        {
            try
            {
                var reportData = Session["MainData"] as List<IPSflatobject>;
                var parameters = Session["ReportParameters"] as Dictionary<string, object>;

                if (reportData == null || !reportData.Any())
                {
                    return Json(new { success = false, message = "No report data found in session." });
                }

                // Load Crystal Report
                ReportDocument rd = new ReportDocument();
                string reportPath = Server.MapPath("~/AppFiles/Accountingv2Reporting/ReportRPT/InsurancePremiumReport.rpt");
                rd.Load(reportPath);

                // Set data source
                rd.SetDataSource(reportData);

                // Set parameters
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        rd.SetParameterValue(param.Key, param.Value);
                    }
                }

                // Export based on format
                ExportFormatType exportFormat = format.ToUpper() == "PDF"
                    ? ExportFormatType.PortableDocFormat
                    : ExportFormatType.Excel;

                using (var stream = rd.ExportToStream(exportFormat))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    string mimeType = format.ToUpper() == "PDF"
                        ? "application/pdf"
                        : "application/vnd.ms-excel";

                    string fileName = $"InsurancePremium_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToLower()}";

                    return File(buffer, mimeType, fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Export failed: {ex.Message}" });
            }
        }
    }
}