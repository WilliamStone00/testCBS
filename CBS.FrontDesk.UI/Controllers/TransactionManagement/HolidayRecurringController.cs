
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    //[CheckSessionTimeOutAttribute]

    public class HolidayRecurringController : BaseController
    {
        // GET: HolidayRecurring
        private readonly HolyDayRecurringServices _services;
        private readonly SavingProductServices _savingProductServices;
        private readonly BranchServices _branchServices;

        public HolidayRecurringController(HolyDayRecurringServices services, SavingProductServices feeServices = null, BranchServices branchServices = null)
        {
            _services = services;
            _savingProductServices = feeServices;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetValues();
            return View(new HolyDayRecurring());
        }
        [HttpPost]
        public async Task<ActionResult> Create(HolyDayRecurring model)
        {
            // Perform conditional validation
            if (model.ExludeDayAndMonth)
            {
                // Remove errors related to Month and DayOfMonth if ExcludeDayAndMonth is true
                ModelState.Remove(nameof(model.Month));
                ModelState.Remove(nameof(model.DayOfMonth));
            }

            if (model.IsGlobal)
            {
                // Remove errors related to BranchId if IsGlobal is true
                ModelState.Remove(nameof(model.BranchId));
            }

            // Validate the model state after adjustments
            if (!ModelState.IsValid)
            {
                // Concatenate all validation messages into a single string
                var validationMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                // Return the validation messages as a single string in the JSON response
                return Json(new { success = false, message = $"Validation failed: {validationMessage}" });
            }

            // If the Id is null, it's a new HolidayRecurring entry, so call the Create service
            if (model.Id == null)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                // If the Id is not null, it's an update, so call the Update method
                return await Update(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Update(HolyDayRecurring model)
        {
            if (model.ExludeDayAndMonth)
            {
                // Remove errors related to Month and DayOfMonth if ExcludeDayAndMonth is true
                ModelState.Remove(nameof(model.Month));
                ModelState.Remove(nameof(model.DayOfMonth));
            }

            if (model.IsGlobal)
            {
                // Remove errors related to BranchId if IsGlobal is true
                ModelState.Remove(nameof(model.BranchId));
            }

            // Validate the model state after adjustments
            if (!ModelState.IsValid)
            {
                // Concatenate all validation messages into a single string
                var validationMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                // Return the validation messages as a single string in the JSON response
                return Json(new { success = false, message = $"Validation failed: {validationMessage}" });
            }
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        private async Task GetValues()
        {
            var branches = await _branchServices.GetBranches();
            
            var conf = await _savingProductServices.GetSavingConfigurationAggregates();
            ViewBag.DayOfWeeks = conf.DayOfWeeks;
            ViewBag.RecurrencePatterns = conf.RecurrencePatterns;
            ViewBag.HolidayTypes = conf.HolidayTypes;
            ViewBag.Branches = branches;
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetHolyDayRecurrings();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                await GetValues();
                return PartialView(partialView, new HolyDayRecurring());
            }
            else
            {
                await GetValues();

                ViewBag.Key = KEY;
                var HolyDayRecurring = await _services.GetHolyDayRecurring(KEY);
                return PartialView(partialView, HolyDayRecurring);

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetHolyDayRecurring(string Key)
        {
            var data = await _services.GetHolyDayRecurring(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key != null)
            {
                var listing = await _branchServices.GetBranchesByBankId(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }

}