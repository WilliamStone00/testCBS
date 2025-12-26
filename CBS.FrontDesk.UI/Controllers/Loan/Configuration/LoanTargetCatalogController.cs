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
                return View();
            }

            public ActionResult List()
            {
                return View();
            }

            

            public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,
                string path = null, string serviceOption = null)
            {
                if (path == "details")
                {
                    var data = await _loanTargetCatalogService.GetByIdAsync(KEY);
                    return PartialView(partialView, data);
                }
                else if (path == "edit")
                {
                    var data = await _loanTargetCatalogService.GetByIdAsync(KEY);
                    if (data != null)
                    {
                        var updateModel = new UpdateLoanTargetCatalogRequest
                        {
                            Id = data.Id,
                            NameEn = data.NameEn,
                            NameFr = data.NameFr,
                            PcmfPopulation = data.PcmfPopulation,
                            IsActive = data.IsActive
                        };
                        return PartialView(partialView, updateModel);
                    }
                    return PartialView(partialView, new UpdateLoanTargetCatalogRequest());
                }
                else if (path == "delete")
                {
                    var data = await _loanTargetCatalogService.GetByIdAsync(KEY);
                    return PartialView(partialView, data);
                }
                else // create
                {
                    return PartialView(partialView, new CreateLoanTargetCatalogRequest());
                }
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<ActionResult> Create(CreateLoanTargetCatalogRequest model)
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

                var result = await _loanTargetCatalogService.CreateAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
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

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<ActionResult> Delete(string id)
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "ID is required." });

                var result = await _loanTargetCatalogService.DeleteAsync(id);
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
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new { success = false, message = "ID is required" },
                        JsonRequestBehavior.AllowGet);
                }

                var data = await _loanTargetCatalogService.GetByIdAsync(id);
                return Json(new { success = data != null, data = data }, JsonRequestBehavior.AllowGet);
            }
        }
    }

