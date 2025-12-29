using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.LoanConf.PCMFStructure;
using CBS.FrontDesk.Data.Entity.Overdraft;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PcmfPopulation = CBS.FrontDesk.Data.Entity.LoanConf.PcmfPopulation;
using PcmfPurposeKey = CBS.FrontDesk.Data.Entity.LoanConf.PCMFStructure.PcmfPurposeKey;

namespace CBS.FrontDesk.UI.Controllers.Configuration
{
    [CheckSessionTimeOutAttribute]
    public class LoanProductController : BaseController
    {
        // GET: LoanProduct
        private readonly LoanProductServices _LoanProductServices;
        private readonly LoanProductCategoryServices _loanProductCategoryServices;
        private readonly LoanTermServices _loanTermServices;
        private readonly PenaltyServices _PenaltyServices;
        private readonly AffiliateAccountService _accountingServices;
        public LoanProductController(LoanProductServices LoanProductServices, AffiliateAccountService accountingServices, PenaltyServices penaltyServices, LoanProductCategoryServices loanProductCategoryServices, LoanTermServices loanTermServices)
        {
            _LoanProductServices = LoanProductServices;
            _accountingServices = accountingServices;
            _PenaltyServices = penaltyServices;
            _loanProductCategoryServices = loanProductCategoryServices;
            _loanTermServices = loanTermServices;
        }
        public async Task<ActionResult> Index()
        {
            await GetValues();
            return View(new PCMFLoanProductManagementObjects());
        }
        public ActionResult ProductListingPolicySet()
        {
            return View();
        }

        public async Task<ActionResult> Policy(string Key)
        {
            await GetValues();
            var LoanProduct = await _LoanProductServices.GetLoanProduct(Key);
            var loanProductObject = new LoanProductObject();
            loanProductObject.UpdateLoanProductCommand = _LoanProductServices.ProductMappingToUpdateObject(LoanProduct, "N/A", "N/A");
            return View(loanProductObject);

        }

        // ✅ ONE POST: create if Id empty, update if Id present
        [HttpPost]
        public async Task<ActionResult> CreateOrUpdateSimple(PCMFLoanProductManagementObjects model)
        {
            var cmd = model?.CreateOrUpdate;

            if (cmd == null)
                return Json(new { success = false, status = false, message = "Invalid request payload." });

            // ✅ basic validation (keep it strict + user-friendly)
            if (string.IsNullOrWhiteSpace(cmd.ProductCode))
                return Json(new { success = false, status = false, message = "Product Code is required." });

            if (string.IsNullOrWhiteSpace(cmd.ProductName))
                return Json(new { success = false, status = false, message = "Product Name is required." });

            if (string.IsNullOrWhiteSpace(cmd.LoanTargetId))
                return Json(new { success = false, status = false, message = "Loan Target is required." });

            if (string.IsNullOrWhiteSpace(cmd.PcmfLoanPurposeId))
                return Json(new { success = false, status = false, message = "PCMF Loan Purpose is required." });

            // LoanTermId can be empty ONLY for overdraft
            // If you want to enforce: overdraft => LoanTermId empty
            // else => required. (Front UI already helps)
            // if (!isOverdraft && string.IsNullOrWhiteSpace(cmd.LoanTermId)) ...

            try
            {
                // ✅ CREATE
                if (string.IsNullOrWhiteSpace(cmd.Id))
                {
                    var data = await _LoanProductServices.CreatePcmfLoanProductSimple(cmd);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }

                // ✅ UPDATE
                var update = await _LoanProductServices.UpdatePcmfLoanProductSimple(cmd);
                return Json(new { success = update.Result, status = update.MessageStatus, message = Messaging.MessageResult(update) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = ex.Message });
            }
        }



