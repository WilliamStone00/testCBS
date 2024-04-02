using CBS.BusinessService.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Message;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity;

namespace CBS.FrontDesk.UI.Controllers.Configuration.Localization
{
    //[SessionTimeoutFilterAttribute]
    public class CountryController : BaseController
    {
        // GET: Country
        private readonly CountryServices _CountryServices;

        public CountryController(CountryServices countryServices)
        {
            _CountryServices= countryServices;
        }
       
        public async Task<ActionResult> Index()
        {
            var model = new Country();
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(Country model)
        {
            if (ModelState.IsValid)
            {
                var data = await _CountryServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(Country model)
        {
            if (ModelState.IsValid)
            {
                var data = await _CountryServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
       
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,string path=null)
        {
            ViewBag.KEY = KEY;
            if (path== "list")
            {
                var data = await _CountryServices.GetCountries();
                return PartialView(partialView, data);
                
            }
            else if (path == "new")
            {
                return PartialView(partialView, new Country());
            }
            else
            {
                var country = await _CountryServices.GetCountry(KEY);
                return PartialView(partialView, country);
            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _CountryServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}