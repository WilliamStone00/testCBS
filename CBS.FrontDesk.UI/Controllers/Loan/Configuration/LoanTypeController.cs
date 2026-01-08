using CBS.BusinessService.LoanP;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Loan.Configuration
{
   // [CheckSessionTimeOutAttribute]
    public class LoanTypeController : Controller
    {
        private readonly LoanTypeService _loanTypeService;

        public LoanTypeController(LoanTypeService loanTypeService)
        {
            _loanTypeService = loanTypeService;
        }

        public async Task<ActionResult> Index()
        {
            await LoaderAsync();
            return View();
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,
            string path = null, string serviceOption = null)
        { 
            var model = await LoaderAsync();
            if (path == "details")
            {
                var data = await _loanTypeService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else if (path == "edit")
            {
                var data = await _loanTypeService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else if (path == "delete")
            {
                var data = await _loanTypeService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else // create
            {            
                return PartialView(partialView, model);            
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(LoanType model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false, message = "Enter all values .", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var isCreate = string.IsNullOrWhiteSpace(model.Id);
            var result = isCreate
                ? await _loanTypeService.CreateAsync(model)
                : await _loanTypeService.UpdateAsync(model);

            return Json(new
            {
                success = result.Result,
                message = Messaging.MessageResult(result)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "ID is required." });

            var result = await _loanTypeService.DeleteAsync(id);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpGet]
        public async Task<JsonResult> GetAll()
         {
            try
            {
                var data = await _loanTypeService.GetAllAsync();               

                return Json(new
                {
                    success = true,
                    data = data,
                    count = data?.Count() ?? 0
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    data = new List<LoanType>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "ID is required" },
                    JsonRequestBehavior.AllowGet);
            }

            var data = await _loanTypeService.GetByIdAsync(id);
            return Json(new { success = data != null, data = data }, JsonRequestBehavior.AllowGet);
        }

        private async Task<LoanType> LoaderAsync()
        {
            var data = await _loanTypeService.GetAllAsync();

            // Existing Codes
            var existingCodes = data
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .Select(x => x.Code.Trim())
                .ToHashSet();

            // Available Codes 001–100
            var availableCodes = Enumerable.Range(1, 100)
                .Select(i => i.ToString("D3"))
                .Where(code => !existingCodes.Contains(code))
                .Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                })
                .ToList();

            ViewBag.AvailableLoanTypeCodes = availableCodes;

            // Next DisplayOrder
            int nextDisplayOrder = data.Any()
                ? data.Max(x => x.DisplayOrder) + 1
                : 1;

            // SSF Exists?
            bool isSSFExists = data.Any(x => x.IsSSF);

            return new LoanType
            {
                DisplayOrder = nextDisplayOrder,
                IsSSFexist = isSSFExists
            };
        }

    }
}