using CBS.BusinessService.AccountingV2;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public JournalHeadController(BranchServices branchServices, JournalHeadService journalHeadService)
        {

            _branchServices = branchServices;
            _journalHeadService = journalHeadService;


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
            ViewBag.WorkTickets = new List<SelectListItem>
{
    new SelectListItem { Value = "Temp", Text = "Temp Operations" },
    new SelectListItem { Value = "Reconciled", Text = "Reconciliated Data" }
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
                var journalHeaders = JsonConvert.DeserializeObject<List<JournalEntry>>(
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

            JournalEntry entry = null;
            try
            {
                entry = await _journalHeadService.GetJournalEntryByIdAsync(id);
            }
            catch (Exception ex)
            {
                // log as needed
                return new HttpStatusCodeResult(404, ex.Message);
            }

            // return HTML fragment (partial) — this will be injected into main modal body
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
                var Workticket = JsonConvert.DeserializeObject<List<JournalEntry>>(
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
        public async Task<ActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "Journal Entry ID is required" });

            try
            {
                bool result = await _journalHeadService.ApproveAsync(id);
                return Json(new { success = result, message = result ? "Approved successfully" : "Approval failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =========================
        // REJECT ENTRY
        // =========================
        [HttpPost]
        public async Task<ActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "Journal Entry ID is required" });

            try
            {
                bool result = await _journalHeadService.RejectAsync(id);
                return Json(new { success = result, message = result ? "Rejected successfully" : "Rejection failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        //[HttpGet]
        //public async Task<ActionResult> Details(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //        return new HttpStatusCodeResult(400, "Journal Entry ID is required");

        //    JournalEntry entry = null;

        //    try
        //    {
        //        entry = await _journalHeadService.GetJournalEntryByIdAsync(id);
        //    }
        //    catch (Exception ex)
        //    {
        //        // You can log the exception here
        //        return new HttpStatusCodeResult(404, ex.Message);
        //    }

        //    //return View(entry); // MVC 5 expects Details.cshtml
        //    return PartialView("_DestDetails", entry);
        //}
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "Journal Entry ID is required");

            JournalEntry entry = null;

            try
            {
                entry = await _journalHeadService.GetJournalEntryByIdAsync(id);
            }
            catch (Exception ex)
            {
                // You can log the exception here
                return new HttpStatusCodeResult(404, ex.Message);
            }

            //return View(entry); // MVC 5 expects Details.cshtml
            return PartialView("_DestDetails", entry);
        }


        public async Task<ActionResult> Table()
        {
            
            return View();
        }
    }
}