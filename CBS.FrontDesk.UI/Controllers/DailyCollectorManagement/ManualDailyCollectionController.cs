//using CBS.BusinessService.Config;
//using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
//using CBS.FrontDesk.Data.Entity.ManualDailycollection;
//using CBS.FrontDesk.Data.Message;
//using CBS.FrontDesk.UI.Helper;
//using System;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;

//namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
//{
//    //[CheckSessionTimeOut]
//    public class ManualDailyCollectionController : BaseController
//    {
//        private readonly IManualDailyCollectionService _manualService;
//      private readonly ManualDailyCollectionMockService _mockService;
//        private readonly BranchServices _branchServices;

//        /// <summary>
//        /// Injects the required services via dependency injection (Unity).
//        /// </summary>
//        public ManualDailyCollectionController(IManualDailyCollectionService manualService, BranchServices branchServices, ManualDailyCollectionMockService mockService)
//        {
//            _manualService = manualService;
//            _branchServices = branchServices;
//            _mockService = mockService;
//        }

//        /// <summary>
//        /// Displays the main container view for the module.
//        /// </summary>
//        public async Task<ActionResult> Index()
//        {
//            await Loader();
//            // The view itself is just a container; partial views will be loaded into it.
//            // We pass a new entity to satisfy the initial model binding.
//            return View(new FileUploadResponse());
//        }

//        /// <summary>
//        /// Shared helper to load common data (like Branches) into the ViewBag.
//        /// </summary>
//        private async Task Loader()
//        {
//            ViewBag.Branches = await _branchServices.GetBranches();
//        }

//        /// <summary>
//        /// This is the central router action that loads different partial views based on the 'path' parameter.
//        /// </summary>
//        /// <param name="KEY">The ID of the file to fetch for details view.</param>
//        /// <param name="partialView">The name of the .cshtml partial view file to render.</param>
//        /// <param name="path">The context ("list", "new", "details").</param>
//        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
//        {
//            if (path == "list")
//            {
//                // For the list, we fetch all files and pass the entity list to the partial view.
//                var data = await _mockService.GetAllFilesAsync();
//                return PartialView(partialView, data);
//            }
//            else if (path == "new")
//            {
//                // For the "new" view, we simply provide a new, empty entity.
//                await Loader(); // Load branches for dropdowns on the upload form
//                return PartialView(partialView, new FileUploadResponse());
//            }
//            else // This defaults to the "details" view
//            {
//                // For details, we fetch a specific file by its ID.
//                var data = await _mockService.GetFileDetailsByIdAsync(KEY);
//                return PartialView(partialView, data);
//            }
//        }

//        /// <summary>
//        /// Handles the file upload. In this pattern, there is no "update" for a file upload,
//        /// so this action is focused solely on creation (uploading).
//        /// </summary>
//        [HttpPost]
//        public async Task<ActionResult> CreateOrUpdate(FileUploadResponse model, HttpPostedFileBase uploadedFile)
//        {
//            // The concept of "Id" doesn't apply to a new upload, so we check the file itself.
//            if (uploadedFile != null && uploadedFile.ContentLength > 0)
//            {
//                var response = await _mockService.UploadManualEntryFileAsync(uploadedFile);

//                if (response.IsSuccess && response.ApiResponseData.Success)
//                {
//                    // If successful, return the validated data from the file.
//                    return Json(new
//                    {
//                        success = true,
//                        status = "Success",
//                        message = response.ApiResponseData.Message,
//                        data = response.ApiResponseData.Data // This contains the details to display
//                    });
//                }
//                else
//                {
//                    // If it fails, return the error message from the service.
//                    return Json(new
//                    {
//                        success = false,
//                        status = "Failed",
//                        message = response.ApiResponseData?.Message ?? response.Message
//                    });
//                }
//            }

//            // This part is for a potential "update" which is not a typical file upload operation.
//            // You can add logic here if you need to update the metadata of an ALREADY uploaded file.
//            // For now, it indicates a validation failure if no file is provided.
//            return Json(new { success = false, status = "Validation Error", message = "Please select a file to upload." });
//        }

//        /// <summary>
//        /// Handles the file upload from the _UploadForm partial view.
//        /// </summary>
//        [HttpPost]
//        public async Task<ActionResult> UploadFile(HttpPostedFileBase uploadedFile) // Simplified signature
//        {
//            if (uploadedFile == null || uploadedFile.ContentLength == 0)
//            {
//                return Json(new { success = false, message = "Please select a file to upload." });
//            }

//            var response = await _manualService.UploadManualEntryFileAsync(uploadedFile);

