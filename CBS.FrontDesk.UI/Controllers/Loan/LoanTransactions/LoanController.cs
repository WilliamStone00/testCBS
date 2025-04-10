using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.AuditTrailP;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.FrontDesk.UI.Helper;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.LoanTransactions
{
    //[CheckSessionTimeOutAttribute]

    public class LoanController : BaseController
    {
        // GET: Loan

        private readonly LoanServices _LoanServices;
        private readonly LoanCommiteeMemberServices _loanCommiteeMember;
        private readonly UserManagementServices _userManagementServices;
        private readonly BranchServices _branchServices;
        public LoanController(LoanServices LoanServices, LoanCommiteeMemberServices loanCommiteeMember, UserManagementServices userManagementServices, BranchServices branchServices = null)
        {
            _LoanServices = LoanServices;
            _loanCommiteeMember = loanCommiteeMember;
            _userManagementServices = userManagementServices;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View(new Loan { InitiateLoanDownloadCommand=new InitiateLoanDownloadCommand()});
        }
        public async Task<ActionResult> Configuration()
        {

            return View();
        }
        //
        public async Task<ActionResult> Details(string KEY = null)
        {
            var loan = await _LoanServices.GetLoanWithCustomerAndBranch(KEY);
            return View(loan);
        }
       

        // Action to handle file download
        public async Task<ActionResult> DownloadFile(string fileId = null)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                var downloadInfoLoans = await _LoanServices.GetAllFileDownloadInfoLoanPerUser();
                var Branches = await _branchServices.GetBranches();
                ViewBag.Branches = Branches;
                return View(new Loan { FileDownloadInfoLoans = downloadInfoLoans.ToList() });
            }

            try
            {
                // Call the service to download the file
                var response = await _LoanServices.DownloadFile(fileId);

                if (response != null)
                {
                    // If response is successful, return the file
                    return File(response.FileData, response.ContentType, response.FileName);
                }
                else
                {
                    // If the response is null or contains errors, return an error view

                    return View("Error", new HandleErrorInfo(new Exception(response.ErrorMessage), "ControllerName", "ActionName"));
                }
            }
            catch (Exception ex)
            {
                // Handle exception and return an error view
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return View("Error", new HandleErrorInfo(ex, "ControllerName", "ActionName"));
            }
        }

        public async Task<ActionResult> LoanReportGeneration()
        {
            var downloadInfoLoans = await _LoanServices.GetAllFileDownloadInfoLoanPerUser();
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View(new Loan { FileDownloadInfoLoans= downloadInfoLoans.ToList() });
        }
        [HttpPost]
        public async Task<ActionResult> DownloadFile(InitiateLoanDownloadCommand initiateLoanDownloadCommand)
        {
            var data = await _LoanServices.InitiateBulkDownloadLoansBranch(initiateLoanDownloadCommand);
            ViewBag.Message = Messaging.MessageResult(data);
            ViewBag.Status = data.Result;
            var downloadInfoLoans = await _LoanServices.GetAllFileDownloadInfoLoanPerUser();
            var Branches = await _branchServices.GetBranches();

            ViewBag.Branches = Branches;
            return View(new Loan { FileDownloadInfoLoans = downloadInfoLoans.ToList() });
        }
        [HttpGet]
        public async Task<ActionResult> Download(
            string searchCriteria = "all",
            string dateFrom = null,
            string dateTo = null,
            string status = "Open",
            string deliquentstatus = "Current",
            string branchid = null,
            string exportReportType = "Loan Query")
        {
            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;
                var Branch = new Branch();

                // Parse date strings if provided
                if (!string.IsNullOrWhiteSpace(dateFrom))
                {
                    startDate = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", null);
                }

                if (!string.IsNullOrWhiteSpace(dateTo))
                {
                    endDate = DateTime.ParseExact(dateTo, "dd/MM/yyyy", null).AddDays(1).AddTicks(-1);  // Include the whole day
                }


                if (!string.IsNullOrWhiteSpace(branchid))
                {
                    var branch = await _branchServices.GetBranch(branchid);
                    Branch = branch;
                }
                if (!_branchServices.IsHeadOffice())
                {
                    var branch = await _branchServices.GetBranch(_branchServices.GetBranchID());
                    Branch = branch;
                }
                // Prepare the DataTable query
                var getLoansDataTableQuery = new GetLoansDataTableQuery
                {
                    DataTableOptions = new DataTableOptions
                    {
                        pageSize = 10000,  // Export large number of records
                        start = 0,
                        searchValue = searchCriteria
                    },
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MinValue,
                    BranchId = branchid,
                    DeliquentStatus = deliquentstatus,
                    Status = status,
                    MemberId = "n/a",
                };

                getLoansDataTableQuery.DataTableOptions = GetDataTableOptions();

                if (string.IsNullOrWhiteSpace(searchCriteria))
                {
                    searchCriteria = "all";
                }

                getLoansDataTableQuery.DataTableOptions.pageSize = 30000;
                getLoansDataTableQuery.DataTableOptions.start = 0;

                // Fetch the data
                var dataTable = await _LoanServices.GetDataTableAsync(getLoansDataTableQuery, searchCriteria);

                // Convert dataTable.data to List<Loan>
                var loans1 = JsonConvert.DeserializeObject<List<Loan>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                
                string exportedBy = Session["FullName"].ToString();
                var branches = await _branchServices.GetBranches();
                var loans=_LoanServices.MapLoansWithBranchDetails(branches.ToList(), loans1);

                // Generate Excel file
                //var exportFile = new ExportFileResult();
                // Filter loans based on the selected `exportReportType`
                switch (exportReportType)
                {
                    case "Approved_Loans":
                        var exportFilea = LoanExcelGenerator.GenerateLoanExcel(loans, Branch, exportedBy, fileTitle: exportReportType, dateFrom, dateTo);
                        // Return file to client for download
                        return File(exportFilea.Content, exportFilea.ContentType, exportFilea.FileName);
                    case "Paid_Loans":
                        var exportFilep = LoanExcelGenerator.GenerateLoanExcel(loans, Branch, exportedBy, fileTitle: exportReportType, dateFrom, dateTo);
                        // Return file to client for download
                        return File(exportFilep.Content, exportFilep.ContentType, exportFilep.FileName);
                    case "Delinquent_Loans":
                        var exportFiled = LoanExcelGenerator.GenerateLoanExcel(loans, Branch, exportedBy, fileTitle: exportReportType, dateFrom, dateTo);
                        // Return file to client for download
                        return File(exportFiled.Content, exportFiled.ContentType, exportFiled.FileName);
                    case "Current_Loans":
                        var exportFilec = LoanExcelGenerator.GenerateLoanExcel(loans, Branch, exportedBy, fileTitle: exportReportType, dateFrom, dateTo);
                        // Return file to client for download
                        return File(exportFilec.Content, exportFilec.ContentType, exportFilec.FileName);
                    case "All_Loans":
                        var exportFile=LoanExcelGenerator.GenerateLoanExcel(loans, Branch, exportedBy, fileTitle: exportReportType, dateFrom, dateTo);
                        // Return file to client for download
                        return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
                       
                    default:
                        var exportFiler = LoanExcelGenerator.GenerateLoanExcel(loans, Branch, exportedBy, fileTitle: exportReportType, dateFrom, dateTo);
                        // Return file to client for download
                        return File(exportFiler.Content, exportFiler.ContentType, exportFiler.FileName);
                }

               

               
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting data.");
            }
        }


        [HttpPost]
        public async Task<ActionResult> LoadLoanData(string searchCriteria = "all", string dateFrom = null, string dateTo = null, string status = "Open", string deliquentstatus = "Current", string branchid = null)
        {
            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom))
                {
                    startDate = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", null);
                }

                if (!string.IsNullOrWhiteSpace(dateTo))
                {
                    endDate = DateTime.ParseExact(dateTo, "dd/MM/yyyy", null).AddDays(1).AddTicks(-1);
                }

                var query = new GetLoansDataTableQuery
                {
                    DataTableOptions = PostDataTableOptions(),
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MinValue,
                    BranchId = branchid,
                    DeliquentStatus = deliquentstatus,
                    Status = status, MemberId="n/a",
                };

                var dataTable = await _LoanServices.GetDataTableAsync(query, searchCriteria);
                var loanList = JsonConvert.DeserializeObject<List<Loan>>(JsonConvert.SerializeObject(dataTable.data));

                return Json(new
                {
                    draw = query.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = loanList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading loan data.");
            }
        }

        //
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)

        {

            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY, serviceOption);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }

            return HttpNotFound(); // Or return a default view for handling unknown paths
        }


        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key, string serviceOption)

        {
            if (serviceOption == "Loan")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        //var data = await auditTrailServices.GetLoans();
                        var sysData = new MemberOperationPanel { Loans = null };
                        return PartialView(partialView, sysData);
                    };
                }

            }
            else if (serviceOption == "LoanCommiteeMember")
            {
                //if (path == "list")
                //{
                //    return async () =>
                //    {
                //        var data = await _loanCommiteeMember.GetLoanCommiteeMembers();
                //        var sysData = new LoanCommitee { LoanCommiteeMembers = data.ToList() };
                //        return PartialView(partialView, sysData);
                //    };
                //}
                //else if (path == "new")
                //{
                //    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = new LoanCommiteeMember() });
                //}
                //else
                //{
                //    ViewBag.Key = key;
                //    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = await _loanCommiteeMember.GetLoanCommiteeMember(key) });
                //}

            }

            return null;
        }

        public async Task<bool> GetList()
        {
            //ViewBag.Groups = await auditTrailServices.GetLoans();
            var users = await _userManagementServices.GetUserDropDownList();
            ViewBag.Users = users.ToList();
            return true;
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _LoanServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> LoanPortfolioStatistics(GenerateLoanPortfolioReportCommand reportCommand)
        {
            try
            {
                // Use beginning of month to today if not specified
                var endDate = reportCommand.EndDate == default ? DateTime.Today : reportCommand.EndDate;
                var startDate = reportCommand.StartDate == default ? new DateTime(endDate.Year, endDate.Month, 1) : reportCommand.StartDate;

                reportCommand.StartDate = startDate;
                reportCommand.EndDate = endDate;

                var loanPortfolioAnalysis = await _LoanServices.GetLoanPortfolioAnalysisAsync(reportCommand);
                if (loanPortfolioAnalysis == null)
                    return Json(new { success = false, message = "No data was found." });

                string mainReportType = (reportCommand.MainReportType ?? "All").ToLowerInvariant();
                string reportFilePath;
                switch (mainReportType)
                {
                    case "currentloan":
                        reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/CurrentLoanRPT.rpt";
                        break;
                    case "delinquentloan":
                        reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/DelinquentLoanRPT.rpt";
                        break;
                    case "loanbypurpose":
                        reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/LoanByPurposeRPT.rpt";
                        break;
                    default:
                        reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/MainPortFolioRPT.rpt";
                        break;
                }

                var report = new ReportDocument();
                report.Load(Server.MapPath(reportFilePath));

                // Indexable report source
                var reportData = new List<LoanDelinquencyReportResultRPT> { loanPortfolioAnalysis };
                report.SetDataSource(reportData);

               
                // Only load subreports if the main report is the full one
                if (mainReportType == "all" && reportData.Count > 0)
                {
                    var selectedReports = string.IsNullOrWhiteSpace(reportCommand.SubReportType)
                        ? new List<string> { "All" }
                        : reportCommand.SubReportType.Split(',').Select(s => s.Trim()).ToList();

                    bool ShouldInclude(string name) =>
                        selectedReports.Contains("All", StringComparer.OrdinalIgnoreCase) ||
                        selectedReports.Contains(name, StringComparer.OrdinalIgnoreCase);

                    var main = reportData[0];

                    foreach (ReportDocument subReport in report.Subreports)
                    {
                        switch (subReport.Name)
                        {
                            case "Flat_AgeAndGenderSubReport.rpt" when ShouldInclude("Flat_AgeAndGender"):
                                subReport.SetDataSource(main.Flat_AgeAndGender);
                                break;
                            case "Flat_AgeAndLoanTypeSubReport.rpt" when ShouldInclude("Flat_AgeAndLoanType"):
                                subReport.SetDataSource(main.Flat_AgeAndLoanType);
                                break;
                            case "Flat_ByLoanTermSubReport.rpt" when ShouldInclude("Flat_ByLoanTerm"):
                                subReport.SetDataSource(main.Flat_ByLoanTerm);
                                break;
                            case "Flat_ByTargetGroupSubReport.rpt" when ShouldInclude("Flat_ByTargetGroup"):
                                subReport.SetDataSource(main.Flat_ByTargetGroup);
                                break;
                            case "Flat_ByCategorySubReport.rpt" when ShouldInclude("Flat_ByCategory"):
                                subReport.SetDataSource(main.Flat_ByCategory);
                                break;
                            case "Flat_ByZoneSubReport.rpt" when ShouldInclude("Flat_ByZone"):
                                subReport.SetDataSource(main.Flat_ByZone);
                                break;
                            case "PortfolioDetailsSubReport.rpt" when ShouldInclude("PortfolioDetails"):
                                subReport.SetDataSource(main.PortfolioDetails);
                                break;
                        }
                    }

                    // Set parameters for the main report only
                    foreach (ParameterField param in report.ParameterFields)
                    {
                        switch (param.Name)
                        {
                            case "DateFrom":
                                report.SetParameterValue("DateFrom", startDate);
                                break;
                            case "DateTo":
                                report.SetParameterValue("DateTo", endDate);
                                break;
                            case "BranchName":
                                report.SetParameterValue("BranchName", loanPortfolioAnalysis.BranchName);
                                break;
                            case "PrintedBy":
                                report.SetParameterValue("PrintedBy", Session["FullName"].ToString());
                                break;
                            case "ReportTitle":
                                report.SetParameterValue("ReportTitle", $"Loan Delinquency Repor");
                                break;
                        }
                    }

                }

                // Export logic
                var exportType = (reportCommand.ReportDownloadType ?? "PDF").ToLowerInvariant();
                ExportFormatType crystalExportType;
                string contentType, extension;

                switch (exportType)
                {
                    case "excel":
                        crystalExportType = ExportFormatType.Excel;
                        contentType = "application/vnd.ms-excel";
                        extension = "xls";
                        break;
                    case "word":
                        crystalExportType = ExportFormatType.WordForWindows;
                        contentType = "application/msword";
                        extension = "doc";
                        break;
                    default:
                        crystalExportType = ExportFormatType.PortableDocFormat;
                        contentType = "application/pdf";
                        extension = "pdf";
                        break;
                }

                var fileName = $"LoanPortfolioReport_{DateTime.Now:yyyyMMddHHmmss}.{extension}";
                var stream = report.ExportToStream(crystalExportType);
                stream.Seek(0, SeekOrigin.Begin);

                Response.AppendHeader("Content-Disposition", $"attachment; filename={fileName}");
                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        //public async Task<ActionResult> LoanPortfolioStatistics(GenerateLoanPortfolioReportCommand reportCommand)
        //{
        //    try
        //    {
        //        // Use beginning of month to today if not specified
        //        var endDate = reportCommand.EndDate == default ? DateTime.Today : reportCommand.EndDate;
        //        var startDate = reportCommand.StartDate == default ? new DateTime(endDate.Year, endDate.Month, 1) : reportCommand.StartDate;

        //        reportCommand.StartDate = startDate;
        //        reportCommand.EndDate = endDate;

        //        var loanPortfolioAnalysis = await _LoanServices.GetLoanPortfolioAnalysisAsync(reportCommand);
        //        if (loanPortfolioAnalysis == null)
        //            return Json(new { success = false, message = "No data was found." });

        //        string mainReportType = (reportCommand.MainReportType ?? "All").ToLowerInvariant();
        //        string reportFilePath;
        //        switch (mainReportType)
        //        {
        //            case "currentloan":
        //                reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/CurrentLoanRPT.rpt";
        //                break;
        //            case "delinquentloan":
        //                reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/DelinquentLoanRPT.rpt";
        //                break;
        //            case "loanbypurpose":
        //                reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/LoanByPurposeRPT.rpt";
        //                break;
        //            default:
        //                reportFilePath = "~/AppFiles/Reporting/Loan/PortFolio/MainPortFolioRPT.rpt";
        //                break;
        //        }

        //        ReportDocument report = new ReportDocument();
        //        report.Load(Server.MapPath(reportFilePath));
        //        report.SetDataSource(new List<LoanPortfolioAnalysis> { loanPortfolioAnalysis });

        //        // Set parameters on main report only (they apply to subreports via linking)
        //        foreach (ParameterField param in report.ParameterFields)
        //        {
        //            switch (param.Name)
        //            {
        //                case "StartDate":
        //                    report.SetParameterValue("StartDate", startDate); break;
        //                case "EndDate":
        //                    report.SetParameterValue("EndDate", endDate); break;
        //                case "BranchName":
        //                    report.SetParameterValue("BranchName", loanPortfolioAnalysis.BranchName); break;
        //                case "PrintedBy":
        //                    report.SetParameterValue("PrintedBy", User.Identity?.Name ?? "System"); break;
        //                case "ReportTitle":
        //                    report.SetParameterValue("ReportTitle", "Loan Portfolio Report"); break;
        //            }
        //        }

        //        if (mainReportType == "all")
        //        {
        //            var selectedReports = string.IsNullOrWhiteSpace(reportCommand.SubReportType)
        //                ? new List<string> { "All" }
        //                : reportCommand.SubReportType.Split(',').Select(s => s.Trim()).ToList();

        //            bool ShouldInclude(string name) =>
        //                selectedReports.Contains("All", StringComparer.OrdinalIgnoreCase) ||
        //                selectedReports.Contains(name, StringComparer.OrdinalIgnoreCase);

        //            foreach (ReportDocument subReport in report.Subreports)
        //            {
        //                switch (subReport.Name)
        //                {
        //                    case "DeliquencyLoanAgingPortFolioRPTSub.rpt" when ShouldInclude("AgingAnalysis"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.AgingAnalysis); 
        //                        break;
        //                    case "GenderAgingAnalysisSubReport" when ShouldInclude("GenderAgingAnalysis"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.GenderAgingAnalysis); break;
        //                    case "GroupDelinquencySubReport" when ShouldInclude("GroupDelinquency"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.GroupDelinquency); break;
        //                    case "IndividualDelinquencySubReport" when ShouldInclude("IndividualDelinquency"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.IndividualDelinquency); break;
        //                    case "LoanTypeDelinquencySubReport" when ShouldInclude("LoanTypeDelinquency"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.LoanTypeDelinquency); break;
        //                    case "MemberAgeDelinquencySubReport" when ShouldInclude("MemberAgeDelinquency"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.MemberAgeDelinquency); break;
        //                    case "LoanPortfoliosSubReport" when ShouldInclude("LoanPortfolios"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.LoanPortfolios); break;
        //                    case "LoanTargetGenderAnalysisSubReport" when ShouldInclude("LoanTargetGenderAnalysis"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.LoanTargetGenderAnalysis); break;
        //                    case "LoanProductTypeTargetGenderAnalysisSubReport" when ShouldInclude("LoanProductTypeTargetGenderAnalysis"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.LoanProductTypeTargetGenderAnalysis); break;
        //                    case "LoanTermProductTargetGenderAnalysisSubReport" when ShouldInclude("LoanTermProductTargetGenderAnalysis"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.LoanTermProductTargetGenderAnalysis); break;
        //                    case "LoanCategoryTermProductTargetGenderAnalysisSubReport" when ShouldInclude("LoanCategoryTermProductTargetGenderAnalysis"):
        //                        subReport.SetDataSource(loanPortfolioAnalysis.LoanCategoryTermProductTargetGenderAnalysis); break;
        //                }
        //            }
        //        }

        //        var exportType = (reportCommand.ReportDownloadType ?? "PDF").ToLowerInvariant();
        //        ExportFormatType crystalExportType;
        //        string contentType, extension;

        //        switch (exportType)
        //        {
        //            case "excel":
        //                crystalExportType = ExportFormatType.Excel;
        //                contentType = "application/vnd.ms-excel";
        //                extension = "xls";
        //                break;
        //            case "word":
        //                crystalExportType = ExportFormatType.WordForWindows;
        //                contentType = "application/msword";
        //                extension = "doc";
        //                break;
        //            default:
        //                crystalExportType = ExportFormatType.PortableDocFormat;
        //                contentType = "application/pdf";
        //                extension = "pdf";
        //                break;
        //        }

        //        var fileName = $"LoanPortfolioReport_{DateTime.Now:yyyyMMddHHmmss}.{extension}";
        //        var stream = report.ExportToStream(crystalExportType);
        //        stream.Seek(0, SeekOrigin.Begin);

        //        Response.AppendHeader("Content-Disposition", $"attachment; filename={fileName}");
        //        return File(stream, contentType);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"Error: {ex.Message}" });
        //    }
        //}




        //[HttpPost]
        //public async Task<ActionResult> LoanPortfolioStatistics(GenerateLoanPortfolioReportCommand reportCommand)
        //{

        //    var loanPortfolioAnalysis = await _LoanServices.GetLoanPortfolioAnalysisAsync(reportCommand);
        //    if (loanPortfolioAnalysis != null)
        //    {

        //        var analysis = GetLoanPortfolioAnalysis(); // your populated object
        //        var dataset = GetLoanPortfolioReportData(analysis);

        //        var report = new ReportDocument();
        //        report.Load(Server.MapPath("~/Reports/LoanPortfolioReport.rpt"));
        //        report.SetDataSource(dataset);

        //        CrystalReportViewer1.ReportSource = report;
        //        CrystalReportViewer1.DataBind();


        //        this.HttpContext.Session["rptSource"] = loanPortfolioAnalysis;
        //        return Json(new { success = true, message = "OK" });

        //    }
        //    return Json(new { success = false, message = $"No data was found." });
        //}
        [HttpGet]
        public async Task<ActionResult> LoanPortfolioReport()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
    }
}