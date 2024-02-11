using CBS.BusinessService.Accounting;
using CBS.BusinessService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Message;
using System.Web.Services.Description;
using CBS.FrontDesk.Data.Entity;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data;


namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class BankingOperationsController : BaseController
    {
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;
        private readonly AccountingServices _accountingServices;
        public BankingOperationsController()
        {
            _chartOfAccountServices= new ChartOfAccountServices();
            _accountingServices = new AccountingServices();
            _accountingEntryServices = new AccountingEntryServices();
        }
        // GET: BankingOperation
        public async Task<ActionResult> Index()
        
        {
            await GetList();
            return View(new BankingOperations());
        }

        public async Task GetList(string language = "En")
        {
  
            ViewBag.BookingDirections = await _chartOfAccountServices.GetBookingDirections();
            var DebitAccounts = await _accountingServices.GetAllAccounting();
            var CreditAccounts = BuildMenuViewBag(DebitAccounts, language);
  
            ViewBag.Accounts = CreditAccounts;
        }

        private dynamic BuildMenuViewBag(IEnumerable<Account> debitAccounts, string language = "En")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in debitAccounts)
            {
                if (language == "En")
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.AccountHolder });
                else
                {
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.AccountHolder });

                }
            }

            return list;
        }
        [HttpPost]
        public async Task<ActionResult> Create(BankingOperations model)
        {
            if (ModelState.IsValid)
            {
                var entry = model.ManualAccountingEntry;

                var data = await _accountingEntryServices.Create(entry);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> CreateCashInfusion(BankingOperations model)
        {
            if (ModelState.IsValid)
            {

                var entry = model.CashInfusion;
                var data = await _accountingEntryServices.CreateCashInfusion(entry);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

    }
}