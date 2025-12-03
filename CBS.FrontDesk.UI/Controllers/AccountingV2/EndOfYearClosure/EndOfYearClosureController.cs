using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.EndOfYearClosure;
using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.EndOfYearClosure
{
    public class EndOfYearClosureController : Controller
    {


        private readonly BranchServices _branchServices;
        private readonly EndOfYearClosureService _endOfYearClosureService;
        


        public EndOfYearClosureController(BranchServices branchServices, EndOfYearClosureService endOfYearClosureService)
        {

            _branchServices = branchServices;
            _endOfYearClosureService = endOfYearClosureService;
            

        }
        // GET: EndOfYearClosure
        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new EndOfYear());
        }


        public async Task<bool> loader()
        {

            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;


            var CounterBranches = await _branchServices.GetBranches();
            ViewBag.CounterBranches = CounterBranches;

            ViewBag.AdjustmentType = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Depreciation", Text = "Depreciation" },
                    new SelectListItem { Value = "Provision(Bad Debts)", Text = "Provision(Bad Debts)" },
                    new SelectListItem { Value = "Accrual / Deferral", Text = "Accrual / Deferral" },
                    new SelectListItem { Value = "Tax Provision", Text ="Tax Provision" },
                    new SelectListItem { Value = "Error Correction", Text = "Error Correction" },
                    new SelectListItem { Value = "Other", Text = "Other" }
                };


            return true;
        }
        [HttpGet]
        public async Task<ActionResult> GetClosureOpenYearByBranchId(string branchId)
        {
            try
            {
                var years = await _endOfYearClosureService.GetAccountingYearByBranchIdAsync(branchId);

                var result = years.Select(x => new
                {
                    id = x.Id,
                    year = x.Year
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }




        [HttpPost]
        public async Task<ActionResult> InitiateClosure(EndOfYear model)
        {
          

            try
            {
                var result = await _endOfYearClosureService.SaveInitiateClosure(model);

                if (result == null)
                    return Json(new { success = false, message = "No response from Close Of Year service." });



                // Return summary as JSON
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $" Close of Year failed: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ReviewClosure(EndOfYear model)
        {
            try
            {
                var result = await _endOfYearClosureService.SaveReviewClosure(model);

                if (result == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No response from Review Closure service."
                    });
                }

                return Json(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Review of year closure failed: {ex.Message}"
                });
            }
        }





        [HttpGet]
        public async Task<ActionResult> GetAllEndTask()
        {
            try
            {
                var entries = await _endOfYearClosureService.GetAllEndTaskAsync();

                if (entries == null || !entries.Any())
                    return HttpNotFound("Not found");

                var model = new EndOfYearTaskViewModel
                {
                    Tasks = entries
                };

                return PartialView("_Review", model);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }



    }
}