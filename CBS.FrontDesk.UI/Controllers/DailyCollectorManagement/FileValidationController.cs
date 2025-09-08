
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
    [CheckSessionTimeOut]
    public class FileValidationController : BaseController
    {
        private readonly ManualDailyCollectionService _manualService;
        private readonly BranchServices _branchServices;

        public FileValidationController(ManualDailyCollectionService manualService, BranchServices branchServices)
        {
            _manualService = manualService;
            _branchServices = branchServices;
        }

       
        // ACTION 1: Loads the main page with filter controls
        public async Task<ActionResult> Index()
        {
            // Populate the Status dropdown for the view
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Extracted", "Rejected","Treated","Completed","F"});
            ViewBag.Branches = await _branchServices.GetBranches();
            return View();
        }

        // In FileValidationController.cs

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> LoadFiles(GetFilesForDataTableQuery query)
        {
            try
            {
                // 1. Fetch the data from the service. The service passes the query directly to the API.
                var dataTable = await _manualService.GetFilesForDataTableAsync(query);

                // 2. Deserialize the generic data into a strongly-typed list.
                var fileList = JsonConvert.DeserializeObject<List<FileUploadSummary>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                // 3. Process the list to generate the final UI data.
                var resultData = fileList.Select(f => new
                {
                    fileName = f.FileName,
                    branchName = f.BranchName,
                    uploadedBy = f.UploadedBy,
                    uploadedOn = f.UploadedOn.ToString("yyyy-MM-dd HH:mm"),
                    status = GetStatusBadge(f.SalaryProcessingStatus),
                    // The 'actions' property now gets the UNIVERSAL dropdown menu.
                    actions = GenerateActionButtons(f) // We no longer need to pass the status context.
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
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading file data.");
            }
        }

        // In FileValidationController.cs

        private string GetStatusBadge(string status)
        {
            if (string.IsNullOrEmpty(status)) return "";

            string badgeClass = "bg-secondary"; // Default color
            switch (status.ToLowerInvariant())
            {
                case "pending":
                    badgeClass = "bg-warning text-dark"; // Yellow badge for pending
                    break;
                case "approved":
                case "extracted":
                case "completed":
                case "treated":
                    badgeClass = "bg-success"; // Green for success states
                    break;
                case "rejected":
                case "failed": // Assuming 'F' might mean failed
                    badgeClass = "bg-danger"; // Red for failure states
                    break;
            }

            // Return the HTML for the badge
            return $"<span class='badge {badgeClass}'>{status}</span>";
        }

        /// <summary>
        /// Helper method to generate the UNIVERSAL action dropdown for every row.
        /// </summary>
        private string GenerateActionButtons(FileUploadSummary file)
        {
            var fileId = file.FileUploadId;

            // This HTML is now generated for every single file, regardless of its status.
            return $@"
        <select class='form-select form-select-sm js-action-menu' onchange='handleAction(this)'>
            <option selected value=""""> Select Action...
            </option>
            <option value=""approve"" data-fileid=""{{fileId}}"">
                ✅ Approve
            </option>
            <option value=""review"" data-fileid=""{{fileId}}"">
                🔍 Review
            </option>
            <option value=""reject"" data-fileid=""{{fileId}}"">
                ❌ Reject
            </option>
        </select>";
        }

        // ACTION 3: Gets the partial view for the validation/review/approve modal
        [HttpGet]
        public ActionResult GetActionForm(string fileUploadId, string mode)
        {
            var model = new ValidationDto
            {
                FileUploadId = fileUploadId,
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