using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.CashReconciliation;
using CBS.BusinessService.AccountingV2.ConfigurationsManualEntry;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.AccountingYear
{
    public class AccountingYearController : Controller
    {

        public readonly AccountingYearService _accountingYearService;
        private readonly BranchServices _branchServices;
        public AccountingYearController(BranchServices branchServices, AccountingYearService accountingYearService)
        {
            _accountingYearService = accountingYearService;
            _branchServices = branchServices;

        }
        // GET: AccountingYear
        public async Task<ActionResult> Index()
        {
            await loader();
            return View(); //**//*
        }



        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            ViewBag.Status = new List<SelectListItem>
{
    new SelectListItem { Value = "Open", Text = "OPEN" },
    new SelectListItem { Value = "Close", Text = "CLOSE" },
    new SelectListItem { Value = "Lock", Text = "LOCK" }
};


            // Reconciliation Status (WorkTicket) dropdown
            ViewBag.Year = Enumerable.Range(2025, 10)
                  .Select(y => new SelectListItem
                  {
                      Value = y.ToString(),
                      Text = y.ToString()
                  })
                  .ToList();
            return true;

        }




        [HttpGet]
        public async Task<ActionResult> New()
        {
            await loader();
            return PartialView("_Update", new accountingyear());
        }


        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                //var entries = await _accountingYearService.GetAllAsync();
                var entries = await _accountingYearService.GetAllAsyncs();
                if (entries == null || !entries.Any())
                    return HttpNotFound("Not found");

                // Return a view or partial view depending on your page setup
                return PartialView("_Datatable", entries);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }

        }

        [HttpGet]
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "ID is required");

            try
            {
                var entry = await _accountingYearService.GetDatas(id);
                if (entry == null)
                    return HttpNotFound("accounting year not found");

                await loader();
                // Return the partial view that will be injected into the modal
                return PartialView("_Update", entry);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }



        [HttpPost]
        public async Task<ActionResult> CreatAccountingYear(accountingyear model)
        {
            if (model == null)
                return Json(new { success = false, message = " Invalid or empty model." });

            try
            {
                var execMessage = await _accountingYearService.Create(model);

                if (execMessage == null)
                    return Json(new { success = false, message = "No response from service." });

                if (!execMessage.Result)
                    return Json(new
                    {
                        success = false,
                        message = execMessage.MessageString ?? " Failed to save Accounting year.",
                        data = execMessage.Data
                    });

                return Json(new
                {
                    success = true,
                    message = execMessage.MessageString ?? "Accounting year saved successfully.",
                    data = execMessage.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }



        }

        [HttpPost]
        public async Task<ActionResult> Update(accountingyear model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var result = await _accountingYearService.UpdateAccoutingYearAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

    }
}