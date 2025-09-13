using CBS.BusinessService.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    [CheckSessionTimeOutAttribute]
    public class AccountCategoryController : BaseController
    {
        // GET: AccountCategory

        private readonly AccountCategoryServices _Services;
        public AccountCategoryController(AccountCategoryServices services)
        {
            _Services = services;
        }

        public async Task<ActionResult> Index()
        {
            GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(AccountCategory model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(AccountCategory model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Services.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            GetList();
            if (path == "list")
            {
                var data = await _Services.GetAccountCategory();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new AccountCategory());
            }
            else
            {
                var OperationEventAttribute = await _Services.GetAccountCategory(KEY);
                return PartialView(partialView, OperationEventAttribute);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _Services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public void GetList()
        {

            //ViewBag.Branches = _Services.GetAllBranchesByBankId(_Services.BankId);

        }
    }
}