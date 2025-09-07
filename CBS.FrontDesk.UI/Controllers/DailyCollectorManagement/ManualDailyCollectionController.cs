//using CBS.BusinessService.Config;
//using CBS.BusinessService.CustomerManagement;
//using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
//using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
//using CBS.FrontDesk.Data.Entity.DataTable;
//using CBS.FrontDesk.Data.Entity.ManualDailycollection;
//using CBS.FrontDesk.Data.Message;
//using CBS.FrontDesk.UI.Helper;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;

//namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
//{
//    [CheckSessionTimeOut]
//    public class ManualDailyCollectionController : BaseController
//    {
//        private readonly ManualDailyCollectionService _manualService;
//        private readonly BranchServices _branchServices;
//        private readonly IndividualProfileServices _individualProfileServices;

//        public ManualDailyCollectionController(ManualDailyCollectionService manualService, BranchServices branchServices, IndividualProfileServices individualProfileServices)
//        {
//            _manualService = manualService;
//            _branchServices = branchServices;
//            _individualProfileServices = individualProfileServices;
//        }

//        /// <summary>
//        /// ACTION 1: Returns the main container page for the entire module.
//        /// </summary>
//        public async Task<ActionResult> Index()
//        {
//            await loader();
//            return View(new FileUploadResponse());
//        }

//        public async Task<bool> loader()
//        {
//            var branches = await _branchServices.GetBranches();
//            ViewBag.Branches = branches;

//            var collectors = await _manualService.GetDailyCollectorsAsSelectListAsync();
//            ViewBag.collectors = collectors;

//            return true;
//        }

//        /// <summary>
//        /// ACTION 2: This is the central router action that loads all partial views, called by your generic JS.
//        /// </summary>
//        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string customerType = null)
//        {
//            if (path == "list")
//            {
//                var data = await _manualService.GetAllFilesAsync();
//                return PartialView(partialView, data);
//            }
//            else if (path == "new")
//            {
//                await loader();
//                return PartialView(partialView, new FileUploadResponse());
//            }
//            else
//            {
//                var data = await _manualService.GetFileDetailsByIdAsync(KEY);
//                return PartialView(partialView, data);
//            }
//        }

//        /// <summary>0
//        /// ACTION 3: Handles the AJAX file upload from the _UploadForm.
//        /// </summary>
//        [HttpPost]
//        public async Task<ActionResult> UploadFile(string BranchId, string CollectorId, HttpPostedFileBase uploadedFile)
//        {
//            // --- Server-side validation remains the same ---
//            if (uploadedFile == null || uploadedFile.ContentLength == 0)
//            {
//                return Json(new { success = false, message = "Please select a file to upload." });
//            }
//            // ... other validations for BranchId, CollectorId ...

//            // The service call remains the same
//            var response = await _manualService.UploadManualEntryFileAsync(uploadedFile, BranchId, CollectorId);

//            // --- THIS IS THE NEW, EFFICIENT LOGIC ---
//            if (response.IsSuccess && response.ApiResponseData.Success)
//            {
//                // The service call was successful and the backend processed the file.
//                // The full details are in response.ApiResponseData.Data.

//                return Json(new
//                {
//                    success = true,
//                    message = response.ApiResponseData.Message ?? "File processed successfully.",
//                    data = response.ApiResponseData.Data // Return the COMPLETE data object
//                });
//            }

//            // Handle failure cases
//            return Json(new { success = false, message = response.ApiResponseData?.Message ?? response.Message ?? "An error occurred during file processing." });
//        }

//        /// <summary>
//        /// ACTION 4: Handles the "Download Template" button click.
//        /// </summary>
//        [HttpGet]
//        public ActionResult DownloadTemplate()
//        {
//            try
//            {
//                string physicalPath = Server.MapPath("~/AppFiles/ManualDailyCollection/DailyCollectorFieldReport_Template.xlsx");
//                if (!System.IO.File.Exists(physicalPath))
//                {
//                    // In a real application, you'd have a more graceful error page or message.
//                    return HttpNotFound("Template file not found.");
//                }

//                byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
//                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DailyCollectorFieldReport_Template.xlsx");
//            }
//            catch (Exception ex)
//            {
//                // Log the exception in a real application
//                // Returning a JSON error is suitable if this is called via AJAX,
//                // but a direct link click might show raw JSON. A dedicated error page is better.
//                return new HttpStatusCodeResult(500, "An error occurred while preparing the download.");
//            }
//        }


//        /// <summary>
//        /// ACTION 5: Deletes an uploaded file by its ID.
//        /// </summary>
//        [HttpGet] // Matching your existing pattern
//        public async Task<ActionResult> Delete(string KEY)
//        {
//            if (string.IsNullOrEmpty(KEY))
//            {
//                return Json(new { success = false, status = "Bad Request", message = "File ID cannot be null." }, JsonRequestBehavior.AllowGet);
//            }

