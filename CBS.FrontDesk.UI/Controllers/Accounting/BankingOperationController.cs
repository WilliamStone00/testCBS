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
            ViewBag.Decisions = BuildMenuViewBag();
            ViewBag.Accounts = CreditAccounts;
        }

        private dynamic BuildMenuViewBag(IEnumerable<Account> debitAccounts, string language = "En")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in debitAccounts)
            {
                if (language == "En")
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.AccountName });
                else
                {
                    list.Add(new SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.AccountName });

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

                var data = await _accountingEntryServices.CreateManualAccountingEntry(entry);
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
                var data = await _accountingEntryServices.CashReplenishmentRequest(entry);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> CreateTransactionReversalRequest()
        {
            await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateTransactionReversalRequest(TransactionReversalRequest model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountingEntryServices.CreateTransactionReversalRequest(model);
                if (result.MessageStatus.Equals("Failed"))
                {

                    var datasw = await _accountingEntryServices.GetTransactionReversalRequestByReferenceId(model.ReferenceNumber);

                    return View("Failed_Request_View", datasw);
                }
                else
                {

                    var datasw = await _accountingEntryServices.GetTransactionReversalRequestByReferenceId(model.ReferenceNumber);

                    return View("Successfull_Request_View", datasw);
                }

            }


            return View("Failed_Request_View", model);
        }
        public async Task<ActionResult> CreateTransactionReversalRequestApproval()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateTransactionReversalRequestApproval(TransactionReversalRequestApproval model)
         {
            model.IsApproved = model.Status == "Approve" ? true : false;
            if (ModelState.IsValid)
            {
                var datas = await _accountingEntryServices.TransactionReversalRequestApproval(model.ConvertToTransactionReversalRequestApproval());
                if (datas.MessageStatus.Equals("Failed"))
                {
                    var datasw = await _accountingEntryServices.GetTransasctionReversalRequestById(model.Id);

                  
                    return View("Failed_RequestApproval_View", datasw);
                }
                else
                {
                    var datasw = await _accountingEntryServices.GetTransasctionReversalRequestById(model.Id);
                    return View("Successfull_RequestApproval_View", datasw);
                }

            }
            else
            {
                var datasw = await _accountingEntryServices.GetTransasctionReversalRequestById(model.Id);
                return View("Failed_RequestApproval_View", datasw);
            }




        }
        private dynamic BuildMenuViewBag()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = $"Approve", Value = "Approve" });

            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });


            return list;
        }
        [HttpGet]
        public async Task<ActionResult> GetTransactionReversalRequest(string KEY)
        {
            await GetList();

            var datas = await _accountingEntryServices.GetTransasctionReversalRequestById(KEY);



            return View(datas);
        }
        [HttpGet]
        public async Task<ActionResult> GetAllTransasctionReversalRequest()
        {
            var datas = await _accountingEntryServices.GetAllTransasctionReversalRequest();
            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.IssuedBy equals user.id.ToString()
                         select new TransactionReversalDetailRequestDto
                         {
                             Id = request.Id,
                             ReferenceId = request.ReferenceId,
                           

                             RequestMessage = request.RequestMessage,
                             IssuedBy = user.name + "," + user.roleName,
                             IssuedDate = request.IssuedDate,
                             ApprovedBy = request.ApprovedBy,
                             ApprovedDate = request.ApprovedDate,
                             IsApproved = request.IsApproved,
                          
                             Status = request.Status,
                             ApprovedMessage = request.ApprovedMessage
                         };

            return View(result);
        }

    }
}