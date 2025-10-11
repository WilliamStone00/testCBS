using Antlr.Runtime.Misc;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Configurations.ChequeNumber;
using CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeRequestService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Operations.ChequeRequest
{
   [CheckSessionTimeOut]
    public class ChequeRequestController : BaseController
    {
       
        private readonly ChequeRequestService _chequeRequestService1;   
      //  private readonly ChequeRequestMockService _chequeRequestService;
        private readonly BranchServices _branchServices;
        private readonly CategoryConfigService _categoryServices;
        private readonly CustomerService _customerService;


        public ChequeRequestController(BranchServices branchServices,CategoryConfigService categoryConfigService,CustomerService customerService,ChequeRequestService chequeRequestService1)
        {
            //_chequeRequestService = chequeRequestService;
            _branchServices = branchServices;
            _categoryServices = categoryConfigService;
            _customerService = customerService;
            _chequeRequestService1 = chequeRequestService1;
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

          
            // Keep method async-friendly — replace this with real awaited calls later
            await Task.CompletedTask;
        }

        // The central router for loading all our partial views.
        // Keep your existing controller, just ensure these actions exist:

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            await Loader();

            if (path == "list")
            {
                var data = await _chequeRequestService1.GetAllRequestsAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new ChequeBookRequest());
            }
            else if (path == "get")
            {
                var data = await _chequeRequestService1.GetRequestByIdAsync(KEY);
                return PartialView(partialView, data);
            }

            return PartialView("_Error");
        }

        //[HttpGet]
        //public async Task<ActionResult> GetCustomerAccounts(string customerId)
        //{
        //    try
        //    {
        //        // Use IsNullOrWhiteSpace like your existing pattern
        //        if (string.IsNullOrWhiteSpace(customerId))
        //            return Json(new { success = false, message = "Customer ID is required." });

        //        var customerData = await _customerService.GetCustomerAccountDropdownAsync(customerId);

        //        if (customerData?.AccountSelectList?.Any() != true)
        //            return Json(new { success = false, message = "No accounts found for this customer." });

        //        return Json(new { success = true, data = customerData });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception here if you have logging
        //        // _logger.LogError(ex, "Error fetching customer accounts for {CustomerId}", customerId);

        //        return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        //    }
        //}

        [HttpGet]
        public async Task<ActionResult> GetRequestForm(string customerId)
        {
            await Loader();
            try
            {
                if (string.IsNullOrWhiteSpace(customerId))
                    return PartialView("_ChequeRequestForm", new ChequeBookRequest());

                // Get customer data to populate ViewBag
                var customerData = await _customerService.GetCustomerAccountDropdownAsync(customerId);

                if (customerData?.AccountSelectList?.Any() == true)
                {
                    // Pass accounts to ViewBag for the form
                    ViewBag.CustomerAccounts = customerData.AccountSelectList;

                    // Pre-populate the model with customer info
                    var model = new ChequeBookRequest
                    {
                        CustomerId = customerId,
                        CustomerName = $"{customerData.CustomerDto.FirstName} {customerData.CustomerDto.LastName}"
                    };

                    return PartialView("_ChequeRequestForm", model);
                }

                return PartialView("_ChequeRequestForm", new ChequeBookRequest());
            }
            catch (Exception )
            {
                // Log error
                return PartialView("_ChequeRequestForm", new ChequeBookRequest());
            }
        }

        [HttpPost]
        public async Task<ActionResult> TakeAction(Approval approval, string action)
        {
            if (approval.approvalNote == null)
                return Json(new { success = false, message = "Please Enter a value ." });


            // Your existing logic
            ExecutionMessages result;
            switch (action?.ToLower())
            {
                case "approve":
                    result = await _chequeRequestService1.ApproveRequestAsync(approval);
                    break;
                case "reject":
                    result = await _chequeRequestService1.RejectRequestAsync(approval);
                    break;
                //case "review":
                //    // Add review logic to your service
                //    result = await _chequeRequestService1.ReviewRequestAsync(requestId, note);
                //    break;
                case "delivered":
                    // change to correct method when endpoint provided
                    result = await _chequeRequestService1.RejectRequestAsync(approval);
                    break;
                default:
                    result = new ExecutionMessages { Result = false, MessageString = "Invalid action" };
                    break;
            }

            return Json(new { success = result.Result, message = result.MessageString });
        }
       
        [HttpPost]
        public async Task<JsonResult> LoadChequeBooksData(ChequeRequestQuery query)
        {
            try
            {
                var data = await _chequeRequestService1.GetChequeBooksrequestDataTableAsync(query);

                var numConfigs = JsonConvert.DeserializeObject<List<ChequeBookRequest>>(JsonConvert.SerializeObject(data.data));

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
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

       //[HttpGet]
        //public async Task<JsonResult> GetCustomerDetails(string customerId)
        
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(customerId))
        //        {
        //            return Json(new { success = false, message = "Customer ID is required" }, JsonRequestBehavior.AllowGet);
        //        }

        //        // Get customer data using the refactored service method
        //        var customerData = await _customerService.GetCustomerAccountDropdownAsync(customerId);

        //        if (customerData != null)
        //        {
        //            return Json(new { success = true, data = customerData }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Customer not found" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (you might want to add logging here)
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

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
                result = await _chequeRequestService1.CreateRequestAsync(model);
                operationType = "Insert";
            }
            else
            {
                // --- UPDATE PATH ---
                result = await _chequeRequestService1.CreateRequestAsync(model);
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

       
    }
}