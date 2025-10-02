using CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Web.Mvc;
using ZXing;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Operations.ChequeBookListing
{
    public class ChequeBookController : BaseController
    {
        private readonly ChequeBookService _chequeBookService;
        private readonly ChequeBookMockService _chequeBookMockService;
        private readonly BranchServices _branchServices;

        public ChequeBookController(ChequeBookService chequeBookService,
                                  ChequeBookMockService chequeBookMockService,
                                  BranchServices branchServices)
        {
            _chequeBookService = chequeBookService;
            _chequeBookMockService = chequeBookMockService;
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
                new { Value = "Active", Text = "Active" },
                new { Value = "Used", Text = "Used" },
                new { Value = "Cancelled", Text = "Cancelled" },
                new { Value = "Blocked", Text = "Blocked" }
            };
        }


        [HttpPost]
        public async Task<JsonResult> GetChequeBooks(ChequeBookQuery query)
        {
            try
            {
                // Try main service first
                var data = await _chequeBookService.GetChequeBooksDataTableAsync(query);
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
                return Json(new { success = false, message = ex.Message });
                // Fall back to mock service
                //try
                //{
                /*    var mockData = await _chequeBookMockService.GetChequeBooksDataTableAsync(query);*/
                /*     return Json(new
                     {
                         draw = query.DataTableOptions.draw,
                         recordsTotal = mockData.recordsTotal,
                         recordsFiltered = mockData.recordsFiltered,
                         data = mockData.data
                     });
                 }
                 catch (Exception mockEx)
                 {
                     return Json(new
                     {
                         draw = query.DataTableOptions.draw,
                         recordsTotal = 0,
                         recordsFiltered = 0,
                         data = new List<object>(),
                         error = "Failed to load cheque books data"
                     });
                 }*/
            }
        }

        // using Microsoft.AspNetCore.Mvc;

       
        [HttpGet]
        public async Task<ActionResult> List()
        {
            await LoadViewBagData();
            return View();
        }

        // Note: use [FromBody] so model binder reads the JSON DataTables sends.
        [HttpPost]
        public async Task<JsonResult> LoadChequeBooksData(ChequeBookQuery query)
        {
            try
            {
                var data = await _chequeBookService.GetChequeBooksDataTableAsync(query);

                var chequeBooks = JsonConvert.DeserializeObject<List<Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.ChequeBook>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = chequeBooks
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

            // Example Download endpoint (GET) receives querystring params for export
            //[HttpGet]
            //public async Task<IActionResult> DownloadChequeBooks(ChequeBookQuery query)
            //{
            //    // implement export using the query (server will bind from query string)
            //    var fileBytes = await _chequeBookService.GenerateChequeBookExportAsync(query);
            //    return File(fileBytes, "application/octet-stream", "chequebooks.csv");
            //}
        


        public async Task<ActionResult> GetChequeBookDetails(string id)
        {
            try
            {
                // Try main service first
                var chequeBook = await _chequeBookService.GetChequeBookByIdAsync(id);
                if (chequeBook == null)
                {
                    // Fall back to mock service
                    chequeBook = await _chequeBookMockService.GetChequeBookByIdAsync(id);
                }

                if (chequeBook == null)
                {
                    return HttpNotFound($"Cheque book with ID {id} not found");
                }

                return PartialView("_ChequeBookDetails", chequeBook);
            }
            catch (Exception ex)
            {
                // Final fallback to mock service
                var chequeBook = await _chequeBookMockService.GetChequeBookByIdAsync(id);
                if (chequeBook == null)
                {
                    return HttpNotFound($"Cheque book with ID {id} not found");
                }

                return PartialView("_ChequeBookDetails", chequeBook);
            }
        }

        public async Task<ActionResult> GetLeafDetails(string leafId)
        {
            try
            {
                // Extract cheque book ID from leaf ID
                var chequeBookId = leafId.Split('-')[0];
                ChequeBook chequeBook;

                // Try main service first
                try
                {
                    chequeBook = await _chequeBookService.GetChequeBookByIdAsync(chequeBookId);
                    if (chequeBook == null)
                    {
                        chequeBook = await _chequeBookMockService.GetChequeBookByIdAsync(chequeBookId);
                    }
                }
                catch
                {
                    chequeBook = await _chequeBookMockService.GetChequeBookByIdAsync(chequeBookId);
                }

                if (chequeBook == null)
                {
                    return HttpNotFound("Cheque book not found");
                }

                var leaf = chequeBook.ChequeLeaves.Find(l => l.id == leafId);
                if (leaf == null)
                {
                    return HttpNotFound("Leaf not found");
                }

                ViewBag.ChequeBook = chequeBook;
                return PartialView("_LeafDetails", leaf);
            }
            catch (Exception ex)
            {
                return HttpNotFound("Error loading leaf details");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CancelChequeBook(string chequeBookId, string cancellationReason)
        {
            try
            {
                ExecutionMessages result;

                // Try main service first
                try
                {
                    result = await _chequeBookService.CancelChequeBookAsync(chequeBookId, cancellationReason);
                }
                catch
                {
                    // Fall back to mock service
                    result = await _chequeBookMockService.CancelChequeBookAsync(chequeBookId, cancellationReason);
                }

                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while cancelling the cheque book" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarkLeafAsUsed(string leafId, string statement)
        {
            try
            {
                ExecutionMessages result;

                // Try main service first
                try
                {
                    result = await _chequeBookService.MarkLeafAsUsedAsync(leafId, statement);
                }
                catch
                {
                    // Fall back to mock service
                    result = await _chequeBookMockService.MarkLeafAsUsedAsync(leafId, statement);
                }

                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while marking leaf as used" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> BlockLeaf(string leafId, string blockReason)
        {
            try
            {
                ExecutionMessages result;

                // Try main service first
                try
                {
                    result = await _chequeBookService.BlockLeafAsync(leafId, blockReason);
                }
                catch
                {
                    // Fall back to mock service
                    result = await _chequeBookMockService.BlockLeafAsync(leafId, blockReason);
                }

                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while blocking the leaf" });
            }
        }

        public ActionResult CancelChequeBookForm(string chequeBookId)
        {
            ViewBag.ChequeBookId = chequeBookId;
            return PartialView("_CancelChequeBookForm");
        }

        public ActionResult MarkLeafUsedForm(string leafId)
        {
            ViewBag.LeafId = leafId;
            return PartialView("_MarkLeafUsedForm");
        }

        public ActionResult BlockLeafForm(string leafId)
        {
            ViewBag.LeafId = leafId;
            return PartialView("_BlockLeafForm");
        }
    }
}