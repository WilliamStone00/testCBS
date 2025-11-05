using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.BranchCashConfigV;
using CBS.BusinessService.CheckManagementSystem.BranchConfiguration;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Message;

namespace CBS.FrontDesk.UI.Controllers.AccountingManagement
{
	//[CheckSessionTimeOut]
	using System.Linq;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Web.Mvc;
	using CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV;
    using System;

    public class BranchCashConfigController : BaseController
    {
        private readonly BranchCashConfigService _branchCashConfigService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        private readonly GLAccountService _glAccountService;

        public BranchCashConfigController(
            BranchCashConfigService branchCashConfigService,
            BranchServices branchServices,
            GLAccountService glAccountService,
            BranchAccountService branchAccountService)
        {
            _branchCashConfigService = branchCashConfigService;
            _branchServices = branchServices;
            _glAccountService = glAccountService;
            _branchAccountService = branchAccountService;
        }

        // -------- Index ----------
        public async Task<ActionResult> Index()
        {
            var branches = await _branchServices.GetBranches();

            var vm = new BranchCashConfigManagement
            {
                BranchCashConfig = new BranchCashConfig(),
                BranchCashConfigDetailsVm = null,
                BranchCashConfigs = (await _branchCashConfigService.GetBranchCashConfigsAsync()).ToList()
            };

            ViewBag.Branches = branches;
            ViewBag.BranchAccounts = Enumerable.Empty<object>(); // filled after branch select
            return View(vm);
        }

        // Small helper to preload dropdown sources when needed (Edit/_Create)
        public async Task<bool> LoadInitialData()
        {
            var branches = await _branchServices.GetBranches();
            //var glAccounts = await _branchAccountService.GetBranchFromEndpointAsync();

            ViewBag.Branches = branches;
            //ViewBag.BranchAccounts = glAccounts;
            ViewBag.BranchesDict = branches.ToDictionary(b => b.Id, b => b.Name);
            return true;
        }

        // -------- List (partial shell used by "View list") ----------
        [HttpGet]
        public async Task<ActionResult> _Data()
        {
            // Fetch raw configs
            var configs = await _branchCashConfigService.GetBranchCashConfigsAsync();
            var branches = await _branchServices.GetBranches();
			var accounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(null);

            string BranchNameOf(string id) =>
                string.IsNullOrWhiteSpace(id) ? "" : (branches.FirstOrDefault(b => b.Id == id)?.Name ?? "");

            (string name, string number) NameNo(string id)
            {
                if (string.IsNullOrWhiteSpace(id)) return ("", "");
                var a = accounts.FirstOrDefault(x => x.Id == id);
                return a is null ? ("", "") : (a.Name, a.Code); // Code = account number
            }

            // Build flat rows (name + number already resolved)
            var rows = configs.Select(c =>
            {
                var (tillName, tillNo) = NameNo(c.CashInHandAccountId);
                var (vaultName, vaultNo) = NameNo(c.VaultAccountId);
                var (lioName, lioNo) = NameNo(c.HeadOfficeLiaisonAccountId);

                return new BranchCashConfigRowDto
                {
                    Id = c.Id,
                    BranchId = c.BranchId,
                    BranchName = BranchNameOf(c.BranchId),

                    CashInHandAccountName = tillName,
                    CashInHandAccountNumber = tillNo,

                    VaultAccountName = vaultName,
                    VaultAccountNumber = vaultNo,

                    HeadOfficeLiaisonAccountName = lioName,
                    HeadOfficeLiaisonAccountNumber = lioNo,

                    // Optional fields already supported by your DTO; leave empty if you don't need them here
                    RealTimeCashPosting = c.RealTimeCashPosting
                };
            }).ToList();

            var vm = new CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV.BranchCashConfigManagement
            {
                BranchCashConfigs = configs.ToList()
            };

            ViewBag.Rows = rows; // keep model shape; rows go via ViewBag
            return PartialView("_Data", vm);
        }
        // -------- Create/Edit form (partial) ----------
        // Supports either: /_Create?id=...  OR  /_Create?branchId=...
        [HttpGet]
        public async Task<ActionResult> _Create(string id = null, string branchId = null)
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            BranchCashConfig model = null;

