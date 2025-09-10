
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

    public class HolidayController : BaseController
    {
        // GET: HoliDay
        private readonly HolyDayServices _services;
        private readonly BranchServices _branchServices;

        public HolidayController(HolyDayServices services, BranchServices branchServices = null)
        {
            _services = services;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetValues();
            return View(new HolyDay());
        }
        [HttpPost]
        public async Task<ActionResult> Create(HolyDay model)
        {
            if (model.IsCentralisedConfiguration)
            {
                // Remove errors related to BranchId if IsGlobal is true
                ModelState.Remove(nameof(model.BranchId));
            }
            // Validate the model state
            if (!ModelState.IsValid)
            {
                // If model validation fails, return validation errors as JSON response
                return Json(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            // If the Id is null, it's a new holiday entry, so call the Create service
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
        public async Task<ActionResult> Update(HolyDay model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        private async Task GetValues()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "list")
            {
                var data = await _services.GetHolyDays();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                await GetValues();
                return PartialView(partialView, new HolyDay());
            }
            else
            {
                await GetValues();

                ViewBag.Key = KEY;
                var HolyDay = await _services.GetHolyDay(KEY);
                return PartialView(partialView, HolyDay);

            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetHolyday(string Key)
        {
            var data = await _services.GetHolyDay(Key);
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