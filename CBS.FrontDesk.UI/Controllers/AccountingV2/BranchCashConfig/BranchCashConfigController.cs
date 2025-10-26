// BranchCashConfigController.cs
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
        //private readonly MockBranchCashConfigService _mockData;
        private readonly GLAccountService _glAccountService;

        public BranchCashConfigController(
            BranchCashConfigService branchCashConfigService,
            BranchServices branchServices,
            //MockBranchCashConfigService mockData,
            GLAccountService glAccountService
,
            BranchAccountService branchAccountService)
        {
            _branchCashConfigService = branchCashConfigService;
            _branchServices = branchServices;
            // _mockData = mockData;
            _glAccountService = glAccountService;
            _branchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index()
        {
            await LoadInitialData();
            return View(new BranchCashConfigDto());
        }

        // In your BranchCashConfigController, update the LoadInitialData method:
        public async Task<bool> LoadInitialData()
        {
            var branches = await _branchServices.GetBranches();
            var glAccounts = await _glAccountService.GetGLAccounts();

            ViewBag.Branches = branches;
            ViewBag.GLAccounts = glAccounts;

            // Create dictionary for branch lookup
            ViewBag.BranchesDict = branches.ToDictionary(b => b.Id, b => b.Name);

            // Load chart of accounts for GL account dropdowns
            //var chartOfAccounts = await _chartOfAccountService.GetActiveChartOfAccounts();
            //ViewBag.GLAccounts = chartOfAccounts;

            // Create dictionary for GL account lookup
           // ViewBag.GLAccountsDict = chartOfAccounts.ToDictionary(a => a.Id, a => a.AccountName);

            return true;
        }

        public async Task<ActionResult> InitializeData(
             string KEY = null,
             string partialView = null,
             string path = null,
             string branchId = null,
             string serviceOption = null)
        {

            if (branchId != null)
            {
                var branchAccount = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);
                ViewBag.BranchAccounts = branchAccount;
            }
            if (path == "list")
            {
                var data = await _branchCashConfigService.GetBranchCashConfigsAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                await LoadInitialData();
                ViewBag.CurrentBranchName = _branchServices.GetBranchName();
                return PartialView(partialView, new BranchCashConfigDto());
            }
            else
            {
                await LoadInitialData();
                var data = await _branchCashConfigService.GetBranchCashConfigByIdAsync(KEY);
                ViewBag.CurrentBranchName = _branchServices.GetBranchName();
                return PartialView(partialView, data);
            }
        }



        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(BranchCashConfigDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _branchCashConfigService.CreateBranchCashConfigAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                return await Update(model);
            }
        }
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            // Get the data (from mock or service)
            var data = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
            if (data == null)
                return Content("<div class='alert alert-warning'>Record not found.</div>");

            return PartialView("_Details", data); // same as your ChequeCertification
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(BranchCashConfigDto model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _branchCashConfigService.UpdateBranchCashConfigAsync(model);
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

        [HttpPost]
        public async Task<ActionResult> GetByBranch(string branchId)
        {




            if (string.IsNullOrEmpty(branchId))
                return Json(new { success = false, message = "Branch ID is required." });


            var branchAccount =await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);
            ViewBag.BranchAccounts = branchAccount;

            var config = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);
            //var dataTable2 = JsonConvert.DeserializeObject<List<CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfig.BranchCashConfigDto>>(JsonConvert.SerializeObject(config.data));


            if (config != null)
            {
                return Json(new { success = true, data = config });
            }
            else
            {
                return Json(new { success = false, message = "No configuration found for this branch." });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> _BranchCashConfigDataTable(BranchCashConfigQueryDto branchCashConfigQueryDto)
        //{
        //    var dataTable = await _branchCashConfigService.GetDataTableAsync(branchCashConfigQueryDto);
        //    return Json(dataTable, JsonRequestBehavior.AllowGet);
        //}

        public async Task<ActionResult> _BranchCashConfigDataTable(BranchCashConfigQueryDto branchCashConfigQueryDto)
        {
            try
            {
                var dataTable = await _branchCashConfigService.GetDataTableAsync(branchCashConfigQueryDto);

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

        [HttpGet]
        public async Task<ActionResult> _Create(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
                return PartialView("_Error", "No branch selected.");

            await LoadInitialData();

            var branchAccount = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);
            ViewBag.BranchAccounts = branchAccount;

            // Try to fetch existing configuration
            var existingConfig = await _branchCashConfigService.GetBranchCashConfigByIdAsync(branchId);

            if (existingConfig != null)
            {
                return PartialView("_Create", existingConfig); // Edit mode
            }

            // If no config found, load an empty DTO for creation
            var model = new BranchCashConfigDto
            {
                BranchId = branchId
            };

            return PartialView("_Create", model); // Create mode
        }

    }
}