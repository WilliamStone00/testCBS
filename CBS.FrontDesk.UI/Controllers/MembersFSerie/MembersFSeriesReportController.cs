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
            if (parameters == null)
            {
                return Json(new { success = false, message = "No parameters provided." });
            }

            FinancialReportFilter filter = new FinancialReportFilter
            {
                MemberReference = parameters.CustomerId,
                AccountIds = parameters.AccountIds,
                DateFrom = parameters.DateFrom,
                DateTo = parameters.DateTo,
                OperationDate = DateTime.Now
            };

            object reportData = null;

            switch (parameters.ReportType)
            {
                case "MemberSituation":
                    filter.ReportType = (int)FinancialReportType.MemberSituation;
                    filter.LoanId = parameters.LoanId;
                    filter.AccountId = parameters.AccountTypeId;
                    reportData = await _reportBuilder.BuildMemberSituationRows(filter);
                    break;

                case "accountSituation":
                    filter.ReportType = (int)FinancialReportType.AccountSituation;
                    reportData = await _reportBuilder.BuildAccountSituationRows(filter);
                    break;

                case "AccountStatement":
                    filter.ReportType = (int)FinancialReportType.AccountStatement;
                    filter.AccountNumber = parameters.AccountTypeId;
                    reportData = await _reportBuilder.BuildAccountStatementRows(filter);
                    break;

                case "LoanRepayment":
                    filter.ReportType = (int)FinancialReportType.LoanRepayment;
                    filter.LoanId = parameters.LoanId;
                    filter.LoanRepaymentMode = parameters.ByRepayment
                        ? (int)LoanRepaymentReportMode.ByRepaymentPeriod
                        : (int)LoanRepaymentReportMode.BySpecificLoan;
                    reportData = await _reportBuilder.BuildLoanRepaymentRows(filter);
                    break;

                case "LoanHistory":
                    filter.ReportType = (int)FinancialReportType.LoanHistory;
                    filter.LoanStatus = parameters.LoanStatus;
                   // reportData = await _reportBuilder.BuildLoanHistoryRows(filter);
                    break;

                default:
                    return Json(new { success = false, message = "Invalid report type selected." });
            }

            if (reportData == null)
            {
                return Json(new { success = false, message = "No data found." });
            }

            Session["rptSource"] = reportData;
            Session["ReportFilter"] = filter;

            // Store data in session for Crystal Reports
            //Session["rptSource"] = reportData;

            Session["ReportParameters"] = parameters;
            Session["ReportItemCount"] = GetItemCount(reportData);

            return Json(new
            {
                success = true,
                reportType = parameters.ReportType,
                count = GetItemCount(reportData)
            });
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