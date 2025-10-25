//using CBS.BusinessService.CheckManagementSystem;
//using CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration;
//using CBS.BusinessService.Config;
//using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
//using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
//using CBS.FrontDesk.Data.Message;
//using Newtonsoft.Json;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Mvc;

//namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Configurations.FeeConfiguration
//{
// //   [CheckSessionTimeOut]
//    public class FeeConfigController : BaseController
//    {
//        private readonly FeeConfigService _feeConfigService;
//        private readonly BranchServices _branchServices;

//        public FeeConfigController(FeeConfigService feeConfigService, BranchServices branchServices)
//        {
//            _feeConfigService = feeConfigService;
//            _branchServices = branchServices;
//        }

//        public async Task<ActionResult> Index()
//        {
//            await Loader();
//            return View(new FeeConfig());
//        }

//        private async Task Loader()
//        {
//            ViewBag.Branches = await _branchServices.GetBranches();
//            ViewBag.FeeTypes = await _feeConfigService.GetFeeTypesMockAsync(); // Using Mock
//        }

//        // THIS IS THE ONLY INITIALIZE ACTION WE NEED
//        //public async Task<ActionResult> InitializeData(string partialView, bool? isCentralized, string feeType, string branchId = null)
//        //{
//        //    bool isCentralizedValue = isCentralized ?? false;
//        //    await Loader();
//        //    FeeConfig model = null;

//        //    if (!string.IsNullOrWhiteSpace(feeType))
//        //    {
//        //        var configs = await _feeConfigService.GetConfigsMockAsync(feeType, isCentralizedValue ? null : branchId, isCentralizedValue);
//        //        model = configs.FirstOrDefault();
//        //    }

//        //    if (model == null)
//        //    {
//        //        model = new FeeConfig
//        //        {
//        //            IsCentralized = isCentralizedValue,
//        //            BranchId = isCentralizedValue ? null : branchId,
//        //            FeeType = feeType,
//        //            IsActive = true,
//        //            AcceptPercentage = true
//        //        };
//        //    }
//        //    return PartialView(partialView, model);
//        //}

//        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
//        {
//            // ROUTE 1: Handle the "View All Files" button click.
//            if (path == "list")
//            {
//                var data = await _feeConfigService.GetAllConfigsAsSummaryAsync();
//                return PartialView(partialView, data);
//            }

//            // ROUTE 2: Handle the "Load" button click for the configuration form.
//            // This is the default path.
//            else
//            {
//                await Loader(); // Ensure dropdowns are populated
//                FeeConfig model = null;

//                // The specific parameters (isCentralized, branchId, feeType) are now
//                // passed inside the generic 'serviceOption' string as a JSON object.
//                if (!string.IsNullOrWhiteSpace(serviceOption))
//                {
//                    // Deserialize the JSON string from serviceOption into a temporary object.
//                    var options = JsonConvert.DeserializeObject<dynamic>(serviceOption);

//                    // Extract the values.
//                    bool isCentralized = options.isCentralized;
//                    string branchId = options.branchId;
//                    string feeType = options.feeType;

//                    // Load the existing configuration using the extracted values.
//                    if (!string.IsNullOrWhiteSpace(feeType))
//                    {
//                        var configs = await _feeConfigService.GetConfigsMockAsync(feeType, isCentralized ? null : branchId, isCentralized);
//                        model = configs.FirstOrDefault();
//                    }

//                    // If no existing model was found, create a new one to pre-populate the form.
//                    if (model == null)
//                    {
//                        model = new FeeConfig
//                        {
//                            IsCentralized = isCentralized,
//                            BranchId = isCentralized ? null : branchId,
//                            FeeType = feeType,
//                            IsActive = true,
//                            AcceptPercentage = true
//                        };
//                    }
//                }

//                // If, for some reason, the process fails, return a new empty model.
//                if (model == null)
//                {
//                    model = new FeeConfig();
//                }

//                return PartialView(partialView, model);
//            }
//        }

//        [HttpPost]
//        public async Task<ActionResult> CreateOrUpdate(FeeConfig model)
//        {

//            if (!ModelState.IsValid)
//            {
//                return Json(new { success = false, status = "Failed", message = "Please fill all required fields." });
//            }

