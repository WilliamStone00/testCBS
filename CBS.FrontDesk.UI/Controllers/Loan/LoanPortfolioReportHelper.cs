using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Reporting;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.FrontDesk.UI.AppFiles.Reporting.Transactions.UpdatedStatement.Loan;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers
{
    public class LoanPortfolioReportHelper
    {
        private readonly LoanServices _loanService;
        private readonly Controller _controller;

        public LoanPortfolioReportHelper(LoanServices loanService, Controller controller)
        {
            _loanService = loanService;
            _controller = controller;
        }

        public async Task<JsonResult> GenerateStandardReportAsync(GenerateLoanPortfolioReportCommand reportCommand)
        {
            return await GenerateReportInternalAsync(reportCommand, useQueryParam: false);
        }

        public async Task<JsonResult> GenerateQueryBasedReportAsync(GenerateLoanPortfolioReportCommand reportCommand)
        {
            return await GenerateReportInternalAsync(reportCommand, useQueryParam: true);
        }

        private async Task<JsonResult> GenerateReportInternalAsync(GenerateLoanPortfolioReportCommand reportCommand, bool useQueryParam)
        {
            var reportType = (reportCommand.MainReportType ?? "All").ToLowerInvariant();
            var rptData = new LoanDelinquencyReportResultRPT();
            var delinquencyData = new LoanDelinquencyReportDto();
            string reportTitle, relativePath;

            switch (reportType)
            {
                case "currentloan":
                    relativePath = "Loan/PortFolio/CurrentLoanRPT.rpt";
                    reportTitle = "Current Loans Report";
                    rptData = await _loanService.GetLoanPortfolioAnalysisAsync(reportCommand);
                    if (rptData == null)
                        return new JsonResult
                        {
                            Data = new { success = false, message = "No data found." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    break;

                case "delinquentloansummary":
                    relativePath = "Loan/LoanDeliquentReport/DeliquentLoanSummaryMAINRPT.rpt";
                    reportTitle = "LOANS DELINQUENCY SUMMARY REPORT";
                    delinquencyData = await _loanService.GetLoanDelinquencyReportAsync(reportCommand);
                    if (delinquencyData == null || delinquencyData.CategorySummaries == null || !delinquencyData.FlattenedRows.Any())
                        return new JsonResult
                        {
                            Data = new { success = false, message = "No delinquency data found." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    break;
               
                case "loansituations":
                    relativePath = "Loan/LoanSituationAlpha/LoanSituationAlphaRPT.rpt";
                    reportTitle = "LOANS SITUATION REPORT";
                    delinquencyData = await _loanService.GetLoanDelinquencyReportAsync(reportCommand);
                    if (delinquencyData == null || delinquencyData.CategorySummaries == null || !delinquencyData.LoanEntries.Any())
                        return new JsonResult
                        {
                            Data = new { success = false, message = "No loan situation data found." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    break;
                case "delinquentloanslistingwithaging":
                    relativePath = "Loan/LoanSituationAlpha/DeliquentLoanListingWithAgingRPT.rpt";
                    reportTitle = "DELINQUENT LOANS WITH AGING REPORT";
                    delinquencyData = await _loanService.GetLoanDelinquencyReportAsync(reportCommand);
                    if (delinquencyData == null || delinquencyData.CategorySummaries == null || !delinquencyData.LoanEntries.Any())
                        return new JsonResult
                        {
                            Data = new { success = false, message = "No loan situation data found." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    break;

                case "delinquentloan":
                    relativePath = "Loan/PortFolio/DelinquentLoansRPT.rpt";
                    reportTitle = "PORTFOLIO OF DELINQUENT LOANS";
                    rptData = await _loanService.GetLoanPortfolioAnalysisAsync(reportCommand);
                    if (rptData == null)
                        return new JsonResult
                        {
                            Data = new { success = false, message = "No data found." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    break;
                

                case "loanbypurpose":
                case "loanbytypes":
                    relativePath = "Loan/PortFolio/LoanByPurposeRPT.rpt";
                    reportTitle = "Loans by Purpose Report";
                    rptData = await _loanService.GetLoanPortfolioAnalysisAsync(reportCommand);
                    if (rptData == null)
                        return new JsonResult
                        {
                            Data = new { success = false, message = "No data found." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    break;
                case "LoanGeneralR":
                   relativePath = "Transactions/UpdatedStatement/Loan/GeneralLoanReportRPT.rpt";
                    reportTitle = "Loans by Purpose Report";
                    rptData = await _loanService.GetLoanPortfolioAnalysisAsync(reportCommand);
                    if (rptData == null)
                        return new JsonResult
                        {
                            Data = new { success = false, message = "No data found." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    break;

                default:
                    relativePath = "Loan/PortFolio/MainPortFolioRPT.rpt";
                    reportTitle = "Loan Portfolio Analysis Report";
                    break;
            }

            DateTime.TryParse(reportCommand.StartDate, out var sDate);
            DateTime.TryParse(reportCommand.EndDate, out var eDate);

            var parameters = new Dictionary<string, object>
            {
                { "DateFrom", sDate.ToString("dd/MM/yyyy") },
                { "DateTo", eDate.ToString("dd/MM/yyyy") },
                { "BranchName", reportType == "delinquentloansummary" ? delinquencyData.BranchName : rptData.BranchName },
                { "CurrentYear", DateTime.Now.Year.ToString() },
                { "PrintedBy", _controller.Session["FullName"]?.ToString() ?? "System" },
                { "ReportTitle", reportTitle },
                { "PrintedOn", DateTime.Now.ToString("dd/MM/yyyy, hh:mm:ss") }
            };

            var subReports = new Dictionary<string, object>();
            if (reportType == "all")
            {
                var selectedReports = string.IsNullOrWhiteSpace(reportCommand.SubReportType)
                    ? new List<string> { "All" }
                    : reportCommand.SubReportType.Split(',').Select(s => s.Trim()).ToList();

                bool ShouldInclude(string name) => selectedReports.Contains("All", StringComparer.OrdinalIgnoreCase)
                    || selectedReports.Contains(name, StringComparer.OrdinalIgnoreCase);

                subReports = new Dictionary<string, object>
                {
                    { "Flat_AgeAndGenderSubReport", ShouldInclude("Flat_AgeAndGender") ? rptData.Flat_AgeAndGender : null },
                    { "Flat_AgeAndLoanTypeSubReport", ShouldInclude("Flat_AgeAndLoanType") ? rptData.Flat_AgeAndLoanType : null },
                    { "Flat_ByLoanTermSubReport", ShouldInclude("Flat_ByLoanTerm") ? rptData.Flat_ByLoanTerm : null },
                    { "Flat_ByTargetGroupSubReport", ShouldInclude("Flat_ByTargetGroup") ? rptData.Flat_ByTargetGroup : null },
                    { "Flat_ByCategorySubReport", ShouldInclude("Flat_ByCategory") ? rptData.Flat_ByCategory : null },
                    { "Flat_ByZoneSubReport", ShouldInclude("Flat_ByZone") ? rptData.Flat_ByZone : null },
                    { "PortfolioDetailsSubReport", ShouldInclude("PortfolioDetails") ? rptData.PortfolioDetails : null }
                };
            }
            else if (reportType == "delinquentloansummary")
            {
                subReports = new Dictionary<string, object>
                {
                    { "SubDelinquencyByGenderSummaryRPT", delinquencyData.GenderSummariesAfter60Days },
                    { "SubDelinquencyByLoanTypeSummaryRPT", delinquencyData.LoanTypeSummariesAfter60Days },
                    { "SubLoanDelinquencyCategorySummaryRPT", delinquencyData.CategorySummaries }
                };
                _controller.Session["MainData"] = new List<LoanDelinquencyReportDto> { delinquencyData };
            }
            else if (reportType == "loansituations")
            {
                _controller.Session["MainData"] = delinquencyData.LoanEntries;
            }
            else if (reportType == "delinquentloanslistingwithaging")
            {
                _controller.Session["MainData"] =delinquencyData.LoanEntries;
            }
            // delinquentloanslistingwithaging
            else
            {
                if (reportType == "delinquentloan")
                    _controller.Session["MainData"] = rptData.PortfolioDetails;
                else
                    _controller.Session["MainData"] = new List<LoanDelinquencyReportResultRPT> { rptData };

            }

            _controller.Session["ReportParameters"] = parameters;
            _controller.Session["SubReportsData"] = subReports;

            string viewerUrl = _controller.Url.Content($"/ReportForm/ReportViewer.aspx?reportPath={HttpUtility.UrlEncode(relativePath)}&reportName={HttpUtility.UrlEncode(reportTitle)}");

            return new JsonResult
            {
                Data = new { success = true, redirectUrl = viewerUrl },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }

}