
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Math;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ZXing.QrCode.Internal;

namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
{
    // [CheckSessionTimeOut]
    public class FileValidationController : BaseController
    {
        private readonly ManualDailyCollectionService _manualService;
        private readonly BranchServices _branchServices;

        public FileValidationController(ManualDailyCollectionService manualService, BranchServices branchServices)
        {
            _manualService = manualService;
            _branchServices = branchServices;
        }


        public async Task<ActionResult> Index()
        {
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Extracted", "Rejected", "Treated", "Completed", "Failed" });
            ViewBag.Branches = await _branchServices.GetBranches();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadFiles(GetFilesForDataTableQuery query)
         {
            try
            {
                var dataTable = await _manualService.GetextractedFilesForDataTableAsync(query);
                var fileList = JsonConvert.DeserializeObject<List<FileUploadSummary>>(JsonConvert.SerializeObject(dataTable.data));

                var resultData = fileList.Select(f => new
                {
                    fileUploadId = f.FileUploadId,
                   
                    collectorName = f.CollectorName, 
                    branchName = f.BranchName,
                    id = f.Id,
                    uploadedBy = f.UploadedBy,
                    uploadedOn = f.UploadedOn.ToString("yyyy-MM-dd HH:mm"),
                    Status = f.status
                }).ToList();

                return Json(new
                {
                    draw = query.Options?.draw ?? "1",
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = resultData
                });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading file data.");
            }
        }

        // Returns the partial view with file summary and DataTable placeholder
        [HttpGet]
        public async Task<ActionResult> GetextractedFileDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

            var model = await _manualService.GetExtractedDetailsAsync(id);

            if (model == null)
                return HttpNotFound();

            return PartialView("_FileDetails", model);
        }

        // Returns JSON for DataTable
        [HttpGet]
        public async Task<ActionResult> GetTransactionDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

            var fileDetails = await _manualService.GetExtractedDetailsAsync(id);

            if (fileDetails?.Details == null || !fileDetails.Details.Any())
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            var jsonData = fileDetails.Details.Select(d => new
            {
                memberName = d.MemberName,
                accountNumber = d.AccountNumber,
                memberBranchName = d.MemberBranchName,
                amount = d.Amount
            });

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }





        // ACTION 3: Gets the partial view for the validation/review/approve modal
        [HttpGet]
        public ActionResult GetActionForm(string fileUploadId, string mode)
        {
            var model = new ValidationDto
            {
                ManualEntryDailyCollectorId = fileUploadId,
                ApprovedBy = Session["FullName"]?.ToString(),
                Mode = mode // "approve", "review", or "reject"
            };
            return PartialView("_ActionForm", model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAction(ValidationDto model)
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

            var result = await _manualService.SubmitFileActionAsync(model);
            // Ensure Messaging.MessageResult returns a string
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }


        [HttpPost]
        public async Task<ActionResult> RejectFile(string KEY)
        {
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return Json(new { success = false, message = "Invalid ID provided for rejection." });
            }

            var result = await _manualService.RejectFileAsync(KEY);
            // Ensure Messaging.MessageResult returns a string
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

    }
}