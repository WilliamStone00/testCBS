using CBS.BusinessService.AccountingV2.EndOfYearClosure;
using CBS.BusinessService.AccountingV2.InterestProductConfig;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.AccountingV2.SharedMonth;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.SharedMonth
{
    public class SharedMonthSimulationController : Controller
    {

        private readonly SharedMonthSimulationService _sharedMonthSimulationService;
        private readonly BranchServices _branchServices;
        private readonly InterestProductConfigService _interestProductConfigService;


        public SharedMonthSimulationController(BranchServices branchServices, SharedMonthSimulationService sharedMonthSimulationService, InterestProductConfigService interestProductConfigService)
        {
            _sharedMonthSimulationService = sharedMonthSimulationService;
            _branchServices = branchServices;
            _interestProductConfigService = interestProductConfigService;
           

        }
        // GET: SharedMonthSimulation
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }
        private List<StringValues> getNext5Years()
        {
            return Enumerable.Range(DateTime.Now.Year, 5)
                .Select(y => new StringValues
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                })
                .ToList();
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

                    ViewBag.IntrestDistributionType = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Share Month", Text = "Share Month" },
                        new SelectListItem { Value = "Preference Share", Text = "Preference Share" }
                    };

           

            ViewBag.Period = new List<SelectListItem>
            {
                new SelectListItem { Value = "January", Text = "January" },
                new SelectListItem { Value = "February", Text = "February" },
                new SelectListItem { Value = "March", Text = "March" },
                new SelectListItem { Value = "April", Text = "April" },
                new SelectListItem { Value = "May", Text = "May" },
                new SelectListItem { Value = "June", Text = "June" },
                new SelectListItem { Value = "July", Text = "July" },
                new SelectListItem { Value = "August", Text = "August" },
                new SelectListItem { Value = "September", Text = "September" },
                new SelectListItem { Value = "October", Text = "October" },
                new SelectListItem { Value = "November", Text = "November" },
                new SelectListItem { Value = "December", Text = "December" }
            };

            ViewBag.Year = getNext5Years();



            var products = await _interestProductConfigService.GetProductAsync();

            // PRODUCTS DROPDOWN
            ViewBag.Products = products?
                .Select(p => new SelectListItem
                {
                    Value = p.Id,                       // ProductId posted
                    Text = $"{p.Code} {p.Name}"         // [Code] [Name] shown
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();


            return true;
        }



        


        //[HttpPost]
        //public async Task<ActionResult> LoadSimulationData(SharedMonthSimulation model)
        //{
        //    model.IntrestDistributionType = "Share Month";
        //    try
        //    {
        //        var result = await _sharedMonthSimulationService.GetSimulationData(model);

        //        if (result == null)
        //            return Json(new { success = false });

        //        // ✅ Return structured JSON based on result
        //        return Json(new
        //        {
        //            success = true,
        //            statusCode = 200,
        //            message = (result as dynamic)?.Message ,
        //            data = result
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            statusCode = 500,
        //            message = $"Simulation failed: {ex.Message}"
        //        });
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> LoadSimulationData(SharedMonthSimulation model)
        {
            try
            {
                var result = await _sharedMonthSimulationService.GetSimulationData(model);

                if (result == null || !result.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No simulation data found"
                    });
                }

                return Json(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Simulation completed successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Simulation failed: {ex.Message}"
                });
            }
        }




    }
}