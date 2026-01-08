using BusinessServices;
using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.EndOfYearClosure;
using CBS.BusinessService.AccountingV2.InterestProductConfig;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.AccountingV2.SharedMonth;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.SharedMonth
{
    public class SharedMonthSimulationController : Controller
    {

        private readonly SharedMonthSimulationService _sharedMonthSimulationService;
        private readonly BranchServices _branchServices;
        private readonly InterestProductConfigService _interestProductConfigService;
        
        private readonly string _appFilesRoot;

        // Allowed extensions for these templates (adjust if needed)
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };

        public SharedMonthSimulationController(BranchServices branchServices, SharedMonthSimulationService sharedMonthSimulationService, InterestProductConfigService interestProductConfigService)
        {
            _sharedMonthSimulationService = sharedMonthSimulationService;
            _branchServices = branchServices;
            _interestProductConfigService = interestProductConfigService;
             

            _appFilesRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "AppFiles");
        }
        // GET: SharedMonthSimulation
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }
        private List<StringValues> getAccountingYears()
        {
            int currentYear = DateTime.Now.Year;

            return Enumerable.Range(currentYear - 3, 5)
                .Select(y => new StringValues
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                })
                .ToList();
        }


        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            ViewBag.IntrestDistributionType = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "OrdinaryShare", Text = " Ordinary Share" },
                        new SelectListItem { Value = "PreferenceShare", Text = "Preference Share" }
                    };

            ViewBag.AccountType = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "OrdinaryShare", Text = " Ordinary Share" },
                        new SelectListItem { Value = "PreferenceShare", Text = "Preference Share" },
                        new SelectListItem { Value = "Savings", Text = "Savings " }
                    };

            ViewBag.Month = new List<SelectListItem>
            {
                new SelectListItem { Value = "January", Text = "January" },
                new SelectListItem { Value = "February", Text = "February" },
                new SelectListItem { Value = "March", Text = "March" },
                new SelectListItem { Value = "April", Text = "April" },
                new SelectListItem { Value = "May", Text = "May" },
                new SelectListItem { Value = "June", Text = "June" },
                new SelectListItem { Value = "July", Text = "July" },
                new SelectListItem { Value = "August", Text = "August" },
                new SelectListItem { Value = "September", Text = "September" },
                new SelectListItem { Value = "October", Text = "October" },
                new SelectListItem { Value = "November", Text = "November" },
                new SelectListItem { Value = "December", Text = "December" },
                 new SelectListItem { Value = "Anaully", Text = "Anaully" }
            };

            ViewBag.Year = getAccountingYears();


            ViewBag.FileType = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "MonthlyUnprocessData", Text = " Monthly Unprocessed Data" },
                        new SelectListItem { Value = "MonthlyprocessData", Text = "Monthly processed Data" },
                        new SelectListItem { Value = "AnnualprocessData", Text = " Anual processed Data" }
                    };


            var products = await _interestProductConfigService.GetProductAsync();

            // PRODUCTS DROPDOWN
            ViewBag.Products = products?
                .Select(p => new SelectListItem
                {
                    Value = p.Id,                       // ProductId posted
                    Text = $" {p.Name}"         // [Code] [Name] shown
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();

            return true;
        }
        public ActionResult AnnualprocessData()
        {
            string relative = Path.Combine("ShareMonthUpload", "Anual data.xlsx");
            return ServeFileFromAppFiles(relative, "Template file for unprocessed data not found.");
        }
        public ActionResult MonthlyprocessData()
        {
            string relative = Path.Combine("ShareMonthUpload", "Processed Data.xlsx");
            return ServeFileFromAppFiles(relative, "Template file for unprocessed data not found.");
        }
        public ActionResult MonthlyUnprocessData()
        {
            string relative = Path.Combine("ShareMonthUpload", "Unprocessed data.xlsx");
            return ServeFileFromAppFiles(relative, "Template file for unprocessed data not found.");
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



        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }

        public async Task<ActionResult> Data()
        {
            await loader();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> GenerateSimulationData(InstrestCalculation model)
        {

            model.StartDate = model.StartDate.Date;
            model.EndDate = model.EndDate.Date;
            try
            {
                var result = await _sharedMonthSimulationService.ActivateSimulationAsync(model);

                //if (result == null == false)
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "No simulation data found"
                //    });
                //}

                return Json(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Simulation completed successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Simulation failed: {ex.Message}"
                });
            }
        }



        [HttpPost]
        public async Task<ActionResult> LoadSimulationData(SharedMonthSimulation model)
        {
            try
            {
                var result = await _sharedMonthSimulationService.GetSimulationData(model);

                if (result == null || !result.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No simulation data found"
                    });
                }

                return Json(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Simulation completed successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Simulation failed: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ProceedRequestData(SimulationFilterRequest model)
        {
            try
            {
                var result = await _sharedMonthSimulationService.GetRequestDataAsync(model);

                var response = new
                {
                    success = true,
                    statusCode = 200,
                    message = "Simulation completed successfully",
                    data = result
                };

                // 🔥 FORCE ISO DATES (NO /Date(...) )
                return Content(
                    JsonConvert.SerializeObject(response, new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }),
                    "application/json"
                );
            }
            catch (Exception ex)
            {
                return Content(
                    JsonConvert.SerializeObject(new
                    {
                        success = false,
                        statusCode = 500,
                        message = ex.Message
                    }),
                    "application/json"
                );
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveSimulation(CreateSimulations model)
        {
            if (model == null || model.ShareMonthPsiReportLineDto == null || !model.ShareMonthPsiReportLineDto.Any())
            {
                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = "Simulation data is required"
                });
            }

            var Branch = await _branchServices.GetBranch(model.BranchId);
            model.BranchName = Branch?.Name ?? "—";

            try
            {
                var result = await _sharedMonthSimulationService.CreateShareMonthSimulationAsync(model);

                if (result == null)
                {
                    return Json(new
                    {
                        success = false,
                        statusCode = 500,
                        message = "No response from simulation service."
                    });
                }

                if (result.IsSuccess && result.ApiResponseData?.Data != null)
                {
                    return Json(new
                    {
                        success = true,
                        statusCode = 200,
                        message = result.ApiResponseData.Data.Description
                                  ?? "Simulation saved successfully",
                        data = result.ApiResponseData.Data
                    });
                }

                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message ?? "Simulation save failed",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Simulation save failed: {ex.Message}"
                });
            }
        }

        [HttpPost]
       
        public async Task<ActionResult> UploadSharedMonth(SharedMonthFileupload model)
        {
            try
            {
                // Basic validation
                if (model == null)
                    return Json(new { success = false, message = "Invalid request." });

                if (model.file == null || model.file.ContentLength == 0)
                    return Json(new { success = false, message = "No file provided." });

                if (string.IsNullOrWhiteSpace(model.BranchId))
                    return Json(new { success = false, message = "Branch is required." });


                


                // Call service
                var response = await _sharedMonthSimulationService.SharedMonthUpload(model);

                if (response?.IsSuccess == true && response.ApiResponseData != null)
                {
                    return Json(new
                    {
                        success = true,
                        message = response.ApiResponseData.Message ?? "File uploaded successfully.",
                        data = response.ApiResponseData.Data
                    });
                }

                // Service-level failure
                var msg = response?.ApiResponseData?.Message
                          ?? response?.Message
                          ?? "Upload failed.";

                return Json(new { success = false, message = msg });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetAccountingYearsByBranch(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            try
            {
                var years = await _sharedMonthSimulationService
                    .GetOpenAccountingYearsByBranchAsync(branchId);

                var result = years
                    .OrderByDescending(y => y.Year) // or Year
                    .Select(y => new
                    {
                        Id = y.Id,   // ✅ Sent to server on submit
                        DisplayName = $"{y.Year}" // customize
                    });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




    }
}