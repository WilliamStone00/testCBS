
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounting;

using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.BusinessService;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class AccountingRuleController : BaseController
    {
        // GET: OperationEventAttribute
        private readonly AccountingRuleService _Services;
        private readonly OperationEventServices _operationEventServices;

        public AccountingRuleController(AccountingRuleService services, OperationEventServices opServices)
        {
            _Services = services;
            _operationEventServices = opServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(AccountingRule model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(AccountingRule model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Services.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpGet]
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            GetList();
            if (path == "list")
            {
             
                var AccountingRules = await _Services.GetAccountingRules();

                var events = await _operationEventServices.GetOperationEvents();
                var data = ConvertToDtos(AccountingRules.ToList(), events.ToList());
                return PartialView(partialView, data);
            }
            //else if (path == "LoadAccountingRules")
            //{
               
            //}

            else if (path == "new")
            {
                return PartialView(partialView, new AccountingRule());
            }
            else
            {
                var OperationEventAttribute = await _Services.GetAccountingRuleById(KEY);
                return PartialView(partialView, OperationEventAttribute);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _Services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public List<AccountingRuleDto> ConvertToDtos(List<AccountingRule> attributes, List<OperationEvent> events)
        {
            return (from a in attributes
                join e in events on a.OperationEventId equals e.Id
                select new AccountingRuleDto
                {
                    Id = a.Id,
                    RuleName = a.RuleName,
                    OperationName = e.OperationEventName
                }).ToList();
        }
        public async Task GetList()
        {
           
             var Branches = await _operationEventServices.GetOperationEvents();
             ViewBag.OperationEvents = Branches;
          
        }
    }
}