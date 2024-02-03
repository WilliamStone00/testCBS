using CBS.BusinessService.Accounting;
using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class CashFlowManagementController : BaseController
    {
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;

        public CashFlowManagementController()
        {
            _chartOfAccountServices = new ChartOfAccountServices();
            _accountingEntryServices = new AccountingEntryServices();
        }
        // GET: BankingOperation
        public async Task<ActionResult> Index()

        {
            await GetList();
            return View(new CashFlowManagement());
        }

        public async Task GetList(string language = "En")
        {

            ViewBag.BookingDirections = await _chartOfAccountServices.GetBookingDirections();
            var DebitAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
            var CreditAccounts = BuildMenuViewBag(DebitAccounts, language);
            ViewBag.Accounts = CreditAccounts;
            ViewBag.CurrencyCodes = BuildMenuViewBagCurrency( _accountingEntryServices.Currencies());
        }
        private dynamic BuildMenuViewBagCurrency(List<CBS.FrontDesk.Data.Entity.Accounting.Currency> currencies)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in currencies)
            {
               
                    list.Add(new SelectListItem { Text = item.Code, Value = item.Name + "-" + item.Symbol });
                
            }

            return list;
        }
        private dynamic BuildMenuViewBag(IEnumerable<ChartOfAccount> debitAccounts, string language = "En")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in debitAccounts)
            {
                if (language == "En")
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.LabelEn });
                else
                {
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.LabelFr });

                }
            }

            return list;
        }
        [HttpPost]
        public async Task<ActionResult> Create(CashFlowManagement model)
        {
            if (ModelState.IsValid)
            {


                var data = await _accountingEntryServices.Create(model.ManualAccountingEntry);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> CreateCashInfusion(CashFlowManagement model)
        {
            if (ModelState.IsValid)
            {


                var data = await _accountingEntryServices.CreateCashInfusion(model.CashInfusion);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
 
    
      
    }
}