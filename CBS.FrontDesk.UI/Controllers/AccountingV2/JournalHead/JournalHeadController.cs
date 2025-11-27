using CBS.BusinessService.AccountingV2;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.JournalHead
{
    public class JournalHeadController : Controller
    {
        private readonly BranchServices _branchServices;
        private readonly JournalHeadService _journalHeadService;
        private readonly JournalHeadExcelExportGenerator _JournalHeadExcelExportGenerator;


        public JournalHeadController(BranchServices branchServices, JournalHeadService journalHeadService, JournalHeadExcelExportGenerator journalHeadExcelExportGenerator)
        {

            _branchServices = branchServices;
            _journalHeadService = journalHeadService;
            _JournalHeadExcelExportGenerator = journalHeadExcelExportGenerator;

        }
        // GET: JournalHead
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }
        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }
        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            ViewBag.OperationTypes = new List<SelectListItem>
{
    new SelectListItem { Value = "Cashin", Text = "CASH IN" },
    new SelectListItem { Value = "Cashout", Text = "CASH OUT" }
};

            // Reconciliation Status (WorkTicket) dropdown
            ViewBag.OperationCode = new List<SelectListItem>
{
    new SelectListItem { Value = "Temp", Text = "Temp " },
    new SelectListItem { Value = "Reconciled", Text = "Reconciliated" }
};
            ViewBag.TicketSource = new List<SelectListItem>
{
    new SelectListItem { Value = "Source", Text = "Source " },
    new SelectListItem { Value = "Destination", Text = "Destination" }
};
            return true;
        }
    

         [HttpPost]
        public async Task<JsonResult> LoadJournalHeaderData(JournalEntryQuery query)
        {
            try
            {
                var data = await _journalHeadService.GetJournalHeaderDataTableAsync(query);

                // Deserialize DataTable payload into strongly-typed list
                var journalHeaders = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.JournalHead>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {

                    draw = data.Options.draw ?? "1",
                    recordsTotal = data.Options.recordsTotal,
                    recordsFiltered = data.Options.recordsFiltered,
                    data = journalHeaders,
                    success = true,
                    message = "Display DataTable for Journal Head  successfully"
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
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "Journal Entry ID is required");

            Data.Entity.AccountingV2.JournalHead entry = null;

            try
            {
                // 1️⃣ Get Journal Entry
                entry = await _journalHeadService.GetJournalEntryByIdAsync(id);
                
                var counterpartyBranch = await _branchServices.GetBranch(entry.CounterpartyBranchId);
                entry.CounterpartyBranchName = counterpartyBranch?.Name ?? "—";

            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, ex.Message);
            }

            return PartialView("_JournalHeadDetails", entry);
        }

        //[HttpGet]
        //public async Task<ActionResult> GetDetails(string id)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(id))
        //            return Json(new { success = false, message = "⚠️ Journal Entry ID is required." }, JsonRequestBehavior.AllowGet);

        //        // ✅ Call the service which internally handles branchId
        //        var result = await _journalHeadService.GetJournalEntryByIdAsync(id);

        //        if (result == null)
        //            return Json(new { success = false, message = "⚠️ Journal Entry not found." }, JsonRequestBehavior.AllowGet);

        //        return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        return Json(new { success = false, message = "⚠️ Timeout while fetching journal entry — backend service not responding." }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        [HttpPost]
        public async Task<JsonResult> LoadJournalSourceData(JournalEntryQuery query)
        {
            try
            {
              

                var data = await _journalHeadService.GetJournalSourceDataTableAsync(query);
               

                // Deserialize DataTable payload into strongly-typed list
                var Workticket = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.JournalHead>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    
                    draw = data.Options.draw??"1",
                    recordsTotal = data.Options.recordsTotal,
                    recordsFiltered = data.Options.recordsFiltered,
                    data = Workticket,
                    success = true,
                    message = "Display DataTable for Journal Head  successfully"
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

        [HttpPost]
        public async Task<ActionResult> Approve(JournalApproval model)
        {
            if (model == null || string.IsNullOrEmpty(model.Reference))
                return Json(new { success = false, message = "Journal Reference is required" });

            model.SourceBranchId = model.BranchId;
            model.DestinationBranchId = model.BranchId;
            try
            {
                object result = null;

                // ✅ Route to the correct service based on ticket type
                switch (model.TicketType?.ToUpperInvariant())
                {
                    case "SOURCE":
                        result = await _journalHeadService.ApproveSourceAsync(model);
                        break;

                    case "DESTINATION":
                        result = await _journalHeadService.ApproveDestinationAsync(model);
                        break;

                    case "SOURCE_MEMBERS_BALANCE_RECONCILIATION":
                        result = await _journalHeadService.ApproveMemberReconciliationAsync(model);
                        break;

                    case "SOURCE_CASH_GLS_RECONCILATION":
                        result = await _journalHeadService.ApproveCashReconciliationAsync(model);
                        break;

                    default:
                        return Json(new { success = false, message = "Unknown Ticket Type." });
                }

                if (result == null)
                    return Json(new { success = false, message = "No response from approval service." });

                // ✅ Return structured JSON based on result
                return Json(new
                {
                    success = true,
                    statusCode = 200,
                    message = (result as dynamic)?.Message ?? "Approved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Approval failed: {ex.Message}"
                });
            }
        }








        
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "Journal Entry ID is required");

            Data.Entity.AccountingV2.WorkflowTicket entry = null;

            try
            {
                entry = await _journalHeadService.GetJournalSourceByIdAsync(id);

                var Branch = await _branchServices.GetBranch(entry.BranchId);
                entry.BranchName = Branch?.Name ?? "—";
            }
            catch (Exception ex)
            {
                // You can log the exception here
                return new HttpStatusCodeResult(404, ex.Message);
            }

            //return View(entry); // MVC 5 expects Details.cshtml
            await loader();
            return PartialView("_DestDetails", entry);
        }


        [HttpGet]

        public async Task<ActionResult> DownloadJournalHeadExcelSheet(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Listing", new { error = "Invalid journal ID" });

            try
            {
                // ✅ Fetch the journal entry by ID instead of by reference
                var model = await _journalHeadService.GetJournalEntryByIdAsync(id);
                var counterpartyBranch = await _branchServices.GetBranch(model.CounterpartyBranchId);
                model.CounterpartyBranchName = counterpartyBranch?.Name ?? "—";


                if (model == null)
                    return HttpNotFound();

                // ✅ Prepare file name and paths
                string fileName = $"JournalHead_{model.Reference}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // ✅ Generate Excel file
                _JournalHeadExcelExportGenerator.GenerateJournalHeadExcelSheet(model, filePath, exportedBy);

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


        [HttpGet]
       
        public async Task<ActionResult> DownloadJournalHeadExcel(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Listing", new { error = "Invalid journal ID" });

            try
            {
                // ✅ Fetch the journal entry by ID instead of by reference
                var model = await _journalHeadService.GetJournalSourceByIdAsync(id);
                if (model == null)
                    return HttpNotFound();

                // ✅ Prepare file name and paths
                string fileName = $"JournalHead_{model.Reference}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // ✅ Generate Excel file
                _JournalHeadExcelExportGenerator.GenerateJournalHeadExcel(model, filePath, exportedBy);

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

    }
}