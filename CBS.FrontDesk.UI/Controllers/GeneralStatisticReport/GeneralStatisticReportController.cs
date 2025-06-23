using BusinessServices;
using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Accounts.GeneralStatisticReport;
using CBS.BusinessService.AuditTrailP;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.GeneralStatisticReport;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.FrontDesk.UI.Helper;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.GeneralStatisticReport
{
    //[CheckSessionTimeOutAttribute]

    public class GeneralStatisticReportController : BaseController
    {
        private readonly GeneralStatisticReportServices _loanServices;
        private readonly BranchServices _branchServices;

        public GeneralStatisticReportController(GeneralStatisticReportServices loanServices, BranchServices branchServices)
        {
            _loanServices = loanServices;
            _branchServices = branchServices;
        }

        // GET: GeneralStatisticReport
        public async Task<ActionResult> Index()
        {
            var branches = await _branchServices.GetBranches();

            ViewBag.Branches = branches.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.Name
            }).ToList();

            ViewBag.AccountTypes = Enum.GetNames(typeof(AccountType)).Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();

            ViewBag.AccountProfiles = AccountProfileTypes.All.Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();

            return View();
        }
        public async Task<ActionResult> Dashboard()
        {
            var branches = await _branchServices.GetBranches();

            ViewBag.Branches = branches.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.Name
            }).ToList();

            ViewBag.AccountTypes = Enum.GetNames(typeof(AccountType)).Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();

            ViewBag.AccountProfiles = AccountProfileTypes.All.Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();

            return View(new GeneralStatisticsDashboard());
        }

        [HttpPost]
        public async Task<ActionResult> Dashboard(GenerateGeneralStatisticsReportQuery reportCommand)
        {
            var report = await _loanServices.GetGeneralStatisticsReportAsync(reportCommand);
            if (report == null)
                return Content(""); // return empty HTML so JS can hide

            var dashboard = new GeneralStatisticsDashboard
            {
                BranchName = report.BranchName,
                DateRange = $"{reportCommand.StartDate:dd/MM/yyyy} - {reportCommand.EndDate:dd/MM/yyyy}",
                MembersWithAccounts = report.MembersWithAccounts,
                AccountTypeSummaries = report.AccountTypeSummaries,
                LoanSummaries = report.LoanSummaries,
                LoanBreakdowns = report.LoanBreakdowns,
                MembersDetails = report.MembersDetails,
                MissingCustomers = report.MissingCustomers,
                CustomerGenderSummary = report.CustomerGenderSummary
            };

            return PartialView("_Dashboard", dashboard);
        }


        [HttpPost]
        public async Task<ActionResult> GeneralReport(GenerateGeneralStatisticsReportQuery reportCommand)
        {
            try
            {
                var generalStatisticsReport = await _loanServices.GetGeneralStatisticsReportAsync(reportCommand);
                if (generalStatisticsReport == null)
                    return Json(new { success = false, message = "No data found." });

                string mainReportType = (reportCommand.MainReportType ?? "generalstatistics").ToLowerInvariant();

                string relativeReportPath;
                string reportTitle;

                switch (mainReportType)
                {
                    case "generalmembersreport":
                        relativeReportPath = "Reports/Members/CurrentLoanRPT.rpt";
                        reportTitle = "📌 General Member's Statistical Report";
                        break;

                    case "generalmemberslisting":
                        relativeReportPath = "Reports/Members/MembersListMAINRPT.rpt";
                        reportTitle = "📋 Approved Members Listing";
                        break;

                    case "customerslistingwithoutaccount":
                        relativeReportPath = "Reports/Members/MissingAccountsMAINRPT.rpt";
                        reportTitle = "❗ Members Without Active Accounts";
                        break;

                    case "customergendersummary":
                        relativeReportPath = "Reports/Members/GenderDistributionMAINRPT.rpt";
                        reportTitle = "📊 Gender Distribution Summary Report";
                        break;

                    case "general":
                    case "generalstatistics":
                        relativeReportPath = "Reports/Members/GeneralStatisticsMAINRPT.rpt";
                        reportTitle = "📈 General Member & Account Statistics Report";
                        break;

                    default:
                        relativeReportPath = "Reports/Members/GeneralStatisticsMAINRPT.rpt";
                        reportTitle = "📊 General Membership Report";
                        break;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "DateFrom", reportCommand.StartDate.ToString("dd/MM/yyyy") },
                    { "DateTo", reportCommand.EndDate.ToString("dd/MM/yyyy") },
                    { "BranchName", generalStatisticsReport.BranchName },
                    { "CurrentYear", DateTime.Now.Year.ToString() },
                    { "PrintedBy", Session["FullName"]?.ToString() ?? "System" },
                    { "ReportTitle", reportTitle },
                    { "PrintedOn", DateTime.Now.ToString("dd/MM/yyyy, hh:mm:ss") }
                };

                Dictionary<string, object> subReportData = new Dictionary<string, object>();

                if (mainReportType == "general" || mainReportType =="generalstatistics")
                {
                    subReportData = new Dictionary<string, object>
                {
                    { "SubMembersWithAccountsRPT", generalStatisticsReport.MembersWithAccounts },
                    { "SubAccountTypeSummariesRPT", generalStatisticsReport.AccountTypeSummaries },
                    { "SubLoanSummariesRPT", generalStatisticsReport.LoanSummaries },
                    { "SubLoanBreakdowns", generalStatisticsReport.LoanBreakdowns }
                };

                    Session["MainData"] = new List<GeneralStatisticsReportRPT> { generalStatisticsReport };
                    Session["SubReportsData"] = subReportData;
                }
                else
                {
                    switch (mainReportType)
                    {
                        case "generalmemberslisting":
                            Session["MainData"] = generalStatisticsReport.MembersDetails;
                            break;

                        case "customerslistingwithoutaccount":
                            Session["MainData"] = generalStatisticsReport.MissingCustomers;
                            break;

                        case "customergendersummary":
                            Session["MainData"] = new List<GenderDistributionSummary> { generalStatisticsReport.CustomerGenderSummary };
                            break;

                        case "generalmembersreport":
                            Session["MainData"] = generalStatisticsReport.MembersDetails;
                            break;

                        default:
                            return Json(new { success = false, message = "No suitable data found for this report type." });
                    }

                    Session.Remove("SubReportsData");
                }

                Session["ReportParameters"] = parameters;

                string viewerUrl = Url.Content($"/ReportForm/ReportViewer.aspx?reportPath={HttpUtility.UrlEncode(relativeReportPath)}&reportName={HttpUtility.UrlEncode(reportTitle)}");
                return Json(new { success = true, redirectUrl = viewerUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while generating the report: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> LoanPortfolioReport()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            return View();
        }
    }
}