//            var result = await _manualService.DeleteFileByIdAsync(KEY);
//            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
//        }


//        [HttpGet]
//        public ActionResult IndexValidate()
//        {
//            // Populate the Status dropdown for the view
//            ViewBag.Statuses = new SelectList(new[]
//            {
//            new { Value = "Pending", Text = "Pending" },
//            new { Value = "Approved", Text = "Approved" },
//            new { Value = "Rejected", Text = "Rejected" }
//        }, "Value", "Text");

//            return View(); // <-- Returns ~/Views/FileValidation/Index.cshtml
//        }

//        [HttpPost]
//        public async Task<ActionResult> LoadFiles(FileValidationRequestDto request)
//        {
//            try
//            {
//                if (request == null) request = new FileValidationRequestDto();
//                if (request.DataTableOptions == null) request.DataTableOptions = new DataTableOptions();

//                var files = (await _manualService.GetFilesByStatusAsync(request.StatusFilter)).ToList();
//                var recordsTotal = files.Count;

//                var pagedData = files
//                    .Skip(request.DataTableOptions.start)
//                    .Take(request.DataTableOptions.length)
//                    .ToList();

//                var resultData = pagedData.Select(f =>
//                {
//                    var actionsHtml = $@"
//                <div class='btn-group'>
//                  <button class='btn btn-sm btn-primary dropdown-toggle' type='button' data-bs-toggle='dropdown' aria-expanded='false'>Actions</button>
//                  <ul class='dropdown-menu'>
//                    <li><a class='dropdown-item js-validate' href='#' data-fileid='{f.FileUploadId}' data-approvedby='{Session["FullName"]}'>Validate</a></li>
//                    <li><a class='dropdown-item js-review' href='#' data-fileid='{f.FileUploadId}' data-approvedby='{Session["FullName"]}'>Review</a></li>
//                    <li><a class='dropdown-item js-delete text-danger' href='#' data-fileid='{f.FileUploadId}'>Delete</a></li>
//                  </ul>
//                </div>";

//                    return new
//                    {
//                        fileName = f.FileName,
//                        branchName = f.BranchName,
//                        uploadedBy = f.UploadedBy,
//                        uploadedOn = f.UploadedOn.ToString("yyyy-MM-dd HH:mm"),
//                        status = f.SalaryProcessingStatus,
//                        actions = actionsHtml
//                    };
//                }).ToList();

//                return Json(new
//                {
//                    draw = request.DataTableOptions.draw,
//                    recordsTotal = recordsTotal,
//                    recordsFiltered = recordsTotal,
//                    data = resultData
//                });
//            }
//            catch (Exception)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
//            }
//        }


