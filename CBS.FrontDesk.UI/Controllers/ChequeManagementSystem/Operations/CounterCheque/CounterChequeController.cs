using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque;
using CBS.BusinessService.Config; // Assuming BranchServices is here
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
using CBS.FrontDesk.Data.Message;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Operations.CounterCheque
{
    [CheckSessionTimeOut]
    public class CounterChequeController : BaseController
    {
        // Depend on the INTERFACE, not the concrete class.
        private readonly CounterChequeService _counterChequeService;
        private readonly BranchServices _branchServices;
        // Inject customer service here if you need a customer dropdown

        public CounterChequeController(CounterChequeService counterChequeService, BranchServices branchServices)
        {
            _counterChequeService = counterChequeService;
            _branchServices = branchServices;
        }

        /// <summary>
        /// Loads the main container page for the module.
        /// </summary>
        public async Task<ActionResult> Index()
        {
            await Loader();
            return View();
        }

        /// <summary>
        /// Prepares data needed for dropdowns in the partial views.
        /// </summary>
        private async Task Loader()
        {           
            ViewBag.Branches = await _branchServices.GetBranches();
           // ViewBag.Customers = await _customerService.GetActiveCustomersForDropdown();
        }


        [HttpGet]
        public async Task<ActionResult> Search(string CustomerId, string BranchId)
        {
            if (string.IsNullOrEmpty(CustomerId) || string.IsNullOrEmpty(BranchId))
                return new HttpStatusCodeResult(400, "CustomerId and BranchId are required");

            List<CounterCheques> entries;

            try
            {
                entries = await _counterChequeService.GetChequeDetails(CustomerId, BranchId);

                if (entries == null || !entries.Any())
                    return new HttpStatusCodeResult(404, "No cheque books found");

                // Return only id and name for dropdown
                var result = entries.Select(x => new { chequebookid = x.ChequeBookId, name = x.Name }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, ex.Message);
            }
        }



        /// <summary>

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            await Loader();
            if (path == "list")
            {
                // For a server-side DataTable, we just return the empty partial view.
                // The view's script will make the AJAX call to get the data.
                return PartialView(partialView);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new CounterCheques());
            }
            else if (path == "action")
            {
                // Prepares the model for the modal action form
                var model = new CounterChequeActionDto { CounterChequeId = KEY };
                return PartialView(partialView, model);
            }
            return HttpNotFound();
        }

        /// <summary>
        /// Handles the submission of a new counter cheque. Called by 'AjaxPostAndUpdate'.
        /// </summary>
        [HttpPost]
       
        public async Task<ActionResult> Create(CounterCheques model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed. Please fill all required fields." });
            }

            var result = await _counterChequeService.IssueCounterChequeAsync(model);

            // Return the rich JSON response your generic script expects to reload the list view
            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result),
                optype = "Insert",
                reloadDataView = "Yes",
                controllerName = "CounterCheque",
                divLoaderList = "datalistingview",
                dataLoaderActionName = "_DataTable" // The name of the partial view for the list
            });
        }

        [HttpGet]
        public async Task<ActionResult> GetCounterChequeDetails(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    // return an empty model fragment if id missing
                    return PartialView("_CounterChequeDetails", new CounterCheques());
                }

                var counterCheque = await _counterChequeService.GetCounterChequeDetailsAsync(id);

                // If null, return empty model to avoid Razor null refs
                if (counterCheque == null) counterCheque = new CounterCheques();

                return PartialView("_CounterChequeDetails", counterCheque);
            }
            catch (Exception ex)
            {
                // log (keep your logging approach)
                System.Diagnostics.Debug.WriteLine($"Error in GetCounterChequeDetails: {ex.Message}");
                ViewBag.ErrorMessage = "Failed to load counter cheque details. Please try again.";
                return PartialView("_CounterChequeDetails", new CounterCheques());
            }
        }


        [HttpPost]
        public async Task<JsonResult> LoadCounterCheques(CounterChequeQuery query)
        {
            try
            {
                var data = await _counterChequeService.GetCounterChequesForDataTableAsync(query);
                var numConfigs = JsonConvert.DeserializeObject<List<ChequeRequestDto>>(JsonConvert.SerializeObject(data.data));
                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = numConfigs
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



        /// <summary>
        /// Handles the submission of an action (Review, Validate, Reject) from the modal.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TakeAction(CounterChequeActionDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "A motive/reason is required." });
            }
            var result = await _counterChequeService.TakeActionAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        //public async Task<ActionResult> GetChequeFullDetails(string chequeBookId)
        //{
        //    if (string.IsNullOrEmpty(chequeBookId))
        //        return Json(new { success = false, message = "Invalid parameters." }, JsonRequestBehavior.AllowGet);

        //    try
        //    {
        //        var result = await _counterChequeService.GetfullChequeDetails(chequeBookId);

        //        if (result == null)
        //            return Json(new { success = false, message = "ChequeBook not found." }, JsonRequestBehavior.AllowGet);

        //        return Json(new
        //        {
        //            success = true,
        //            data = result
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpGet]
        public async Task<ActionResult> GetChequeFullDetails(string chequeBookId)
        {
            if (string.IsNullOrEmpty(chequeBookId))
                return new HttpStatusCodeResult(400, "ChequeBookId is required");

            try
            {
                var entry = await _counterChequeService.GetfullChequeDetails(chequeBookId);
                if (entry == null)
                    return HttpNotFound("Cheque book details not found");

                return PartialView("_Detailscountercheque", entry);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }



    }
}