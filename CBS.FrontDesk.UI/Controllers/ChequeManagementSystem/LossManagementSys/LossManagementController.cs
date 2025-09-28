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

        public LossManagementController(LossManagementService lossManagementService)
        {
            _lossManagementService = lossManagementService;
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

            var result = await _lossManagementService.GetCustomerCheckbooks(memberRef);
            if (result != null && result.Checkbooks.Any())
            {
                // Return as partial view for table display
                return PartialView("_CheckbooksTable", result);
            }

            return Json(new { success = false, message = "No checkbooks found for this customer." });
        }

        [HttpPost]
        public async Task<ActionResult> GetCheckLeavesByCheckbook(int checkBookId)
        {
            var result = await _lossManagementService.GetCheckLeavesByCheckbook(checkBookId);
            if (result != null && result.CheckLeaves.Any())
            {
                return PartialView("_CheckLeavesTable", result);
            }

            return Json(new { success = false, message = "No check leaves found for this checkbook." });
        }

        public async Task<ActionResult> RequestLossForCheckbook(int checkBookId)
        {
            var lossRequestData = await _lossManagementService.RequestLossForCheckbook(checkBookId);
            if (lossRequestData != null)
            {
                return View("LossRequestForm", lossRequestData);
            }

            TempData["ErrorMessage"] = "Unable to load loss request form for the selected checkbook.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> RequestLossForCheckLeaf(int checkLeafId)
        {
            var lossRequestData = await _lossManagementService.RequestLossForCheckLeaf(checkLeafId);
            if (lossRequestData != null)
            {
                return View("LossRequestForm", lossRequestData);
            }

            TempData["ErrorMessage"] = "Unable to load loss request form for the selected check leaf.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitLossRequest(LossRequestDto request)
        {
            if (ModelState.IsValid)
            {
                var result = await _lossManagementService.SubmitLossRequest(request);
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

        // Additional method for loading details view
        public async Task<ActionResult> CheckbookDetails(int checkBookId)
        {
            var checkbookDetails = await _lossManagementService.GetCheckbookDetails(checkBookId);
            if (checkbookDetails != null)
            {
                return View(checkbookDetails);
            }

            TempData["ErrorMessage"] = "Checkbook details not found.";
            return RedirectToAction("Index");
        }
    }
}