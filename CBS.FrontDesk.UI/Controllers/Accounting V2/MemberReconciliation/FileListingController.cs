using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.MemberReconciliation;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.PendingAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.Affiliate;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation.FileUploadData;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.MemberReconciliation
{
    public class FileListingController : Controller
    {
        private readonly FileListingService _fileListingService;
        private readonly BranchServices _branchServices;
        public FileListingController(FileListingService fileListingService, BranchServices branchServices)
        {
            _fileListingService = fileListingService;
            _branchServices = branchServices;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();

        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            return true;
        }

        [HttpPost]
        public async Task<JsonResult> LoadData(FileListingQuery query)
        {
            //await loader();
            try
            {

                if (!_fileListingService.IsHeadOffice())
                {
                    query.BranchId = _fileListingService.GetBranchID();
                }

                var data = await _fileListingService.DataTableAsync(query);

                var result = JsonConvert.DeserializeObject<List<FileListing>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = result
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
        [HttpGet]
        public async Task<ActionResult> GLhistory()
        {
            await loader();
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LoadData2(GLHistoryListingQuery query)
        {
            //await loader();
            try
            {
                var data = await _fileListingService.DataTableAsync2(query);
                var result = JsonConvert.DeserializeObject<List<GLHistoryDto>>(JsonConvert.SerializeObject(data.data));
                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = result
                });
            }
            catch (Exception ex)
            {
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

        //public async Task<ActionResult> Details(string id, string partialView = null)
        //{
        //    var data = await _fileListingService.GetByIdAsync(id);
        //    return PartialView(partialView, data);

        //}

        public async Task<ActionResult> Details(string id, string partialView = null)
        {
            var data = await _fileListingService.GetByIdAsync(id);
            if (string.IsNullOrEmpty(partialView))
            {
               return View("Detailss", data); 
            }
           return PartialView(partialView, data);
        }

        public async Task<ActionResult> DetailsGLH(string id, string partialView = null)
        {
            var data = await _fileListingService.GetByIdAsync(id);
            return PartialView(partialView, data);

        }

        [HttpPost]
        public async Task<ActionResult> ConfirmUpload(ConfirmReconciliation modal)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _fileListingService.ConfirmReconciliation(modal);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmUpload(string id, string branch)
        {
            var data = new ConfirmReconciliation
            {
               Id = id,
               BranchId = branch
            };
            return PartialView("_Validation", data);

        }

    }
}