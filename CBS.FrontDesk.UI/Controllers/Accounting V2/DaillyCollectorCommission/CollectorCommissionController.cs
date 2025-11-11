using CBS.BusinessService.Accounting_V2;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.Config;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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

        /// <summary>
        /// Injects the required CollectorCommissionController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public CollectorCommissionController(ChartOfAccountsV2Service chartOfAccountsV2Service1, ManualDailyCollectionService manualDailyCollectionService, ChartOfAccountsV2Service chartOfAccountsV2Service, CollectorCommissionService collectorCommissionService, BranchServices branchServices, BranchAccountService branchAccountService)
        {
            _collectorCommissionService = collectorCommissionService;
            _chartOfAccountsV = chartOfAccountsV2Service;
            _branchServices = branchServices;
            _manualService = manualDailyCollectionService;
            _branchAccountService = branchAccountService;
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

                var Affiliate = JsonConvert.DeserializeObject<List<DataTableResponse>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = Affiliate
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CollectorComissionResponse model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _collectorCommissionService.CreateAsync(model);
            return Json(new { success = false, message = "No data returned from service." });
        }



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

        [HttpPost]
        public async Task<JsonResult> GetCommissionData(CollectorComissionResponse model)
        {
            try
            {
                var commissionData = await _collectorCommissionService.GetCommissionAsync(model);

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

        [HttpPost]
        public async Task<JsonResult> ProcessPayment(CollectorComissionResponse model)
        {
            try
            {
                var result = await _collectorCommissionService.CreateAsync(model);
                return Json(new { success = false, message = "No data returned from service." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomerAccounts(string customerId)
        {
            try
            {
                var customerData = await _collectorCommissionService.GetCustomerAccountDropdownAsync(customerId);
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
                string fileName = $"Commission_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // Generate Excel file using service - FIXED NAMESPACE
                CBS.BusinessService.Accounting_V2.Affiliate.CommissionExcelExportGenerator.GenerateCommissionExcel(
                    request.CommissionData, filePath, exportedBy);

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
    }
}


