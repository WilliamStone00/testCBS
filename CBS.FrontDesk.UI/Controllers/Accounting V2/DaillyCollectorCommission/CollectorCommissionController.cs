using CBS.BusinessService.Accounting_V2;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.IPS;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.DaillyCollectorCommission
{
    //[CheckSessionTimeOut]
    public class CollectorCommissionController : Controller
    {
        private readonly ManualDailyCollectionService _manualService;
        private readonly CollectorCommissionService _collectorCommissionService;
        private readonly BranchServices _branchServices;
        private readonly ChartOfAccountsV2Service _chartOfAccountsV;
        private readonly BranchAccountService _branchAccountService;
        private readonly CommissionExcelExportGenerator _commissionExcelExportGenerator;
        private readonly IPSClaimService _ipsClaimService;

        /// <summary>
        /// Injects the required CollectorCommissionController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public CollectorCommissionController(IPSClaimService iPSClaimService, CommissionExcelExportGenerator commissionExcelExportGenerator, ChartOfAccountsV2Service chartOfAccountsV2Service1, ManualDailyCollectionService manualDailyCollectionService, ChartOfAccountsV2Service chartOfAccountsV2Service, CollectorCommissionService collectorCommissionService, BranchServices branchServices, BranchAccountService branchAccountService)
        {
            _collectorCommissionService = collectorCommissionService;
            _chartOfAccountsV = chartOfAccountsV2Service;
            _branchServices = branchServices;
            _manualService = manualDailyCollectionService;
            _branchAccountService = branchAccountService;
            _commissionExcelExportGenerator = commissionExcelExportGenerator;
            _ipsClaimService = iPSClaimService;
        }

        public async Task<ActionResult> Index()
       {
            await loader();
            return View(new CollectorComissionResponse());
        }


        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            return true;

        }

        // Note: use [FromBody] so model binder reads the JSON DataTables sends.
        [HttpPost]
        public async Task<JsonResult> DataTable(commisionQuery query)
        {
            try
            {
                var data = await _collectorCommissionService.CommisionDataTableAsync(query);

                var Response = JsonConvert.DeserializeObject<List<DataTableResponse>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Response
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.DataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(CollectorComissionResponse model)
        //{
        //    if (!ModelState.IsValid)
        //        return Json(new { success = false, message = "Validation failed." });

        //    var result = await _collectorCommissionService.CreateAsync(model);
        //    return Json(new { success = false, message = "No data returned from service." });
        //}



        [HttpGet]
        public async Task<JsonResult> GetCollectorsByBranch(string branchId)
        {
            try
            {
                int order = 2;
                // This would call your service to get collectors by branch
                var collectors = await _manualService.GetCollectorsAsSelectListAsync(branchId, order);

                return Json(collectors, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public async Task<JsonResult> GetCommissionData(CollectorComissionResponse model)
        //{
        //    try
        //    {
        //        var commissionData = await _collectorCommissionService.GetCommissionAsync(model);

        //        if (commissionData != null)
        //        {
        //            return Json(new { success = true, data = commissionData });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "No commission data found for the selected criteria." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<JsonResult> GetCommissionData(CollectorComissionResponse model)
        {
            try
            {
                // ✅ Always store JobId in Session


                var commissionData = await _collectorCommissionService.GetCommissionAsync(model);
                if (!string.IsNullOrWhiteSpace(commissionData.JobId))
                {
                    Session["CollectorCommissionJobId"] = commissionData.JobId;
                }
                else
                {
                    Session["CollectorCommissionJobId"] = null;
                }

                if (commissionData != null)
                {
                    return Json(new { success = true, data = commissionData });
                }
                else
                {
                    return Json(new { success = false, message = "No commission data found for the selected criteria." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public ActionResult ExportCommission()
        {
            return PartialView("_ExportCommission");
        }

        [HttpGet]
        public async Task<ActionResult> CommissionTreatment(string BranchId)
        {
            return PartialView("_CommissionTreatment", new CollectorComissionResponse());
        }

        [HttpGet]
        public ActionResult GetbyId(string id)
        {
            // Get the commission record by id
            var commission = _collectorCommissionService.GetByIdAsync(id);
            if (commission == null)
            {
                return Content("<div class='alert alert-danger'>Commission record not found.</div>");
            }

            return PartialView("_Details", commission);
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomerAccounts(string customerId)
        {
            try
            {
                var customerData = await _collectorCommissionService.GetCustomerAccountDropdownAsync(customerId);
                // var customerData = await _ipsClaimService.GetinfoAsync(customerId);

                return Json(new
                {
                    success = true,
                    customer = customerData.CustomerDto,
                    accounts = customerData.AccountSelectList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ExportCommissionToExcel(ExportCommissionRequest request)
        {
            try
            {
                if (request?.CommissionData == null)
                {
                    return Json(new { success = false, message = "No commission data available for export." });
                }

                // DEBUG: Check what data we're receiving in the controller
                Console.WriteLine($"=== CONTROLLER DEBUG ===");
                Console.WriteLine($"CommissionData received: {request.CommissionData != null}");
                Console.WriteLine($"SharedAmounts count: {request.CommissionData.SharedAmounts?.Count ?? 0}");
                Console.WriteLine($"MemberStats count: {request.CommissionData.MemberStats?.Count ?? 0}");

                if (request.CommissionData.SharedAmounts != null)
                {
                    foreach (var item in request.CommissionData.SharedAmounts)
                    {
                        Console.WriteLine($"  - Stakeholder: {item.Stakeholder}, Percentage: {item.Percentage}, Amount: {item.Amount}");
                    }
                }

                // Prepare file name and paths
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileName = $"Commission_Report_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // Generate Excel file using service - pass export options
                CBS.BusinessService.Accounting_V2.Affiliate.CommissionExcelExportGenerator.GenerateCommissionExcel(
                    request.CommissionData, filePath, exportedBy, request.ExportOptions);

                // Read and send file to browser
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Delete temp file after sending
                System.IO.File.Delete(filePath);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Excel Export Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while exporting to Excel: " + ex.Message });
            }
        }
        //[HttpGet]
        //public async Task<JsonResult> GetPcmfAccountsByBranch(string branchId)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(branchId))
        //        {
        //            return Json(new { success = false, message = "Branch ID is required" }, JsonRequestBehavior.AllowGet);
        //        }
        //         var pcmfAccounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);

        //        return Json(new { success = true, accounts = pcmfAccounts }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpGet]
        public async Task<JsonResult> GetPcmfAccountsByBranch(string branchId)
       {
            try
            {
                if (string.IsNullOrEmpty(branchId))
                {
                    return Json(new { success = false, message = "Branch ID is required" }, JsonRequestBehavior.AllowGet);
                }

                // Get PCMF accounts for the branch
                var pcmfAccounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);

                // Convert to SelectList format with proper Text and Value
                var selectList = pcmfAccounts.Select(a => new
                {
                    Value = a.Id?.ToString() ?? "",
                    Text = $"{a.Name}" // Format the display text
                }).ToList();

                return Json(new { success = true, accounts = selectList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET endpoint for direct download
        [HttpGet]
        public async Task<ActionResult> DownloadCommissionExcel()
        {
            try
            {
                // Get current commission data from session or global variable
                // You might need to adjust this based on how you store the data
                var commissionData = Session["CurrentCommissionData"] as CollectorComissionResponse;

                if (commissionData == null)
                {
                    return Json(new { success = false, message = "No commission data available. Please load commission data first." },
                               JsonRequestBehavior.AllowGet);
                }

                // Prepare file name and paths
                string fileName = $"Commission_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // Generate Excel file using service
                CommissionExcelExportGenerator.GenerateCommissionExcel(
                    commissionData, filePath, exportedBy);

                // Read and send file to browser
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Delete temp file after sending
                System.IO.File.Delete(filePath);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Excel Export Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while exporting to Excel." },
                           JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ProcessPayment(CollectorComissionResponse model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Invalid request." }, JsonRequestBehavior.AllowGet);

                // Server-side validation
                if (string.IsNullOrWhiteSpace(model.BranchId))
                    ModelState.AddModelError(nameof(model.BranchId), "Branch is required.");

                if (string.IsNullOrWhiteSpace(model.AccountNumber))
                    ModelState.AddModelError(nameof(model.AccountNumber), "Account number is required.");

                if (string.IsNullOrWhiteSpace(model.BranchCommisionGLId))
                    ModelState.AddModelError(nameof(model.BranchCommisionGLId), "Branch commission GL is required.");

                // Validate Month and Year
                if (string.IsNullOrWhiteSpace(model.Month))
                    ModelState.AddModelError(nameof(model.Month), "Month is required.");

                if (model.Incentives > 0 && model.ExpenseGL == null)
                    ModelState.AddModelError(nameof(model.Month), "Expense Gl is required when Incentive Greater than zero (0).");

                if (model.Year == 0)
                    ModelState.AddModelError(nameof(model.Year), "Year is required.");

                // Handle date parsing for both "MM/dd/yyyy hh:mm:ss AM" and "yyyy-MM-dd" formats
                DateTime accountingDate;
                bool dateParsed = false;

                // Try parsing as "MM/dd/yyyy hh:mm:ss AM" format first (from frontend)
                if (DateTime.TryParseExact(model.AccountingDate, "MM/dd/yyyy hh:mm:ss tt",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out accountingDate))
                {
                    dateParsed = true;
                }
                // Try parsing as "yyyy-MM-dd" format (fallback)
                else if (DateTime.TryParseExact(model.AccountingDate, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out accountingDate))
                {
                    dateParsed = true;
                }
                // Try generic parsing
                else if (DateTime.TryParse(model.AccountingDate, out accountingDate))
                {
                    dateParsed = true;
                }

                if (!dateParsed)
                {
                    ModelState.AddModelError(nameof(model.AccountingDate),
                        "Accounting date is required and must be valid. Expected format: MM/dd/yyyy hh:mm:ss AM");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return Json(new { success = false, errors = errors }, JsonRequestBehavior.AllowGet);
                }

                if (model.SharedAmounts != null)
                {
                    foreach (var share in model.SharedAmounts)
                    {
                        System.Diagnostics.Debug.WriteLine($"Share: {share.Stakeholder} - {share.Percentage}% - {share.Amount}");
                    }
                }

                // ======= Map model to Payment =======
                var payment = new Payment
                {
                    DailyCollectorId = model.CollectorId,
                    BranchId = model.BranchId,
                    AccountNumber = model.DailyCollectorAccount, // Use AccountNumber from payload
                    BranchCommisionGLId = model.BranchCommisionGLId,
                    IncentiveAmount = model.Incentives,
                    CollectorTotalCommision = model.Total, // Use from payload instead of calculating
                    AccountingDate = accountingDate,
                    TotalAmountToShare = model.CollectorTotalCommision,
                    Month = int.Parse(model.Month), // Month is now properly extracted as string "11"
                    Year = model.Year,
                    ExpenseGL = model.ExpenseGL,
                    Memo = model.Memo,
                    MemoIncentive = model.MemoIncentive,
                    SharedAmounts = (model.SharedAmounts ?? new List<StakeholderShare>())
                        .Select(s => new SharedAmount
                        {
                            StakeHolderId = model.CollectorId,
                            Stakeholder = s.Stakeholder,
                            Percentage = s.Percentage,
                            Amount = s.Amount
                        }).ToList(), 
                };

                // ======= Persist via service/repository =======
                var result = await _collectorCommissionService.CreateAsync(payment);

                if (result != null && (result.Result))
                {
                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var message = Messaging.MessageResult(result) ?? "Failed to create payment.";
                    return Json(new { success = false, message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in ProcessPayment: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Log inner exception if exists
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return Json(new { success = false, message = "An error occurred while saving payment.", details = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost]
        public async Task<ActionResult> ExportTableData(ExportTableRequest request)
        {
            try
            {
                Console.WriteLine($"=== EXPORT DEBUG START ===");
                Console.WriteLine($"Request received: {request != null}");
                Console.WriteLine($"Data count: {request?.Data?.Count ?? 0}");
                Console.WriteLine($"Total Records: {request?.TotalRecords ?? 0}");
                Console.WriteLine($"Export Options: {request?.ExportOptions?.FileName ?? "N/A"}");

                if (request?.Data == null || !request.Data.Any())
                {
                    Console.WriteLine("No data to export");
                    return Json(new { success = false, message = "No commission data available for export." });
                }

                // Debug: Log data structure
                Console.WriteLine($"=== DATA STRUCTURE ANALYSIS ===");
                if (request.Data.Any())
                {
                    var firstItem = request.Data.First();
                    Console.WriteLine($"First item type: {firstItem?.GetType()?.Name ?? "NULL"}");

                    if (firstItem is IDictionary<string, object> dict)
                    {
                        Console.WriteLine($"Properties ({dict.Count}):");
                        foreach (var kvp in dict.Take(5))
                        {
                            Console.WriteLine($"  {kvp.Key}: {kvp.Value} (Type: {kvp.Value?.GetType()?.Name ?? "NULL"})");
                        }
                    }
                }

                // Convert dynamic data to strongly typed list
                Console.WriteLine($"=== CONVERTING DATA ===");
                var commissionData = CommissionExcelExportGenerator.ConvertToCommissionData(request.Data);
                Console.WriteLine($"Successfully converted {commissionData.Count} records");

                if (!commissionData.Any())
                {
                    Console.WriteLine("No data converted successfully");
                    return Json(new { success = false, message = "No valid commission data could be processed for export." });
                }

                // Analyze data for logging
                var branches = commissionData.Select(x => new { x.BranchCode, x.BranchName }).Distinct().ToList();
                var collectors = commissionData.Select(x => x.CollectorName).Distinct().ToList();
                Console.WriteLine($"Data analysis: {branches.Count} branches, {collectors.Count} collectors");

                // Prepare file name and paths
                string timestamp = DateTime.Now.ToString("dd/MM/yyyy");
                string fileName = $"{request.ExportOptions?.FileName ?? "Commission_Report"}_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                Console.WriteLine($"=== FILE PREPARATION ===");
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

                // Generate Excel file with multiple sheets
                Console.WriteLine($"=== GENERATING EXCEL WITH MULTIPLE SHEETS ===");
                _commissionExcelExportGenerator.GenerateCommissionExcelFromTableData(commissionData, filePath, exportedBy, request.ExportOptions);

                // Verify file was created
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("ERROR: Excel file was not created");
                    return Json(new { success = false, message = "Failed to generate Excel file." });
                }

                Console.WriteLine($"Excel file generated successfully: {filePath}");
                Console.WriteLine($"File size: {new FileInfo(filePath).Length} bytes");

                // Read and send file to browser
                Console.WriteLine($"=== READING FILE ===");
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                Console.WriteLine($"File bytes read: {fileBytes.Length} bytes");

                // Delete temp file after sending
                Console.WriteLine($"=== CLEANUP ===");
                System.IO.File.Delete(filePath);
                Console.WriteLine("Temp file deleted");

                Console.WriteLine($"=== EXPORT COMPLETE ===");
                Console.WriteLine($"Returning file: {fileName} ({fileBytes.Length} bytes)");

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EXPORT ERROR ===");
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
                    message = $"An error occurred while exporting to Excel: {ex.Message}"
                });
            }
        }



        private async Task<JobMonitorViewModel> GetJobMonitorViewModel()
        {
            var viewModel = new JobMonitorViewModel
            {
                LastUpdated = DateTime.Now,
                AutoRefreshEnabled = false,
                CurrentJobId = Session["CollectorCommissionJobId"]?.ToString(),
                HasActiveJob = false
            };

            if (!string.IsNullOrWhiteSpace(viewModel.CurrentJobId))
            {
                try
                {
                    var result = await _collectorCommissionService
                        .GetJobSnapshotAsync(viewModel.CurrentJobId);

                    if (result != null && result.Data != null)
                    {
                        var snapshot = (JobSnapshotDto)result.Data;

                        viewModel.HasActiveJob = true;
                        viewModel.JobData = snapshot;

                        // Enable auto-refresh for Pending or Running jobs
                        if (snapshot.Status == 0 || snapshot.Status == 1)
                        {
                            viewModel.AutoRefreshEnabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log only – UI should not break
                    System.Diagnostics.Debug.WriteLine(
                        $"[JobMonitor] Error loading job snapshot: {ex.Message}");
                }
            }

            return viewModel;
        }


        [HttpGet]
        public async Task<ActionResult> RefreshJobMonitor()
        {
            var viewModel = await GetJobMonitorViewModel();
            return PartialView("_JobMonitor", viewModel);
        }

        /// <summary>
        /// Gets job snapshot by ID (from parameter or Session)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetJobSnapshot()
        {
            try
            {
                string jobId = Session["CollectorCommissionJobId"]?.ToString();

                if (string.IsNullOrWhiteSpace(jobId))
                {
                    return Json(new { success = false, message = "Job ID is required" }, JsonRequestBehavior.AllowGet);
                }

                var snapshot = await _collectorCommissionService.GetJobSnapshotAsync(jobId);

                if (snapshot != null)
                {
                    return Json(new { success = true, data = snapshot }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Job not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Gets the current active job from Session
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetCurrentJob()
        {
            try
            {
                string jobId = Session["CollectorCommissionJobId"]?.ToString();

                if (string.IsNullOrWhiteSpace(jobId))
                {
                    return Json(new { success = false, message = "No active job in session" }, JsonRequestBehavior.AllowGet);
                }

                var snapshot = await _collectorCommissionService.GetJobSnapshotAsync(jobId);

                if (snapshot != null)
                {
                    return Json(new { success = true, data = snapshot }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Job not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       
    }
}
