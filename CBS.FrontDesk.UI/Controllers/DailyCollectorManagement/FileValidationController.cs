// Location: ~/Controllers/FileValidationController.cs

using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
{
    [CheckSessionTimeOut]
    public class FileValidationController : BaseController
    {
        private readonly ManualDailyCollectionService _manualService;

        public FileValidationController(ManualDailyCollectionService manualService)
        {
            _manualService = manualService;
        }

        // ACTION 1: Loads the main container page for the list
        public ActionResult Index()
        {
            // Prepare data for the status filter dropdown
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Extracted", "Rejected" });
            return View();
        }

        public async Task<ActionResult> LoadFiles(GetFilesForDataTableQuery query)
        {
            try
            {
                // 1. Fetch the raw data from the service.
                var dataTable = await _manualService.GetFilesForDataTableAsync(query);

                // 2. Deserialize the generic 'data' property into a strongly-typed list.
                var fileList = JsonConvert.DeserializeObject<List<FileUploadSummary>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                // 3. Process the list to generate the final UI data, including action buttons.
                var resultData = fileList.Select(f => new
                {
                    fileName = f.FileName,
                    branchName = f.BranchName,
                    uploadedBy = f.UploadedBy,
                    uploadedOn = f.UploadedOn.ToString("yyyy-MM-dd HH:mm"),
                    status = f.SalaryProcessingStatus,
                    actions = GenerateActionButtons(f, query.StatusFilter)
                }).ToList();

                // 4. Return the final JSON payload.
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
                // Log ex
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading file data.");
            }
        }

        // The helper method remains unchanged
        private string GenerateActionButtons(FileUploadSummary file, string statusContext)
        {
            var fileId = file.FileUploadId;
            if (!string.IsNullOrEmpty(statusContext) && statusContext.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                return $@"
                    <select class='form-select form-select-sm js-action-menu' onchange='handleAction(this)'>
                        <option selected>Select Action...</option>
                        <option value='approve' data-fileid='{fileId}'>Approve</option>
                        <option value='review' data-fileid='{fileId}'>Review</option>
                        <option value='reject' data-fileid='{fileId}'>Reject</option>
                        <option value='details' data-fileid='{fileId}'>View Details</option>
                    </select>";
            }
            else
            {
                return $@"
                    <a href='/ManualDailyCollection/Details/{fileId}' class='btn btn-sm btn-outline-info'>Details</a>
                    <button type='button' class='btn btn-sm btn-outline-danger ms-1 js-delete-btn' data-fileid='{fileId}'>Delete</button>";
            }
        }
        // ACTION 3: Gets the partial view for the validation/review/deny modal
        [HttpGet]
        public ActionResult GetActionForm(string fileUploadId, string mode)
        {
            var model = new ValidationDto // A simple DTO for the form
            {
                FileUploadId = fileUploadId,
                ApprovedBy = Session["FullName"]?.ToString(), // Get current user
                Mode = mode // "validate", "review", or "deny"
            };
            return PartialView("_ActionForm", model);
        }

        // ACTION 4: Handles the submission of the validation/review/deny form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitAction(ValidationDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Statement is required." });
            }
            // The service method will handle which API to call based on the mode
            var result = await _manualService.SubmitFileActionAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        // Helper to build the action buttons
        private string GenerateActionButtons(FileUploadSummary file)
        {
            var fileId = file.FileUploadId;
            // Only show the validation dropdown for "Pending" files
            if (file.SalaryProcessingStatus?.Equals("Pending", StringComparison.OrdinalIgnoreCase) == true)
            {
                return $@"
                    <select class='form-select form-select-sm js-action-menu' onchange='handleAction(this)'>
                        <option selected>Select Action...</option>
                        <option value='validate' data-fileid='{fileId}'>Approve</option>
                        <option value='review' data-fileid='{fileId}'>Review</option>
                        <option value='deny' data-fileid='{fileId}'>Reject</option>
                        <option value='details' data-fileid='{fileId}'>View Details</option>
                    </select>";
            }
            // For all other statuses, just show a Details button
            return $"<a href='/ManualDailyCollection/Details/{fileId}' class='btn btn-sm btn-outline-info'>Details</a>";
        }
    }
}