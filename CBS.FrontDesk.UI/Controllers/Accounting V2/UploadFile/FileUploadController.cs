using CBS.BusinessService.Accounting_V2;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.FilesUpload;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FileUpload;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Message;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.UploadFile
{
    public class FileUploadController : Controller
    {
        private readonly FileUploadService _fileUploadService;
        private readonly BranchServices _branchServices;
        private readonly AffiliateAccountMockService _affiliateAccountMockService;
        private readonly AffiliateService _affiliateService;
        private readonly ChartOfAccountsV2Service _chartOfAccountsV;
        private readonly AffiliateAccountService _affiliateAccountService;
        // Root folder (absolute) that contains downloadable files
        private readonly string _appFilesRoot;

        // Allowed extensions for these templates (adjust if needed)
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };
        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public FileUploadController(AffiliateService affiliateService, FileUploadService fileUploadService, BranchServices branchServices, AffiliateAccountMockService affiliateAccountMockService, ChartOfAccountsV2Service chartOfAccountsV, AffiliateAccountService affiliateAccountService)
        {
            _fileUploadService = fileUploadService;
            _branchServices = branchServices;
            _affiliateAccountMockService = affiliateAccountMockService;
            _affiliateService = affiliateService;
            _chartOfAccountsV = chartOfAccountsV;
            _affiliateAccountService = affiliateAccountService;
            // AppDomain.CurrentDomain.BaseDirectory is the application's root folder
            _appFilesRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "AppFiles");
        }
       

        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }

        private async Task loader()
        {
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Extracted", "Rejected", "Treated", "Completed" });
            ViewBag.Branches = await _branchServices.GetBranches();
            var affiliate = await _affiliateService.GetAffiliatesAsync();
            ViewBag.Affiliates = affiliate;
            var classes = _chartOfAccountsV.GetAllClass();
            ViewBag.Classes = classes;
        }


        /// <summary>
        /// Download Affiliate Template (absolute path, streams file)
        /// URL: /FileUpload/AffiliateTemplate
        /// </summary>
        [HttpGet]
        public ActionResult AffiliateTemplate()
        {
            // Relative path under AppFiles
            string relative = Path.Combine("AffiliateAccountUpload", "BAPCCUL_ACCOUNTS2.xlsx");
            return ServeFileFromAppFiles(relative, "Template file for Affiliate not found.");
        }

        /// <summary>
        /// Download Branch Account Template (absolute path, streams file)
        /// URL: /FileUpload/BranchAccountTemplate
        /// </summary>
        [HttpGet]
        public ActionResult BranchAccountTemplate()
        {
            string relative = Path.Combine("AffiliateAccountUpload", "SampleBalanceSheet2.xlsx");
            return ServeFileFromAppFiles(relative, "Template file for Branch Accounts not found.");
        }

        #region Helper
        /// <summary>
        /// Centralized secure file serving from AppFiles.
        /// </summary>
        private ActionResult ServeFileFromAppFiles(string relativePathUnderAppFiles, string notFoundMessage = "File not found.")
        {
            if (string.IsNullOrWhiteSpace(relativePathUnderAppFiles))
                return new HttpStatusCodeResult(400, "Invalid file path.");

            // Protect against weird input - normalize and combine
            string safeRelative = relativePathUnderAppFiles.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                                           .Replace('/', Path.DirectorySeparatorChar)
                                                           .Replace('\\', Path.DirectorySeparatorChar);

            string combined = Path.Combine(_appFilesRoot, safeRelative);

            // Resolve full path & guard against path traversal
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(combined);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(400, "Invalid file path.");
            }

            string rootFull = Path.GetFullPath(_appFilesRoot);
            if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            {
                // Attempted path traversal
                return new HttpStatusCodeResult(403, "Access denied.");
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return HttpNotFound(notFoundMessage);
            }

            // Validate extension
            string ext = Path.GetExtension(fullPath);
            if (string.IsNullOrEmpty(ext) || Array.IndexOf(AllowedExtensions, ext, 0) < 0)
            {
                return new HttpStatusCodeResult(403, "File type not allowed.");
            }

            try
            {
                // Use MIME mapping from System.Web
                string contentType = MimeMapping.GetMimeMapping(fullPath);
                string downloadFileName = Path.GetFileName(fullPath);

                // FilePathResult streams the file directly from disk (memory-friendly)
                return File(fullPath, contentType, downloadFileName);
            }
            catch (Exception ex)
            {
                // Optional: replace with your logger (ILogger / log4net / NLog, etc.)
                // e.g. _logger.LogError(ex, "Failed to serve file {FullPath}", fullPath);
                return new HttpStatusCodeResult(500, "An error occurred while processing the download.");
            }
        }
        #endregion
    

        ///// <summary>
        ///// ACTION 4: Handles the "Download Template" button click.
        ///// </summary>
        //public ActionResult AffiliateTemplate()
        //{
        //    string physicalPath = Server.MapPath("~AppFiles/AffiliateAccountUpload/BAPCCUL_ACCOUNTS2.xlsx");
        //    if (!System.IO.File.Exists(physicalPath)) return HttpNotFound("Template file for Affiliate not found.");
        //    byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
        //    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BAPCCUL_ACCOUNTS2.xlsx");
        //}

        ///// <summary>
        ///// ACTION 4: Handles the "Download Template" button click.
        ///// </summary>
        //public ActionResult BranchAccountTemplate()
        //{
        //    string physicalPath = Server.MapPath("~AppFiles/AffiliateAccountUpload/SampleBalanceSheet2.xlsx");
        //    if (!System.IO.File.Exists(physicalPath)) return HttpNotFound("Template file for Branch Accounts not found.");
        //    byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
        //    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleBalanceSheet2.xlsx");
        //}

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();

        }

        [HttpPost]
        public async Task<JsonResult> LoadAccountwaitingData(AccountwaitingCorrespondanceQuery query)
        {
            //await loader();
            try
            {

                var data = await _fileUploadService.AccountwaitingDataTableAsync(query);
                //var data = await _fileUploadService.AccountwaitingMockDataTableAsync(query);

                var Accountwaiting = JsonConvert.DeserializeObject<List<Accountwaiting>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Accountwaiting
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Corespondancelist()
        {
            await loader();
            return View();

        }


        [HttpPost]
        public async Task<JsonResult> LoadCorrespondanceDatatable(CorespondanceQUERY query)
        {
            //await loader();
            try
            {

                var data = await _fileUploadService.CorrespondanceDataTableAsync(query);

                var response = JsonConvert.DeserializeObject<List<CorrespondenceRequestDto>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = response
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await loader();
            if (path == "list")
            {
                var data = await _fileUploadService.GetAllAsync();
                return PartialView(partialView, data);

            }
            //GetRolePermissions
            else if (path == "new")
            {
                return PartialView(partialView, new AddCORRESPONDANCE());
            }

            else
            {
                var data = await _fileUploadService.GetByIdAsync(KEY);
                return PartialView(partialView, data);

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Addcorrespondance(AddCORRESPONDANCE model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _fileUploadService.CreateCorrespondanceAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(Accountwaiting model)
        {
            // Use IsNullOrWhiteSpace so empty string Ids don't behave like null
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed." });

                var result = await _fileUploadService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                // IMPORTANT: return the ActionResult from Update
                return await Update(model);
            }

            // unreachable now but keep for safety (or remove)
            // return Json(new { success = false, status = false, message = "Fillsss the required fields." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(Accountwaiting model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _fileUploadService.UpdateAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            var result = await _fileUploadService.DeactivateAsync(KEY);

            // Map to simple JSON shape the client expects. Adjust if result has different property names.
            bool success = result?.Result ?? false;
            string message = Messaging.MessageResult(result) ?? "Operation completed.";

            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Upload affiliate file (multipart/form-data).
        /// Expects a file and affiliateId as form fields.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadAffiliate(FileUpload model)
        {
            try
            {

                if (model.file == null)
                    return Json(new { success = false, message = "No file provided." });
                if (model.isAffiliate == true)
                {
                    var response = await _fileUploadService.AffiliateUpload(model);
                    if (response?.IsSuccess == true && response.ApiResponseData != null)
                    {
                        return Json(new
                        {
                            success = true,
                            data = response.ApiResponseData.Data,
                            message = response.ApiResponseData.Message ?? response.Message
                        });
                    }

                    // service-level error or failure
                    var msg = response?.ApiResponseData?.Message ?? response?.Message ?? "Upload failed";
                    return Json(new { success = false, message = msg });
                }
                else
                {
                    var response = await _fileUploadService.BranchUpload(model);
                    if (response?.IsSuccess == true && response.ApiResponseData != null)
                    {
                        return Json(new
                        {
                            success = true,
                            data = response.ApiResponseData.Data,
                            message = response.ApiResponseData.Message ?? response.Message
                        });
                    }

                    // service-level error or failure
                    var msg = response?.ApiResponseData?.Message ?? response?.Message ?? "Upload failed";
                    return Json(new { success = false, message = msg });
                }
            }
            catch (Exception ex)
            {
                // Consider logging ex
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload branch (A-Branch) file (multipart/form-data).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadBranch(FileUpload model)
        {
            try
            {
                if (model.file == null)
                    return Json(new { success = false, message = "No file provided." });


                var response = await _fileUploadService.BranchUpload(model);

                if (response?.IsSuccess == true && response.ApiResponseData != null)
                {
                    return Json(new
                    {
                        success = true,
                        data = response.ApiResponseData.Data,
                        message = response.ApiResponseData.Message ?? response.Message

                    });
                }

                var msg = response?.ApiResponseData?.Message ?? response?.Message ?? "Upload failed";
                return Json(new { success = false, message = msg });
            }
            catch (Exception ex)
            {
                // Consider logging ex
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(correspondanceR_A request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Id))
                return Json(new { isSuccess = false, message = "Invalid request" });

            try
            {
                var response = await _fileUploadService.CorrespondenceValidationAsync(request);
                return Json(new { isSuccess = false, message = "Failed to approve correspondence" });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "Server error while approving" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reject(correspondanceR_A request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Id))
                return Json(new { isSuccess = false, message = "Invalid request" });

            try
            {
                var ok = await _fileUploadService.RejectCorrespondenceAsync(request);
                return Json(new { isSuccess = false, message = "Failed to reject correspondence" });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "Server error while rejecting" });
            }
        }


        public async Task<ActionResult> GetCorrespondance(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            var data = await _fileUploadService.GetCorrespondanceByIdAsync(KEY);
            return PartialView(partialView, data);
        }

        [HttpGet]
        public async Task<ActionResult> GetCorrespondencePartial(string recordId, string name, string scope, string branchId,string hoPcmfAccountId = null,string affiliateAccountId = null,string branchAccountId = null)
        {
            // Basic normalization
            recordId = recordId?.Trim();
            name = name?.Trim();
            scope = scope?.Trim();
            branchId = branchId?.Trim();
            hoPcmfAccountId = hoPcmfAccountId?.Trim();
            affiliateAccountId = affiliateAccountId?.Trim();
            branchAccountId = branchAccountId?.Trim();

            // Resolve type from scope
            var type = string.Equals(scope, "Branch", StringComparison.OrdinalIgnoreCase)
                ? "BranchToAffiliate"
                : "AffiliateToHo";

            var accountPcmf =await _chartOfAccountsV.GetAllPCMFAccounts();

            ViewBag.ChartOfAccountPcmf = accountPcmf;

            // get full flat list (service can supply)
            var affiliateAccounts = await _affiliateAccountService.GetAllAffiliateAccounts();

            ViewBag.ChartOfAccountMFI= affiliateAccounts;
            // Build the model for the partial
            var model = new AddCORRESPONDANCE
            {
                // Required flow metadata
                Type = type,

                // Context you likely want in the form (rename to your actual props)
                Name = name,
                BranchId = branchId,
                HoPcmfAccountId = hoPcmfAccountId,
                AffiliateAccountId = affiliateAccountId,
                BranchAccountId = branchAccountId
            };

            // If your partial uses dropdowns, you can preload ViewBags here (optional)
            // ViewBag.Branches = await _branchRepo.GetAsSelectListAsync();
            // ViewBag.Affiliates = await _affiliateRepo.GetAsSelectListAsync();

            return PartialView("_AddCorrespondance", model);
        }


    }

}
