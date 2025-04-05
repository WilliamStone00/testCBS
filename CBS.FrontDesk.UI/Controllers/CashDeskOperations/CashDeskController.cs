using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CashDeskOperations
{
    [CheckSessionTimeOutAttribute]

    public class CashDeskController : BaseController
    {
        private readonly CashDeskServices _cashDeskService;
        private readonly AccountingServices _accountingServices;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;


        public CashDeskController(CashDeskServices cashDeskService = null, AccountingServices accountingServices = null, ChartOfAccountServicesAnnex chartOfAccountServices = null)
        {
            _cashDeskService = cashDeskService;
            _accountingServices = accountingServices;
            this.chartOfAccountServices=chartOfAccountServices;
        }
        // GET: CashDesk
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LoanRepaymentSimulation()
        {
            return View();
        }
        public ActionResult MomocashCollection()
        {
            return View();
        }
        public ActionResult NoneCashOperations()
        {
            return View();
        }
        
        public async Task<ActionResult> OtherCashTransactions()
        {
            ViewBag.Operation = "income_expense";
            var cashDesk = await _cashDeskService.GetOtherCashDeskTransactions();
            //ViewBag.Members = _cashDeskService.LoadMembersToList(cashDesk.Customers);
            ViewBag.MemberAccounts = new SelectList(new List<StringValues>(), "None", "No-Account-Loaded");
            await GetEventNames("FEE");
            //ViewBag.EventCodes = await _accountingServices.GetEventNames("INCOME");
            return View(cashDesk);
        }
        public async Task<ActionResult> OtherCashMobileMoney()
        {
            var cashDesk = await _cashDeskService.GetOtherCashDeskMobileMoney();
            return View(cashDesk);
        }
        //OtherCashMobileMoney
        public async Task<ActionResult> ExpenseOtherPayment()
        {
            ViewBag.Operation = "income_expense";
            var cashDesk = await _cashDeskService.GetOtherCashDeskTransactions();
            //ViewBag.Members = _cashDeskService.LoadMembersToList(cashDesk.Customers);
            ViewBag.MemberAccounts = new SelectList(new List<StringValues>(), "None", "No-Account-Loaded");
            await GetEventNames("EXPENSE");
            //ViewBag.EventCodes = await _accountingServices.GetEventNames("INCOME");
            return View(cashDesk);
        }
        private async Task GetEventNames(string operationType)
        {
            ViewBag.EventCodes = await _accountingServices.GetEventNamesOtherCashIn(operationType);

        }
        public async Task<ActionResult> Ajaxloader(string Key,string path)
        {
            if (path== "getmember")
            {
                var listing = await _cashDeskService.GetCustomer(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var listing = await _cashDeskService.LoadMembersAccountByMemberReference(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
        }
       
        //income_expense
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = "_DataNotFound", string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (path=="")
                {
                    path = "cashin";
                }
                if (path == "search")
                {
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    var cashDesk = await _cashDeskService.GetMember(KEY);
                    if (cashDesk == null)
                    {
                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    return PartialView(partialView, cashDesk);


                }
                else if (path == "cashin" ||path=="repayment"|| path == "cashout" || path == "cashoutsws" || path == "repayment" || path == "withdrawalnotification" || path== "loanapplicationfeepayment")
                {
                    if (KEY == null || KEY == "")
                    {
                        ViewBag.message = "Empty data was submited. Please enter search criterial";
                        return PartialView("_DataNotFound", new CashDesk());
                    }

                    var cashDesk = await _cashDeskService.GetAccountByAccountNumberSearch(KEY, path);
                    if (cashDesk == null)
                    {

                        ViewBag.message = $"{KEY} was not found in the database.";
                        return PartialView("_DataNotFound", new CashDesk());
                    }
                    ViewBag.Operation = path;
                    return PartialView(partialView, cashDesk);

                }
                else if (path == "new_depositor")
                {
                    return PartialView(partialView, new CashDesk());

                }
                ViewBag.message = "Invalid option selected";
                return PartialView("_NoRecordFound", new CashDesk());

            }
            catch (Exception ex)
            {

                ViewBag.message = ex.Message; // Store error message
                return PartialView("_NoRecordFound", new CashDesk());
            }
        }
        [HttpPost]
        public async Task<ActionResult> PostRequestCash(List<BulkDeposit> deposits)
        {
            try
            {
                if (deposits != null)
                {
                    var data = await _cashDeskService.BulkDeposi(deposits);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }
                return Json(new { success = false, status = false, message = $"No data was submitted." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetReport(string path)
        {
            if (path=="loan")
            {
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"MainReportLoan.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/Loan/MainReportLoan.rpt";
                this.HttpContext.Session["rpttitle"] = $"MemberLoanReceipts";
                return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                this.HttpContext.Session["rptType"] = "ReportParameterLess";
                this.HttpContext.Session["ReportName"] = $"MainReport.rpt";
                this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/MainReport.rpt";
                this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
                return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public async Task<ActionResult> GetReportOtherPayment()
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"OtherTransactionReciept.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Reciepts/OtherTransactionReciept.rpt";
            this.HttpContext.Session["rpttitle"] = $"MemberReceiptsOtherPayment";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

        }

    }
}