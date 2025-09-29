using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.ChequeClearance;
using CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using ZXing;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Clearance.ChequeClearance
{
    public class ChequeClearanceController : Controller
    {

        private readonly ChequeClearanceService _chequeClearanceService;
        private readonly BranchServices _branchServices;
       private readonly MockCheckClearanceService _mockCheckClearanceService;
        private readonly FeeConfigService _feeConfigService;

        public ChequeClearanceController(ChequeClearanceService chequeClearanceService, BranchServices branchServices, FeeConfigService feeConfigService, MockCheckClearanceService mockCheckClearanceService)
        {
            _chequeClearanceService = chequeClearanceService;
            _branchServices = branchServices;
           _mockCheckClearanceService = mockCheckClearanceService;
            _feeConfigService = feeConfigService;


        }

        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new OptionRequest()); // pass categories as model
        }
        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var feeConfigure = await _feeConfigService.GetFeeTypesAsync();
            ViewBag.FeeTypes = feeConfigure;

            return true;
        }
        // Ajax entry point
        public async Task<ActionResult> InitializeData(string KEY = null,string partialView = null, string path = null,string serviceOption = null)
        {
            await loader();

            if (path == "list")
            {
                // ✅ When loading list view
                var data = await _mockCheckClearanceService.GetAllAsync();
                return PartialView(partialView?? "_ChequeClearanceDataTable", data);
            }
            else if (path == "new")
            {               

                OptionRequest model = null;
                OptionRequest options = new OptionRequest(); // ✅ always initialized

                if (!string.IsNullOrWhiteSpace(serviceOption))
                {
                    try
                    {
                        options = JsonConvert.DeserializeObject<OptionRequest>(serviceOption)
                                  ?? new OptionRequest(); // ✅ fallback if null
                    }
                    catch
                    {
                        options = new OptionRequest(); // ✅ fallback on bad JSON
                    }

                    // ✅ Now options can never be null
                    model = await _mockCheckClearanceService.GetByBranchAndBookAsync(
                        options.External,
                        options.BranchId,
                        options.CheckBookNumber,
                        options.CheckBookPageNumber
                    );

                    if (model == null)
                    {
                        model = new OptionRequest
                        {
                            External = options.External,
                            BranchId = options.BranchId,
                            CheckBookNumber = options.CheckBookNumber,
                            CheckBookPageNumber = options.CheckBookPageNumber
                        };
                    }
                }

                if (model == null)
                    model = new OptionRequest();

                model.ChequeImagePath = "~/AppFiles/Images/noimage.jpg";

                return PartialView(partialView ?? "_Create", model);
            }

            else
            {
                // ✅ Edit existing record
                await loader();
                var data = await _mockCheckClearanceService.GetByIdAsync(KEY);
                return PartialView(partialView ?? "_Edit", data);
            }
        }


        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

            var clearance = await _mockCheckClearanceService.GetByIdAsync(id);

            if (clearance == null)
                return HttpNotFound();

            return PartialView("_ChequeClearanceDetails", clearance);
        }




        [HttpGet]
        public ActionResult GetActionForm(string fileUploadId, string mode)
        {
            var model = new ClearanceValidation
            {
                ChequeClearanceId = fileUploadId,
                ApprovedBy = Session["FullName"]?.ToString(),
                Mode = mode // "approve", "review", or "reject",""
            };
            return PartialView("_ValidationForm", model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAction(ClearanceValidation model)
        {
            if (!ModelState.IsValid)
            {
                // --- THIS IS THE CRITICAL CHANGE ---
                // We need to return the ModelState errors in a format the client can parse.
                var errors = new Dictionary<string, string[]>();
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        errors[key] = state.Errors.Select(e => e.ErrorMessage).ToArray();
                    }
                }
                return Json(new { success = false, message = "Please correct the validation errors.", errors = errors });
            }

            var result = await _chequeClearanceService.SubmitFileActionAsync(model);
            // Ensure Messaging.MessageResult returns a string
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }
        [HttpPost]
        public async Task<JsonResult> LoadClearanceData(ClearanceQuery query)
        {
            try
            {
                var data = await _chequeClearanceService.GetClearanceDataTableAsync(query);
                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = data.data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.Options?.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(OptionRequest model)
       {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, status = "Failed", message = "Please fill all required fields." });
            }

            ExecutionMessages data;
            if (string.IsNullOrWhiteSpace(model.ChequeClearanceId))
            {
                data = await _chequeClearanceService.CreateAsync(model);
            }
            else
            {
                data = await _chequeClearanceService.UpdateAsync(model);
            }

            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }





    }
}




