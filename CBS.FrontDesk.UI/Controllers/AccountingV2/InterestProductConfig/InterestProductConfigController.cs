using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.EndOfYearClosure;
using CBS.BusinessService.AccountingV2.InterestProductConfig;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.BulkOperations;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.InterestProductConfig;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.InterestProductConfig
{
    public class InterestProductConfigController : Controller
    {


        private readonly InterestProductConfigService _interestProductConfigService;
        private readonly BranchServices _branchServices;
        private readonly EndOfYearClosureService _endOfYearClosureService;

        public InterestProductConfigController(BranchServices branchServices, EndOfYearClosureService endOfYearClosureService,InterestProductConfigService interestProductConfigService)
        {
            _interestProductConfigService = interestProductConfigService;
            _branchServices = branchServices;
            _endOfYearClosureService = endOfYearClosureService;

        }
        // GET: InterestProductConfig
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }

        public async Task<ActionResult> New()
        {
            await loader();
            return PartialView("_Update", new ProductCalculationConfig());
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


            ViewBag.Year = getNext5Years();

                    ViewBag.Product = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Regular", Text = "Regular saving Account" },
                        new SelectListItem { Value = "Premium", Text = "Premium savings Account" }
                    };
            ViewBag.Frequency = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Weekly", Text = "Weekly" },
                        new SelectListItem { Value = "Monthly", Text = "Monthly" },
                        new SelectListItem { Value = "Trimestely", Text = "Trimestely (Every 3 months)" },
                        new SelectListItem { Value = "Quaterly", Text = "Quaterly (Q1, Q2, Q3, Q4)" },
                        new SelectListItem { Value = "Semesterly", Text = "Semesterly (Every 6 months)" },
                        new SelectListItem { Value = "Anually", Text = "Anually" }
                     
                    };
            ViewBag.ComputationMode = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "BeginningBalance", Text = "Beginning Balance" },
                        new SelectListItem { Value = "AverageofPoints", Text = "Average of Points" },
                        new SelectListItem { Value = "DailyAverage", Text = "Daily Average" },
                        new SelectListItem { Value = "PSIA(PreferenceShareInterestAverage)", Text = "PSIA (Preference Share Interest Average)" }

                    };
            ViewBag.RateSource = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "FixedValue", Text = "Fixed Value" },
                        new SelectListItem { Value = "YearlyBoardDecision", Text = "Yearly Board Decision" },
                        new SelectListItem { Value = "ProductLevelRate", Text = "Product Level Rate" }

                    };

            ViewBag.ShareMonthMode = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "None", Text = "None" },
                        new SelectListItem { Value = "SimpleBalance", Text = "Simple Balance" },
                        new SelectListItem { Value = "AverageofPoints", Text = "Average of Points" },
                        new SelectListItem { Value = "DailyAverage", Text = "Daily Average" }
                       

                    };



            var products = await _interestProductConfigService.GetProductAsync();

            // PRODUCTS DROPDOWN
            ViewBag.Products = products?
                .Select(p => new SelectListItem
                {
                    Value = p.Id,                       // ProductId posted
                    Text = $" {p.Name}"         // [Code] [Name] shown
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();



            return true;
        }


       



        [HttpPost]

        public async Task<ActionResult> CreateOrUpdate(ProductCalculationConfig model)
        {
           
            if (model == null)
                return Json(new { success = false, message = "Invalid or empty model." });

           

            try
            {
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    // No Id → create new record
                    var execMessage = await _interestProductConfigService.Create(model);

                    if (execMessage == null)
                        return Json(new { success = false, message = "No response from service." });

                    if (!execMessage.Result)
                        return Json(new
                        {
                            success = false,
                            message = execMessage.MessageString ?? "Failed to Interest Product Config.",
                            data = execMessage.Data
                        });

                    return Json(new
                    {
                        success = true,
                        message = execMessage.MessageString  /*"Interest Product Config created successfully."*/,
                        data = execMessage.Data
                    });
                }
                else
                {
                    // Id present → update existing record
                    var result = await _interestProductConfigService.UpdateAsync(model);

                    if (result == null)
                        return Json(new { success = false, message = "No response from service." });

                    return Json(new
                    {
                        success = result.Result,
                        message = Messaging.MessageResult(result),
                        data = result.Data
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "ID is required");

           
            try
            {
                var entry = await _interestProductConfigService.GetinterestProductConfig(id);
                if (entry == null)
                    return HttpNotFound(" not found");


                // Return the partial view that will be injected into the modal
                await loader();
                return PartialView("_Update", entry);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }





        [HttpPost]
        public async Task<JsonResult> LoadInterestProductConfigData(InterestProductConfigQuery query)
        {
            try
            {
                var data = await _interestProductConfigService.GetDataTableAsync(query);

                var interestProducts =
                    JsonConvert.DeserializeObject<List<ProductCalculationConfig>>(
                        JsonConvert.SerializeObject(data.data));

                var products = await _interestProductConfigService.GetProductAsync();
                var productLookup = products.ToDictionary(p => p.Id, p => p.Name);

                foreach (var item in interestProducts)
                {
                    if (!string.IsNullOrEmpty(item.ProductId) &&
                        productLookup.TryGetValue(item.ProductId, out var productName))
                    {
                        item.ProductName = productName;
                    }
                }

                return Json(new
                {
                    //draw = query.Options.draw,
                    //recordsTotal = data.Options.recordsTotal,
                    //recordsFiltered = data.Options.recordsFiltered,
                    data = interestProducts,
                    success = true
                }, JsonRequestBehavior.AllowGet);
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

        
    }
}