using CBS.API.Helper;
using CBS.BusinessService.Communication;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Communication;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Communication
{
    /// <summary>
    /// Communication Controller (FrontDesk MVC5)
    /// Handles:
    ///  - SMS Upload Preview (Excel upload -> parse rows -> show preview)
    ///  - SMS Upload Send (send all rows for an uploaded file)
    ///  - SMS Upload DataTable (list uploaded files)
    ///  - SMS Upload Download by Id
    ///
    /// API Source: Communication microservice (api/v1/SmsUpload/*)
    /// </summary>
    [CheckSessionTimeOutAttribute]
    public class CommunicationController : BaseController
    {
        private readonly CommunicationServices _communicationServices;
        private readonly BranchServices _branchServices;

        /// <summary>
        /// Initializes controller.
        /// </summary>
        public CommunicationController(CommunicationServices communicationServices, BranchServices branchServices)
        {
            _communicationServices = communicationServices ?? throw new ArgumentNullException(nameof(communicationServices));
            _branchServices = branchServices ?? throw new ArgumentNullException(nameof(branchServices));
        }

        // ============================================================
        // VIEWS
        // ============================================================

        /// <summary>
        /// Main page for SMS upload module.
        /// </summary>
        public async Task<ActionResult> SmsUpload()
        {
            await loadPreInformation();
            return View();
        }

        /// <summary>
        /// Listing page (uploaded files table).
        /// </summary>
        public async Task<ActionResult> Index()
        {
           await loadPreInformation();
            return View();
        }

        /// <summary>
        /// Listing page (uploaded files table).
        /// </summary>
        public async Task<ActionResult> SmsUploadDetails(string fileId)
        {
           //await loadPreInformation();
            var smsFileUploadDetails= await _communicationServices.GetSmsUploadDetaisAsync(fileId);

            if (smsFileUploadDetails == null)
            {
                return HttpNotFound();
            }
            return View(smsFileUploadDetails);
        }

        private async Task loadPreInformation() {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
        }

        // ============================================================
        // PREVIEW (UPLOAD FILE)
        // ============================================================

        /// <summary>
        /// Uploads an SMS Excel file and returns preview result.
        /// - Endpoint called: POST api/v1/SmsUpload/preview
        /// - Returns preview rows and computed message bodies.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> PreviewSmsUpload(HttpPostedFileBase file, string branchId = null, string defaultMessageTemplate = null, string senderService = null, string title = null, string purpose = null)
        {
            try
            {
                if (file == null || file.ContentLength <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        status = false,
                        message = "Please select a valid Excel file (XLSX/XLS)."
                    }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(branchId))
                {
                    return Json(new { success = false, status = false, message = "Branch is required." }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrWhiteSpace(defaultMessageTemplate))
                {
                    return Json(new { success = false, status = false, message = "Message Template is required." }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(senderService))
                {
                    return Json(new { success = false, status = false, message = "Sender Service is required." }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    return Json(new { success = false, status = false, message = "Title is required." }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(purpose))
                {
                    return Json(new { success = false, status = false, message = "Purpose is required." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _communicationServices.PreviewSmsUploadAsync(file,branchId, defaultMessageTemplate,senderService,title,purpose);

                if (result == null || !result.IsSuccess || result.ApiResponseData == null)
                {
                    return Json(new
                    {
                        success = false,
                        status = false,
                        message = result == null ? "Failed to preview file." : (result.Message ?? "Failed to preview file."),
                        error = result?.ApiResponseData?.Errors
                    }, JsonRequestBehavior.AllowGet);
                }

                // DataTables friendly response (like your StandingOrder preview)
                // ApiResponseData.Data is your SmsUploadPreviewSummaryDto
                var preview = result.ApiResponseData.Data;

                return Json(new
                {
                    draw = Request.Form["draw"] ?? "1",
                    recordsTotal = preview?.Rows?.Count ?? 0,
                    recordsFiltered = preview?.Rows?.Count ?? 0,
                    data = preview,
                    success = true,
                    status = true,
                    message = "Preview generated successfully."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    status = false,
                    message = "An error occurred while processing your file.",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // ============================================================
        // SEND SMS FOR UPLOAD
        // ============================================================

        /// <summary>
        /// Sends all SMS rows for an existing upload (FileUploadId).
        /// - Endpoint called: POST api/v1/SmsUpload/send
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> SendSmsUpload(SendSmsFileUploadCommand model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, status = false, message = "Invalid request." }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(model.FileUploadId))
                {
                    return Json(new { success = false, status = false, message = "FileUploadId is required." }, JsonRequestBehavior.AllowGet);
                }

                

                var result = await _communicationServices.SendSmsUploadAsync(model);

                if (result == null || !result.IsSuccess || result.ApiResponseData == null)
                {
                    return Json(new
                    {
                        success = false,
                        status = false,
                        message = result == null ? "SMS send failed." : (result.Message ?? "SMS send failed."),
                        error = result?.ApiResponseData?.Errors
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    status = true,
                    message = "SMS upload send completed.",
                    data = result.ApiResponseData.Data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    status = false,
                    message = "An error occurred while sending SMS.",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // ============================================================
        // DATATABLE (LIST UPLOADS)
        // ============================================================

        /// <summary>
        /// Loads SMS file uploads in DataTables format.
        /// - Endpoint called: POST api/v1/SmsUpload/datatable
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> LoadSmsUploadData(GetSmsFileUploadDataTableQuery query)
        {
            try
            {
                if (!_communicationServices.IsHeadOffice())
                {
                    query.BranchId=_communicationServices.GetBranchID();
                }

                var dt = await _communicationServices.GetSmsUploadDataTableAsync(query);

                // dt is CustomDataTable (your standard)
                // deserialize into your concrete view model if needed
                // otherwise return raw dt.data
                var uploads = JsonConvert.DeserializeObject<List<SmsFileUploadDto>>(JsonConvert.SerializeObject(dt.data));

                return Json(new
                {
                    draw = dt.draw,
                    recordsTotal = dt.recordsTotal,
                    recordsFiltered = dt.recordsFiltered,
                    data = uploads
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        // ============================================================
        // DATATABLE (LIST UPLOADS)
        // ============================================================

        /// <summary>
        /// Loads SMS file uploads in DataTables format.
        /// - Endpoint called: POST api/v1/SmsUpload/datatable
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetSmsFileUploadHistoryDataTable(GetSmsFileUploadHistoryDataTableQuery query)
        {
            try
            {
                if (!_communicationServices.IsHeadOffice())
                {
                    query.BranchId=_communicationServices.GetBranchID();
                }

                var dt = await _communicationServices.GetSmsFileUploadHistoryDataTableAsync(query);

                // dt is CustomDataTable (your standard)
                // deserialize into your concrete view model if needed
                // otherwise return raw dt.data
                var uploads = JsonConvert.DeserializeObject<List<SmsFileUploadHistoryDto>>(JsonConvert.SerializeObject(dt.data));

                return Json(new
                {
                    draw = dt.draw,
                    recordsTotal = dt.recordsTotal,
                    recordsFiltered = dt.recordsFiltered,
                    data = uploads
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> DownloadSmsUploadTemplate()
        {
            try
            {
                const string fileName = "BulkSmsExecutionTemplate.xlsx";
                string directoryPath = Server.MapPath("~/AppFiles/Communication");

                // Validate directory exists
                if (!Directory.Exists(directoryPath))
                {
                    return Json(new { success = false, status = false, message = "Communication template directory not found" });
                }

                string filePath = Path.Combine(directoryPath, fileName);

                // Validate file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, status = false, message = "Template file not found" });
                }

                // Read file asynchronously
                byte[] fileBytes;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    fileBytes = new byte[fileStream.Length];
                    await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
                }

                // Return the file
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, status = false, message = "Access denied to template file" });
            }
            catch (IOException ex)
            {
                return Json(new { success = false, status = false, message = $"Error reading template file: {ex.Message}" });
            }
            catch (Exception ex)
            {

                // Log the exception here
                return Json(new { success = false, status = false, message = $"An unexpected error occurred: {ex.Message}" });
            }
        }

        // ============================================================
        // DOWNLOAD FILE BY ID
        // ============================================================

        /// <summary>
        /// Downloads an uploaded SMS file by FileId (Mongo Id or FileUploadId depending on your API).
        /// - Endpoint called: GET api/v1/SmsUpload/download/{fileId}
        /// </summary>
        public async Task<ActionResult> DownloadSmsUploadFile(string fileId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileId))
                    return Json(new { success = false, status = false, message = "FileId is required." }, JsonRequestBehavior.AllowGet);

                var result = await _communicationServices.DownloadSmsUploadFileByIdAsync(fileId);

                if (result == null || !result.IsSuccess || result.ApiResponseData == null || result.ApiResponseData.Data == null)
                {
                    return Json(new
                    {
                        success = false,
                        status = false,
                        message = result == null ? "Download failed." : (result.Message ?? "Download failed."),
                        error = result?.ApiResponseData?.Errors
                    }, JsonRequestBehavior.AllowGet);
                }

                var dto = result.ApiResponseData.Data;

                // Assumption: SmsFileDownloadDto has:
                //   - FileName
                //   - ContentType
                //   - FileBytes (byte[]) OR Base64Content (string)
                //
                // Adjust these mappings to your real DTO.

                byte[] fileBytes = null;

                if (dto.FileData != null && dto.FileData.Length > 0)
                {
                    fileBytes = dto.FileData;
                }
                /*else if (!string.IsNullOrWhiteSpace(dto.Base64Content))
                {
                    fileBytes = Convert.FromBase64String(dto.Base64Content);
                }*/
                else
                {
                    return Json(new { success = false, status = false, message = "File content is empty." }, JsonRequestBehavior.AllowGet);
                }

                var contentType = string.IsNullOrWhiteSpace(dto.ContentType)
                    ? "application/octet-stream"
                    : dto.ContentType;

                var fileName = string.IsNullOrWhiteSpace(dto.FileName)
                    ? "SmsUploadFile.xlsx"
                    : dto.FileName;

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    status = false,
                    message = "An error occurred while downloading the file.",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }

    
}
