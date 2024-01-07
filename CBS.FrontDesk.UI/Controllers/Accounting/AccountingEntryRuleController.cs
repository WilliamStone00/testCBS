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

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class AccountingEntryRuleController : BaseController
    {
        private readonly AccountingEntryRuleService _Service;
        private readonly OperationEventAttributeServices _OperationEventService;
        private readonly ChartOfAccountServices _chartOfAccountServices;
        public AccountingEntryRuleController(ChartOfAccountServices chartOfAccountServices,AccountingEntryRuleService services, OperationEventAttributeServices OperationEventService)
        {
            _Service = services;
            _OperationEventService = OperationEventService;
            _chartOfAccountServices = chartOfAccountServices;
        }
        // GET: AccountingEntryRule
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View();
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
            await GetList();
            if (path == "list")
            {
                var data = await _Service.GetAccountingEntryRules();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                return PartialView(partialView, new AccountingRule());
            }
            else
            {
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
             ViewBag.AccountingRules=await _Service.GetAccountingEntryRules();
             ViewBag.BookingDirections= new SelectListItem[] { new SelectListItem { Text = "Debit", Value = "Debit" }, new SelectListItem { Text = "Credit", Value = "Credit" } };
             ViewBag.OperationEventAttributes = await _OperationEventService.GetOperationEventAttributes();
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