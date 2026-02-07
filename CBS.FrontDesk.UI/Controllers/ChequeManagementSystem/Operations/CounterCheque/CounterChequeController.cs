using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque;
using CBS.BusinessService.Config; // Assuming BranchServices is here
using CBS.FrontDesk.Data.Entity.AccountingV2.MobileMoneyV2;
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
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Operations.CounterCheque
{
   // [CheckSessionTimeOut]
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
        /// Loads the main container page for the module.
        /// </summary>
        public async Task<ActionResult> List()
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
        public async Task<ActionResult> Search(string customerId)
        {
			if (string.IsNullOrEmpty(customerId))
				return new HttpStatusCodeResult(400, "customerId is required");

			try
			{
				var entry = await _counterChequeService.GetCustomerChequeBooks(customerId);
				if (entry == null)
					return HttpNotFound("customerId details not found");

				return PartialView("_CustomerCheckBookCarousel", entry);
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, ex.Message);
			}
		}

		[HttpGet]
		public async Task<ActionResult> GetChequeLeaves(string chequeBookId, string viewType)
		{
			if (string.IsNullOrEmpty(chequeBookId))
				return new HttpStatusCodeResult(400, "ChequeBookId is required");

			try
			{
				var leaves = await _counterChequeService.GetChequeBookWithLeaves(chequeBookId);
				if (leaves == null)
					return HttpNotFound("Cheque book details not found");

				switch (viewType?.ToLower())
				{
					case "grid":
						return PartialView("_ChequeLeavesGrid", leaves);

					case "list":
						return PartialView("_ChequeLeavesDataTable", leaves);

					default:
						return new HttpStatusCodeResult(400, "Invalid view type");
				}
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, ex.Message);
			}
		}


		[HttpGet]
		public async Task<ActionResult> GetChequeLeafDetails(string leafId)
		{
			if (string.IsNullOrEmpty(leafId))
				return new HttpStatusCodeResult(400, "LeafId is required");

			try
			{
				var leaf = await _counterChequeService.GetChequeLeafDetails(leafId);

				if (leaf == null)
					return HttpNotFound("Cheque leaf not found");

				return PartialView("_ChequeLeafDetails", leaf);
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, ex.Message);
			}
		}

		/// <summary
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

                var data = new CounterChecks() {CheckLeafId = KEY };
                return PartialView(partialView, data);
            }
            else if (path == "action")
            {
                // Prepares the model for the modal action form
                var model = new CounterChequeActionDto { CounterChequeId = KEY };
                return PartialView(partialView, model);
            }
            return HttpNotFound();
        }

		[HttpPost]
		public async Task<ActionResult> Create(CounterChecks model)
		{

			// Validate the model state
			if (!ModelState.IsValid)
			{
				// If model validation fails, return validation errors as JSON response
				return Json(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
			}

			// If the Id is null, it's a new holiday entry, so call the Create service
			if (model.Id == null)
			{
				// Adding Created Date
				var data = await _counterChequeService.Create(model);
				return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
			}
			else
			{
				// If the Id is not null, it's an update, so call the Update method
				return await Update(model);
			}
		}

		[HttpPost]
		public async Task<ActionResult> Update(CounterChecks model)
		{
			var data = await _counterChequeService.Update(model);
			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
		}

		/// <summary>
		/// Handles the submission of a new counter cheque. Called by 'AjaxPostAndUpdate'.
		/// </summary>
		[HttpPost]
        public async Task<ActionResult> CreateT(CounterChecks model)
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
                    return PartialView("_CounterChequeDetails", new CounterChecks());
                }

                var counterCheque = await _counterChequeService.GetCounterChequeDetailsAsync(id);

                // If null, return empty model to avoid Razor null refs
                if (counterCheque == null) counterCheque = new CounterChecks();

                return PartialView("_CounterChequeDetails", counterCheque);
            }
            catch (Exception ex)
            {
                // log (keep your logging approach)
                System.Diagnostics.Debug.WriteLine($"Error in GetCounterChequeDetails: {ex.Message}");
                ViewBag.ErrorMessage = "Failed to load counter cheque details. Please try again.";
                return PartialView("_CounterChequeDetails", new CounterChecks());
            }
        }


        [HttpPost]
        public async Task<JsonResult> LoadCounterCheques(CounterChequeQuery query)
        {
            try
            {
                var data = await _counterChequeService.GetCounterChequesForDataTableAsync(query);
                var numConfigs = JsonConvert.DeserializeObject<List<CounterChequeDto>>(JsonConvert.SerializeObject(data.data));
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



        // Add this method to your CounterChequeController
        [HttpGet]
        public async Task<ActionResult> GetCustomerStatistics(string customerId)
         {
            if (string.IsNullOrEmpty(customerId))
                return new HttpStatusCodeResult(400, "CustomerId is required");

            try
            {
                var statistics = await _counterChequeService.GetCustomerCheckBookStatistics(customerId);
                if (statistics == null)
                    return HttpNotFound("Customer statistics not found");

                return PartialView("_CustomerStatistics", statistics);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }


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

		[HttpGet]
		public async Task<ActionResult> CounterRequest(string leafId)
		{
			if (string.IsNullOrEmpty(leafId))
				return HttpNotFound("LeafId is required");

			await Loader();

			var leaf = await _counterChequeService.GetChequeLeafDetails(leafId);
			if (leaf == null)
				return HttpNotFound("Cheque leaf not found");

			return View("_CounterRequest", leaf);
		}

		[HttpGet]
		public async Task<ActionResult> Approval(string leafId)
		{
			if (string.IsNullOrEmpty(leafId))
				return HttpNotFound("LeafId is required");

			await Loader();

			var leaf = await _counterChequeService.GetChequeLeafDetails(leafId);
			if (leaf == null)
				return HttpNotFound("Cheque leaf not found");

			return View("_Approval", leaf);
		}

		[HttpGet]
		public async Task<ActionResult> Payment(string leafId)
		{
			if (string.IsNullOrEmpty(leafId))
				return HttpNotFound("LeafId is required");

			await Loader();

			var leaf = await _counterChequeService.GetChequeLeafDetails(leafId);
			if (leaf == null)
				return HttpNotFound("Cheque leaf not found");

			return View("_Payment", leaf);
		}


	}
}