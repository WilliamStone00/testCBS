using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Config;
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
        private readonly IndividualProfileServices _individualProfileServices;

        public SalaryProcessedController(BranchServices branchServices, SalaryProcessedServices salaryProcessedServices, UserManagementServices userManagementServices, IndividualProfileServices individualProfileServices)
        {
            _branchServices = branchServices;
            _salaryProcessedServices = salaryProcessedServices;
            _userManagementServices = userManagementServices;
            _individualProfileServices = individualProfileServices;
        }

        public async Task<ActionResult> Index()
        {
            //// Branches
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches.ToList();

            // Users (for UploadedBy / ExecutedBy filters)
            var users = await _userManagementServices.GetUserDropDownList(branches.ToList());
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
        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            if (agrAggregates == null)
            {
                agrAggregates = await _individualProfileServices.GetAggregates();
            }
          
            ViewBag.Banks = agrAggregates.Banks;
            ViewBag.Branches = agrAggregates.Branches;
            ViewBag.EconomicActivities = agrAggregates.EconomicActivities;
            ViewBag.Countries = agrAggregates.Countries;
            ViewBag.Regions = agrAggregates.Regions;
            ViewBag.Divisions = agrAggregates.Divisions;
            ViewBag.Subdivisions = agrAggregates.Subdivisions;
            ViewBag.Towns = agrAggregates.Towns;
            ViewBag.Savings = agrAggregates.Savings;
            ViewBag.Organizations = agrAggregates.Organizations;
            ViewBag.bankingRelationships = agrAggregates.CustomerDefaultEnum.bankingRelationships;
            ViewBag.Genders = agrAggregates.CustomerDefaultEnum.genders;
            ViewBag.membershipApprovalStatuses = agrAggregates.CustomerDefaultEnum.membershipApprovalStatuses;
            ViewBag.activeStatuses = agrAggregates.CustomerDefaultEnum.activeStatuses;
            ViewBag.workingStatuses = agrAggregates.CustomerDefaultEnum.workingStatuses;
            ViewBag.legalForms = agrAggregates.CustomerDefaultEnum.legalForms;
            ViewBag.formalOrInformalSectors = agrAggregates.CustomerDefaultEnum.formalOrInformalSectors;
            ViewBag.maritalStatuses = agrAggregates.CustomerDefaultEnum.maritalStatuses;
            ViewBag.Languages = _individualProfileServices.GetLanguages();
            ViewBag.Categories = agrAggregates.CustomerDefaultEnum.customerCategories;
            ViewBag.relationships = agrAggregates.CustomerDefaultEnum.relationships;

        }

       

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
        // GET: /SalaryProcessed/RevokePv?tempPayCodeId=...&branchId=...
        [HttpGet]
        public async Task<ActionResult> RevokePv(string tempPayCodeId, string branchId = null)
        {
            if (string.IsNullOrWhiteSpace(tempPayCodeId))
                return new HttpStatusCodeResult(400, "Missing TempPayCodeId");

            // Optional: enrich the PV (amount, status, expiresAt, member/non-member, branch…)
            var info = await _salaryProcessedServices.GetSalary(tempPayCodeId);
            ViewBag.TempInfo = info;

            // Build a minimal SalaryExtract for the PV (the view expects SalaryExtract)
            var model = new SalaryExtract
            {
                Id = info?.Id,                      // if your DTO provides it
                BranchId = branchId ?? info?.BranchId,
                BranchCode = info?.BranchCode,
                BranchName = info?.BranchName,
                MemberName = info?.MemberName,
                MemberReference = info?.MemberReference,
                NonMemberReference = info?.NonMemberReference,
                LastTempPayCodeId = tempPayCodeId,

                RevokeTempPayCodesByIdList = new RevokeTempPayCodesByIdList
                {
                    BranchId = branchId ?? info?.BranchId,
                    TempPayCodeIds = new List<string> { tempPayCodeId },
                    Reason = null
                }
            };

            return PartialView("_RevokeTempPayCodePv", model);
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
            await PopulateAggregatesInViewBag();
            var vm = new RegisterNonMemberAndGenerateTempCode
            {
                SalaryExtractId = salaryExtractId,
                Kyc = new NoneMemberProfileCreateionRequest
                {
                    BranchId = branchId,
                    BranchCode = branchCode, 
                },
                ExpiresAt = null // let service default to now + 48h
            };
            var salaryExtract = new SalaryExtract { RegisterNonMemberAndGenerateTempCode = vm, Id= salaryExtractId };
            return PartialView("_RegisterNonMemberPv", salaryExtract);
        }
        // POST: /SalaryProcessed/RevokeTempPayCodes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RevokeTempPayCodes(RevokeTempPayCodesByIdList cmd)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.BranchId) || cmd.TempPayCodeIds == null || cmd.TempPayCodeIds.Count == 0)
                return new HttpStatusCodeResult(400, "Missing BranchId or TempPayCodeIds");

            try
            {
                var rsp = await _salaryProcessedServices.Delete(cmd); // implement in service
                                                                               // Expect rsp.Success + rsp.Message
                return Json(new { success = rsp.MessageStatus, message = rsp.MessageString, revokedIds = cmd.TempPayCodeIds });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // POST: /SalaryProcessed/RegisterNonMemberAndGenerateTempCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterNonMemberAndGenerateTempCode(RegisterNonMemberAndGenerateTempCode cmd)
        {
            var vm = new SalaryExtract { RegisterNonMemberAndGenerateTempCode = cmd };

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_RegisterNonMemberPv", vm);
            }

            try
            {
                var rsp = await _salaryProcessedServices.Register(cmd); // ExecutionMessages

                if (rsp == null)
                    return Json(new { success = false, message = "No response." });

                // ✅ make sure we pass the DTO back
                return Json(new
                {
                    success = rsp.Result,                 // bool
                    status = rsp.MessageStatus,          // optional
                    message = Messaging.MessageResult(rsp),
                    data = rsp.Data as TempPayCodeResultDto
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = ex.Message });
            }
        }




    }

}