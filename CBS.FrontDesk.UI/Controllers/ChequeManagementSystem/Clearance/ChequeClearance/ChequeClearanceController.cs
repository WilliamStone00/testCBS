using CBS.BusinessService.Accounting_V2.IPS;
using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.ChequeClearance;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.IPS;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
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
        private readonly ChequeBookService _chequeBookService;

        public ChequeClearanceController(ChequeClearanceService chequeClearanceService, BranchServices branchServices, CounterChequeService counterChequeService, ChequeBookService chequeBookService)
        {
            _chequeClearanceService = chequeClearanceService;
            _branchServices = branchServices;
            _counterChequeService = counterChequeService;
            _chequeBookService = chequeBookService;

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

        //[HttpGet]
        //public async Task<ActionResult> ImageUpload(string clearanceId)
        //{
        //    ViewBag.Clearance = clearanceId;
        //    return View();
        //}

        [HttpGet]
        public async Task<ActionResult> ImageUpload(string clearanceId)
        {
            ViewBag.ClearanceId = clearanceId;

            var model = new ClearanceRequestImage
            {
                Id = clearanceId
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> LoadExternal()
        {

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


        public async Task<ActionResult> GetChequeBookDetail(string id)
        {
            try
            {
                // Try main service first
                var chequeBook = await _chequeBookService.GetChequeBookByIdAsync(id);


                return View("_chequeDetails", chequeBook);
            }
            catch (Exception ex)
            {
                // Final fallback to mock service
                return HttpNotFound($"Cheque book with ID {id} not found");

            }
        }



        [HttpGet]
        public async Task<ActionResult> GetChequeLeafDetail(string leafId)
        {
            if (string.IsNullOrEmpty(leafId))
                return new HttpStatusCodeResult(400, "LeafId is required");

            try
            {
                var leaf = await _counterChequeService.GetChequeLeafDetails(leafId);

                if (leaf == null)
                    return HttpNotFound("Cheque leaf not found");

                var model = new OptionRequest
                {

                    CheckBookId = leaf.CheckBookId,
                    CheckBookPageNumber = leaf.SerialNumber,
                    // adjust if needed
                    // You can leave others empty for user input
                };

                return View("_InternalChequeRequest", model);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }



        [HttpPost]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(OptionRequest model)
        {
            try
            {
                var result = await _chequeClearanceService.CreateAsync(model);

                if (result.Result)
                {
                    var clearance = result.Data as ClearanceResponce;

                    return Json(new
                    {
                        success = true,
                        message = "Clearance request created successfully!",
                        clearanceId = clearance?.Id,
                        uploadUrl = Url.Action("ImageUpload", "ChequeClearance",
                                        new { clearanceId = clearance?.Id }),
                        listingUrl = Url.Action("Index", "ChequeClearance")
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.MessageString
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing clearance: " + ex.Message
                });
            }
        }

        public async Task<ActionResult> ImageUploads(ClearanceRequestImage model)
        {
            if (model.AttachedFiles == null || !model.AttachedFiles.Any())
            {
                return Json(new
                {
                    success = false,
                    status = "Failed",
                    message = "No files were uploaded."
                });
            }

            var data = await _chequeClearanceService.UploadFiles(model);

            return Json(new
            {
                success = data.Result,
                status = data.MessageStatus ?? "Insert",
                message = Messaging.MessageResult(data),
                reloadDataView = "Yes"
            });
        }


        public async Task<JsonResult> LoadClearanceData(ClearanceQuery query)
        {
            try
            {
                var data = await _chequeClearanceService.ClearanceDataTableAsync(query);

                var Request = JsonConvert.DeserializeObject<List<Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest.ClearanceResponce>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {

                    draw = data.DataTableOptions.draw ?? "1",
                    recordsTotal = data.DataTableOptions.recordsTotal,
                    recordsFiltered = data.DataTableOptions.recordsFiltered,
                    data = Request,
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
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
    }
}







