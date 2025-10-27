using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.BranchCashConfig;
using CBS.BusinessService.AccountingV2.LiaisonAccount2;
using CBS.BusinessService.CheckManagementSystem.BranchConfiguration;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfig;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.UI.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingManagement
{
    //[CheckSessionTimeOut]
    public class BranchCashConfigController : BaseController
    {
        private readonly BranchCashConfigService _branchCashConfigService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        // Removed commented-out _mockData
        private readonly GLAccountService _glAccountService;

        public BranchCashConfigController(
            BranchCashConfigService branchCashConfigService,
            BranchServices branchServices,
            // Removed commented-out MockData
            GLAccountService glAccountService,
            BranchAccountService branchAccountService)
        {
            _branchCashConfigService = branchCashConfigService;
            _branchServices = branchServices;
            // _mockData = mockData; // No longer needed
            _glAccountService = glAccountService;
            _branchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index()
        {
            await LoadInitialData();
            return View(new BranchCashConfigDto());
        }

        // In BranchCashConfigController.cs

        public async Task<bool> LoadInitialData()
        {
            var branches = await _branchServices.GetBranches();
            var glAccounts = await _glAccountService.GetGLAccounts();

            ViewBag.Branches = branches;
            ViewBag.GLAccounts = glAccounts;
            // This dictionary is what we will use to look up the branch name!
            ViewBag.BranchesDict = branches.ToDictionary(b => b.Id, b => b.Name);

            return true;
        }
        // Removed the commented-out InitializeData method

        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            // Get the data (from mock or service)
            var data = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
            if (data == null)
                return Content("<div class='alert alert-warning'>Record not found.</div>");

            return PartialView("_Details", data);
        }

        // Removed the commented-out CreateOrUpdate method

        // This method should be the sole Create/Update handler
        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(BranchCashConfigDto model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed. Please check all required fields." });

            // Call the service method that handles both create and update logic based on model.Id
            var result = await _branchCashConfigService.CreateOrUpdateBranchCashConfigAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }


        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _branchCashConfigService.DeactivateBranchCashConfigAsync(KEY);

            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

        // Removed the commented-out GetByBranch method

        // Removed the commented-out _BranchCashConfigDataTable method

        // This is the functional _BranchCashConfigDataTable
        public async Task<ActionResult> _BranchCashConfigDataTable(BranchCashConfigQueryDto branchCashConfigQueryDto)
        {
            try
            {
                var dataTable = await _branchCashConfigService.GetDataTableAsync(branchCashConfigQueryDto);

                // Removed the redundant JsonConvert.Deserialize/Serialize step
                var dataTable2 = JsonConvert.DeserializeObject<List<CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfig.BranchCashConfigDto>>(JsonConvert.SerializeObject(dataTable.data));

                return Json(new
                {
                    draw = branchCashConfigQueryDto.Options?.draw ?? "1",
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = dataTable2
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error loading liaison mapping data." });
            }
        }

        // In BranchCashConfigController.cs

        [HttpGet]
        public async Task<ActionResult> _Create(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
                return PartialView("_Error", "No branch selected.");

            // Ensure initial data (including BranchesDict) is loaded
            await LoadInitialData();

            // Retrieve the dictionary from ViewBag for easy access
            var branchesDict = ViewBag.BranchesDict as Dictionary<string, string>;

            // Default the branch name to the ID if not found, though it should be found
            string branchName = branchesDict != null && branchesDict.ContainsKey(branchId)
                                ? branchesDict[branchId]
                                : branchId;

            var branchAccount = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);
            ViewBag.BranchAccounts = branchAccount;

            // 1. Check for existing configuration using the Service method
            var existingConfig = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);

            if (existingConfig != null)
            {
                // 2. Configuration FOUND (Bafia case): Load existing data for EDIT
                ViewBag.CurrentBranchName = branchName; // Use the retrieved branchName
                return PartialView("_Create", existingConfig);
            }

            // 3. Configuration NOT FOUND: Load new DTO for CREATE
            var model = new BranchCashConfigDto
            {
                BranchId = branchId
            };

            ViewBag.CurrentBranchName = branchName; // Use the retrieved branchName
            return PartialView("_Create", model);
        }
        // This is the functional GetByBranch
        [HttpPost]
        public async Task<ActionResult> GetByBranch(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
                return Json(new { success = false, message = "Branch ID is required." });

            try
            {
                var branchAccount = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);
                ViewBag.BranchAccounts = branchAccount;

                var config = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);

                if (config != null)
                {
                    return Json(new { success = true, data = config, exists = true });
                }
                else
                {
                    // Return empty config with branch ID for new configuration
                    var newConfig = new BranchCashConfigDto { BranchId = branchId };
                    return Json(new { success = true, data = newConfig, exists = false });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading branch configuration." });
            }
        }

        // Removed the separate Update method (as it's consolidated into CreateOrUpdate)

        // Removed the Improved InitializeData method (as the original was removed)
    }
}