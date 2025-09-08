
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
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
                    status = f.SalaryProcessingStatus
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

        [HttpGet]
        public async Task<ActionResult> GetextractedFileDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "File ID is required.");
            }
            var fileDetails = await _manualService.GetextractedFileDetailsAsync(id);
            if (fileDetails == null)
            {
                return PartialView("_ErrorDetails", "Could not retrieve details for the selected file.");
            }
            return PartialView("_FileDetails", fileDetails);
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

        // ACTION 4: Handles the submission from the modal form
        [HttpPost]
        public async Task<ActionResult> SubmitAction(ValidationDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Statement is required." });
            }
            var result = await _manualService.SubmitFileActionAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }


        [HttpPost]
        public async Task<ActionResult> RejectFile(string KEY)
        {
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return Json(new { success = false, message = "Invalid ID provided for rejection." });
            }

            // You will need a 'RejectFileAsync' method in your service.
            var result = await _manualService.RejectFileAsync(KEY);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

    }
}