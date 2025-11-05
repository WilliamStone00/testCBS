using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.LiaisonAccount2;
using CBS.BusinessService.AccountingV2.LiaisonAccountConfiguration;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonAccountConfiguration;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.AccountingV2.LiaisonAccountConfiguration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.LiaisonAccount2
{
    public class LiaisonMappingController : BaseController
    {
        private readonly LiaisonMappingService _liaisonMappingService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        //private readonly BranchCashConfigService _branchCashConfig;

        public LiaisonMappingController(
            LiaisonMappingService liaisonMappingService,
            BranchServices branchServices,
            BranchAccountService branchAccountService
            //BranchCashConfigService branchCashConfig
            )
        {
			//_branchCashConfig = branchCashConfigLiaisonMappingModel
			_liaisonMappingService = liaisonMappingService;
            _branchServices = branchServices;
            _branchAccountService = branchAccountService;
        }

        public async Task<ActionResult> Index(string Branchid)
        {
            await PopulateViewBags(Branchid);
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadLiaisonMappingsData(GetLiaisonMappingsDataTableQuery query)
        {
            try
            {
                var dataTable = await _liaisonMappingService.GetDataTableAsync(query);

                var mappingList = JsonConvert.DeserializeObject<
                    List<LiaisonMapping>
                >(JsonConvert.SerializeObject(dataTable.data));

                return Json(new
                {
                    draw = dataTable.Options?.draw ?? "1",
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = mappingList,
                    success = true,
                    message = "Display DataTable for Liaison Mapping successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error loading liaison mapping data." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLiaisonMappingFormPartial(string id = null, string Branchid = null)
        {
            try
            {
                LiaisonMapping model;

                if (!string.IsNullOrEmpty(id))
                {
                    var existingMapping = await _liaisonMappingService.GetLiaisonMappingAsync(id);
                    model = new LiaisonMapping
                    {
                        Id = existingMapping.Id,
                        BranchId = existingMapping.BranchId,
                        CounterpartyBranchId = existingMapping.CounterpartyBranchId,
                        DueFromAssetAccountId = existingMapping.DueFromAssetAccountId,
                        DueToLiabilityAccountId = existingMapping.DueToLiabilityAccountId
                    };
                }
                else
                {
                    model = new LiaisonMapping();
                }

                await PopulateViewBags(Branchid);
                return PartialView("_LiaisonMappingForm", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading form." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(CreateOrUpdateLiaisonMappingCommand model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data." });
            }

            try
            {
                var result = await _liaisonMappingService.CreateOrUpdateLiaisonMappingAsync(model);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving liaison mapping." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var result = await _liaisonMappingService.DeleteLiaisonMappingAsync(id);
                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting liaison mapping." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAssetAccounts(string branchId)
        {
            try
            {
                // Call branch account service to get all accounts for the branch
                var branchAccounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);

                // Filter by Class property - looking for asset classes
                // Common asset classes: ASSET, CURRENT_ASSET, FIXED_ASSET, etc.
                var assetClasses = new[] { "ASSET", "CURRENT_ASSET", "FIXED_ASSET", "ASSETS" };
                var assetAccounts = branchAccounts.Where(ba =>
                    assetClasses.Contains(ba.Class?.ToUpper()) ||
                    (ba.Class?.ToUpper().Contains("ASSET") ?? false)
                ).ToList();

                return Json(assetAccounts.Select(a => new
                {
                    Id = a.Id,
                    Name = a.Name // Already formatted as "[Code] - Name" by the service
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLiabilityAccounts(string branchId)
        {
            try
            {
                // Call branch account service to get all accounts for the branch
                var branchAccounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);

                // Filter by Class property - looking for liability classes
                // Common liability classes: LIABILITY, CURRENT_LIABILITY, LONG_TERM_LIABILITY, etc.
                var liabilityClasses = new[] { "LIABILITY", "CURRENT_LIABILITY", "LONG_TERM_LIABILITY", "LIABILITIES" };
                var liabilityAccounts = branchAccounts.Where(ba =>
                    liabilityClasses.Contains(ba.Class?.ToUpper()) ||
                    (ba.Class?.ToUpper().Contains("LIABILITY") ?? false)
                ).ToList();

                return Json(liabilityAccounts.Select(a => new
                {
                    Id = a.Id,
                    Name = a.Name // Already formatted as "[Code] - Name" by the service
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        private async Task PopulateViewBags(string branchId)
        {
            // Always populate branches for both filter and modal
            ViewBag.Branches = await _branchServices.GetBranches();

            // Initialize defaults to avoid ArgumentNullException in view
            ViewBag.BranchAccounts = new List<object>();
            ViewBag.DueFromAssetAccount = new List<object>();
            ViewBag.DueToLiability = new List<object>();

            if (!string.IsNullOrEmpty(branchId))
            {
                var branchAccounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync(branchId);

                // Asset accounts for "Due From"
                var assetClasses = new[] { "ASSET", "CURRENT_ASSET", "FIXED_ASSET", "ASSETS" };
                var dueFromAssetAccounts = branchAccounts
                    .Where(a => assetClasses.Contains(a.Class?.ToUpper()) || (a.Class?.ToUpper().Contains("ASSET") ?? false))
                    .ToList();

                // Liability accounts for "Due To"
                var liabilityClasses = new[] { "LIABILITY", "CURRENT_LIABILITY", "LONG_TERM_LIABILITY", "LIABILITIES" };
                var dueToLiabilityAccounts = branchAccounts
                    .Where(a => liabilityClasses.Contains(a.Class?.ToUpper()) || (a.Class?.ToUpper().Contains("LIABILITY") ?? false))
                    .ToList();

                ViewBag.BranchAccounts = branchAccounts;
                ViewBag.DueFromAssetAccount = dueFromAssetAccounts;
                ViewBag.DueToLiability = dueToLiabilityAccounts;
            }
        }



        [HttpGet]
        public async Task<ActionResult> AjaxLoader(string key, string type = "asset")
        {
            try
            {
                if (string.IsNullOrEmpty(key) || key == "null")
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                // Call branch account service
                var branchAccounts = await _branchAccountService.GetBranchAccountsByBranchIdAsync(key);

                IEnumerable<object> filteredAccounts;

                if (type.ToLower() == "asset")
                {
                    var assetClasses = new[] { "ASSET", "CURRENT_ASSET", "FIXED_ASSET", "ASSETS" };
                    filteredAccounts = branchAccounts
                        .Where(ba => assetClasses.Contains(ba.Class?.ToUpper()) ||
                                   (ba.Class?.ToUpper().Contains("ASSET") ?? false))
                        .Select(a => new
                        {
                            Id = a.Id,
                            Name = a.Name
                        });
                }
                else
                {
                    var liabilityClasses = new[] { "LIABILITY", "CURRENT_LIABILITY", "LONG_TERM_LIABILITY", "LIABILITIES" };
                    filteredAccounts = branchAccounts
                        .Where(ba => liabilityClasses.Contains(ba.Class?.ToUpper()) ||
                                   (ba.Class?.ToUpper().Contains("LIABILITY") ?? false))
                        .Select(a => new
                        {
                            Id = a.Id,
                            Name = a.Name
                        });
                }

                return Json(filteredAccounts.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCounterpartyBranches(string branchId)
        {
            try
            {
                // Get all branches except the current one for counterparty selection
                var allBranches = await _branchServices.GetBranches();

                var counterpartyBranches = allBranches
                    .Where(b => b.Id != branchId)
                    .Select(b => new
                    {
                        Id = b.Id,
                        Name = b.Name
                    }).ToList();

                return Json(counterpartyBranches, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}