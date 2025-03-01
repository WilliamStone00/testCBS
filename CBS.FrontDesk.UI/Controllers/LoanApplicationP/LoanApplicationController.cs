using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Application;
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
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.LoanApplicationP
{
    [CheckSessionTimeOutAttribute]

    public class LoanApplicationController : BaseController
    {
        // GET: Loan

        private readonly LoanApplicationServices _LoanServices;
        private readonly LoanCommiteeMemberServices _loanCommiteeMember;
        private readonly UserManagementServices _userManagementServices;
        private readonly BranchServices _branchServices;
        public LoanApplicationController(LoanApplicationServices LoanServices, LoanCommiteeMemberServices loanCommiteeMember, UserManagementServices userManagementServices, BranchServices branchServices = null)
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
        public async Task<ActionResult> Details(string KEY = null)
        {
            var loan = await _LoanServices.GetLoanWithCustomerAndBranch(KEY);
            return View(loan);
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
                var getLoansDataTableQuery = new GetLoanApplicationsDataTableQuery
                {
                    DataTableOptions = new DataTableOptions
                    {
                        pageSize = 30000,  // Export large number of records
                        start = 0,
                        searchValue = searchCriteria
                    },
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MinValue,
                    BranchId = branchid,
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
                var loans = JsonConvert.DeserializeObject<List<Loan>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                string exportedBy = Session["FullName"].ToString();

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

                var query = new GetLoanApplicationsDataTableQuery
                {
                    DataTableOptions = PostDataTableOptions(),
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MinValue,
                    BranchId = branchid,
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
    }
}