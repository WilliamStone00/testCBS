using CBS.BusinessService.CheckManagementSystem.LossManagementSystem;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CheckManagementSystem.LossManagementSystem
{
    [CheckSessionTimeOutAttribute]
    public class LossManagementController : BaseController
    {
        private readonly LossManagementService _lossManagementService;
        private readonly MockLossManagementService _mockCustomerCheckbooks;

        public LossManagementController(LossManagementService lossManagementService, MockLossManagementService mockCustomerCheckbooks)
        {
            _lossManagementService = lossManagementService;
            _mockCustomerCheckbooks = mockCustomerCheckbooks;
        }

        // GET: LossManagement - Landing page with search input
        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetCheckbooksByCustomer(string memberRef)
        {
            if (string.IsNullOrEmpty(memberRef))
            {
                return Json(new { success = false, message = "Member Reference is required." });
            }

            var result = await _mockCustomerCheckbooks.GetCustomerCheckbooks(memberRef);
            if (result != null && result.Checkbooks.Any())
            {
                return PartialView("_CheckbooksTable", result);
            }

            return PartialView("_NoCheckbooksFound");
        }

        [HttpPost]
        public async Task<ActionResult> GetCheckLeavesByCheckbook(string checkBookId)
        {
            var result = await _mockCustomerCheckbooks.GetCheckLeavesByCheckbook(checkBookId);
            if (result != null && result.CheckLeaves.Any())
            {
                return PartialView("_CheckLeavesTable", result);
            }

            return PartialView("_NoCheckLeavesFound");
        }

        // This should return a partial view for the modal
        public async Task<ActionResult> RequestLossForCheckbook(string checkBookId)
        {
            var lossRequestData = await _mockCustomerCheckbooks.RequestLossForCheckbook(checkBookId);
            if (lossRequestData != null)
            {
                return PartialView("_LossRequestForm", lossRequestData);
            }

            return PartialView("_Error", "Unable to load loss request form for the selected checkbook.");
        }

        // This should return a partial view for the modal
        public async Task<ActionResult> RequestLossForCheckLeaf(string checkLeafId)
        {
            var lossRequestData = await _mockCustomerCheckbooks.RequestLossForCheckLeaf(checkLeafId);
            if (lossRequestData != null)
            {
                return PartialView("_LossRequestForm", lossRequestData);
            }

            return PartialView("_Error", "Unable to load loss request form for the selected check leaf.");
        }

        // This should return a partial view for the modal with checkbook details AND check leaves
        public async Task<ActionResult> CheckbookDetails(string checkBookId)
        {
            var checkbookDetails = await _mockCustomerCheckbooks.GetCheckbookDetails(checkBookId);
            if (checkbookDetails != null)
            {
                return PartialView("_CheckbookDetails", checkbookDetails);
            }

            return PartialView("_Error", "Checkbook details not found.");
        }

        public async Task<ActionResult> CheckLeafDetails(string checkLeafId)
        {
            var checkLeafDetails = await _mockCustomerCheckbooks.GetCheckLeafDetails(checkLeafId);
            if (checkLeafDetails != null)
            {
                return PartialView("_CheckLeafDetails", checkLeafDetails);
            }

            return PartialView("_Error", "Check leaf details not found.");
        }

        // Add these to your LossManagementController
        public async Task<ActionResult> GetBranches()
        {
            var branches = await _mockCustomerCheckbooks.GetBranches();
            return Json(branches, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetLossReasons()
        {
            var reasons = await _mockCustomerCheckbooks.GetLossReasons();
            return Json(reasons, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetClientSuggestions()
        {
            var suggestions = await _mockCustomerCheckbooks.GetClientSuggestions();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetReportedByOptions()
        {
            var options = await _mockCustomerCheckbooks.GetReportedByOptions();
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitLossRequest(LossRequestDto request)
        {
            if (ModelState.IsValid)
            {
                var result = await _mockCustomerCheckbooks.SubmitLossRequest(request);
                if (result.Result)
                {
                    return Json(new
                    {
                        success = true,
                        status = result.MessageStatus,
                        message = Messaging.MessageResult(result)
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        status = result.MessageStatus,
                        message = Messaging.MessageResult(result)
                    });
                }
            }

            return Json(new
            {
                success = false,
                status = false,
                message = "Please fill all required fields correctly."
            });
        }
    }
}