// ReportsController.cs
using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.ReportingMembersFSeries;
using CBS.FrontDesk.Data.Entity.ReportMembersFSeries;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.UI.AppFiles.Accountingv2Reporting.ReportRPT;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers
{
    public class MembersFSeriesReportController : BaseController
    {
        private readonly FinancialReportBuilder _reportBuilder;
        private readonly BranchServices _branchServices;
        private readonly AccountServices _accountServices;
        private readonly LoanServices _loanServices;

        public MembersFSeriesReportController(
            BranchServices branchServices,
            AccountServices accountServices,
            LoanServices loanServices,
            FinancialReportBuilder reportBuilder)
        {
            _branchServices = branchServices;
            _accountServices = accountServices;
            _loanServices = loanServices;
            _reportBuilder = reportBuilder;
        }
                
        public async Task<ActionResult> Index()
        {
          
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateReport(ReportParameters parameters)
        {
            try
            {
                // DEBUG: Log all form values
                System.Diagnostics.Debug.WriteLine("=== FORM DATA RECEIVED ===");
                foreach (var key in Request.Form.AllKeys)
                {
                    System.Diagnostics.Debug.WriteLine($"{key}: {Request.Form[key]}");
                }
                System.Diagnostics.Debug.WriteLine("==========================");

                // DEBUG: Log model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    System.Diagnostics.Debug.WriteLine("ModelState Errors: " + string.Join(", ", errors));
                }

                // Log received parameters
                System.Diagnostics.Debug.WriteLine($"ReportType: {parameters?.ReportType}");
                System.Diagnostics.Debug.WriteLine($"AccountTypeId: {parameters?.AccountTypeId}");
                System.Diagnostics.Debug.WriteLine($"LoanId: {parameters?.LoanId}");
                System.Diagnostics.Debug.WriteLine($"DateFrom: {parameters?.DateFrom}");
                System.Diagnostics.Debug.WriteLine($"DateTo: {parameters?.DateTo}");

                // Basic validation
                if (parameters == null)
                {
                    return Json(new { success = false, message = "No parameters provided." });
                }

                if (string.IsNullOrEmpty(parameters.ReportType))
                {
                    return Json(new { success = false, message = "Report type is required." });
                }

                object reportData = null;

                switch (parameters.ReportType)
                {
                    case "MemberSituation":
                        // Validate Member Situation required fields
                        if (string.IsNullOrEmpty(parameters.AccountTypeId))
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Account Type is required for Member Situation report."
                            });
                        }

                        if (string.IsNullOrEmpty(parameters.LoanId))
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Loan selection is required for Member Situation report."
                            });
                        }



                        reportData = await _reportBuilder.BuildMemberSituationRows(parameters);
                        break;

                    case "AccountStatement":
                        // Validate Account Statement required fields
                        if (string.IsNullOrEmpty(parameters.AccountTypeId))
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Account selection is required for Account Statement."
                            });
                        }
                                             

                        reportData = await _reportBuilder.BuildAccountStatementRows(parameters);
                        break;

                    case "LoanRepayment":
                        // Validate Loan Repayment required fields
                        if (string.IsNullOrEmpty(parameters.LoanId))
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Loan selection is required for Loan Repayment report."
                            });
                        }

                       

                        reportData = await _reportBuilder.BuildLoanRepaymentRows(parameters);
                        break;

                    case "LoanSituation":
                        // Validate Loan Situation required fields
                       
                        reportData = await _reportBuilder.BuildLoanSituationRows(parameters);
                        break;

                    case "accountSituation":
                        // Validate Loan Situation required fields
                       
                        reportData = await _reportBuilder.BuildAccountSituationRows(parameters);
                        break;

                    default:
                        return Json(new { success = false, message = "Invalid report type selected." });
                }

                if (reportData == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No data found for the selected criteria."
                    });
                }

                // Store data in session for Crystal Reports
                Session["rptSource"] = reportData;
                Session["ReportParameters"] = parameters;
                Session["ReportItemCount"] = GetItemCount(reportData);

                return Json(new
                {
                    success = true,
                    count = GetItemCount(reportData),
                    reportType = parameters.ReportType,
                    message = $"Report generated successfully. Found {GetItemCount(reportData)} records."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Report Generation Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = $"Error generating report: {ex.Message}"
                });
            }
        }
       
        // Helper method to get item count
        private int GetItemCount(object data)
        {
            if (data == null) return 0;

            if (data is System.Collections.ICollection collection)
                return collection.Count;

            // Try reflection for List<T>
            var countProperty = data.GetType().GetProperty("Count");
            if (countProperty != null)
            {
                return (int)countProperty.GetValue(data);
            }

            // Try Count() extension method
            var countMethod = data.GetType().GetMethod("Count");
            if (countMethod != null)
            {
                return (int)countMethod.Invoke(data, null);
            }

            return 0;
        }

        // POST: Reports/PrepareReport
        [HttpPost]
        public ActionResult PrepareReport()
        {
            try
            {
                var parameters = Session["ReportParameters"] as ReportParameters;
                if (parameters == null)
                {
                    return Json(new { success = false, message = "Report parameters not found." });
                }

                // Determine which report template to use
                string reportName = GetReportName(parameters.ReportType);
                string reportTitle = GetReportTitle(parameters.ReportType);

                Session["rptType"] = "ReportParameterLess";
                Session["ReportName"] = reportName;
                Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/{reportName}";
         
            Session["rpttitle"] = reportTitle;
                Session["DateFrom"] = parameters.DateFrom.ToString("dd/MM/yyyy");
                Session["DateTo"] = parameters.DateTo.ToString("dd/MM/yyyy");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //// GET: Reports/ReportParameterLessCOLL
        //public void ReportParameterLessCOLL()
        //{
        //    ReportDocument rd = new ReportDocument();
        //    try
        //    {
        //        string strReportName = Session["ReportName"]?.ToString();
        //        var rptSource = Session["rptSource"];
        //        var rptpath = Session["rptpath"]?.ToString();
        //        var rpttitle = Session["rpttitle"]?.ToString();

        //        if (string.IsNullOrEmpty(strReportName) || rptSource == null || rptpath == null || rpttitle == null)
        //        {
        //            Response.Write("<H2>❌ No Report with such Name found</H2>");
        //            Response.Write($"<p>ReportName: {strReportName ?? "null"}</p>");
        //            return;
        //        }

        //        string strRptPath = Server.MapPath(rptpath);

        //        // Check if report file exists
        //        if (!System.IO.File.Exists(strRptPath))
        //        {
        //            Response.Write($"<H2>❌ Report file not found</H2>");
        //            Response.Write($"<p>Path: {strRptPath}</p>");
        //            return;
        //        }

        //        rd.Load(strRptPath);
        //        rd.SetDataSource(rptSource);

        //        // Set parameters if available
        //        string strFromDate = Session["DateFrom"]?.ToString() ?? string.Empty;
        //        string strToDate = Session["DateTo"]?.ToString() ?? string.Empty;

        //        if (!string.IsNullOrEmpty(strFromDate) && !string.IsNullOrEmpty(strToDate))
        //        {
        //            try
        //            {
        //                rd.SetParameterValue("DateFrom", strFromDate);
        //                rd.SetParameterValue("DateTo", strToDate);
        //            }
        //            catch
        //            {
        //                // Continue without parameters
        //            }
        //        }

        //        // Export to PDF
        //        string savedFileName = $"{rpttitle}-{DateTime.UtcNow:dd_MM_yyyy_HHmmss}";
        //        rd.ExportToHttpResponse(
        //            ExportFormatType.PortableDocFormat,
        //            Response,
        //            false,
        //            savedFileName);
        //    }
        //    catch (CrystalDecisions.CrystalReports.Engine.LoadSaveReportException loadEx)
        //    {
        //        Response.Write($"<H2>❌ Report Loading Error</H2>");
        //        Response.Write($"<p>{loadEx.Message}</p>");
        //    }
        //    catch (CrystalDecisions.CrystalReports.Engine.DataSourceException dataEx)
        //    {
        //        Response.Write($"<H2>❌ Data Binding Error</H2>");
        //        Response.Write($"<p>{dataEx.Message}</p>");
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Write($"<H2>❌ An error occurred while generating the report</H2>");
        //        Response.Write($"<p>{ex.Message}</p>");
        //    }
        //    finally
        //    {
        //        CleanReport(rd);
        //    }
        //}

        // GET: Reports/DebugReportData
        public ActionResult DebugReportData()
        {
            try
            {
                if (Session["rptSource"] == null)
                {
                    return Content("Session[rptSource] is null");
                }

                var data = Session["rptSource"];
                var parameters = Session["ReportParameters"] as ReportParameters;

                string result = $"Report Type: {parameters?.ReportType}<br>";
                result += $"Data Type: {data.GetType().FullName}<br>";
                result += $"Item Count: {Session["ReportItemCount"]}<br><br>";

                // Display first few items
                if (data is List<AccountStatementRow> accountStatement)
                {
                    result += $"Account Statement Rows: {accountStatement.Count}<br>";
                    if (accountStatement.Count > 0)
                    {
                        var first = accountStatement[0];
                        result += $"First Item: Bank={first.BankName}, Account={first.AccountNo}<br>";
                    }
                }
                else if (data is List<MemberSituationRow> memberSituation)
                {
                    result += $"Member Situation Rows: {memberSituation.Count}<br>";
                }
                // Add other types as needed

                return Content(result);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}<br>{ex.StackTrace}");
            }
        }

        // Helper methods
        private string GetReportName(string reportType)
        {
            switch (reportType)
            {
                case "MemberSituation":
                    return "MemberSituation.rpt";

                case "AccountStatement":
                    return "AccountStatement.rpt";

                case "LoanRepayment":
                    return "LoanRepayment.rpt";

                case "LoanSituation":
                    return "LoanSituation.rpt";

                default:
                    return "FinancialReport.rpt";
            }
        }

        private string GetReportTitle(string reportType)
        {
            switch (reportType)
            {
                case "MemberSituation":
                    return "MemberSituationReport";

                case "AccountStatement":
                    return "AccountStatement";

                case "LoanRepayment":
                    return "LoanRepaymentReport";

                case "LoanSituation":
                    return "LoanSituationReport";

                default:
                    return "FinancialReport";
            }
        }


      

        private void CleanReport(ReportDocument rd)
        {
            if (rd != null)
            {
                rd.Close();
                rd.Dispose();
            }
        }
    }
}