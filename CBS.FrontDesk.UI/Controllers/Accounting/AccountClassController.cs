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
    public class AccountClassController : BaseController
    {
        // GET: AccountClass

        private readonly AccountClassServices _Services;
        private readonly AccountCategoryServices catServices;

        public AccountClassController(AccountClassServices services, AccountCategoryServices Opservices)
        {
            _Services = services;
            catServices = Opservices;
        }

        public async Task<ActionResult> Index()
        {
           await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(AccountClass model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(AccountClass model)
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
                var attributes = await _Services.GetAccountClass();

                var events = await catServices.GetAccountCategory();
                var data = ConvertToDtos(attributes.ToList(), events.ToList());
                return PartialView(partialView, attributes);
          
            }
            else if (path == "new")
            {
                return PartialView(partialView, new AccountClass());
            }
            else
            {
                var OperationEventAttribute = await _Services.GetAccountClass(KEY);
                return PartialView(partialView, OperationEventAttribute);

            }
        }
        public List<AccountClassDto> ConvertToDtos(List<AccountClass> attributes, List<AccountCategory> events)
        {
            return (from a in attributes
                join e in events on a.AccountCategoryId equals e.Id
                select new AccountClassDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    AccountCategory = e.Name
                }).ToList();
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _Services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task GetList()
        {

            ViewBag.AccountCartegories = await catServices.GetAccountCategory();

        }
    }
  
}