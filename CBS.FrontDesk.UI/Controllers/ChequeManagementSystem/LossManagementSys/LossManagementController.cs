using CBS.BusinessService.CheckManagementSystem.LossManagementSystem;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.ChequeBookQuery;

namespace CBS.FrontDesk.UI.Controllers.CheckManagementSystem.LossManagementSystem
{
    [CheckSessionTimeOutAttribute]
    public class LossManagementController : BaseController
    {
        private readonly LossManagementService _lossManagementService;
        //private readonly MockLossManagementService _mockCustomerCheckbooks;

        public LossManagementController(LossManagementService lossManagementService)
        {
            _lossManagementService = lossManagementService;
            //_mockCustomerCheckbooks = mockCustomerCheckbooks;
        }

        // GET: LossManagement - Landing page with search input
        public async Task<ActionResult> Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<ActionResult> GetCheckbooksByCustomer(string memberRef)
        {
            //if (string.IsNullOrEmpty(memberRef))
            //{
            //    return Json(new { success = false, message = "Member Reference is required." });
            //}

            var result = await _lossManagementService.GetCustomerCheckbooks(memberRef);
            if (result != null && result.Checkbooks.Any())
            {
                return PartialView("_CheckbooksTable", result);
            }

            return PartialView("_NoCheckbooksFound");
        }

        [HttpPost]
        public async Task<JsonResult> LoadCheckbooksData(ChequeBookQuery query)
        {
            try
            {
                var dataTable = await _lossManagementService.GetChequeBooksDataTableAsync(query);

                var chequeBooks = JsonConvert.DeserializeObject<List<Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.ChequeBook>>(JsonConvert.SerializeObject(dataTable.data));

                return Json(new
                {
                    draw = dataTable.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = chequeBooks
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.Options.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        //[HttpPost]
        //public async Task<JsonResult> LoadChequeBooksData(ChequeBookQuery query)
        //{
        //    try
        //    {
        //        var data = await _lossManagementService.GetChequeBooksDataTableAsync(query);

        //        var chequeBooks = JsonConvert.DeserializeObject<List<Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.ChequeBook>>(JsonConvert.SerializeObject(data.data));

        //        return Json(new
        //        {
        //            draw = data.draw,
        //            recordsTotal = data.recordsTotal,
        //            recordsFiltered = data.recordsFiltered,
        //            data = chequeBooks
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // return a DataTables-compatible empty result on error
        //        return Json(new
        //        {
        //            draw = query?.Options?.draw ?? "1",
        //            recordsTotal = 0,
        //            recordsFiltered = 0,
        //            data = new List<object>(),
        //            error = ex.Message
        //        });
        //    }
        //}



        [HttpPost]
        public async Task<JsonResult> LoadCheckleavesData(CheckLeafQuery query)
        {
            try
            {
                var dataTable = await _lossManagementService.GetChequeleavesDataTableAsync(query);

                var chequeLeaves = JsonConvert.DeserializeObject<List<Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.ChequeLeaf>>(JsonConvert.SerializeObject(dataTable.data));


                return Json(new
                {
                    draw = dataTable.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = chequeLeaves
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.Options?.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }



        [HttpGet]
        public async Task<ActionResult> GetCheckLeavesByCheckbook(string KEY)
        {
            var result = await _lossManagementService.GetCheckLeavesByCheckbook(KEY);
            if (result != null && result.CheckLeaves.Any())
            {
                return PartialView("_CheckLeavesTable", result);
            }

            return PartialView("_NoCheckLeavesFound");
        }

        // This should return a partial view for the modal
        [HttpGet]
        public async Task<ActionResult> RequestLossForCheckbook(string KEY)
        {
            var lossRequestData = await _lossManagementService.RequestLossForCheckbook(KEY);
            if (lossRequestData != null)
            {
                return View("LossRequestForm", lossRequestData);
            }

            return View("_Error", "Unable to load loss request form for the selected checkbook.");
        }

        // This should return a partial view for the modal
        [HttpGet]
        public async Task<ActionResult> RequestLossForCheckLeaf(string KEY)
        {
            var lossRequestData = await _lossManagementService.RequestLossForCheckLeaf(KEY);
            if (lossRequestData != null)
            {
                return View("LossRequestForm", lossRequestData);
            }

            return PartialView("_Error", "Unable to load loss request form for the selected check leaf.");
        }

        // This should return a partial view for the modal with checkbook details AND check leaves
        [HttpGet]
        public async Task<ActionResult> CheckbookDetails(string KEY)
        {
            var checkbookDetails = await _lossManagementService.GetCheckbookDetails(KEY);
            if (checkbookDetails != null)
            {
                return View(checkbookDetails);
            }

            return PartialView("_Error", "Checkbook details not found.");
        }

        [HttpGet]
        public async Task<ActionResult> CheckLeafDetails(string KEY)
        {
            var checkLeafDetails = await _lossManagementService.GetCheckLeafDetails(KEY);
            if (checkLeafDetails != null)
            {
                return PartialView("_CheckLeafDetails", checkLeafDetails);
            }

            return PartialView("_Error", "Check leaf details not found.");
        }



        // Add these to your LossManagementController
        //public async Task<ActionResult> GetBranches()
        //{
        //    var branches = await _lossManagementService.GetBranches();
        //    return Json(branches, JsonRequestBehavior.AllowGet);
        //}

        //public async Task<ActionResult> GetLossReasons()
        //{
        //    var reasons = await _lossManagementService.GetLossReasons();
        //    return Json(reasons, JsonRequestBehavior.AllowGet);
        //}

        //public async Task<ActionResult> GetClientSuggestions()
        //{
        //    var suggestions = await _lossManagementService.GetClientSuggestions();
        //    return Json(suggestions, JsonRequestBehavior.AllowGet);
        //}

        //public async Task<ActionResult> GetReportedByOptions()
        //{
        //    var options = await _lossManagementService.GetReportedByOptions();
        //    return Json(options, JsonRequestBehavior.AllowGet);
        //}

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
    }
}