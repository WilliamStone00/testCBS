using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using StringValues = CBS.FrontDesk.Data.Entity.StringValues;


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
            return View(new FileUploadResponse());
        }
        [HttpGet]
        public async Task<ActionResult> GetCollectorsByBranch(string branchId)
        {
            var collectors = await _manualService.GetCollectorsAsSelectListAsync(branchId);
            return Json(collectors,JsonRequestBehavior.AllowGet);
        }


        // Helper to load ViewBag data
        private async Task Loader()
        {
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Extracted", "Rejected", "Treated", "Completed" });
            ViewBag.Branches = await _branchServices.GetBranches();
            ViewBag.Collectors = new List<StringValues>();
        }

        // ACTION 2: The central router for loading partial views.
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                // IMPORTANT: For a server-side DataTable, we just return the EMPTY partial view.
                // The table will make its own AJAX call to get the data.
                await Loader();
                return PartialView(partialView);
            }
            else if (path == "new")
            {
                await Loader();
                return PartialView(partialView, new FileUploadResponse());
            }
            else // "details" view
            {
                var data = await _manualService.GetFileDetailsByIdAsync(KEY);
                return PartialView(partialView, data);
            }
        }

        // ACTION 3: The dedicated endpoint for the server-side DataTable.
        //[HttpPost]
        //public async Task<ActionResult> LoadFilesForDataTable(GetFilesForDataTableQuery query)
        //{
        //    try
        //    {
        //        var dataTable = await _manualService.GetFilesForDataTableAsync(query);

        //        // Deserialize the data into a type that has the properties we need
        //        var fileList = JsonConvert.DeserializeObject<List<FileUploadResponse>>(
        //            JsonConvert.SerializeObject(dataTable.data)
        //        );

        //        return Json(new
        //        {
        //            draw = query.Options?.draw ?? "1",
        //            recordsTotal = dataTable.recordsTotal,
        //            recordsFiltered = dataTable.recordsFiltered,
        //            data = fileList // Send the raw data; JavaScript will render it
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading file data.");
        //    }
        //}
        [HttpGet]
        public async Task<ActionResult> GetFileDetailsModal(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

            var model = await _manualService.GetFileDetailsByIdAsync(id);
            if (model == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "File not found");

            return PartialView("_FileDetails", model); // your existing detail PV
        }

        [HttpPost]
        public async Task<ActionResult> LoadFilesForDataTable(GetFilesForDataTableQuery query)
        {
            try
            {
                // If client sent JSON (content-type application/json), read and deserialize it.
                // This handles the new client behavior that sends JSON in the request body.
                var contentType = Request.ContentType ?? string.Empty;
                if (contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Request.InputStream.Position = 0;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        var body = await reader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            // body can be { "query": { ... } } or just { ... }
                            try
                            {
                                var wrapper = JsonConvert.DeserializeObject<JObject>(body);
                                if (wrapper != null && wrapper["query"] != null)
                                {
                                    query = wrapper["query"].ToObject<GetFilesForDataTableQuery>();
                                }
                                else
                                {
                                    query = wrapper.ToObject<GetFilesForDataTableQuery>();
                                }
                            }
                            catch (JsonException)
                            {
                                // ignore — leave the model binder's value in 'query' if json fails
                            }
                        }
                    }
                }

                // Ensure query.Options is non-null (your constructor already does this but be safe)
                if (query == null)
                {
                    query = new GetFilesForDataTableQuery();
                }
                var dataTable = await _manualService.GetFilesForDataTableAsync(query);

                var fileList = JsonConvert.DeserializeObject<List<FileUploadResponse>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                return Json(new
                {
                    draw = dataTable.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = fileList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception server-side (not shown here). Return JSON and set 500 status code.
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { error = "Error loading file data.", details = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// ACTION 3: Handles the AJAX file upload from the _UploadForm.
        /// </summary>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> UploadFile(string BranchId, string CollectorAndUser, HttpPostedFileBase UploadedFile, DateTime)
        //{
        //    // ... (Manual validation for parameters) ...
        //    if (UploadedFile == null || UploadedFile.ContentLength == 0)
        //        return Json(new { success = false, message = "Please select a file to upload." });
        //    if (string.IsNullOrWhiteSpace(BranchId))
        //        return Json(new { success = false, message = "Please select a branch." });
        //    if (string.IsNullOrWhiteSpace(CollectorAndUser))
        //        return Json(new { success = false, message = "Please select a collector." });

        //    try
        //    {
        //        var collectorAndUserParts = CollectorAndUser.Split('|');
        //        var collectorId = collectorAndUserParts[0];
        //        var userId = collectorAndUserParts[1];

        //        var response = await _manualService.UploadManualEntryFileAsync(UploadedFile, BranchId, collectorId, userId);

        //        if (response.IsSuccess && response.ApiResponseData.Success)
        //        {
        //            return Json(new
        //            {
        //                success = true,
        //                message = response.ApiResponseData.Message ?? "File processed successfully.",
        //                data = response.ApiResponseData.Data // Return the COMPLETE data object
        //            });
        //        }
        //        return Json(new { success = false, message = response.ApiResponseData?.Message ?? response.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log ex
        //        return Json(new { success = false, message = "An unexpected server error occurred." });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFile(string BranchId,string CollectorAndUser,HttpPostedFileBase UploadedFile,string AccountingDate)
        {   if (UploadedFile == null || UploadedFile.ContentLength == 0)
                return Json(new { success = false, message = "Please select a file to upload." });

            if (string.IsNullOrWhiteSpace(BranchId))
                return Json(new { success = false, message = "Please select a branch." });

            if (string.IsNullOrWhiteSpace(CollectorAndUser))
                return Json(new { success = false, message = "Please select a collector." });

            if (string.IsNullOrWhiteSpace(AccountingDate))
                return Json(new { success = false, message = "Please select a date." });

            // --- 2) Parse and validate the date ---
            DateTime parsedDate;
            if (!DateTime.TryParseExact(AccountingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return Json(new { success = false, message = "Invalid date format." });
            }

            if (parsedDate == DateTime.MinValue || parsedDate == new DateTime(1, 1, 1))
            {
                return Json(new { success = false, message = "Invalid date selection." });
            }

            if (parsedDate.Date > DateTime.Now.Date)
            {
                return Json(new { success = false, message = "Date cannot be in the future." });
            }

            try
            {
                // --- 3) Extract collector & user ---
                var collectorAndUserParts = CollectorAndUser.Split('|');
                var collectorId = collectorAndUserParts[0];
                var userId = collectorAndUserParts[1];

                // --- 4) Pass date into your service if needed ---
                var response = await _manualService.UploadManualEntryFileAsync(UploadedFile,BranchId,collectorId,userId, AccountingDate );

                // --- 5) Return result ---
                if (response.IsSuccess && response.ApiResponseData.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = response.ApiResponseData.Message ?? "File processed successfully.",
                        data = response.ApiResponseData.Data
                    });
                }

                return Json(new { success = false, message = response.ApiResponseData?.Message ?? response.Message });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return Json(new { success = false, message = "An unexpected server error occurred." });
            }
        }


        /// <summary>
        /// ACTION 4: Handles the "Download Template" button click.
        /// </summary>
        public ActionResult DownloadTemplate()
        {
            string physicalPath = Server.MapPath("~/AppFiles/ManualDailyCollection/Manualdaillycollectiontemplete.xlsx");
            if (!System.IO.File.Exists(physicalPath)) return HttpNotFound("Template file not found.");
            byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DailyCollectorFieldReport_Template.xlsx");
        }

        [HttpPost]
        public async Task<ActionResult> extract(string KEY)
        {
            // 1. Validate the input key
            if (string.IsNullOrEmpty(KEY))
            {
                // Return a JSON error object in the expected format
                return Json(new
                {
                    status = "ERROR",
                    statusDescription = "Bad Request: File ID cannot be null.",
                    data = (object)null
                });
            }

            try
            {
                // 2. Call your service to get the extraction result
                var result = await _manualService.ExtractFileAsync(KEY);

                // 3. Check if the service call was successful AND returned data
                //    IMPORTANT: You may need to change 'result.Data' to the correct property name
                //    that holds your extracted file details (e.g., result.Payload, result.ExtractedData)
                if (result != null && result.Result == true && result.Data != null)
                {
                    // 4. On SUCCESS, return the full data payload inside the 'data' property
                    return Json(new
                    {
                        status = "SUCCESS",
                        statusDescription = Messaging.MessageResult(result), // Use your existing message helper
                        data = result.Data // THIS IS THE CRITICAL FIX
                    });
                }
                else
                {
                    // 5. On FAILURE, return an error message.
                    return Json(new
                    {
                        status = "ERROR",
                        statusDescription = Messaging.MessageResult(result) ?? "Failed to extract file. No data returned.",
                        data = (object)null
                    });
                }
            }
            catch (System.Exception ex)
            {
                // 6. If an unexpected exception occurs, log it and return a generic error.
                // TODO: Log the full exception ex.ToString()
                return Json(new
                {
                    status = "ERROR",
                    statusDescription = "A critical server error occurred during the extraction process.",
                    data = (object)null
                });
            }
        }

        /// <summary>
        /// ACTION 5: Deletes an uploaded file by its ID. Works with your generic DeleteRecordDataTable helper.
        /// </summary>
        [HttpGet] // Matching your existing pattern, but [HttpPost] is recommended for security.
        public async Task<ActionResult> Delete(string fileUploadId)
        {
            if (string.IsNullOrEmpty(fileUploadId))
            {
                return Json(new { success = false, status = "Bad Request", message = "File ID cannot be null." }, JsonRequestBehavior.AllowGet);
            }
            var result = await _manualService.DeleteFileByIdAsync(fileUploadId);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) }, JsonRequestBehavior.AllowGet);
        }
    }
}