//            ExecutionMessages data;
//            if (string.IsNullOrWhiteSpace(model.Id))
//            {
//                data = await _feeConfigService.CreateMockAsync(model);
//            }
//            else
//            {
//                data = await _feeConfigService.UpdateMockAsync(model);
//            }
//            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
//        }

//        [HttpGet]
//        public async Task<ActionResult> Delete(string KEY)
//        {
//            if (string.IsNullOrWhiteSpace(KEY))
//            {
//                return Json(new { success = false, status = "Failed", message = "Invalid ID provided." });
//            }
//            var result = await _feeConfigService.DeleteMockAsync(KEY);
//            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
//        }
//    }
//}

// -----------------------------------------------------------------------------
// File: FeeConfigController.cs
// -----------------------------------------------------------------------------
using CBS.BusinessService.CheckManagementSystem;
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
    [CheckSessionTimeOut]
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

        //// Single initialize entry point supporting list path and form path.
        //// Single initialize entry point supporting list path and form path.
        //public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        //{
        //    // ROUTE 1: return summary list
        //    if (path == "list")
        //    {
        //        var data = await _feeConfigService.GetConfigsAsync();
        //        return PartialView(partialView ?? "_ListPartial", data);
        //    }

        //    // ROUTE 2: load data for the form
        //    await Loader();
        //    FeeConfig model = null;

        //    if (!string.IsNullOrWhiteSpace(serviceOption))
        //    {
        //        InitOptions options = null;
        //        try { options = JsonConvert.DeserializeObject<InitOptions>(serviceOption); }
        //        catch { options = new InitOptions(); }

        //        bool isCentralized = options?.isCentralized ?? false;
        //        string branchId = options?.branchId;
        //        string feeType = options?.feeType;

        //        if (!string.IsNullOrWhiteSpace(feeType))
        //        {
        //            var configs = await _feeConfigService.GetConfigsAsync(feeType, isCentralized ? null : branchId, isCentralized);
        //            model = configs.FirstOrDefault();
        //        }

        //        if (model == null)
        //        {
        //            model = new FeeConfig
        //            {
        //                IsCentralized = isCentralized,
        //                BranchId = isCentralized ? null : branchId,
        //                FeeType = options?.feeType,
        //                IsActive = true,
        //                AcceptPercentage = true
        //            };
        //        }
        //    }

        //    // C# 7.3 compatible replacement for `model ??= new FeeConfig();`
        //    if (model == null) model = new FeeConfig();

        //    return PartialView(partialView ?? "_FormPartial", model);
        //}


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
                        IsActive = true,
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
                    IsActive = true,
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

        [HttpGet]
       // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return Json(new { success = false, status = "Failed", message = "Invalid ID provided." });
            }
            var result = await _feeConfigService.DeleteAsync(KEY);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }
    }
}

///************************************** EXPLANATION OF THE WHOLE FLOW OF FEE CONFIG ********************************************************
///
//The controller InitializeData expects partialView parameter. The Index load sends partialView: '_Create' — ensure the controller uses that exact name when returning the partial (your controller code already accepts partialView).

//The POSTs call controller endpoints CreateOrUpdate and Delete and include the anti-forgery token — these match the controller you created ([ValidateAntiForgeryToken] on Delete and CreateOrUpdate).

//The _DataTable partial uses ViewData["index"] to set correct field names. When creating rows client-side we replace {INDEX} with the current index.

//If you run into model-binding issues for decimal fields, ensure the user's culture and decimal separators are configured properly; using step="0.01" on inputs is recommended.

//If your front-end uses select2, the post-injection if ($.fn.select2) block reinitializes select2 for any .select2 elements in the partial — remove or adapt if you do not use select2.The controller InitializeData expects partialView parameter. The Index load sends partialView: '_Create' — ensure the controller uses that exact name when returning the partial (your controller code already accepts partialView).

//The POSTs call controller endpoints CreateOrUpdate and Delete and include the anti-forgery token — these match the controller you created ([ValidateAntiForgeryToken] on Delete and CreateOrUpdate).

//The _DataTable partial uses ViewData["index"] to set correct field names. When creating rows client-side we replace {INDEX} with the current index.

//If you run into model-binding issues for decimal fields, ensure the user's culture and decimal separators are configured properly; using step="0.01" on inputs is recommended.

//If the front-end uses select2, the post-injection if ($.fn.select2) block reinitializes select2 for any .select2 elements in the partial — remove or adapt if you do not use select2.