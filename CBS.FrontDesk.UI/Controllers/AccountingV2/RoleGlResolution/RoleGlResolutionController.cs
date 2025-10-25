using CBS.BusinessService.Accounting;
using CBS.BusinessService.AccountingV2.RoleGlResolution;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingV2.RoleGlResolution;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.RoleGlResolution
{
    public class RoleGlResolutionController : BaseController
    {
        private readonly RoleGlResolutionService _roleGlResolutionService;
        private readonly BranchServices _branchServices;
        private readonly ChartOfAccountServices _chartOfAccountServices;

        public RoleGlResolutionController(
            RoleGlResolutionService roleGlResolutionService,
            BranchServices branchServices,
            ChartOfAccountServices chartOfAccountServices)
        {
            _roleGlResolutionService = roleGlResolutionService;
            _branchServices = branchServices;
            _chartOfAccountServices = chartOfAccountServices;
        }

        public async Task<ActionResult> Index()
        {
            await PopulateViewBags();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadRoleGlResolutionsData(GetRoleGlResolutionsDataTableQuery query)
        {
            try
            {
                var dataTable = await _roleGlResolutionService.GetDataTableAsync(query);
                var resolutionList = JsonConvert.DeserializeObject<List<RoleGlResolutionDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                return Json(new
                {
                    draw = query.Options?.draw ?? "1",
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = resolutionList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading Role GL Resolution data." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetRoleGlResolutionFormPartial(string id = null)
        {
            try
            {
                RoleGlResolutionModel model;

                if (!string.IsNullOrEmpty(id))
                {
                    var existingResolution = await _roleGlResolutionService.GetRoleGlResolutionAsync(id);
                    model = new RoleGlResolutionModel
                    {
                        Id = existingResolution.Id,
                        Scope = existingResolution.Scope,
                        ScopeId = existingResolution.ScopeId,
                        RoleId = existingResolution.RoleId,
                        BranchAccountId = existingResolution.BranchAccountId,
                        ScopeName = existingResolution.ScopeName,
                        RoleName = existingResolution.RoleName,
                        BranchAccountName = existingResolution.BranchAccountName
                    };
                }
                else
                {
                    model = new RoleGlResolutionModel();
                }

                await PopulateViewBags();
                return PartialView("_RoleGlResolutionForm", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading form." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(RoleGlResolutionModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data." });
            }

            try
            {
                var command = new CreateOrUpdateRoleGlResolutionCommand
                {
                    Id = model.Id,
                    Scope = model.Scope,
                    ScopeId = model.ScopeId,
                    RoleId = model.RoleId,
                    BranchAccountId = model.BranchAccountId
                };

                var result = await _roleGlResolutionService.CreateOrUpdateRoleGlResolutionAsync(command);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving Role GL Resolution." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var result = await _roleGlResolutionService.DeleteRoleGlResolutionAsync(id);
                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting Role GL Resolution." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetBranchAccounts(string branchId)
        {
            try
            {
                var allAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
                var result = allAccounts.Select(a => new {
                    Id = a.Id,
                    Name = $"{a.AccountNumber} - {a.LabelEn}"
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetRoles()
        {
            try
            {
                // This would typically come from your system configuration
                var roles = new List<object>
                {
                    new { Id = "TELLER", Name = "Teller" },
                    new { Id = "VAULT", Name = "Vault" },
                    new { Id = "CUSTOMER", Name = "Customer" },
                    new { Id = "BRANCH", Name = "Branch" },
                    new { Id = "LIAISON", Name = "Liaison" },
                    new { Id = "HEAD_OFFICE", Name = "Head Office" },
                    new { Id = "AFFILIATE", Name = "Affiliate" }
                };

                return Json(roles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        private async Task PopulateViewBags()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            ViewBag.Scopes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Global", Value = "GLOBAL" },
                new SelectListItem { Text = "Affiliate", Value = "AFFILIATE" },
                new SelectListItem { Text = "Branch", Value = "BRANCH", Selected = true }
            };
        }
    }
}