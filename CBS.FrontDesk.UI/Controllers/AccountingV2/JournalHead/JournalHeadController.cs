using CBS.API.Helper;
using CBS.BusinessService.AccountingV2;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
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
        private readonly JournalDataTableExcelExportGenerator _JournalDataTableExcelExportGenerator;


        public JournalHeadController(BranchServices branchServices, JournalHeadService journalHeadService, JournalHeadExcelExportGenerator journalHeadExcelExportGenerator, JournalDataTableExcelExportGenerator journalDataTableExcelExportGenerator)
        {

            _branchServices = branchServices;
            _journalHeadService = journalHeadService;
            _JournalHeadExcelExportGenerator = journalHeadExcelExportGenerator;
            _JournalDataTableExcelExportGenerator = journalDataTableExcelExportGenerator;

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
                    ViewBag.JournalStatus = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "RECEIVED", Text = "RECEIVED" },
                        new SelectListItem { Value = "TEMP_CREATED", Text = "TEMP_CREATED" },
                        new SelectListItem { Value = "TEMP_UPDATED", Text = "TEMP_UPDATED" },
                        new SelectListItem { Value = "WORKFLOW_PENDING", Text = "WORKFLOW_PENDING" },
                        new SelectListItem { Value = "RECONCILED", Text = "RECONCILED" },
                        new SelectListItem { Value = "FAILED", Text = "FAILED" },
                        new SelectListItem { Value = "VALIDATED", Text = "VALIDATED" },
                        new SelectListItem { Value = "REJECTED", Text = "REJECTED" }
                    };

                    ViewBag.TicketSource = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Source", Text = "Source " },
                        new SelectListItem { Value = "Destination", Text = "Destination" }
                    };

                    ViewBag.Source = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "temp", Text = "TEMPORAL JOURNAL (TODAY JOURNAL) " },
                        new SelectListItem { Value = "real", Text = "RECONCILED JOURNAL (n - 1) JOURNAL" }
                    };




            var filterRequest = new GetFirlterData
            {
                BranchId = "",          // as you said
                JournalStatus = null,
                Source = null
            };

            var response = await _journalHeadService.GetFilters(filterRequest);

            // SAFETY CHECK
            var data = response?.Data;

            // OPERATION CODES
            ViewBag.OperationCodes = data?.OperationCodes?
                .Select(o => new SelectListItem
                {
                    Value = o.Code,
                    Text = o.Code
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();

            // DAILY OPERATORS (Initiated By)
            ViewBag.DailyOperators = data?.InitiatedBy?
                .Select(u => new SelectListItem
                {
                    Value = u.Name,
                    Text = u.Name
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();

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
                ApiResponse<ResponseObject<JournalApprovalResponse>> result = null;

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

                if (result.IsSuccess && result.ApiResponseData !=null && result.ApiResponseData.Data!= null )
                {
                    // ✅ Return structured JSON based on result
                    return Json(new
                    {
                        success = true,
                        statusCode = 200,
                        message = result.ApiResponseData.Data.Message ?? "Approved successfully",
                        data = result.ApiResponseData.Data
                    });
                }

                // ✅ Return structured JSON based on result
                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message ?? "Approval Failed",
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


        [HttpPost]
        public async Task<ActionResult> Reject(JournalApproval model)
        {
            if (model == null || string.IsNullOrEmpty(model.Reference))
                return Json(new { success = false, message = "Journal id is required" });

            model.SourceBranchId = model.BranchId;
            try
            {
                    var result = await _journalHeadService.RejectAsync(model);
               
                if (result == null)
                    return Json(new { success = false, message = "No response from approval service." });

                if (result.IsSuccess && result.ApiResponseData != null && result.ApiResponseData.Data != null)
                {
                    // ✅ Return structured JSON based on result
                    return Json(new
                    {
                        success = true,
                        statusCode = 200,
                        message = result.ApiResponseData.Data.Message ?? "Rejected successfully",
                        data = result.ApiResponseData.Data
                    });
                }

                // ✅ Return structured JSON based on result
                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message ?? "Rejection Failed",
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


        [HttpPost]
        public async Task<ActionResult> LoadFilterDependencies(GetFirlterData model)
        {
            try
            {

                
                var response = await _journalHeadService.GetFilters(model);

                if (response == null)
                    return Json(new { success = false });

                return Json(new
                {
                    success = true,
                    operationCodes = response.Data.OperationCodes.Select(x => x.Code).ToList(),
                    initiatedBy = response.Data.InitiatedBy.Select(x => x.Name).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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



        [HttpPost]



        public async Task<ActionResult> ExportJournalData(ExportJournalRequest request)
        {
            try
            {



                // ✅ Build JournalEntryQuery from Filters
                var query = new JournalEntryQuery
                {
                    Options = new DataTableOptions
                    {
                        draw = "1",
                        start = 0,
                        length = int.MaxValue // fetch all filtered rows
                    },
                    BranchId = request.Filters?.BranchId,
                    OperationCode = request.Filters?.OperationCode,
                    Reference = request.Filters?.Reference,
                    StartAccountingDate = request.Filters?.StartAccountingDate,
                    EndAccountingDate = request.Filters?.EndAccountingDate,
                    StartDate = request.Filters?.StartDate,
                    EndDate = request.Filters?.EndDate,
                    TicketSource = request.Filters?.TicketSource,
                    JournalStatus = request.Filters?.JournalStatus,
                    DailyOperator = request.Filters?.DailyOperator,
                    IsInterbranch = request.Filters?.IsInterbranch ?? false,

                    Source = request.Filters?.Source
                };
                // Fetch data from service
                var data = await _journalHeadService.GetJournalHeaderDataTableAsync(query);

                var journalList = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.JournalHead>>(
                    JsonConvert.SerializeObject(data.data));

                if (!journalList.Any())
                    return Json(new { success = false, message = "No journal data available for export." });

                // Populate BranchName for each journal entry
                foreach (var entry in journalList)
                {
                    var branch = await _branchServices.GetBranch(entry.BranchId);
                    entry.BranchName = branch?.Name ?? "—";
                }

                // Convert data for Excel
                var journalDataForExcel = JournalDataTableExcelExportGenerator.ConvertToJournalData(journalList);

                if (!journalDataForExcel.Any())
                    return Json(new { success = false, message = "No valid journal data could be processed for export." });

                // Prepare file name and path
                string timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");

                string fileName = $"{request.ExportOptions?.FileName ?? "Journal_Report"}_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // Generate Excel
                var exportGenerator = new JournalDataTableExcelExportGenerator();
                exportGenerator.GenerateJournalExcelFromTableData(journalDataForExcel, filePath, exportedBy, request.ExportOptions);

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
                    message = $"An error occurred while exporting journal data to Excel: {ex.Message}"
                });
            }
        }



        public async Task<ActionResult> ExportWorkflowData(ExportWorkflowRequest request)
        {
            try
            {
                // Build query from filters
                var query = new JournalEntryQuery
                {
                    // Add your query parameters here
                    Options = new DataTableOptions
                    {
                        draw = "1",
                        start = 0,
                        length = int.MaxValue // get all filtered rows
                    }
                };

                // Fetch data from service
                var data = await _journalHeadService.GetJournalSourceDataTableAsync(query);

                var workflowList = JsonConvert.DeserializeObject<List<WorkflowTicket>>(
                    JsonConvert.SerializeObject(data.data));

                if (!workflowList.Any())
                    return Json(new { success = false, message = "No workflow data available for export." });

                // Populate BranchName for each workflow ticket if needed
                foreach (var ticket in workflowList.Where(x => string.IsNullOrEmpty(x.BranchName)))
                {
                    var branch = await _branchServices.GetBranch(ticket.BranchId);
                    ticket.BranchName = branch?.Name ?? "—";
                }

                // Convert data for Excel
                var workflowDataForExcel = WorkflowTicketExcelExportGenerator.ConvertToWorkflowData(workflowList);

                if (!workflowDataForExcel.Any())
                    return Json(new { success = false, message = "No valid workflow data could be processed for export." });

                // Prepare file name and path
                string timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");

                string fileName = $"{request.ExportOptions?.FileName ?? "Workflow_Report"}_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? "System";

                // Generate Excel
                var exportGenerator = new WorkflowTicketExcelExportGenerator();
                exportGenerator.GenerateWorkflowExcelFromTableData(workflowDataForExcel, filePath, exportedBy, request.ExportOptions);

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
                    message = $"An error occurred while exporting workflow data to Excel: {ex.Message}"
                });
            }
        }


    }
}