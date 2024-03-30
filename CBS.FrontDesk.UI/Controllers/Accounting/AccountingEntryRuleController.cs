using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using CBS.BusinessService.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting;
using System.Web.Services.Description;
using CBS.BusinessService;
using CBS.BusinessService.Accounts;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class AccountingEntryRuleController : BaseController
    {
        
        private readonly AccountingEntryRuleService _Service;
        private readonly AccountingServices _accountService;
        private readonly OperationEventAttributeServices _OperationEventAttributeService;
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountingRuleService _ServiceRule;
        private readonly OperationEventServices _OperationEventService;
        public AccountingEntryRuleController(AccountingServices accountService, OperationEventServices eventServices, AccountingRuleService servicesRule,ChartOfAccountServices chartOfAccountServices,AccountingEntryRuleService services, OperationEventAttributeServices OperationEventAttributeService)
        {
            _accountService = accountService;
            _ServiceRule = servicesRule;
            _Service = services;
            _OperationEventService = eventServices;
            _OperationEventAttributeService = OperationEventAttributeService;
            _chartOfAccountServices = chartOfAccountServices;
        }
        // GET: AccountingEntryRule
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> LoadComboboxOfOperationEventAttributeByEventID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var listOfOperation = await _OperationEventAttributeService.GetOperationEventAttributes();

                SelectList data = new SelectList(_OperationEventAttributeService.ConvertToSelectedList( listOfOperation.ToList()), "Text", "Value", 0);

                return Json(data, JsonRequestBehavior.AllowGet);
                
            }
            else
            {
                return Json(new { success = "", status = "notOk" });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Create(AccountingRuleEntry model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Service.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(AccountingRuleEntry model)
        {
            if (ModelState.IsValid)
            {
                var data = await _Service.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
           // await GetList();
            if (path == "list")
            {
            
                var  OperationEventList = await _OperationEventService.GetOperationEvents();
                var OperationEventAttributes = await _OperationEventAttributeService.GetOperationEventAttributes();
                var DebitAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
                var dataList = await _Service.GetAccountingEntryRules();
                var data = await _Service.GetAccountingEntryRulesDto(dataList, OperationEventList, OperationEventAttributes, DebitAccounts);
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new AccountingRuleEntry());
            }
            else
            {
                await GetList();
                var OperationEventAttribute = await _Service.GetAccountingRuleEntryById(KEY);
                return PartialView(partialView, OperationEventAttribute);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _Service.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task GetList(string language="En")
        {
             ViewBag.OperationEvent = await _OperationEventService.GetOperationEvents();
            ViewBag.BookingDirections = await _ServiceRule.GetBookingDirections();
            ViewBag.OperationEventAttributes = await _OperationEventAttributeService.GetOperationEventAttributes();
            var DebitAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
            var CreditAccounts =  BuildMenuViewBag (DebitAccounts, language);
           ViewBag.CreditAccounts = CreditAccounts;
           ViewBag.DebitAccounts = CreditAccounts;
        }

        private dynamic BuildMenuViewBag(IEnumerable<ChartOfAccount> debitAccounts, string language = "En")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in debitAccounts)
            {
                if (language == "EN")
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.LabelEn });
                else
                {
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.LabelFr });

                }
            }

            return list;
        }

        //private List<SelectListItem> BuildMenuViewBag(List<ChartOfAccount>  ListOfChartOfAccounts)
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    foreach (var item in ListOfChartOfAccounts)
        //    {
        //        list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber+"-"+item.Label });
        //    }

        //    return list;
        //}
    }
}