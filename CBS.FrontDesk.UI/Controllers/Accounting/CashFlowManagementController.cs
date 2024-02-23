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
using System.Web.Services.Description;
using System.Web.ModelBinding;
using System.Data;
using CBS.FrontDesk.Data;
using System.Security.Cryptography.Xml;
using CBS.FrontDesk.Helper;

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
            return View();
        }

        public async Task GetList(string language = "En")
        {

            ViewBag.BookingDirections = await _chartOfAccountServices.GetBookingDirections();
            var DebitAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();
            var CreditAccounts = BuildMenuViewBag(DebitAccounts, language);
            ViewBag.Decisions = BuildMenuViewBag();
            ViewBag.Accounts = CreditAccounts;
     
            ViewBag.OpeningOfDayId = await _accountingEntryServices.GetCashReplenimentRequestId();
            if (ViewBag.OpeningOfDayId == null)
            {
                ViewBag.OpeningOfDayId = new SelectListItem { Text = "Id001", Value ="Current Opening Reference" };
            }
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


        private dynamic BuildMenuViewBag()
        {
            List<SelectListItem> list = new List<SelectListItem>();
           
                    list.Add(new SelectListItem { Text = $"Approve", Value = "Approve" });

            list.Add(new SelectListItem { Text = $"Rejected", Value = "Rejected" });


            return list;
        }
        [HttpPost]
        public async Task<ActionResult> Create(CashFlowManagement model)
        {
            if (ModelState.IsValid)
            {


                var data = await _accountingEntryServices.CreateManualAccountingEntry(model.ManualAccountingEntry);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult>  CreateCashReplenishmentRequest()
        {
            await GetList();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateCashReplenishmentRequest(CashInfusion model)
        {
            if (ModelState.IsValid)
            {
                var data = await _accountingEntryServices.CashReplenishmentRequest(model);
                if (data.MessageStatus.Equals("Failed"))
                {
                    return View("Failed_Request_View", model.ConvertToCashReplenimentRequest());
                }
                else
                {
                    var datasw = await _accountingEntryServices.GetCashReplenimentReferenceRequest(model.ReferenceNumber);
                    return View("Successfull_Request_View", datasw);
                }
           
            }

            return View("Failed_Request_View", model.ConvertToCashReplenimentRequest());
        }

        [HttpGet]
        public async Task<ActionResult> GetCashReplenimentRequest(string KEY)
        {
            await GetList();
            var datas = await _accountingEntryServices.GetCashReplenimentRequest(KEY);

            CashReplenimentRequestDto model = datas.ConvertToCashReplenimentRequestDto();

            return View(model);
        }
        [HttpGet]
        public async Task<ActionResult> GetAllCashReplenimentRequestData()
        {
            var datas = await _accountingEntryServices.GetAllCashReplenimentRequest();
            var dataUser = (await _accountingEntryServices.GetUserList()).ToList();
            var result = from request in datas
                         join user in dataUser on request.IssuedBy equals user.id.ToString()
                         select new CashReplenimentRequest
                         {
                             Id = request.Id,
                             ReferenceId = request.ReferenceId,
                             AmountRequested = request.AmountRequested,

                             RequestMessage = request.RequestMessage,
                             IssuedBy = user.name + "," + user.roleName,
                             IssuedDate = request.IssuedDate,
                             ApprovedBy = request.ApprovedBy,
                             ApprovedDate = request.ApprovedDate,
                             IsApproved = request.IsApproved,
                             CurrencyCode = request.CurrencyCode,
                             Status = request.Status,
                             ApprovedMessage = request.ApprovedMessage
                         };
 
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> CreateApprovalRequest()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateApprovalRequest( CashReplenimentRequestDto model)
        
        {
            model.IsApproved = model.ApprovedBy == "Approve" ? true: false;
            if (ModelState.IsValid)
            {
                var datas = await _accountingEntryServices.CreateApprovalRequest(model.ConvertToCashApprovalResponse());
                if (datas.MessageStatus.Equals("Failed"))
                {
                    var datasw = await _accountingEntryServices.GetCashReplenimentRequest(model.Id);
                    return View("Failed_RequestApproval_View", datasw);
                }
                else
                {
                    var datasw = await _accountingEntryServices.GetCashReplenimentRequest(model.Id);
                    return View("Successfull_RequestApproval_View", datasw);
                }

            }
            else
            {
                var datasw = await _accountingEntryServices.GetCashReplenimentRequest(model.Id);
                return View("Failed_RequestApproval_View", datasw);
            }




        }
    }
}