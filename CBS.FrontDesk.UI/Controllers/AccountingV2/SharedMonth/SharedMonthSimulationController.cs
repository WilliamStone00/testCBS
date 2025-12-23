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
        private List<StringValues> getAccountingYears()
        {
            int currentYear = DateTime.Now.Year;

            return Enumerable.Range(currentYear - 3, 5)
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
                        new SelectListItem { Value = "OrdinaryShare", Text = " Ordinary Share" },
                        new SelectListItem { Value = "PreferenceShare", Text = "Preference Share" }
                    };

           

            ViewBag.Month = new List<SelectListItem>
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
                new SelectListItem { Value = "December", Text = "December" },
                 new SelectListItem { Value = "Anaully", Text = "Anaully" }
            };

            ViewBag.Year = getAccountingYears();



            

            return true;
        }




        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }



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

        [HttpPost]
        public async Task<ActionResult> SaveSimulation(CreateSimulations model)
        {
            if (model == null || model.ShareMonthPsiReportLineDto == null || !model.ShareMonthPsiReportLineDto.Any())
            {
                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = "Simulation data is required"
                });
            }

            var Branch = await _branchServices.GetBranch(model.BranchId);
            model.BranchName = Branch?.Name ?? "—";

            try
            {
                var result = await _sharedMonthSimulationService.CreateShareMonthSimulationAsync(model);

                if (result == null)
                {
                    return Json(new
                    {
                        success = false,
                        statusCode = 500,
                        message = "No response from simulation service."
                    });
                }

                if (result.IsSuccess && result.ApiResponseData?.Data != null)
                {
                    return Json(new
                    {
                        success = true,
                        statusCode = 200,
                        message = result.ApiResponseData.Data.Description
                                  ?? "Simulation saved successfully",
                        data = result.ApiResponseData.Data
                    });
                }

                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message ?? "Simulation save failed",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Simulation save failed: {ex.Message}"
                });
            }
        }



    }
}