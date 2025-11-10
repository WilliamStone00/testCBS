using CBS.BusinessService.Accounting_V2;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.DaillyCollectorCommission
{
    //[CheckSessionTimeOut]
    public class CollectorCommissionController : Controller
    {
        private readonly ManualDailyCollectionService _manualService;
        private readonly CollectorCommissionService _collectorCommissionService;
        private readonly BranchServices _branchServices;
        private readonly ChartOfAccountsV2Service _chartOfAccountsV;

        /// <summary>
        /// Injects the required CollectorCommissionController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public CollectorCommissionController(ManualDailyCollectionService manualDailyCollectionService, ChartOfAccountsV2Service chartOfAccountsV2Service, CollectorCommissionService collectorCommissionService, BranchServices branchServices)
        {
            _collectorCommissionService = collectorCommissionService;
            _chartOfAccountsV = chartOfAccountsV2Service;
            _branchServices = branchServices;
            _manualService = manualDailyCollectionService;
        }

        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new CollectorComissionResponse());
        }


        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var pcmfs = await _chartOfAccountsV.GetAllPCMFAccounts();
            ViewBag.HoPcmfAccounts = pcmfs;
            return true;

        }

        // Note: use [FromBody] so model binder reads the JSON DataTables sends.
        [HttpPost]
        public async Task<JsonResult> DataTable(commisionQuery query)
        {
            try
            {
                var data = await _collectorCommissionService.CommisionDataTableAsync(query);

                var Affiliate = JsonConvert.DeserializeObject<List<DataTableResponse>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Affiliate
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.DataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CollectorComissionResponse model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _collectorCommissionService.CreateAsync(model);
            return Json(new { success = false, message = "No data returned from service." });
        }



        [HttpGet]
        public async Task<JsonResult> GetCollectorsByBranch(string branchId)
        {
            try
            {
                int order = 2;
                // This would call your service to get collectors by branch
                var collectors = await _manualService.GetCollectorsAsSelectListAsync(branchId, order);

                return Json(collectors, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetCommissionData(CollectorComissionResponse model)
        {
            try
            {
                var commissionData = await _collectorCommissionService.GetCommissionAsync(model);

                if (commissionData != null)
                {
                    return Json(new { success = true, data = commissionData });
                }
                else
                {
                    return Json(new { success = false, message = "No commission data found for the selected criteria." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult ExportCommission()
        {
            return PartialView("_ExportCommission");
        }

        [HttpGet]
        public ActionResult CommissionTreatment()
        {
            return PartialView("_CommissionTreatment", new CollectorComissionResponse());
        }

        [HttpPost]
        public async Task<JsonResult> ProcessPayment(CollectorComissionResponse model)
        {
            try
            {
                var result = await _collectorCommissionService.CreateAsync(model);
                return Json(new { success = false, message = "No data returned from service." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomerAccounts(string customerId)
        {
            try
            {
                var customerData = await _collectorCommissionService.GetCustomerAccountDropdownAsync(customerId);
                return Json(new
                {
                    success = true,
                    customer = customerData.CustomerDto,
                    accounts = customerData.AccountSelectList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}