//        // Action to handle the form submission
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> SubmitValidation(FileValidationRequest model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return Json(new { success = false, message = "Please fill all required fields." });
//            }

//            var result = await _manualService.ValidateFileAsync(model);
//            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
//        }

//        // GET partial used for both Validate & Review
//        [HttpGet]
//        public ActionResult GetValidationForm(string fileUploadId, string approvedBy, string mode)
//        {
//            // mode = "validate" or "review"
//            var model = new ValidationFormViewModel
//            {
//                FileUploadId = fileUploadId,
//                ApprovedBy = approvedBy,
//                Mode = mode // "validate" or "review"
//            };

//            // _ValidationForm.cshtml is the partial below
//            return PartialView("_ValidationForm", model);
//        }

//        // POST validate
//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<ActionResult> ValidateFile(ValidationDto dto)
//        //{
//        //    if (!ModelState.IsValid)
//        //    {
//        //        return Json(new { success = false, message = "Validation failed." });
//        //    }

//        //    try
//        //    {
//        //        // call your service to validate
//        //        var result = await _manualService.ApproveFileAsync(dto.FileUploadId, dto.ApprovedBy, dto.ApprovalStatement);
//        //        if (result.IsSuccess) return Json(new { success = true, message = "File validated successfully" });
//        //        return Json(new { success = false, message = result.Message ?? "Could not validate file" });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        // log ex
//        //        return Json(new { success = false, message = "An error occurred while validating file." });
//        //    }
//        //}

//        //// POST review
//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<ActionResult> ReviewFile(ReviewDto dto)
//        //{
//        //    if (!ModelState.IsValid)
//        //    {
//        //        return Json(new { success = false, message = "Validation failed." });
//        //    }

//        //    try
//        //    {
//        //        // call your service to mark as reviewed
//        //        var result = await _manualService.ReviewFileAsync(dto.FileUploadId, dto.ApprovedBy, dto.ReviewerStatement);
//        //        if (result.IsSuccess) return Json(new { success = true, message = "File submitted for review successfully" });
//        //        return Json(new { success = false, message = result.Message ?? "Could not submit review" });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        // log ex
//        //        return Json(new { success = false, message = "An error occurred while submitting review." });
//        //    }
//        //}

//        //// POST delete
//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<ActionResult> DeleteFile(string fileUploadId)
//        //{
//        //    if (string.IsNullOrEmpty(fileUploadId)) return Json(new { success = false, message = "Invalid file id" });

//        //    try
//        //    {
//        //        var result = await _manualService.DeleteFileAsync(fileUploadId);
//        //        if (result.IsSuccess) return Json(new { success = true, message = "File deleted successfully" });
//        //        return Json(new { success = false, message = result.Message ?? "Could not delete file" });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        // log ex
//        //        return Json(new { success = false, message = "An error occurred while deleting file." });
//        //    }
//        //}

//    }

//}
// Location: ~/Controllers/DailyCollectorManagement/ManualDailyCollectionController.cs

using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
{
    [CheckSessionTimeOut]
    public class ManualDailyCollectionController : BaseController
    {
        private readonly ManualDailyCollectionService _manualService;
        private readonly BranchServices _branchServices;

        public ManualDailyCollectionController(ManualDailyCollectionService manualService, BranchServices branchServices)
        {
            _manualService = manualService;
            _branchServices = branchServices;
        }

        /// <summary>
        /// ACTION 1: Returns the main container page for the entire module.
        /// </summary>
        public async Task<ActionResult> Index()
        {
            await Loader();
            // Pass the ViewModel required by the _UploadForm partial view
            return View(new FileUploadResponse());
        }

        /// <summary>
        /// Helper to load data for dropdowns into the ViewBag.
        /// </summary>
        private async Task Loader()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            ViewBag.Collectors = await _manualService.GetCollectorsAsSelectListAsync();
        }

        /// <summary>
        /// ACTION 2: This is the central router action that works with your generic JS helpers.
        /// It loads partial views for the 'list' of all files, a 'new' upload form, or the 'details' of a file.
        /// </summary>
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                // This handles the "View All Files" button click.
                var data = await _manualService.GetAllFilesAsync();
                // It returns the _FileList.cshtml partial, which expects a List<FileUploadResponse>.
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                // This handles the "Upload New File" button click.
                await Loader();
                // It returns the _UploadForm.cshtml partial.
                return PartialView(partialView, new ManualCollectionUploadViewModel());
            }
            else // This defaults to the "details" path
            {
                // This handles the "Details" button click from the list view.
                var data = await _manualService.GetFileDetailsByIdAsync(KEY);
                // It returns the _FileDetails.cshtml partial, which expects a single FileUploadResponse.
                return PartialView(partialView, data);
            }
        }

        /// <summary>
        /// ACTION 3: Handles the AJAX file upload from the _UploadForm.
        /// </summary>
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFile(string BranchId, string CollectorAndUser, HttpPostedFileBase UploadedFile)
        {
            // ... (Manual validation for parameters) ...
            if (UploadedFile == null || UploadedFile.ContentLength == 0)
                return Json(new { success = false, message = "Please select a file to upload." });
            if (string.IsNullOrWhiteSpace(BranchId))
                return Json(new { success = false, message = "Please select a branch." });
            if (string.IsNullOrWhiteSpace(CollectorAndUser))
                return Json(new { success = false, message = "Please select a collector." });

            try
            {
                var collectorAndUserParts = CollectorAndUser.Split('|');
                var collectorId = collectorAndUserParts[0];
                var userId = collectorAndUserParts[1];

                var response = await _manualService.UploadFileAsync(UploadedFile, BranchId, collectorId, userId);

                if (response.IsSuccess && response.ApiResponseData.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = response.ApiResponseData.Message ?? "File processed successfully.",
                        data = response.ApiResponseData.Data // Return the COMPLETE data object
                    });
                }
                return Json(new { success = false, message = response.ApiResponseData?.Message ?? response.Message });
            }
            catch (Exception ex)
            {
                // Log ex
                return Json(new { success = false, message = "An unexpected server error occurred." });
            }
        }

        /// <summary>
        /// ACTION 4: Handles the "Download Template" button click.
        /// </summary>
        [System.Web.Mvc.HttpGet]
        public ActionResult DownloadTemplate()
        {
            // ... (Your existing download logic is correct) ...
            string physicalPath = Server.MapPath("~/AppFiles/ManualDailyCollection/DailyCollectorFieldReport_Template.xlsx");
            if (!System.IO.File.Exists(physicalPath)) return HttpNotFound("Template file not found.");
            byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DailyCollectorFieldReport_Template.xlsx");
        }

        /// <summary>
        /// ACTION 5: Deletes an uploaded file by its ID. Works with your generic DeleteRecordDataTable helper.
        /// </summary>
        [System.Web.Mvc.HttpGet] // Matching your existing pattern, but [HttpPost] is recommended for security.
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
            {
                return Json(new { success = false, status = "Bad Request", message = "File ID cannot be null." }, JsonRequestBehavior.AllowGet);
            }
            var result = await _manualService.DeleteFileByIdAsync(KEY);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
        }
    }
}