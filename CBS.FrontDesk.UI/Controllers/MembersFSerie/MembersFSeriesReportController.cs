// ReportsController.cs
using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.ReportingMembersFSeries;
using CBS.FrontDesk.Data.Entity.ReportMembersFSeries;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis;
using CBS.FrontDesk.UI.AppFiles.Accountingv2Reporting.ReportRPT;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

            if (parameters.AccountIds != null)
            {
                parameters.AccountIds = parameters.AccountIds
                    .SelectMany(id => id.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(id => id.Trim())
                    .Distinct()
                    .ToList();
            }


            if (parameters == null)
                return Json(new { success = false, message = "No parameters provided." });

            var filter = new FinancialReportFilter
            {
                MemberReference = parameters.CustomerId,
                AccountIds = parameters.AccountIds,
                DateFrom = parameters.DateFrom,
                DateTo = parameters.DateTo,
                OperationDate = DateTime.Now,
                InterestVadPenaltyType = parameters.InterestVadPenaltyType
            };

            object mainData = null;
            Dictionary<string, object> subReports = null;

            string reportTitle = string.Empty;
            string relativePath = string.Empty;

            switch (parameters.ReportType)
            {
                /* ===================== MULTI / SUB REPORT ===================== */
                case "MemberSituation":
                    {
                        filter.ReportType = (int)FinancialReportType.MemberSituation;
                        filter.LoanId = parameters.LoanId;
                        filter.AccountId = parameters.AccountTypeId;

                        var memberSituation =
                            await _reportBuilder.BuildMemberSituationRows(filter);

                        if (memberSituation == null ||
                            !memberSituation.AccountSituations.Any() ||
                            !memberSituation.LoanHistories.Any())
                        {
                            return Json(new { success = false, message = "No data found." });
                        }

                        relativePath =
                            "~/AppFiles/Reporting/Transactions/UpdatedStatement/MemberSituation/MemberSituation.rpt";
                        reportTitle = "MEMBER SITUATION REPORT";

                        // MAIN REPORT DATA
                        mainData = new List<MemberSituationMainRpt> { memberSituation };

                        // SUB REPORTS
                        subReports = new Dictionary<string, object>
                        {
                            { "SubAccountSituationRPT", memberSituation.AccountSituations },
                            { "SubLoanSituationRPT", memberSituation.LoanHistories }
                        };
                        break;
                    }

                /* ===================== SINGLE REPORTS ===================== */
                case "AccountSituation":
                    filter.ReportType = (int)FinancialReportType.AccountSituation;
                    mainData = await _reportBuilder.BuildAccountSituationRows(filter);
                    break;

                case "AccountStatement":
                    filter.ReportType = (int)FinancialReportType.AccountStatement;
                    filter.AccountNumber = parameters.AccountTypeId;
                    mainData = await _reportBuilder.BuildAccountStatementRows(filter);
                    break;

                case "LoanRepayment":
                    filter.ReportType = (int)FinancialReportType.LoanRepayment;
                    filter.LoanId = parameters.LoanId;
                    filter.LoanRepaymentMode = parameters.ByRepayment
                        ? (int)LoanRepaymentReportMode.ByRepaymentPeriod
                        : (int)LoanRepaymentReportMode.BySpecificLoan;
                    mainData = await _reportBuilder.BuildLoanRepaymentRows(filter);
                    break;

                case "LoanSituation":
                    filter.ReportType = (int)FinancialReportType.LoanHistory;
                    filter.LoanStatus = parameters.LoanStatus;
                    mainData = await _reportBuilder.BuildLoanSituationRows(filter);
                    break;

                case "Interest":
                    filter.ReportType = (int)FinancialReportType.Interest;
                    mainData = await _reportBuilder.BuildInterestRows(filter);
                    break;

                case "VAT":
                    filter.ReportType = (int)FinancialReportType.VAT;
                    mainData = await _reportBuilder.BuildVatRows(filter);
                    break;

                case "Penalty":
                    filter.ReportType = (int)FinancialReportType.Penalty;
                    mainData = await _reportBuilder.BuildPenaltyRows(filter);
                    break;

                default:
                    return Json(new { success = false, message = "Invalid report type selected." });
            }

            if (mainData == null)
                return Json(new { success = false, message = "No data found." });

            /* ===================== SESSION BINDING ===================== */
            Session["MainData"] = mainData;
            Session["SubReportsData"] = subReports; // null for single reports
            Session["ReportFilter"] = filter;
            Session["ReportParameters"] = parameters;
            Session["ReportItemCount"] = GetItemCount(mainData);

            /* ===================== VIEWER ===================== */
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
                //count = GetItemCount(mainData)
            });
        }


        //Helper method to get item count
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
                if (parameters.ReportType == "MemberSituation")
                {
                    Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/UpdatedStatement/MemberSituation/MemberSituation.rpt";
                }
                else
                {
                    Session["rptpath"] = $"~/AppFiles/Accountingv2Reporting/ReportRPT/{reportName}";
                }
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

                case "AccountSituation":
                    return "AccountSituation.rpt";

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

                case "AccountSituation":
                    return "AccountSituation";

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