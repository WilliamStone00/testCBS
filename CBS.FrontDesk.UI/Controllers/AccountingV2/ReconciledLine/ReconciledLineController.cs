using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.AccountingV2.ReconciledLine;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReconciledLine;
using CBS.FrontDesk.Data.Entity.DataTable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.ReconciledLine
{
    public class ReconciledLineController : Controller
    {
        private readonly ReconciledLineService _reconciledLineService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        private readonly GenerateReconciledLinesExcelExport _generateReconciledLinesExcelExport;
        public ReconciledLineController(ReconciledLineService reconciledLineService, BranchServices branchServices, BranchAccountService branchAccountService, GenerateReconciledLinesExcelExport generateReconciledLinesExcelExport)
        {
            _reconciledLineService = reconciledLineService;
            _branchServices = branchServices;
            _branchAccountService = branchAccountService;
            _generateReconciledLinesExcelExport = generateReconciledLinesExcelExport;
        }
        // GET: ReconciledLine
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }


        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            var CounterBranches = await _branchServices.GetCounterpartyBranches();
            ViewBag.CounterBranches = CounterBranches;

            return true;
        }


        [HttpPost]
        public async Task<JsonResult> LoadDataTable(ReconciledQuery query)
        {
            try
            {
                var data = await _reconciledLineService.GetReconciledLineDataTableAsync(query);

                // Deserialize DataTable payload into strongly-typed list
                var ReconciledLine = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.ReconciledLine.Reconciled>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {

                    draw = data.Options.draw ?? "1",
                    recordsTotal = data.Options.recordsTotal,
                    recordsFiltered = data.Options.recordsFiltered,
                    data = ReconciledLine,
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return DataTables-compatible empty result on error
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
        public async Task<ActionResult> GetBranchAccounts(string branchId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(branchId))
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                var branchAccounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);
                var result = _branchAccountService.DropDownGen(branchAccounts.ToList());


                // var result = await _manualJournalEntryService.GetAccountsByBranchAsync(branchId);

                if (result == null || !result.Any())
                    return Json(new { success = false  }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (TaskCanceledException)
            {
                return Json(new { success = false  }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<ActionResult> Details(string referenceNumber)
        {
            if (string.IsNullOrEmpty(referenceNumber))
            {
                return RedirectToAction("Index", new { error = "Invalid journal header ID" });
            }

            try
            {
                var reconciledLines = await _reconciledLineService.GetReconciledLinesByJournalHeaderId(referenceNumber);

                if (reconciledLines == null || reconciledLines.Count == 0)
                {
                    return HttpNotFound();
                }



                return PartialView("_Details", reconciledLines);
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return View("Error");
            }
        }






        public async Task<ActionResult> DownloadReconciledLinesExcel(string referenceNumber)
        {
            if (string.IsNullOrEmpty(referenceNumber))
                return RedirectToAction("Listing", new { error = "Invalid reference number" });

            try
            {
                // ✅ Fetch reconciled lines by reference number
                var reconciledLines = await _reconciledLineService.GetReconciledLinesByJournalHeaderId(referenceNumber);
                if (reconciledLines == null || !reconciledLines.Any())
                    return Json(new { success = false, message = "No reconciled lines found." }, JsonRequestBehavior.AllowGet);

                // ✅ Prepare file name and paths
                string fileName = $"ReconciledLines_{referenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // ✅ Generate Excel file
                _generateReconciledLinesExcelExport.GenerateReconciledLinesExcel(reconciledLines, filePath, exportedBy, referenceNumber);

                // ✅ Read and send file to browser
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Delete temp file after sending
                System.IO.File.Delete(filePath);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Log error or handle gracefully
                Console.WriteLine($"Excel Export Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while exporting to Excel." }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost]
        public async Task<ActionResult> ExportReconciledData(ExportReconciledEntry request)
        {
            try
            {
                // Build query from filters
                var query = new ReconciledQuery
                {
                    Options = new DataTableOptions
                    {
                        draw = "1",
                        start = 0,
                        length = int.MaxValue // fetch all filtered rows
                    },
                    BranchId = request.Filters?.BranchId,
                    StartDate = request.Filters?.StartDate,
                    EndDate = request.Filters?.EndDate,
                    BranchAccountId = request.Filters?.BranchAccountId,
                    ReferenceNumber = request.Filters?.ReferenceNumber,
                    AccountNumber = request.Filters?.AccountNumber,
                    AccountName = request.Filters?.AccountName,
                    UserName = request.Filters?.UserName,
                    AuxiliaryRef = request.Filters?.AuxiliaryRef,
                    CounterpartyBranchId = request.Filters?.CounterpartyBranchId,
                    IsInterbranch = request.Filters?.IsInterbranch?? false
                };

                // Fetch data from service
                var data = await _reconciledLineService.GetReconciledLineDataTableAsync(query);

                // Deserialize the data
                var reconciledList = JsonConvert.DeserializeObject<List<Reconciled>>(
                    JsonConvert.SerializeObject(data.data));

                if (!reconciledList.Any())
                    return Json(new { success = false, message = "No reconciled data available for export." });

                // Enhance data with branch names if needed
                foreach (var entry in reconciledList.Where(e => string.IsNullOrEmpty(e.BranchName)))
                {
                    var branch = await _branchServices.GetBranch(entry.BranchId);
                    entry.BranchName = branch?.Name ?? "—";
                }

                // Prepare file name and path
                string timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                string fileName = $"{request.ExportOptions?.FileName ?? "Reconciled_Report"}_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // Generate Excel
                var exportGenerator = new ReconciledExcelExportGenerator();
                exportGenerator.GenerateReconciledExcel(reconciledList, filePath, exportedBy, request.ExportOptions);

                if (!System.IO.File.Exists(filePath))
                    return Json(new { success = false, message = "Failed to generate Excel file." });

                // Send file to browser
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                System.IO.File.Delete(filePath); // cleanup

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred while exporting reconciled data: {ex.Message}"
                });
            }
        }
    }


}
