// -----------------------------------------------------------------------------
// File: FeeConfigController.cs
// -----------------------------------------------------------------------------
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Configurations.ChequeNumber;
using CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Configurations.FeeConfiguration
{
   // [CheckSessionTimeOut]
    public class FeeConfigController : BaseController
    {
        private readonly FeeConfigService _feeConfigService;
        private readonly BranchServices _branchServices;
        private readonly MockFeeConfigService _mockFeeConfigService;

        public FeeConfigController(FeeConfigService feeConfigService, BranchServices branchServices, MockFeeConfigService mockFeeConfigService)
        {
            _feeConfigService = feeConfigService;
            _branchServices = branchServices;
            _mockFeeConfigService = mockFeeConfigService;
        }

        public async Task<ActionResult> Index()
       {
            await Loader();
            return View(new FeeConfig());
        }

        private async Task Loader()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            ViewBag.FeeTypes = await _feeConfigService.GetFeeTypesAsync(); // keep using mock for dropdowns by default
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await Loader();
            return View();
        }

        // Note: use [FromBody] so model binder reads the JSON DataTables sends.
        [HttpPost]
        public async Task<JsonResult> LoadfeeData(FeeConfigQuery query)
        {
            try
            {
                var data = await _feeConfigService.GetFeeDataTableAsync(query);

                var FeeConfig = JsonConvert.DeserializeObject<List<Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration.FeeConfig>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = FeeConfig
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,string path = null,string serviceOption = null)
        {
            await Loader(); 
            // ROUTE: Return summary list
            if (string.Equals(path, "list", StringComparison.OrdinalIgnoreCase))
            {
              // We just return the empty partial view structure
                return PartialView(partialView ?? "_FeeConfigList");
            }

            // ROUTE: Get by id (KEY expected)
            if (string.Equals(path, "get", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(KEY))
                    return new HttpStatusCodeResult(400, "KEY is required for 'get'.");

                FeeConfig model = null;

                // Prefer a dedicated GetById method if available
                // await _feeConfigService.GetConfigByIdAsync(KEY)
                try
                {
                    // If your service has GetConfigByIdAsync use it:
                    model = await _feeConfigService.GetByIdAsync(KEY);
                }
                catch (MissingMethodException)
                {
                    // Fallback: query all and find by Id (less efficient)
                    var all = await _feeConfigService.GetConfigsAsync();
                    model = all?.FirstOrDefault(m => string.Equals(m.id, KEY, StringComparison.OrdinalIgnoreCase));
                }

                if (model == null)
                    return HttpNotFound($"FeeConfig with id '{KEY}' not found.");

                // await Loader();
                return PartialView(partialView , model);
            }

            // ROUTE: Create (return a new FeeConfig prefilled if serviceOption provided)
            if (string.Equals(path, "_Create", StringComparison.OrdinalIgnoreCase))
            {
               // await Loader();

                FeeConfig model = null;
                if (!string.IsNullOrWhiteSpace(serviceOption))
                {
                    InitOptions options = null;
                    try { options = JsonConvert.DeserializeObject<InitOptions>(serviceOption); }
                    catch { options = new InitOptions(); }

                    bool isCentralized = options?.isCentralized ?? false;
                    string branchId = options?.branchId;
                    string feeType = options?.feeType;

                    model = new FeeConfig
                    {
                        isCentralized = isCentralized,
                        branchId = isCentralized ? null : branchId,
                        feeType = feeType,
                        acceptPercentage = true
                    };
                }

                // Ensure model not null
                if (model == null) model = new FeeConfig();

                return PartialView(partialView ?? "_Create", model);
            }

            // DEFAULT BEHAVIOUR:
            // If path is null or unknown, attempt: if KEY provided => get by id, else => create new
            //await Loader();
            FeeConfig defaultModel = null;

            if (!string.IsNullOrWhiteSpace(KEY))
            {
                try
                {
                    defaultModel = await _feeConfigService.GetByIdAsync(KEY);
                }
                catch (MissingMethodException)
                {
                    var all = await _feeConfigService.GetConfigsAsync();
                    defaultModel = all?.FirstOrDefault(m => string.Equals(m.id, KEY, StringComparison.OrdinalIgnoreCase));
                }

                if (defaultModel == null)
                    return HttpNotFound($"FeeConfig with id '{KEY}' not found.");
            }
            else if (!string.IsNullOrWhiteSpace(serviceOption))
            {
                // prefill new model from serviceOption when KEY not provided
                InitOptions opts = null;
                try { opts = JsonConvert.DeserializeObject<InitOptions>(serviceOption); }
                catch { opts = new InitOptions(); }

                defaultModel = new FeeConfig
                {
                    isCentralized = opts?.isCentralized ?? false,
                    branchId = (opts?.isCentralized ?? false) ? null : opts?.branchId,
                    feeType = opts?.feeType,
                    acceptPercentage = true
                };
            }

            if (defaultModel == null) defaultModel = new FeeConfig();
            return PartialView(partialView ?? "_Create", defaultModel);
        }

       

        [HttpGet]
        public async Task<JsonResult> GetAllFeeConfigs()
        {
            try
            {
                var data = await _feeConfigService.GetConfigsAsync();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(FeeConfig model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, status = "Failed", message = "Please fill all required fields." });
            }

            ExecutionMessages data;
            if (string.IsNullOrWhiteSpace(model.id))
            {
                data = await _feeConfigService.CreateAsync(model);
            }
            else
            {
                data = await _feeConfigService.UpdateAsync(model);
            }

            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

       // [HttpGet]
       //// [ValidateAntiForgeryToken]
       // public async Task<ActionResult> Delete(string KEY)
       // {
       //     if (string.IsNullOrWhiteSpace(KEY))
       //     {
       //         return Json(new { success = false, status = "Failed", message = "Invalid ID provided." });
       //     }
       //     var result = await _feeConfigService.DeleteAsync(KEY);
       //     return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
       // }

		[HttpGet]
		public async Task<ActionResult> Delete(string KEY)
		{
			if (string.IsNullOrEmpty(KEY))
				return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

			var result = await _feeConfigService.DelateAsync(KEY);

			bool success = result?.Result ?? false;
			string message = Messaging.MessageResult(result) ?? "Operation completed.";

			return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}