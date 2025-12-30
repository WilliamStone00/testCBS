using CBS.BusinessService.LoanP.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Loan.Configuration
{
    public class LoanTargetCatalogController : Controller
    {

            private readonly LoanTargetCatalogService _loanTargetCatalogService;

            public LoanTargetCatalogController(LoanTargetCatalogService loanTargetCatalogService)
            {
                _loanTargetCatalogService = loanTargetCatalogService;
            }

            public ActionResult Index()
            {
                loader();
                return View();
            }
                   

            public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,
                string path = null, string serviceOption = null)
            {
                 loader();
                if (path == "details")
                {
                    var data = await _loanTargetCatalogService.GetByIdAsync(KEY);
                    return PartialView(partialView, data);
                }
                else if (path == "edit")
                {
                    var data = await _loanTargetCatalogService.GetByIdAsync(KEY);                   
                    return PartialView(partialView, new UpdateLoanTargetCatalogRequest());
                }
                else if (path == "delete")
                {
                    var data = await _loanTargetCatalogService.GetByIdAsync(KEY);
                    return PartialView(partialView, data);
                }
                else // create
                {
                    return PartialView(partialView, new UpdateLoanTargetCatalogRequest());
                }
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(UpdateLoanTargetCatalogRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            // Determine whether this request is for creating a new Loan Target
            // If Id is null, empty, or contains only whitespace, it means the record does not exist yet
            var isCreate = string.IsNullOrWhiteSpace(model.Id);

            // Based on the operation type:
            // - If creating, call the CreateAsync service method
            // - If updating, call the UpdateAsync service method
            // The ternary operator helps avoid duplicated if/else logic
            var result = isCreate
                ? await _loanTargetCatalogService.CreateAsync(model)
                : await _loanTargetCatalogService.UpdateAsync(model);


            return Json(new
            {
                success = result.Result,
                message = Messaging.MessageResult(result)
            });
        }


        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<ActionResult> Update(UpdateLoanTargetCatalogRequest model)
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

                var result = await _loanTargetCatalogService.UpdateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }

            [HttpGet]
             public async Task<ActionResult> Delete(string KEY)
            {
                if (string.IsNullOrEmpty(KEY))
                    return Json(new { success = false, message = "ID is required." });

                var result = await _loanTargetCatalogService.DeleteAsync(KEY);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }

            [HttpGet]
            public async Task<JsonResult> GetAll()
              {
                var data = await _loanTargetCatalogService.GetAllAsync();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
              }

        [HttpGet]
        public async Task<JsonResult> GetById(string id)
        {
            loader();
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "ID is required" },
                    JsonRequestBehavior.AllowGet);
            }

            var data = await _loanTargetCatalogService.GetByIdAsync(id);
            return Json(new { success = data != null, data = data }, JsonRequestBehavior.AllowGet);
        }
        private bool loader()
        {
            // Load LoanTermKind enum values
            var population = Enum.GetValues(typeof(PcmfPopulation))
                .Cast<PcmfPopulation>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                })
                .ToList();
            ViewBag.population = population;
            return true;
        }
    }
}