        //Policy
        public async Task<ActionResult> LoanAccountMapping()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanProductObject model, PCMFLoanProductManagementObjects model2)
        {
            if (model.ServiceOption == "insert")
            {
                if (ModelState.IsValid)
                {
                    var data = await _LoanProductServices.Create(model.AddLoanProductCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else
                {
                    var errorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Where(e => e.ErrorMessage != null)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    // Convert the list of error messages to a single string with each message on a new line
                    string errorMessage = string.Join("\n", errorMessages);

                    // Pass the error message as the message
                    return Json(new { success = false, status = false, message = errorMessage });
                }
            }
            else
            {
                return await Update(model2);
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> Update(LoanProductObject model)
        //{



        //    if (model.ServiceOption == "set_penalty")
        //    {
        //        var data = await _PenaltyServices.Create(model.Penalty);
        //        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        //    }
        //    else
        //    {
        //        model.UpdateLoanProductCommand.ServiceOption = model.ServiceOption;
        //        if (model.ServiceOption == "product")
        //        {
        //            model.UpdateLoanProductCommand.ProductCode = model.AddLoanProductCommand.ProductCode;
        //            model.UpdateLoanProductCommand.TargetType = model.AddLoanProductCommand.TargetType;
        //            model.UpdateLoanProductCommand.ProductName = model.AddLoanProductCommand.ProductName;
        //            model.UpdateLoanProductCommand.Id = model.AddLoanProductCommand.Id;
        //            model.UpdateLoanProductCommand.Description = model.AddLoanProductCommand.Description;
        //            model.UpdateLoanProductCommand.ActiveStatus = model.AddLoanProductCommand.ActiveStatus;
        //            model.UpdateLoanProductCommand.ServiceOption = model.ServiceOption;
        //            model.UpdateLoanProductCommand.LoanTermId = model.AddLoanProductCommand.LoanTermId;
        //            model.UpdateLoanProductCommand.LoanProductCategoryId = model.AddLoanProductCommand.LoanProductCategoryId;
        //            model.UpdateLoanProductCommand.IsProductWithSavingFacilities = model.AddLoanProductCommand.IsProductWithSavingFacilities;
        //            model.UpdateLoanProductCommand.IsMortgage = model.AddLoanProductCommand.IsMortgage;

        //        }
        //        else if (model.ServiceOption == "duration")
        //        {
        //            var selectedLoanTerm = await _loanTermServices.GetLoanTerm(model.UpdateLoanProductCommand.LoanTermId); // Fetch LoanTerm details
        //            if (selectedLoanTerm == null)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    status = false,
        //                    message = "The selected loan term is invalid. Please select a valid loan term and try again Or Set the loan term."
        //                });
        //            }

        //            // Validate the minimum duration period
        //            if (model.UpdateLoanProductCommand.MinimumDurationPeriod < selectedLoanTerm.MinInMonth ||
        //                model.UpdateLoanProductCommand.MinimumDurationPeriod > selectedLoanTerm.MaxInMonth)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    status = false,
        //                    message = $"The provided Minimum Duration Period of {model.UpdateLoanProductCommand.MinimumDurationPeriod} months does not fall within the allowable range of {selectedLoanTerm.MinInMonth} to {selectedLoanTerm.MaxInMonth} months for the selected loan term '{selectedLoanTerm.Name}'. Please review and adjust accordingly."
        //                });
        //            }

        //            // Validate the maximum duration period
        //            if (model.UpdateLoanProductCommand.MaximumDurationPeriod < selectedLoanTerm.MinInMonth ||
        //                model.UpdateLoanProductCommand.MaximumDurationPeriod > selectedLoanTerm.MaxInMonth)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    status = false,
        //                    message = $"The provided Maximum Duration Period of {model.UpdateLoanProductCommand.MaximumDurationPeriod} months exceeds the permissible range of {selectedLoanTerm.MinInMonth} to {selectedLoanTerm.MaxInMonth} months for the selected loan term '{selectedLoanTerm.Name}'. Please review and adjust your input."
        //                });
        //            }

        //            // Validate that minimum is not greater than maximum   ML Commercial Real Estate Loan E
        //            if (model.UpdateLoanProductCommand.MinimumDurationPeriod > model.UpdateLoanProductCommand.MaximumDurationPeriod)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    status = false,
        //                    message = $"The Minimum Duration Period ({model.UpdateLoanProductCommand.MinimumDurationPeriod} months) cannot be greater than the Maximum Duration Period ({model.UpdateLoanProductCommand.MaximumDurationPeriod} months) for the selected loan term '{selectedLoanTerm.Name}'. Please ensure the values are entered correctly."
        //                });
        //            }
        //        }


        //        var data = await _LoanProductServices.Update(model.UpdateLoanProductCommand);
        //        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        //    }

        //}

        [HttpPost]
        public async Task<ActionResult> Update(PCMFLoanProductManagementObjects model)
        {
            var policy = model.Policy;

            //if (model.ServiceOption == "set_penalty")
            //{
            //    // Standalone penalty creation logic (from original snippet)
            //    var data = await _PenaltyServices.Create(model.Product.Policy.PenaltyId); // Adjusted to match likely structure
            //    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            //}
            //else
            //{
            // Handle specific validations and mappings based on the Accordion/Section submitted
            switch (model.ServiceOption)
            {


                case "loan_range":
                    // Validation: Min vs Max
                    if (policy.LoanMinimumAmount > policy.LoanMaximumAmount)
                    {
                        return Json(new { success = false, status = false, message = "Minimum Loan Amount cannot be greater than Maximum Loan Amount." });
                    }

                    // Map Policy values to Command (or Service uses Policy object directly)
                    // Assuming Command has these properties or Service accepts the Policy object
                    // For this example, we assume we need to hydrate the Command or a specific DTO property
                    break;

                case "interest":
                    // Validation: Min vs Max Interest
                    if (policy.MinimumInterestRate > policy.MaximumInterestRate)
                    {
                        return Json(new { success = false, status = false, message = "Minimum Interest Rate cannot be greater than Maximum Interest Rate." });
                    }

                    // Validation: Waiver
                    if (policy.IsInterestWaiverApplied && (policy.MinimumInterestWaiver > policy.MaximumInterestWaiver))
                    {
                        return Json(new { success = false, status = false, message = "Minimum Interest Waiver cannot be greater than Maximum Interest Waiver." });
                    }
                    break;

                case "repayment":
                    // Handle Repayment Cycles (ListBox)
                    if (model.Product.RepaymentCycles == null || !model.Product.RepaymentCycles.Any())
                    {
                        // Optional: Validation if at least one cycle is required
                        // return Json(new { success = false, message = "Please select at least one repayment cycle." });
                    }
                    // Map list to command if needed
                    break;

                case "charges": // Corresponds to Defaulted Loan Settings
                                // Logic for "IsChargesApplied" and penalties is handled here
                    break;

                case "gurantee": // Corresponds to Collateral Check List
                                 // Logic for collateral requirements, guarantors, etc.
                    if (policy.ShorteeMustHaveFundToGuranteeLoan && policy.MinimumPercentageCoverageOfShortee <= 0)
                    {
                        return Json(new { success = false, status = false, message = "Please specify a valid Minimum Percentage Coverage for the Shortee." });
                    }
                    break;

                case "mortgage":
                    // Mortgage specific logic
                    if (policy.IsMortgage && policy.MaxLoanToValueRatio > 1.0M)
                    {
                        return Json(new { success = false, status = false, message = "LTV Ratio is usually expressed as a decimal (e.g., 0.80). Values > 1 imply > 100%." });
                    }
                    break;

                case "duration":

                    if (!string.IsNullOrEmpty(policy.TermId))
                    {
                        var selectedLoanTerm = await _loanTermServices.GetLoanTerm(policy.TermId);
                        if (selectedLoanTerm == null)
                        {
                            return Json(new { success = false, status = false, message = "Invalid loan term selected." });
                        }

                        // Validate ranges against the Term Configuration
                        if (policy.MinimumDurationPeriod < selectedLoanTerm.MinInMonth || policy.MinimumDurationPeriod > selectedLoanTerm.MaxInMonth)
                        {
                            return Json(new { success = false, status = false, message = $"Minimum Duration ({policy.MinimumDurationPeriod}) is outside the allowable range ({selectedLoanTerm.MinInMonth}-{selectedLoanTerm.MaxInMonth})." });
                        }

                        if (policy.MaximumDurationPeriod < selectedLoanTerm.MinInMonth || policy.MaximumDurationPeriod > selectedLoanTerm.MaxInMonth)
                        {
                            return Json(new { success = false, status = false, message = $"Maximum Duration ({policy.MaximumDurationPeriod}) is outside the allowable range ({selectedLoanTerm.MinInMonth}-{selectedLoanTerm.MaxInMonth})." });
                        }

                        if (policy.MinimumDurationPeriod > policy.MaximumDurationPeriod)
                        {
                            return Json(new { success = false, status = false, message = "Minimum Duration cannot be greater than Maximum Duration." });
                        }
                    }
                    break;
            }


            // 3. Execute Update
            // We pass the ServiceOption so the generic Update method knows which fields specifically to update 
            // (This avoids overwriting other sections with nulls if partial updates are performed).
            // Note: We are passing 'model.Policy' as well, assuming the Service signature accepts the policy 
            // or the Command object has been extended to include Policy properties.

            // If your service only accepts 'command', make sure to map 'policy' fields to 'command' fields above.
            // Assuming signature: Update(CreateLoanProductCommand command, LoanProductPolicy policy = null, string serviceOption = "")

            var data = await _LoanProductServices.Update(model.Policy, model.ServiceOption);

            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }

        [HttpGet]
        public async Task<ActionResult> GetPurposesByTerm(string termId, string loanFacility = "Classic", string lang = "en")
        {
            var data = await _LoanProductServices.GetPcmfLoanProductUiCatalogQuery(
                new GetPcmfLoanProductUiCatalogQuery
                {
                    LoanTermId = termId,
                    LoanFacility = loanFacility,
                    Lang = lang
                });

            return Json(data?.Purposes ?? new List<UiOptionDto>(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetTargetsForSelection(string purposeId, string termId, string loanFacility = "Classic", string lang = "en")
        {
            var data = await _LoanProductServices.GetPcmfLoanProductUiCatalogQuery(
                new GetPcmfLoanProductUiCatalogQuery
                {
                    LoanTermId = termId,
                    PcmfLoanPurposeId = purposeId,
                    LoanFacility = loanFacility,
                    Lang = lang
                });

            return Json(data?.Targets ?? new List<UiOptionDto>(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetPcmfPreview(string termId, string purposeId, string targetId, string loanFacility = "Classic", string lang = "en")
        {
            var data = await _LoanProductServices.GetPcmfLoanProductUiCatalogQuery(
                new GetPcmfLoanProductUiCatalogQuery
                {
                    LoanTermId = termId,
                    PcmfLoanPurposeId = purposeId,
                    LoanTargetId = targetId,
                    LoanFacility = loanFacility,
                    Lang = lang
                });

            return Json(data?.Preview, JsonRequestBehavior.AllowGet);
        }





        // ✅ Used by Index ajax loaders (list / new / edit)
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
         {
            await GetValues();
            partialView = string.IsNullOrWhiteSpace(partialView) ? "_Data" : partialView;
            path = path?.Trim();

            // -----------------------------
            // LIST
            // -----------------------------
            if (string.Equals(path, "list", StringComparison.OrdinalIgnoreCase))
            {
                var products = await _LoanProductServices.PCMFGetLoanProducts(); // List<PCMFLoanProduct>

                var vm = new PCMFLoanProductManagementObjects
                {
                    Products = products?.ToList() ?? new List<PCMFLoanProduct>()
                };

                return PartialView(partialView, vm);
            }

            // -----------------------------
            // NEW
            // -----------------------------
            if (string.Equals(path, "new", StringComparison.OrdinalIgnoreCase))
            {

                var vm = new PCMFLoanProductManagementObjects
                {
                    CreateOrUpdate = new CreateLoanProductCommand
                    {
                        ActiveStatus = true
                    }
                };

                return PartialView(partialView, vm);
            }

            // -----------------------------
            // SIMPLE EDIT (same form)
            // -----------------------------
            if (string.Equals(path, "simple_edit", StringComparison.OrdinalIgnoreCase))
            {
                //await GetValues();

                if (string.IsNullOrWhiteSpace(KEY))
                    return Content("Invalid product key.");

                var p = await _LoanProductServices.PcmfGetLoanProduct(KEY);
                if (p == null) return Content("Product not found.");

                var vm = new PCMFLoanProductManagementObjects
                {
                    Product = p,
                    CreateOrUpdate = _LoanProductServices.MapToCreateCommand(p)
                };

                return PartialView(partialView, vm);
            }

            // -----------------------------
            // MANAGE: requires KEY
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(path) &&
                (string.Equals(path, "preview", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(path, "account_mapping", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(path, "policy", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(path, "penalties", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(path, "overdraft", StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrWhiteSpace(KEY))
                    return Content("Invalid product key.");

                // Load product (base)
                var p = await _LoanProductServices.PcmfGetLoanProduct(KEY);
                if (p == null) return Content("Product not found.");

                var vm = new PCMFLoanProductManagementObjects
                {
                    // Ensure Product is never null
                    Product = p,
                    // Ensure CreateOrUpdate is always initialized
                    CreateOrUpdate = _LoanProductServices.MapToCreateCommand(p)
                      ?? new CreateLoanProductCommand()
                };


                // ✅ Preview: only product data
                if (string.Equals(path, "preview", StringComparison.OrdinalIgnoreCase))
                {
                    //await GetValues();
                    return PartialView(partialView, vm);
                }

                // ✅ Accounting Mapping
                if (string.Equals(path, "account_mapping", StringComparison.OrdinalIgnoreCase))
                {
                    //await GetValues();
                    // Only load what mapping needs
                    await LoadChartOfAccounts();

                    // Load mapping (create if missing)
                    vm.AccountingProfile = p.AccountingMapping ?? new LoanProductAccountingProfile { LoanProductId = p.Id };
                    // Snapshot (optional but recommended)
                    vm.AccountingProfile.PcmfBaseCode = p.PcmfBaseCode.Value;
                    vm.AccountingProfile.PcmfSection = p.PcmfSection;
                    vm.AccountingProfile.PcmfGroupCode = p.PcmfGroupCode.Value;
                    vm.AccountingProfile.PcmfPopulation = p.PcmfPopulation;
                    return PartialView(partialView, vm);
                }

                // ✅ Policy
                if (string.Equals(path, "policy", StringComparison.OrdinalIgnoreCase))
                {
                   // await GetValues();
                    vm.Policy = p.Policy ?? new LoanProductPolicy { LoanProductId = p.Id, TermId = p.LoanTermId };
                    vm.Policy.LoanProductId = p.Id;
                    return PartialView(partialView, vm);
                }

                // ✅ Penalties (if you have a penalties object in wrapper)
                if (string.Equals(path, "penalties", StringComparison.OrdinalIgnoreCase))
                {
                    // vm.Penalties = await _LoanProductServices.GetLoanProductPenalties(p.Id) ?? new LoanProductPenaltyProfile();
                    return PartialView(partialView, vm);
                }

                // ✅ Overdraft Facility (only if OD)
                if (string.Equals(path, "overdraft", StringComparison.OrdinalIgnoreCase))
                {
                    vm.OverdraftFacilityConfig = p.OverdraftFacilityConfig
                                              ?? new OverdraftFacilityConfig();

                    return PartialView(partialView, vm);
                }
            }

            // fallback
           // await GetValues();
            return PartialView(partialView, new PCMFLoanProductManagementObjects());
        }


        // -----------------------------
        // Helper: load chart of accounts for select2
        // -----------------------------
        private async Task LoadChartOfAccounts()
        {
            var profiles = await _accountingServices.GetAllAffiliateAccounts();
            ViewBag.ChartOfAccounts = profiles;
        }
        [HttpPost]
        public async Task<ActionResult> SaveAccountingMapping(PCMFLoanProductManagementObjects model, string LoanProductId)
        {
            try
            {
                if (model?.AccountingProfile == null)
                    return Json(new { success = false, message = "Invalid mapping payload." });
                model.AccountingProfile.LoanProductId = LoanProductId;

                // TODO: save mapping
                var data = await _LoanProductServices.UpdateAccountingProfileSimple(model.AccountingProfile);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to save accounting mapping.", error = ex.Message });
            }
        }

        public async Task<bool> GetValues()
        {
            var agreggates = await _LoanProductServices.GetAgreggates();
            var productEnumAgregates = await _LoanProductServices.GetLoanProductEnumAggregates();
            var loanProductCategories = await _loanProductCategoryServices.GetLoanProductCategorys();
            var loanTerms = await _loanTermServices.GetLoanTerms();

            ViewBag.SheduleTypes = _LoanProductServices.GetScheduleTypes();
            ViewBag.Penalties = productEnumAgregates.Penalties;
            ViewBag.ProductCategories = loanProductCategories;
            ViewBag.LoanTerms = loanTerms;
            ViewBag.Fees = agreggates.Fees;
            ViewBag.DocumentPackes = agreggates.DocumentPackes;
            ViewBag.GuranteePackes = agreggates.GuranteePackes;
            ViewBag.Taxes = agreggates.Taxes;
            ViewBag.FundingLines = agreggates.FundingLines;
            ViewBag.InstallmentTypes = agreggates.InstallmentTypes;
            ViewBag.CalculateInterestOn = productEnumAgregates.CalculateInterestOn;
            ViewBag.RepaymentCycles = productEnumAgregates.RepaymentCycles;
            ViewBag.LoanInterestMethods = productEnumAgregates.LoanInterestMethods;
            ViewBag.LoanStatuses = productEnumAgregates.LoanStatuses;
            ViewBag.LoanInterestTypes = productEnumAgregates.LoanInterestTypes;
            ViewBag.LoanInterestPeriods = productEnumAgregates.LoanInterestPeriods;
            ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
            ViewBag.RefundOrders = productEnumAgregates.RefundOrders;
            ViewBag.PenaltyTypes = productEnumAgregates.PenaltyTypes;
            ViewBag.YesOrNo = productEnumAgregates.YesOrNo;
            ViewBag.LoanCategories = productEnumAgregates.LoanCategories;
            //ViewBag.LoanTargets = productEnumAgregates.LoanTargets;
            ViewBag.LoanTargets = agreggates.LoanTargetCatalogs;
            ViewBag.PcmfLoanPurposes = agreggates.PcmfLoanPurposes;
            // Accounting Profiles
            var profiles = await _accountingServices.GetAllAffiliateAccounts();
            ViewBag.AccountingProfiles = profiles;
            ViewBag.ChartOfAccounts = profiles;

            return true;
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _LoanProductServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}