            if (!string.IsNullOrWhiteSpace(id))
            {
                // Edit by id
                model = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
                if (model == null) return PartialView("_Error", "Configuration not found.");

                // load branch accounts for the model’s branch
                var listing1 = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(model.BranchId);
                ViewBag.BranchAccounts = _branchAccountService.DropDownGen(listing1.ToList());

                ViewBag.CurrentBranchName = ResolveBranchName(model.BranchId);
                ViewBag.Id = id;
                return PartialView("_Create", new BranchCashConfigManagement { BranchCashConfig = model });
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                // No context → empty form; accounts remain empty until user picks branch
                ViewBag.BranchAccounts = Enumerable.Empty<object>();
                return PartialView("_Create", new BranchCashConfigManagement { BranchCashConfig = new BranchCashConfig() });
            }

            // New config for a specific branch
            var existing = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);
			var listing = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);
			var accountsDto = _branchAccountService.DropDownGen(listing.ToList());
            ViewBag.BranchAccounts = accountsDto;
			ViewBag.CurrentBranchName = ResolveBranchName(model.BranchId);

            if (existing != null)
            {
                return PartialView("_Create", new BranchCashConfigManagement { BranchCashConfig = existing });
            }

            return PartialView("_Create", new BranchCashConfigManagement
            {
                BranchCashConfig = new BranchCashConfig { BranchId = branchId, RealTimeCashPosting = true }
            });
        }

        // -------- Create/Update POST (matches your form action="Create") ----------
        [HttpPost]
        public async Task<ActionResult> Create(BranchCashConfigManagement model)
        {
            if (model is null || model.BranchCashConfig is null)
                return Json(new { success = false, message = "Invalid payload." });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed. Please check all required fields." });

            var result = await _branchCashConfigService.CreateOrUpdateBranchCashConfigAsync(model.BranchCashConfig);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        // (Optional) keep old route alive if something else calls it
        [HttpPost]
        public Task<ActionResult> CreateOrUpdate(BranchCashConfigManagement model)
            => Create(model);

        // -------- Delete/Deactivate ----------
        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _branchCashConfigService.DeactivateBranchCashConfigAsync(KEY);
            return Json(new
            {
                success = result?.Result ?? false,
                message = Messaging.MessageResult(result) ?? "Operation completed."
            }, JsonRequestBehavior.AllowGet);
        }

        // -------- Ajax: load accounts + existing config for a branch (used by fetchBranchDetails) ----------
        [HttpPost]
        public async Task<ActionResult> GetByBranch(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
                return Json(new { success = false, message = "Branch ID is required." });

            try
            {
                // Load accounts for the branch (shape: IEnumerable<BranchAccountResponse>)
                var listing = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);

                // Convert to SelectListItem for UI
                var accounts = _branchAccountService.DropDownGen(listing.ToList());

                // Load existing config (if any)
                var config = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);

                if (config != null)
                {
                    return Json(new
                    {
                        success = true,
                        exists = true,
                        data = new
                        {
                            id = config.Id,
                            branchId = config.BranchId,
                            cashInHandAccountId = config.CashInHandAccountId,
                            vaultAccountId = config.VaultAccountId,
                            surplusIncomeAccountId = config.SurplusIncomeAccountId,
                            revenueAccountId = config.RevenueAccountId,
                            shortageExpenseAccountId = config.ShortageExpenseAccountId,
                            partnerAccountId = config.PartnerAccountId,
                            camcculAccountId = config.CamcculAccountId,
                            headOfficeLiaisonAccountId = config.HeadOfficeLiaisonAccountId,
                            formFeeIncomeAccountId = config.FormFeeIncomeAccountId,
                            realTimeCashPosting = config.RealTimeCashPosting
                        },
                        // send as simple array of {text, value} for the client
                        accounts = accounts
                    });
                }

                // New config for this branch
                return Json(new
                {
                    success = true,
                    exists = false,
                    data = new { id = (string)null, branchId, realTimeCashPosting = true },
                    accounts = accounts
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error loading branch configuration." });
            }
        }

        // -------- DataTable feed (name + account number) ----------
        [HttpPost]
        public async Task<ActionResult> GetDataTable()
        {
            var configs = await _branchCashConfigService.GetBranchCashConfigsAsync();
            var branches = await _branchServices.GetBranches();
            var accounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync("");

            (string name, string number) NameNo(string id)
            {
                if (string.IsNullOrWhiteSpace(id)) return ("", "");
                var a = accounts.FirstOrDefault(x => x.Id == id);
                return a is null ? ("", "") : (a.Name, a.Code); // adjust Code if your number field differs
            }

            string BranchNameOf(string id) =>
                string.IsNullOrWhiteSpace(id) ? "" : (branches.FirstOrDefault(b => b.Id == id)?.Name ?? "");

            var rows = configs.Select(c =>
            {
                var (tillName, tillNo) = NameNo(c.CashInHandAccountId);
                var (vaultName, vaultNo) = NameNo(c.VaultAccountId);
                var (lioName, lioNo) = NameNo(c.HeadOfficeLiaisonAccountId);

                return new BranchCashConfigRowDto
                {
                    Id = c.Id,
                    BranchId = c.BranchId,
                    BranchName = BranchNameOf(c.BranchId),

                    CashInHandAccountName = tillName,
                    CashInHandAccountNumber = tillNo,

                    VaultAccountName = vaultName,
                    VaultAccountNumber = vaultNo,

                    SurplusIncomeAccountName = NameNo(c.SurplusIncomeAccountId).name,
                    SurplusIncomeAccountNumber = NameNo(c.SurplusIncomeAccountId).number,

                    ShortageExpenseAccountName = NameNo(c.ShortageExpenseAccountId).name,
                    ShortageExpenseAccountNumber = NameNo(c.ShortageExpenseAccountId).number,

                    RevenueAccountName = NameNo(c.RevenueAccountId).name,
                    RevenueAccountNumber = NameNo(c.RevenueAccountId).number,

                    PartnerAccountName = NameNo(c.PartnerAccountId).name,
                    PartnerAccountNumber = NameNo(c.PartnerAccountId).number,

                    CamcculAccountName = NameNo(c.CamcculAccountId).name,
                    CamcculAccountNumber = NameNo(c.CamcculAccountId).number,

                    HeadOfficeLiaisonAccountName = lioName,
                    HeadOfficeLiaisonAccountNumber = lioNo,

                    FormFeeIncomeAccountName = NameNo(c.FormFeeIncomeAccountId).name,
                    FormFeeIncomeAccountNumber = NameNo(c.FormFeeIncomeAccountId).number,

                    RealTimeCashPosting = c.RealTimeCashPosting
                };
            }).ToList();

            return Json(new { data = rows });
        }

        // -------- Details modal ----------
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Content("<div class='text-danger p-2'>Invalid Id</div>");

            var c = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
            if (c == null)
                return Content("<div class='text-warning p-2'>Configuration not found.</div>");

            var branches = await _branchServices.GetBranches();
            //var accounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync("");

            var accounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(c.BranchId);

            // Convert to SelectListItem for UI
            //var accounts = _branchAccountService.DropDownGen(listing.ToList());



            string BranchNameOf(string bid) =>
                string.IsNullOrWhiteSpace(bid) ? "" : (branches.FirstOrDefault(b => b.Id == bid)?.Name ?? "");

            AccountPair PairOf(string accId)
            {
                if (string.IsNullOrWhiteSpace(accId)) return new AccountPair();
                var a = accounts.FirstOrDefault(x => x.Id == accId);
                return new AccountPair { Name = a?.Name ?? "", Number = a?.Code ?? "" };
            }

            var details = new BranchCashConfigDetailsVm
            {
                Id = c.Id,
                Branch = BranchNameOf(c.BranchId),
                RealTimeCashPosting = c.RealTimeCashPosting,
                CashInHand = PairOf(c.CashInHandAccountId),
                Vault = PairOf(c.VaultAccountId),
                SurplusIncome = PairOf(c.SurplusIncomeAccountId),
                ShortageExpense = PairOf(c.ShortageExpenseAccountId),
                Revenue = PairOf(c.RevenueAccountId),
                Partner = PairOf(c.PartnerAccountId),
                Camccul = PairOf(c.CamcculAccountId),
                HeadOfficeLiaison = PairOf(c.HeadOfficeLiaisonAccountId),
                FormFeeIncome = PairOf(c.FormFeeIncomeAccountId)
            };

            var vm = new BranchCashConfigManagement { BranchCashConfigDetailsVm = details };
            return PartialView("_Details", vm);
        }

        // -------- Edit shortcut (loads _Create with the model) ----------
        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Content("<div class='text-danger p-2'>Invalid Id</div>");

            var c = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
            if (c == null)
                return Content("<div class='text-warning p-2'>Configuration not found.</div>");

            await LoadInitialData();
            ViewBag.BranchAccounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync(c.BranchId);

            var vm = new BranchCashConfigManagement { BranchCashConfig = c };
            return PartialView("_Create", vm);
        }
        private string ResolveBranchName(string id)
        {
            var dict = ViewBag.BranchesDict as Dictionary<string, string>;
            if (!string.IsNullOrWhiteSpace(id) && dict != null && dict.TryGetValue(id, out var name))
                return name;
            return id ?? string.Empty;
        }

    }

    //public class BranchCashConfigController : BaseController
    //{
    //    private readonly BranchCashConfigService _branchCashConfigService;
    //    private readonly BranchServices _branchServices;
    //    private readonly BranchAccountService _branchAccountService;
    //    // Removed commented-out _mockData
    //    private readonly GLAccountService _glAccountService;

    //    public BranchCashConfigController(
    //        BranchCashConfigService branchCashConfigService,
    //        BranchServices branchServices,
    //        // Removed commented-out MockData
    //        GLAccountService glAccountService,
    //        BranchAccountService branchAccountService)
    //    {
    //        _branchCashConfigService = branchCashConfigService;
    //        _branchServices = branchServices;
    //        // _mockData = mockData; // No longer needed
    //        _glAccountService = glAccountService;
    //        _branchAccountService = branchAccountService;
    //    }

    //    // Index: load main management page
    //    public async Task<ActionResult> Index()
    //    {
    //        var branches = await _branchServices.GetBranches();

    //        var vm = new BranchCashConfigManagement
    //        {
    //            BranchCashConfig = new BranchCashConfig(),
    //            BranchCashConfigDetailsVm = null,
    //            BranchCashConfigs = (await _branchCashConfigService.GetBranchCashConfigsAsync()).ToList()
    //        };

    //        ViewBag.Branches = branches;
    //        ViewBag.BranchAccounts = Enumerable.Empty<object>(); // initially empty, loaded via AJAX on branch selection
    //        return View(vm);
    //    }

    //    // In BranchCashConfigController.cs

    //    public async Task<bool> LoadInitialData()
    //    {
    //        var branches = await _branchServices.GetBranches();
    //        var glAccounts = await _branchAccountService.GetBranchFromEndpointAsync();

    //        ViewBag.Branches = branches;
    //        ViewBag.BranchAccounts = glAccounts;
    //        // This dictionary is what we will use to look up the branch name!
    //        ViewBag.BranchesDict = branches.ToDictionary(b => b.Id, b => b.Name);

    //        return true;
    //    }
    //    // Removed the commented-out InitializeData method

    //    [HttpGet]
    //    public async Task<ActionResult> Detailsx(string id)
    //    {
    //        // Get the data (from mock or service)
    //        var data = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
    //        if (data == null)
    //            return Content("<div class='alert alert-warning'>Record not found.</div>");

    //        return PartialView("_Details", data);
    //    }

    //    // Removed the commented-out CreateOrUpdate method

    //    // This method should be the sole Create/Update handler
    //    [HttpPost]
    //    public async Task<ActionResult> CreateOrUpdate(BranchCashConfig model)
    //    {
    //        if (!ModelState.IsValid)
    //            return Json(new { success = false, message = "Validation failed. Please check all required fields." });

    //        // Call the service method that handles both create and update logic based on model.Id
    //        var result = await _branchCashConfigService.CreateOrUpdateBranchCashConfigAsync(model);
    //        return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
    //    }


    //    [HttpGet]
    //    public async Task<ActionResult> Delete(string KEY)
    //    {
    //        if (string.IsNullOrEmpty(KEY))
    //            return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

    //        var result = await _branchCashConfigService.DeactivateBranchCashConfigAsync(KEY);

    //        bool success = result?.Result ?? false;
    //        string message = Messaging.MessageResult(result) ?? "Operation completed.";

    //        return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
    //    }

    //    // Removed the commented-out GetByBranch method

    //    // Removed the commented-out _BranchCashConfigDataTable method

    //    // This is the functional _BranchCashConfigDataTable
    //    public async Task<ActionResult> _BranchCashConfigDataTable(BranchCashConfigQueryDto branchCashConfigQueryDto)
    //    {
    //        try
    //        {
    //            var dataTable = await _branchCashConfigService.GetDataTableAsync(branchCashConfigQueryDto);

    //            // Removed the redundant JsonConvert.Deserialize/Serialize step
    //            var dataTable2 = JsonConvert.DeserializeObject<List<BranchCashConfig>>(JsonConvert.SerializeObject(dataTable.data));

    //            return Json(new
    //            {
    //                draw = branchCashConfigQueryDto.Options?.draw ?? "1",
    //                recordsTotal = dataTable.recordsTotal,
    //                recordsFiltered = dataTable.recordsFiltered,
    //                data = dataTable2
    //            }, JsonRequestBehavior.AllowGet);
    //        }
    //        catch (Exception)
    //        {
    //            return Json(new { success = false, message = "Error loading liaison mapping data." });
    //        }
    //    }

    //    // In BranchCashConfigController.cs

    //    [HttpGet]
    //    public async Task<ActionResult> _Create(string branchId)
    //    {
    //        if (string.IsNullOrEmpty(branchId))
    //            return PartialView("_Error", "No branch selected.");

    //        // Ensure initial data (including BranchesDict) is loaded
    //        await LoadInitialData();

    //        // Retrieve the dictionary from ViewBag for easy access
    //        var branchesDict = ViewBag.BranchesDict as Dictionary<string, string>;

    //        // Default the branch name to the ID if not found, though it should be found
    //        string branchName = branchesDict != null && branchesDict.ContainsKey(branchId)
    //                            ? branchesDict[branchId]
    //                            : branchId;

    //        var branchAccount = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);
    //        ViewBag.BranchAccounts = branchAccount;

    //        // 1. Check for existing configuration using the Service method
    //        var existingConfig = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);

    //        if (existingConfig != null)
    //        {
    //            // 2. Configuration FOUND (Bafia case): Load existing data for EDIT
    //            ViewBag.CurrentBranchName = branchName; // Use the retrieved branchName
    //            return PartialView("_Create", existingConfig);
    //        }

    //        // 3. Configuration NOT FOUND: Load new DTO for CREATE
    //        var model = new BranchCashConfig
    //        {
    //            BranchId = branchId
    //        };

    //        ViewBag.CurrentBranchName = branchName; // Use the retrieved branchName
    //        return PartialView("_Create", model);
    //    }

    //    [HttpPost]
    //    public async Task<ActionResult> GetByBranch(string branchId)
    //    {
    //        if (string.IsNullOrEmpty(branchId))
    //            return Json(new { success = false, message = "Branch ID is required." });

    //        try
    //        {
    //            var branchAccounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);

    //            // Project minimal shape for the client
    //            var accountsDto = branchAccounts.Select(a => new { id = a.Id, name = a.Name }).ToList();

    //            var config = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);

    //            if (config != null)
    //            {
    //                return Json(new
    //                {
    //                    success = true,
    //                    exists = true,
    //                    data = new
    //                    {
    //                        id = config.Id,
    //                        branchId = config.BranchId,
    //                        cashInHandAccountId = config.CashInHandAccountId,
    //                        vaultAccountId = config.VaultAccountId,
    //                        surplusIncomeAccountId = config.SurplusIncomeAccountId,
    //                        revenueAccountId = config.RevenueAccountId,
    //                        shortageExpenseAccountId = config.ShortageExpenseAccountId,
    //                        partnerAccountId = config.PartnerAccountId,
    //                        camcculAccountId = config.CamcculAccountId,
    //                        headOfficeLiaisonAccountId = config.HeadOfficeLiaisonAccountId,
    //                        formFeeIncomeAccountId = config.FormFeeIncomeAccountId,
    //                        realTimeCashPosting = config.RealTimeCashPosting
    //                    },
    //                    accounts = accountsDto
    //                });
    //            }
    //            else
    //            {
    //                return Json(new
    //                {
    //                    success = true,
    //                    exists = false,
    //                    data = new
    //                    {
    //                        id = (string)null,
    //                        branchId = branchId,
    //                        realTimeCashPosting = true
    //                    },
    //                    accounts = accountsDto
    //                });
    //            }
    //        }
    //        catch
    //        {
    //            return Json(new { success = false, message = "Error loading branch configuration." });
    //        }
    //    }

    //    // Simple DTO for the grid row (names resolved server-side)

    //    [HttpPost]
    //    public async Task<ActionResult> GetDataTable()
    //    {
    //        // Fetch all configs
    //        var configs = await _branchCashConfigService.GetBranchCashConfigsAsync();

    //        // Preload lookups (branches + accounts) to resolve names once
    //        var branches = await _branchServices.GetBranches();
    //        var accounts = await _branchAccountService.GetBranchFromEndpointAsync();

    //        string NameOf(string id) =>
    //            string.IsNullOrWhiteSpace(id) ? "" : (accounts.FirstOrDefault(a => a.Id == id)?.Name ?? "");

    //        (string name, string number) NameNo(string id)
    //        {
    //            if (string.IsNullOrWhiteSpace(id)) return ("", "");
    //            var a = accounts.FirstOrDefault(x => x.Id == id);
    //            return a is null ? ("", "") : (a.Name, a.Code); // <-- adjust field if different
    //        }

    //        string BranchNameOf(string id) =>
    //            string.IsNullOrWhiteSpace(id) ? "" : (branches.FirstOrDefault(b => b.Id == id)?.Name ?? "");

    //        var rows = configs.Select(c =>
    //        {
    //            var (tillName, tillNo) = NameNo(c.CashInHandAccountId);
    //            var (vaultName, vaultNo) = NameNo(c.VaultAccountId);
    //            var (surpName, surpNo) = NameNo(c.SurplusIncomeAccountId);
    //            var (shrtName, shrtNo) = NameNo(c.ShortageExpenseAccountId);
    //            var (revName, revNo) = NameNo(c.RevenueAccountId);
    //            var (partName, partNo) = NameNo(c.PartnerAccountId);
    //            var (camName, camNo) = NameNo(c.CamcculAccountId);
    //            var (lioName, lioNo) = NameNo(c.HeadOfficeLiaisonAccountId);
    //            var (feeName, feeNo) = NameNo(c.FormFeeIncomeAccountId);

    //            return new BranchCashConfigRowDto
    //            {
    //                Id = c.Id,
    //                BranchId = c.BranchId,
    //                BranchName = BranchNameOf(c.BranchId),

    //                CashInHandAccountName = tillName,
    //                CashInHandAccountNumber = tillNo,

    //                VaultAccountName = vaultName,
    //                VaultAccountNumber = vaultNo,

    //                SurplusIncomeAccountName = surpName,
    //                SurplusIncomeAccountNumber = surpNo,

    //                ShortageExpenseAccountName = shrtName,
    //                ShortageExpenseAccountNumber = shrtNo,

    //                RevenueAccountName = revName,
    //                RevenueAccountNumber = revNo,

    //                PartnerAccountName = partName,
    //                PartnerAccountNumber = partNo,

    //                CamcculAccountName = camName,
    //                CamcculAccountNumber = camNo,

    //                HeadOfficeLiaisonAccountName = lioName,
    //                HeadOfficeLiaisonAccountNumber = lioNo,

    //                FormFeeIncomeAccountName = feeName,
    //                FormFeeIncomeAccountNumber = feeNo,

    //                RealTimeCashPosting = c.RealTimeCashPosting
    //            };
    //        }).ToList();

    //        return Json(new { data = rows });
    //    }

    //    [HttpGet]
    //    public async Task<ActionResult> Details(string id)
    //    {
    //        if (string.IsNullOrWhiteSpace(id))
    //            return Content("<div class='text-danger p-2'>Invalid Id</div>");

    //        var c = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
    //        if (c == null)
    //            return Content("<div class='text-warning p-2'>Configuration not found.</div>");

    //        var branches = await _branchServices.GetBranches();
    //        var accounts = await _branchAccountService.GetBranchFromEndpointAsync();

    //        string BranchNameOf(string bid) =>
    //            string.IsNullOrWhiteSpace(bid) ? "" : (branches.FirstOrDefault(b => b.Id == bid)?.Name ?? "");

    //        AccountPair PairOf(string accId)
    //        {
    //            if (string.IsNullOrWhiteSpace(accId)) return new AccountPair();
    //            var a = accounts.FirstOrDefault(x => x.Id == accId);
    //            return new AccountPair { Name = a?.Name ?? "", Number = a?.Code ?? "" };
    //        }

    //        var details = new BranchCashConfigDetailsVm
    //        {
    //            Id = c.Id,
    //            Branch = BranchNameOf(c.BranchId),
    //            RealTimeCashPosting = c.RealTimeCashPosting,
    //            CashInHand = PairOf(c.CashInHandAccountId),
    //            Vault = PairOf(c.VaultAccountId),
    //            SurplusIncome = PairOf(c.SurplusIncomeAccountId),
    //            ShortageExpense = PairOf(c.ShortageExpenseAccountId),
    //            Revenue = PairOf(c.RevenueAccountId),
    //            Partner = PairOf(c.PartnerAccountId),
    //            Camccul = PairOf(c.CamcculAccountId),
    //            HeadOfficeLiaison = PairOf(c.HeadOfficeLiaisonAccountId),
    //            FormFeeIncome = PairOf(c.FormFeeIncomeAccountId)
    //        };

    //        var vm = new CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV.BranchCashConfigManagement
    //        {
    //            BranchCashConfigDetailsVm = details
    //        };

    //        return PartialView("_Details", vm);
    //    }


    //    // Edit → reuse the _Create partial with model pre-filled
    //    [HttpGet]
    //    public async Task<ActionResult> Edit(string id)
    //    {
    //        if (string.IsNullOrWhiteSpace(id))
    //            return Content("<div class='text-danger p-2'>Invalid Id</div>");

    //        var c = await _branchCashConfigService.GetBranchCashConfigByIdAsync(id);
    //        if (c == null)
    //            return Content("<div class='text-warning p-2'>Configuration not found.</div>");

    //        var vm = new CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV.BranchCashConfigManagement
    //        {
    //            BranchCashConfig = c
    //        };

    //        // preload dropdowns for the selected branch (so selects render with correct options)
    //        var accounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync(c.BranchId);
    //        ViewBag.BranchAccounts = accounts;
    //        ViewBag.Branches = await _branchServices.GetBranches();

    //        return PartialView("_Create", vm);
    //    }

    //    // Helper to build a readable Details VM
    //    private async Task<dynamic> BuildDetailsVmAsync(BranchCashConfig c)
    //    {
    //        var branches = await _branchServices.GetBranches();
    //        var accounts = await _branchAccountService.GetBranchFromEndpointAsync();

    //        string NameOf(string id) =>
    //            string.IsNullOrWhiteSpace(id) ? "" : (accounts.FirstOrDefault(a => a.Id == id)?.Name ?? "");

    //        string BranchNameOf(string id) =>
    //            string.IsNullOrWhiteSpace(id) ? "" : (branches.FirstOrDefault(b => b.Id == id)?.Name ?? "");

    //        return new
    //        {
    //            Id = c.Id,
    //            Branch = BranchNameOf(c.BranchId),
    //            CashInHand = NameOf(c.CashInHandAccountId),
    //            Vault = NameOf(c.VaultAccountId),
    //            SurplusIncome = NameOf(c.SurplusIncomeAccountId),
    //            ShortageExpense = NameOf(c.ShortageExpenseAccountId),
    //            Revenue = NameOf(c.RevenueAccountId),
    //            Partner = NameOf(c.PartnerAccountId),
    //            Camccul = NameOf(c.CamcculAccountId),
    //            HeadOfficeLiaison = NameOf(c.HeadOfficeLiaisonAccountId),
    //            FormFeeIncome = NameOf(c.FormFeeIncomeAccountId),
    //            RealTimeCashPosting = c.RealTimeCashPosting
    //        };
    //    }

    //}
}