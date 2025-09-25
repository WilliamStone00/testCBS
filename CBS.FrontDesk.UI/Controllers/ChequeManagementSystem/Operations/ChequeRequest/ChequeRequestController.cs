using Antlr.Runtime.Misc;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeRequestService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Operations.ChequeRequest
{
   // [CheckSessionTimeOut]
    public class ChequeRequestController : BaseController
    {
       
        //private readonly ChequeRequestService _chequeRequestService;   
        private readonly ChequeRequestMockService _chequeRequestService;
        private readonly BranchServices _branchServices;
        private readonly CategoryConfigService _categoryServices;
        private readonly CustomerService _customerService;


        public ChequeRequestController(ChequeRequestMockService chequeRequestService,BranchServices branchServices,CategoryConfigService categoryConfigService,CustomerService customerService)
        {
            _chequeRequestService = chequeRequestService;
            _branchServices = branchServices;
            _categoryServices = categoryConfigService;
            _customerService = customerService;
        }

        // The main container page.
        public async Task<ActionResult> Index()
        {
            await Loader();
            return View();
        }

        // Helper to load ViewBag data for the forms.
        private async Task Loader()
        {

            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var categories = await _categoryServices.GetCategories();
            ViewBag.Categories = categories;

            // Mock data for the category dropdown in the create form
            //ViewBag.Categories = new List<SelectListItem>
            //{
            //    new SelectListItem { Value = "CAT01", Text = "Standard - 25 Leaves" },
            //    new SelectListItem { Value = "CAT02", Text = "Business Gold - 100 Leaves" }
            //};

            // Mock data for a customer dropdown
            ViewBag.Customers = new List<SelectListItem>
            {
                new SelectListItem { Value = "CUST01", Text = "John Doe" },
                new SelectListItem { Value = "CUST02", Text = "Jane Smith" }
            };

            // Keep method async-friendly — replace this with real awaited calls later
            await Task.CompletedTask;
        }

        // The central router for loading all our partial views.
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            await Loader();
            if (path == "list")
            {
                var data = await _chequeRequestService.GetAllRequestsAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new ChequeBookRequest());
            }
            else // "get" for details
            {
                var data = await _chequeRequestService.GetRequestByIdAsync(KEY);
                return PartialView(partialView, data);
            }
        }

        // In ChequeRequestController.cs

        [HttpPost]
        public async Task<ActionResult> LoadRequestsForDataTable(ChequeRequestQuery query)
        {
            try
            {
                // IMPORTANT: You will need a new service method that accepts the query object
                var dataTable = await _chequeRequestService.GetRequestsForDataTableAsync(query);

                return Json(new
                {
                    draw = dataTable.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = dataTable.data
                });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error loading request data.");
            }
        }


        [HttpGet]
        public async Task<JsonResult> GetCustomerDetails(string customerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerId))
                {
                    return Json(new { success = false, message = "Customer ID is required" }, JsonRequestBehavior.AllowGet);
                }

                // Get customer data using the refactored service method
                var customerData = await _customerService.GetCustomerByIdAsync(customerId);

                if (customerData != null)
                {
                    return Json(new { success = true, data = customerData }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Customer not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to add logging here)
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(ChequeBookRequest model)
        {
            if (!ModelState.IsValid)
            {
                // Return a clear validation error
                return Json(new
                {
                    success = false,
                    message = "Validation failed. Please check the required fields.",
                    status = "ValidationError"
                });
            }

            ExecutionMessages result;
            string operationType;

            // The core logic: check if the ID is present.
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                // --- CREATE PATH ---
                result = await _chequeRequestService.CreateRequestAsync(model);
                operationType = "Insert";
            }
            else
            {
                // --- UPDATE PATH ---
                result = await _chequeRequestService.UpdateRequestAsync(model);
                operationType = "Update";
            }

            // Return the standardized, rich JSON response that your generic script expects
            return Json(new
            {
                success = result.Result,
                message = Messaging.MessageResult(result),
                status = result.MessageStatus,

                // These properties guide the generic 'AjaxPostAndUpdate' script on how to refresh the UI
                optype = operationType,
                reloadDataView = "Yes",
                controllerName = "ChequeRequest",
                divLoaderList = "datalistingview", // The div where the list should be reloaded
                dataLoaderActionName = "_RequestList" // The partial view for the list
            });
        }

        // This single action will handle Approve, Reject, and Delivered from the modal.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TakeAction(string requestId, string note, string action)
        {
            ExecutionMessages result;
            switch (action?.ToLower())
            {
                case "approve":
                    result = await _chequeRequestService.ApproveRequestAsync(requestId, note);
                    break;
                case "reject":
                    result = await _chequeRequestService.RejectRequestAsync(requestId, note);
                    break;
                case "delivered":
                    result = await _chequeRequestService.MarkAsDeliveredAsync(requestId, note);
                    break;
                default:
                    result = new ExecutionMessages { Result = false, MessageString = "Invalid action specified." };
                    break;
            }
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }
    }
}