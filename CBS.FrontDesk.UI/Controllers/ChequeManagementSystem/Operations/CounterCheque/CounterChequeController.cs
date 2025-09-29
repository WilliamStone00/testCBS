using CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque;
using CBS.BusinessService.Config; // Assuming BranchServices is here
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
using CBS.FrontDesk.Data.Message;
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

        /// <summary>
        /// The central router action that loads different partial views based on the path.
        /// Works with your generic 'LoadDataGen' and 'AddORUpdateGen' scripts.
        /// </summary>
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
        [ValidateAntiForgeryToken]
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

        /// <summary>
        /// The dedicated AJAX endpoint for the server-side DataTable.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> LoadCounterCheques(CounterChequeQuery query)
        {
            try
            {
                var dataTable = await _counterChequeService.GetCounterChequesForDataTableAsync(query);

                // Deserialize the generic 'data' into our strongly-typed object
                var dataList = JsonConvert.DeserializeObject<List<CounterCheques>>(JsonConvert.SerializeObject(dataTable.data));

                // Return the data in the exact format the DataTable expects
                return Json(new
                {
                    draw = dataTable.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = dataList
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return new HttpStatusCodeResult(500, "An error occurred while loading data.");
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
    }
}