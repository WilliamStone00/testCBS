using CBS.BusinessService.AccountingV2.CashReconciliation;
using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
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



        private readonly BranchServices _branchServices;
        private readonly GLSystemReconciliationService _glSystemReconciliationService;

        public GLSystemReconciliationController(BranchServices branchServices, GLSystemReconciliationService glSystemReconciliationService)
        {

            _branchServices = branchServices;
            _glSystemReconciliationService = glSystemReconciliationService;


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

            model.StartUtc = model.StartDate;
            model.EndUtc = model.EndDate;
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



        

        
        [HttpPost]
        public ActionResult LoadRecordsForm(ReconciliationData summary)
        {

            
            return PartialView("_RecordsForm", summary);
        }




        [HttpGet]
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "System Reconciliation ID is required");

            Data.Entity.AccountingV2.GLSystemReconciliation.ReconciliationDetails entry = null;

            try
            {
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
                var summary = await _glSystemReconciliationService.PushRecordAsync(model);

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

    }
}


    