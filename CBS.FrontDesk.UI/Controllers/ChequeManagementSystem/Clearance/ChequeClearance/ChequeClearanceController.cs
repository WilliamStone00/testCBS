using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.ChequeClearance;

using CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;

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
        private readonly CounterChequeService _counterChequeService;

        public ChequeClearanceController(ChequeClearanceService chequeClearanceService, BranchServices branchServices, CounterChequeService counterChequeService,)
        {
            _chequeClearanceService = chequeClearanceService;
            _branchServices = branchServices;
            _counterChequeService = counterChequeService;
        }

        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new OptionRequest() { Discount= new Discount()}); // pass categories as model
        }
        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            

            ViewBag.Status = new List<SelectListItem>
{
    new SelectListItem { Value = "APPROVED", Text = "APPROVED" },
    new SelectListItem { Value = "INITIATED", Text = "INITIATED" },
    new SelectListItem { Value = "REJECTED", Text = "REJECTED" }
};
            ViewBag.ChequeType = new List<SelectListItem>
{
    new SelectListItem { Value = "Internal", Text = "INTERNAL" },
    new SelectListItem { Value = "External", Text = "EXTERNAL" }
};
            return true;
        }
       
        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadExternal()
        {
            //var model = new OptionRequest
            //{
            //    External = true
            //};
            await loader();
            return PartialView("_Create");
        }





        [HttpGet]
        public async Task<ActionResult> Search(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                return new HttpStatusCodeResult(400, "customerId is required");

            try
            {
                var entry = await _counterChequeService.GetCustomerChequeBooks(customerId);
                if (entry == null)
                    return HttpNotFound("customerId details not found");

                return PartialView("_MainForm", entry);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }




        //[HttpGet]
        //public async Task<ActionResult> Details(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

        //    var clearance = await _mockCheckClearanceService.GetByIdAsync(id);

        //    if (clearance == null)
        //        return HttpNotFound();

        //    return PartialView("_ChequeClearanceDetails", clearance);
        //}





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




