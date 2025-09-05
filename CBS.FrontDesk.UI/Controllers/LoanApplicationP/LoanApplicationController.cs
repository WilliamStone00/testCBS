using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Application;
using CBS.BusinessService.AuditTrailP;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.LoanApplicationP
{
    //[CheckSessionTimeOutAttribute]

    public class LoanApplicationController : BaseController
    {
        // GET: Loan

        private readonly LoanApplicationServices _LoanServices;
        private readonly LoanCommiteeMemberServices _loanCommiteeMember;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly LoanProductServices _loanProductServices;
        private readonly LoanPurposeServices _loanPurposeServices;
        private readonly LoanTermServices _loanTermServices;

        public LoanApplicationController(LoanApplicationServices LoanServices, LoanCommiteeMemberServices loanCommiteeMember, BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null, LoanProductServices loanProductServices = null, LoanPurposeServices loanPurposeServices = null, LoanTermServices loanTermServices = null)
        {
            _LoanServices = LoanServices;
            _loanCommiteeMember = loanCommiteeMember;
            _branchServices = branchServices;
            _individualProfileServices=individualProfileServices;
            _loanProductServices=loanProductServices;
            _loanPurposeServices=loanPurposeServices;
            _loanTermServices=loanTermServices;
        }
        //EditLoanApplication
        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View(new LoanApplication());
        }
        //
        [HttpPost]
        public async Task<ActionResult> UpdateLoanApplication(UpdateLoanApplicationCommand model)
        {
            
                    if (!ModelState.IsValid)
                    {
                        return JsonValidationErrorResponse();

                    }
                   
                    var data = await _LoanServices.Update(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> EditLoanApplication(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            var loanApplication = await _LoanServices.GetLoanWithCustomerAndBranch(KEY);
            if (loanApplication == null)
            {
                return View("NotFound"); // Show a Not Found view if loan is null
            }
            ViewBag.KEY = KEY;
            //var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            //ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;
            var customer = await InitializeCustomerData(loanApplication.CustomerId);
            ViewBag.LoanProducts = await _loanProductServices.GetLoanProductsDropDown();
            var fees = await _loanProductServices.GetFees();
            ViewBag.LoanFees = new MultiSelectList(
        fees,
        "value", // Replace with the property name for the value (e.g., ID)
        "Text",  // Replace with the property name for the display text (e.g., Name)
        selectedValues: null // Optionally, pass a list of selected values
    );
            ViewBag.MembersLoan = ViewBag.LoanFees;
            ViewBag.KEY = KEY;
            //await PopulateAggregatesInViewBag();
            var LoanApplicationToCommand = _LoanServices.MapLoanApplicationToCommand(loanApplication);
            LoanApplicationToCommand.Customer=customer.CustomerList;
            //var customer = await InitializeCustomerData(KEY);
            return View(LoanApplicationToCommand);


            //var CustomerLoans = await _loanservices.GetLoanByCustomerID(KEY);
        }

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {

            var results = await _individualProfileServices.GetCustomerLight(KEY);
            return results;
        }
        [HttpGet]
        public async Task<ActionResult> Details(string KEY = null)
        {
            if (string.IsNullOrEmpty(KEY))
            {
                return RedirectToAction("Index"); // Redirect to list page if KEY is not provided
            }

            var loanApplication = await _LoanServices.GetLoanWithCustomerAndBranch(KEY);
            if (loanApplication == null)
            {
                return View("NotFound"); // Show a Not Found view if loan is null
            }
            var customer = await InitializeCustomerData(loanApplication.CustomerId);
            loanApplication.Customer=customer;
            return View(loanApplication);
        }


        [HttpGet]
        public async Task<ActionResult> Download(
            string searchCriteria = "all",
            string dateFrom = null,
            string dateTo = null,
            string status = "Open",
            string deliquentStatus = "Current",
            string branchId = null,
            string loanCategory = "all",
            string loanTarget = "all",
            string approvalStatus = "all")
        {
            try
            {
           
                Branch branch = null;

                // Date Parsing with Improved Error Handling
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParseExact(dateFrom, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate))
                {
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParseExact(dateTo, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
                {
                    endDate = parsedEndDate.AddDays(1).AddTicks(-1); // Set end of the day
                }

                // ✅ Retrieve branch details efficiently
                if (!string.IsNullOrWhiteSpace(branchId))
                {
                    branch = await _branchServices.GetBranch(branchId);
                }
                else if (!_branchServices.IsHeadOffice())
                {
                    branch = await _branchServices.GetBranch(_branchServices.GetBranchID());
                }

                // Ensure valid branch
                branch = new Branch();

                // ✅ Construct query object
                var getLoansDataTableQuery = new GetLoanApplicationsDataTableQuery
                {
                    DataTableOptions = new DataTableOptions
                    {
                        pageSize = 30000, // Export a large number of records
                        start = 0,
                        searchValue = !string.IsNullOrWhiteSpace(searchCriteria) ? searchCriteria : "all"
                    },
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue,
                    BranchId = !string.IsNullOrWhiteSpace(branchId) ? branchId : "all",
                    Status = status,
                    MemberId = "n/a",
                    LoanCategory = loanCategory,
                    LoanTarget = loanTarget,
                    ApprovalStatus = approvalStatus,
                };
                getLoansDataTableQuery.DataTableOptions = GetDataTableOptions();

                //if (string.IsNullOrWhiteSpace(searchCriteria))
                //{
                //    searchCriteria = "all";
                //}

                getLoansDataTableQuery.DataTableOptions.pageSize = 30000;
                getLoansDataTableQuery.DataTableOptions.start = 0;

                // ✅ Fetch loan data
                var dataTable = await _LoanServices.GetDataTableAsync(getLoansDataTableQuery, searchCriteria);
                var loans = JsonConvert.DeserializeObject<List<LoanApplication>>(JsonConvert.SerializeObject(dataTable.data));

                if (loans == null || loans.Count == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NoContent, "No data available for export.");
                }

                // ✅ GetAllowAnonymous exported by user
                string exportedBy = Session["FullName"]?.ToString() ?? "Unknown";

                // ✅ Generate Excel file
                var exportFile = LoanApplicationExcelGenerator.GenerateLoanApplicationExcel(
                    loans,
                    branch,
                    exportedBy,
                    fileTitle: "LOAN APPLICATION QUERY",
                    dateFrom,
                    dateTo
                );

                return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting data.");
            }
        }


        [HttpPost]
        public async Task<ActionResult> LoadLoanData(
            string searchCriteria = "all",
            string dateFrom = null,
            string dateTo = null,
            string status = "Open",
            string deliquentStatus = "Current",
            string branchId = null,
            string loanCategory = "all",
            string loanTarget = "all",
            string approvalStatus = "all"
        )
        {
            try
            {
                // Date Parsing with Improved Error Handling
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParseExact(dateFrom, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate))
                {
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParseExact(dateTo, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
                {
                    endDate = parsedEndDate.AddDays(1).AddTicks(-1); // Set end of the day
                }

                // Construct Query Object with Additional Parameters
                var query = new GetLoanApplicationsDataTableQuery
                {
                    DataTableOptions = PostDataTableOptions(),
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue,
                    BranchId = !string.IsNullOrWhiteSpace(branchId) ? branchId : "all",
                    Status = status,
                    MemberId = "n/a", 
                    LoanCategory = loanCategory,
                    LoanTarget = loanTarget,
                    ApprovalStatus = approvalStatus,
                };

                // Fetch Data
                var dataTable = await _LoanServices.GetDataTableAsync(query, searchCriteria);
                var loanList = JsonConvert.DeserializeObject<List<LoanApplication>>(JsonConvert.SerializeObject(dataTable.data));

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
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading loan data: " + ex.Message);
            }
        }
        
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
        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            if (agrAggregates == null)
            {
                agrAggregates = await _individualProfileServices.GetAggregates();
            }
            var loanpurpose = await _loanPurposeServices.GetAllLoanPurpose();
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            var loanTerms = await _loanProductServices.GetProductTermOrDurationFromConfiguredProduct();
            var categories = await _loanProductServices.GetProductCategoryFromConfiguredProduct();
            ViewBag.LoanTypes = productEnumAgregates.LoanTypes;

            ViewBag.EconomicActivities = agrAggregates.EconomicActivities;
            ViewBag.CalculateInterestOn = productEnumAgregates.CalculateInterestOn;
            ViewBag.RepaymentCycles = productEnumAgregates.RepaymentCycles;
            ViewBag.LoanInterestMethods = productEnumAgregates.LoanInterestMethods;
            ViewBag.LoanStatuses = productEnumAgregates.LoanStatuses;
            ViewBag.LoanInterestTypes = productEnumAgregates.LoanInterestTypes;
            ViewBag.LoanInterestPeriods = productEnumAgregates.LoanInterestPeriods;
            ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
            ViewBag.RefundOrders = productEnumAgregates.RefundOrders;
            ViewBag.LoanPurposes = loanpurpose;
            ViewBag.LoanTypes = productEnumAgregates.LoanTypes;
            ViewBag.AmortizationTypes = productEnumAgregates.AmortizationTypes;
            ViewBag.LoanApplicationTypes = productEnumAgregates.LoanApplicationTypes;
            ViewBag.LoanCommiteeValidationStatuses = productEnumAgregates.LoanCommiteeValidationStatuses;
            ViewBag.LoanCategories = productEnumAgregates.LoanCategories;
            ViewBag.LoanTargets = productEnumAgregates.LoanTargets;
            ViewBag.LoanTerms = loanTerms;
            ViewBag.Categories = categories;
        }
        public async Task<bool> GetList()
        {
            //ViewBag.Groups = await auditTrailServices.GetLoans();
            //var users = await _userManagementServices.GetUserDropDownList();
            //ViewBag.Users = users.ToList();
            return true;
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _LoanServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}