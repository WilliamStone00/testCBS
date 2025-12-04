
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.AccountingV2.BranchCashConfigV;
using CBS.BusinessService.AccountingV2.CashReconciliation;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]

    public class VaultController : BaseController
    {
        // GET: Vault
        private readonly VaultServices _services;
        private readonly BranchServices _branchServices;
        private readonly CashReconciliationService _cashReconciliationService;
        private readonly BranchCashConfigService _branchCashConfigService;

        public VaultController(VaultServices services, BranchServices branchServices = null, CashReconciliationService cashReconciliationService = null, BranchCashConfigService branchCashConfigService = null)
        {
            _services = services;
            _branchServices = branchServices;
            _cashReconciliationService = cashReconciliationService;
            _branchCashConfigService = branchCashConfigService;
        }

        public async Task<ActionResult> Index()
        {
            await GetValues();
            return View(new Vault());
        }
        public async Task<ActionResult> Initilization()
        {
            await GetValues();
            return View(new Vault());
        }
        [HttpPost]
        public async Task<ActionResult> Create(Vault model)
        {
           
            // Validate the model state
            if (!ModelState.IsValid)
            {
                // If model validation fails, return validation errors as JSON response
                return Json(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            // If the Id is null, it's a new holiday entry, so call the Create service
            if (model.AddVaultCommand.Id == null)
            {
                var data = await _services.Create(model.AddVaultCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                // If the Id is not null, it's an update, so call the Update method
                return await Update(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Update(Vault model)
        {
            var data = await _services.Update(model.AddVaultCommand);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        [HttpPost]
        public async Task<ActionResult> VaultInitialization(Vault model)
        {
            // Basic guard
            if (model?.VaultInitializationCommand == null)
            {
                return Json(new
                {
                    success = false,
                    status = "Error",
                    message = "Invalid vault initialization request."
                });
            }

            // 🔐 Control: Cash in hand must be zero before initializing denominations
            if (model.VaultInitializationCommand.AmountInHand > 0m)
            {
                const string msg =
                    "Vault cannot be initialized while Cash in hand is greater than zero. " +
                    "Please reconcile the cashier, move all cash to the vault, then retry vault initialization.";

                return Json(new
                {
                    success = false,
                    status = "Warning", // use your enum/string here if you have one
                    message = msg
                });
            }

            // ✅ Proceed with normal flow
            var data = await _services.VaultInitialization(model.VaultInitializationCommand);

            return Json(new
            {
                success = data.Result,
                status = data.MessageStatus,
                message = Messaging.MessageResult(data)
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetVaultBalances(string branchId)
        {
            if (!Request.IsAjaxRequest())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid request type.");

            if (string.IsNullOrWhiteSpace(branchId))
            {
                return Json(
                    new { success = false, message = "Branch Id is required." },
                    JsonRequestBehavior.AllowGet
                );
            }

            // Optional: restrict branch visibility
            // if (!_userInfoToken.IsHeadOffice &&
            //     !string.Equals(branchId, _userInfoToken.BranchID, StringComparison.OrdinalIgnoreCase))
            // {
            //     return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
            //         "You are not allowed to query another branch's vault.");
            // }

            try
            {
                var config = await _branchCashConfigService.GetBranchCashConfigByBranchIdAsync(branchId);

                if (config == null)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Branch cash configuration not found for the selected branch."
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                if (string.IsNullOrWhiteSpace(config.VaultAccountId) ||
                    string.IsNullOrWhiteSpace(config.CashInHandAccountId))
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Vault or cash-in-hand account is not configured for this branch."
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                var cashInHand = await _cashReconciliationService.GetAccountBalance(config.CashInHandAccountId);
                var cashInVault = await _cashReconciliationService.GetAccountBalance(config.VaultAccountId);
                decimal cashInHandBalance = cashInHand.TrialBalanceBalance;
                decimal cashInVaultBalance = cashInVault.TrialBalanceBalance;
                var balanceBroughtForward = cashInHand.TrialBalanceBalance + cashInVault.TrialBalanceBalance; // adjust if you use another rule

                return Json(
                    new
                    {
                        success = true,
                        branchId,
                        cashInHandBalance,
                        cashInVaultBalance,
                        balanceBroughtForward
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
              

                return Json(
                    new
                    {
                        success = false,
                        message = "An error occurred while retrieving balances. Please try again."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        private async Task GetValues()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetVaults();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                await GetValues();
                return PartialView(partialView, new Vault());
            }
            else
            {
                await GetValues();
                ViewBag.Key = KEY;
                var Vault = await _services.GetVault(KEY);
                Vault.AddVaultCommand=_services.MapVaultToAddVaultCommand(Vault);
                return PartialView(partialView, Vault);

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetVault(string Key)
        {
            var data = await _services.GetVault(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetVaultPartialView(string Key)
        {
            var data = await _services.GetVault(Key);
            if (data == null)
            {
                return HttpNotFound();
            }

            return PartialView("_VaultDetailsPartial", data);
        }
        public async Task<ActionResult> GetBranch(string Key)
        {
            var branch = await _branchServices.GetBranch(Key);
            return Json(branch, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key != null)
            {
                var listing = await _branchServices.GetBranchesByBankId(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }

}