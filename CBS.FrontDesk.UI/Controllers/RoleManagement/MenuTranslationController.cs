using CBS.BusinessService;
using CBS.BusinessService.Accounting;

using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MenuTranslationP;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.RoleManagement
{
    [CheckSessionTimeOutAttribute]

    public class MenuTranslationController : BaseController
    {
        // GET: Role
        private readonly MenuTranslationServices _services;
        public MenuTranslationController(MenuTranslationServices services)
        {
            _services = services;
        }

        public ActionResult Index()
        {
            ViewBag.Languages = LanguageHelper.GetLanguages(); 
            return View();
        }
        public ActionResult Update()
        {
            ViewBag.Languages = LanguageHelper.GetLanguages();
            return View();
        }

        public ActionResult TranslatedMenus()
        {

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SubmitTranslations(List<MenuTranslation> model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> UpdateTranslations(List<MenuTranslation> model)
        {
            if (ModelState.IsValid)
            {
                var data = await _services.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        public async Task<ActionResult> Untranslated(string lang)
        {
            var data = await _services.GetAllUnTransalatedMenus(lang);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> UpdateMenuTranslation(string lang)
        {
            var data = await _services.GetAllTranslationsAsync(lang);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}