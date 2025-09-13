using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Azure.Core.HttpHeader;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{

    [CheckSessionTimeOutAttribute]

    public class MobileMoneyCashTopupController : BaseController
    {
        // GET: MobileMoneyCashTopup
        private readonly MobileMoneyTopupServices _services;
        private readonly BranchServices _branchServices;


        public MobileMoneyCashTopupController(MobileMoneyTopupServices services, BranchServices branchServices = null)
        {
            _services = services;
            _branchServices = branchServices;
        }
        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View(new AddMobileMoneyCashTopup());
        }
        public async Task<ActionResult> PendingRequest()
        {
            var getAllMobileMoneyCashTopupQuery = new GetAllMobileMoneyCashTopupQuery { BranchId = Session["BranchID"].ToString(), ByBranch = true, QueryParameter = "Pending" };
            var pending = await _services.GetMobileMoneyCashTopups(getAllMobileMoneyCashTopupQuery);
            return View(pending.ToList());
        }
        [HttpGet]
        public async Task<ActionResult> AllRequests()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AllRequests(string branchid, string status)
        {
            try
            {
                if (string.IsNullOrEmpty(branchid) || string.IsNullOrEmpty(status))
                {
                    return Json(new { error = "Invalid data provided." });
                }

                var datatableOptions = PostDataTableOptions();
                int pageSize = datatableOptions.length != null ? Convert.ToInt32(datatableOptions.length) : 0;
                int skip = datatableOptions.start != null ? Convert.ToInt32(datatableOptions.start) : 0;

                IQueryable<MobileMoneyCashTopup> gridItems = null;

                var getAllMobileMoneyCashTopupQuery = new GetAllMobileMoneyCashTopupQuery
                {
                    BranchId = branchid,
                    ByBranch = true,
                    QueryParameter = status
                };

                var pending = await _services.GetMobileMoneyCashTopups(getAllMobileMoneyCashTopupQuery);
                gridItems = pending.AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(datatableOptions.searchValue))
                {
                    var searchValueLower = datatableOptions.searchValue.ToLower();
                    gridItems = gridItems.Where(obj => obj.Id.ToString().Contains(searchValueLower));
                }

                // Sorting
                //if (!string.IsNullOrEmpty(datatableOptions.sortColumnName) && !string.IsNullOrEmpty(datatableOptions.sortColumnDirection))
                //{
                //    var sortDirection = datatableOptions.sortColumnDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) ? "ascending" : "descending";
                //    gridItems = gridItems.OrderBy($"{datatableOptions.sortColumnName} {sortDirection}");
                //}

                int resultTotal = gridItems.Count();
                var result = gridItems.Skip(skip).Take(pageSize).ToList();

                return Json(new
                {
                    draw = datatableOptions.draw,
                    recordsFiltered = resultTotal,
                    recordsTotal = resultTotal,
                    data = result
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { error = "An error occurred while processing your request." });
            }
        }
        //[HttpPost]
        //public async Task<ActionResult> LoadData(string searchCriteria = "All")
        //{
        //    try
        //    {
        //        var dataTable = await auditTrailServices.GetDataTable(PostDataTableOptions(), searchCriteria, true);
        //        return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        public async Task<ActionResult> RequestValidation(string Id)
        {
            var cashReplenishmentSub = await _services.GetMobileMoneyCashTopup(Id);
            if (cashReplenishmentSub.Id != "0")
            {
                ViewBag.HasError = false;
                cashReplenishmentSub.ValidateMobileMoneyCashTopup.RequestApprovalNote = $"Approved: {cashReplenishmentSub.OperatorType.ToUpper()} top-up request authorized to ensure sufficient funds for customer transactions and service continuity.";
                cashReplenishmentSub.RequestApprovedBy = Session["FullName"].ToString();
                cashReplenishmentSub.ValidateMobileMoneyCashTopup.RequestApprovalStatus = "Approved";
                return View(cashReplenishmentSub);

            }
            else
            {
                ViewBag.HasError = true;
                ViewBag.Error = cashReplenishmentSub.RequestApprovalNote;
                return View();
            }
        }
        public async Task<ActionResult> Request()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;

            return View(new AddMobileMoneyCashTopup());
        }
        [HttpPost]
        public async Task<ActionResult> Create(AddMobileMoneyCashTopup model)
        {
            if (!ModelState.IsValid)
            {
                // If model state is invalid, return the validation errors in the response
                return Json(new { success = false, status = "ValidationFailed", message = "Invalid input data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            // Proceed if model is valid
            var data = await _services.Create(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        [HttpPost]
        public async Task<ActionResult> Validate(MobileMoneyCashTopup model)
        {
            if (model.ValidateMobileMoneyCashTopup == null)
            {
                // If ValidateMobileMoneyCashTopup is not provided, return an error message
                return Json(new { success = false, status = "ValidationFailed", message = "Validation data is missing" });
            }

            // Manually validate only ValidateMobileMoneyCashTopup
            if (!ModelState.IsValid)
            {
                // If validation fails, return validation errors
                return Json(new { success = false, status = "ValidationFailed", message = "Invalid validation data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            // Proceed with the validation process if model is valid
            var data = await _services.ValidateRequest(model.ValidateMobileMoneyCashTopup);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }



        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string dateFrom = null, string dateTo = null)
        {

            if (path == "new")
            {
                return PartialView(partialView, new AddMobileMoneyCashTopup());
            }
            else
            {
                var Guaranty = await _services.GetMobileMoneyCashTopup(KEY);
                return PartialView(partialView, Guaranty);

            }
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _services.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> Details(string id)
        {
            try
            {
                // Ensure the ID is valid
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid ID provided." });
                }

                // Fetch the details of the MobileMoneyCashTopup by ID
                var cashTopup = await _services.GetMobileMoneyCashTopup(id);

                // Check if the entry exists
                if (cashTopup == null)
                {
                    return Json(new { success = false, message = "Record not found." });
                }

                // Return the details in a JSON format, including the success flag
                return Json(new { success = true, data = cashTopup }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

    }
}