using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.AccountingV2.ReconciledLine;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReconciledLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.ReconciledLine
{
    public class ReconciledLineController : Controller
    {
        private readonly ReconciledLineService _reconciledLineService;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;
        public ReconciledLineController(ReconciledLineService reconciledLineService, BranchServices branchServices, BranchAccountService branchAccountService)
        {
            _reconciledLineService = reconciledLineService;
            _branchServices = branchServices;
            _branchAccountService = branchAccountService;
        }
        // GET: ReconciledLine
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }


        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            var CounterBranches = await _branchServices.GetCounterpartyBranches();
            ViewBag.CounterBranches = CounterBranches;

            return true;
        }


        [HttpPost]
        public async Task<JsonResult> LoadDataTable(ReconciledQuery query)
        {
            try
            {
                var data = await _reconciledLineService.GetReconciledLineDataTableAsync(query);

                // Deserialize DataTable payload into strongly-typed list
                var ReconciledLine = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.ReconciledLine.Reconciled>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {

                    draw = data.Options.draw ?? "1",
                    recordsTotal = data.Options.recordsTotal,
                    recordsFiltered = data.Options.recordsFiltered,
                    data = ReconciledLine,
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return DataTables-compatible empty result on error
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

        [HttpGet]
        public async Task<ActionResult> GetBranchAccounts(string branchId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(branchId))
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                var branchAccounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);
                var result = _branchAccountService.DropDownGen(branchAccounts.ToList());


                // var result = await _manualJournalEntryService.GetAccountsByBranchAsync(branchId);

                if (result == null || !result.Any())
                    return Json(new { success = false  }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (TaskCanceledException)
            {
                return Json(new { success = false  }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}