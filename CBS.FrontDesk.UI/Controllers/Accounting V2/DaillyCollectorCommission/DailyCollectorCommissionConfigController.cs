using CBS.BusinessService.Accounting_V2.DailyCollectorCommission;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.DaillyCollectorCommission
{
 

   
        [CheckSessionTimeOut]
        public class DailyCollectorCommissionConfigController : BaseController
        {
            private readonly DailyCollectorCommissionConfigService _configService;
            private readonly BranchServices _branchServices;
        private readonly ManualDailyCollectionService _manualService;

        public DailyCollectorCommissionConfigController(
                DailyCollectorCommissionConfigService configService,
                BranchServices branchServices, ManualDailyCollectionService manualDailyCollectionService)
            {
                _configService = configService;
                _branchServices = branchServices;
                _manualService = manualDailyCollectionService
          ;
            }

            public async Task<ActionResult> Index()
            {
                await LoaderViewBagData();
                return View(new DailyCollectorCommissionShareConfig());
            }

            [HttpGet]
            public async Task<ActionResult> List()
            {
                await LoaderViewBagData();
                return View();
            }

            private async Task LoaderViewBagData()
            {
                var branches = await _branchServices.GetBranches();
                ViewBag.Branches = branches;

        }

        [HttpPost]
        public async Task<JsonResult> LoadConfigData(DaillycollectorCommissionConfigQuery query)
        {
            try
            {
                var data = await _configService.GetConfigDataTableAsync(query);
                var configs = JsonConvert.DeserializeObject<List<DailyCollectorCommissionShareReponse>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = configs
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.dataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }



        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
            {
                await LoaderViewBagData();

                if (path == "list")
                {
                    var data = await _configService.GetAllConfigsAsync();
                    return PartialView(partialView, data);
                }
                else if (path == "new")
                {
                    return PartialView(partialView ?? "_CommissionConfigForm", new DailyCollectorCommissionShareConfig());
                }
                else
                {
                    var data = await _configService.GetConfigByIdAsync(KEY);
                    return PartialView(partialView ?? "_CommissionConfigDetails", data);
                }
            }

            [HttpPost]
            public async Task<ActionResult> CreateOrUpdate(DailyCollectorCommissionShareConfig model)
            {
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    if (!ModelState.IsValid)
                        return Json(new { success = false, message = "Validation failed." });

                    var result = await _configService.CreateConfigAsync(model);
                    return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
                }
                else
                {
                    return await Update(model);
                }
            }

            [HttpPost]
             public async Task<ActionResult> Update(DailyCollectorCommissionShareConfig model)
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _configService.UpdateConfigAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _configService.DeleteConfigAsync(KEY);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
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
    }

}