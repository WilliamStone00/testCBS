using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.FallBack;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FallBack;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.FallBack
{
    //[CheckSessionTimeOut]
    public class AffiliateToBranchFallbackController : Controller
    {
        private readonly AffiliateToBranchFallbackService _fallbackService;
        private readonly FallbackExcelExportGenerator _FallbackExcelExportGenerator;
        private readonly BranchAccountService _BranchAccountService;

        public AffiliateToBranchFallbackController( AffiliateToBranchFallbackService fallbackService, FallbackExcelExportGenerator fallbackExcelExportGenerator, BranchAccountService branchAccountService)
        {
            _fallbackService = fallbackService;
            _FallbackExcelExportGenerator = fallbackExcelExportGenerator;
            _BranchAccountService = branchAccountService;
        }


        [HttpGet]
        public async Task<ActionResult> List()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LoadFallbackData(FallbackLogQuery query)
        {
            try
            {
                var data = await _fallbackService.GetFallbackLogDataTableAsync(query);

                var fallbackLogs = JsonConvert.DeserializeObject<List<FallbackLogResponse>>(JsonConvert.SerializeObject(data.data));

                // Calculate reconciliation statistics
                int toBeReconciled = 0;
                int alreadyReconciled = 0;
                int total = fallbackLogs?.Count ?? 0;

                if (fallbackLogs != null)
                {
                    foreach (var log in fallbackLogs)
                    {
                        if (log.isResolved)
                            alreadyReconciled++;
                        else
                            toBeReconciled++;
                    }
                }

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = fallbackLogs,
                    statistics = new
                    {
                        toBeReconciled,
                        alreadyReconciled,
                        total
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.DataTable?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    statistics = new
                    {
                        toBeReconciled = 0,
                        alreadyReconciled = 0,
                        total = 0
                    },
                    error = ex.Message
                });
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "details")
            {
                var data = await _fallbackService.GetByIdAsync(KEY);

                // Build reconcile model from the fetched data
                var reconcile = new CBS.FrontDesk.Data.Entity.Accounting_V2.FallBack.ReconcileFallbackRequest
                {
                    Id = data.id,
                    SourceGlAccountNumber = data.supposedGlAccountNumber,
                    SourceAmount = data.amount,
                    SourceSit = data.operationCode,
                    SourceDescription = data.reference
                };

                var BranchAccount = await _BranchAccountService.GetAllBranchAccountsFromDataTableAsync(data.branchId);

                // Create a view model that contains both the main data and the reconcile model
                var viewModel = new FallbackDetailsViewModel
                {
                    FallbackData = data,
                    ReconcileModel = reconcile,
                    BranchAccount = BranchAccount
                };

                ViewBag.BranchAccount = BranchAccount;
                ViewData["ReconcileModel"] = reconcile;

                return PartialView(partialView, viewModel);
            }
            else if (path == "reconcile")
            {
                var data = await _fallbackService.GetByIdAsync(KEY);
                var reconcileModel = new ReconcileFallbackRequest
                {
                    Id = data?.id,
                    SourceGlAccountNumber = data?.supposedGlAccountNumber,
                    SourceAmount = data?.amount ?? 0,
                    SourceSit = data?.operationCode,
                    SourceDescription = $"Affiliate: {data?.requestedAffiliateAccountIdOrBranchCode}",

                    DestinationGlAccountNumber = data?.fallbackBranchAccountNumber,
                    DestinationAmount = data?.amount ?? 0,
                    DestinationSit = data?.operationCode,
                    DestinationDescription = $"Fallback: {data?.fallbackBranchAccountName}"
                };
                return PartialView(partialView, reconcileModel);
            }
            else
            {
                return PartialView(partialView, new FallbackLogResponse());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reconcile(ReconcileFallbackRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var resolveRequest = new ResolveFallbackRequest
            {
                Id = model.Id,
                SourceGlAccountNumber = model.SourceGlAccountNumber,
                SourceAmount = model.SourceAmount,
                SourceSit = model.SourceSit,
                SourceDescription = model.SourceDescription,
                DestinationGlAccountNumber = model.DestinationGlAccountNumber,
                DestinationAmount = model.DestinationAmount,
                DestinationSit = model.DestinationSit,
                DestinationDescription = model.DestinationDescription
            };

            var result = await _fallbackService.ResolveAsync(resolveRequest);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<ActionResult> ExportFallbackData(FallbackExportRequest request)
        {
            try
            {
                Console.WriteLine($"=== FALLBACK EXPORT DEBUG START ===");
                Console.WriteLine($"Request received: {request != null}");
                Console.WriteLine($"Data count: {request?.Data?.Count ?? 0}");
                Console.WriteLine($"Total Records: {request?.TotalRecords ?? 0}");
                Console.WriteLine($"Export Options: {request?.ExportOptions?.FileName ?? "N/A"}");

                if (request?.Data == null || !request.Data.Any())
                {
                    Console.WriteLine("No fallback data to export");
                    return Json(new { success = false, message = "No fallback data available for export." });
                }

                // Log data structure
                Console.WriteLine($"=== FALLBACK DATA STRUCTURE ANALYSIS ===");
                if (request.Data.Any())
                {
                    var firstItem = request.Data.First();
                    Console.WriteLine($"First item - Branch: {firstItem.BranchName}, Amount: {firstItem.Amount}, Status: {firstItem.Status}");
                }

                // Convert to export data
                Console.WriteLine($"=== CONVERTING FALLBACK DATA ===");
                var fallbackData = FallbackExcelExportGenerator.ConvertToFallbackData(request.Data);
                Console.WriteLine($"Successfully converted {fallbackData.Count} fallback records");

                if (!fallbackData.Any())
                {
                    Console.WriteLine("No fallback data converted successfully");
                    return Json(new { success = false, message = "No valid fallback data could be processed for export." });
                }

                // Prepare file name and paths
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileName = $"{request.ExportOptions?.FileName ?? "Fallback_Report"}_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                Console.WriteLine($"=== FALLBACK FILE PREPARATION ===");
                Console.WriteLine($"Directory: {directoryPath}");
                Console.WriteLine($"File Name: {fileName}");

                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine("Creating temp directory");
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                Console.WriteLine($"Full Path: {filePath}");
                Console.WriteLine($"Exported By: {exportedBy}");

                // Generate Excel file
                Console.WriteLine($"=== GENERATING FALLBACK EXCEL ===");
                var fallbackExcelGenerator = new FallbackExcelExportGenerator();
                fallbackExcelGenerator.GenerateFallbackExcelFromTableData(fallbackData, filePath, exportedBy, request.ExportOptions);

                // Verify file was created
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("ERROR: Fallback Excel file was not created");
                    return Json(new { success = false, message = "Failed to generate Excel file." });
                }

                Console.WriteLine($"Fallback Excel file generated successfully: {filePath}");
                Console.WriteLine($"File size: {new FileInfo(filePath).Length} bytes");

                // Read and send file to browser
                Console.WriteLine($"=== READING FALLBACK FILE ===");
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                Console.WriteLine($"File bytes read: {fileBytes.Length} bytes");

                // Delete temp file after sending
                Console.WriteLine($"=== CLEANUP ===");
                System.IO.File.Delete(filePath);
                Console.WriteLine("Temp file deleted");

                Console.WriteLine($"=== FALLBACK EXPORT COMPLETE ===");
                Console.WriteLine($"Returning file: {fileName} ({fileBytes.Length} bytes)");

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== FALLBACK EXPORT ERROR ===");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Inner exception details
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
                }

                return Json(new
                {
                    success = false,
                    message = $"An error occurred while exporting fallback data to Excel: {ex.Message}"
                });
            }
        }
    }
}