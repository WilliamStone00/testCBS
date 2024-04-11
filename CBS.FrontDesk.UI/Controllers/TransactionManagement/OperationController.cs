using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using System.Web.Services.Description;
using CBS.BusinessService.Application;
using CBS.BusinessService;
using CBS.BusinessService.Accounting;

namespace CBS.FrontDesk.UI.Controllers
{
    public class OperationController : BaseController
    {
        // GET: Operation/Transfer
        private readonly AccountServices _acountServices;
        private readonly TellerProvissioningServices _services;
        private readonly LoanServices _loanServices;
        public OperationController(AccountServices acountServices, TellerProvissioningServices services = null, LoanServices loanServices = null)
        {
            _acountServices = acountServices;
            _services = services;
            _loanServices = loanServices;
        }
        public async Task<ActionResult> AccountDetails(string KEY = null)
        {

            //502846231941202110
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> Deposit(string KEY = null)
        {
            //Statement

            ViewBag.Operation = "Deposit";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> Statement(string KEY = null)
        {
            ViewBag.Operation = "Deposit";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> LoanRepayment(string KEY = null)
        {
            ViewBag.Operation = "LoanRepayment";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            var loan = await _loanServices.GetLoanByCustomerID(account.CustomerId);
            account.Loans = loan.Where(x => x.LoanStatus == "Open").ToList();
            account.LoanRepayments = loan.SelectMany(x=>x.Refunds).ToList();
            return View(account);
        }
        //LoanRepayment
        public async Task<ActionResult> Withdrawal(string KEY = null)
        {
            ViewBag.Operation = "Withdrawal";
            ViewBag.Sources = _acountServices.GetPaymentSources();
            var account = await _acountServices.GetAccountByAccountNumber(KEY);
            return View(account);
        }
        public async Task<ActionResult> Transactions()
        {
            return View();
        }
        public async Task<ActionResult> Transfers()
        {
            return View();
        }
        public async Task<ActionResult> TransferPending()
        {
            return View();
        }
        public async Task<ActionResult> TransferRequest()
        {
            var account = await _acountServices.SourceAndDestinationAccount();
            var conf = await _acountServices.GetSavingConfigurationAggregates();
            ViewBag.Source = account;
            ViewBag.Destination = account;
            ViewBag.TransferTypes = conf.transferTypes;
            return View(new Account());
        }
       
        //Transactions
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (path == "transactions")
                {
                    var account = await _acountServices.GetTransactionsAsync();
                    return PartialView(partialView, account);
                }
                else if (path== "confirmation_request")
                {
                    var Statuses = await _acountServices.GetSavingConfigurationAggregates();
                    ViewBag.Status = Statuses.Statuses;
                    var transfer = await _acountServices.GetTransfer(KEY);
                    var account=new Account {Transfer=transfer, TransferConfirmation=new TransferConfirmation{ TransferId= transfer.Id} };
                    return PartialView(partialView, account);
                }
                else if (path == "details")
                {
                    var transfer = await _acountServices.GetTransfer(KEY);
                    var account = new Account { Transfer = transfer};
                    return PartialView(partialView, account);
                }
                else if (path == "pending_request")
                {
                   
                    var transfers = await _acountServices.GetPendingTransfers();
                    var account = new Account { Transfers = transfers };
                    return PartialView(partialView, account);
                }
                else if (path == "all_transfer_request")
                {
                    var transfers = await _acountServices.GetTransfers();
                    var account = new Account { Transfers = transfers };
                    return PartialView(partialView, account);
                }
                else if (path == "transfer_request")
                {
                    var account = await _acountServices.SourceAndDestinationAccount();
                    var conf = await _acountServices.GetSavingConfigurationAggregates();
                    ViewBag.Source = account;
                    ViewBag.Destination = account;
                    ViewBag.TransferTypes = conf.transferTypes;
                    return PartialView(partialView, new Account());
                }
                else
                {
                    var account = await _acountServices.GetAccountByAccountNumber(KEY);
                    ViewBag.Sources = _acountServices.GetPaymentSources();
                    return PartialView(partialView, account);
                }

            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = ex.Message; // Store error message
                return RedirectToAction("Index", "Error"); // Redirect to error page
            }
        }
        [HttpPost]
        public async Task<ActionResult> Deposit(Account model)
        {
            if (model.OperationType == "Deposit")
            {

                var data = await _acountServices.Deposit(model.DepositRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "InitialDeposit")
            {
                var data = await _acountServices.MakeInitialDeposit(model.AccountActivationRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "Withdrawal")
            {

                var data = await _acountServices.Withdrawal(model.DepositRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "LoanRepayment")
            {

                var data = await _acountServices.LoanRepayment(model.DepositRequest);
               
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "Transfer")
            {
                var data = await _acountServices.Transfer(model.TransferRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.OperationType == "TransferConfirmation")
            {
                var data = await _acountServices.TransferConfirmation(model.TransferConfirmation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return Json(new { success = false, status = false, message = "Fill the required fields." });
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetReport(string rptType = null, string ReportName = null, string serviceoption = null,string reportpath=null,string fileTitle=null, string ReadOptions = null, string KEY = null, string path = null, string yearID = null, string datefrom = null, string dateto = null)
        {
            if (path == "export_transactions")
            {
                var account = await _acountServices.GetTransactionsAsync();
                this.HttpContext.Session["rptSource"] = _acountServices.GetTransactionHistoryExports(account.TransactionHistories);
                if (!account.TransactionHistories.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rptType"] = rptType;
                this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";

            }
            else if (path== "customer_account_transaction")
            {
                var transactionHistories = await _acountServices.GetCustomerTransactionsByAccountNumber(KEY);
                this.HttpContext.Session["rptSource"] = _acountServices.GetTransactionHistoryExports(transactionHistories);
                if (!transactionHistories.Any())
                {
                    this.HttpContext.Session["rptSource"] = "empty";
                }
                this.HttpContext.Session["rptType"] = rptType;
                this.HttpContext.Session["ReportName"] = $"{ReportName}.rpt";
                this.HttpContext.Session["rptpath"] = $"~/{reportpath}/" + ReportName + ".rpt";
                this.HttpContext.Session["rpttitle"] = $"{fileTitle}";
            }
            else if (path == "by_date_history")
            {
                //this.HttpContext.Session["rptSource"] = _helper._object.Receipts;
                //this.HttpContext.Session["DateFrom"] = datefrom;
                //this.HttpContext.Session["DateTo"] = dateto;
            }
           

            return Json("", JsonRequestBehavior.AllowGet);

        }
       
        public async Task<ActionResult> GetLoan(string Key)
        {
            var loan = await _loanServices.GetLoan(Key);
            //return Json(loan, JsonRequestBehavior.AllowGet);
            return Json(loan, JsonRequestBehavior.AllowGet);

        }
    }
}