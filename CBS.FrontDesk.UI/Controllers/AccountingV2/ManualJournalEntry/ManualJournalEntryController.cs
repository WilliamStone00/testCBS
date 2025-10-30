
using CBS.BusinessService.AccountingV2;
using CBS.BusinessService.AccountingV2.ConfigurationsManualEntry;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;




namespace CBS.FrontDesk.UI.Controllers.AccountingV2
{
    public class ManualJournalEntryController : Controller
    {
        private readonly BranchServices _branchServices;
        private readonly ManualJournalEntryService _manualJournalEntryService;

        public ManualJournalEntryController(BranchServices branchServices, ManualJournalEntryService manualJournalEntryService)
        {

            _branchServices = branchServices;
            _manualJournalEntryService = manualJournalEntryService;


        }
        // GET: ManualJournalEntry
        //public async Task<ActionResult> Index()
        //{

        //    return View(new JournalEntry());
        //}

        public async Task<ActionResult> Index()
        {
            await loader();

            return View(new JournalEntryPayload());
        }


        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetCounterpartyBranches();
            ViewBag.Branches = branches;

            
            return true;
        }

        public async Task<ActionResult> GetAccounts()
        {
            try
            {
                // ✅ Await the async method
                var result = await _manualJournalEntryService.GetAccountsByBranchAsync();

                if (result == null || !result.Any())
                    return Json(new { success = false, message = "⚠️ No accounts found for your branch." }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (TaskCanceledException)
            {
                return Json(new { success = false, message = "⚠️ Timeout while fetching accounts — backend service not responding." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostJournalEntry(JournalEntryPayload model)
        {
            try
            {


                // ✅ Validate payload
                if (model == null)
                    return Json(new { success = false, message = "Invalid payload received." });

                if (model.Payload == null || model.Payload.Entries == null || !model.Payload.Entries.Any())
                    return Json(new { success = false, message = "No journal lines provided." });

                // ✅ Ensure Debit = Credit
                var totalDr = model.Payload.Entries.Where(e => e.Dr).Sum(e => e.Amount);
                var totalCr = model.Payload.Entries.Where(e => e.Cr).Sum(e => e.Amount);
                if (totalDr != totalCr)
                    return Json(new { success = false, message = "Debit and Credit totals must balance." });

                // ✅ Add metadata
                model.PostMode = "HOLD_FOR_APPROVAL";
                model.CreatedBy = User?.Identity?.Name ?? "System";
                model.CreatedDate = DateTime.Now;
                model.State = "INITIATED";

                // ✅ Call service
                var execMessage = await _manualJournalEntryService.PostJournalAsync(model);

                // ✅ Handle response (pattern same as CreateOrUpdate)
                if (execMessage == null)
                    return Json(new { success = false, message = "No response from service." });

                if (!execMessage.Result)
                    return Json(new
                    {
                        success = false,
                        message = execMessage.MessageString ?? "Failed to post journal entry.",
                        data = execMessage.Data
                    });

                return Json(new
                {
                    success = true,
                    message = execMessage.MessageString ?? "Journal entry posted successfully.",
                    data = execMessage.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        //[HttpGet]
        //public async Task<ActionResult> List()
        //{
        //    await loader();
        //    return View();
        //}

        //[HttpPost]
        //public async Task<JsonResult> LoadJournalHeaderData(JournalEntryQuery query)
        //{
        //    try
        //    {
        //        var data = await _manualJournalEntryService.GetJournalHeaderDataTableAsync(query);

        //        // Deserialize DataTable payload into strongly-typed list
        //        var journalHeaders = JsonConvert.DeserializeObject<List<CBS.FrontDesk.Data.Entity.AccountongV2.JournalEntry>>(
        //            JsonConvert.SerializeObject(data.data));

        //        return Json(new
        //        {
        //            draw = data.draw,
        //            recordsTotal = data.recordsTotal,
        //            recordsFiltered = data.recordsFiltered,
        //            data = journalHeaders
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return DataTables-compatible empty result on error
        //        return Json(new
        //        {
        //            draw = query?.Options?.draw ?? "1",
        //            recordsTotal = 0,
        //            recordsFiltered = 0,
        //            data = new List<object>(),
        //            error = ex.Message
        //        });
        //    }
        //}


        //[HttpGet]
        //public async Task<ActionResult> GetDetails(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //        return new HttpStatusCodeResult(400, "Journal Entry ID is required");

        //    JournalEntry entry = null;
        //    try
        //    {
        //        entry = await _manualJournalEntryService.GetJournalEntryByIdAsync(id);
        //    }
        //    catch (Exception ex)
        //    {
        //        // log as needed
        //        return new HttpStatusCodeResult(404, ex.Message);
        //    }

        //    // return HTML fragment (partial) — this will be injected into main modal body
        //    return PartialView("_JournalHeadDetails", entry);
        //}


        [HttpGet]
        public async Task<ActionResult> Mode()
        {
            await loader();
            return View();
        }
        //public async Task<JsonResult> LoadJournalSourceData(JournalEntryQuery query)
        //{
        //    try
        //    {
        //        var data = await _manualJournalEntryService.GetJournalHeaderDataTableAsync(query);

        //        // Deserialize DataTable payload into strongly-typed list
        //        var journalHeaders = JsonConvert.DeserializeObject<List<CBS.FrontDesk.Data.Entity.AccountongV2.JournalEntry>>(
        //            JsonConvert.SerializeObject(data.data));

        //        return Json(new
        //        {
        //            draw = data.draw,
        //            recordsTotal = data.recordsTotal,
        //            recordsFiltered = data.recordsFiltered,
        //            data = journalHeaders
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return DataTables-compatible empty result on error
        //        return Json(new
        //        {
        //            draw = query?.Options?.draw ?? "1",
        //            recordsTotal = 0,
        //            recordsFiltered = 0,
        //            data = new List<object>(),
        //            error = ex.Message
        //        });
        //    }
        //}



        

        [HttpGet]
        public async Task<ActionResult> Table()
        {
            await loader();
            return View();
        }

        //[HttpPost]
        //public async Task<JsonResult> LoadDestinationDataTable(JournalEntryQuery query)
        //{
        //    try
        //    {
        //        var data = await _manualJournalEntryService.GetJournalHeaderDataTableAsync(query);

        //        // Deserialize DataTable payload into strongly-typed list
        //        var journalHeaders = JsonConvert.DeserializeObject<List<CBS.FrontDesk.Data.Entity.AccountongV2.JournalEntry>>(
        //            JsonConvert.SerializeObject(data.data));

        //        return Json(new
        //        {
        //            draw = data.draw,
        //            recordsTotal = data.recordsTotal,
        //            recordsFiltered = data.recordsFiltered,
        //            data = journalHeaders
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return DataTables-compatible empty result on error
        //        return Json(new
        //        {
        //            draw = query?.Options?.draw ?? "1",
        //            recordsTotal = 0,
        //            recordsFiltered = 0,
        //            data = new List<object>(),
        //            error = ex.Message
        //        });
        //    }
        //}
    }




}

