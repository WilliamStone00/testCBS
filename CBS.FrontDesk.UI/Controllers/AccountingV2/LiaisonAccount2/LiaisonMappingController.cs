using CBS.BusinessService.Accounting;
using CBS.BusinessService.AccountingV2.BranchCashConfig;
using CBS.BusinessService.AccountingV2.LiaisonAccount2;
using CBS.BusinessService.AccountingV2.LiaisonAccountConfiguration; // adjust namespace as needed
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.LiaisonAccount2
{
    public class LiaisonMappingController : BaseController
    {
        private readonly LiaisonMappingService _liaisonMappingService;
        private readonly BranchServices _branchServices;
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly BranchCashConfigService _branchCashConfig;


        public LiaisonMappingController(
            LiaisonMappingService liaisonMappingService,
            BranchServices branchServices,
            ChartOfAccountServices chartOfAccountServices,
            BranchCashConfigService branchCashConfig

            )
        {
            _branchCashConfig = branchCashConfig;
            _liaisonMappingService = liaisonMappingService;
            _branchServices = branchServices;
            _chartOfAccountServices = chartOfAccountServices;
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
                    List<CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping.LiaisonMappingModel>
                >(JsonConvert.SerializeObject(dataTable.data));

                return Json(new
                {
                    draw = query.Options?.draw ?? "1",
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = mappingList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error loading liaison mapping data." });
            }
        }


        [HttpGet]
        public async Task<ActionResult> GetLiaisonMappingFormPartial(string id = null,string Branchid=null)
        {
            try
            {
                LiaisonMappingModel model;

                if (!string.IsNullOrEmpty(id))
                {
                    var existingMapping = await _liaisonMappingService.GetLiaisonMappingAsync(id);
                    model = new LiaisonMappingModel
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
                    model = new LiaisonMappingModel();
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
                var accounts = await _chartOfAccountServices.GetAssetAccountsByBranch(branchId);
                return Json(accounts.Select(a => new
                {
                    a.Id,
                    Name = $"{a.AccountNumber} - {a.LabelEn}"
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLiabilityAccounts(string branchId)
        {
            try
            {
                var accounts = await _chartOfAccountServices.GetLiabilityAccountsByBranch(branchId);
                return Json(accounts.Select(a => new
                {
                    a.Id,
                    Name = $"{a.AccountNumber} - {a.LabelEn}"
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        private async Task PopulateViewBags(string branchId)
        {
            ViewBag.Branches = await _branchServices.GetBranches();
           ViewBag.DueFromAssetAccount = await _branchCashConfig.GetBranchesAsync(branchId);
           ViewBag.DueToLiability = await _branchCashConfig.GetBranchesAsync(branchId);
            
        }


        [HttpGet]
        public async Task<ActionResult> AjaxLoader(string key, string type = "asset")
        {
            try
            {
                // If no branch selected, return empty list
                if (string.IsNullOrEmpty(key) || key == "null")
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                IEnumerable<ChartOfAccount> accounts;

                if (type.ToLower() == "asset")
                {
                    accounts = await _chartOfAccountServices.GetAssetAccountsByBranch(key);
                }
                else
                {
                    accounts = await _chartOfAccountServices.GetLiabilityAccountsByBranch(key);
                }

                var result = accounts.Select(a => new
                {
                    Id = a.Id,
                    Name = $"{a.AccountNumber} - {a.LabelEn}"
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return empty list on error
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCounterpartyBranches(string branchId)
        {
            try
            {
                // if empty, return all branches (or an empty list depending on desired behavior)
                var branches = await _branchCashConfig.GetBranchesAsync(branchId);

                // Map to a small JSON-friendly object for dropdown consumption
                var result = branches.Select(b => new
                {
                    Id = b.Id,
                    Name = b.Name
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log exception if you have a logging mechanism
                // Return an empty array so UI remains stable
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

    }
}