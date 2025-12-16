using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.BranchCashConfigV;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Message;

namespace CBS.FrontDesk.UI.Controllers.AccountingManagement
{
    //[CheckSessionTimeOut]
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV;

    public class BranchCashConfigController : BaseController
    {
        private readonly BranchCashConfigService _branchCashConfigService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        public BranchCashConfigController(
            BranchCashConfigService branchCashConfigService,
            BranchServices branchServices,
            BranchAccountService branchAccountService)
        {
            _branchCashConfigService = branchCashConfigService;
            _branchServices = branchServices;
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

                    // --- Core Cash-Control ---
                    CashInHandAccountName = tillName,
                    CashInHandAccountNumber = tillNo,

                    VaultAccountName = vaultName,
                    VaultAccountNumber = vaultNo,

                    SurplusIncomeAccountName = NameNo(c.SurplusIncomeAccountId).name,
                    SurplusIncomeAccountNumber = NameNo(c.SurplusIncomeAccountId).number,

                    ShortageExpenseAccountName = NameNo(c.ShortageExpenseAccountId).name,
                    ShortageExpenseAccountNumber = NameNo(c.ShortageExpenseAccountId).number,

                    // --- Inter-branch / HO ---
                    HeadOfficeLiaisonAccountName = lioName,
                    HeadOfficeLiaisonAccountNumber = lioNo,

                    // --- Revenue, Partner, CamCCUL ---
                    RevenueAccountName = NameNo(c.RevenueAccountId).name,
                    RevenueAccountNumber = NameNo(c.RevenueAccountId).number,

                    PartnerAccountName = NameNo(c.PartnerAccountId).name,
                    PartnerAccountNumber = NameNo(c.PartnerAccountId).number,

                    CamcculAccountName = NameNo(c.CamcculAccountId).name,
                    CamcculAccountNumber = NameNo(c.CamcculAccountId).number,

                    // --- Form Fee Income ---
                    FormFeeIncomeAccountName = NameNo(c.FormFeeIncomeAccountId).name,
                    FormFeeIncomeAccountNumber = NameNo(c.FormFeeIncomeAccountId).number,

                    // --- Loan Transit / Interest ---
                    LoanTransitAccountName = NameNo(c.LoanTransitAccountId).name,
                    LoanTransitAccountNumber = NameNo(c.LoanTransitAccountId).number,

                    InterestGeneratedFromAccountName = NameNo(c.InterestGeneratedFromAccountId).name,
                    InterestGeneratedFromAccountNumber = NameNo(c.InterestGeneratedFromAccountId).number,

                    // --- Commissions ---
                    CashInCommisionAccountName = NameNo(c.CashInCommisionAccountIDAccountId).name,
                    CashInCommisionAccountNumber = NameNo(c.CashInCommisionAccountIDAccountId).number,

                    CashOutCommisionAccountName = NameNo(c.CashOutCommisionAccountIDAccountId).name,
                    CashOutCommisionAccountNumber = NameNo(c.CashOutCommisionAccountIDAccountId).number,

                    TransfterCommisionAccountName = NameNo(c.TransfterCommisionAccountIDAccountId).name,
                    TransfterCommisionAccountNumber = NameNo(c.TransfterCommisionAccountIDAccountId).number,

                    // --- VAT ---
                    VATAccountName = NameNo(c.VATAccountId).name,
                    VATAccountNumber = NameNo(c.VATAccountId).number,

                    // --- Mobile Money (MoMoCash) ---
                    MomocashAccountName = NameNo(c.MomocashAccountId).name,
                    MomocashAccountNumber = NameNo(c.MomocashAccountId).number,

                    MomocashCommissionGlName = NameNo(c.MomocashCommissionGlId).name,
                    MomocashCommissionGlNumber = NameNo(c.MomocashCommissionGlId).number,

                    // --- Global Fallback Suspense ---
                    FallbackSuspenseAccountName = NameNo(c.FallbackSuspenseAccountId).name,
                    FallbackSuspenseAccountNumber = NameNo(c.FallbackSuspenseAccountId).number,

                    // --- Status ---
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

                            // Core cash-control (mandatory)
                            cashInHandAccountId = config.CashInHandAccountId,
                            vaultAccountId = config.VaultAccountId,
                            surplusIncomeAccountId = config.SurplusIncomeAccountId,
                            shortageExpenseAccountId = config.ShortageExpenseAccountId,

                            // ✅ NEW: Cash reversal account
                            cashReversalAccountId = config.CashReversalAccountId,

                            // Generic / fallback revenue & partners
                            revenueAccountId = config.RevenueAccountId,
                            partnerAccountId = config.PartnerAccountId,
                            camcculAccountId = config.CamcculAccountId,
                            headOfficeLiaisonAccountId = config.HeadOfficeLiaisonAccountId,
                            formFeeIncomeAccountId = config.FormFeeIncomeAccountId,

                            // Loan transit
                            loanTransitAccountId = config.LoanTransitAccountId,

                            // 🔹 Fallback / backup accounts
                            cashInCommisionAccountIDAccountId = config.CashInCommisionAccountIDAccountId,
                            cashOutCommisionAccountIDAccountId = config.CashOutCommisionAccountIDAccountId,
                            transfterCommisionAccountIDAccountId = config.TransfterCommisionAccountIDAccountId,
                            vatAccountId = config.VATAccountId,
                            interestGeneratedFromAccountId = config.InterestGeneratedFromAccountId,

                            // 🔹 Global fallback suspense GL
                            fallbackSuspenseAccountId = config.FallbackSuspenseAccountId,

                            // 🔥 Mobile Money (MoMoCash)
                            momocashAccountId = config.MomocashAccountId,
                            momocashCommissionGlId = config.MomocashCommissionGlId,

                            // Other flags
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
                    data = new
                    {
                        id = (string)null,
                        branchId,

                        // ✅ NEW: default value for cash reversal account
                        cashReversalAccountId = (string)null,

                        realTimeCashPosting = true
                    },
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

                // ✅ NEW: Cash reversal GL
                var (revName, revNo) = NameNo(c.CashReversalAccountId);

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

                    // ✅ NEW: expose reversal account
                    CashReversalAccountName = revName,
                    CashReversalAccountNumber = revNo,

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

                    LoanTransitAccountName = NameNo(c.LoanTransitAccountId).name,
                    LoanTransitAccountNumber = NameNo(c.LoanTransitAccountId).number,

                    // 🔹 Commission fallback accounts
                    CashInCommisionAccountName = NameNo(c.CashInCommisionAccountIDAccountId).name,
                    CashInCommisionAccountNumber = NameNo(c.CashInCommisionAccountIDAccountId).number,

                    CashOutCommisionAccountName = NameNo(c.CashOutCommisionAccountIDAccountId).name,
                    CashOutCommisionAccountNumber = NameNo(c.CashOutCommisionAccountIDAccountId).number,

                    TransfterCommisionAccountName = NameNo(c.TransfterCommisionAccountIDAccountId).name,
                    TransfterCommisionAccountNumber = NameNo(c.TransfterCommisionAccountIDAccountId).number,

                    VATAccountName = NameNo(c.VATAccountId).name,
                    VATAccountNumber = NameNo(c.VATAccountId).number,

                    InterestGeneratedFromAccountName = NameNo(c.InterestGeneratedFromAccountId).name,
                    InterestGeneratedFromAccountNumber = NameNo(c.InterestGeneratedFromAccountId).number,

                    // 🔥 Mobile Money accounts
                    MomocashAccountName = NameNo(c.MomocashAccountId).name,
                    MomocashAccountNumber = NameNo(c.MomocashAccountId).number,

                    MomocashCommissionGlName = NameNo(c.MomocashCommissionGlId).name,
                    MomocashCommissionGlNumber = NameNo(c.MomocashCommissionGlId).number,

                    // 🔹 Global Fallback Suspense GL
                    FallbackSuspenseAccountName = NameNo(c.FallbackSuspenseAccountId).name,
                    FallbackSuspenseAccountNumber = NameNo(c.FallbackSuspenseAccountId).number,

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

            var accounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(c.BranchId);

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

                // Core cash-control accounts
                CashInHand = PairOf(c.CashInHandAccountId),
                Vault = PairOf(c.VaultAccountId),
                SurplusIncome = PairOf(c.SurplusIncomeAccountId),
                ShortageExpense = PairOf(c.ShortageExpenseAccountId),

                // ✅ NEW: Cash reversal account
                CashReversal = PairOf(c.CashReversalAccountId),

                // Revenue & partner accounts
                Revenue = PairOf(c.RevenueAccountId),
                Partner = PairOf(c.PartnerAccountId),
                Camccul = PairOf(c.CamcculAccountId),
                FormFeeIncome = PairOf(c.FormFeeIncomeAccountId),

                // Head Office Liaison
                HeadOfficeLiaison = PairOf(c.HeadOfficeLiaisonAccountId),

                // Loan & interest accounts
                LoanTransit = PairOf(c.LoanTransitAccountId),
                InterestGeneratedFrom = PairOf(c.InterestGeneratedFromAccountId),

                // Commission fallback accounts
                CashInCommission = PairOf(c.CashInCommisionAccountIDAccountId),
                CashOutCommission = PairOf(c.CashOutCommisionAccountIDAccountId),
                TransferCommission = PairOf(c.TransfterCommisionAccountIDAccountId),

                // VAT backup account
                VAT = PairOf(c.VATAccountId),

                // MoMoCash accounts
                Momocash = PairOf(c.MomocashAccountId),
                MomocashCommission = PairOf(c.MomocashCommissionGlId),

                // 🔹 Global Fallback Suspense GL
                FallbackSuspense = PairOf(c.FallbackSuspenseAccountId)
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

}