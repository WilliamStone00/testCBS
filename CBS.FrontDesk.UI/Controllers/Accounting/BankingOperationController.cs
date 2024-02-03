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
 

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class BankingOperationsController : BaseController
    {
        private readonly ChartOfAccountServices _chartOfAccountServices;
        private readonly AccountingEntryServices _accountingEntryServices;

        public BankingOperationsController()
        {
            _chartOfAccountServices= new ChartOfAccountServices();
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
            var DebitAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
            var CreditAccounts = BuildMenuViewBag(DebitAccounts, language);
  
            ViewBag.Accounts = CreditAccounts;
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
        public async Task<ActionResult> Create(ManualAccountingEntry model)
        {
            if (ModelState.IsValid)
            {
            

                var data = await _accountingEntryServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> CreateCashInfusion(CashInfusion model)
        {
            if (ModelState.IsValid)
            {


                var data = await _accountingEntryServices.CreateCashInfusion(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

    }
}