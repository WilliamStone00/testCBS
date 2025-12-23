using CBS.BusinessService.Accounting_V2.MemberReconciliation;
using CBS.BusinessService.AccountingV2.CashReconciliation;
using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using DocumentFormat.OpenXml.EMMA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;


namespace CBS.FrontDesk.UI.Controllers.AccountingV2.GLSystemReconciliation
{
    public class GLSystemReconciliationController : Controller
    {

        private static readonly Random _random = new Random();

        private readonly BranchServices _branchServices;
        private readonly GLSystemReconciliationService _glSystemReconciliationService;
        private readonly JournalHeadService _journalHeadService;

        public GLSystemReconciliationController(BranchServices branchServices, GLSystemReconciliationService glSystemReconciliationService, JournalHeadService journalHeadService)
        {

            _branchServices = branchServices;
            _glSystemReconciliationService = glSystemReconciliationService;
            _journalHeadService = journalHeadService;


        }
        // GET: GLSystemReconciliation
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }


        public async Task<bool> loader()
        {


            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            ViewBag.OperationCode = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Cashin", Text = "CASH IN" },
                    new SelectListItem { Value = "Cashout", Text = "CASH OUT" }
                };

            ViewBag.Status = new List<SelectListItem>
                {
                    new SelectListItem { Value = "EXCEPTION", Text = "EXCEPTION" },
                    new SelectListItem { Value = "MISSING_IN_JOURNAL", Text = "MISSING IN JOURNAL" },
                    new SelectListItem { Value = "AMOUNT_MISMATCH", Text = "AMOUNT MISMATCH" }
                };


            return true;
        }


        public async Task<JsonResult> LoadReconciliationData(ReconciliationQuery query)
        {
            try
            {
                var data = await _glSystemReconciliationService.GetReconciliationDataTableAsync(query);

                var reconciliations = JsonConvert.DeserializeObject<List<Reconciliation>>(
                    JsonConvert.SerializeObject(data.data));



                return Json(new
                {
                    draw = data.Options.draw ?? "1",
                    recordsTotal = data.Options.recordsTotal,
                    recordsFiltered = data.Options.recordsFiltered,
                    data = reconciliations,
                    success = true,
                    message = "Reconciliation DataTable loaded successfully"
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
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetReconciliationSummary(ReconciliationQuerys model)
         {

            model.EndUtc = model.StartDate;
            model.StartUtc = model.StartDate;
            model.EndDate = model.StartDate;
            
           
            try
            {
                var summary = await _glSystemReconciliationService.GetReconciliationSummaryAsync(model);

                if (summary == null)
                    return Json(new { success = false, message = "Empty summary response." });

                // Return summary as JSON
                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public string GenerateReference()
        {
            // Use _random instance to prevent duplicate numbers
            return $"REF-{DateTime.Now:yyyyMMddHHmmss}-{_random.Next(100, 999)}";
        }




        [HttpPost]
        public ActionResult LoadRecordsForm(ReconciliationData summary)
        {

            
            return PartialView("_RecordsForm", summary);
        }
        [HttpGet]
        public async Task<ActionResult> LoadCloseOfDayForm()
        {
            await loader();


            var model = new CloseOfDayModel
            {
                Reference = GenerateReference()
            };


            return PartialView("_CloseOfDayForm", model);
        }



        [HttpPost]
        public async Task<ActionResult> SubmitCloseOfDay(CloseOfDayModel model)
        {

            try
            {
                var result = await _glSystemReconciliationService.SaveCloseOfDay(model);

                if (result == null)
                    return Json(new { success = false, message = "No response from Close Of Day service." });

                

                // Return summary as JSON
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $" Close of Day failed: {ex.Message}"
                });
            }
        }



        [HttpGet]
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "System Reconciliation ID is required");

            Data.Entity.AccountingV2.GLSystemReconciliation.ReconciliationDetails entry = null;

            try
            {
                Session["ReconciliationId"] = id;
                entry = await _glSystemReconciliationService.GetReconciliationByIdAsync(id);
            }
            catch (Exception ex)
            {
                // You can log the exception here
                return new HttpStatusCodeResult(404, ex.Message);
            }

            return PartialView("_ReconciliationDetails", entry);
        }



        

        [HttpPost]
        public async Task<ActionResult> PushRecord(PushRequest model)
        {
            try
            {
                // 1️⃣ Get all branches
                var branches = await _branchServices.GetBranches();

                // 2️⃣ Find the branch matching the incoming BranchId
                var selectedBranch = branches
                    .FirstOrDefault(b => b.Id == model.BranchId);
                model.BranchName = selectedBranch.Name;
                model.BranchCode = selectedBranch.BranchCode;

                if (selectedBranch == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid BranchId. Branch not found."
                    });
                }

                // 3️⃣ Process push request
                var summary = await _glSystemReconciliationService.PushRecordAsync(model);

                if (summary == null)
                    return Json(new { success = false, message = "Empty summary response." });

                // 4️⃣ Attach branch info to the response
                return Json(new
                {
                    success = true,
                    data = summary,
                    branch = new
                    {
                        BranchId = selectedBranch.Id,
                        BranchName = selectedBranch.Name,
                        BranchCode = selectedBranch.BranchCode
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        public async Task<JsonResult> LoadStatisticsData(OperationDetailsFilter query)
        {
            try
            {
                var data = await _glSystemReconciliationService.GetStatisticDataTableAsync(query);

                var reconciliations = JsonConvert.DeserializeObject<List<StatisticsDetails>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    //draw = data.Options.draw ?? "1",
                    //recordsTotal = data.Options.recordsTotal,
                    //recordsFiltered = data.Options.recordsFiltered,
                    data = reconciliations,
                    success = true
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
                });
            }
        }


        public async Task<ActionResult> LoadOperationDetailsData(OperationDetailsFilter query)
        {
            return View("_StatisticsDataTable",query);
        }


        [HttpGet]
        public async Task<ActionResult> GetStatisticsDetails(string id)
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

            return PartialView("_StatisticsDetails", entry);
        }

        [HttpPost]
        public async Task<ActionResult> UpdatePayload(TillCloseModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Reference))
                return Json(new { success = false, message = "Reference is required" });
            var idFromSession = Session["ReconciliationId"] as string;


            try
            {
                model.Id = idFromSession;
                var result = await _glSystemReconciliationService.UpdateTillClosePayloadAsync(model);

                if (result != null && result.IsSuccess)
                {
                    return Json(new
                    {
                        success = true,
                        statusCode = 200,
                        message = result.Message ?? "Payload updated successfully",
                        data = result
                    });
                }

                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = result?.Message ?? "Failed to update payload",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Update failed: {ex.Message}"
                });
            }
        }


    }
}


    