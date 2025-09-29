using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    //[CheckSessionTimeOutAttribute]
    public class SalaryProcessedController : BaseController
    {
        // GET: SalaryUpload
        private readonly SalaryProcessedServices _salaryProcessedServices;
        private readonly BranchServices _branchServices;
        private readonly SalaryAnalysisResultServices _salaryAnalysisResultServices;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;
        private readonly UserManagementServices _userManagementServices;
        public SalaryProcessedController(BranchServices branchServices, SalaryProcessedServices salaryProcessedServices, UserManagementServices userManagementServices)
        {
            _branchServices = branchServices;
            _salaryProcessedServices = salaryProcessedServices;
            _userManagementServices = userManagementServices;
        }

        public async Task<ActionResult> Index()
        {
            // Branches
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches.ToList();

            // Users (for UploadedBy / ExecutedBy filters)
            var users = await _userManagementServices.GetUserDropDownList();
            ViewBag.Users = users.ToList();
            // Optionally preload common filter values for Status (Paid / Pending)
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "All", Value = "" },
                new SelectListItem { Text = "Salary Paid", Value = "true" },
                new SelectListItem { Text = "Pending", Value = "false" }
            };

            return View();
        }


        // In your controller (fields assumed already injected):
        // private readonly IBranchServices _branchServices;
        // private readonly IChartOfAccountServices chartOfAccountServices;

        // Reuse one immutable list for File Types (no per-request allocation)
        //private static readonly IReadOnlyList<SelectListItem> FileTypeOptions =
        //    new List<SelectListItem>
        //    {
        //        new SelectListItem { Value = "CivilServants",       Text = "Civil Servant Files" },
        //        new SelectListItem { Value = "PrivateInstitutions", Text = "Private Institution Files" },
        //        new SelectListItem { Value = "StandingOrder",       Text = "Standing Order Files" },
        //        new SelectListItem { Value = "Analysis",            Text = "Analysed Files" },
        //        new SelectListItem { Value = "ManualEntryDailyCollection", Text = "Daily Collection Files" },
        //        new SelectListItem { Value = "Others",              Text = "Other Files" }
        //    }.AsReadOnly();

        // One helper to populate common dropdowns; optionally include Chart of Accounts
        //private async Task PopulateDropdownsAsync(bool includeChartOfAccounts)
        //{
        //    var branchesTask = _branchServices.GetBranches();
        //    Task<IEnumerable<object>> coaTask = Task.FromResult(Enumerable.Empty<object>());

        //    if (includeChartOfAccounts)
        //        coaTask = chartOfAccountServices.GetChartOfAccounts(false).ContinueWith(t => t.Result.Cast<object>());

        //    // Run in parallel when both are needed
        //    await Task.WhenAll(includeChartOfAccounts ? new Task[] { branchesTask, coaTask } : new Task[] { branchesTask });

        //    ViewBag.Branches = (await branchesTask);                 // original behavior
        //    ViewBag.FileTypes = FileTypeOptions;                     // reused list

        //    if (includeChartOfAccounts)
        //        ViewBag.StandingOrderSourceAccountOptions = (await coaTask).ToList(); // original ToList()
        //}

        // Actions

        [HttpPost]
        public async Task<ActionResult> LoadData(GetProcessedSalaryDataTableQuery query)
        {
            try
            {
                var dataTable = await _salaryProcessedServices.GetDataTableAsync(query);
                var userList = JsonConvert.DeserializeObject<List<SalaryExtract>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                return Json(new
                {
                    draw = query.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = userList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error loading users: " + ex.Message);
            }
        }
        // GET: /SalaryProcessed/DetailsPv?id=...
        [HttpGet]
        public async Task<ActionResult> DetailsPv(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return new HttpStatusCodeResult(400, "Missing id");
            var model = await _salaryProcessedServices.GetSalary(id);
            if (model == null) return HttpNotFound("SalaryExtract not found");
            // Returns a compact, well-structured detail partial for the modal
            return PartialView("_ProcessedSalaryDetails", model);
        }
       

        // GET: /SalaryProcessed/RegistrationPv?salaryExtractId=...&branchId=...&branchCode=...
        [HttpGet]
        public async Task<ActionResult> RegistrationPv(string salaryExtractId, string branchId = null, string branchCode = null)
        {
            if (string.IsNullOrWhiteSpace(salaryExtractId)) return new HttpStatusCodeResult(400, "Missing SalaryExtractId");

            var se = await _salaryProcessedServices.GetSalary(salaryExtractId);
            if (se == null) return HttpNotFound("SalaryExtract not found");
            if (se.Status) // already paid
                return new HttpStatusCodeResult(400, "Entry already paid; activation not allowed.");

            var vm = new RegisterNonMemberAndGenerateTempCode
            {
                SalaryExtractId = salaryExtractId,
                Kyc = new NoneMemberProfileCreateionRequest
                {
                    BranchId = branchId ?? se.BranchId,
                    BranchCode = branchCode ?? se.BranchCode ?? ""
                },
                ExpiresAt = null // let service default to now + 48h
            };

            return PartialView("_RegisterNonMember", vm);
        }

        // POST: /SalaryProcessed/RegisterNonMemberAndGenerateTempCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterNonMemberAndGenerateTempCode(RegisterNonMemberAndGenerateTempCode cmd)
        {
            if (!ModelState.IsValid)
            {
                // re-render the PV with validation messages
                return PartialView("_RegisterNonMember", cmd);
            }

            try
            {
                var rsp = await _salaryProcessedServices.Register(cmd);
                //if (!rsp.Success)
                //{
                //    ModelState.AddModelError("", rsp.Message ?? "Registration failed");
                //    return PartialView("_RegisterNonMember", cmd);
                //}

                //// signal client to refresh table and close modal
                return Json(new { success = rsp.Result, status = rsp.MessageStatus, message = Messaging.MessageResult(rsp) });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("_RegisterNonMember", cmd);
            }
        }


    }

}