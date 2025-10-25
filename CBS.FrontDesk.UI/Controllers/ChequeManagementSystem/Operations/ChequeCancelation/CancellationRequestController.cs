using CBS.BusinessService.CheckManagementSystem.ChequeClearance;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeCancelation;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeCancelation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Operations.ChequeCancelation
{
    public class ChequeCancellationController : BaseController
    {
        private readonly ChequeCancellationService _chequeCancellationService;
        private readonly ChequeCancellationMockService _chequeCancellationMockService;
        private readonly BranchServices _branchServices;

        public ChequeCancellationController(
            ChequeCancellationService chequeCancellationService,
            ChequeCancellationMockService chequeCancellationMockService,
            BranchServices branchServices)
        {
            _chequeCancellationService = chequeCancellationService;
            _chequeCancellationMockService = chequeCancellationMockService;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await LoadViewBagData();
            return View();
        }

        private async Task LoadViewBagData()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            ViewBag.StatusList = new[]
            {
                new { Value = "Pending", Text = "Pending" },
                new { Value = "Approved", Text = "Approved" },
                new { Value = "Rejected", Text = "Rejected" },
                new { Value = "Cancelled", Text = "Cancelled" },
                
            };
        }

        // GET: Load partial views for listing, forms, details
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "list")
            {
                await LoadViewBagData();

                try
                {
                    var data = await _chequeCancellationMockService.GetAllAsync();
                    return PartialView(partialView ?? "_CancellationDataTable", data);
                }
                catch (Exception ex)
                {
                    // Fall back to mock service
                    var mockData = await _chequeCancellationMockService.GetCancellationRequestsAsync();
                    return PartialView(partialView, mockData);
                }
            }
            else if (path == "new")
            {
                await LoadViewBagData();
                return PartialView(partialView, new CancellationRequest());
            }
            
            else if (path == "review")
            {
                // For review form, we just need the request ID
                ViewBag.RequestId = KEY;
                return PartialView(partialView);
            }
            else
            {
                await LoadViewBagData();
                try
                {
                    var data = await _chequeCancellationService.GetCancellationRequestByIdAsync(KEY);
                    if (data == null)
                    {
                        data = await _chequeCancellationMockService.GetCancellationRequestByIdAsync(KEY);
                    }
                    return PartialView(partialView, data);
                }
                catch (Exception ex)
                {
                    var data = await _chequeCancellationMockService.GetCancellationRequestByIdAsync(KEY);
                    return PartialView(partialView, data);
                }
            }
        }

        // POST: Create or Update cancellation request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(CancellationRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            ExecutionMessages result;
            string operationType;

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                // Try main service first
                try
                {
                    result = await _chequeCancellationService.CreateCancellationRequestAsync(model);
                }
                catch (Exception ex)
                {
                    // Fall back to mock service
                    result = await _chequeCancellationMockService.CreateCancellationRequestAsync(model);
                }
                operationType = "Insert";
            }
            else
            {
                // Try main service first
                try
                {
                    result = await _chequeCancellationService.UpdateCancellationRequestAsync(model);
                }
                catch (Exception ex)
                {
                    // Fall back to mock service
                    result = await _chequeCancellationMockService.UpdateCancellationRequestAsync(model);
                }
                operationType = "Update";
            }

            bool isAjaxSuccess = result.Result || (result.MessageStatus == SystemMessageStatus.Exist.ToString());

            return Json(new
            {
                success = isAjaxSuccess,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result),
                optype = operationType,
                reloadDataView = "Yes",
                controllerName = "ChequeCancellation",
                divLoaderList = "datalistingview",
                tableName = "cancellationDataTable",
                dataLoaderActionName = "_CancellationDataTable",
                divLoaderCreator = "datalistingview",
                reinitializedActionName = "_CancellationRequests"
            });
        }



        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

            var clearance = await _chequeCancellationMockService.GetByIdAsync(id);

            if (clearance == null)
                return HttpNotFound();

            return PartialView("_CancellationRequestDetails", clearance);
        }

        [HttpGet]
        public ActionResult GetActionForm(string fileUploadId, string mode)
        {
            var model = new CancellationValidatiion
            {
                Id = fileUploadId,
                ApprovedBy = Session["FullName"]?.ToString(),
                Mode = mode // "approve", "review", or "reject",""
            };
            return PartialView("_VerificationForm", model);
        }




        [HttpPost]
        public async Task<ActionResult> SubmitAction(CancellationValidatiion model)
        {
            if (!ModelState.IsValid)
            {
                // --- THIS IS THE CRITICAL CHANGE ---
                // We need to return the ModelState errors in a format the client can parse.
                var errors = new Dictionary<string, string[]>();
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        errors[key] = state.Errors.Select(e => e.ErrorMessage).ToArray();
                    }
                }
                return Json(new { success = false, message = "Please correct the validation errors.", errors = errors });
            }

            var result = await _chequeCancellationService.SubmitFileActionAsync(model);
            // Ensure Messaging.MessageResult returns a string
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }



















        // POST: Review/Approve/Reject request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReviewRequest(string requestId, bool isApproved, string statement)
        {
            if (string.IsNullOrEmpty(requestId))
                return Json(new { success = false, message = "Invalid request ID." });

            ExecutionMessages result;

            // Try main service first
            try
            {
                result = await _chequeCancellationService.ReviewCancellationRequestAsync(requestId, isApproved, statement);
            }
            catch (Exception ex)
            {
                // Fall back to mock service
                result = await _chequeCancellationMockService.ReviewCancellationRequestAsync(requestId, isApproved, statement);
            }

            bool isAjaxSuccess = result.Result || (result.MessageStatus == SystemMessageStatus.Exist.ToString());

            return Json(new
            {
                success = isAjaxSuccess,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result),
                optype = "Review",
                reloadDataView = "Yes",
                controllerName = "ChequeCancellation",
                divLoaderList = "datalistingview",
                tableName = "cancellationDataTable",
                dataLoaderActionName = "_CancellationDataTable",
                divLoaderCreator = "datalistingview",
                reinitializedActionName = "_CancellationRequests"
            });
        }

        // GET: Delete/Deactivate request
        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." }, JsonRequestBehavior.AllowGet);

            ExecutionMessages result;

            // Try main service first
            try
            {
                result = await _chequeCancellationService.DeactivateCancellationRequestAsync(KEY);
            }
            catch (Exception ex)
            {
                // Fall back to mock service
                result = await _chequeCancellationMockService.DeactivateCancellationRequestAsync(KEY);
            }

            if (result.Result)
            {
                return Json(new
                {
                    success = true,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result),
                    controller = "ChequeCancellation",
                    datatable = "cancellationDataTable",
                    PartialView = "_CancellationDataTable",
                    pr = 0,
                    div = "datalistingview",
                    id = (string)null,
                    path = "list"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = false,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: DataTable endpoint
        [HttpPost]
        public async Task<JsonResult> GetCancellationRequestsDataTable(CancellationRequestQuery query)
        {
            try
            {
                CustomDataTable data;

                // Try main service first
                try
                {
                    data = await _chequeCancellationService.GetCancellationRequestsDataTableAsync(query);
                }
                catch (Exception ex)
                {
                    // Fall back to mock service
                    data = await _chequeCancellationMockService.GetCancellationRequestsDataTableAsync(query);
                }

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = data.data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.DataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = "Failed to load cancellation requests"
                });
            }
        }
    }
}