//            if (response.IsSuccess && response.ApiResponseData.Success)
//            {
//                return Json(new
//                {
//                    success = true,
//                    message = response.ApiResponseData.Message,
//                    // We return the validated data so the client-side script can display it
//                    data = response.ApiResponseData.Data
//                });
//            }

//            return Json(new
//            {
//                success = false,
//                message = response.ApiResponseData?.Message ?? response.Message
//            });
//        }

//        /// <summary>
//        /// Deletes an uploaded file by its ID.
//        /// WARNING: Using [HttpGet] for a delete operation is a security risk.
//        /// It should be changed to [HttpPost] in a production environment.
//        /// </summary>
//        [HttpGet]
//        public async Task<ActionResult> Delete(string KEY)
//        {
//            if (string.IsNullOrEmpty(KEY))
//            {
//                return Json(new { success = false, status = "Bad Request", message = "File ID cannot be null." }, JsonRequestBehavior.AllowGet);
//            }

//            var result = await _mockService.DeleteFileByIdAsync(KEY);
//            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
//        }
//    }
//}


using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly IndividualProfileServices _individualProfileServices;

        public ManualDailyCollectionController(ManualDailyCollectionService manualService, BranchServices branchServices, IndividualProfileServices individualProfileServices)
        {
            _manualService = manualService;
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
        }

        /// <summary>
        /// ACTION 1: Returns the main container page for the entire module.
        /// </summary>
        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new FileUploadResponse());
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var collectors = await _manualService.GetDailyCollectorsAsSelectListAsync();
            ViewBag.collectors = collectors;

            return true;
        }

        /// <summary>
        /// ACTION 2: This is the central router action that loads all partial views, called by your generic JS.
        /// </summary>
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string customerType = null)
        {
            if (path == "list")
            {
                var data = await _manualService.GetAllFilesAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                await loader();
                return PartialView(partialView, new FileUploadResponse());
            }
            else
            {
                var data = await _manualService.GetFileDetailsByIdAsync(KEY);
                return PartialView(partialView, data);
            }
        }

        /// <summary>0
        /// ACTION 3: Handles the AJAX file upload from the _UploadForm.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> UploadFile(string BranchId, string CollectorId, HttpPostedFileBase uploadedFile)
        {
            // --- Server-side validation remains the same ---
            if (uploadedFile == null || uploadedFile.ContentLength == 0)
            {
                return Json(new { success = false, message = "Please select a file to upload." });
            }
            // ... other validations for BranchId, CollectorId ...

            // The service call remains the same
            var response = await _manualService.UploadManualEntryFileAsync(uploadedFile, BranchId, CollectorId);

            // --- THIS IS THE NEW, EFFICIENT LOGIC ---
            if (response.IsSuccess && response.ApiResponseData.Success)
            {
                // The service call was successful and the backend processed the file.
                // The full details are in response.ApiResponseData.Data.

                return Json(new
                {
                    success = true,
                    message = response.ApiResponseData.Message ?? "File processed successfully.",
                    data = response.ApiResponseData.Data // Return the COMPLETE data object
                });
            }

            // Handle failure cases
            return Json(new { success = false, message = response.ApiResponseData?.Message ?? response.Message ?? "An error occurred during file processing." });
        }

        /// <summary>
        /// ACTION 4: Handles the "Download Template" button click.
        /// </summary>
        [HttpGet]
        public ActionResult DownloadTemplate()
        {
            try
            {
                string physicalPath = Server.MapPath("~/AppFiles/ManualDailyCollection/DailyCollectorFieldReport_Template.xlsx");
                if (!System.IO.File.Exists(physicalPath))
                {
                    // In a real application, you'd have a more graceful error page or message.
                    return HttpNotFound("Template file not found.");
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DailyCollectorFieldReport_Template.xlsx");
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                // Returning a JSON error is suitable if this is called via AJAX,
                // but a direct link click might show raw JSON. A dedicated error page is better.
                return new HttpStatusCodeResult(500, "An error occurred while preparing the download.");
            }
        }


        /// <summary>
        /// ACTION 5: Deletes an uploaded file by its ID.
        /// </summary>
        [HttpGet] // Matching your existing pattern
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
            {
                return Json(new { success = false, status = "Bad Request", message = "File ID cannot be null." }, JsonRequestBehavior.AllowGet);
            }

            var result = await _manualService.DeleteFileByIdAsync(KEY);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult IndexValidate()
        {
            // Populate the Status dropdown for the view
            ViewBag.Statuses = new SelectList(new[]
            {
            new { Value = "Pending", Text = "Pending" },
            new { Value = "Approved", Text = "Approved" },
            new { Value = "Rejected", Text = "Rejected" }
        }, "Value", "Text");

            return View(); // <-- Returns ~/Views/FileValidation/Index.cshtml
        }

        [HttpPost]
        public async Task<ActionResult> LoadFiles(FileValidationRequestDto request)
        {
            try
            {
                if (request == null) request = new FileValidationRequestDto();
                if (request.DataTableOptions == null) request.DataTableOptions = new DataTableOptions();

                var files = (await _manualService.GetFilesByStatusAsync(request.StatusFilter)).ToList();
                var recordsTotal = files.Count;

                var pagedData = files
                    .Skip(request.DataTableOptions.start)
                    .Take(request.DataTableOptions.length)
                    .ToList();

                var resultData = pagedData.Select(f =>
                {
                    var actionsHtml = $@"
                <div class='btn-group'>
                  <button class='btn btn-sm btn-primary dropdown-toggle' type='button' data-bs-toggle='dropdown' aria-expanded='false'>Actions</button>
                  <ul class='dropdown-menu'>
                    <li><a class='dropdown-item js-validate' href='#' data-fileid='{f.FileUploadId}' data-approvedby='{Session["FullName"]}'>Validate</a></li>
                    <li><a class='dropdown-item js-review' href='#' data-fileid='{f.FileUploadId}' data-approvedby='{Session["FullName"]}'>Review</a></li>
                    <li><a class='dropdown-item js-delete text-danger' href='#' data-fileid='{f.FileUploadId}'>Delete</a></li>
                  </ul>
                </div>";

                    return new
                    {
                        fileName = f.FileName,
                        branchName = f.BranchName,
                        uploadedBy = f.UploadedBy,
                        uploadedOn = f.UploadedOn.ToString("yyyy-MM-dd HH:mm"),
                        status = f.SalaryProcessingStatus,
                        actions = actionsHtml
                    };
                }).ToList();

                return Json(new
                {
                    draw = request.DataTableOptions.draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = resultData
                });
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }


        // Action to handle the form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitValidation(FileValidationRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill all required fields." });
            }

            var result = await _manualService.ValidateFileAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        // GET partial used for both Validate & Review
        [HttpGet]
        public ActionResult GetValidationForm(string fileUploadId, string approvedBy, string mode)
        {
            // mode = "validate" or "review"
            var model = new ValidationFormViewModel
            {
                FileUploadId = fileUploadId,
                ApprovedBy = approvedBy,
                Mode = mode // "validate" or "review"
            };

            // _ValidationForm.cshtml is the partial below
            return PartialView("_ValidationForm", model);
        }

        // POST validate
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ValidateFile(ValidationDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Json(new { success = false, message = "Validation failed." });
        //    }

        //    try
        //    {
        //        // call your service to validate
        //        var result = await _manualService.ApproveFileAsync(dto.FileUploadId, dto.ApprovedBy, dto.ApprovalStatement);
        //        if (result.IsSuccess) return Json(new { success = true, message = "File validated successfully" });
        //        return Json(new { success = false, message = result.Message ?? "Could not validate file" });
        //    }
        //    catch (Exception ex)
        //    {
        //        // log ex
        //        return Json(new { success = false, message = "An error occurred while validating file." });
        //    }
        //}

        //// POST review
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ReviewFile(ReviewDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Json(new { success = false, message = "Validation failed." });
        //    }

        //    try
        //    {
        //        // call your service to mark as reviewed
        //        var result = await _manualService.ReviewFileAsync(dto.FileUploadId, dto.ApprovedBy, dto.ReviewerStatement);
        //        if (result.IsSuccess) return Json(new { success = true, message = "File submitted for review successfully" });
        //        return Json(new { success = false, message = result.Message ?? "Could not submit review" });
        //    }
        //    catch (Exception ex)
        //    {
        //        // log ex
        //        return Json(new { success = false, message = "An error occurred while submitting review." });
        //    }
        //}

        //// POST delete
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteFile(string fileUploadId)
        //{
        //    if (string.IsNullOrEmpty(fileUploadId)) return Json(new { success = false, message = "Invalid file id" });

        //    try
        //    {
        //        var result = await _manualService.DeleteFileAsync(fileUploadId);
        //        if (result.IsSuccess) return Json(new { success = true, message = "File deleted successfully" });
        //        return Json(new { success = false, message = result.Message ?? "Could not delete file" });
        //    }
        //    catch (Exception ex)
        //    {
        //        // log ex
        //        return Json(new { success = false, message = "An error occurred while deleting file." });
        //    }
        //}

    }

}
